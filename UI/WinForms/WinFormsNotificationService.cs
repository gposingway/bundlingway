using Bundlingway.Core.Interfaces;
using Bundlingway.Utilities.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms;

namespace Bundlingway.UI.WinForms
{
    /// <summary>
    /// WinForms implementation of user notification service.
    /// Bridges core notifications to WinForms UI elements.
    /// </summary>
    public class WinFormsNotificationService : IUserNotificationService
    {
        private readonly frmLanding? _mainForm;

        public WinFormsNotificationService(frmLanding? mainForm = null)
        {
            _mainForm = mainForm;
        }

        public async Task AnnounceAsync(string message)
        {
            if (_mainForm != null)
            {
                await _mainForm.Announce(message);
            }
            
            // Fallback to console for headless scenarios
            if (_mainForm == null)
            {
                Console.WriteLine($"[INFO] {message}");
            }
        }

        public async Task ShowErrorAsync(string message, Exception? exception = null)
        {
            var fullMessage = exception != null ? $"{message}\n\nDetails: {exception.Message}" : message;
            
            if (_mainForm != null)
            {
                _mainForm.DoAction(() =>
                {
                    MessageBox.Show(fullMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            else
            {
                Console.WriteLine($"[ERROR] {fullMessage}");
            }

            await Task.CompletedTask;
        }

        public async Task ShowInfoAsync(string message)
        {
            if (_mainForm != null)
            {
                _mainForm.DoAction(() =>
                {
                    MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            else
            {
                Console.WriteLine($"[INFO] {message}");
            }

            await Task.CompletedTask;
        }

        public async Task ShowWarningAsync(string message)
        {
            if (_mainForm != null)
            {
                _mainForm.DoAction(() =>
                {
                    MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
            }
            else
            {
                Console.WriteLine($"[WARNING] {message}");
            }

            await Task.CompletedTask;
        }

        public async Task ShowSuccessAsync(string message)
        {
            await AnnounceAsync(message); // Treat success as announcement for now
        }

        public async Task<bool> ConfirmAsync(string message, string title = "Confirm")
        {
            if (_mainForm != null)
            {
                var result = DialogResult.No;
                _mainForm.DoAction(() =>
                {
                    result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                });
                return result == DialogResult.Yes;
            }
            else
            {
                // For headless mode, default to false for safety
                Console.WriteLine($"[CONFIRM] {message} (defaulting to No in headless mode)");
                return false;
            }
        }
    }
}
