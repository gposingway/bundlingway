using static Bundlingway.Constants;

namespace Bundlingway.Utilities
{
    public static class UI
    {
        private static System.Threading.Timer _idleTimer;
        private static string _currentMessage;

        static UI()
        {
            _idleTimer = new System.Threading.Timer(async a =>
            {
                await Announce(MessageCategory.IdleCommentary);
                _idleTimer?.Change(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));

            });
        }

        public static frmLanding _landing { get; set; }

        public static async Task Announce(string message)
        {
            if (_currentMessage != message)
            {
                _idleTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _currentMessage = message;
                await _landing.Announce(message);
                _idleTimer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            }
        }

        public static async Task Announce(MessageCategory category, params string[] args) => await Announce(Constants.Bundlingway.GetMessage(category, args));

        public static async Task NotifyAsync(string topic, string message)
        {
            //new ToastContentBuilder()
            // .AddArgument("action", "viewConversation")
            // .AddArgument("conversationId", 9813)
            // .AddText(topic)
            // .AddText(message)
            // .Show();
            //;
        }

        public static async Task UpdateElements() => await _landing.UpdateElements();
        public static async Task StartProgress(long count) => await _landing.StartProgress(count);
        public static async Task SetProgress(long value) => await _landing.SetProgress(value);
        public static async Task StopProgress() => await _landing.StopProgress();
        internal static void DisableEverything() => _landing.DisableEverything();
        internal static void EnableEverything() => _landing.EnableEverything();
    }
}