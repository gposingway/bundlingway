using System.Text;

namespace Bundlingway.Utilities.Extensions
{
    public static class CriptographyExtensions
    {
        public static string Sha512(this string input)
        {
            using var sha = System.Security.Cryptography.SHA512.Create();
            byte[] textData = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string Sha512FromFile(string filePath)
        {
            using var sha = System.Security.Cryptography.SHA512.Create();
            using var fileStream = File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(fileStream);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string MD5(this string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] textData = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string MD5FromFile(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var fileStream = File.OpenRead(filePath);
            byte[] hash = md5.ComputeHash(fileStream);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }
}
