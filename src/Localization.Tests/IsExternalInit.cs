#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    // Polyfill for the init accessors on .NET Framework (the type exists from .NET 5)
    internal static class IsExternalInit
    {
    }
}
#endif
