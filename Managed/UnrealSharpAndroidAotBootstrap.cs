#if ANDROID && NATIVE_AOT
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnrealSharp.Binds;
using UnrealSharp.Core;
using UnrealSharp.Plugins;

namespace UnrealSharp.NativeAotBootstrap;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct FCSInitializationResult
{
    public const int MessageCapacity = 4096;
    public NativeBool Success;
    public fixed byte Message[MessageCapacity];
}

internal static class UnrealSharpAndroidAotBootstrap
{
    [UnmanagedCallersOnly(EntryPoint = "InitializeUnrealSharp")]
    private static unsafe void InitializeUnrealSharp(
        byte* workingDirectoryUtf8,
        PluginsCallbacks* pluginCallbacks,
        nint bindsCallbacks,
        nint managedCallbacks,
        FCSInitializationResult* result)
    {
        try
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY",
                Marshal.PtrToStringUTF8((nint)workingDirectoryUtf8)!);

            PluginsCallbacks.Initialize(pluginCallbacks);
            ManagedCallbacks.Initialize(managedCallbacks);
            NativeBinds.Initialize(bindsCallbacks);
            result->Success = NativeBool.True;
        }
        catch (Exception exception)
        {
            result->Success = NativeBool.False;
            WriteMessage((nint)result->Message, exception.ToString());
        }
    }

    private static void WriteMessage(nint buffer, string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        int length = Math.Min(bytes.Length, FCSInitializationResult.MessageCapacity - 1);
        Marshal.Copy(bytes, 0, buffer, length);
        Marshal.WriteByte(buffer, length, 0);
    }
}
#endif