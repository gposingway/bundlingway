using System.Diagnostics;
using System.Threading.Tasks;
using Bundlingway.Core.Interfaces;

namespace Bundlingway.Core.Services
{
    public class SystemService : ISystemService
    {
        public Task OpenInExplorerAsync(string path)
        {
            Process.Start("explorer.exe", path);
            return Task.CompletedTask;
        }

        public Task OpenInNotepadAsync(string filePath)
        {
            Process.Start("notepad.exe", filePath);
            return Task.CompletedTask;
        }
    }
}
