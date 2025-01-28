using static Bundlingway.Constants;
using System.Threading;
using System;
using System.Xml;


namespace Bundlingway.Utilities
{
    public static class UI
    {
        private static System.Threading.Timer _idleTimer;

        static UI()
        {
            _idleTimer = new System.Threading.Timer(async a =>
            {

                await Announce(MessageCategory.IdleCommentary);
                _idleTimer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));

            });
        }

        public static Landing _landing { get; set; }

        public static async Task Announce(string message)
        {
            await _landing.Announce(message);
        }

        public static async Task Announce(MessageCategory category, params string[] args)
        {
            _idleTimer.Change(Timeout.Infinite, Timeout.Infinite);
            await _landing.Announce(Constants.Bundlingway.GetMessage(category, args));
            _idleTimer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

     
    }
}