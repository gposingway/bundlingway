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
                try
                {
                    var deserialized = JsonSerializer.Deserialize<List<T>>(json);
                    if (deserialized != null)
                    {
                        Clear();
                        AddRange(deserialized);
                    }
                }
                catch (Exception ex)
                {
                    // Optionally log or handle the error, but do not clear existing data
                }
            }
        }
    }
}
