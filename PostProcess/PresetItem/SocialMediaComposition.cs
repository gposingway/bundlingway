using Bundlingway.Model;

namespace Bundlingway.PostProcess.PresetItem
{
    public class SocialMediaComposition : ITemplatePostProcess
    {
        public int Order { get; set; } = 0;
        public ParameterList Techniques { get; set; } = new ParameterList() { EndWith = ["AspectRatioComposition@AspectRatioComposition.fx"] };
        public Dictionary<string, string> RootElements { get; set; } = new Dictionary<string, string> {
            { "KeyAspectRatioComposition@AspectRatioComposition.fx", "107,1,0,0" }
        };
        public List<Section> Sections { get; set; } = [new()
        {
            Name = "AspectRatioComposition.fx",
            Parameters = {
                { "iUIAspectRatio", "4,3" } ,
                { "iUIGridFractions", "0" } ,
                { "iUIGridType", "1" }
            }
        }];
        public List<string> PresetExclusionList { get; set; } = [
            "- Note -  Disable Glamarye_Fast_Effects for even more FPS.ini",
            "- Note -  Enable Fast AO under Glamarye_Fast_Effects for better quality.ini",
            "- Note -  Disable 'DAMP RT' if shading looks weird.ini",
            "- Note - Enable 'DAMP RT' for Raytraced Global Illumination.ini",
            "- Note -  Check README for list of required shaders.ini"
            ];

        public bool PostProcess(Preset preset)
        {
            preset.Techniques["AspectRatioComposition@AspectRatioComposition.fx"] = false;
            return true;
        }
    }
}
