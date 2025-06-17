using Bundlingway.Core.Interfaces;
using Bundlingway.Utilities;

namespace Bundlingway
{
    public partial class frmShortcuts : Form
    {
        private Dictionary<string, string> temporaryShortcuts = [];

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

        private readonly IPackageService _packageService;
        private readonly IConfigurationService _configService;


        public frmShortcuts()
        {
            InitializeComponent();
            _packageService = Core.Services.ServiceLocator.TryGetService<IPackageService>()!;
            _configService = Core.Services.ServiceLocator.TryGetService<IConfigurationService>()!;
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

                var key = textBox.Tag?.ToString() ?? string.Empty;

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
            var safeTextBoxName = textBoxName ?? string.Empty;
            if (!string.IsNullOrEmpty(safeTextBoxName) && temporaryShortcuts.TryGetValue(safeTextBoxName, out var shortcutKey) && shortcutKey != null)
            {
                return shortcutKey;
            }

            if (!string.IsNullOrEmpty(safeTextBoxName) && Utilities.Handler.ReShadeConfig.Shortcuts.TryGetValue(safeTextBoxName, out var shortcutKey2) && shortcutKey2 != null)
            {
                return shortcutKey2;
            }


            if (!string.IsNullOrEmpty(safeTextBoxName) && _configService.Configuration.Shortcuts.TryGetValue(safeTextBoxName, out var shortcutKey3) && shortcutKey3 != null)
            {
                return shortcutKey3;
            }

            if (!string.IsNullOrEmpty(safeTextBoxName) && Constants.DefaultShortcuts.TryGetValue(safeTextBoxName, out var shortcutKey4) && shortcutKey4 != null)
            {
                return shortcutKey4;
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

                var safeKeyName = keyName ?? string.Empty;
                if (!string.IsNullOrEmpty(safeKeyName) && KeyNameMappings.TryGetValue(safeKeyName, out var mappedName) && mappedName != null)
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
                temporaryShortcuts.Remove(textBox.Tag?.ToString() ?? string.Empty);
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

                if ((textBox.Tag?.ToString() ?? string.Empty).Contains("@"))
                {
                    _mustPropagate = true;
                    lblWarning.Visible = true;
                }

                temporaryShortcuts[textBox.Tag?.ToString() ?? string.Empty] = shortcut;

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
                _configService.Configuration.Shortcuts[kvp.Key] = kvp.Value;

            _configService.Save();

            Utilities.Handler.ReShadeConfig.SaveShortcuts(temporaryShortcuts);

            if (_mustPropagate)
            {
                _ = ModernUI.Announce("Shortcuts saved! Updating installed presets...");
                _ = _packageService.ScanPackagesAsync();
                _ = ModernUI.Announce("Installed presets updated!");
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
            _ = ModernUI.Announce("A-Okay!");
            Close();
        }

        private void btnCancel_MouseEnter(object sender, EventArgs e)
        {
            _ = ModernUI.Announce(((Control)sender).Tag?.ToString() ?? string.Empty);
        }

        private void btnSave_MouseEnter(object sender, EventArgs e)
        {
            _ = ModernUI.Announce(((Control)sender).Tag?.ToString() ?? string.Empty);
        }

        private void btnApplyAll_MouseEnter(object sender, EventArgs e)
        {
            _ = ModernUI.Announce(((Control)sender).Tag?.ToString() ?? string.Empty);
        }

        private void btnDefault_MouseEnter(object sender, EventArgs e)
        {
            _ = ModernUI.Announce(((Control)sender).Tag?.ToString() ?? string.Empty);
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            var textBoxes = GetAllTextBoxes(this).ToList();

            foreach (var kvp in Constants.DefaultShortcuts)
            {
                var safeKvpKey = kvp.Key ?? string.Empty;
                if (!string.IsNullOrEmpty(safeKvpKey) && temporaryShortcuts.TryGetValue(safeKvpKey, out var currentShortcut) && currentShortcut != null && currentShortcut != kvp.Value)
                {
                    temporaryShortcuts[safeKvpKey] = kvp.Value;

                    var textBox = textBoxes.FirstOrDefault(tb => (tb.Tag?.ToString() ?? string.Empty) == kvp.Key);
                    if (textBox != null)
                    {
                        textBox.Text = FormatShortcutString(kvp.Value);
                        textBox.Font = new Font(textBox.Font, FontStyle.Bold);

                        if ((textBox.Tag?.ToString() ?? string.Empty).Contains("@"))
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
