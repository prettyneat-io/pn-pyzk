using System;
using System.Linq;
using PyZK.DotNet;

namespace PyZK.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize Python.NET (call once at application startup)
            // For Windows, you may need to specify the Python DLL path:
            // PyZKClient.InitializePython(@"C:\Python39\python39.dll", @"C:\Python39");
            // For Linux/Mac:
            PyZKClient.InitializePython();

            Console.WriteLine("PyZK .NET Integration Example");
            Console.WriteLine("==============================\n");

            // Replace with your device IP address
            string deviceIp = "192.168.1.201";
            int devicePort = 4370;

            try
            {
                // Create client instance
                using (var client = new PyZKClient(deviceIp, devicePort, timeout: 5))
                {
                    Console.WriteLine($"Connecting to device at {deviceIp}:{devicePort}...");

                    // Connect to device
                    var connectionResult = client.Connect();

                    if (!connectionResult.Success)
                    {
                        Console.WriteLine($"Failed to connect: {connectionResult.Error ?? connectionResult.Message}");
                        return;
                    }

                    Console.WriteLine("✓ Connected successfully!");
                    Console.WriteLine($"  Firmware: {connectionResult.DeviceDetails.FirmwareVersion}");
                    Console.WriteLine($"  Platform: {connectionResult.DeviceDetails.Platform}");
                    Console.WriteLine($"  Device: {connectionResult.DeviceDetails.DeviceName}");
                    Console.WriteLine($"  Serial: {connectionResult.DeviceDetails.SerialNumber}\n");

                    // Get detailed device information
                    Console.WriteLine("Getting device information...");
                    var deviceInfo = client.GetDeviceInfo();

                    if (deviceInfo.Success)
                    {
                        Console.WriteLine($"  MAC Address: {deviceInfo.MacAddress}");
                        Console.WriteLine($"  Device Time: {deviceInfo.DeviceTime}");
                        Console.WriteLine($"  Users: {deviceInfo.UsersCount} / {deviceInfo.UsersCapacity}");
                        Console.WriteLine($"  Fingerprints: {deviceInfo.FingerprintsCount} / {deviceInfo.FingerprintsCapacity}");
                        Console.WriteLine($"  Records: {deviceInfo.RecordsCount}\n");
                    }

                    // Get all users
                    Console.WriteLine("Retrieving users...");
                    var usersResponse = client.GetUsers();

                    if (usersResponse.Success)
                    {
                        Console.WriteLine($"✓ Found {usersResponse.Count} user(s):");
                        foreach (var user in usersResponse.Users)
                        {
                            Console.WriteLine($"  - UID: {user.Uid}, Name: {user.Name}, " +
                                            $"Privilege: {user.PrivilegeName}, User ID: {user.UserId}");
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get users: {usersResponse.Error}\n");
                    }

                    // Get attendance records
                    Console.WriteLine("Retrieving attendance records...");
                    var attendanceResponse = client.GetAttendance();

                    if (attendanceResponse.Success)
                    {
                        Console.WriteLine($"✓ Found {attendanceResponse.Count} attendance record(s)");

                        if (attendanceResponse.Count > 0)
                        {
                            Console.WriteLine("  Recent records:");
                            var recentRecords = attendanceResponse.Attendances
                                .OrderByDescending(a => a.Timestamp)
                                .Take(5);

                            foreach (var record in recentRecords)
                            {
                                var timestamp = record.GetTimestamp();
                                Console.WriteLine($"  - UID: {record.Uid}, Time: {timestamp}, " +
                                                $"Status: {record.Status}, Punch: {record.Punch}");
                            }
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get attendance: {attendanceResponse.Error}\n");
                    }

                    // Example: Add a new user
                    Console.WriteLine("Example: Adding a new user...");
                    var addUserResult = client.AddUser(
                        uid: 9999,
                        name: "Test User",
                        privilege: PyZKClient.Privilege.User,
                        password: "1234",
                        userId: "TEST001"
                    );

                    if (addUserResult.Success)
                    {
                        Console.WriteLine($"✓ {addUserResult.Message}");

                        // Verify user was added
                        usersResponse = client.GetUsers();
                        var addedUser = usersResponse.Users?.FirstOrDefault(u => u.Uid == 9999);
                        if (addedUser != null)
                        {
                            Console.WriteLine($"  Verified: User '{addedUser.Name}' exists in device");
                        }

                        // Clean up: Delete the test user
                        Console.WriteLine("  Cleaning up test user...");
                        var deleteResult = client.DeleteUser(9999);
                        if (deleteResult.Success)
                        {
                            Console.WriteLine($"  ✓ {deleteResult.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add user: {addUserResult.Error}");
                    }
                    Console.WriteLine();

                    // Example: Set device time
                    Console.WriteLine("Example: Synchronizing device time...");
                    var setTimeResult = client.SetTime();
                    if (setTimeResult.Success)
                    {
                        Console.WriteLine($"✓ {setTimeResult.Message}\n");
                    }

                    // Example: Test voice
                    Console.WriteLine("Example: Playing test voice (Thank You)...");
                    var voiceResult = client.TestVoice(0);
                    if (voiceResult.Success)
                    {
                        Console.WriteLine($"✓ {voiceResult.Message}\n");
                    }

                    // Disconnect
                    Console.WriteLine("Disconnecting...");
                    var disconnectResult = client.Disconnect();
                    if (disconnectResult.Success)
                    {
                        Console.WriteLine("✓ Disconnected successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace:\n{ex.StackTrace}");
            }
            finally
            {
                // Shutdown Python.NET (call at application exit)
                // Note: Commenting out as it may cause issues on some systems
                // PyZKClient.ShutdownPython();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
