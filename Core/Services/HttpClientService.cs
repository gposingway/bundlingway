using Bundlingway.Core.Interfaces;
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

        public HttpClientService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetStringAsync(string url) => await _httpClient.GetStringAsync(url);

        public async Task<byte[]> GetByteArrayAsync(string url) => await _httpClient.GetByteArrayAsync(url);

        public async Task<Stream> GetStreamAsync(string url) => await _httpClient.GetStreamAsync(url);

        public async Task DownloadFileAsync(string url, string localPath, IProgressReporter? progressReporter = null)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            
            if (progressReporter != null && totalBytes > 0)
            {
                await progressReporter.StartProgressAsync(totalBytes, $"Downloading {Path.GetFileName(localPath)}");
            }

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            var totalRead = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (progressReporter != null && totalBytes > 0)
                {
                    await progressReporter.UpdateProgressAsync(totalRead);
                }
            }

            if (progressReporter != null)
            {
                await progressReporter.StopProgressAsync();
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string url) => await _httpClient.GetAsync(url);

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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
