#if ANDROID && NATIVE_AOT
using Android.App;
using Android.OS;

namespace UnrealSharp.NativeAotBootstrap;

[Activity(Label = "UnrealSharp", MainLauncher = false)]
public sealed class UnrealSharpAndroidAotActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Finish();
    }
}
#endif
