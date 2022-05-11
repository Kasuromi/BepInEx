using BepInEx.Configuration;
using BepInEx.IL2CPP.Hook.Dobby;
using BepInEx.IL2CPP.Hook.Funchook;
using System;
using System.Runtime.InteropServices;

namespace BepInEx.IL2CPP.Hook;

public class NativeDetourHelper
{
    internal enum DetourProvider
    {
        Default,
        Dobby,
        Funchook
    }
    private static readonly ConfigEntry<DetourProvider> DetourProviderType = ConfigFile.CoreConfig.Bind(
        "Detours", "DetourProviderType",
        DetourProvider.Default,
        "The native provider to use for managed detours"
    );

    public static bool DEBUG_DETOURS = false;

    private static INativeDetour CreateDefault(nint original, nint target)
    {
        // TODO: check and provide an OS accurate provider
        return new DobbyDetour(original, target);
    }

    public static INativeDetour Create(nint original, nint target)
    {
        return DetourProviderType.Value switch
        {
            DetourProvider.Dobby => new DobbyDetour(original, target),
            DetourProvider.Funchook => new FunchookDetour(original, target),
            _ => CreateDefault(original, target)
        };
    }

    public static INativeDetour CreateAndApply<T>(nint from, T to, out T original, CallingConvention? callingConvention = null)
        where T : Delegate
    {
        // TODO: callingConvention ignored currently
        var toPtr = Marshal.GetFunctionPointerForDelegate(to);
        return CreateAndApply(from, toPtr, out original);
    }

    public static INativeDetour CreateAndApply<T>(nint from, nint to, out T original)
        where T : Delegate
    {
        var detour = Create(from, to);
        detour.Apply();
        // TODO: GenerateTrampoline throws exceptions about a call being made to an UnmanagedCallersOnly
        original = Marshal.GetDelegateForFunctionPointer<T>(detour.TrampolinePtr);
        return detour;
    }
}
