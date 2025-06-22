using Bundlingway.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO.Pipes;
using System.Text;

namespace Bundlingway.Core.Services
{
    public class ProtocolServerService : IProtocolServerService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _pipeName;
        private CancellationTokenSource? _cancellationTokenSource;

        public ProtocolServerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _pipeName = "BundlingwayProtocol";
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            try
            {
                await RunServerLoopAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Log.Information("Protocol server stopped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Protocol server encountered an error");
            }
        }

        private async Task RunServerLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(_pipeName, PipeDirection.In);
                    await server.WaitForConnectionAsync(cancellationToken);

                    byte[] buffer = new byte[1024];
                    int bytesRead = await server.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    await HandleIncomingMessageAsync(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error in protocol server loop");
                    await Task.Delay(1000, cancellationToken); // Wait before retrying
                }
            }
        }        private async Task HandleIncomingMessageAsync(string message)
        {
            try
            {
                if (message == "BRING_TO_FRONT")
                {
                    await HandleBringToFrontRequestAsync();
                }
                else if (message.StartsWith("NOTIFICATION:"))
                {
                    string notificationText = message.Substring("NOTIFICATION:".Length);
                    await HandleNotificationAsync(notificationText);
                }
                else
                {
                    Log.Warning("Unknown protocol message received: {Message}", message);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to handle incoming protocol message: {Message}", message);
            }
        }        private async Task HandleBringToFrontRequestAsync()
        {
            try
            {
                // Use the notification service to bring window to front
                var notificationService = _serviceProvider.GetRequiredService<IUserNotificationService>();
                await notificationService.BringToFrontAsync();
                
                Log.Information("Brought main window to front via protocol request");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to bring main window to front");
            }
        }

        private async Task HandleNotificationAsync(string notificationText)
        {
            try
            {
                var notificationService = _serviceProvider.GetRequiredService<IUserNotificationService>();
                await notificationService.AnnounceAsync(notificationText);
                
                Log.Information("Displayed notification via protocol: {Notification}", notificationText);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to display protocol notification: {Notification}", notificationText);
            }        }

        public async Task<bool> SendMessageToExistingInstanceAsync(string message)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
                await client.ConnectAsync(2000); // 2 second timeout

                byte[] data = Encoding.UTF8.GetBytes(message);
                await client.WriteAsync(data, 0, data.Length);
                await client.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to send message to existing instance: {Message}", message);
                return false;
            }
        }

        public async Task<bool> NotifyExistingInstanceAsync(string notificationText)
        {
            return await SendMessageToExistingInstanceAsync($"NOTIFICATION:{notificationText}");
        }

        public async Task<bool> BringExistingInstanceToFrontAsync()
        {
            return await SendMessageToExistingInstanceAsync("BRING_TO_FRONT");
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
