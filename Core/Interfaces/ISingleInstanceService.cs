namespace Bundlingway.Core.Interfaces
{
    public interface ISingleInstanceService
    {
        /// <summary>
        /// Checks if another instance of the application is already running.
        /// </summary>
        /// <returns>True if another instance is running, false otherwise</returns>
        Task<bool> IsAnotherInstanceRunningAsync();

        /// <summary>
        /// Acquires the single-instance mutex to prevent other instances from starting.
        /// </summary>
        /// <returns>A disposable that releases the mutex when disposed</returns>
        IDisposable AcquireInstanceLock();
    }
}
