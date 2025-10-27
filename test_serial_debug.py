#!/usr/bin/env python3
import sys
import socket
import struct

sys.path.append("zk")
from zk.base import ZK

# Monkey-patch to add debug output
original_send_command = ZK._ZK__send_command

def debug_send_command(self, command, command_string=b'', response_size=8):
    print(f"\n=== Sending command {command} ===")
    try:
        result = original_send_command(command, command_string, response_size)
        print(f"=== Command {command} SUCCESS ===")
        return result
    except Exception as e:
        print(f"=== Command {command} FAILED: {e} ===")
        if hasattr(self, '_ZK__tcp_data_recv'):
            data = self._ZK__tcp_data_recv
            print(f"Received data length: {len(data)} bytes")
            print(f"First 16 bytes (hex): {data[:16].hex() if len(data) >= 16 else data.hex()}")
            if len(data) >= 8:
                try:
                    tcp_header = struct.unpack('<HHI', data[:8])
                    print(f"TCP header: magic1=0x{tcp_header[0]:04x}, magic2=0x{tcp_header[1]:04x}, length={tcp_header[2]}")
                except:
                    pass
        raise

ZK._ZK__send_command = debug_send_command

# Now run the test
print("Connecting...")
zk = ZK('127.0.0.1', port=4370, timeout=5)
conn = zk.connect()
print("Connected!")

print("\nGetting pin width...")
pw = conn.get_pin_width()
print(f"Pin Width: {pw}")

print("\nGetting serial number...")
try:
    sn = conn.get_serialnumber()
    print(f"Serial Number: {sn}")
except Exception as e:
    print(f"Error: {e}")

conn.disconnect()
