using FlowBlox.AppWindow.ContentFactories;
using FlowBlox.AppWindow.Contents;
using FlowBlox.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow
{
    public partial class AppWindow
    {
        private void InitializeDockPanel(bool exceptProjectPanel = false, bool exceptAiAssistantView = false)
        {
            _defaultPaneActivationApplied = false;
            this.dockPanel.SuspendLayout();

            this.dockPanel.Theme = new VS2015DarkTheme();

            foreach (var dockContent in dockPanel.Contents
                .OfType<DockContent>()
                .Where(x =>
                    (!exceptProjectPanel || x is not ProjectPanel) &&
                    (!exceptAiAssistantView || x is not AIAssistantView))
                .ToList())
            {
                dockContent.Close();
            }

            if (!exceptProjectPanel)
            {
                var projectPanelFactory = new ProjectPanelFactory(dockPanel);
                _dockContentProjectPanel = projectPanelFactory.Create();
            }

            var componentLibraryPanelFactory = new ComponentLibraryPanelFactory(dockPanel);
            _componentLibraryPanel = componentLibraryPanelFactory.Create();

            var fieldViewPanelFactory = new FieldViewPanelFactory(dockPanel);
            _fieldViewPanel = fieldViewPanelFactory.Create();

            var managedObjectsViewPanelFactory = new ManagedObjectsViewPanelFactory(dockPanel);
            _managedObjectsViewPanel = managedObjectsViewPanelFactory.Create();

            var testViewPanelFactory = new TestViewPanelFactory(dockPanel);
            _testViewPanel = testViewPanelFactory.Create();

            if (!exceptAiAssistantView || _aiAssistantViewPanel == null)
            {
                var aiAssistantPanelFactory = new AIAssistantViewPanelFactory(dockPanel);
                _aiAssistantViewPanel = aiAssistantPanelFactory.Create();
            }

            var problemsViewPanelFactory = new ProblemsViewPanelFactory(dockPanel);
            _problemsViewPanel = problemsViewPanelFactory.Create();

            var runtimeViewPanelFactory = new RuntimeViewPanelFactory(dockPanel);
            _runtimeViewPanel = runtimeViewPanelFactory.Create();

            this.dockPanel.ResumeLayout();
            ApplyDefaultPaneActivationOnce();
        }

        private bool _isStepThroughActivationRunning;
        private readonly HashSet<DockContent> _initializedDockContents = new HashSet<DockContent>();

        private List<DockContent> GetRelevantPaneContents(DockPane pane)
        {
            if (pane == null)
                return new List<DockContent>();

            return pane.Contents
                .OfType<DockContent>()
                .Where(x =>
                    !x.IsDisposed &&
                    !x.IsHidden &&
                    x.DockState != DockState.Hidden &&
                    x.DockState != DockState.Unknown)
                .ToList();
        }

        private bool ShouldUseStepThrough(DockContent target, out DockPane pane, out List<DockContent> paneContents)
        {
            pane = target?.Pane;
            paneContents = GetRelevantPaneContents(pane);

            if (target == null || pane == null)
                return false;

            if (paneContents.Count <= 2)
                return false;

            if (!paneContents.Contains(target))
                return false;

            return true;
        }

        private void ActivateDockContentWithStepThrough(DockContent target)
        {
            if (_isStepThroughActivationRunning)
                return;

            if (!ShouldUseStepThrough(target, out var pane, out var paneContents))
                return;

            if (_initializedDockContents.Contains(target))
            {
                target.Activate();
                return;
            }

            var targetIndex = paneContents.IndexOf(target);
            if (targetIndex < 0)
                return;

            _isStepThroughActivationRunning = true;

            dockPanel.SuspendLayout();
            pane.SuspendLayout();
            try
            {
                for (int i = 0; i <= targetIndex; i++)
                {
                    var content = paneContents[i];

                    if (_initializedDockContents.Contains(content))
                        continue;

                    content.Activate();
                    Application.DoEvents();

                    _initializedDockContents.Add(content);
                }

                _initializedDockContents.Add(target);
            }
            finally
            {
                pane.ResumeLayout(true);
                dockPanel.ResumeLayout(true);

                _isStepThroughActivationRunning = false;
            }
        }

        private void DockPanel_ActiveContentChanged(object sender, EventArgs e)
        {
            if (!_defaultPaneActivationApplied)
                return;

            if (_isStepThroughActivationRunning)
                return;

            if (dockPanel.ActiveContent is not DockContent dockContent)
                return;

            if (!ShouldUseStepThrough(dockContent, out _, out _))
                return;

            BeginInvoke(new MethodInvoker(() =>
            {
                ActivateDockContentWithStepThrough(dockContent);
            }));
        }

        private enum DockRegion
        {
            Left,
            Right,
            Bottom
        }

        private void ActivateInitialDockContents()
        {
            if (_defaultPaneActivationApplied || dockPanel == null || dockPanel.IsDisposed)
                return;

            var previouslyActive = dockPanel.ActiveContent as DockContent;
            Trace.WriteLine($"[DockInit] Previous active content: {DescribeDockContent(previouslyActive)}");
            var targetRegions = new[] 
            { 
                DockRegion.Left, 
                DockRegion.Right, 
                DockRegion.Bottom 
            };

            foreach (var region in targetRegions)
            {
                var firstVisibleContent = GetFirstVisibleContentByRegion(region);
                if (firstVisibleContent == null)
                {
                    Trace.WriteLine($"[DockInit] Region={region}: no visible dock content found.");
                    continue;
                }

                Trace.WriteLine($"[DockInit] Region={region}: activating {DescribeDockContent(firstVisibleContent)}");
                firstVisibleContent.Activate();

                Trace.WriteLine($"[DockInit] Region={region}: initializedDockContents add {DescribeDockContent(firstVisibleContent)}");
                _initializedDockContents.Add(firstVisibleContent);
            }

            _defaultPaneActivationApplied = true;
            Trace.WriteLine("[DockInit] Initial dock activation pass completed.");
        }

        private DockContent GetFirstVisibleContentByRegion(DockRegion region)
        {
            return dockPanel.Contents
                .OfType<DockContent>()
                .Where(dc =>
                    !dc.IsDisposed &&
                    !dc.IsHidden &&
                    dc.DockState != DockState.Hidden &&
                    dc.DockState != DockState.Unknown &&
                    GetDockRegion(dc.DockState) == region)
                .FirstOrDefault();
        }

        private static DockRegion? GetDockRegion(DockState dockState)
        {
            return dockState switch
            {
                DockState.DockLeft => DockRegion.Left,
                DockState.DockLeftAutoHide => DockRegion.Left,
                DockState.DockRight => DockRegion.Right,
                DockState.DockRightAutoHide => DockRegion.Right,
                DockState.DockBottom => DockRegion.Bottom,
                DockState.DockBottomAutoHide => DockRegion.Bottom,
                _ => null
            };
        }

        private void ApplyDefaultPaneActivationOnce()
        {
            if (_defaultPaneActivationApplied)
                return;

            if (IsHandleCreated)
                BeginInvoke(new MethodInvoker(ActivateInitialDockContents));
            else
                ActivateInitialDockContents();
        }

        private static string DescribeDockContent(DockContent dockContent)
        {
            if (dockContent == null)
                return "<null>";

            return $"{dockContent.GetType().Name} Name='{dockContent.Name}' Text='{dockContent.Text}' DockState={dockContent.DockState} IsHidden={dockContent.IsHidden}";
        }

        private void DockPanel_ContentAdded(object sender, DockContentEventArgs e)
        {
            var dockContent = ((DockContent)e.Content);

            var toolstripMenuItem = new ToolStripMenuItem()
            {
                Text = dockContent.Text
            };

            toolstripMenuItem.Image = DockContentIconResolver.Resolve(dockContent);

            toolstripMenuItem.Click += (s, e2) =>
            {
                dockContent.Show();
            };

            itmDockablePanels.DropDownItems.Add(toolstripMenuItem);

            FlowBloxStyle.ApplyStyle(this.menuStrip);
        }

        private void DockPanel_ContentRemoved(object sender, DockContentEventArgs e)
        {
            var removedContent = (DockContent)e.Content;
            foreach (ToolStripItem item in itmDockablePanels.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Text == removedContent.Name)
                {
                    itmDockablePanels.DropDownItems.Remove(item);
                    break;
                }
            }
        }
    }
}
