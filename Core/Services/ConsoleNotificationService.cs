using Bundlingway.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Console-based implementation of IUserNotificationService for headless mode.
    /// </summary>
    public class ConsoleNotificationService : IUserNotificationService
    {
        public Task AnnounceAsync(string message)
        {
            Console.WriteLine($"[INFO] {message}");
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string message, Exception? exception = null)
        {
            Console.Error.WriteLine($"[ERROR] {message}");
            if (exception != null)
                Console.Error.WriteLine(exception);
            return Task.CompletedTask;
        }

        public Task ShowInfoAsync(string message)
        {
            Console.WriteLine($"[INFO] {message}");
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string message)
        {
            Console.WriteLine($"[WARNING] {message}");
            return Task.CompletedTask;
        }

        public Task ShowSuccessAsync(string message)
        {
            Console.WriteLine($"[SUCCESS] {message}");
            return Task.CompletedTask;
        }

        public Task<bool> ConfirmAsync(string message)
        {
            Console.Write($"[CONFIRM] {message} (y/n): ");
            var key = Console.ReadKey();
            Console.WriteLine();
            return Task.FromResult(key.KeyChar == 'y' || key.KeyChar == 'Y');
        }

        public Task<bool> ConfirmAsync(string message, string title = "Confirm")
        {
            Console.Write($"[CONFIRM] {title}: {message} (y/n): ");
            var key = Console.ReadKey();
            Console.WriteLine();
            return Task.FromResult(key.KeyChar == 'y' || key.KeyChar == 'Y');
        }

        public Task BringToFrontAsync()
        {
            // No-op for console mode - there's no window to bring to front
            return Task.CompletedTask;
        }
    }
}
