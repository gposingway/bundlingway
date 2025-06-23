using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Service for handling operations that require elevated privileges.
    /// Follows the principle of least privilege by requesting elevation only when needed.
    /// </summary>
    public interface IElevationService
    {
        /// <summary>
        /// Checks if the current process is running with elevated privileges.
        /// </summary>
        bool IsElevated { get; }

        /// <summary>
        /// Requests elevation for a specific operation.
        /// If already elevated, returns true immediately.
        /// If not elevated, prompts user and restarts application with elevation if approved.
        /// </summary>
        /// <param name="operationName">Human-readable name of the operation requiring elevation</param>
        /// <param name="operationId">Unique identifier for the operation</param>
        /// <param name="arguments">Additional arguments to pass to the elevated process</param>
        /// <returns>True if elevation is available, false if denied or failed</returns>
        Task<bool> RequestElevationAsync(string operationName, string operationId, string arguments = "");

        /// <summary>
        /// Executes an operation that requires elevation.
        /// Handles elevation request internally if needed.
        /// </summary>
        /// <param name="operationName">Human-readable name of the operation</param>
        /// <param name="operationId">Unique identifier for the operation</param>
        /// <param name="operation">The operation to execute when elevated</param>
        /// <param name="fallback">Optional fallback action if elevation is denied</param>
        /// <returns>True if operation was executed successfully</returns>
        Task<bool> ExecuteElevatedAsync(string operationName, string operationId, Func<Task> operation, Func<Task>? fallback = null);
    }
}
