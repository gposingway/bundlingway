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

            var process = System.Diagnostics.Process.GetCurrentProcess();
            var mainModule = process.MainModule;
            if (mainModule == null || string.IsNullOrEmpty(mainModule.FileName))
            {
                Log.Error("MainModule or FileName is null.");
                return;
            }
            var appPath = mainModule.FileName;            // Check if the protocol is already registered
            using (RegistryKey? existingKey = Registry.ClassesRoot.OpenSubKey(protocolName))
            {
                if (existingKey != null && !forceRegister)
                {
                    // Check the actual command registration, not just the main key
                    string? existingAppPath = existingKey.OpenSubKey("shell")?.OpenSubKey("open")?.OpenSubKey("command")?.GetValue("")?.ToString();
                    if (!string.IsNullOrEmpty(existingAppPath) && existingAppPath.Contains(appPath))
                    {
                        Log.Information($"{protocolName} protocol handler is already registered.");
                        isMainProtocolRegistered = true;
                    }
                    else
                    {
                        isMainProtocolRegistered = false;
                    }
                }
                else
                {
                    isMainProtocolRegistered = false;
                }
            }            // If the protocol is not registered, create the registry keys
            if (!isMainProtocolRegistered)
            {
                using (RegistryKey? key = Registry.ClassesRoot.CreateSubKey(protocolName))
                {
                    if (key == null) { Log.Error($"Failed to create registry key for {protocolName}."); return; }
                    key.SetValue("", $"URL: {protocolName} Protocol");
                    key.SetValue("URL Protocol", "");
                    key.SetValue("Description", description); // Set the optional description
                    key.CreateSubKey("DefaultIcon")?.SetValue("", $"{appPath},1");
                    key.CreateSubKey("shell")?.CreateSubKey("open")?.CreateSubKey("command")?.SetValue("", $"{appPath} \"%1\"");
                }

                Log.Information($"Registered {protocolName} protocol handler.");
                
                // Verify the registration was successful
                if (!VerifyProtocolRegistration(protocolName))
                {
                    Log.Error($"Protocol registration verification failed for {protocolName}");
                }
            }

            var extensionName = "." + protocolName;

            // Check if the extension is already registered
            using (RegistryKey? existingKey = Registry.ClassesRoot.OpenSubKey(extensionName))
            {
                isExtensionRegistered = (existingKey != null && !forceRegister);

                if (isExtensionRegistered)
                {
                    string? existingAppPath = existingKey?.GetValue("")?.ToString();
                    if (!string.IsNullOrEmpty(existingAppPath) && existingAppPath.Equals(protocolName))
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
        }        /// <summary>
        /// Checks if a custom protocol is registered.
        /// </summary>
        /// <param name="protocolName">The name of the protocol to check.</param>
        /// <returns>The status of the protocol registration.</returns>
        public static string IsCustomProtocolRegistered(string protocolName)
        {
            var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
            if (mainModule == null || string.IsNullOrEmpty(mainModule.FileName))
            {
                Log.Error("MainModule or FileName is null.");
                return "Not Set";
            }
            var appPath = mainModule.FileName;
            string status = "Not Set";

            try
            {
                using (RegistryKey? existingKey = Registry.ClassesRoot.OpenSubKey(protocolName))
                {
                    if (existingKey != null)
                    {
                        string? existingAppPath = existingKey.OpenSubKey("shell")?.OpenSubKey("open")?.OpenSubKey("command")?.GetValue("")?.ToString();
                        Log.Debug($"Checking protocol '{protocolName}': Current app: {appPath}, Registry command: {existingAppPath}");
                        
                        if (!string.IsNullOrEmpty(existingAppPath))
                        {
                            if (existingAppPath.Contains(appPath))
                            {
                                status = "Set";
                            }
                            else
                            {
                                status = "Outdated";
                                Log.Information($"Protocol '{protocolName}' is outdated. Expected app: {appPath}, Found: {existingAppPath}");
                            }
                        }
                        else
                        {
                            Log.Warning($"Protocol '{protocolName}' exists but has no command value");
                        }
                    }
                    else
                    {
                        Log.Debug($"Protocol '{protocolName}' is not registered");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error checking protocol registration for '{protocolName}'");
            }
            
            return status;
        }

        /// <summary>
        /// Verifies that a protocol is properly registered by testing the actual registry entries.
        /// </summary>
        /// <param name="protocolName">The protocol name to verify.</param>
        /// <returns>True if the protocol is properly registered.</returns>
        public static bool VerifyProtocolRegistration(string protocolName)
        {
            try
            {
                var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                if (mainModule == null || string.IsNullOrEmpty(mainModule.FileName))
                {
                    Log.Error("Cannot verify protocol registration: MainModule or FileName is null.");
                    return false;
                }
                var appPath = mainModule.FileName;

                using (RegistryKey? protocolKey = Registry.ClassesRoot.OpenSubKey(protocolName))
                {
                    if (protocolKey == null)
                    {
                        Log.Warning($"Protocol key '{protocolName}' not found in registry");
                        return false;
                    }

                    // Check if URL Protocol marker exists
                    var urlProtocolValue = protocolKey.GetValue("URL Protocol");
                    if (urlProtocolValue == null)
                    {
                        Log.Warning($"URL Protocol marker not found for '{protocolName}'");
                        return false;
                    }

                    // Check command registration
                    using (RegistryKey? commandKey = protocolKey.OpenSubKey("shell\\open\\command"))
                    {
                        if (commandKey == null)
                        {
                            Log.Warning($"Command key not found for protocol '{protocolName}'");
                            return false;
                        }

                        string? commandValue = commandKey.GetValue("")?.ToString();
                        if (string.IsNullOrEmpty(commandValue))
                        {
                            Log.Warning($"Command value is empty for protocol '{protocolName}'");
                            return false;
                        }

                        if (!commandValue.Contains(appPath))
                        {
                            Log.Warning($"Command value doesn't contain current app path. Expected: {appPath}, Found: {commandValue}");
                            return false;
                        }

                        Log.Information($"Protocol '{protocolName}' is properly registered with command: {commandValue}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error verifying protocol registration for '{protocolName}'");
                return false;
            }
        }
    }
}
