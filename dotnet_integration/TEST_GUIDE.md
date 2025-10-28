# PyZKClient.cs Unit Tests

This directory contains comprehensive unit tests for the `PyZKClient.cs` library, which provides .NET integration for the PyZK library.

## Test Overview

The test suite covers:

### Model Tests
- **DeviceInfo** - Connection responses and device information
- **DetailedDeviceInfo** - Extended device information with capacity details
- **ZKUser** - User data and privilege levels
- **UsersResponse** - User list responses
- **Attendance** - Attendance records with timestamp parsing
- **AttendanceResponse** - Attendance list responses
- **OperationResponse** - Generic operation results
- **Template** - Fingerprint template information
- **TemplatesResponse** - Template list responses
- **Various Response Models** - Lock state, versions, network params, etc.

### Functionality Tests
- **JSON Deserialization** - All response models from Python wrapper
- **Error Handling** - Error responses and edge cases
- **Timestamp Parsing** - DateTime conversion and null handling
- **Privilege Constants** - User and Admin privilege values
- **Complete Workflows** - User management, attendance, and template operations

## Test Statistics

- **Total Test Classes**: 12
- **Total Test Methods**: 60+
- **Code Coverage**: Models and deserialization logic

## Prerequisites

### Required Software
- .NET SDK 6.0 or higher
- Python 3.7+ (for Python.NET runtime)

### Required NuGet Packages
The test project uses:
- `xunit` (v2.6.2) - Testing framework
- `xunit.runner.visualstudio` (v2.5.4) - Test runner
- `Microsoft.NET.Test.Sdk` (v17.8.0) - Test SDK
- `Moq` (v4.20.70) - Mocking framework
- `FluentAssertions` (v6.12.0) - Assertion library
- `pythonnet` (v3.0.3) - Python.NET interop
- `coverlet.collector` (v6.0.0) - Code coverage

## Running the Tests

### Option 1: Using .NET CLI

```bash
# Navigate to the dotnet_integration directory
cd /home/kris/Development/Pyzk/pyzk/dotnet_integration

# Run all tests
dotnet test PyZKClient.Tests.csproj

# Run with detailed output
dotnet test PyZKClient.Tests.csproj --verbosity normal

# Run specific test class
dotnet test PyZKClient.Tests.csproj --filter "FullyQualifiedName~DeviceInfoTests"

# Run with code coverage
dotnet test PyZKClient.Tests.csproj --collect:"XPlat Code Coverage"
```

### Option 2: Using Visual Studio

1. Open the solution in Visual Studio
2. Open Test Explorer (Test → Test Explorer)
3. Click "Run All" to run all tests
4. Or right-click specific tests to run individually

### Option 3: Using VS Code

1. Install the ".NET Core Test Explorer" extension
2. Open the Command Palette (Ctrl+Shift+P)
3. Type "Test: Run All Tests"

## Test Structure

### Model Tests
Tests verify that all response models correctly deserialize from JSON:

```csharp
[Fact]
public void DeviceInfo_ShouldDeserializeFromJson()
{
    var json = @"{""success"": true, ""message"": ""Connected""}";
    var deviceInfo = JsonSerializer.Deserialize<DeviceInfo>(json);
    deviceInfo.Success.Should().BeTrue();
}
```

### Error Handling Tests
Tests ensure proper error handling:

```csharp
[Theory]
[InlineData(@"{""success"": false, ""error"": ""Not connected""}")]
public void Responses_ShouldHandleErrorMessages(string json)
{
    var response = JsonSerializer.Deserialize<OperationResponse>(json);
    response.Success.Should().BeFalse();
    response.Error.Should().NotBeNullOrEmpty();
}
```

### Edge Case Tests
Tests cover null values, empty lists, and invalid data:

```csharp
[Fact]
public void Attendance_GetTimestamp_ShouldReturnNullForInvalidTimestamp()
{
    var attendance = new Attendance { Timestamp = "invalid" };
    var parsedTime = attendance.GetTimestamp();
    parsedTime.Should().BeNull();
}
```

### Workflow Tests
Integration-style tests verify complete operations:

```csharp
[Fact]
public void CompleteUserWorkflow_ShouldDeserializeCorrectly()
{
    // Tests: Get users → Add user → Delete user
}
```

## Test Categories

### 1. Deserialization Tests
Verify JSON to C# object conversion for:
- Device information responses
- User management responses
- Attendance records
- Fingerprint templates
- Device configuration responses

### 2. Validation Tests
Verify data validation and parsing:
- Timestamp parsing (valid, invalid, null)
- Privilege level constants
- Null and empty value handling

### 3. Error Scenario Tests
Verify error responses:
- Connection failures
- Operation failures
- Not connected errors
- Invalid data errors

### 4. Integration Tests
Verify complete workflows:
- User lifecycle (add, retrieve, delete)
- Attendance management (get, clear)
- Template management (get, delete)

## Code Coverage

To generate a detailed code coverage report:

```bash
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test PyZKClient.Tests.csproj --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator \
  -reports:"./TestResults/*/coverage.cobertura.xml" \
  -targetdir:"./CoverageReport" \
  -reporttypes:Html

# Open the report
xdg-open ./CoverageReport/index.html
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
    
    - name: Restore dependencies
      run: dotnet restore dotnet_integration/PyZKClient.Tests.csproj
    
    - name: Build
      run: dotnet build dotnet_integration/PyZKClient.Tests.csproj --no-restore
    
    - name: Test
      run: dotnet test dotnet_integration/PyZKClient.Tests.csproj --no-build --verbosity normal
```

## Test Naming Convention

Tests follow the pattern: `[MethodName]_Should[ExpectedBehavior]_[StateUnderTest]`

Examples:
- `DeviceInfo_ShouldDeserializeFromJson` - Tests basic deserialization
- `Attendance_GetTimestamp_ShouldReturnNullForInvalidTimestamp` - Tests error case
- `UsersResponse_ShouldHandleEmptyUserList` - Tests edge case

## Assertions

The tests use FluentAssertions for readable assertions:

```csharp
// Instead of:
Assert.Equal(expected, actual);

// We use:
actual.Should().Be(expected);

// More examples:
response.Success.Should().BeTrue();
users.Should().HaveCount(2);
error.Should().NotBeNullOrEmpty();
timestamp.Should().BeNull();
```

## Troubleshooting

### Python.NET Issues
If you encounter Python.NET initialization errors:

1. Ensure Python is installed and in PATH
2. Install required Python packages: `pip install pyzk`
3. Set Python path in environment:
   ```bash
   export PYTHONHOME=/usr/bin/python3
   ```

### Missing Dependencies
If tests fail to run:

```bash
# Restore NuGet packages
dotnet restore PyZKClient.Tests.csproj

# Clean and rebuild
dotnet clean
dotnet build
```

### Test Discovery Issues
If tests don't appear in Test Explorer:

1. Rebuild the solution
2. Restart VS Code or Visual Studio
3. Check that test methods have `[Fact]` or `[Theory]` attributes

## Contributing

When adding new tests:

1. **Follow naming conventions** - Use descriptive test names
2. **Test all paths** - Cover success, failure, and edge cases
3. **Use FluentAssertions** - For readable assertions
4. **Document complex tests** - Add comments for non-obvious logic
5. **Group related tests** - Use test classes to organize tests
6. **Test error cases** - Don't just test happy paths

## Future Enhancements

Potential additions to the test suite:

- [ ] Mock Python.NET runtime for testing PyZKClient methods
- [ ] Integration tests with mock ZK device
- [ ] Performance/stress tests
- [ ] Thread safety tests
- [ ] Memory leak detection tests
- [ ] Full end-to-end workflow tests

## Test Results Example

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    62, Skipped:     0, Total:    62, Duration: 1.2s
```

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [.NET Test Documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/)

## License

These tests are part of the PyZK project and follow the same license terms.
