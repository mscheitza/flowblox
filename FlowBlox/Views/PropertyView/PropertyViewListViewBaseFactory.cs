using FlowBlox.Core.Attributes;
using FlowBlox.Core.Interfaces;
using FlowBlox.Core.Models.Base;
using FlowBlox.Core.Util.Controls;
using FlowBlox.Core.Util.Drawing;
using FlowBlox.Core.Util.Resources;
using FlowBlox.UICore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FlowBlox.Views.PropertyView
{
    public class PropertyViewListViewBaseFactory : WinFormsPropertyViewControlFactory
    {
        
        protected readonly FlowBlockListViewAttribute _listViewAttribute;
        protected ListView _listView;
        protected ImageList imageList_ComponentImages;
        protected Type _listType;
        private ListViewColumnAdjustmentHandler _adjustmentHandler;
        protected IList _list;

        public PropertyViewListViewBaseFactory(PropertyInfo property, object target, bool readOnly) : base(property, target, readOnly)
        {
            _listViewAttribute = property.GetCustomAttribute<FlowBlockListViewAttribute>();

            if (_listViewAttribute == null)
                throw new InvalidOperationException("Missing FlowBloxListViewAttribute on target property.");

            var propertyValue = _property.GetValue(_target);
            if (!(propertyValue is IList))
                throw new InvalidOperationException("Member is not a list.");

            this._list = (IList)propertyValue;
            this._listType = _list.GetType().GetGenericArguments()[0];
        }

        protected IEnumerable<PropertyInfo> GetListViewProperties()
        {
            return _listType.GetProperties().Where(x => _listViewAttribute.LVColumnMemberNames.Contains(x.Name));
        }

        protected void InitializeColumns()
        {
            foreach (var property in GetListViewProperties())
            {
                var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
                string headerText = displayAttr != null ? FlowBloxResourceUtil.GetDisplayName(displayAttr) : property.Name;
                _listView.Columns.Add(headerText);
            }
        }

        protected void InsertOrAddListViewItem(object item, int? index = null)
        {
            var listViewItem = new ListViewItem();
            var properties = GetListViewProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(item)?.ToString();
                if (property == properties.First())
                    listViewItem.Text = value;
                else
                    listViewItem.SubItems.Add(value);
            }

            listViewItem.Tag = item;

            if (item is FlowBloxComponent component)
            {
                var typeKey = component.GetType().FullName;
                EnsureTypeIconRegistered(typeKey, component);
                listViewItem.ImageKey = typeKey;
            }

            if (index.HasValue && index.Value >= 0 && index.Value <= _listView.Items.Count)
                _listView.Items.Insert(index.Value, listViewItem);
            else
                _listView.Items.Add(listViewItem);

            _adjustmentHandler.AdjustListViewColumns();
        }

        private void EnsureTypeIconRegistered(string typeKey, FlowBloxComponent component)
        {
            if (string.IsNullOrWhiteSpace(typeKey))
                return;

            if (imageList_ComponentImages.Images.ContainsKey(typeKey))
                return;

            var icon = component.Icon16;
            if (icon == null)
                return;

            var image = SkiaToSystemDrawingHelper.ToSystemDrawingImage(icon);
            if (image == null)
                return;

            using (var resized = ImageHelper.CopyImage(image,
                imageList_ComponentImages.ImageSize.Width,
                imageList_ComponentImages.ImageSize.Height))
            {
                imageList_ComponentImages.Images.Add(typeKey, resized);
            }
        }

        internal ListView CreateListView()
        {
            _listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                Dock = DockStyle.Fill,
                GridLines = true,
            };

            imageList_ComponentImages = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16) 
            };
            _listView.SmallImageList = imageList_ComponentImages;

            _adjustmentHandler = ListViewColumnAdjustmentHandler.Register(_listView);
            ListViewHelper.EnableDoubleBuffer(_listView);

            InitializeColumns();
            InitializeListViewItems();
            return _listView;
        }

        private void InitializeListViewItems()
        {
            foreach (var item in _list)
            {
                InsertOrAddListViewItem(item);
            }
            _adjustmentHandler.AdjustListViewColumns();
        }

        protected override object CreateNewInstance(Type type)
        {
            if (_listViewAttribute.LVItemFactory != null)
            {
                var factoryType = _listViewAttribute.LVItemFactory;
                var factoryInstance = (IItemFactory<IFlowBloxComponent>)Activator.CreateInstance(factoryType);
                var result = factoryInstance.Create();

                if (result == null || !type.IsInstanceOfType(result))
                    throw new InvalidOperationException($"Factory returned an incompatible instance. Expected type: {type.FullName}, actual: {result?.GetType().FullName ?? "null"}");

                return result;
            }
            else
            {
                return base.CreateNewInstance(type);
            }
        }

        public override void Reload()
        {
            _listView.Items.Clear();
            InitializeListViewItems();
            base.Reload();
        }
    }
}
