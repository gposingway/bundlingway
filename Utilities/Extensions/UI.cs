namespace Bundlingway.Utilities.Extensions
{
    public static class UI
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
