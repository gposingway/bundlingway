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
        private readonly HttpClient _httpClient;        public HttpClientService()
        {
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
        }        public async Task DownloadFileAsync(string url, string localPath, IProgressReporter? progressReporter = null, System.Threading.CancellationToken cancellationToken = default)
        {
            LogToConsole($"Starting download from {url} to {localPath}");

            // Create a timeout cancellation token to prevent hanging
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            LogToConsole("Creating request...");

            // Create request with timeout handling
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            LogToConsole("Sending request with ResponseHeadersRead...");

            // Use ResponseHeadersRead to enable true streaming/chunked downloads
            // This avoids loading the entire file into memory and prevents locking
            // Use ConfigureAwait(false) to prevent potential deadlocks
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, combinedCts.Token).ConfigureAwait(false);
            
            LogToConsole("Response received, checking status...");
            
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            bool isSmallFile = totalBytes > 0 && totalBytes < 1024 * 1024; // Less than 1MB
            
            LogToConsole($"File size: {(totalBytes > 0 ? $"{totalBytes} bytes" : "unknown")} (small file: {isSmallFile})");            bool progressStarted = false;
            if (progressReporter != null && totalBytes > 0)
            {
                try
                {
                    LogToConsole($"Starting progress reporting for {totalBytes} bytes");
                    await progressReporter.StartProgressAsync(totalBytes, $"Downloading {Path.GetFileName(localPath)}").ConfigureAwait(false);
                    progressStarted = true;
                    LogToConsole("Progress reporting started successfully");
                }
                catch (Exception ex)
                {
                    LogToConsole($"Failed to start progress reporting: {ex.Message}");
                    // Continue without progress reporting
                    progressStarted = false;
                }
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            LogToConsole("Getting content stream...");

            // Get the content stream with ConfigureAwait(false)
            using var contentStream = await response.Content.ReadAsStreamAsync(combinedCts.Token).ConfigureAwait(false);
            
            LogToConsole("Creating file stream...");
            
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, FileOptions.SequentialScan);

            var buffer = new byte[8192];
            var totalRead = 0L;
            int bytesRead;

            LogToConsole("Starting data transfer...");

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, combinedCts.Token).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, combinedCts.Token).ConfigureAwait(false);
                totalRead += bytesRead;

                if (progressReporter != null)
                {
                    if (totalBytes > 0)
                    {
                        await progressReporter.UpdateProgressAsync(totalRead).ConfigureAwait(false);
                    }
                    else
                    {
                        // For unknown file size, show bytes downloaded
                        await progressReporter.UpdateProgressAsync(0, $"Downloaded {totalRead:N0} bytes").ConfigureAwait(false);
                    }
                }
            }

            await fileStream.FlushAsync(combinedCts.Token).ConfigureAwait(false);

            if (progressReporter != null && progressStarted)
            {
                await progressReporter.StopProgressAsync().ConfigureAwait(false);
            }

            LogToConsole($"Download completed successfully: {localPath}");
        }        public async Task DownloadFileAsync(string url, string localPath, Dictionary<string, string> headers, IProgressReporter? progressReporter = null, System.Threading.CancellationToken cancellationToken = default)
        {
            LogToConsole($"Starting download with headers from {url} to {localPath}");

            // Create a timeout cancellation token to prevent hanging
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            using var request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
            
            LogToConsole("Sending request with headers and ResponseHeadersRead...");

            // Use ResponseHeadersRead to enable true streaming/chunked downloads
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, combinedCts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            bool isSmallFile = totalBytes > 0 && totalBytes < 1024 * 1024; // Less than 1MB
            
            LogToConsole($"File size: {(totalBytes > 0 ? $"{totalBytes} bytes" : "unknown")} (small file: {isSmallFile})");

            bool progressStarted = false;
            if (progressReporter != null && totalBytes > 0)
            {
                await progressReporter.StartProgressAsync(totalBytes, $"Downloading {Path.GetFileName(localPath)}"). ConfigureAwait(false);
                progressStarted = true;
            }

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

                if (progressReporter != null)
                {
                    if (totalBytes > 0)
                    {
                        await progressReporter.UpdateProgressAsync(totalRead).ConfigureAwait(false);
                    }
                    else
                    {
                        // For unknown file size, show bytes downloaded
                        await progressReporter.UpdateProgressAsync(0, $"Downloaded {totalRead:N0} bytes").ConfigureAwait(false);
                    }
                }
            }

            await fileStream.FlushAsync(combinedCts.Token).ConfigureAwait(false);            if (progressReporter != null && progressStarted)
            {
                await progressReporter.StopProgressAsync().ConfigureAwait(false);
            }

            LogToConsole($"Download with headers completed successfully: {localPath}");
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

        /// <summary>
        /// Safely logs to console if available, ignoring any exceptions.
        /// </summary>
        /// <param name="message">The message to log</param>
        private static void LogToConsole(string message)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [HTTP] {message}");
            }
            catch
            {
                // Console might not be available, ignore silently
            }
        }
    }
}
