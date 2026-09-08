param(
    [Parameter(Mandatory = $true)]
    [string] $Project,

    [Parameter(Mandatory = $true)]
    [string] $Engine
)

$ErrorActionPreference = 'Stop'
$projectFile = (Resolve-Path -LiteralPath $Project).Path
$projectRoot = Split-Path -Parent $projectFile
$projectName = [IO.Path]::GetFileNameWithoutExtension($projectFile)
$runUat = Join-Path $Engine 'Engine\Build\BatchFiles\RunUAT.bat'
$logDirectory = Join-Path $projectRoot 'Saved\Logs'
$logFile = Join-Path $logDirectory 'UnrealSharp-AndroidQuest-Package.log'

New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
Start-Transcript -Path $logFile -Force

try {

if (-not (Test-Path -LiteralPath $runUat)) {
    throw "RunUAT.bat was not found under '$Engine'."
}

function Invoke-UAT {
    param([string[]] $Arguments)

    & $runUat @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Unreal AutomationTool failed with exit code $LASTEXITCODE."
    }
}

Write-Host 'Step 1/3: Generating Android Game bindings...' -ForegroundColor Cyan
Invoke-UAT @(
    "-ScriptsForProject=$projectFile",
    'BuildCookRun',
    "-project=$projectFile",
    '-noP4',
    '-platform=Android',
    '-clientconfig=Development',
    '-build',
    '-skipcook',
    '-skipstage',
    '-skippackage',
    '-utf8output'
)

Write-Host 'Step 2/3: Publishing .NET 11 Android ARM64 NativeAOT...' -ForegroundColor Cyan
Invoke-UAT @(
    "-ScriptsForProject=$projectFile",
    'PackageProject',
    "-Project=$projectFile",
    "-ArchiveDirectory=$projectRoot",
    '-UETargetType=Game',
    '-UEBuildConfig=Development',
    '-TargetPlatform=Android',
    '-TargetArchitecture=arm64',
    '-NativeAOT',
    '-UserParams=-p:MicrosoftNETCoreAppRefPackageVersion=11.0.0-preview.7.26381.103',
    '-UserParams=-p:MicrosoftNETCoreAppRuntimePackageVersion=11.0.0-preview.7.26381.103',
    '-UserParams=-p:MicrosoftDotNetILCompilerPackageVersion=11.0.0-preview.7.26381.103'
)

Write-Host 'Step 3/3: Cooking and packaging Android ASTC...' -ForegroundColor Cyan
Invoke-UAT @(
    "-ScriptsForProject=$projectFile",
    'BuildCookRun',
    "-project=$projectFile",
    '-noP4',
    '-platform=Android',
    '-clientconfig=Development',
    '-build',
    '-cook',
    '-stage',
    '-pak',
    '-package',
    '-compressed',
    '-cookflavor=ASTC',
    '-utf8output'
)

Write-Host "Packaging complete: $projectRoot\Binaries\Android" -ForegroundColor Green
}
catch {
    Write-Host ''
    Write-Host 'Android/Quest packaging failed:' -ForegroundColor Red
    Write-Host $_ -ForegroundColor Red
    Write-Host "Full log: $logFile" -ForegroundColor Yellow
}
finally {
    Stop-Transcript
    Read-Host 'Press Enter to close'
}
