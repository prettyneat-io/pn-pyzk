# PyZK .NET Integration Test Suite

This directory contains comprehensive unit tests for both the Python wrapper (`pyzk_wrapper.py`) and the C# client (`PyZKClient.cs`) in the PyZK .NET integration.

## Overview

This test suite consists of two parts:

1. **Python Tests** (`test_pyzk_wrapper.py`) - Tests the Python wrapper layer that interfaces with the pyzk library
2. **C# Tests** (`PyZKClientTests.cs`) - Tests the C# client models and JSON deserialization

Both test suites use mocking to avoid requiring physical ZK devices.

---

## Python Tests (pyzk_wrapper.py)

The Python test suite uses Python's `unittest` framework with mocking to test all wrapper methods.

## Test Coverage

### Test Classes

1. **TestPyZKWrapperInit** (6 tests)
   - Initialization with default and custom parameters
   - Connection success and failure scenarios
   - Disconnection handling

2. **TestPyZKWrapperUsers** (9 tests)
   - Getting users list as JSON
   - Adding new users
   - Deleting users by UID or user_id
   - Error handling for user operations

3. **TestPyZKWrapperAttendance** (5 tests)
   - Getting attendance records
   - Handling null timestamps
   - Clearing attendance records
   - Error handling

4. **TestPyZKWrapperDeviceInfo** (2 tests)
   - Getting comprehensive device information
   - Handling connection errors

5. **TestPyZKWrapperTimeManagement** (4 tests)
   - Setting device time with specific timestamp
   - Setting device time to current time
   - Invalid timestamp handling

6. **TestPyZKWrapperDeviceControl** (9 tests)
   - Testing device voice
   - Restarting device
   - Powering off device
   - Enabling/disabling device
   - Refreshing and freeing data
   - Clearing all device data

7. **TestPyZKWrapperDoorControl** (4 tests)
   - Unlocking door with default and custom times
   - Getting lock state

8. **TestPyZKWrapperLCDControl** (2 tests)
   - Writing text to LCD
   - Clearing LCD

9. **TestPyZKWrapperVersionInfo** (4 tests)
   - Getting face algorithm version
   - Getting fingerprint algorithm version
   - Checking face function status
   - Checking firmware compatibility

10. **TestPyZKWrapperConfiguration** (5 tests)
    - Getting network parameters
    - Getting PIN width
    - Getting extend format configurations
    - Setting SDK build

11. **TestPyZKWrapperTemplates** (5 tests)
    - Getting all fingerprint templates
    - Getting specific user template
    - Deleting templates
    - Error handling

12. **TestPyZKWrapperEnrollment** (5 tests)
    - User enrollment
    - User verification
    - Canceling capture
    - Registering for events

13. **TestPyZKWrapperErrorHandling** (1 test)
    - Verifying all methods check connection status

## Running the Tests

### Prerequisites

```bash
# No additional dependencies needed - uses only Python standard library
```

### Run All Tests

```bash
cd /home/kris/Development/Pyzk/pyzk/dotnet_integration
python3 test_pyzk_wrapper.py
```

### Run Specific Test Class

```bash
python3 -m unittest test_pyzk_wrapper.TestPyZKWrapperUsers
```

### Run Specific Test Method

```bash
python3 -m unittest test_pyzk_wrapper.TestPyZKWrapperUsers.test_add_user_success
```

### Run with Verbose Output

```bash
python3 test_pyzk_wrapper.py -v
```

## Test Statistics

- **Total Tests**: 61
- **Test Classes**: 13
- **Coverage**: All 50+ wrapper methods
- **Mocking**: All tests use mocks - no physical device required

## Test Methodology

### Mocking Strategy

All tests use `unittest.mock` to simulate:
- ZK device connections
- Device responses
- User objects
- Attendance records
- Fingerprint templates
- Error conditions

### Test Patterns

1. **Success Cases**: Verify correct behavior with valid inputs
2. **Error Cases**: Verify proper error handling
3. **Edge Cases**: Test boundary conditions (null values, empty lists, etc.)
4. **Connection Checks**: Ensure methods verify connection status

### Example Test

```python
def test_add_user_success(self):
    """Test adding a new user."""
    result_json = self.wrapper.add_user(
        uid=10,
        name="Test User",
        privilege=const.USER_DEFAULT,
        password="1234",
        user_id="010"
    )
    result = json.loads(result_json)
    
    self.assertTrue(result["success"])
    self.assertIn("Test User", result["message"])
    
    self.wrapper.conn.disable_device.assert_called_once()
    self.wrapper.conn.set_user.assert_called_once()
    self.wrapper.conn.enable_device.assert_called_once()
```

## Continuous Integration

These tests can be integrated into CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run PyZK Wrapper Tests
  run: |
    cd dotnet_integration
    python3 test_pyzk_wrapper.py
```

## Adding New Tests

When adding new wrapper methods:

1. Add test class or method to appropriate test class
2. Use mocking to simulate device behavior
3. Test success, failure, and edge cases
4. Verify proper enable/disable device calls
5. Check JSON response format

### Template for New Test

```python
def test_new_method(self):
    """Test description."""
    # Setup
    self.wrapper.conn.method_name.return_value = expected_value
    
    # Execute
    result_json = self.wrapper.method_name(param1, param2)
    result = json.loads(result_json)
    
    # Assert
    self.assertTrue(result["success"])
    self.assertEqual(result["key"], expected_value)
    self.wrapper.conn.method_name.assert_called_once()
```

## Troubleshooting

### Import Errors

If you see import errors, ensure the parent directory is in the Python path:

```python
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
```

### Mock Failures

If mocks aren't working correctly, verify:
- Mock setup matches actual method signatures
- Return values match expected types
- Mock paths match import structure

### Test Failures

For debugging test failures:
1. Run with `-v` flag for verbose output
2. Check mock setup and assertions
3. Verify object constructors match actual classes
4. Print intermediate values for debugging

## Best Practices

1. **Isolation**: Each test should be independent
2. **Mocking**: Never require actual hardware
3. **Coverage**: Test success, failure, and edge cases
4. **Naming**: Use descriptive test names
5. **Documentation**: Include docstrings for all tests
6. **Assertions**: Use specific assertions (assertEqual vs assertTrue)

## License

Same as PyZK project - see main LICENSE file.

---

## C# Tests (PyZKClient.cs)

The C# test suite uses xUnit with FluentAssertions to test the .NET client models and JSON deserialization.

### Test Coverage

The C# tests cover:

1. **Model Deserialization** - All response models from JSON
2. **Error Handling** - Error responses and validation
3. **Edge Cases** - Null values, empty lists, invalid data
4. **Workflows** - Complete operation sequences
5. **Data Parsing** - Timestamp parsing and validation
6. **Constants** - Privilege levels and configuration values

### Test Classes

1. **DeviceInfoTests** - Connection and device information responses
2. **DetailedDeviceInfoTests** - Extended device information with capacity
3. **ZKUserTests** - User data and privilege handling
4. **UsersResponseTests** - User list responses
5. **AttendanceTests** - Attendance records and timestamp parsing
6. **AttendanceResponseTests** - Attendance list responses
7. **OperationResponseTests** - Generic operation results
8. **TemplateTests** - Fingerprint template information
9. **TemplatesResponseTests** - Template list responses
10. **ResponseModelsTests** - All specialized response models
11. **PyZKClientPrivilegeTests** - Privilege constants
12. **ErrorHandlingTests** - Error scenarios
13. **JsonSerializationTests** - Edge cases and null handling
14. **WorkflowTests** - Complete operation workflows

### Running C# Tests

#### Prerequisites

- .NET SDK 9.0 or higher
- NuGet packages (automatically restored):
  - xUnit 2.6.2
  - FluentAssertions 6.12.0
  - Moq 4.20.70
  - pythonnet 3.0.3
  - Microsoft.NET.Test.Sdk 17.8.0

#### Using the Test Runner Script (Linux/Mac)

```bash
cd /home/kris/Development/Pyzk/pyzk/dotnet_integration

# Run all tests
./run_tests.sh

# Run with verbose output
./run_tests.sh --verbose

# Run with code coverage
./run_tests.sh --coverage

# Run specific test class
./run_tests.sh --filter DeviceInfoTests

# Show help
./run_tests.sh --help
```

#### Using PowerShell (Windows)

```powershell
cd C:\path\to\pyzk\dotnet_integration

# Run all tests
.\run_tests.ps1

# Run with verbose output
.\run_tests.ps1 -Verbose

# Run with code coverage
.\run_tests.ps1 -Coverage

# Run specific test class
.\run_tests.ps1 -Filter DeviceInfoTests
```

#### Using .NET CLI

```bash
# Restore dependencies
dotnet restore PyZKClient.Tests.csproj

# Build tests
dotnet build PyZKClient.Tests.csproj

# Run all tests
dotnet test PyZKClient.Tests.csproj --verbosity normal

# Run with detailed output
dotnet test PyZKClient.Tests.csproj --logger "console;verbosity=detailed"

# Run specific test
dotnet test PyZKClient.Tests.csproj --filter "FullyQualifiedName~DeviceInfoTests"

# Run with code coverage
dotnet test PyZKClient.Tests.csproj --collect:"XPlat Code Coverage"
```

### Test Statistics (C#)

- **Total Tests**: 41+
- **Test Classes**: 14
- **Coverage**: All response models and data structures
- **Framework**: xUnit with FluentAssertions

### C# Test Methodology

#### JSON Deserialization Testing

```csharp
[Fact]
public void DeviceInfo_ShouldDeserializeFromJson()
{
    // Arrange
    var json = @"{
        ""success"": true,
        ""message"": ""Connected successfully""
    }";

    // Act
    var deviceInfo = JsonSerializer.Deserialize<DeviceInfo>(json);

    // Assert
    deviceInfo.Should().NotBeNull();
    deviceInfo!.Success.Should().BeTrue();
    deviceInfo.Message.Should().Be("Connected successfully");
}
```

#### Edge Case Testing

```csharp
[Fact]
public void Attendance_GetTimestamp_ShouldReturnNullForInvalidTimestamp()
{
    // Arrange
    var attendance = new Attendance { Timestamp = "invalid-timestamp" };

    // Act
    var parsedTime = attendance.GetTimestamp();

    // Assert
    parsedTime.Should().BeNull();
}
```

#### Error Handling Testing

```csharp
[Theory]
[InlineData(@"{""success"": false, ""error"": ""Not connected""}")]
public void Responses_ShouldHandleErrorMessages(string json)
{
    // Act
    var response = JsonSerializer.Deserialize<OperationResponse>(json);

    // Assert
    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Error.Should().NotBeNullOrEmpty();
}
```

### Adding New C# Tests

When adding new response models:

1. Add model tests to verify JSON deserialization
2. Test success and error responses
3. Test null and default values
4. Add workflow tests for complete operations

Example template:

```csharp
[Fact]
public void NewModel_ShouldDeserializeFromJson()
{
    // Arrange
    var json = @"{ ""success"": true, ""data"": ""value"" }";

    // Act
    var model = JsonSerializer.Deserialize<NewModel>(json);

    // Assert
    model.Should().NotBeNull();
    model!.Success.Should().BeTrue();
    model.Data.Should().Be("value");
}
```

### Code Coverage

Generate code coverage reports:

```bash
# Run tests with coverage
dotnet test PyZKClient.Tests.csproj --collect:"XPlat Code Coverage"

# Install report generator (if not installed)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"./TestResults/*/coverage.cobertura.xml" \
  -targetdir:"./CoverageReport" \
  -reporttypes:Html

# Open report
xdg-open ./CoverageReport/index.html  # Linux
open ./CoverageReport/index.html      # Mac
start ./CoverageReport/index.html     # Windows
```

---

## Combined Test Execution

Run both Python and C# tests together:

### Linux/Mac

```bash
# Create a combined test script
cat > run_all_tests.sh << 'EOF'
#!/bin/bash
echo "========================================="
echo "Running Python Tests"
echo "========================================="
python3 test_pyzk_wrapper.py
PYTHON_RESULT=$?

echo ""
echo "========================================="
echo "Running C# Tests"
echo "========================================="
./run_tests.sh
CSHARP_RESULT=$?

echo ""
echo "========================================="
echo "Test Summary"
echo "========================================="
if [ $PYTHON_RESULT -eq 0 ] && [ $CSHARP_RESULT -eq 0 ]; then
    echo "✓ All tests passed!"
    exit 0
else
    echo "✗ Some tests failed"
    exit 1
fi
EOF

chmod +x run_all_tests.sh
./run_all_tests.sh
```

### Windows PowerShell

```powershell
# Run Python tests
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Running Python Tests" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
python test_pyzk_wrapper.py
$pythonResult = $LASTEXITCODE

# Run C# tests
Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Running C# Tests" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
.\run_tests.ps1
$csharpResult = $LASTEXITCODE

# Summary
Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Test Summary" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
if ($pythonResult -eq 0 -and $csharpResult -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ Some tests failed" -ForegroundColor Red
    exit 1
}
```

---

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
    
    - name: Setup Python
      uses: actions/setup-python@v4
      with:
        python-version: '3.11'
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Install Python dependencies
      run: pip install -r dotnet_integration/requirements.txt
    
    - name: Run Python tests
      run: cd dotnet_integration && python3 test_pyzk_wrapper.py
    
    - name: Restore .NET dependencies
      run: dotnet restore dotnet_integration/PyZKClient.Tests.csproj
    
    - name: Build C# tests
      run: dotnet build dotnet_integration/PyZKClient.Tests.csproj
    
    - name: Run C# tests
      run: dotnet test dotnet_integration/PyZKClient.Tests.csproj --no-build --verbosity normal
```

---

## Documentation

For detailed C# test documentation, see:
- [TEST_GUIDE.md](TEST_GUIDE.md) - Comprehensive C# test guide

For usage examples, see:
- [QUICKSTART.md](QUICKSTART.md) - Quick start guide
- [README.md](README.md) - Main documentation

---
