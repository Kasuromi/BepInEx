using Il2CppInterop.Runtime.Injection;
using System;
using System.Runtime.InteropServices;

namespace BepInEx.IL2CPP.Hook;

public class UnhollowerDetourHandler : IManagedDetour
{
    public T Detour<T>(IntPtr from, T to) where T : Delegate
    {
        NativeDetourHelper.CreateAndApply(from, to, out var original);
        return original;
    }
}
