using Bundlingway.Model;

namespace Bundlingway.PostProcess.PresetItem
{
    public class SocialMediaComposition : ITemplatePostProcess
    {
        public int Order { get; set; } = 16;
        public ParameterList Techniques { get; set; } = new ParameterList() { EndWith = ["AspectRatioComposition@AspectRatioComposition.fx"] };
        public Dictionary<string, string> RootElements { get; set; } = new Dictionary<string, string> {
            { "KeyAspectRatioComposition@AspectRatioComposition.fx", "%KeyAspectRatioComposition@AspectRatioComposition.fx%" }
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
        public List<string> PresetExclusionList { get; set; } = ["- Note -*","*WiFi*"];

        public bool PostProcess(Preset preset)
        {
            preset.Techniques["AspectRatioComposition@AspectRatioComposition.fx"] = false;
            return true;
        }
    }
}
