using System.Text.Json;

namespace GPosingway.Model
{
    public class JsonObject<T>
    {
        private readonly string fileName;

        public JsonObject()
        {
            // Use the full namespace and class name as the file name + ".json"
            fileName = $"{typeof(T).FullName}.json";
        }

        public void Save(T item)
        {
            var json = JsonSerializer.Serialize(item);
            File.WriteAllText(fileName, json);
        }

        public T Load()
        {
            if (File.Exists(fileName))
            {
                var json = File.ReadAllText(fileName);
                return JsonSerializer.Deserialize<T>(json);
            }
            return default; // Return a default value if the file doesn't exist
        }
    }
}
