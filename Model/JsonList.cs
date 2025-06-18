using System.Text.Json;

namespace Bundlingway.Model
{
    public class JsonList<T>: List<T>
    {
        private readonly string fileName;

        public JsonList()
        {
            // Use the class name (together with its namespace) as the file name + ".json"
            fileName = $"{typeof(T).FullName}.json";
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(fileName, json);
        }

        public void Load()
        {
            if (File.Exists(fileName))
            {
                var json = File.ReadAllText(fileName);
                Clear();
                var deserialized = JsonSerializer.Deserialize<List<T>>(json);
                if (deserialized != null)
                {
                    AddRange(deserialized);
                }
            }
        }
    }
}
