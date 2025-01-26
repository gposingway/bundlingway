namespace Bundlingway.Model
{
    public class GposingwayDefinitions
    {
        public string version { get; set; }
        public string gposingwayUrl { get; set; }
        public List<string> Deprecated { get; set; }
        public List<OptionalItem> Optional { get; set; }

        public class OptionalItem
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string Mappings { get; set; }
        }
    }

}
