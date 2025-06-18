using Bundlingway.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// Console-based implementation of IProgressReporter for headless mode.
    /// </summary>
    public class ConsoleProgressReporter : IProgressReporter
    {
        private long _total;
        private long _current;
        private string? _description;

        public Task StartProgressAsync(long total, string? description = null)
        {
            _total = total;
            _current = 0;
            _description = description;
            Console.WriteLine($"[PROGRESS] Started: {description ?? "Operation"} (Total: {total})");
            return Task.CompletedTask;
        }

        public Task UpdateProgressAsync(long current)
        {
            _current = current;
            Console.WriteLine($"[PROGRESS] {(_description ?? "Operation")}: {_current}/{_total}");
            return Task.CompletedTask;
        }

        public Task UpdateProgressAsync(long current, string? description = null)
        {
            _current = current;
            if (description != null) _description = description;
            Console.WriteLine($"[PROGRESS] {(_description ?? "Operation")}: {_current}/{_total}");
            ProgressUpdated?.Invoke(this, new ProgressEventArgs { Current = _current, Total = _total, Description = _description, IsCompleted = false });
            return Task.CompletedTask;
        }

        public Task UpdateProgressAsync(double percentage, string? description = null)
        {
            if (description != null) _description = description;
            Console.WriteLine($"[PROGRESS] {(_description ?? "Operation")}: {percentage:0.##}%");
            ProgressUpdated?.Invoke(this, new ProgressEventArgs { Current = (long)(_total * percentage / 100), Total = _total, Description = _description, IsCompleted = false });
            return Task.CompletedTask;
        }

        public Task StopProgressAsync()
        {
            Console.WriteLine($"[PROGRESS] Completed: {_description ?? "Operation"}");
            return Task.CompletedTask;
        }

        public bool IsProgressActive => _current < _total;

        public event EventHandler<ProgressEventArgs>? ProgressUpdated;
    }
}
