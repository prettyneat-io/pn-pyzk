#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Debug test for simulator
"""

import sys
import os

CWD = os.path.dirname(os.path.realpath(__file__))
ROOT_DIR = os.path.dirname(CWD)
sys.path.insert(0, ROOT_DIR)

from zk import ZK

def debug_test():
    print("Connecting to simulator...")
    zk = ZK('127.0.0.1', port=4370, timeout=5, verbose=True, ommit_ping=True)
    
    try:
        conn = zk.connect()
        print("\n=== Connected ===\n")
        
        print("Getting users...")
        users = conn.get_users()
        print(f"Got {len(users)} users")
        for user in users:
            print(f"  - {user}")
        
        print("\nDisconnecting...")
        conn.disconnect()
        print("Done!")
        
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == '__main__':
    debug_test()
