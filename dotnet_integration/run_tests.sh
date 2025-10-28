#!/bin/bash

# PyZKClient.cs Test Runner Script
# This script runs the unit tests for PyZKClient.cs

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}PyZKClient.cs Test Runner${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed${NC}"
    echo "Please install .NET SDK 6.0 or higher from https://dotnet.microsoft.com/download"
    exit 1
fi

# Display .NET version
echo -e "${YELLOW}Using .NET SDK:${NC}"
dotnet --version
echo ""

# Change to the project directory
cd "$SCRIPT_DIR"

# Parse command line arguments
COVERAGE=false
VERBOSE=false
FILTER=""

while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -f|--filter)
            FILTER="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  -c, --coverage    Generate code coverage report"
            echo "  -v, --verbose     Run tests with detailed output"
            echo "  -f, --filter      Filter tests by name (e.g., 'DeviceInfoTests')"
            echo "  -h, --help        Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                          # Run all tests"
            echo "  $0 --verbose                # Run with detailed output"
            echo "  $0 --coverage               # Run with code coverage"
            echo "  $0 --filter DeviceInfoTests # Run only DeviceInfoTests"
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Restore dependencies
echo -e "${YELLOW}Restoring dependencies...${NC}"
dotnet restore PyZKClient.Tests.csproj --verbosity quiet
echo -e "${GREEN}✓ Dependencies restored${NC}"
echo ""

# Build the project
echo -e "${YELLOW}Building test project...${NC}"
dotnet build PyZKClient.Tests.csproj --no-restore --verbosity quiet
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Build successful${NC}"
else
    echo -e "${RED}✗ Build failed${NC}"
    exit 1
fi
echo ""

# Prepare test command
TEST_CMD="dotnet test PyZKClient.Tests.csproj --no-build"

if [ "$VERBOSE" = true ]; then
    TEST_CMD="$TEST_CMD --verbosity normal"
else
    TEST_CMD="$TEST_CMD --verbosity minimal"
fi

if [ -n "$FILTER" ]; then
    TEST_CMD="$TEST_CMD --filter \"FullyQualifiedName~$FILTER\""
fi

if [ "$COVERAGE" = true ]; then
    TEST_CMD="$TEST_CMD --collect:\"XPlat Code Coverage\""
fi

# Run tests
echo -e "${YELLOW}Running tests...${NC}"
echo ""

eval $TEST_CMD
TEST_RESULT=$?

echo ""

if [ $TEST_RESULT -eq 0 ]; then
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}✓ All tests passed!${NC}"
    echo -e "${GREEN}========================================${NC}"
    
    # Generate coverage report if requested
    if [ "$COVERAGE" = true ]; then
        echo ""
        echo -e "${YELLOW}Generating coverage report...${NC}"
        
        # Check if reportgenerator is installed
        if ! command -v reportgenerator &> /dev/null; then
            echo -e "${YELLOW}Installing reportgenerator tool...${NC}"
            dotnet tool install -g dotnet-reportgenerator-globaltool
        fi
        
        # Find the latest coverage file
        COVERAGE_FILE=$(find ./TestResults -name "coverage.cobertura.xml" -type f -printf '%T@ %p\n' | sort -n | tail -1 | cut -f2- -d" ")
        
        if [ -n "$COVERAGE_FILE" ]; then
            reportgenerator \
                -reports:"$COVERAGE_FILE" \
                -targetdir:"./CoverageReport" \
                -reporttypes:"Html;TextSummary" \
                > /dev/null 2>&1
            
            echo -e "${GREEN}✓ Coverage report generated${NC}"
            echo -e "${YELLOW}Coverage summary:${NC}"
            cat ./CoverageReport/Summary.txt
            echo ""
            echo -e "${YELLOW}Full report: file://$SCRIPT_DIR/CoverageReport/index.html${NC}"
            
            # Try to open the report in browser (Linux)
            if command -v xdg-open &> /dev/null; then
                xdg-open "./CoverageReport/index.html" &> /dev/null &
            fi
        else
            echo -e "${RED}✗ Coverage file not found${NC}"
        fi
    fi
else
    echo -e "${RED}========================================${NC}"
    echo -e "${RED}✗ Tests failed!${NC}"
    echo -e "${RED}========================================${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Done!${NC}"
