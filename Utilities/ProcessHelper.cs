using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace Bundlingway.Utilities
{
    public static class ProcessHelper
    {
        public static event EventHandler<string> NotificationReceived;

        public static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        public static string GetProcessPath(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                return processes[0].MainModule.FileName;
            }
            else
            {
                return null; // Process is not running
            }
        }


        public static void NotifyOtherInstances(string eventName)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "BundlingwayEventNotification", PipeDirection.Out))
            {
                try
                {
                    pipeClient.Connect(1000); // Wait for 1 second to connect
                    using (StreamWriter writer = new(pipeClient, Encoding.UTF8))
                    {
                        writer.WriteLine(eventName);
                    }
                }
                catch (TimeoutException)
                {
                    // Handle the case where no other instances are listening
                }
            }
        }

        public static async Task ListenForNotifications()
        {
            using (NamedPipeServerStream pipeServer = new("BundlingwayEventNotification", PipeDirection.In))
            {
                while (true)
                {
                    await pipeServer.WaitForConnectionAsync();
                    using (StreamReader reader = new(pipeServer, Encoding.UTF8))
                    {
                        string message = await reader.ReadLineAsync();
                        NotificationReceived?.Invoke(null, message);
                    }
                    pipeServer.Disconnect();
                }
            }
        }
    }
}