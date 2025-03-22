﻿using Microsoft.Win32;
using Serilog;

namespace Bundlingway.Utilities
{
    public static class CustomProtocolHandler
    {
        /// <summary>
        /// Registers a custom protocol for browser integration.
        /// </summary>
        /// <param name="protocolName">The name of the protocol to register.</param>
        /// <param name="description">The description of the protocol.</param>
        /// <param name="forceRegister">Whether to force the registration even if it's already registered.</param>
        public static async Task RegisterCustomProtocolAsync(string protocolName, string description = "", bool forceRegister = false)
        {
            var isExtensionRegistered = true;
            var isMainProtocolRegistered = true;

            var appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

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

            // If the protocol is not registered, create the registry keys
            if (!isMainProtocolRegistered)
            {
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

            // If the extension is not registered, create the registry keys
            if (!isExtensionRegistered)
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(extensionName))
                {
                    key.SetValue("", protocolName);
                }

                Log.Information($"Registered {extensionName} extension handler.");
            }
        }

        /// <summary>
        /// Checks if a custom protocol is registered.
        /// </summary>
        /// <param name="protocolName">The name of the protocol to check.</param>
        /// <returns>The status of the protocol registration.</returns>
        public static string IsCustomProtocolRegistered(string protocolName)
        {
            var appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string status = "Not Set";

            using (RegistryKey existingKey = Registry.ClassesRoot.OpenSubKey(protocolName))
            {
                if (existingKey != null)
                {
                    string existingAppPath = existingKey.OpenSubKey("shell")?.OpenSubKey("open")?.OpenSubKey("command")?.GetValue("")?.ToString();
                    if (existingAppPath != null)
                    {
                        if (existingAppPath.Contains(appPath))
                        {
                            status = "Set";
                        }
                        else
                        {
                            status = "Outdated";
                        }
                    }
                }
            }
            return status;
        }
    }
}
