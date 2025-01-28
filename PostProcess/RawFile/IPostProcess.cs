using Bundlingway.Model;
using Bundlingway.PostProcess.PresetItem;

public interface IRawFileProcess : IPostProcess
{
    bool ApplyToPresets { get; set; }
    bool ApplyToShaders { get; set; }

    Dictionary<string, string> GetReplacementMap(ResourcePackage package, List<string> presetFileList, string baselinePath, Bundlingway.Utilities.InstallLogger _logger);
}
