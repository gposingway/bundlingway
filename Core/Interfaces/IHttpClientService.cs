using Bundlingway.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{    /// <summary>
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
        /// Downloads content from a URL as string with custom headers.
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="headers">Custom headers to include in the request</param>
        Task<string> GetStringAsync(string url, Dictionary<string, string> headers);

        /// <summary>
        /// Downloads content from a URL as byte array.
        /// </summary>
        /// <param name="url">URL to download from</param>
        Task<byte[]> GetByteArrayAsync(string url);

        /// <summary>
        /// Downloads content from a URL as byte array with custom headers.
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="headers">Custom headers to include in the request</param>
        Task<byte[]> GetByteArrayAsync(string url, Dictionary<string, string> headers);

        /// <summary>
        /// Downloads content from a URL as stream.
        /// </summary>
        /// <param name="url">URL to download from</param>
        Task<Stream> GetStreamAsync(string url);

        /// <summary>
        /// Downloads content from a URL as stream with custom headers.
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="headers">Custom headers to include in the request</param>
        Task<Stream> GetStreamAsync(string url, Dictionary<string, string> headers);

        /// <summary>
        /// Downloads a file from URL to local path with progress reporting.
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="localPath">Local file path to save to</param>
        /// <param name="progressReporter">Optional progress reporter</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        Task DownloadFileAsync(string url, string localPath, IProgressReporter? progressReporter = null, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a file from URL to local path with progress reporting and custom headers.
        /// </summary>
        /// <param name="url">URL to download from</param>
        /// <param name="localPath">Local file path to save to</param>
        /// <param name="headers">Custom headers to include in the request</param>
        /// <param name="progressReporter">Optional progress reporter</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        Task DownloadFileAsync(string url, string localPath, Dictionary<string, string> headers, IProgressReporter? progressReporter = null, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an HTTP response message.
        /// </summary>
        /// <param name="url">URL to request</param>
        Task<HttpResponseMessage> GetAsync(string url);

        /// <summary>
        /// Gets an HTTP response message with custom headers.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="headers">Custom headers to include in the request</param>
        Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers);

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
