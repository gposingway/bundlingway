using Bundlingway.Model;
using Bundlingway.Utilities;

namespace Bundlingway.PostProcess.RawFile
{
    public class FixGaussianFXPasses : IRawFileProcess
    {
        public bool ApplyToPresets { get; set; } = true;
        public bool ApplyToShaders { get; set; } = true;
        public int Order { get; set; } = 0;


        public bool PostProcess(Preset preset)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetReplacementMap(ResourcePackage package, List<string> presetFileList, string baselinePath, Bundlingway.Core.Utilities.Logging _logger)
        {
            return new Dictionary<string, string> {
            { "gN_PASSES=6", "gN_PASSES=5" } ,
            { "gN_PASSES=7", "gN_PASSES=5" } ,
            { "gN_PASSES=8", "gN_PASSES=5" } ,
            { "gN_PASSES=9", "gN_PASSES=5" }};
        }
    }
}