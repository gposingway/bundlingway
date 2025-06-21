using Bundlingway.Core.Interfaces;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Service for HTTP operations.
    /// Abstracts HTTP requests to enable testing and different HTTP implementations.
    /// </summary>
    public interface IHttpClientService
    {
        /// <summary>
        /// Downloads content from a URL as string.
        /// </summary>
        /// <param name="url">URL to download from</param>
        Task<string> GetStringAsync(string url);

        /// <summary>
        /// Downloads content from a URL as byte array.
        /// </summary>
        /// <param name="url">URL to download from</param>
        Task<byte[]> GetByteArrayAsync(string url);

        /// <summary>
        /// Downloads content from a URL as stream.
        /// </summary>
        /// <param name="url">URL to download from</param>
        Task<Stream> GetStreamAsync(string url);

        /// <summary>
        /// Downloads a file from URL to local path with progress reporting.
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="localPath">Local file path to save to</param>
        /// <param name="progressReporter">Optional progress reporter</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        Task DownloadFileAsync(string url, string localPath, IProgressReporter? progressReporter = null, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an HTTP response message.
        /// </summary>
        /// <param name="url">URL to request</param>
        Task<HttpResponseMessage> GetAsync(string url);

        /// <summary>
        /// Posts content to a URL.
        /// </summary>
        /// <param name="url">URL to post to</param>
        /// <param name="content">Content to post</param>
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);

        /// <summary>
        /// Sets the user agent for HTTP requests.
        /// </summary>
        /// <param name="userAgent">User agent string</param>
        void SetUserAgent(string userAgent);

        /// <summary>
        /// Adds a default header to all requests.
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Header value</param>
        void AddDefaultHeader(string name, string value);
    }
}
