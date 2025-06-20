using static Bundlingway.Constants;

namespace Bundlingway.Utilities
{
    public static class UI
    {
        static UI()
        {
            // Don't start the timer in static constructor - causes deadlock
            // Timer will be initialized when landing form is set
        }

        private static void InitializeIdleTimer()
        {
            // No-op: Idle timer logic removed for decoupling
        }

        public static async Task Announce(string message)
        {
            // No-op: UI decoupled
            await Task.CompletedTask;
        }

        public static async Task Announce(MessageCategory category, params string[] args) => await Announce(Constants.Bundlingway.GetMessage(category, args));

        public static async Task NotifyAsync(string topic, string message)
        {
            // No-op: UI decoupled
            await Task.CompletedTask;
        }

        public static async Task UpdateElements() 
        {
            // No-op: UI decoupled
            await Task.CompletedTask;
        }

        public static async Task StartProgress(long count) 
        {
            // No-op: UI decoupled
            await Task.CompletedTask;
        }

        public static async Task SetProgress(long value) 
        {
            // No-op: UI decoupled
            await Task.CompletedTask;
        }

        public static async Task StopProgress() 
        {
            // No-op: UI decoupled
            await Task.CompletedTask;
        }

        internal static void DisableEverything() 
        {
            // No-op: UI decoupled
        }

        internal static void EnableEverything() 
        {
            // No-op: UI decoupled
        }

        internal static async Task BringToFront()
        {
            // No-op: UI decoupled
            await Task.CompletedTask;
        }
    }
}