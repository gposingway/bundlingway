using Bundlingway.PostProcess.PresetItem;

public interface IRawFileProcess : IPostProcess
{
    bool ApplyToPresets { get; set; }
    bool ApplyToShaders { get; set; }

    Dictionary<string, string> ReplacementMap(List<string> presetFileList, string baselinePath);
}
