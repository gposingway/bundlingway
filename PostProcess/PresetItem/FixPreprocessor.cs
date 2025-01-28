using Bundlingway.Model;
using System.Data;

namespace Bundlingway.PostProcess.PresetItem
{
    public class FixPreprocessor : IPresetProcess
    {
        public int Order { get; set; } = 200;
        public ParameterList Techniques { get; set; } = new ParameterList();
        public Dictionary<string, string> RootElements { get; set; } = [];
        public List<Section> Sections { get; set; } = [];
        public List<string> PresetExclusionList { get; set; } = [];

        private Dictionary<string, List<string>> MappedDefinitions = new() {
            { "MultiStageDepth.fx", new List<string>() { "MultiStageDepthTexture_Source" }} ,
            { "MultiLUT.fx",new List<string>() { "MultiLUTTexture2", "MultiLUTTexture3", "MultiLUTTexture_Source", "MultiLUTTexture2_Source", "MultiLUTTexture3_Source" } },
            { "Copyright.fx", new List<string>() { "_Copyright_Texture_Source" }} ,
        };

        public bool PostProcess(Preset preset)
        {

            var update = false;

            // This Post-process moves some specific PreprocessorDefinitions entries to the global scope.

            try
            {
                var globalDefs = preset.IniHandler.Global["PreprocessorDefinitions"]?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries).Order()
                    .Where(i => i.Contains('='))
                    .OrderBy(i => i)
                    .Select(i => new KeyValuePair<string, string>(i.Split('=')[0], i.Split('=')[1]))
                    .GroupBy(i => i.Key)
                    .ToDictionary(g => g.Key, g => g.Last().Value) ?? [];

                foreach (var md in MappedDefinitions)
                {

                    if (!preset.IniHandler.Sections.ContainsSection(md.Key)) continue;

                    var probe = preset.IniHandler[md.Key]["PreprocessorDefinitions"];

                    if (probe != null)
                    {
                        var localDefs = probe
                            .Split(',', StringSplitOptions.RemoveEmptyEntries).Order()
                            .Where(i => i.Contains('='))
                            .OrderBy(i => i)
                            .Select(i => new KeyValuePair<string, string>(i.Split('=')[0], i.Split('=')[1]))
                            .GroupBy(i => i.Key)
                            .ToDictionary(g => g.Key, g => g.Last().Value) ?? [];

                        foreach (var k in md.Value)
                        {
                            if (localDefs.ContainsKey(k))
                            {
                                globalDefs[k] = localDefs[k];
                                update = true;
                            }
                        }
                    }
                }

                if (update)
                {

                    var newPPDstring = string.Join(",", globalDefs.Select(i => i.Key + "=" + i.Value).ToList());

                    Console.WriteLine("[Transposed PreprocessorDefinitions]" + Path.GetFileName(preset.Filename));

                    preset.IniHandler.Global["PreprocessorDefinitions"] = newPPDstring;
                    return true;
                }

            }
            catch (Exception e)
            {

                throw;
            }

            return false;


        }
    }
}