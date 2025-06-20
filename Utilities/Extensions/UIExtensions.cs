namespace Bundlingway.Utilities.Extensions
{
    public static class UIExtensions
    {
        /// <summary>
        /// Executes the specified action on the UI thread associated with the control.
        /// </summary>
        /// <param name="control">The control whose UI thread is to be used.</param>
        /// <param name="action">The action to be executed.</param>
        public static void DoAction(this Control control, Action action)
        {
            // Check if the action needs to be invoked on the UI thread
            if (control.InvokeRequired)
            {
                // Invoke the action on the UI thread
                control.Invoke(action);
            }
            else
            {
                // Execute the action directly
                action();
            }
        }
    }
}
