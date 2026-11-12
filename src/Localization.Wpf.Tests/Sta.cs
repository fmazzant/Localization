using System.Runtime.ExceptionServices;

namespace Mafesoft.Localization.Wpf.Tests
{
    /// <summary>
    /// Runs a test body on an STA thread, as the WPF controls require (xUnit runs the tests on MTA threads).
    /// </summary>
    internal static class Sta
    {
        public static void Run(Action action)
        {
            ExceptionDispatchInfo error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    error = ExceptionDispatchInfo.Capture(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            error?.Throw();
        }
    }
}
