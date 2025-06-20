using Bundlingway.Model;

namespace Bundlingway.PostProcess.PresetItem
{
    /// <summary>
    /// Represents a UI post-process template.
    /// </summary>
    public class UI : ITemplatePostProcess
    {
        /// <summary>
        /// Gets or sets the order of the post-process.
        /// </summary>
        public int Order { get; set; } = 10;

        /// <summary>
        /// Gets or sets the list of techniques.
        /// </summary>
        public ParameterList Techniques { get; set; } = new ParameterList
        {
            StartWith = ["FFKeepUI@KeepUI.fx"],
            EndWith = ["FFRestoreUI@KeepUI.fx"]
        };

        /// <summary>
        /// Gets or sets the root elements.
        /// </summary>
        public Dictionary<string, string> RootElements { get; set; } = [];

        /// <summary>
        /// Gets or sets the sections.
        /// </summary>
        public List<Section> Sections { get; set; } = [];

        /// <summary>
        /// Gets or sets the preset exclusion list.
        /// </summary>
        public List<string> PresetExclusionList { get; set; } =
        [
            "- Note -  Disable Glamarye_Fast_Effects for even more FPS.ini",
            "- Note -  Enable Fast AO under Glamarye_Fast_Effects for better quality.ini",
            "- Note -  Disable 'DAMP RT' if shading looks weird.ini",
            "- Note - Enable 'DAMP RT' for Raytraced Global Illumination.ini",
            "- Note -  Check README for list of required shaders.ini",
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
            "ipsusuQuestingLite - Amber.ini"
        ];

        /// <summary>
        /// Executes the post-process on the given preset.
        /// </summary>
        /// <param name="preset">The preset to process.</param>
        /// <param name="logger">The logger to use for logging.</param>
        /// <returns>Always returns false.</returns>
        public bool PostProcess(Preset preset) => false;
    }
}
