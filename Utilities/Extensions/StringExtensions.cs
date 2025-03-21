namespace Bundlingway.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string ToFileSystemSafeName(this string value)
        {
            return string.Join("_", value.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
