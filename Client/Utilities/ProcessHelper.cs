using System.Diagnostics;

namespace Bundlingway.Utilities
{
    public class ProcessHelper
    {
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
    }
}