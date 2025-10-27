#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Test script for ZKTeco simulator

This script tests the ZKTeco simulator by connecting to it using the pyzk library.
Run the simulator first (zk_simulator.py), then run this test script.
"""

import sys
import os

# Add parent directory to path to import zk module
CWD = os.path.dirname(os.path.realpath(__file__))
ROOT_DIR = os.path.dirname(CWD)
sys.path.insert(0, ROOT_DIR)

from zk import ZK


def test_simulator(ip='127.0.0.1', port=4370):
    """Test the ZKTeco simulator"""
    
    print("=" * 70)
    print("ZKTeco Simulator Test")
    print("=" * 70)
    print(f"\nConnecting to simulator at {ip}:{port}...")
    
    conn = None
    zk = ZK(ip, port=port, timeout=5, ommit_ping=True)
    
    try:
        # Connect to device
        print("\n1. Testing connection...")
        conn = zk.connect()
        print("   ✓ Connected successfully!")
        
        # Get device information
        print("\n2. Testing device information retrieval...")
        
        try:
            firmware = conn.get_firmware_version()
            print(f"   - Firmware Version: {firmware}")
        except Exception as e:
            print(f"   ✗ Failed to get firmware: {e}")
        
        try:
            serial = conn.get_serialnumber()
            print(f"   - Serial Number: {serial}")
        except Exception as e:
            print(f"   ✗ Failed to get serial: {e}")
        
        try:
            platform = conn.get_platform()
            print(f"   - Platform: {platform}")
        except Exception as e:
            print(f"   ✗ Failed to get platform: {e}")
        
        try:
            device_name = conn.get_device_name()
            print(f"   - Device Name: {device_name}")
        except Exception as e:
            print(f"   ✗ Failed to get device name: {e}")
        
        try:
            mac = conn.get_mac()
            print(f"   - MAC Address: {mac}")
        except Exception as e:
            print(f"   ✗ Failed to get MAC: {e}")
        
        try:
            current_time = conn.get_time()
            print(f"   - Current Time: {current_time}")
        except Exception as e:
            print(f"   ✗ Failed to get time: {e}")
        
        # Get network parameters
        print("\n3. Testing network parameters...")
        try:
            network = conn.get_network_params()
            print(f"   - IP Address: {network.get('ip')}")
            print(f"   - Netmask: {network.get('mask')}")
            print(f"   - Gateway: {network.get('gateway')}")
        except Exception as e:
            print(f"   ✗ Failed to get network params: {e}")
        
        # Read sizes/capacity
        print("\n4. Testing capacity information...")
        try:
            conn.read_sizes()
            print(f"   - Users: {conn.users}/{conn.users_cap}")
            print(f"   - Fingers: {conn.fingers}/{conn.fingers_cap}")
            print(f"   - Records: {conn.records}/{conn.rec_cap}")
            print(f"   - Faces: {conn.faces}/{conn.faces_cap}")
        except Exception as e:
            print(f"   ✗ Failed to read sizes: {e}")
        
        # Get users
        print("\n5. Testing user retrieval...")
        try:
            users = conn.get_users()
            print(f"   ✓ Retrieved {len(users)} users:")
            for user in users:
                print(f"     - UID: {user.uid}, User ID: {user.user_id}, Name: {user.name}, "
                      f"Privilege: {user.privilege}, Card: {user.card}")
        except Exception as e:
            print(f"   ✗ Failed to get users: {e}")
        
        # Enable/Disable device
        print("\n6. Testing device enable/disable...")
        try:
            conn.disable_device()
            print("   ✓ Device disabled")
            conn.enable_device()
            print("   ✓ Device enabled")
        except Exception as e:
            print(f"   ✗ Failed to enable/disable: {e}")
        
        # Disconnect
        print("\n7. Testing disconnection...")
        conn.disconnect()
        print("   ✓ Disconnected successfully!")
        
        print("\n" + "=" * 70)
        print("All tests completed!")
        print("=" * 70)
        
        return True
        
    except Exception as e:
        print(f"\n✗ Error during testing: {e}")
        import traceback
        traceback.print_exc()
        return False
        
    finally:
        if conn:
            try:
                conn.disconnect()
            except:
                pass


def main():
    """Main entry point"""
    import argparse
    
    parser = argparse.ArgumentParser(description='Test ZKTeco Simulator')
    parser.add_argument('--ip', default='127.0.0.1', help='Simulator IP address (default: 127.0.0.1)')
    parser.add_argument('--port', type=int, default=4370, help='Simulator port (default: 4370)')
    
    args = parser.parse_args()
    
    success = test_simulator(args.ip, args.port)
    sys.exit(0 if success else 1)


if __name__ == '__main__':
    main()
