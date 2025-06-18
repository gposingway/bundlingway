using Bundlingway.Model;

namespace Bundlingway.PostProcess.PresetItem
{
    public class KeepUI : ITemplatePostProcess
    {
        public int Order { get; set; } = 20;
        public ParameterList Techniques { get; set; } = new ParameterList() { StartWith = ["FFKeepUI@KeepUI.fx"], EndWith = ["FFRestoreUI@KeepUI.fx"] };
        public Dictionary<string, string> RootElements { get; set; } = [];
        public List<Section> Sections { get; set; } = [];
        public List<string> PresetExclusionList { get; set; } = [

            "ipsusuGameplayLite - Vanilla.ini",
            "ipsusuGameplayLite - Warm.ini",
            "ipsusuGameplayLite - Melon.ini",
            "ipsusuGameplayLite - Pastel.ini",
            "ipsusuGameplayLite - Cool.ini",
            "ipsusuGameplayLite - Crystal.ini",
            "ipsusuGameplayLite - Amber.ini",

            "ipsusuQuestingLite - Vanilla.ini",
            "ipsusuQuestingLite - Warm.ini",
            "ipsusuQuestingLite - Melon.ini",
            "ipsusuQuestingLite - Pastel.ini",
            "ipsusuQuestingLite - Cool.ini",
            "ipsusuQuestingLite - Crystal.ini",
            "ipsusuQuestingLite - Amber.ini",
            ];
        public bool PostProcess(Preset preset)
        {

            var techniqueList = preset.Techniques.ToList();


            var probe = "MartysMods_Launchpad@MartysMods_LAUNCHPAD.fx";

            if (preset.Techniques.ContainsKey(probe))
            {
                // Need to be placed first.

                var val = preset.Techniques[probe];

                preset.Techniques.Remove(probe);

                techniqueList.RemoveAll(i => i.Key == probe);
                techniqueList.Insert(0, new KeyValuePair<string, bool>(probe, val));

                preset.Techniques = techniqueList.ToDictionary(i => i.Key, i => i.Value);

                return true;
            }

            return false;
        }
    }
}
