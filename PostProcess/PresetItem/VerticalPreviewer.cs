using Bundlingway.Model;

namespace Bundlingway.PostProcess.PresetItem
{
    public class VerticalPreviewer : ITemplatePostProcess
    {
        public int Order { get; set; } = 18;

        private List<string> textureTechniques = ["Layer", "Layer2", "Layer3", "Layer4", "Layer5", "StageDepth", "StageDepth2", "StageDepth3", "StageDepth4", "StageDepth5", "MultiStageDepth"];
        private List<string> textureTechniques2 = [
            "Layer@Layer.fx",
            "Layer2@Layer.fx",
            "Layer3@Layer.fx",
            "Layer4@Layer.fx",
            "Layer5@Layer.fx",
            "StageDepth@StageDepth.fx",
            "StageDepth2@StageDepth.fx",
            "StageDepth3@StageDepth.fx",
            "StageDepth4@StageDepth.fx",
            "StageDepth5@StageDepth.fx",
            "MultiStageDepth@MultiStageDepth.fx"];

        public List<string> PresetExclusionList { get; set; } = ["- Note -*", "WiFi*"];

        public ParameterList Techniques { get; set; } = new ParameterList()
        {
            EndWith = ["Vertical_Previewer@VerticalPreviewer.fx"]
        };
        public Dictionary<string, string> RootElements { get; set; } = new Dictionary<string, string> {
            { "KeyVertical_Previewer@VerticalPreviewer.fx", "%KeyVertical_Previewer@VerticalPreviewer.fx%" }
        };

        public List<Section> Sections { get; set; } = [new()
        {
            Name = "VerticalPreviewer.fx",
            Parameters = {
                { "cLayerVPre_Composition", "0" },
                { "cLayerVPre_Angle", "2" },
            }
        }];

        public bool PostProcess(Preset preset)
        {

            preset.Techniques["Vertical_Previewer@VerticalPreviewer.fx"] = false;

            preset.IniHandler.Global.RemoveKey("KeyLayer@Layer.fx");

            // First remove all previous keys.
            foreach (var tt in textureTechniques2)
                preset.IniHandler.Global.RemoveKey("Key" + tt);


            var elligibleTechniques = preset.Techniques.Where(i => textureTechniques.Any(t => i.Value && i.Key.StartsWith(t + "@"))).OrderBy(i => i.Key).ToList();

            foreach (var kvp in elligibleTechniques)
                RootElements["Key" + kvp.Key] = "%KeyStageDepth@StageDepth.fx%";



            var probe = textureTechniques.Any(i => preset.Techniques.Any(test => test.Value && test.Key.StartsWith(i + "@")));

            if (probe)
            {
                preset.Techniques["Vertical_Previewer@VerticalPreviewer.fx"] = false;
                return true;
            }

            return true;
        }
    }
}