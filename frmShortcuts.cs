using Bundlingway.Utilities;

namespace Bundlingway
{
    public partial class frmShortcuts : Form
    {
        private Dictionary<string, string> temporaryShortcuts = new Dictionary<string, string>();

        private bool _mustPropagate = false;

        private static readonly List<Keys> IgnoredKeys =
        [
            Keys.ShiftKey,
            Keys.ControlKey,
            Keys.Menu,
            Keys.Tab,
            Keys.CapsLock,
            Keys.Enter,
            Keys.Apps,
            Keys.LWin,
            Keys.RWin
        ];



        private static readonly Dictionary<string, string> KeyNameMappings = new()
        {
            { "Add", "Plus" },
            { "Subtract", "Minus" },
            { "OemPeriod", "." },
            { "Oem2", "/" },
            { "Oem7", "\"" },
            { "OemSemicolon", ";" },
            { "Oem4", "[" },
            { "Oem6", "]" },
            { "Oemcomma", "," },
            { "OemMinus", "-" },
            { "Oemplus", "=" },
            { "Oem1", ";" },
            { "Oem5", "\\" },
            { "Oem102", "<" },
            { "Oem3", "`" },
            { "OemPipe", "|" },
            { "D1", "1" },
            { "D2", "2" },
            { "D3", "3" },
            { "D4", "4" },
            { "D5", "5" },
            { "D6", "6" },
            { "D7", "7" },
            { "D8", "8" },
            { "D9", "9" },
            { "D0", "0" }
        };

        public frmShortcuts()
        {
            InitializeComponent();
        }

        private void fromShortcuts_Load(object sender, EventArgs e)
        {
            var textBoxes = GetAllTextBoxes(this).ToList();

            foreach (TextBox textBox in textBoxes)
            {
                InitializeShortcutTextBox(textBox);
            }


            FetchAllShortcuts();
        }

        private void FetchAllShortcuts()
        {
            var textBoxes = GetAllTextBoxes(this).ToList();

            foreach (TextBox textBox in textBoxes)
            {

                var key = textBox.Tag.ToString();

                string shortcutKey = GetShortcutKeyFromSettings(key);

                temporaryShortcuts[key] = shortcutKey;

                if (!string.IsNullOrEmpty(shortcutKey))
                {
                    textBox.Text = FormatShortcutString(shortcutKey);
                }
            }
        }

        private string GetShortcutKeyFromSettings(string textBoxName)
        {
            if (temporaryShortcuts.TryGetValue(textBoxName, out string shortcutKey))
            {
                return shortcutKey;
            }

            if (Utilities.Handler.ReShadeConfig.Shortcuts.TryGetValue(textBoxName, out shortcutKey))
            {
                return shortcutKey;
            }


            if (Instances.LocalConfigProvider.Configuration.Shortcuts.TryGetValue(textBoxName, out shortcutKey))
            {
                return shortcutKey;
            }

            if (Constants.DefaultShortcuts.TryGetValue(textBoxName, out shortcutKey))
            {
                return shortcutKey;
            }

            return null;
        }

        private string FormatShortcutString(string shortcut)
        {
            string[] parts = shortcut.Split(',');
            if (parts.Length == 4)
            {
                int keyCode = int.Parse(parts[0]);
                bool ctrl = parts[1] == "1";
                bool shift = parts[2] == "1";
                bool alt = parts[3] == "1";

                string formattedString = "";
                if (ctrl) formattedString += "Ctrl+";
                if (shift) formattedString += "Shift+";
                if (alt) formattedString += "Alt+";

                string keyName = ((Keys)keyCode).ToString();

                if (KeyNameMappings.TryGetValue(keyName, out string mappedName))
                {
                    keyName = mappedName;
                }

                formattedString += keyName;

                return formattedString;
            }
            return "";
        }

        private IEnumerable<TextBox> GetAllTextBoxes(Control control)
        {
            foreach (Control child in control.Controls)
            {
                if (child is TextBox textBox)
                {
                    yield return textBox;
                }
                foreach (TextBox subTextBox in GetAllTextBoxes(child))
                {
                    yield return subTextBox;
                }
            }
        }

        public void InitializeShortcutTextBox(TextBox textBox)
        {
            textBox.KeyDown += (sender, e) => HandleShortcutKeyDown(textBox, e);
        }

        private void HandleShortcutKeyDown(TextBox textBox, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.ActiveControl = null;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                temporaryShortcuts.Remove(textBox.Tag.ToString());
                textBox.Text = string.Empty;
                btnSave.Enabled = true;
            }
            else if (!IgnoredKeys.Contains(e.KeyCode))
            {
                string shortcut = ((int)e.KeyCode).ToString();
                if (e.Control) shortcut += ",1"; else shortcut += ",0";
                if (e.Shift) shortcut += ",1"; else shortcut += ",0";
                if (e.Alt) shortcut += ",1"; else shortcut += ",0";

                e.Handled = true;
                e.SuppressKeyPress = true;

                textBox.Font = new Font(textBox.Font, FontStyle.Bold);

                if (textBox.Tag.ToString().Contains("@"))
                {
                    _mustPropagate = true;
                    lblWarning.Visible = true;
                }

                temporaryShortcuts[textBox.Tag.ToString()] = shortcut;

                btnSave.Enabled = true;

                FetchAllShortcuts();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DisableAllElements();

            btnSave.Text = "Updating...";

            Refresh();

            foreach (var kvp in temporaryShortcuts)
                Instances.LocalConfigProvider.Configuration.Shortcuts[kvp.Key] = kvp.Value;

            Instances.LocalConfigProvider.Save();

            Utilities.Handler.ReShadeConfig.SaveShortcuts(temporaryShortcuts);

            if (_mustPropagate)
            {
                _ = UI.Announce("Shortcuts saved! Updating installed presets...");
                _ = Utilities.Handler.Package.RefreshInstalled();
                _ = UI.Announce("Installed presets updated!");
            }

            Close();
        }

        private void DisableAllElements()
        {
            foreach (Control control in Controls)
            {
                control.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _ = UI.Announce("A-Okay!");
            Close();
        }

        private void btnCancel_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce(((Control)sender).Tag.ToString());
        }

        private void btnSave_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce(((Control)sender).Tag.ToString());
        }

        private void btnApplyAll_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce(((Control)sender).Tag.ToString());
        }

        private void btnDefault_MouseEnter(object sender, EventArgs e)
        {
            _ = UI.Announce(((Control)sender).Tag.ToString());
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            var textBoxes = GetAllTextBoxes(this).ToList();

            foreach (var kvp in Constants.DefaultShortcuts)
            {
                if (temporaryShortcuts.TryGetValue(kvp.Key, out string currentShortcut) && currentShortcut != kvp.Value)
                {
                    temporaryShortcuts[kvp.Key] = kvp.Value;

                    var textBox = textBoxes.FirstOrDefault(tb => tb.Tag.ToString() == kvp.Key);
                    if (textBox != null)
                    {
                        textBox.Text = FormatShortcutString(kvp.Value);
                        textBox.Font = new Font(textBox.Font, FontStyle.Bold);

                        if (textBox.Tag.ToString().Contains("@"))
                        {
                            _mustPropagate = true;
                            lblWarning.Visible = true;
                        }

                        btnSave.Enabled = true;
                    }
                }
            }
        }
    }
}
