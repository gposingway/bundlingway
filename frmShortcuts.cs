using Bundlingway.Utilities;

namespace Bundlingway
{
    public partial class frmShortcuts : Form
    {
        private Dictionary<string, string> temporaryShortcuts = new Dictionary<string, string>();

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
                string shortcutKey = GetShortcutKeyFromSettings(textBox.Tag.ToString());
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
                switch (keyName)
                {
                    case "Add":
                        keyName = "Plus";
                        break;
                    case "Subtract":
                        keyName = "Minus";
                        break;
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
            else if (e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.Menu)
            {
                string shortcut = ((int)e.KeyCode).ToString();
                if (e.Control) shortcut += ",1"; else shortcut += ",0";
                if (e.Shift) shortcut += ",1"; else shortcut += ",0";
                if (e.Alt) shortcut += ",1"; else shortcut += ",0";

                e.Handled = true;
                e.SuppressKeyPress = true;

                textBox.Font = new Font(textBox.Font, FontStyle.Bold);

                temporaryShortcuts[textBox.Tag.ToString()] = shortcut;

                btnSave.Enabled = true;

                FetchAllShortcuts();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (var kvp in temporaryShortcuts)
                Instances.LocalConfigProvider.Configuration.Shortcuts[kvp.Key] = kvp.Value;

            Instances.LocalConfigProvider.Save();

            _ = UI.Announce("Shortcuts saved!");
            Close();
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
    }
}
