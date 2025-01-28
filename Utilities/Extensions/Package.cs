using System.Security.Cryptography;

namespace Bundlingway.Utilities.Extensions
{
    /// <summary>
    /// Utility class for file hashing and package comparison.
    /// </summary>
    public static class Package
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
        public static Dictionary<TKey, TValue> ReplaceKey<TKey, TValue>(this Dictionary<TKey, TValue> originalDictionary, TKey oldKey, TKey newKey)
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
        /// <returns>A dictionary where the key is the shader file name and the value is the comparison result.</returns>
        public static Dictionary<string, string> ComparePackages(string PackageA, string PackageB)
        {
            var a = Instances.Packages.Get(PackageA);
            var b = Instances.Packages.Get(PackageB);

            var comparisonMap = new Dictionary<string, string>();

            foreach (var item in a.ShaderFiles)
            {
                var result = "Same";

                if (b.ShaderFiles.ContainsKey(item.Key))
                {
                    if (b.ShaderFiles[item.Key] != item.Value)
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

            foreach (var item in b.ShaderFiles.Where(i => !a.ShaderFiles.ContainsKey(i.Key)))
            {
                comparisonMap[item.Key] = PackageB + " only";
            }

            comparisonMap = comparisonMap.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);

            return comparisonMap;
        }
    }

}
