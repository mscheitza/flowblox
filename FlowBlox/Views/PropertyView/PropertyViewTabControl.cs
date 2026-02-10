using FlowBlox.Core;
using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Models.Components;
using FlowBlox.Core.Models.FlowBlocks.Base;
using FlowBlox.Core.Attributes;
using FlowBlox.Grid.Views.Main;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Manager;

namespace FlowBlox.Views.PropertyView
{
    public enum PropertyViewTabControlState
    {
        Opened,
        Applied,
        Cancelled
    }

    public class PropertyViewTabControl : TabControl
    {
        public event TargetChangedEventHandler TargetChanged;

        public PropertyViewTabControlState State { get; set; }

        private object _target;
        private object _transientTarget;
        private bool _deepCopy;
        private PropertyViewTransactionManager _transactionManager;
        private List<PropertyViewTableLayoutPanel> _propertyViews;
        
        public List<PropertyViewTableLayoutPanel> GetAssociatedPropertyViews() => _propertyViews;

        public PropertyViewTabControl(bool deepCopy = true)
        {
            _deepCopy = deepCopy;
            _transactionManager = new PropertyViewTransactionManager();
            _propertyViews = new List<PropertyViewTableLayoutPanel>();
        }

        public object TransientTarget => _transientTarget;

        public object Target => _target;

        private string GetGroupName(PropertyInfo property)
        {
            string groupName = property.GetCustomAttribute<DisplayAttribute>().GroupName;
            if (string.IsNullOrEmpty(groupName))
                groupName = "Global_Groups_Default";
            return groupName;
        }

        public void Initialize(object target, bool readOnly)
        {
            TabPages.Clear();

            if (_deepCopy)
            {
                var result = _transactionManager.Open(target);
                _transientTarget = result.TransientTarget;
            }
            else
            {
                _transientTarget = target;
            }

            _target = target;

            var groupAttributes = _target.GetType().GetCustomAttributes<FlowBlockUIGroupAttribute>(true)
                .OrderBy(x => x.Order)
                .ToList();

            var groupedProperties = _target.GetType().GetProperties()
                .Where(property => property.GetCustomAttribute<DisplayAttribute>() != null)
                .GroupBy(property => GetGroupName(property))
                .OrderBy(propertyGroup => {

                    var groupAttribute = groupAttributes.FirstOrDefault(x => x.Name == propertyGroup.Key);
                    if (groupAttribute == null)
                        return 0;

                    return groupAttributes.IndexOf(groupAttribute);
                });

            var adjustmentHandlerRegistered = false;

            // Erstelle einen PropertyView für jede Gruppe und füge ihn zu einer neuen TabPage hinzu
            foreach (var propertyGroup in groupedProperties)
            {
                var propertyView = new PropertyViewTableLayoutPanel()
                {
                    Dock = DockStyle.Fill
                };
                propertyView.AutoScroll = true;
                propertyView.TargetChanged += (t, p) => this.TargetChanged?.Invoke(t, p);

                var groupAttribute = groupAttributes.FirstOrDefault(x => x.Name == propertyGroup.Key);

                ControlAlignment controlAlignment;

                if (groupedProperties.Count() > 1)
                    controlAlignment = groupAttribute?.ControlAlignment ?? ControlAlignment.Top;
                else
                    controlAlignment = ControlAlignment.Fill;

                propertyView.Initialize(_transientTarget, propertyGroup.OrderBy(
                    prop => prop.GetCustomAttribute<DisplayAttribute>().GetOrder() ?? 0), controlAlignment, readOnly);
                _propertyViews.Add(propertyView);

                var tabPage = new TabPage(FlowBloxResourceUtil.GetLocalizedString(propertyGroup.Key));
                tabPage.Name = propertyGroup.Key;
                tabPage.Controls.Add(propertyView);

                if (!adjustmentHandlerRegistered)
                {
                    FormHeightAdjustmentHandler.Register(propertyView);
                    adjustmentHandlerRegistered = true;
                }

                TabPages.Add(tabPage);
            }

            // Eigene TabPages
            InitializeCustomTabPages(readOnly);

            // Stil anwenden
            FlowBloxStyle.ApplyStyle(this);
        }

        private bool IsResultField(object managedObject, object target)
        {
            if (managedObject is not FieldElement)
                return false;

            if (target is not BaseResultFlowBlock)
                return false;

            var resultFlowBlock = (BaseResultFlowBlock)target;
            return resultFlowBlock.Fields.Contains(managedObject);
        }

        private Func<bool> _customApplyMethod;
        private void InitializeCustomTabPages(bool readOnly)
        {
            var flowBlockType = ReflectionHelper.GetImplementationTypeForTypeWithGeneric(typeof(IFlowBlockCustomTabPageProvider<>), _transientTarget.GetType());
            if (flowBlockType != null)
            {
                var inst = Activator.CreateInstance(flowBlockType);

                var methodInfoInit = flowBlockType.GetMethod(nameof(IFlowBlockCustomTabPageProvider<BaseFlowBlock>.Initialize));
                methodInfoInit.Invoke(inst, new object[] { _transientTarget });

                var customTabPageProvider = ((IFlowBlockCustomTabPageProvider)inst);
                customTabPageProvider.ReadOnly = readOnly;
                var tabControl = customTabPageProvider.TabControl;
                this._customApplyMethod = () => customTabPageProvider.Apply();

                AddTabPages(tabControl);
            }
        }

        private void AddTabPages(TabControl fromTabControl)
        {
            var curentTabPages = fromTabControl.TabPages.Cast<TabPage>().ToList();

            var allTabPages = fromTabControl.TabPages
                .Cast<TabPage>()
                .Concat(curentTabPages);

            curentTabPages.ForEach(x => this.TabPages.Remove(x));

            int indexToInsert = 0;
            foreach (TabPage tabPage in allTabPages)
            {
                var tabControl = (TabControl)tabPage.Parent;
                if (tabControl != null)
                    tabControl.TabPages.Remove(tabPage);

                this.TabPages.Add(tabPage);
                indexToInsert++;
            }
            this.Refresh();
        }

        public bool Validate(out List<string> invalidProperties)
        {
            var _invalidProperties = new List<string>();
            _propertyViews.ForEach(x => x.Validate(ref _invalidProperties));
            invalidProperties = _invalidProperties;
            return !_invalidProperties.Any();
        }

        public void Cancel()
        {
            if (_deepCopy)
                _transactionManager.Cancel();

            this.State = PropertyViewTabControlState.Cancelled;
        }

        public bool Apply()
        {
            List<string> invalidProperties;
            if (!Validate(out invalidProperties))
            {
                FlowBloxMessageBox.Show(
                    this.FindForm(),
                    string.Format(FlowBloxResourceUtil.GetLocalizedString(nameof(PropertyViewTabControl), "FormInvalid", "Message"), invalidProperties.ToArray()),
                    FlowBloxResourceUtil.GetLocalizedString(nameof(PropertyViewTabControl), "FormInvalid", "Title"),
                    FlowBloxMessageBox.Buttons.OK,
                    FlowBloxMessageBox.Icons.Error
                );

                return false;
            }

            // Custom apply method
            if (_customApplyMethod?.Invoke() == false)
                return false;

            if (_deepCopy)
                _transactionManager.Commit(_target, _transientTarget);

            this.State = PropertyViewTabControlState.Applied;
            return true;
        }
    }
}
