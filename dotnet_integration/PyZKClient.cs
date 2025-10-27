using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Python.Runtime;

namespace PyZK.DotNet
{
    /// <summary>
    /// Device information returned from ZK device
    /// </summary>
    public class DeviceInfo
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("device_info")]
        public DeviceDetails DeviceDetails { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    public class DeviceDetails
    {
        [JsonPropertyName("firmware_version")]
        public string FirmwareVersion { get; set; }

        [JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("device_name")]
        public string DeviceName { get; set; }
    }

    /// <summary>
    /// Detailed device information including capacity
    /// </summary>
    public class DetailedDeviceInfo
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("firmware_version")]
        public string FirmwareVersion { get; set; }

        [JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("device_name")]
        public string DeviceName { get; set; }

        [JsonPropertyName("mac_address")]
        public string MacAddress { get; set; }

        [JsonPropertyName("device_time")]
        public string DeviceTime { get; set; }

        [JsonPropertyName("users_count")]
        public int UsersCount { get; set; }

        [JsonPropertyName("users_capacity")]
        public int UsersCapacity { get; set; }

        [JsonPropertyName("fingerprints_count")]
        public int FingerprintsCount { get; set; }

        [JsonPropertyName("fingerprints_capacity")]
        public int FingerprintsCapacity { get; set; }

        [JsonPropertyName("records_count")]
        public int RecordsCount { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    /// <summary>
    /// User information from ZK device
    /// </summary>
    public class ZKUser
    {
        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("privilege")]
        public int Privilege { get; set; }

        [JsonPropertyName("privilege_name")]
        public string PrivilegeName { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("group_id")]
        public string GroupId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("card")]
        public long Card { get; set; }
    }

    /// <summary>
    /// Response containing list of users
    /// </summary>
    public class UsersResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("users")]
        public List<ZKUser> Users { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    /// <summary>
    /// Attendance record from ZK device
    /// </summary>
    public class Attendance
    {
        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("punch")]
        public int Punch { get; set; }

        public DateTime? GetTimestamp()
        {
            if (string.IsNullOrEmpty(Timestamp))
                return null;

            if (DateTime.TryParse(Timestamp, out DateTime result))
                return result;

            return null;
        }
    }

    /// <summary>
    /// Response containing list of attendance records
    /// </summary>
    public class AttendanceResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("attendances")]
        public List<Attendance> Attendances { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    /// <summary>
    /// Generic operation response
    /// </summary>
    public class OperationResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    /// <summary>
    /// .NET client for PyZK library using Python.NET
    /// </summary>
    public class PyZKClient : IDisposable
    {
        private dynamic _wrapper;
        private bool _disposed = false;
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly int _timeout;
        private readonly int _password;
        private readonly bool _forceUdp;
        private readonly bool _ommitPing;

        /// <summary>
        /// User privilege constants
        /// </summary>
        public static class Privilege
        {
            public const int User = 0;
            public const int Admin = 14;
        }

        /// <summary>
        /// Initializes the Python.NET environment. Call this once at application startup.
        /// </summary>
        /// <param name="pythonDll">Optional path to Python DLL (e.g., "python39.dll")</param>
        /// <param name="pythonHome">Optional Python home directory</param>
        public static void InitializePython(string pythonDll = null, string pythonHome = null)
        {
            if (!PythonEngine.IsInitialized)
            {
                if (!string.IsNullOrEmpty(pythonDll))
                {
                    Runtime.PythonDLL = pythonDll;
                }

                if (!string.IsNullOrEmpty(pythonHome))
                {
                    PythonEngine.PythonHome = pythonHome;
                }

                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
        }

        /// <summary>
        /// Shuts down the Python.NET environment. Call this at application shutdown.
        /// </summary>
        public static void ShutdownPython()
        {
            if (PythonEngine.IsInitialized)
            {
                PythonEngine.Shutdown();
            }
        }

        /// <summary>
        /// Creates a new PyZK client instance
        /// </summary>
        /// <param name="ipAddress">IP address of the ZK device</param>
        /// <param name="port">Port number (default: 4370)</param>
        /// <param name="timeout">Connection timeout in seconds (default: 5)</param>
        /// <param name="password">Device password (default: 0)</param>
        /// <param name="forceUdp">Force UDP communication (default: false)</param>
        /// <param name="ommitPing">Skip ping test (default: false)</param>
        public PyZKClient(string ipAddress, int port = 4370, int timeout = 5,
                          int password = 0, bool forceUdp = false, bool ommitPing = false)
        {
            _ipAddress = ipAddress;
            _port = port;
            _timeout = timeout;
            _password = password;
            _forceUdp = forceUdp;
            _ommitPing = ommitPing;

            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append("./dotnet_integration");

                dynamic wrapper_module = Py.Import("pyzk_wrapper");
                _wrapper = wrapper_module.PyZKWrapper(ipAddress, port, timeout, password, forceUdp, ommitPing);
            }
        }

        /// <summary>
        /// Gets whether the client is currently connected to the device
        /// </summary>
        public bool IsConnected
        {
            get
            {
                using (Py.GIL())
                {
                    return (bool)_wrapper.is_connected;
                }
            }
        }

        /// <summary>
        /// Connects to the ZK device
        /// </summary>
        /// <returns>Device information and connection status</returns>
        public DeviceInfo Connect()
        {
            using (Py.GIL())
            {
                dynamic result = _wrapper.connect();
                var jsonResult = PythonJsonToString(result);
                return JsonSerializer.Deserialize<DeviceInfo>(jsonResult);
            }
        }

        /// <summary>
        /// Disconnects from the ZK device
        /// </summary>
        /// <returns>Disconnect operation result</returns>
        public OperationResponse Disconnect()
        {
            using (Py.GIL())
            {
                dynamic result = _wrapper.disconnect();
                var jsonResult = PythonJsonToString(result);
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Gets all users from the device
        /// </summary>
        /// <returns>List of users</returns>
        public UsersResponse GetUsers()
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.get_users_json();
                return JsonSerializer.Deserialize<UsersResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Gets all attendance records from the device
        /// </summary>
        /// <returns>List of attendance records</returns>
        public AttendanceResponse GetAttendance()
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.get_attendance_json();
                return JsonSerializer.Deserialize<AttendanceResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Gets detailed device information including capacity
        /// </summary>
        /// <returns>Detailed device information</returns>
        public DetailedDeviceInfo GetDeviceInfo()
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.get_device_info_json();
                return JsonSerializer.Deserialize<DetailedDeviceInfo>(jsonResult);
            }
        }

        /// <summary>
        /// Adds a new user to the device
        /// </summary>
        /// <param name="uid">Unique user identifier</param>
        /// <param name="name">User name</param>
        /// <param name="privilege">User privilege (use Privilege.User or Privilege.Admin)</param>
        /// <param name="password">User password (optional)</param>
        /// <param name="groupId">Group identifier (optional)</param>
        /// <param name="userId">User ID string (optional)</param>
        /// <param name="card">Card number (optional)</param>
        /// <returns>Operation result</returns>
        public OperationResponse AddUser(int uid, string name, int privilege = 0,
                                        string password = "", string groupId = "",
                                        string userId = "", int card = 0)
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.add_user(uid, name, privilege, password, groupId, userId, card);
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Deletes a user from the device by UID
        /// </summary>
        /// <param name="uid">User UID to delete</param>
        /// <returns>Operation result</returns>
        public OperationResponse DeleteUser(int uid)
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.delete_user(uid: uid);
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Deletes a user from the device by user ID string
        /// </summary>
        /// <param name="userId">User ID string to delete</param>
        /// <returns>Operation result</returns>
        public OperationResponse DeleteUserByUserId(string userId)
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.delete_user(user_id: userId);
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Sets the device time
        /// </summary>
        /// <param name="timestamp">DateTime to set (null for current time)</param>
        /// <returns>Operation result</returns>
        public OperationResponse SetTime(DateTime? timestamp = null)
        {
            using (Py.GIL())
            {
                string isoTimestamp = timestamp?.ToString("o");
                string jsonResult = (string)_wrapper.set_time(isoTimestamp);
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Clears all attendance records from the device
        /// </summary>
        /// <returns>Operation result</returns>
        public OperationResponse ClearAttendance()
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.clear_attendance();
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Plays a test voice on the device
        /// </summary>
        /// <param name="index">Voice index (0=Thank You, see documentation for full list)</param>
        /// <returns>Operation result</returns>
        public OperationResponse TestVoice(int index = 0)
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.test_voice(index);
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Restarts the device
        /// </summary>
        /// <returns>Operation result</returns>
        public OperationResponse RestartDevice()
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.restart_device();
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Powers off the device
        /// </summary>
        /// <returns>Operation result</returns>
        public OperationResponse PowerOffDevice()
        {
            using (Py.GIL())
            {
                string jsonResult = (string)_wrapper.poweroff_device();
                return JsonSerializer.Deserialize<OperationResponse>(jsonResult);
            }
        }

        /// <summary>
        /// Helper method to convert Python dict to JSON string
        /// </summary>
        private string PythonJsonToString(dynamic pythonDict)
        {
            dynamic json = Py.Import("json");
            return (string)json.dumps(pythonDict);
        }

        /// <summary>
        /// Disposes the client and disconnects if connected
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (IsConnected)
                    {
                        Disconnect();
                    }
                }
                _disposed = true;
            }
        }
    }
}
