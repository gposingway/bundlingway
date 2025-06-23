using Bundlingway.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bundlingway.Core.Services
{
    /// <summary>
    /// File system service implementation using standard .NET file operations.
    /// </summary>
    public class FileSystemService : IFileSystemService
    {
        public bool FileExists(string path) => File.Exists(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public void DeleteFile(string path) => File.Delete(path);

        public void DeleteDirectory(string path, bool recursive = false) => Directory.Delete(path, recursive);

        public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false) => File.Copy(sourcePath, destinationPath, overwrite);

        public void MoveFile(string sourcePath, string destinationPath) => File.Move(sourcePath, destinationPath);

        public async Task<string> ReadAllTextAsync(string path)
        {
            return File.ReadAllText(path); // Ensure the file exists before opening it
        }

        public async Task WriteAllTextAsync(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public IEnumerable<string> GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
            => Directory.GetFiles(path, searchPattern, searchOption);

        public IEnumerable<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
            => Directory.GetDirectories(path, searchPattern, searchOption);

        public long GetFileSize(string path)
        {
            // Use FileStream with read sharing to avoid locking issues
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return stream.Length;
        }

        public DateTime GetCreationTime(string path)
        {
            try
            {
                // Use FileInfo to get creation time - safer than File.GetCreationTime in some scenarios
                return new FileInfo(path).CreationTime;
            }
            catch
            {
                // Fallback to File.GetCreationTime if FileInfo fails
                return File.GetCreationTime(path);
            }
        }

        public DateTime GetLastWriteTime(string path)
        {
            try
            {
                // Use FileInfo to get last write time - safer than File.GetLastWriteTime in some scenarios
                return new FileInfo(path).LastWriteTime;
            }
            catch
            {
                // Fallback to File.GetLastWriteTime if FileInfo fails
                return File.GetLastWriteTime(path);
            }
        }

        public Stream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        public Stream OpenWrite(string path) => new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);

        public string GetSinglePresetsFolder()
        {
            // Use the same logic as Instances.SinglePresetsFolder
            return Path.Combine(GetPackageFolder(), Bundlingway.Constants.Folders.SinglePresets);
        }

        public string GetPackageFolder()
        {
            // Use the same logic as Instances.PackageFolder
            // This should be configurable or derived from configuration
            // For now, fallback to AppData/Bundlingway/Packages
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Bundlingway";
            var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
            var dataFolder = Path.Combine(appData, appName);
            return Path.Combine(dataFolder, Bundlingway.Constants.Folders.Packages);
        }
    }
}
