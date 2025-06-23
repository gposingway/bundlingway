using Bundlingway.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// HTTP client service implementation using HttpClient.
    /// </summary>
    public class HttpClientService : IHttpClientService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IUserNotificationService _notificationService;

        public HttpClientService(IUserNotificationService notificationService)
        {
            _notificationService = notificationService;
            _httpClient = new HttpClient(new HttpClientHandler()
            {
                // Configure to prevent potential threading issues
                UseCookies = false,
                UseDefaultCredentials = false
            });

            // Set timeouts to prevent hanging
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // 10 minute total timeout

            // Set a standard user-agent to avoid API rejections
            // GitHub API specifically requires a proper User-Agent header
            SetUserAgent("Bundlingway/1.0 (Windows; .NET; +https://github.com/gposingway/bundlingway)");
        }
        public async Task<string> GetStringAsync(string url) => await _httpClient.GetStringAsync(url);

        public async Task<string> GetStringAsync(string url, Dictionary<string, string> headers)
        {
            using var request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<byte[]> GetByteArrayAsync(string url) => await _httpClient.GetByteArrayAsync(url);

        public async Task<byte[]> GetByteArrayAsync(string url, Dictionary<string, string> headers)
        {
            using var request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<Stream> GetStreamAsync(string url) => await _httpClient.GetStreamAsync(url);

        public async Task<Stream> GetStreamAsync(string url, Dictionary<string, string> headers)
        {
            using var request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
        public async Task DownloadFileAsync(string url, string localPath, System.Threading.CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, combinedCts.Token).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;

            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var contentStream = await response.Content.ReadAsStreamAsync(combinedCts.Token).ConfigureAwait(false);

            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, FileOptions.SequentialScan);

            var buffer = new byte[8192];
            var totalRead = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, combinedCts.Token).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), combinedCts.Token).ConfigureAwait(false);
                totalRead += bytesRead;
            }

            await fileStream.FlushAsync(combinedCts.Token).ConfigureAwait(false);
        }
        public async Task DownloadFileAsync(string url, string localPath, Dictionary<string, string> headers, System.Threading.CancellationToken cancellationToken = default)
        {
            await _notificationService.ShowInfoAsync($"Starting download with headers from {url} to {localPath}");

            // Create a timeout cancellation token to prevent hanging
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            using var request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);

            await _notificationService.ShowInfoAsync("Sending request with headers and ResponseHeadersRead...");

            // Use ResponseHeadersRead to enable true streaming/chunked downloads
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, combinedCts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            await _notificationService.ShowInfoAsync($"File size: {(totalBytes > 0 ? $"{totalBytes} bytes" : "unknown")}");

            // Ensure directory exists
            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var contentStream = await response.Content.ReadAsStreamAsync(combinedCts.Token).ConfigureAwait(false);
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, FileOptions.SequentialScan);

            var buffer = new byte[8192];
            var totalRead = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, combinedCts.Token).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, combinedCts.Token).ConfigureAwait(false);
                totalRead += bytesRead;
            }

            await fileStream.FlushAsync(combinedCts.Token).ConfigureAwait(false);
            await _notificationService.ShowSuccessAsync($"Download with headers completed successfully: {localPath}");
        }
        public async Task<HttpResponseMessage> GetAsync(string url) => await _httpClient.GetAsync(url);

        public async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string> headers)
        {
            using var request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content) => await _httpClient.PostAsync(url, content);

        public void SetUserAgent(string userAgent)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        public void AddDefaultHeader(string name, string value)
        {
            _httpClient.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Creates an HTTP request message with custom headers.
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Request URL</param>
        /// <param name="headers">Custom headers to add</param>
        /// <returns>HttpRequestMessage with custom headers</returns>
        private HttpRequestMessage CreateRequestWithHeaders(HttpMethod method, string url, Dictionary<string, string> headers)
        {
            var request = new HttpRequestMessage(method, url);

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return request;
        }
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        // Overloads with IProgressReporter parameters for interface compliance
        // Progress reporting is ignored to avoid threading issues
        public async Task DownloadFileAsync(string url, string localPath, IProgressReporter? progressReporter, System.Threading.CancellationToken cancellationToken = default)
        {
            // Ignore progress reporter to avoid threading issues
            await DownloadFileAsync(url, localPath, cancellationToken);
        }

        public async Task DownloadFileAsync(string url, string localPath, Dictionary<string, string> headers, IProgressReporter? progressReporter, System.Threading.CancellationToken cancellationToken = default)
        {
            // Ignore progress reporter to avoid threading issues
            await DownloadFileAsync(url, localPath, headers, cancellationToken);
        }
    }
}
