using Bundlingway.Core;
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
        private readonly IConfigurationService _configService;

        public FileSystemService(IConfigurationService configService)
        {
            _configService = configService;
        }

        public bool FileExists(string path) => File.Exists(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public void DeleteFile(string path) => File.Delete(path);

        public void DeleteDirectory(string path, bool recursive = false) => Directory.Delete(path, recursive);

        public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false) => File.Copy(sourcePath, destinationPath, overwrite);

        public void MoveFile(string sourcePath, string destinationPath) => File.Move(sourcePath, destinationPath);

        public async Task<string> ReadAllTextAsync(string path) => await File.ReadAllTextAsync(path);

        public async Task WriteAllTextAsync(string path, string content) => await File.WriteAllTextAsync(path, content);

        public IEnumerable<string> GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
            => Directory.GetFiles(path, searchPattern, searchOption);

        public IEnumerable<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
            => Directory.GetDirectories(path, searchPattern, searchOption);

        public long GetFileSize(string path) => new FileInfo(path).Length;

        public DateTime GetCreationTime(string path) => File.GetCreationTime(path);

        public DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);

        public Stream OpenRead(string path) => File.OpenRead(path);

        public Stream OpenWrite(string path) => File.OpenWrite(path);

        public string GetSinglePresetsFolder()
        {
            // Use the same logic as Instances.SinglePresetsFolder
            return Path.Combine(GetPackageFolder(), Constants.Folders.SinglePresets);
        }

        public string GetPackageFolder()
        {
            // Use configuration to determine the package folder path
            var configPath = _configService?.Configuration?.Bundlingway?.Location;
            if (!string.IsNullOrEmpty(configPath))
            {
                return Path.Combine(configPath, Constants.Folders.Packages);
            }
            // Fallback to AppData/Bundlingway/Packages
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Bundlingway";
            var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
            var dataFolder = Path.Combine(appData, appName);
            return Path.Combine(dataFolder, Constants.Folders.Packages);
        }
    }
}
