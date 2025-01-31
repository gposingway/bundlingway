using Microsoft.Win32;
using Serilog;
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
            Log.Information($"Checking if process {processName} is running.");
            Process[] processes = Process.GetProcessesByName(processName);
            bool isRunning = processes.Length > 0;
            Log.Information($"Process {processName} running: {isRunning}");
            return isRunning;
        }

        public static string GetProcessPath(string processName)
        {
            Log.Information($"Getting path for process {processName}.");
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length > 0)
            {
                string path = processes[0].MainModule.FileName;
                Log.Information($"Path for process {processName}: {path}");
                return path;
            }
            else
            {
                Log.Information($"Process {processName} is not running.");
                return null; // Process is not running
            }
        }

        public static void NotifyOtherInstances(string eventName)
        {
            Log.Information($"Notifying other instances with event: {eventName}");
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "BundlingwayEventNotification", PipeDirection.Out))
            {
                try
                {
                    pipeClient.Connect(1000); // Wait for 1 second to connect
                    using (StreamWriter writer = new(pipeClient, Encoding.UTF8))
                    {
                        writer.WriteLine(eventName);
                    }
                    Log.Information("Notification sent successfully.");
                }
                catch (TimeoutException)
                {
                    Log.Information("No other instances are listening.");
                    // Handle the case where no other instances are listening
                }
            }
        }

        public static async Task ListenForNotifications()
        {
            Log.Information("Listening for notifications.");
            using (NamedPipeServerStream pipeServer = new("BundlingwayEventNotification", PipeDirection.In))
            {
                while (true)
                {
                    await pipeServer.WaitForConnectionAsync();
                    using (StreamReader reader = new(pipeServer, Encoding.UTF8))
                    {
                        string message;
                        while ((message = await reader.ReadLineAsync()) != null)
                        {
                            Log.Information($"Notification received: {message}");
                            NotificationReceived?.Invoke(null, message);
                        }
                    }
                    pipeServer.Disconnect();
                }
            }
        }

        public static async Task PinToStartScreenAsync()
        {
            Log.Information("Pinning application to start screen.");

            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string appName = Path.GetFileNameWithoutExtension(appPath);
            string appUserModelId = Constants.AppUserModelId;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\" + appName, true) ?? Registry.CurrentUser.CreateSubKey(@"Software\Classes\Applications\" + appName))
            {
                string registeredAppPath = key.GetValue("ApplicationPath") as string;
                if (registeredAppPath != appPath)
                {
                    key.SetValue("ApplicationName", appName);
                    key.SetValue("ApplicationDescription", "Description of the app");
                    key.SetValue("ApplicationIcon", appPath);
                    key.SetValue("ApplicationPath", appPath);
                    key.SetValue("AppUserModelID", appUserModelId);
                    Log.Information("Application registry keys set.");
                }
            }

            using (RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage", true))
            {
                string pinnedApps = explorerKey.GetValue("PinnedApps") as string;
                if (pinnedApps != appPath)
                {
                    explorerKey.SetValue("PinnedApps", appPath, RegistryValueKind.String);
                    Log.Information("Application pinned to start screen.");
                }
            }
        }
    }
}