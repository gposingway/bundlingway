namespace Bundlingway.Model
{
    public class Section
    {
        public required string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = [];
    }
}
