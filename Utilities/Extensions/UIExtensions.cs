namespace Bundlingway.Utilities.Extensions
{
    public static class UIExtensions
    {
        public static void DoAction(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

    }
}
