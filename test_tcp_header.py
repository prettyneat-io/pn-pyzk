#!/usr/bin/env python3
import struct

# Simulate what create_tcp_top does
MACHINE_PREPARE_DATA_1 = 20560  # 0x5050
MACHINE_PREPARE_DATA_2 = 32130  # 0x7d82

def create_tcp_top(packet):
    """Create TCP top header"""
    length = len(packet)
    top = struct.pack('<HHI', MACHINE_PREPARE_DATA_1, MACHINE_PREPARE_DATA_2, length)
    return top + packet

# Test with a 42-byte payload packet
payload_packet = b'X' * 42

result = create_tcp_top(payload_packet)

print(f"Payload packet size: {len(payload_packet)} bytes")
print(f"Result size: {len(result)} bytes")
print(f"TCP header (first 8 bytes): {result[:8].hex()}")

# Parse the header
tcp_header = struct.unpack('<HHI', result[:8])
print(f"Magic1: 0x{tcp_header[0]:04x} (expected 0x5050)")
print(f"Magic2: 0x{tcp_header[1]:04x} (expected 0x7d82)")
print(f"Length: {tcp_header[2]} (expected {len(payload_packet)})")

if tcp_header[2] == len(payload_packet):
    print("\n✓ TCP header length is CORRECT!")
else:
    print(f"\n✗ ERROR: TCP header length is {tcp_header[2]}, should be {len(payload_packet)}")
