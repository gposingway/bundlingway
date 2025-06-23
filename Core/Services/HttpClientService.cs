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
        private readonly HttpClient _httpClient; public HttpClientService()
        {
            _httpClient = new HttpClient();

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

        public async Task DownloadFileAsync(string url, string localPath, IProgressReporter? progressReporter = null, System.Threading.CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            bool progressStarted = false;
            if (progressReporter != null && totalBytes > 0)
            {
                await progressReporter.StartProgressAsync(totalBytes, $"Downloading {Path.GetFileName(localPath)}");
                progressStarted = true;
            }

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            var totalRead = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalRead += bytesRead;

                if (progressReporter != null && totalBytes > 0)
                {
                    await progressReporter.UpdateProgressAsync(totalRead);
                }
            }
            if (progressReporter != null && progressStarted)
            {
                await progressReporter.StopProgressAsync();
            }
        }

        public async Task DownloadFileAsync(string url, string localPath, Dictionary<string, string> headers, IProgressReporter? progressReporter = null, System.Threading.CancellationToken cancellationToken = default)
        {
            using var request = CreateRequestWithHeaders(HttpMethod.Get, url, headers);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            bool progressStarted = false;
            if (progressReporter != null && totalBytes > 0)
            {
                await progressReporter.StartProgressAsync(totalBytes, $"Downloading {Path.GetFileName(localPath)}");
                progressStarted = true;
            }

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            var totalRead = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalRead += bytesRead;

                if (progressReporter != null && totalBytes > 0)
                {
                    await progressReporter.UpdateProgressAsync(totalRead);
                }
            }

            if (progressReporter != null && progressStarted)
            {
                await progressReporter.StopProgressAsync();
            }
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
    }
}
