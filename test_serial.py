#!/usr/bin/env python3
# Simple test for serial number
import sys
sys.path.append("zk")

from zk import ZK

zk = ZK('127.0.0.1', port=4370, timeout=5)
try:
    print('Connecting...')
    conn = zk.connect()
    print('Connected!')
    
    print('Getting pin width...')
    pw = conn.get_pin_width()
    print(f'Pin Width: {pw}')
    
    print('Getting serial number...')
    sn = conn.get_serialnumber()
    print(f'Serial Number: {sn}')
    
    print('Getting MAC...')
    mac = conn.get_mac()
    print(f'MAC: {mac}')
    
    conn.disconnect()
    print('Done!')
except Exception as e:
    print(f'Error: {e}')
    import traceback
    traceback.print_exc()
