using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;

namespace Bundlingway.PostProcess.PresetItem
{
    public class FixMultipleMXAO : IPresetProcess
    {
        public int Order { get; set; } = 0;

        public bool PostProcess(Preset preset)
        {
            var update = false;

            if (preset.Techniques.ContainsKey("MXAO@qUINT_mxao.fx"))
            {

                preset.Techniques = preset.Techniques.ReplaceKey("MXAO@qUINT_mxao.fx", "qMXAO@qUINT_mxao.fx");
                preset.Techniques["qMXAO@qUINT_mxao.fx"] = true;

                Log.Information("[Conflicting Techniques] - MXAO@qUINT_mxao.fx, " + Path.GetFileName(preset.Filename));
                update = true;
            }

            try
            {
                if (preset.Techniques.ContainsKey("qMXAO@qUINT_mxao.fx"))
                {
                    var probe2 = preset.IniHandler["qUINT_mxao.fx"];

                    foreach (var probe3 in probe2)
                    {
                        if (probe3.KeyName.StartsWith("MXAO_"))
                        {
                            probe3.KeyName = "q" + probe3.KeyName;
                            update = true;
                        }

                        if (probe3.KeyName == "PreprocessorDefinitions")
                        {
                            probe3.Value = probe3.Value.Replace("MXAO_", "qMXAO_");

                            do
                            {
                                probe3.Value = probe3.Value.Replace("qqMXAO_", "qMXAO_");

                            } while (probe3.Value.IndexOf("qqMXAO_") != -1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception in FixMultipleMXAO");
                throw;
            }
            return update;
        }
    }
}