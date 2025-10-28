# -*- coding: utf-8 -*-
"""
PyZK Wrapper Tests
==================
Comprehensive tests for the PyZKWrapper class.
Tests use mocking to avoid requiring a real device.
"""

import unittest
import json
from unittest.mock import Mock, MagicMock, patch, PropertyMock
from datetime import datetime
import sys
import os

# Add parent directory to path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from dotnet_integration.pyzk_wrapper import PyZKWrapper
from zk import const
from zk.user import User
from zk.attendance import Attendance
from zk.finger import Finger


class TestPyZKWrapperInit(unittest.TestCase):
    """Test initialization and connection management."""
    
    def test_init_default_parameters(self):
        """Test wrapper initialization with default parameters."""
        wrapper = PyZKWrapper("192.168.1.100")
        
        self.assertEqual(wrapper.ip_address, "192.168.1.100")
        self.assertEqual(wrapper.port, 4370)
        self.assertEqual(wrapper.timeout, 5)
        self.assertEqual(wrapper.password, 0)
        self.assertFalse(wrapper.force_udp)
        self.assertFalse(wrapper.ommit_ping)
        self.assertIsNone(wrapper.conn)
        self.assertFalse(wrapper.is_connected)
    
    def test_init_custom_parameters(self):
        """Test wrapper initialization with custom parameters."""
        wrapper = PyZKWrapper(
            "10.0.0.1",
            port=8000,
            timeout=10,
            password=12345,
            force_udp=True,
            ommit_ping=True
        )
        
        self.assertEqual(wrapper.ip_address, "10.0.0.1")
        self.assertEqual(wrapper.port, 8000)
        self.assertEqual(wrapper.timeout, 10)
        self.assertEqual(wrapper.password, 12345)
        self.assertTrue(wrapper.force_udp)
        self.assertTrue(wrapper.ommit_ping)
    
    @patch('dotnet_integration.pyzk_wrapper.ZK')
    def test_connect_success(self, mock_zk_class):
        """Test successful connection to device."""
        # Setup mock
        mock_zk_instance = Mock()
        mock_conn = Mock()
        mock_conn.get_firmware_version.return_value = "Ver 6.60"
        mock_conn.get_serialnumber.return_value = "ABC123456"
        mock_conn.get_platform.return_value = "ZEM560"
        mock_conn.get_device_name.return_value = "TestDevice"
        
        mock_zk_instance.connect.return_value = mock_conn
        mock_zk_class.return_value = mock_zk_instance
        
        # Test connection
        wrapper = PyZKWrapper("192.168.1.100")
        result = wrapper.connect()
        
        # Verify
        self.assertTrue(result["success"])
        self.assertEqual(result["message"], "Connected successfully")
        self.assertTrue(wrapper.is_connected)
        self.assertEqual(result["device_info"]["firmware_version"], "Ver 6.60")
        self.assertEqual(result["device_info"]["serial_number"], "ABC123456")
        mock_zk_instance.connect.assert_called_once()
    
    @patch('dotnet_integration.pyzk_wrapper.ZK')
    def test_connect_failure(self, mock_zk_class):
        """Test failed connection to device."""
        # Setup mock to raise exception
        mock_zk_instance = Mock()
        mock_zk_instance.connect.side_effect = Exception("Connection timeout")
        mock_zk_class.return_value = mock_zk_instance
        
        # Test connection
        wrapper = PyZKWrapper("192.168.1.100")
        result = wrapper.connect()
        
        # Verify
        self.assertFalse(result["success"])
        self.assertIn("Connection failed", result["message"])
        self.assertFalse(wrapper.is_connected)
        self.assertEqual(result["error"], "Connection timeout")
    
    def test_disconnect_success(self):
        """Test successful disconnection."""
        wrapper = PyZKWrapper("192.168.1.100")
        wrapper.conn = Mock()
        wrapper._is_connected = True
        
        result = wrapper.disconnect()
        
        self.assertTrue(result["success"])
        self.assertFalse(wrapper.is_connected)
        wrapper.conn.disconnect.assert_called_once()
    
    def test_disconnect_with_exception(self):
        """Test disconnection with exception."""
        wrapper = PyZKWrapper("192.168.1.100")
        wrapper.conn = Mock()
        wrapper.conn.disconnect.side_effect = Exception("Disconnect error")
        wrapper._is_connected = True
        
        result = wrapper.disconnect()
        
        self.assertFalse(result["success"])
        self.assertIn("Disconnect error", result["message"])


class TestPyZKWrapperUsers(unittest.TestCase):
    """Test user management methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_get_users_json_success(self):
        """Test getting users list as JSON."""
        # Create mock users
        user1 = User(1, "John Doe", const.USER_DEFAULT, "1234", "1", "001", 12345)
        user2 = User(2, "Jane Admin", const.USER_ADMIN, "5678", "1", "002", 67890)
        
        self.wrapper.conn.get_users.return_value = [user1, user2]
        
        result_json = self.wrapper.get_users_json()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["count"], 2)
        self.assertEqual(result["users"][0]["name"], "John Doe")
        self.assertEqual(result["users"][0]["privilege_name"], "User")
        self.assertEqual(result["users"][1]["name"], "Jane Admin")
        self.assertEqual(result["users"][1]["privilege_name"], "Admin")
        
        self.wrapper.conn.disable_device.assert_called_once()
        self.wrapper.conn.enable_device.assert_called_once()
    
    def test_get_users_json_not_connected(self):
        """Test getting users when not connected."""
        self.wrapper._is_connected = False
        
        result_json = self.wrapper.get_users_json()
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Not connected", result["error"])
    
    def test_get_users_json_exception(self):
        """Test getting users with exception."""
        self.wrapper.conn.get_users.side_effect = Exception("Database error")
        
        result_json = self.wrapper.get_users_json()
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Database error", result["error"])
        self.wrapper.conn.enable_device.assert_called_once()
    
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
        self.assertIn("10", result["message"])
        
        self.wrapper.conn.disable_device.assert_called_once()
        self.wrapper.conn.set_user.assert_called_once()
        self.wrapper.conn.enable_device.assert_called_once()
    
    def test_add_user_not_connected(self):
        """Test adding user when not connected."""
        self.wrapper._is_connected = False
        
        result_json = self.wrapper.add_user(uid=10, name="Test User")
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Not connected", result["error"])
    
    def test_add_user_exception(self):
        """Test adding user with exception."""
        self.wrapper.conn.set_user.side_effect = Exception("Add user failed")
        
        result_json = self.wrapper.add_user(uid=10, name="Test User")
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Add user failed", result["error"])
        self.wrapper.conn.enable_device.assert_called_once()
    
    def test_delete_user_by_uid(self):
        """Test deleting user by UID."""
        result_json = self.wrapper.delete_user(uid=10)
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("deleted successfully", result["message"])
        
        self.wrapper.conn.delete_user.assert_called_once_with(uid=10)
    
    def test_delete_user_by_user_id(self):
        """Test deleting user by user_id."""
        result_json = self.wrapper.delete_user(user_id="010")
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.delete_user.assert_called_once_with(user_id="010")
    
    def test_delete_user_no_identifier(self):
        """Test deleting user without uid or user_id."""
        result_json = self.wrapper.delete_user()
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("uid or user_id must be provided", result["error"])


class TestPyZKWrapperAttendance(unittest.TestCase):
    """Test attendance management methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_get_attendance_json_success(self):
        """Test getting attendance records as JSON."""
        # Create mock attendance records
        # Attendance(user_id, timestamp, status, punch=0, uid=0)
        att1 = Attendance("001", datetime(2025, 10, 28, 9, 0, 0), 0, punch=0, uid=1)
        att2 = Attendance("002", datetime(2025, 10, 28, 17, 30, 0), 1, punch=1, uid=2)
        
        self.wrapper.conn.get_attendance.return_value = [att1, att2]
        
        result_json = self.wrapper.get_attendance_json()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["count"], 2)
        self.assertEqual(result["attendances"][0]["user_id"], "001")
        self.assertEqual(result["attendances"][1]["user_id"], "002")
        
        self.wrapper.conn.disable_device.assert_called_once()
        self.wrapper.conn.enable_device.assert_called_once()
    
    def test_get_attendance_json_with_null_timestamp(self):
        """Test getting attendance with null timestamp."""
        # Attendance(user_id, timestamp, status, punch=0, uid=0)
        att = Attendance("001", None, 0, punch=0, uid=1)
        self.wrapper.conn.get_attendance.return_value = [att]
        
        result_json = self.wrapper.get_attendance_json()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIsNone(result["attendances"][0]["timestamp"])
    
    def test_get_attendance_json_not_connected(self):
        """Test getting attendance when not connected."""
        self.wrapper._is_connected = False
        
        result_json = self.wrapper.get_attendance_json()
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Not connected", result["error"])
    
    def test_clear_attendance_success(self):
        """Test clearing attendance records."""
        result_json = self.wrapper.clear_attendance()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("cleared successfully", result["message"])
        
        self.wrapper.conn.disable_device.assert_called_once()
        self.wrapper.conn.clear_attendance.assert_called_once()
        self.wrapper.conn.enable_device.assert_called_once()
    
    def test_clear_attendance_exception(self):
        """Test clearing attendance with exception."""
        self.wrapper.conn.clear_attendance.side_effect = Exception("Clear failed")
        
        result_json = self.wrapper.clear_attendance()
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Clear failed", result["error"])
        self.wrapper.conn.enable_device.assert_called_once()


class TestPyZKWrapperDeviceInfo(unittest.TestCase):
    """Test device information methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_get_device_info_json_success(self):
        """Test getting device information."""
        self.wrapper.conn.get_firmware_version.return_value = "Ver 6.60"
        self.wrapper.conn.get_serialnumber.return_value = "ABC123"
        self.wrapper.conn.get_platform.return_value = "ZEM560"
        self.wrapper.conn.get_device_name.return_value = "MainGate"
        self.wrapper.conn.get_mac.return_value = "00:11:22:33:44:55"
        self.wrapper.conn.get_time.return_value = datetime(2025, 10, 28, 10, 30, 0)
        self.wrapper.conn.users = 50
        self.wrapper.conn.users_cap = 1000
        self.wrapper.conn.fingers = 100
        self.wrapper.conn.fingers_cap = 2000
        self.wrapper.conn.records = 500
        
        result_json = self.wrapper.get_device_info_json()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["firmware_version"], "Ver 6.60")
        self.assertEqual(result["serial_number"], "ABC123")
        self.assertEqual(result["platform"], "ZEM560")
        self.assertEqual(result["device_name"], "MainGate")
        self.assertEqual(result["mac_address"], "00:11:22:33:44:55")
        self.assertEqual(result["users_count"], 50)
        self.assertEqual(result["users_capacity"], 1000)
        
        self.wrapper.conn.read_sizes.assert_called_once()
    
    def test_get_device_info_json_not_connected(self):
        """Test getting device info when not connected."""
        self.wrapper._is_connected = False
        
        result_json = self.wrapper.get_device_info_json()
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Not connected", result["error"])


class TestPyZKWrapperTimeManagement(unittest.TestCase):
    """Test time management methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_set_time_with_timestamp(self):
        """Test setting device time with specific timestamp."""
        timestamp = "2025-10-28T10:30:00"
        
        result_json = self.wrapper.set_time(timestamp)
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("2025-10-28", result["message"])
        self.wrapper.conn.set_time.assert_called_once()
    
    def test_set_time_without_timestamp(self):
        """Test setting device time to current time."""
        result_json = self.wrapper.set_time()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.set_time.assert_called_once()
    
    def test_set_time_invalid_timestamp(self):
        """Test setting time with invalid timestamp."""
        result_json = self.wrapper.set_time("invalid-timestamp")
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("error", result)
    
    def test_set_time_not_connected(self):
        """Test setting time when not connected."""
        self.wrapper._is_connected = False
        
        result_json = self.wrapper.set_time()
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("Not connected", result["error"])


class TestPyZKWrapperDeviceControl(unittest.TestCase):
    """Test device control methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_test_voice_default_index(self):
        """Test playing voice with default index."""
        result_json = self.wrapper.test_voice()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.test_voice.assert_called_once_with(index=0)
    
    def test_test_voice_custom_index(self):
        """Test playing voice with custom index."""
        result_json = self.wrapper.test_voice(5)
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.test_voice.assert_called_once_with(index=5)
    
    def test_restart_device(self):
        """Test restarting device."""
        result_json = self.wrapper.restart_device()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("restart", result["message"])
        self.wrapper.conn.restart.assert_called_once()
    
    def test_poweroff_device(self):
        """Test powering off device."""
        result_json = self.wrapper.poweroff_device()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("poweroff", result["message"])
        self.wrapper.conn.poweroff.assert_called_once()
    
    def test_enable_device(self):
        """Test enabling device."""
        result_json = self.wrapper.enable_device()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.enable_device.assert_called_once()
    
    def test_disable_device(self):
        """Test disabling device."""
        result_json = self.wrapper.disable_device()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.disable_device.assert_called_once()
    
    def test_refresh_data(self):
        """Test refreshing device data."""
        result_json = self.wrapper.refresh_data()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.refresh_data.assert_called_once()
    
    def test_free_data(self):
        """Test freeing device buffer."""
        result_json = self.wrapper.free_data()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.wrapper.conn.free_data.assert_called_once()
    
    def test_clear_data(self):
        """Test clearing all device data."""
        result_json = self.wrapper.clear_data()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("All data cleared", result["message"])
        self.wrapper.conn.disable_device.assert_called_once()
        self.wrapper.conn.clear_data.assert_called_once()
        self.wrapper.conn.enable_device.assert_called_once()


class TestPyZKWrapperDoorControl(unittest.TestCase):
    """Test door control methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_unlock_door_default_time(self):
        """Test unlocking door with default time."""
        result_json = self.wrapper.unlock_door()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("3 seconds", result["message"])
        self.wrapper.conn.unlock.assert_called_once_with(time=3)
    
    def test_unlock_door_custom_time(self):
        """Test unlocking door with custom time."""
        result_json = self.wrapper.unlock_door(10)
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("10 seconds", result["message"])
        self.wrapper.conn.unlock.assert_called_once_with(time=10)
    
    def test_get_lock_state_locked(self):
        """Test getting lock state when locked."""
        self.wrapper.conn.get_lock_state.return_value = True
        
        result_json = self.wrapper.get_lock_state()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertTrue(result["locked"])
    
    def test_get_lock_state_unlocked(self):
        """Test getting lock state when unlocked."""
        self.wrapper.conn.get_lock_state.return_value = False
        
        result_json = self.wrapper.get_lock_state()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertFalse(result["locked"])


class TestPyZKWrapperLCDControl(unittest.TestCase):
    """Test LCD control methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_write_lcd(self):
        """Test writing text to LCD."""
        result_json = self.wrapper.write_lcd(0, "Hello World")
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("line 0", result["message"])
        self.wrapper.conn.write_lcd.assert_called_once_with(0, "Hello World")
    
    def test_clear_lcd(self):
        """Test clearing LCD."""
        result_json = self.wrapper.clear_lcd()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("cleared", result["message"])
        self.wrapper.conn.clear_lcd.assert_called_once()


class TestPyZKWrapperVersionInfo(unittest.TestCase):
    """Test version information methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_get_face_version(self):
        """Test getting face algorithm version."""
        self.wrapper.conn.get_face_version.return_value = "10.0"
        
        result_json = self.wrapper.get_face_version()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["face_version"], "10.0")
    
    def test_get_fp_version(self):
        """Test getting fingerprint algorithm version."""
        self.wrapper.conn.get_fp_version.return_value = "9.5"
        
        result_json = self.wrapper.get_fp_version()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["fingerprint_version"], "9.5")
    
    def test_get_face_fun_on(self):
        """Test checking if face function is enabled."""
        self.wrapper.conn.get_face_fun_on.return_value = 1
        
        result_json = self.wrapper.get_face_fun_on()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertTrue(result["face_function_enabled"])
    
    def test_get_compat_old_firmware(self):
        """Test checking firmware compatibility."""
        self.wrapper.conn.get_compat_old_firmware.return_value = True
        
        result_json = self.wrapper.get_compat_old_firmware()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertTrue(result["old_firmware_compatible"])


class TestPyZKWrapperConfiguration(unittest.TestCase):
    """Test configuration methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_get_network_params(self):
        """Test getting network parameters."""
        self.wrapper.conn.get_network_params.return_value = {
            "ip": "192.168.1.100",
            "mask": "255.255.255.0",
            "gateway": "192.168.1.1"
        }
        
        result_json = self.wrapper.get_network_params()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["network"]["ip"], "192.168.1.100")
    
    def test_get_pin_width(self):
        """Test getting PIN width."""
        self.wrapper.conn.get_pin_width.return_value = 4
        
        result_json = self.wrapper.get_pin_width()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["pin_width"], 4)
    
    def test_get_extend_fmt(self):
        """Test getting extend format."""
        self.wrapper.conn.get_extend_fmt.return_value = 1
        
        result_json = self.wrapper.get_extend_fmt()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["extend_format"], 1)
    
    def test_get_user_extend_fmt(self):
        """Test getting user extend format."""
        self.wrapper.conn.get_user_extend_fmt.return_value = 0
        
        result_json = self.wrapper.get_user_extend_fmt()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["user_extend_format"], 0)
    
    def test_set_sdk_build_1(self):
        """Test setting SDK build."""
        self.wrapper.conn.set_sdk_build_1.return_value = True
        
        result_json = self.wrapper.set_sdk_build_1()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("SDK build set", result["message"])


class TestPyZKWrapperTemplates(unittest.TestCase):
    """Test fingerprint template methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_get_templates_json_success(self):
        """Test getting all templates."""
        # Finger(uid, fid, valid, template)
        template1 = Finger(1, 0, 1, b'\x00' * 50)
        template2 = Finger(2, 1, 1, b'\x00' * 50)
        
        self.wrapper.conn.get_templates.return_value = [template1, template2]
        
        result_json = self.wrapper.get_templates_json()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["count"], 2)
        self.assertEqual(result["templates"][0]["uid"], 1)
        self.assertEqual(result["templates"][0]["fid"], 0)
        self.assertEqual(result["templates"][0]["template_size"], 50)
        
        self.wrapper.conn.disable_device.assert_called_once()
        self.wrapper.conn.enable_device.assert_called_once()
    
    def test_get_user_template_json_found(self):
        """Test getting specific user template."""
        # Finger(uid, fid, valid, template)
        template = Finger(1, 0, 1, b'\x00' * 50)
        self.wrapper.conn.get_user_template.return_value = template
        
        result_json = self.wrapper.get_user_template_json(uid=1, temp_id=0)
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertEqual(result["template"]["uid"], 1)
        self.assertEqual(result["template"]["fid"], 0)
    
    def test_get_user_template_json_not_found(self):
        """Test getting non-existent template."""
        self.wrapper.conn.get_user_template.return_value = None
        
        result_json = self.wrapper.get_user_template_json(uid=1, temp_id=0)
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("not found", result["error"])
    
    def test_delete_user_template_success(self):
        """Test deleting user template."""
        self.wrapper.conn.delete_user_template.return_value = True
        
        result_json = self.wrapper.delete_user_template(uid=1, temp_id=0)
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("deleted successfully", result["message"])
    
    def test_delete_user_template_not_found(self):
        """Test deleting non-existent template."""
        self.wrapper.conn.delete_user_template.return_value = False
        
        result_json = self.wrapper.delete_user_template(uid=1, temp_id=0)
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("not found", result["error"])


class TestPyZKWrapperEnrollment(unittest.TestCase):
    """Test enrollment and verification methods."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.wrapper = PyZKWrapper("192.168.1.100")
        self.wrapper.conn = Mock()
        self.wrapper._is_connected = True
    
    def test_enroll_user_success(self):
        """Test successful user enrollment."""
        self.wrapper.conn.enroll_user.return_value = True
        
        result_json = self.wrapper.enroll_user(uid=1, temp_id=0, user_id="001")
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("completed successfully", result["message"])
    
    def test_enroll_user_failure(self):
        """Test failed user enrollment."""
        self.wrapper.conn.enroll_user.return_value = False
        
        result_json = self.wrapper.enroll_user(uid=1, temp_id=0)
        result = json.loads(result_json)
        
        self.assertFalse(result["success"])
        self.assertIn("failed", result["error"])
    
    def test_verify_user(self):
        """Test starting verification mode."""
        result_json = self.wrapper.verify_user()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("Verification mode", result["message"])
        self.wrapper.conn.verify_user.assert_called_once()
    
    def test_cancel_capture(self):
        """Test canceling capture operation."""
        self.wrapper.conn.cancel_capture.return_value = True
        
        result_json = self.wrapper.cancel_capture()
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertTrue(result["cancelled"])
    
    def test_reg_event(self):
        """Test registering for events."""
        result_json = self.wrapper.reg_event(0x01)
        result = json.loads(result_json)
        
        self.assertTrue(result["success"])
        self.assertIn("Registered for events", result["message"])
        self.wrapper.conn.reg_event.assert_called_once_with(0x01)


class TestPyZKWrapperErrorHandling(unittest.TestCase):
    """Test error handling across all methods."""
    
    def test_all_methods_check_connection(self):
        """Test that all methods check connection status."""
        wrapper = PyZKWrapper("192.168.1.100")
        wrapper._is_connected = False
        
        # Test a sample of methods
        methods_to_test = [
            (wrapper.get_users_json, []),
            (wrapper.get_attendance_json, []),
            (wrapper.add_user, [1, "Test"]),
            (wrapper.delete_user, [], {"uid": 1}),
            (wrapper.get_device_info_json, []),
            (wrapper.set_time, []),
            (wrapper.clear_attendance, []),
            (wrapper.test_voice, []),
            (wrapper.restart_device, []),
            (wrapper.unlock_door, []),
            (wrapper.write_lcd, [0, "Test"]),
            (wrapper.get_templates_json, []),
        ]
        
        for method, args, *kwargs_list in methods_to_test:
            kwargs = kwargs_list[0] if kwargs_list else {}
            result_json = method(*args, **kwargs)
            result = json.loads(result_json)
            
            self.assertFalse(result["success"], 
                           f"{method.__name__} should fail when not connected")
            self.assertIn("Not connected", result["error"],
                        f"{method.__name__} should return 'Not connected' error")


def run_tests():
    """Run all tests and print results."""
    # Create test suite
    loader = unittest.TestLoader()
    suite = unittest.TestSuite()
    
    # Add all test classes
    test_classes = [
        TestPyZKWrapperInit,
        TestPyZKWrapperUsers,
        TestPyZKWrapperAttendance,
        TestPyZKWrapperDeviceInfo,
        TestPyZKWrapperTimeManagement,
        TestPyZKWrapperDeviceControl,
        TestPyZKWrapperDoorControl,
        TestPyZKWrapperLCDControl,
        TestPyZKWrapperVersionInfo,
        TestPyZKWrapperConfiguration,
        TestPyZKWrapperTemplates,
        TestPyZKWrapperEnrollment,
        TestPyZKWrapperErrorHandling,
    ]
    
    for test_class in test_classes:
        tests = loader.loadTestsFromTestCase(test_class)
        suite.addTests(tests)
    
    # Run tests
    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(suite)
    
    # Print summary
    print("\n" + "="*70)
    print(f"Tests run: {result.testsRun}")
    print(f"Successes: {result.testsRun - len(result.failures) - len(result.errors)}")
    print(f"Failures: {len(result.failures)}")
    print(f"Errors: {len(result.errors)}")
    print("="*70)
    
    return result.wasSuccessful()


if __name__ == "__main__":
    success = run_tests()
    sys.exit(0 if success else 1)
