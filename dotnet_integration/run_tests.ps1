# PyZKClient.cs Test Runner Script (PowerShell)
# This script runs the unit tests for PyZKClient.cs

param(
    [switch]$Coverage,
    [switch]$Verbose,
    [string]$Filter = "",
    [switch]$Help
)

# Colors
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"

function Write-ColorOutput($ForegroundColor, $Message) {
    Write-Host $Message -ForegroundColor $ForegroundColor
}

function Show-Help {
    Write-Host "Usage: .\run_tests.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Coverage     Generate code coverage report"
    Write-Host "  -Verbose      Run tests with detailed output"
    Write-Host "  -Filter       Filter tests by name (e.g., 'DeviceInfoTests')"
    Write-Host "  -Help         Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\run_tests.ps1                          # Run all tests"
    Write-Host "  .\run_tests.ps1 -Verbose                 # Run with detailed output"
    Write-Host "  .\run_tests.ps1 -Coverage                # Run with code coverage"
    Write-Host "  .\run_tests.ps1 -Filter DeviceInfoTests  # Run only DeviceInfoTests"
    exit 0
}

if ($Help) {
    Show-Help
}

Write-ColorOutput $Green "========================================"
Write-ColorOutput $Green "PyZKClient.cs Test Runner"
Write-ColorOutput $Green "========================================"
Write-Host ""

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-ColorOutput $Yellow "Using .NET SDK:"
    Write-Host $dotnetVersion
    Write-Host ""
} catch {
    Write-ColorOutput $Red "Error: .NET SDK is not installed"
    Write-Host "Please install .NET SDK 6.0 or higher from https://dotnet.microsoft.com/download"
    exit 1
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# Restore dependencies
Write-ColorOutput $Yellow "Restoring dependencies..."
dotnet restore PyZKClient.Tests.csproj --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-ColorOutput $Green "✓ Dependencies restored"
} else {
    Write-ColorOutput $Red "✗ Dependency restoration failed"
    exit 1
}
Write-Host ""

# Build the project
Write-ColorOutput $Yellow "Building test project..."
dotnet build PyZKClient.Tests.csproj --no-restore --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-ColorOutput $Green "✓ Build successful"
} else {
    Write-ColorOutput $Red "✗ Build failed"
    exit 1
}
Write-Host ""

# Prepare test command
$testArgs = @("test", "PyZKClient.Tests.csproj", "--no-build")

if ($Verbose) {
    $testArgs += "--verbosity", "normal"
} else {
    $testArgs += "--verbosity", "minimal"
}

if ($Filter) {
    $testArgs += "--filter", "FullyQualifiedName~$Filter"
}

if ($Coverage) {
    $testArgs += "--collect:XPlat Code Coverage"
}

# Run tests
Write-ColorOutput $Yellow "Running tests..."
Write-Host ""

& dotnet $testArgs
$testResult = $LASTEXITCODE

Write-Host ""

if ($testResult -eq 0) {
    Write-ColorOutput $Green "========================================"
    Write-ColorOutput $Green "✓ All tests passed!"
    Write-ColorOutput $Green "========================================"
    
    # Generate coverage report if requested
    if ($Coverage) {
        Write-Host ""
        Write-ColorOutput $Yellow "Generating coverage report..."
        
        # Check if reportgenerator is installed
        try {
            $null = Get-Command reportgenerator -ErrorAction Stop
        } catch {
            Write-ColorOutput $Yellow "Installing reportgenerator tool..."
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }
        
        # Find the latest coverage file
        $coverageFile = Get-ChildItem -Path ".\TestResults" -Filter "coverage.cobertura.xml" -Recurse |
                        Sort-Object LastWriteTime -Descending |
                        Select-Object -First 1 -ExpandProperty FullName
        
        if ($coverageFile) {
            reportgenerator `
                -reports:"$coverageFile" `
                -targetdir:".\CoverageReport" `
                -reporttypes:"Html;TextSummary" | Out-Null
            
            Write-ColorOutput $Green "✓ Coverage report generated"
            Write-ColorOutput $Yellow "Coverage summary:"
            Get-Content ".\CoverageReport\Summary.txt"
            Write-Host ""
            Write-ColorOutput $Yellow "Full report: file:///$($scriptDir)\CoverageReport\index.html"
            
            # Try to open the report in browser
            Start-Process ".\CoverageReport\index.html"
        } else {
            Write-ColorOutput $Red "✗ Coverage file not found"
        }
    }
} else {
    Write-ColorOutput $Red "========================================"
    Write-ColorOutput $Red "✗ Tests failed!"
    Write-ColorOutput $Red "========================================"
    exit 1
}

Write-Host ""
Write-ColorOutput $Green "Done!"
