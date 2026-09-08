using UnrealSharp.Automation.Utilities;

namespace UnrealSharp.Automation.Processes;

public class DotnetProcess : BuildToolProcess
{
    public DotnetProcess() : base(DotNetUtilities.DotNetExecutable)
    {
        StartInfo.Environment.Remove("DOTNET_HOST_PATH");
        StartInfo.Environment.Remove("DOTNET_ROOT");
        StartInfo.Environment.Remove("MSBuildExtensionsPath");
        StartInfo.Environment.Remove("MSBUILD_EXE_PATH");
        StartInfo.Environment.Remove("MSBuildSDKsPath");
        StartInfo.Environment["DOTNET_ROLL_FORWARD"] = "LatestMinor";
    }
}
