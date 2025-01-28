using IniParser.Model;

namespace Bundlingway.Model
{
    public class Preset
    {
        public Dictionary<string, bool> Techniques { get; set; } = [];
        public Dictionary<string, bool> OriginalTechniques { get; set; } = [];

        public List<string> TextureFiles { get; set; } = [];

        public string Filename { get; set; }
        public IniData IniHandler { get; set; }
    }
}
