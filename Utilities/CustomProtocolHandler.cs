using Microsoft.Win32;
using Serilog;

namespace Bundlingway.Utilities
{
    public static class CustomProtocolHandler
    {
        public static async Task RegisterCustomProtocolAsync(string protocolName, string description = "", bool forceRegister = false)
        {
            var isExtensionRegistered = true;
            var isMainProtocolRegistered = true;
            var ass = System.Reflection.Assembly.GetExecutingAssembly();
            string appPath = ass.Location.Replace(".dll", ".exe");

            // Check if the protocol is already registered
            using (RegistryKey existingKey = Registry.ClassesRoot.OpenSubKey(protocolName))
            {
                isMainProtocolRegistered = (existingKey != null && !forceRegister);

                if (isMainProtocolRegistered)
                {
                    string existingAppPath = existingKey.OpenSubKey("shell")?.OpenSubKey("open")?.OpenSubKey("command")?.GetValue("")?.ToString();
                    if (existingAppPath != null && existingAppPath.Contains(appPath))
                    {
                        Log.Information($"{protocolName} protocol handler is already registered.");
                    }
                    else
                    {
                        isMainProtocolRegistered = false;
                    }
                }
            }

            if (!isMainProtocolRegistered)
            {
                // Create registry keys
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(protocolName))
                {
                    key.SetValue("", $"URL: {protocolName} Protocol");
                    key.SetValue("URL Protocol", "");
                    key.SetValue("Description", description); // Set the optional description
                    key.CreateSubKey("DefaultIcon").SetValue("", $"{appPath},1");
                    key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", $"{appPath} \"%1\"");
                }

                Log.Information($"Registered {protocolName} protocol handler.");
            }

            var extensionName = "." + protocolName;

            // Check if the extension is already registered
            using (RegistryKey existingKey = Registry.ClassesRoot.OpenSubKey(extensionName))
            {
                isExtensionRegistered = (existingKey != null && !forceRegister);

                if (isExtensionRegistered)
                {
                    string existingAppPath = existingKey.GetValue("")?.ToString();
                    if (existingAppPath != null && existingAppPath.Equals(protocolName))
                    {
                        Log.Information($".{protocolName} extension is already registered.");
                    }
                    else
                    {
                        isExtensionRegistered = false;
                    }
                }
            }

            if (!isExtensionRegistered)
            {
                // Create registry keys
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(extensionName))
                {
                    key.SetValue("", protocolName);
                }

                Log.Information($"Registered {extensionName} extension handler.");
            }
        }
    }
}