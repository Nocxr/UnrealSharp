#pragma once

#include "CoreMinimal.h"

#if !UNREALSHARP_NATIVE_AOT
#include <coreclr_delegates.h>
#include <hostfxr.h>
#endif

#include "HAL/PlatformProcess.h"

struct FCSManagedCallbacks;
struct FCSManagedPluginCallbacks;

struct FCSInitializationResult
{
#if UNREALSHARP_NATIVE_AOT
	static constexpr int32 MessageCapacity = 4096;
	uint8 bSuccess = 0;
	UTF8CHAR Message[MessageCapacity] = {};
#else
	bool bSuccess = false;
	const TCHAR* Message = nullptr;
#endif
};

using FInitializeUnrealSharp = void (*)(const UTF8CHAR*, FCSManagedPluginCallbacks*, const void*, FCSManagedCallbacks*, FCSInitializationResult*);

struct FCSDotNetLayout
{
	FString DotNetRoot;
	FString HostFxrPath;
	FString AppAssemblyPath;
	FString RuntimeConfigPath;
	bool bSelfContained = false;

	bool IsValid() const
	{
		return !DotNetRoot.IsEmpty() && !HostFxrPath.IsEmpty() && !RuntimeConfigPath.IsEmpty();
	}
};

class FCSDotNetRuntimeHost
{
public:
	FCSDotNetRuntimeHost() = default;
	~FCSDotNetRuntimeHost();

	bool InitializeManagedRuntime();
	void ShutdownManagedRuntime();

private:
#if !UNREALSHARP_NATIVE_AOT
	static FCSDotNetLayout ResolveDotNetLayout(const FString& PluginAssemblyPath);

	load_assembly_and_get_function_pointer_fn InitializeHost();
	load_assembly_and_get_function_pointer_fn ConfigureRuntime(const FCSDotNetLayout& Layout) const;

	template <typename FunctionPointer>
	bool BindExport(FunctionPointer& OutFunctionPointer, const TCHAR* ExportName)
	{
		OutFunctionPointer = reinterpret_cast<FunctionPointer>(FPlatformProcess::GetDllExport(RuntimeHost, ExportName));
		return OutFunctionPointer != nullptr;
	}

	hostfxr_initialize_for_dotnet_command_line_fn Hostfxr_InitForCommandLine = nullptr;
	hostfxr_initialize_for_runtime_config_fn Hostfxr_InitForRuntimeConfig = nullptr;
	hostfxr_get_runtime_delegate_fn Hostfxr_GetRuntimeDelegate = nullptr;
	hostfxr_close_fn Hostfxr_Close = nullptr;
#endif

	void* RuntimeHost = nullptr;
};
