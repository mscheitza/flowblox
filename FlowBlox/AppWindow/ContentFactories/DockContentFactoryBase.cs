using FlowBlox.Core.Util;
using FlowBlox.Core.Util.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlowBlox.AppWindow.ContentFactories
{
    public abstract class DockContentFactoryBase<T> where T : DockContent
    {
        protected readonly DockPanel _dockPanel;
        private bool _ready;
        private bool _closing;

        public DockContentFactoryBase(DockPanel dockPanel)
        {
            _dockPanel = dockPanel;
        }

        protected DockContentSettings LoadSettings(string key)
        {
            var dockSettingsValue = FlowBloxOptions.GetOptionInstance().GetOption("MainPanel.DockSettings").Value;
            if (!string.IsNullOrEmpty(dockSettingsValue))
            {
                var dockSettings = JsonConvert.DeserializeObject<DockSettings>(dockSettingsValue);
                if (dockSettings.DockContentSettings.TryGetValue(key, out var dockContentSettings))
                {
                    return dockContentSettings;
                }
            }
            return GetDefaults();
        }

        protected virtual DockContentSettings GetDefaults()
        {
            return new DockContentSettings
            {
                DockState = DockState.DockBottom,
                Visible = true
            };
        }

        protected void SaveSettings(string key, DockContentSettings settings)
        {
            var dockSettingsOption = FlowBloxOptions.GetOptionInstance().GetOption("MainPanel.DockSettings");
            var dockSettingsValue = dockSettingsOption.Value;
            var dockSettings = !string.IsNullOrEmpty(dockSettingsValue) ?
                JsonConvert.DeserializeObject<DockSettings>(dockSettingsValue) : 
                new DockSettings();

            dockSettings.DockContentSettings[key] = settings;
            dockSettingsOption.Value = JsonConvert.SerializeObject(dockSettings);
            FlowBloxOptions.GetOptionInstance().Save();
        }


        protected Task SaveSettingsAsync(string key, DockContentSettings settings)
        {
            return Task.Run(() =>
            {
                SaveSettings(key, settings);
            });
        }

        protected T Create(string key, T dockContent)
        {
            var settings = LoadSettings(key);

            if (settings.Width != null)
                dockContent.Width = settings.Width.Value;

            if (settings.Height != null)
                dockContent.Height = settings.Height.Value;

            dockContent.Show(_dockPanel, settings.DockState);

            dockContent.HideOnClose = true;

            if (!settings.Visible)
                dockContent.Hide();

            dockContent.Layout += _dockPanel_Layout;

            dockContent.FormClosing += DockContent_FormClosing;

            dockContent.DockStateChanged += (s, e) =>
            {
                if (!_ready)
                    return;

                if (_closing)
                    return;

                if (dockContent.DockState == DockState.Unknown ||
                    dockContent.DockState == DockState.Hidden)
                {
                    settings.Visible = false;
                }
                else
                {
                    settings.DockState = dockContent.DockState;
                    settings.Visible = true;
                }
                SaveSettings(key, settings);
            };

            dockContent.SizeChanged += async (s, e) =>
            {
                if (!_ready)
                    return;
                
                if (_closing)
                    return;

                if (dockContent.Width > 0 && dockContent.Height > 0)
                {
                    settings.Width = dockContent.Width;
                    settings.Height = dockContent.Height;
                    await SaveSettingsAsync(key, settings);
                }
            };

            return dockContent;
        }

        private void DockContent_FormClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true;
        }

        private void _dockPanel_Layout(object sender, LayoutEventArgs e)
        {
            _ready = true;
        }

        public abstract DockContent Create();
    }
}
