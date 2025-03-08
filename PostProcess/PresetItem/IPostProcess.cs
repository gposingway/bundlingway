using Bundlingway.Model;

namespace Bundlingway.PostProcess.PresetItem
{
    public interface IPostProcess
    {
        int Order { get; set; }
        bool PostProcess(Preset preset);
    }

    public interface IPresetProcess: IPostProcess
    {
    }

    public interface ITemplatePostProcess : IPresetProcess
    {
        ParameterList Techniques { get; set; }
        Dictionary<string, string> RootElements { get; set; }
        List<Section> Sections { get; set; }
        List<string> PresetExclusionList { get; set; }
    }

    public interface IDisabledProcess : IPostProcess
    {
        ParameterList Techniques { get; set; }
        Dictionary<string, string> RootElements { get; set; }
        List<Section> Sections { get; set; }
        List<string> PresetExclusionList { get; set; }
        bool PostProcess(Preset preset);
    }
}
