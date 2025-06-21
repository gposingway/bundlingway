using IniParser.Model;

namespace Bundlingway.Model
{
    public class Preset
    {
        public Dictionary<string, bool> Techniques { get; set; } = new();
        public Dictionary<string, bool> OriginalTechniques { get; set; } = new();

        public List<string> TextureFiles { get; set; } = new();

        public required string Filename { get; set; }
        public required IniParser.Model.IniData IniHandler { get; set; }
    }
}
