namespace Bundlingway.Model
{
    public class ResourcePackage
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string LocalBasePath { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool Default { get; set; }
        public bool Installed { get; set; }
    }
}
