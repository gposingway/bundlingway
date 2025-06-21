using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bundlingway.Core.Interfaces
{
    /// <summary>
    /// Service for file system operations.
    /// Abstracts file operations to enable testing and different storage backends.
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="path">File path</param>
        bool FileExists(string path);

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="path">Directory path</param>
        bool DirectoryExists(string path);

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="path">Directory path</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">File path</param>
        void DeleteFile(string path);

        /// <summary>
        /// Deletes a directory.
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="recursive">Whether to delete recursively</param>
        void DeleteDirectory(string path, bool recursive = false);

        /// <summary>
        /// Copies a file.
        /// </summary>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="destinationPath">Destination file path</param>
        /// <param name="overwrite">Whether to overwrite existing file</param>
        void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);

        /// <summary>
        /// Moves a file.
        /// </summary>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="destinationPath">Destination file path</param>
        void MoveFile(string sourcePath, string destinationPath);

        /// <summary>
        /// Reads all text from a file.
        /// </summary>
        /// <param name="path">File path</param>
        Task<string> ReadAllTextAsync(string path);

        /// <summary>
        /// Writes all text to a file.
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="content">Content to write</param>
        Task WriteAllTextAsync(string path, string content);

        /// <summary>
        /// Gets files in a directory.
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search option</param>
        IEnumerable<string> GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets directories in a directory.
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="searchOption">Search option</param>
        IEnumerable<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets the size of a file.
        /// </summary>
        /// <param name="path">File path</param>
        long GetFileSize(string path);

        /// <summary>
        /// Gets the creation time of a file or directory.
        /// </summary>
        /// <param name="path">Path</param>
        DateTime GetCreationTime(string path);

        /// <summary>
        /// Gets the last write time of a file or directory.
        /// </summary>
        /// <param name="path">Path</param>
        DateTime GetLastWriteTime(string path);

        /// <summary>
        /// Opens a file stream for reading.
        /// </summary>
        /// <param name="path">File path</param>
        Stream OpenRead(string path);

        /// <summary>
        /// Opens a file stream for writing.
        /// </summary>
        /// <param name="path">File path</param>
        Stream OpenWrite(string path);

        /// <summary>
        /// Gets the path to the SinglePresetsFolder.
        /// </summary>
        string GetSinglePresetsFolder();

        /// <summary>
        /// Gets the path to the PackageFolder.
        /// </summary>
        string GetPackageFolder();
    }
}
