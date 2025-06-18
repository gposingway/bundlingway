using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    public interface ISystemService
    {
        Task OpenInExplorerAsync(string path);
        Task OpenInNotepadAsync(string filePath);
    }
}
