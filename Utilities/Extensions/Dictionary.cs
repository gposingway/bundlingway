namespace Bundlingway.Utilities.Extensions
{
    public static class Dictionary
    {
        /// <summary>
        /// Retrieves the value associated with the specified key from the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary of key-value pairs.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>The value associated with the specified key, or the default value if the key is not found.</returns>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }
            return default;
        }

        /// <summary>
        /// Adds or updates the value associated with the specified key in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary of key-value pairs.</param>
        /// <param name="key">The key whose value to add or update.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        public static void Put<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary[key] = value;
        }
    }
}
