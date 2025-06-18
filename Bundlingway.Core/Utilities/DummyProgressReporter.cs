using Bundlingway.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Utilities
{
    public class DummyProgressReporter : IProgressReporter
    {
        public Task StartProgressAsync(long total, string? description = null) => Task.CompletedTask;
        public Task UpdateProgressAsync(long current, string? description = null) => Task.CompletedTask;
        public Task UpdateProgressAsync(double percentage, string? description = null) => Task.CompletedTask;
        public Task StopProgressAsync() => Task.CompletedTask;
        public bool IsProgressActive => false;
        public event EventHandler<ProgressEventArgs>? ProgressUpdated { add { } remove { } }
    }
}
