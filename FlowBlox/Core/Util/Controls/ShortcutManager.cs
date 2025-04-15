using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowBlox.Core.Util.Controls
{
    public class ManagedShortcut
    {
        public Keys ShortcutKeys { get; set; }
        public ToolStripButton Button { get; set; }

        public ManagedShortcut(Keys keys, ToolStripButton button)
        {
            ShortcutKeys = keys;
            Button = button;
        }
    }

    public class ShortcutManager
    {
        private List<ManagedShortcut> _shortcuts;
        private ToolStrip _toolStrip;
        private ToolStripButton _previousButton;
        private ManagedShortcut _processingShortcut;

        public ShortcutManager(ToolStrip toolStrip)
        {
            _toolStrip = toolStrip;
            _shortcuts = new List<ManagedShortcut>();
        }

        public void RegisterShortcut(Keys keys, ToolStripButton button)
        {
            if (!_shortcuts.Any(s => s.ShortcutKeys == keys))
            {
                _shortcuts.Add(new ManagedShortcut(keys, button));
                button.Text += $" [{keys.ToString()}]";
            }
        }

        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (_processingShortcut != null)
                return;

            foreach (var shortcut in _shortcuts)
            {
                if ((e.KeyData & shortcut.ShortcutKeys) == shortcut.ShortcutKeys)
                {
                    _previousButton = _toolStrip.Items.OfType<ToolStripButton>().FirstOrDefault(btn => btn.Checked);
                    _processingShortcut = shortcut;

                    ToolStripButton button = shortcut.Button;
                    if (_previousButton != null)
                    {
                        _previousButton.Checked = false;
                    }
                    button.Checked = true;
                    break;
                }
            }
        }

        public void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (_processingShortcut == null)
                return;

            ToolStripButton button = _processingShortcut.Button;
            button.Checked = false;
            _previousButton.Checked = true;
            _processingShortcut = null;
        }
    }
}