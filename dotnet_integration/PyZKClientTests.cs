using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace PyZK.DotNet.Tests
{
    /// <summary>
    /// Tests for DeviceInfo model
    /// </summary>
    public class DeviceInfoTests
    {
        [Fact]
        public void DeviceInfo_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""message"": ""Connected successfully"",
                ""device_info"": {
                    ""firmware_version"": ""Ver 6.60"",
                    ""serial_number"": ""ABC123456"",
                    ""platform"": ""ZEM560"",
                    ""device_name"": ""MainGate""
                }
            }";

            // Act
            var deviceInfo = JsonSerializer.Deserialize<DeviceInfo>(json);

            // Assert
            deviceInfo.Should().NotBeNull();
            deviceInfo!.Success.Should().BeTrue();
            deviceInfo.Message.Should().Be("Connected successfully");
            deviceInfo.DeviceDetails.Should().NotBeNull();
            deviceInfo.DeviceDetails!.FirmwareVersion.Should().Be("Ver 6.60");
            deviceInfo.DeviceDetails.SerialNumber.Should().Be("ABC123456");
        }

        [Fact]
        public void DeviceInfo_ShouldHandleErrorResponse()
        {
            // Arrange
            var json = @"{
                ""success"": false,
                ""message"": ""Connection failed"",
                ""error"": ""Connection timeout""
            }";

            // Act
            var deviceInfo = JsonSerializer.Deserialize<DeviceInfo>(json);

            // Assert
            deviceInfo.Should().NotBeNull();
            deviceInfo!.Success.Should().BeFalse();
            deviceInfo.Message.Should().Be("Connection failed");
            deviceInfo.Error.Should().Be("Connection timeout");
        }
    }

    /// <summary>
    /// Tests for DetailedDeviceInfo model
    /// </summary>
    public class DetailedDeviceInfoTests
    {
        [Fact]
        public void DetailedDeviceInfo_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""firmware_version"": ""Ver 6.60"",
                ""serial_number"": ""ABC123"",
                ""platform"": ""ZEM560"",
                ""device_name"": ""MainGate"",
                ""mac_address"": ""00:11:22:33:44:55"",
                ""device_time"": ""2025-10-28T10:30:00"",
                ""users_count"": 50,
                ""users_capacity"": 1000,
                ""fingerprints_count"": 100,
                ""fingerprints_capacity"": 2000,
                ""records_count"": 500
            }";

            // Act
            var info = JsonSerializer.Deserialize<DetailedDeviceInfo>(json);

            // Assert
            info.Should().NotBeNull();
            info!.Success.Should().BeTrue();
            info.FirmwareVersion.Should().Be("Ver 6.60");
            info.UsersCount.Should().Be(50);
            info.UsersCapacity.Should().Be(1000);
            info.FingerprintsCount.Should().Be(100);
            info.RecordsCount.Should().Be(500);
        }
    }

    /// <summary>
    /// Tests for ZKUser model
    /// </summary>
    public class ZKUserTests
    {
        [Fact]
        public void ZKUser_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""uid"": 1,
                ""name"": ""John Doe"",
                ""privilege"": 0,
                ""privilege_name"": ""User"",
                ""password"": ""1234"",
                ""group_id"": ""1"",
                ""user_id"": ""001"",
                ""card"": 12345
            }";

            // Act
            var user = JsonSerializer.Deserialize<ZKUser>(json);

            // Assert
            user.Should().NotBeNull();
            user!.Uid.Should().Be(1);
            user.Name.Should().Be("John Doe");
            user.Privilege.Should().Be(0);
            user.PrivilegeName.Should().Be("User");
            user.UserId.Should().Be("001");
            user.Card.Should().Be(12345);
        }

        [Fact]
        public void ZKUser_ShouldHandleAdminPrivilege()
        {
            // Arrange
            var json = @"{
                ""uid"": 2,
                ""name"": ""Admin User"",
                ""privilege"": 14,
                ""privilege_name"": ""Admin"",
                ""password"": """",
                ""group_id"": """",
                ""user_id"": ""002"",
                ""card"": 0
            }";

            // Act
            var user = JsonSerializer.Deserialize<ZKUser>(json);

            // Assert
            user.Should().NotBeNull();
            user!.Privilege.Should().Be(14);
            user.PrivilegeName.Should().Be("Admin");
        }
    }

    /// <summary>
    /// Tests for UsersResponse model
    /// </summary>
    public class UsersResponseTests
    {
        [Fact]
        public void UsersResponse_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""count"": 2,
                ""users"": [
                    {
                        ""uid"": 1,
                        ""name"": ""John Doe"",
                        ""privilege"": 0,
                        ""privilege_name"": ""User"",
                        ""password"": ""1234"",
                        ""group_id"": ""1"",
                        ""user_id"": ""001"",
                        ""card"": 12345
                    },
                    {
                        ""uid"": 2,
                        ""name"": ""Jane Admin"",
                        ""privilege"": 14,
                        ""privilege_name"": ""Admin"",
                        ""password"": ""5678"",
                        ""group_id"": ""1"",
                        ""user_id"": ""002"",
                        ""card"": 67890
                    }
                ]
            }";

            // Act
            var response = JsonSerializer.Deserialize<UsersResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Count.Should().Be(2);
            response.Users.Should().HaveCount(2);
            response.Users[0].Name.Should().Be("John Doe");
            response.Users[1].Name.Should().Be("Jane Admin");
        }

        [Fact]
        public void UsersResponse_ShouldHandleEmptyUserList()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""count"": 0,
                ""users"": []
            }";

            // Act
            var response = JsonSerializer.Deserialize<UsersResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Count.Should().Be(0);
            response.Users.Should().BeEmpty();
        }
    }

    /// <summary>
    /// Tests for Attendance model
    /// </summary>
    public class AttendanceTests
    {
        [Fact]
        public void Attendance_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""uid"": 1,
                ""user_id"": ""001"",
                ""timestamp"": ""2025-10-28T09:00:00"",
                ""status"": 0,
                ""punch"": 0
            }";

            // Act
            var attendance = JsonSerializer.Deserialize<Attendance>(json);

            // Assert
            attendance.Should().NotBeNull();
            attendance!.Uid.Should().Be(1);
            attendance.UserId.Should().Be("001");
            attendance.Timestamp.Should().Be("2025-10-28T09:00:00");
            attendance.Status.Should().Be(0);
            attendance.Punch.Should().Be(0);
        }

        [Fact]
        public void Attendance_GetTimestamp_ShouldParseValidTimestamp()
        {
            // Arrange
            var attendance = new Attendance
            {
                Timestamp = "2025-10-28T09:00:00"
            };

            // Act
            var parsedTime = attendance.GetTimestamp();

            // Assert
            parsedTime.Should().NotBeNull();
            parsedTime!.Value.Year.Should().Be(2025);
            parsedTime.Value.Month.Should().Be(10);
            parsedTime.Value.Day.Should().Be(28);
            parsedTime.Value.Hour.Should().Be(9);
        }

        [Fact]
        public void Attendance_GetTimestamp_ShouldReturnNullForInvalidTimestamp()
        {
            // Arrange
            var attendance = new Attendance
            {
                Timestamp = "invalid-timestamp"
            };

            // Act
            var parsedTime = attendance.GetTimestamp();

            // Assert
            parsedTime.Should().BeNull();
        }

        [Fact]
        public void Attendance_GetTimestamp_ShouldReturnNullForEmptyTimestamp()
        {
            // Arrange
            var attendance = new Attendance
            {
                Timestamp = null
            };

            // Act
            var parsedTime = attendance.GetTimestamp();

            // Assert
            parsedTime.Should().BeNull();
        }
    }

    /// <summary>
    /// Tests for AttendanceResponse model
    /// </summary>
    public class AttendanceResponseTests
    {
        [Fact]
        public void AttendanceResponse_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""count"": 2,
                ""attendances"": [
                    {
                        ""uid"": 1,
                        ""user_id"": ""001"",
                        ""timestamp"": ""2025-10-28T09:00:00"",
                        ""status"": 0,
                        ""punch"": 0
                    },
                    {
                        ""uid"": 2,
                        ""user_id"": ""002"",
                        ""timestamp"": ""2025-10-28T17:30:00"",
                        ""status"": 1,
                        ""punch"": 1
                    }
                ]
            }";

            // Act
            var response = JsonSerializer.Deserialize<AttendanceResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Count.Should().Be(2);
            response.Attendances.Should().HaveCount(2);
            response.Attendances[0].UserId.Should().Be("001");
            response.Attendances[1].UserId.Should().Be("002");
        }
    }

    /// <summary>
    /// Tests for OperationResponse model
    /// </summary>
    public class OperationResponseTests
    {
        [Fact]
        public void OperationResponse_ShouldDeserializeSuccessResponse()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""message"": ""Operation completed successfully""
            }";

            // Act
            var response = JsonSerializer.Deserialize<OperationResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Message.Should().Be("Operation completed successfully");
            response.Error.Should().BeNull();
        }

        [Fact]
        public void OperationResponse_ShouldDeserializeErrorResponse()
        {
            // Arrange
            var json = @"{
                ""success"": false,
                ""message"": ""Operation failed"",
                ""error"": ""Device not responding""
            }";

            // Act
            var response = JsonSerializer.Deserialize<OperationResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Operation failed");
            response.Error.Should().Be("Device not responding");
        }
    }

    /// <summary>
    /// Tests for Template model
    /// </summary>
    public class TemplateTests
    {
        [Fact]
        public void Template_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""uid"": 1,
                ""fid"": 0,
                ""valid"": 1,
                ""template_size"": 512
            }";

            // Act
            var template = JsonSerializer.Deserialize<Template>(json);

            // Assert
            template.Should().NotBeNull();
            template!.Uid.Should().Be(1);
            template.Fid.Should().Be(0);
            template.Valid.Should().Be(1);
            template.TemplateSize.Should().Be(512);
        }
    }

    /// <summary>
    /// Tests for TemplatesResponse model
    /// </summary>
    public class TemplatesResponseTests
    {
        [Fact]
        public void TemplatesResponse_ShouldDeserializeFromJson()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""count"": 2,
                ""templates"": [
                    {
                        ""uid"": 1,
                        ""fid"": 0,
                        ""valid"": 1,
                        ""template_size"": 512
                    },
                    {
                        ""uid"": 2,
                        ""fid"": 1,
                        ""valid"": 1,
                        ""template_size"": 512
                    }
                ]
            }";

            // Act
            var response = JsonSerializer.Deserialize<TemplatesResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Count.Should().Be(2);
            response.Templates.Should().HaveCount(2);
            response.Templates[0].Uid.Should().Be(1);
            response.Templates[1].Uid.Should().Be(2);
        }
    }

    /// <summary>
    /// Tests for various response models
    /// </summary>
    public class ResponseModelsTests
    {
        [Fact]
        public void LockStateResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""locked"": true
            }";

            // Act
            var response = JsonSerializer.Deserialize<LockStateResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Locked.Should().BeTrue();
        }

        [Fact]
        public void FaceVersionResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""face_version"": ""10.0""
            }";

            // Act
            var response = JsonSerializer.Deserialize<FaceVersionResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.FaceVersion.Should().Be("10.0");
        }

        [Fact]
        public void FingerprintVersionResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""fingerprint_version"": ""9.5""
            }";

            // Act
            var response = JsonSerializer.Deserialize<FingerprintVersionResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.FingerprintVersion.Should().Be("9.5");
        }

        [Fact]
        public void NetworkParamsResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""network"": ""192.168.1.100""
            }";

            // Act
            var response = JsonSerializer.Deserialize<NetworkParamsResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Network.Should().Be("192.168.1.100");
        }

        [Fact]
        public void PinWidthResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""pin_width"": 4
            }";

            // Act
            var response = JsonSerializer.Deserialize<PinWidthResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.PinWidth.Should().Be(4);
        }

        [Fact]
        public void ExtendFormatResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""extend_format"": 1
            }";

            // Act
            var response = JsonSerializer.Deserialize<ExtendFormatResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.ExtendFormat.Should().Be(1);
        }

        [Fact]
        public void UserExtendFormatResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""user_extend_format"": 0
            }";

            // Act
            var response = JsonSerializer.Deserialize<UserExtendFormatResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.UserExtendFormat.Should().Be(0);
        }

        [Fact]
        public void FaceFunctionResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""face_function_enabled"": true
            }";

            // Act
            var response = JsonSerializer.Deserialize<FaceFunctionResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.FaceFunctionEnabled.Should().BeTrue();
        }

        [Fact]
        public void FirmwareCompatibilityResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""old_firmware_compatible"": true
            }";

            // Act
            var response = JsonSerializer.Deserialize<FirmwareCompatibilityResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.OldFirmwareCompatible.Should().BeTrue();
        }

        [Fact]
        public void CancelCaptureResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""cancelled"": true
            }";

            // Act
            var response = JsonSerializer.Deserialize<CancelCaptureResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Cancelled.Should().BeTrue();
        }

        [Fact]
        public void TemplateResponse_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""template"": {
                    ""uid"": 1,
                    ""fid"": 0,
                    ""valid"": 1,
                    ""template_size"": 512
                }
            }";

            // Act
            var response = JsonSerializer.Deserialize<TemplateResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Template.Should().NotBeNull();
            response.Template!.Uid.Should().Be(1);
        }
    }

    /// <summary>
    /// Tests for PyZKClient privilege constants
    /// </summary>
    public class PyZKClientPrivilegeTests
    {
        [Fact]
        public void Privilege_ShouldHaveCorrectValues()
        {
            // Assert
            PyZKClient.Privilege.User.Should().Be(0);
            PyZKClient.Privilege.Admin.Should().Be(14);
        }
    }

    /// <summary>
    /// Tests for error handling in responses
    /// </summary>
    public class ErrorHandlingTests
    {
        [Theory]
        [InlineData(@"{""success"": false, ""error"": ""Not connected to device""}")]
        [InlineData(@"{""success"": false, ""error"": ""Connection timeout""}")]
        [InlineData(@"{""success"": false, ""error"": ""Device not responding""}")]
        public void Responses_ShouldHandleErrorMessages(string json)
        {
            // Act
            var response = JsonSerializer.Deserialize<OperationResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void UsersResponse_ShouldHandleError()
        {
            // Arrange
            var json = @"{
                ""success"": false,
                ""error"": ""Failed to retrieve users""
            }";

            // Act
            var response = JsonSerializer.Deserialize<UsersResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Error.Should().Be("Failed to retrieve users");
        }

        [Fact]
        public void AttendanceResponse_ShouldHandleError()
        {
            // Arrange
            var json = @"{
                ""success"": false,
                ""error"": ""Failed to retrieve attendance records""
            }";

            // Act
            var response = JsonSerializer.Deserialize<AttendanceResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Error.Should().Be("Failed to retrieve attendance records");
        }

        [Fact]
        public void TemplatesResponse_ShouldHandleError()
        {
            // Arrange
            var json = @"{
                ""success"": false,
                ""error"": ""Failed to retrieve templates""
            }";

            // Act
            var response = JsonSerializer.Deserialize<TemplatesResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Error.Should().Be("Failed to retrieve templates");
        }
    }

    /// <summary>
    /// Tests for JSON serialization edge cases
    /// </summary>
    public class JsonSerializationTests
    {
        [Fact]
        public void Models_ShouldHandleNullValues()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""message"": null,
                ""error"": null
            }";

            // Act
            var response = JsonSerializer.Deserialize<OperationResponse>(json);

            // Assert
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Message.Should().BeNull();
            response.Error.Should().BeNull();
        }

        [Fact]
        public void ZKUser_ShouldHandleDefaultValues()
        {
            // Arrange
            var json = @"{
                ""uid"": 0,
                ""name"": """",
                ""privilege"": 0,
                ""privilege_name"": """",
                ""password"": """",
                ""group_id"": """",
                ""user_id"": """",
                ""card"": 0
            }";

            // Act
            var user = JsonSerializer.Deserialize<ZKUser>(json);

            // Assert
            user.Should().NotBeNull();
            user!.Uid.Should().Be(0);
            user.Name.Should().Be(string.Empty);
            user.Card.Should().Be(0);
        }

        [Fact]
        public void Attendance_ShouldHandleNullTimestamp()
        {
            // Arrange
            var json = @"{
                ""uid"": 1,
                ""user_id"": ""001"",
                ""timestamp"": null,
                ""status"": 0,
                ""punch"": 0
            }";

            // Act
            var attendance = JsonSerializer.Deserialize<Attendance>(json);

            // Assert
            attendance.Should().NotBeNull();
            attendance!.Timestamp.Should().BeNull();
            attendance.GetTimestamp().Should().BeNull();
        }

        [Fact]
        public void DetailedDeviceInfo_ShouldHandleMissingOptionalFields()
        {
            // Arrange
            var json = @"{
                ""success"": true,
                ""firmware_version"": ""Ver 6.60"",
                ""users_count"": 0,
                ""users_capacity"": 0,
                ""fingerprints_count"": 0,
                ""fingerprints_capacity"": 0,
                ""records_count"": 0
            }";

            // Act
            var info = JsonSerializer.Deserialize<DetailedDeviceInfo>(json);

            // Assert
            info.Should().NotBeNull();
            info!.Success.Should().BeTrue();
            info.SerialNumber.Should().BeNull();
            info.Platform.Should().BeNull();
            info.DeviceName.Should().BeNull();
        }
    }

    /// <summary>
    /// Integration-style tests for complete workflows
    /// </summary>
    public class WorkflowTests
    {
        [Fact]
        public void CompleteUserWorkflow_ShouldDeserializeCorrectly()
        {
            // This tests a complete workflow: get users, add user, delete user
            
            // Get users
            var getUsersJson = @"{
                ""success"": true,
                ""count"": 1,
                ""users"": [
                    {
                        ""uid"": 1,
                        ""name"": ""John Doe"",
                        ""privilege"": 0,
                        ""privilege_name"": ""User"",
                        ""password"": ""1234"",
                        ""group_id"": ""1"",
                        ""user_id"": ""001"",
                        ""card"": 12345
                    }
                ]
            }";
            
            var usersResponse = JsonSerializer.Deserialize<UsersResponse>(getUsersJson);
            usersResponse!.Success.Should().BeTrue();
            usersResponse.Count.Should().Be(1);
            
            // Add user
            var addUserJson = @"{
                ""success"": true,
                ""message"": ""User Test User (UID: 2) added successfully""
            }";
            
            var addResponse = JsonSerializer.Deserialize<OperationResponse>(addUserJson);
            addResponse!.Success.Should().BeTrue();
            
            // Delete user
            var deleteUserJson = @"{
                ""success"": true,
                ""message"": ""User deleted successfully""
            }";
            
            var deleteResponse = JsonSerializer.Deserialize<OperationResponse>(deleteUserJson);
            deleteResponse!.Success.Should().BeTrue();
        }

        [Fact]
        public void CompleteAttendanceWorkflow_ShouldDeserializeCorrectly()
        {
            // Get attendance
            var getAttendanceJson = @"{
                ""success"": true,
                ""count"": 1,
                ""attendances"": [
                    {
                        ""uid"": 1,
                        ""user_id"": ""001"",
                        ""timestamp"": ""2025-10-28T09:00:00"",
                        ""status"": 0,
                        ""punch"": 0
                    }
                ]
            }";
            
            var attendanceResponse = JsonSerializer.Deserialize<AttendanceResponse>(getAttendanceJson);
            attendanceResponse!.Success.Should().BeTrue();
            attendanceResponse.Count.Should().Be(1);
            
            // Clear attendance
            var clearAttendanceJson = @"{
                ""success"": true,
                ""message"": ""Attendance records cleared successfully""
            }";
            
            var clearResponse = JsonSerializer.Deserialize<OperationResponse>(clearAttendanceJson);
            clearResponse!.Success.Should().BeTrue();
        }

        [Fact]
        public void CompleteTemplateWorkflow_ShouldDeserializeCorrectly()
        {
            // Get templates
            var getTemplatesJson = @"{
                ""success"": true,
                ""count"": 1,
                ""templates"": [
                    {
                        ""uid"": 1,
                        ""fid"": 0,
                        ""valid"": 1,
                        ""template_size"": 512
                    }
                ]
            }";
            
            var templatesResponse = JsonSerializer.Deserialize<TemplatesResponse>(getTemplatesJson);
            templatesResponse!.Success.Should().BeTrue();
            templatesResponse.Count.Should().Be(1);
            
            // Get specific template
            var getTemplateJson = @"{
                ""success"": true,
                ""template"": {
                    ""uid"": 1,
                    ""fid"": 0,
                    ""valid"": 1,
                    ""template_size"": 512
                }
            }";
            
            var templateResponse = JsonSerializer.Deserialize<TemplateResponse>(getTemplateJson);
            templateResponse!.Success.Should().BeTrue();
            templateResponse.Template.Should().NotBeNull();
            
            // Delete template
            var deleteTemplateJson = @"{
                ""success"": true,
                ""message"": ""Template deleted successfully""
            }";
            
            var deleteResponse = JsonSerializer.Deserialize<OperationResponse>(deleteTemplateJson);
            deleteResponse!.Success.Should().BeTrue();
        }
    }
}
