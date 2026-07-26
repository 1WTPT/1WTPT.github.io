$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $repoRoot "source\JournalTrace\JournalTrace.csproj"
$outputExe = Join-Path $repoRoot "source\JournalTrace\bin\Release\JournalTrace.exe"
$downloads = Join-Path $repoRoot "downloads"
$publishedExe = Join-Path $downloads "JournalTrace.exe"

$msbuildCandidates = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
)

$msbuild = $msbuildCandidates |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if (-not $msbuild) {
    $command = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($command) {
        $msbuild = $command.Source
    }
}

if (-not $msbuild) {
    throw "MSBuild was not found. Install Visual Studio 2022 or Build Tools with .NET desktop build tools."
}

if (-not (Test-Path $project)) {
    throw "Project file was not found: $project"
}

Write-Host "Building JournalTrace Release..." -ForegroundColor Cyan

& $msbuild $project /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path $outputExe)) {
    throw "JournalTrace.exe was not found after the build: $outputExe"
}

New-Item -ItemType Directory -Force -Path $downloads | Out-Null
Copy-Item $outputExe $publishedExe -Force

Write-Host "Done: $publishedExe" -ForegroundColor Green
