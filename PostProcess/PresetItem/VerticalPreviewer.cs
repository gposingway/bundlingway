using Bundlingway.Model;

namespace Bundlingway.PostProcess.PresetItem
{
    public class VerticalPreviewer : ITemplatePostProcess
    {
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

        private List<string> ExcludePresets = [
            "GPosingwayIntro.ini",
            "Wifi_TicketAoi.ini",
            "Wifi_TicketAoi2 - NoFrame.ini",
            "Wifi_TicketKiiro.ini",
            "Wifi_TicketKiiro2 - NoFrame.ini",
            "Wifi_TicketMidori.ini",
            "Wifi_TicketMidori2 - NoFrame.ini",
            "Wifi_TicketPinku.ini",
            "Wifi_TicketPinku2 - NoFrame.ini",
            "Wifi_Memory1.ini",
            "Wifi_Memory2.ini",
            "Wifi_Memory3.ini",

            "- Note -  Disable Glamarye_Fast_Effects for even more FPS.ini",
            "- Note -  Enable Fast AO under Glamarye_Fast_Effects for better quality.ini",
            "- Note -  Disable 'DAMP RT' if shading looks weird.ini",
            "- Note - Enable 'DAMP RT' for Raytraced Global Illumination.ini",
            "- Note -  Check README for list of required shaders.ini",
            ];

        public int Order { get; set; } = 10;
        public ParameterList Techniques { get; set; } = new ParameterList()
        {
            EndWith = ["Vertical_Previewer@VerticalPreviewer.fx"]
        };
        public Dictionary<string, string> RootElements { get; set; } = new Dictionary<string, string> {
            { "KeyVertical_Previewer@VerticalPreviewer.fx", "107,0,0,0" }
        };


        public List<Section> Sections { get; set; } = [new()
        {
            Name = "VerticalPreviewer.fx",
            Parameters = {
                { "cLayerVPre_Composition", "0" },
                { "cLayerVPre_Angle", "2" },
            }
        }];
        public List<string> PresetExclusionList { get; set; } = [];

        public bool PostProcess(Preset preset)
        {
            // Only enable the VerticalPreviewer if layered textures are present.


            preset.Techniques["Vertical_Previewer@VerticalPreviewer.fx"] = false;
            if (ExcludePresets.Any(preset.Filename.EndsWith)) return false;


            preset.IniHandler.Global.RemoveKey("KeyLayer@Layer.fx");


            // First remove all previous keys.
            foreach (var tt in textureTechniques2)
                preset.IniHandler.Global.RemoveKey("Key" + tt);


            var elligibleTechniques = preset.Techniques.Where(i => textureTechniques.Any(t => i.Value && i.Key.StartsWith(t + "@"))).OrderBy(i => i.Key).ToList();

            foreach (var kvp in elligibleTechniques)
                RootElements["Key" + kvp.Key] = "109,0,0,0";



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