using System.Text;

namespace Bundlingway.Utilities.Extensions
{
    public static class Criptography
    {
        public static string Sha512(this string input)
        {
            using var sha = System.Security.Cryptography.SHA512.Create();
            byte[] textData = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }
}
