using Microsoft.Maui.Dispatching;

namespace Mafesoft.Localization.Maui.Tests
{
    /// <summary>
    /// The MAUI bindings need a dispatcher to propagate PropertyChanged (an application has one per UI thread).
    /// Without a platform head there is none: this one runs every callback inline on the calling thread.
    /// </summary>
    internal sealed class InlineDispatcher : IDispatcher, IDispatcherProvider
    {
        public static readonly InlineDispatcher Instance = new();

        /// <summary>Installs the dispatcher for every thread; call it before creating the controls.</summary>
        public static void Install() => DispatcherProvider.SetCurrent(Instance);

        public bool IsDispatchRequired => false;

        public bool Dispatch(Action action)
        {
            action();
            return true;
        }

        public bool DispatchDelayed(TimeSpan delay, Action action)
        {
            action();
            return true;
        }

        public IDispatcherTimer CreateTimer() => throw new NotSupportedException();

        public IDispatcher GetForCurrentThread() => this;
    }
}
