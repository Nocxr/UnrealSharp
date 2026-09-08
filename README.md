# UnrealSharp

UnrealSharp is a free, open-source plugin for writing Unreal Engine 5 games in C# on top of .NET 10.

[Workflow Showcase](https://www.youtube.com/watch?v=xR7M2XgCuNU)

## Features

- **Unreal Engine API in C#**: Derive from any UClass. Implement Actors, ActorComponents, and more in C# with access to the Unreal Engine API.
- **Generated bindings**: The C# API is automatically generated from all reflected C++ code. This includes the engine, plugins, and your own project, so any new reflected types or members are immediately available for use in C#.
- **Hot reload**: Recompile and reload C# code without restarting the editor.
- **Full .NET ecosystem**: Pull in any NuGet package you need.
- **MIT licensed**

## Supported platforms

| Platform | Status   |
|----------|----------|
| Windows  | Supported |
| macOS    | Supported |
| Linux    | Planned  |
| iOS      | Planned  |
| Android  | Planned  |

## Experimental Android NativeAOT branch

The `quest-android-nativeaot` branch contains an experimental, end-to-end path for running UnrealSharp-managed game code on Meta Quest with .NET 11 Android ARM64 NativeAOT. Android remains listed as planned above; this branch documents a tested prototype rather than a supported production release.

Unreal continues to own the Android application, content cook, APK, and OBB. The managed game and its UnrealSharp runtime dependencies are compiled into a native shared library that Unreal includes in its package:

```text
C# game code
  -> UnrealSharp generated bindings and registration
  -> .NET 11 Android ARM64 NativeAOT
  -> libUnrealSharpNativeAot.so
  -> Unreal BuildCookRun
  -> Quest APK and OBB
```

### What this branch adds

- A `.NET 11` Android NativeAOT configuration for `android-arm64`.
- NativeAOT bootstrap entry points and statically linked managed-assembly activation.
- Static registration and trimming roots for generated managed Unreal types.
- Android UPL staging for `libUnrealSharpNativeAot.so`.
- Runtime-source project references for AOT builds while preserving the existing hosted .NET editor workflow.
- `UETargetType=Game` binding generation for packaged builds, excluding editor-only reflected members.
- Build automation for selecting the preview .NET SDK, publishing the managed entry project, and copying its native library into Unreal's package inputs.
- Process-environment cleanup for reliable command-line .NET and MSBuild invocation from Unreal AutomationTool.

No Unreal Engine source modifications are required. The integration is contained in this plugin branch and a small set of game-project changes.

### Tested configuration

- Unreal Engine 5.8
- Windows host
- Meta Quest, ARM64
- Android NDK `27.2.12479018`
- .NET SDK `11.0.100-preview.7.26381.103`
- `Microsoft.NETCore.App` and NativeAOT packs `11.0.0-preview.7.26381.103`

These versions reflect the tested environment and are currently pinned by the prototype. Other .NET 11 previews or stable releases may require updating the versions in `UnrealSharp.AOT.props` and the packaging arguments together.

### Game-project integration

Enable the UnrealSharp plugin in the `.uproject` and add `UnrealSharpCore` as a private dependency of the game's Unreal module:

```csharp
PrivateDependencyModuleNames.Add("UnrealSharpCore");
```

The game module must ensure that UnrealSharp starts in the Android process:

```cpp
#include "Modules/ModuleManager.h"

class FMyGameModule final : public FDefaultGameModuleImpl
{
public:
    virtual void StartupModule() override
    {
        FDefaultGameModuleImpl::StartupModule();

#if PLATFORM_ANDROID
        FModuleManager::LoadModuleChecked<IModuleInterface>(TEXT("UnrealSharpCore"));
#endif
    }
};

IMPLEMENT_PRIMARY_GAME_MODULE(FMyGameModule, MyGame, "MyGame");
```

The NativeAOT build automatically selects the runtime project whose name begins with `Managed`; no additional property is required in its `.csproj`.

Compile and save managed-derived Blueprints and save their maps before cooking. Blueprint assets saved against an older managed class layout may otherwise retain stale component-template references.

### Packaging

The normal workflow is **UnrealSharp > Package > Package for Android/Quest** in the editor. The command opens a packaging console and runs these stages in order:

1. Generate and compile Android Game bindings.
2. Publish the managed game and UnrealSharp runtime graph with .NET 11 Android ARM64 NativeAOT.
3. Run Unreal's Android ASTC build, cook, stage, and package pipeline.

The process stops on the first failed stage and leaves the console open so its result can be inspected. Save all Blueprints and maps before running it.

The commands below are the manual equivalent for diagnostics or automation. Close Unreal Editor before using them directly. Set the local paths for the project, engine, Android SDK, NDK, and JDK:

```powershell
$Project = "H:\projects\unreal\MyGame"
$Engine  = "H:\unreal\UE_5.8"

$env:ANDROID_HOME = "C:\Android\sdk"
$env:ANDROID_SDK_ROOT = $env:ANDROID_HOME
$env:NDKROOT = "C:\Android\sdk\ndk\27.2.12479018"
$env:NDK_ROOT = $env:NDKROOT
$env:JAVA_HOME = "C:\Android\jdk-21.0.3"
```

Generate the Android Game bindings first:

```powershell
& "$Engine\Engine\Build\BatchFiles\RunUAT.bat" `
  "-ScriptsForProject=$Project\MyGame.uproject" `
  BuildCookRun `
  "-Project=$Project\MyGame.uproject" `
  -noP4 `
  -platform=Android `
  -clientconfig=Development `
  -build `
  -skipcook `
  -skipstage `
  -skippackage `
  -utf8output
```

First publish the managed game and UnrealSharp runtime graph as Android ARM64 NativeAOT. Replace `MyGame.uproject` with the actual project filename:

```powershell
& "$Engine\Engine\Build\BatchFiles\RunUAT.bat" `
  "-ScriptsForProject=$Project\MyGame.uproject" `
  PackageProject `
  "-Project=$Project\MyGame.uproject" `
  "-ArchiveDirectory=$Project" `
  -UETargetType=Game `
  -UEBuildConfig=Development `
  -TargetPlatform=Android `
  -TargetArchitecture=arm64 `
  -NativeAOT `
  "-UserParams=-p:MicrosoftNETCoreAppRefPackageVersion=11.0.0-preview.7.26381.103" `
  "-UserParams=-p:MicrosoftNETCoreAppRuntimePackageVersion=11.0.0-preview.7.26381.103" `
  "-UserParams=-p:MicrosoftDotNetILCompilerPackageVersion=11.0.0-preview.7.26381.103"
```

The expected native output is:

```text
Binaries/Managed/net11.0-android/native/arm64-v8a/libUnrealSharpNativeAot.so
```

Then let Unreal build, cook, stage, and package the Android application:

```powershell
& "$Engine\Engine\Build\BatchFiles\RunUAT.bat" `
  "-ScriptsForProject=$Project\MyGame.uproject" `
  BuildCookRun `
  "-Project=$Project\MyGame.uproject" `
  -noP4 `
  -platform=Android `
  -clientconfig=Development `
  -build `
  -cook `
  -stage `
  -pak `
  -package `
  -compressed `
  -cookflavor=ASTC `
  -utf8output
```

Install the generated APK and OBB using Unreal's generated install script:

```powershell
Set-Location "$Project\Binaries\Android"
.\Install_MyGame-arm64.bat
```

The first NativeAOT publish can take several minutes because it compiles the UnrealSharp runtime binding graph. Subsequent builds can reuse unchanged outputs. If Unreal reports that the APK is current after the NativeAOT library changes, force regeneration of the generated APK before running `BuildCookRun` again.

Successful device startup includes log messages for loading `libUnrealSharpNativeAot.so`, initializing the Android NativeAOT callbacks, and activating the statically linked managed game assembly.

## Prerequisites

- Unreal Engine 5.6 - 5.8
- .NET 10.0.5 or newer
- A C++ project (strongly recommended, pure Blueprint projects work but are harder to support)

## Getting started

Visit the website's [Get Started](https://www.unrealsharp.com/getting-started/quickstart) page!

If you want to contribute with documentation, you can contribute to this [repository](https://github.com/UnrealSharp/unrealsharp.github.io)!

## Sample projects

- [Sample Defense Game](https://github.com/UnrealSharp/UnrealSharp-SampleDefenseGame) built for Mini Jam 174.
- [Slime Guzzler](https://github.com/UnrealSharp/Epic-MegaJam-Project) Epic MegaJam 2025 entry.
- [UnrealSharp-Cropout](https://github.com/UnrealSharp/UnrealSharp-Cropout) Epic's Cropout sample, ported from Blueprints to C#.

## Code example

A networked, interactable resource pickup written entirely in C#:

```csharp
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.Niagara;

namespace ManagedSharpProject;

public delegate void OnIsPickedUp(bool bIsPickedUp);

[UClass]
public partial class AResourceBase : AActor, IInteractable
{
    public AResourceBase()
    {
        Replicates = true;
        RespawnTime = 500.0f;
    }

    [UProperty(DefaultComponent = true, RootComponent = true)]
    public partial UStaticMeshComponent Mesh { get; set; }

    [UProperty(DefaultComponent = true)]
    public partial UHealthComponent HealthComponent { get; set; }

    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial int PickUpAmount { get; set; }

    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly)]
    protected partial float RespawnTime { get; set; }

    [UProperty(PropertyFlags.BlueprintReadOnly, ReplicatedUsing = nameof(OnRep_IsPickedUp))]
    protected partial bool bIsPickedUp { get; set; }

    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial TSoftObjectPtr<UNiagaraSystem>? PickUpEffect { get; set; }

    [UProperty(PropertyFlags.BlueprintAssignable)]
    public partial TMulticastDelegate<OnIsPickedUp> OnIsPickedUp { get; set; }

    public override void BeginPlay()
    {
        HealthComponent.OnDeath += OnDeath;
        base.BeginPlay();
    }

    [UFunction]
    protected virtual void OnDeath(APlayer player) {}

    public void OnInteract(APlayer player)
    {
        GatherResource(player);
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    protected void GatherResource(APlayer player)
    {
        if (bIsPickedUp)
        {
            return;
        }

        if (!player.Inventory.AddItem(this, PickUpAmount))
        {
            return;
        }

        UExperienceComponent experienceComponent = UExperienceComponent.Get(player.PlayerState);
        experienceComponent.AddExperience(PickUpAmount);

        SystemLibrary.SetTimer(OnRespawned, RespawnTime, false);

        bIsPickedUp = true;
        OnRep_IsPickedUp();
    }

    [UFunction]
    public void OnRespawned()
    {
        bIsPickedUp = false;
        OnRep_IsPickedUp();
    }

    [UFunction]
    public void OnRep_IsPickedUp()
    {
        if (PickUpEffect is not null)
        {
            UNiagaraFunctionLibrary.SpawnSystemAtLocation(this, PickUpEffect, GetActorLocation(), GetActorRotation());
        }

        OnIsPickedUpChanged(bIsPickedUp);
        OnIsPickedUp.Invoke(bIsPickedUp);
    }

    // Overridable from Blueprints
    [UFunction(FunctionFlags.BlueprintEvent)]
    public partial void OnIsPickedUpChanged(bool bIsPickedUp);
    public partial void OnIsPickedUpChanged_Implementation(bool bIsPickedUp)
    {
        SetActorHiddenInGame(bIsPickedUp);
    }
}
```

## Links

- [Documentation](https://www.unrealsharp.com/) and [FAQ](https://www.unrealsharp.com/faq)
- [Roadmap](https://github.com/orgs/UnrealSharp/projects/3)
- [Discord community](https://discord.gg/HQuJUYFxeV)
- [Documentation repo](https://github.com/UnrealSharp/unrealsharp.github.io)

## Contributing
I accept pull requests and any contributions you make are **greatly appreciated**.

## License

MIT. See [`LICENSE`](LICENSE) for the full text.

## Contact

Discord: **olsson.** (yes, with the dot at the end), or just join the [Discord server](https://discord.gg/HQuJUYFxeV).

## Special Thanks
I'd like to give a huge shoutout to [MonoUE](https://mono-ue.github.io/) (Sadly abandoned :( ) for the great resource for integrating C# into Unreal Engine. Some of the systems are modified versions of their integration, and it's been a real time saver. 
