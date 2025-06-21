using System.Security.Cryptography;
using Bundlingway.Core.Interfaces;
using Bundlingway.Core.Services;
using Bundlingway.Model;

namespace Bundlingway.Utilities.Extensions
{
    /// <summary>
    /// Utility class for file hashing and package comparison.
    /// </summary>
    public static class PackageExtensions
    {
        /// <summary>
        /// Replaces a key in the dictionary with a new key.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="originalDictionary">The original dictionary.</param>
        /// <param name="oldKey">The key to be replaced.</param>
        /// <param name="newKey">The new key to replace the old key.</param>
        /// <returns>The dictionary with the key replaced.</returns>
        public static Dictionary<TKey, TValue> ReplaceKey<TKey, TValue>(this Dictionary<TKey, TValue> originalDictionary, TKey oldKey, TKey newKey) where TKey : notnull
        {
            if (!originalDictionary.ContainsKey(oldKey))
                return originalDictionary; // Key not found, return the original dictionary unchanged

            TValue value = originalDictionary[oldKey];
            originalDictionary.Remove(oldKey);
            originalDictionary[newKey] = value;

            return originalDictionary;
        }

        /// <summary>
        /// Computes the MD5 hash of a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The MD5 hash of the file as a hexadecimal string.</returns>
        public static string ComputeMD5Hash(this string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);

            byte[] hashBytes = md5.ComputeHash(stream);
            string md5Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            return md5Hash;
        }

        /// <summary>
        /// Compares two packages and returns a dictionary indicating the comparison result for each shader file.
        /// </summary>
        /// <param name="PackageA">The name of the first package.</param>
        /// <param name="PackageB">The name of the second package.</param>
        /// <param name="envService">The environment service instance.</param>
        /// <returns>A dictionary where the key is the shader file name and the value is the comparison result.</returns>
        public static Dictionary<string, string> ComparePackages(string PackageA, string PackageB, IAppEnvironmentService envService)
        {
            var a = envService.Packages.Get(PackageA);
            var b = envService.Packages.Get(PackageB);

            var comparisonMap = new Dictionary<string, string>();

            var aShaderFiles = a?.ShaderFiles ?? new Dictionary<string, string>();
            var bShaderFiles = b?.ShaderFiles ?? new Dictionary<string, string>();

            foreach (var item in aShaderFiles)
            {
                var result = "Same";

                if (bShaderFiles.ContainsKey(item.Key))
                {
                    if (bShaderFiles[item.Key] != item.Value)
                    {
                        result = "Different version";
                    }
                }
                else
                {
                    result = PackageA + " only";
                }

                comparisonMap[item.Key] = result;
            }

            foreach (var item in bShaderFiles.Where(i => !aShaderFiles.ContainsKey(i.Key)))
            {
                comparisonMap[item.Key] = PackageB + " only";
            }

            comparisonMap = comparisonMap.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);

            return comparisonMap;
        }

        /// <summary>
        /// Saves the package catalog entry to its local folder.
        /// </summary>
        /// <param name="package">The package to save.</param>
        /// <param name="fileSystemService">The file system service to use for file operations.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SaveAsync(this ResourcePackage package, IFileSystemService fileSystemService)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (fileSystemService == null) throw new ArgumentNullException(nameof(fileSystemService));
            if (string.IsNullOrEmpty(package.LocalFolder)) throw new ArgumentException("Package LocalFolder cannot be null or empty.", nameof(package.LocalFolder));

            var catalogPath = Path.Combine(package.LocalFolder, Bundlingway.Constants.Files.CatalogEntry);
            var json = package.ToJson() ?? throw new InvalidOperationException("Failed to serialize package to JSON.");
            await fileSystemService.WriteAllTextAsync(catalogPath, json);
        }
    }

}
