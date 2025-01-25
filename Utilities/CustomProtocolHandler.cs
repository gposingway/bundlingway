using Microsoft.Win32;

namespace GPosingway.Utilities
{
    public static class CustomProtocolHandler
    {
        public static void RegisterCustomProtocol(string protocolName, string description = "", bool forceRegister = false)
        {
            var isExtensionRegistered = true;
            var isMainProtocolRegistered = true;

            // Check if the protocol is already registered
            using (RegistryKey existingKey = Registry.ClassesRoot.OpenSubKey(protocolName))
            {

                isMainProtocolRegistered = (existingKey != null && !forceRegister);

                if (isMainProtocolRegistered)
                    Console.WriteLine($"{protocolName} protocol handler is already registered.");
            }

            if (!isMainProtocolRegistered)
            {
                var ass = System.Reflection.Assembly.GetExecutingAssembly();


                // Get the current app instance path
                string appPath = ass.Location.Replace(".dll", ".exe");

                // Create registry keys
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(protocolName))
                {
                    key.SetValue("", $"URL: {protocolName} Protocol");
                    key.SetValue("URL Protocol", "");
                    key.SetValue("Description", description); // Set the optional description
                    key.CreateSubKey("DefaultIcon").SetValue("", $"{appPath},1");
                    key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", $"{appPath} \"%1\"");
                }

                Console.WriteLine($"Registered {protocolName} protocol handler.");
            }

            var extensionName = "." + protocolName;

            // Check if the extension is already registered
            using (RegistryKey existingKey = Registry.ClassesRoot.OpenSubKey(extensionName))
            {
                isExtensionRegistered = (existingKey != null && !forceRegister);

                if (isExtensionRegistered)
                    Console.WriteLine($".{protocolName} extension is already registered.");
            }

            if (!isExtensionRegistered)
            {

                // Create registry keys
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(extensionName))
                {
                    key.SetValue("", protocolName);
                }

                Console.WriteLine($"Registered {extensionName} extension handler.");
            }

        }
    }
}