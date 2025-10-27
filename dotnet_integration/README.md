# PyZK .NET Integration Guide

This guide explains how to use the PyZK library from .NET applications using Python.NET.

## Overview

The integration consists of three main components:

1. **pyzk_wrapper.py** - Python wrapper that provides a .NET-friendly API with JSON serialization
2. **PyZKClient.cs** - C# client library that handles Python interop and provides strongly-typed models
3. **Program.cs** - Example console application demonstrating usage

## Prerequisites

### Python Requirements
- Python 3.7 or higher
- pip package manager

### .NET Requirements
- .NET 6.0 or higher SDK
- pythonnet NuGet package (version 3.0.3+)

## Installation Steps

### 1. Install Python Dependencies

From the project root directory:

```bash
pip install pyzk
```

Or if you're working with the source:

```bash
python setup.py install
```

### 2. Set Up .NET Project

Create a new .NET project or use the provided example:

```bash
cd dotnet_integration
dotnet restore PyZKExample.csproj
```

### 3. Configure Python.NET

#### On Windows

You may need to specify the Python DLL path explicitly:

```csharp
PyZKClient.InitializePython(
    @"C:\Python39\python39.dll", 
    @"C:\Python39"
);
```

#### On Linux/macOS

Python.NET usually auto-detects Python:

```csharp
PyZKClient.InitializePython();
```

## Usage Examples

### Basic Connection and Device Info

```csharp
using PyZK.DotNet;

// Initialize Python.NET once at startup
PyZKClient.InitializePython();

// Create client
using (var client = new PyZKClient("192.168.1.201", port: 4370))
{
    // Connect
    var result = client.Connect();
    if (result.Success)
    {
        Console.WriteLine($"Connected to {result.DeviceDetails.DeviceName}");
        
        // Get detailed info
        var info = client.GetDeviceInfo();
        Console.WriteLine($"Users: {info.UsersCount}/{info.UsersCapacity}");
        
        // Disconnect
        client.Disconnect();
    }
}
```

### Managing Users

```csharp
// Get all users
var usersResponse = client.GetUsers();
if (usersResponse.Success)
{
    foreach (var user in usersResponse.Users)
    {
        Console.WriteLine($"{user.Name} (UID: {user.Uid})");
    }
}

// Add a new user
var addResult = client.AddUser(
    uid: 1001,
    name: "John Doe",
    privilege: PyZKClient.Privilege.User,
    password: "1234",
    userId: "JOHN001"
);

// Delete a user
var deleteResult = client.DeleteUser(uid: 1001);
```

### Retrieving Attendance Records

```csharp
var attendanceResponse = client.GetAttendance();
if (attendanceResponse.Success)
{
    Console.WriteLine($"Total records: {attendanceResponse.Count}");
    
    foreach (var record in attendanceResponse.Attendances)
    {
        var timestamp = record.GetTimestamp();
        Console.WriteLine($"UID: {record.Uid}, Time: {timestamp}");
    }
}
```

### Device Operations

```csharp
// Synchronize time
var timeResult = client.SetTime(DateTime.Now);

// Play test voice
var voiceResult = client.TestVoice(0); // 0 = "Thank You"

// Clear attendance records
var clearResult = client.ClearAttendance();

// Restart device
var restartResult = client.RestartDevice();

// Power off device
var poweroffResult = client.PowerOffDevice();
```

## API Reference

### PyZKClient Constructor

```csharp
PyZKClient(
    string ipAddress,           // Device IP address
    int port = 4370,           // Device port
    int timeout = 5,           // Connection timeout (seconds)
    int password = 0,          // Device password
    bool forceUdp = false,     // Force UDP communication
    bool ommitPing = false     // Skip ping test
)
```

### Main Methods

| Method | Return Type | Description |
|--------|------------|-------------|
| `Connect()` | `DeviceInfo` | Connect to device and get basic info |
| `Disconnect()` | `OperationResponse` | Disconnect from device |
| `GetUsers()` | `UsersResponse` | Retrieve all users |
| `GetAttendance()` | `AttendanceResponse` | Retrieve attendance records |
| `GetDeviceInfo()` | `DetailedDeviceInfo` | Get detailed device information |
| `AddUser(...)` | `OperationResponse` | Add new user to device |
| `DeleteUser(int uid)` | `OperationResponse` | Delete user by UID |
| `DeleteUserByUserId(string userId)` | `OperationResponse` | Delete user by user ID |
| `SetTime(DateTime?)` | `OperationResponse` | Set device time |
| `ClearAttendance()` | `OperationResponse` | Clear all attendance records |
| `TestVoice(int index)` | `OperationResponse` | Play test voice |
| `RestartDevice()` | `OperationResponse` | Restart the device |
| `PowerOffDevice()` | `OperationResponse` | Power off the device |

### Response Models

#### DeviceInfo
- `Success` (bool)
- `Message` (string)
- `DeviceDetails` (object)
  - `FirmwareVersion` (string)
  - `SerialNumber` (string)
  - `Platform` (string)
  - `DeviceName` (string)
- `Error` (string)

#### ZKUser
- `Uid` (int) - Unique identifier
- `Name` (string) - User name
- `Privilege` (int) - Privilege level (0=User, 14=Admin)
- `PrivilegeName` (string) - "User" or "Admin"
- `Password` (string) - User password
- `GroupId` (string) - Group identifier
- `UserId` (string) - User ID string
- `Card` (long) - Card number

#### Attendance
- `Uid` (int) - User UID
- `UserId` (string) - User ID
- `Timestamp` (string) - ISO format timestamp
- `Status` (int) - Attendance status
- `Punch` (int) - Punch type
- `GetTimestamp()` - Helper method to parse timestamp to DateTime

## Building the Example

```bash
cd dotnet_integration
dotnet build PyZKExample.csproj
```

## Running the Example

1. Update the device IP address in `Program.cs`
2. Ensure the ZK device is accessible on your network
3. Run the application:

```bash
dotnet run --project PyZKExample.csproj
```

## Deployment Considerations

### Option 1: Embedded Python
Use embedded Python to bundle Python runtime with your .NET application. This eliminates the need for users to install Python separately.

### Option 2: System Python
Require Python to be installed on the target system. Document the specific Python version requirements.

### Option 3: Virtual Environment
Create a virtual environment with your application and configure Python.NET to use it:

```csharp
PyZKClient.InitializePython(
    pythonDll: "/path/to/venv/bin/python",
    pythonHome: "/path/to/venv"
);
```

## Troubleshooting

### Python.NET Initialization Fails

**Problem**: `PythonEngine.Initialize()` throws an exception

**Solutions**:
1. Ensure Python is in your PATH
2. Explicitly set `Runtime.PythonDLL` before initialization
3. Check Python architecture matches your .NET app (x64 vs x86)

### Module Import Errors

**Problem**: "No module named 'zk'" or "No module named 'pyzk_wrapper'"

**Solutions**:
1. Verify pyzk is installed: `pip list | grep pyzk`
2. Check sys.path includes the correct directory
3. Ensure pyzk_wrapper.py is in the dotnet_integration folder

### Connection Timeout

**Problem**: Device connection times out

**Solutions**:
1. Verify device IP address and port
2. Check network connectivity with ping
3. Increase timeout parameter
4. Try `forceUdp: true` parameter
5. Try `ommitPing: true` if device doesn't respond to ping

### GIL (Global Interpreter Lock) Issues

**Problem**: Threading errors or deadlocks

**Solutions**:
1. Always use `using (Py.GIL())` when calling Python code
2. Call `PythonEngine.BeginAllowThreads()` after initialization
3. Don't hold GIL longer than necessary

## Performance Tips

1. **Connection Pooling**: Keep connections alive for multiple operations
2. **Batch Operations**: Retrieve all users/attendance at once rather than individually
3. **Async Wrapper**: Consider wrapping blocking calls in async methods
4. **Error Handling**: Always check `Success` property in responses

## Advanced Usage

### Custom Timeout Handling

```csharp
var client = new PyZKClient("192.168.1.201", timeout: 10);
```

### Working with Multiple Devices

```csharp
var devices = new[] { "192.168.1.201", "192.168.1.202" };

foreach (var ip in devices)
{
    using (var client = new PyZKClient(ip))
    {
        var result = client.Connect();
        if (result.Success)
        {
            // Process device...
            client.Disconnect();
        }
    }
}
```

### Integration with ASP.NET Core

```csharp
// Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Initialize Python once at startup
    PyZKClient.InitializePython();
    
    // Register as singleton or scoped service
    services.AddSingleton<IZKDeviceService, ZKDeviceService>();
}
```

## License

This integration follows the same license as the pyzk library. See LICENSE.txt for details.

## Support

For issues specific to:
- **pyzk library**: https://github.com/fananimi/pyzk/issues
- **Python.NET**: https://github.com/pythonnet/pythonnet/issues
- **This integration**: Create an issue in the pyzk repository with "[.NET Integration]" prefix
