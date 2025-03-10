using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Microsoft.Win32;
using Serilog;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Text;

namespace Bundlingway.Utilities
{
    public static class ProcessHelper
    {
        public static event EventHandler<IPCNotification> NotificationReceived;

        public static void OpenUrlInBrowser(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public static string GetCurrentMethodName()
        {
            return MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name;
        }

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

        public static void NotifyOtherInstances(IPCNotification notification)
        {
            Log.Information($"Notifying other instances with event: {notification}");
            using NamedPipeClientStream pipeClient = new(".", "BundlingwayEventNotification", PipeDirection.Out);

            try
            {
                pipeClient.Connect(1000); // Wait for 1 second to connect
                using (StreamWriter writer = new(pipeClient, Encoding.UTF8))
                {
                    var payload = notification.ToSingleLineJson();
                    writer.Write(payload);
                }
            }
            catch (TimeoutException)
            {
                Log.Information("No other instances are listening.");
                // Handle the case where no other instances are listening
            }
        }

        public static async Task ListenForNotifications()
        {
            Log.Information("Listening for notifications - Recreating Pipe in Loop.");
            NamedPipeServerStream pipeServer = null;
            StreamReader reader = null; // Declare StreamReader outside using block

            while (true)
            {
                try
                {
                    pipeServer = new NamedPipeServerStream("BundlingwayEventNotification", PipeDirection.In);

                    Log.Information("Before WaitForConnectionAsync - Pipe Server IsConnected: {IsConnected}, IsHandleInvalid: {IsHandleInvalid}", pipeServer.IsConnected, pipeServer.SafePipeHandle.IsInvalid);

                    await pipeServer.WaitForConnectionAsync();

                    Log.Information("After WaitForConnectionAsync - Client connected. Pipe Server IsConnected: {IsConnected}, IsHandleInvalid: {IsHandleInvalid}", pipeServer.IsConnected, pipeServer.SafePipeHandle.IsInvalid);
                    Log.Information("Client connected.");

                    reader = new StreamReader(pipeServer, Encoding.UTF8); // Initialize StreamReader here
                    string message;

                    while ((message = await reader.ReadLineAsync()) != null)
                    {
                        var ipcNotification = message.FromJson<IPCNotification>();

                        if (ipcNotification != null)
                        {
                            Log.Information($"Notification received: {ipcNotification.Topic} / {ipcNotification.Message}");
                            try { NotificationReceived?.Invoke(null, ipcNotification); } catch (Exception e) { Log.Error(e, "Error while handling notification."); }
                        }
                    }

                    Log.Information("Client disconnected. Waiting for new connection.");
                    bool isHandleInvalidAfterDisconnect = pipeServer.SafePipeHandle.IsInvalid; // **GET IsHandleInvalid BEFORE LOGGING**
                    Log.Information("After Client Disconnect - Pipe Server IsConnected: {IsConnected}, IsHandleInvalid: {IsHandleInvalid}", pipeServer.IsConnected, isHandleInvalidAfterDisconnect); // Log it
                }
                catch (ObjectDisposedException disposedEx)
                {
                    Log.Error(disposedEx, "ObjectDisposedException in ListenForNotifications loop.");
                    break; // Exit loop on ObjectDisposedException
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "General Error in ListenForNotifications loop.");
                    await Task.Delay(1000);
                }
                finally
                {
                    if (reader != null) // Dispose of StreamReader first
                    {
                        try
                        {
                            reader.Close(); // Close StreamReader
                            reader.Dispose(); // Dispose StreamReader
                            Log.Information("StreamReader Disposed in finally block.");
                        }
                        catch (Exception disposeEx)
                        {
                            Log.Error(disposeEx, "Error disposing reader in finally block.");
                        }
                    }
                    if (pipeServer != null) // Then dispose of pipeServer
                    {
                        try
                        {
                            pipeServer.Close();
                            pipeServer.Dispose();
                            Log.Information("Pipe Server Disposed in finally block.");
                        }
                        catch (Exception disposeEx)
                        {
                            Log.Error(disposeEx, "Error disposing pipeServer in finally block.");
                        }
                    }
                }
            }
            Log.Information("ListenForNotifications loop exited.");
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

            using RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage", true);
            string pinnedApps = explorerKey.GetValue("PinnedApps") as string;
            if (pinnedApps != appPath)
            {
                explorerKey.SetValue("PinnedApps", appPath, RegistryValueKind.String);
                Log.Information("Application pinned to start screen.");
            }
        }

        internal static bool IsGameRunning() => IsProcessRunning(Constants.Files.GameProcess);



        public static void EnsureDesktopShortcut()
        {
            string appName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            EnsureDesktopShortcut(appName, appPath);
        }

        public static void EnsureDesktopShortcut(string shortcutName, string targetPath)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

            if (File.Exists(shortcutPath))
            {
                try
                {
                    File.Delete(shortcutPath);
                    Log.Information("Existing shortcut removed successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error removing existing shortcut.");
                    return;
                }
            }

            Log.Information($"Creating desktop shortcut: {shortcutPath}");

            try
            {
                // Create a new shortcut using COM objects
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    Log.Error("Failed to get WScript.Shell type.");
                    return;
                }

                dynamic shell = Activator.CreateInstance(shellType);
                if (shell == null)
                {
                    Log.Error("Failed to create WScript.Shell instance.");
                    return;
                }

                var shortcut = shell.CreateShortcut(shortcutPath);
                if (shortcut == null)
                {
                    Log.Error("Failed to create shortcut.");
                    return;
                }

                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Description = "Shortcut for " + shortcutName;
                shortcut.IconLocation = targetPath;
                shortcut.Save();

                Log.Information("Desktop shortcut created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating desktop shortcut.");
            }
        }

        public static string CheckDesktopShortcutStatus()
        {
            string appName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            return CheckDesktopShortcutStatus(appName, appPath);
        }

        public static string CheckDesktopShortcutStatus(string shortcutName, string expectedTargetPath)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

            if (!File.Exists(shortcutPath))
            {
                return "Not Created";
            }

            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    Log.Error("Failed to get WScript.Shell type.");
                    return "Error";
                }

                dynamic shell = Activator.CreateInstance(shellType);
                if (shell == null)
                {
                    Log.Error("Failed to create WScript.Shell instance.");
                    return "Error";
                }

                var shortcut = shell.CreateShortcut(shortcutPath);
                if (shortcut == null)
                {
                    Log.Error("Failed to create shortcut.");
                    return "Error";
                }

                string currentTargetPath = shortcut.TargetPath;
                if (currentTargetPath != expectedTargetPath)
                {
                    return "Outdated";
                }

                return "Up-to-date";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking desktop shortcut status.");
                return "Error";
            }
        }
    }
}