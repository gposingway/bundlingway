using System;
using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Service for reporting progress of long-running operations.
    /// Implementations will handle UI-specific progress display.
    /// </summary>
    public interface IProgressReporter
    {
        /// <summary>
        /// Starts a progress operation with a known total count.
        /// </summary>
        /// <param name="total">Total number of items to process</param>
        /// <param name="description">Optional description of the operation</param>
        Task StartProgressAsync(long total, string? description = null);

        /// <summary>
        /// Updates the current progress.
        /// </summary>
        /// <param name="current">Current number of items processed</param>
        /// <param name="description">Optional description update</param>
        Task UpdateProgressAsync(long current, string? description = null);

        /// <summary>
        /// Updates progress with percentage and description.
        /// </summary>
        /// <param name="percentage">Progress percentage (0-100)</param>
        /// <param name="description">Optional description update</param>
        Task UpdateProgressAsync(double percentage, string? description = null);

        /// <summary>
        /// Stops the progress operation.
        /// </summary>
        Task StopProgressAsync();

        /// <summary>
        /// Indicates whether a progress operation is currently active.
        /// </summary>
        bool IsProgressActive { get; }

        /// <summary>
        /// Event fired when progress is updated.
        /// </summary>
        event EventHandler<ProgressEventArgs>? ProgressUpdated;
    }

    /// <summary>
    /// Event arguments for progress updates.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        public long Current { get; set; }
        public long Total { get; set; }
        public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
