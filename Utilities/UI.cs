using static Bundlingway.Constants;

namespace Bundlingway.Utilities
{
    public static class UI
    {
        private static System.Threading.Timer _idleTimer;
        private static string _currentMessage;
        private static frmLanding _landingForm;

        static UI()
        {
            // Don't start the timer in static constructor - causes deadlock
            // Timer will be initialized when landing form is set
        }

        public static frmLanding _landing 
        { 
            get => _landingForm;
            set 
            { 
                _landingForm = value;
                // Initialize timer only after landing form is set
                if (_landingForm != null && _idleTimer == null)
                {
                    InitializeIdleTimer();
                }
            }
        }

        private static void InitializeIdleTimer()
        {
            _idleTimer = new System.Threading.Timer(async _ =>
            {
                if (_landingForm != null)
                {
                    await Announce(MessageCategory.IdleCommentary);
                    _idleTimer?.Change(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public static async Task Announce(string message)
        {
            if (_landingForm == null) return;
            
            if (_currentMessage != message)
            {
                _idleTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _currentMessage = message;
                await _landingForm.Announce(message);
                _idleTimer?.Change(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
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

        public static async Task UpdateElements() 
        {
            if (_landingForm == null) return;
            await _landingForm.UpdateElements();
        }

        public static async Task StartProgress(long count) 
        {
            if (_landingForm == null) return;
            await _landingForm.StartProgress(count);
        }

        public static async Task SetProgress(long value) 
        {
            if (_landingForm == null) return;
            await _landingForm.SetProgress(value);
        }

        public static async Task StopProgress() 
        {
            if (_landingForm == null) return;
            await _landingForm.StopProgress();
        }

        internal static void DisableEverything() 
        {
            if (_landingForm == null) return;
            _landingForm.DisableEverything();
        }

        internal static void EnableEverything() 
        {
            if (_landingForm == null) return;
            _landingForm.EnableEverything();
        }

        internal static async Task BringToFront()
        {
            if (_landingForm == null) return;
            await _landingForm.BringToFrontForm();
        }
    }
}