#!/bin/bash
# Start simulator in background
python3 zk_simulator.py > simulator_output.txt 2>&1 &
SIM_PID=$!
echo "Simulator started with PID $SIM_PID"

# Wait for simulator to start
sleep 2

# Run test
echo "Running test..."
python3 test_serial.py

# Kill simulator
echo "Stopping simulator..."
kill $SIM_PID 2>/dev/null
wait $SIM_PID 2>/dev/null

# Show simulator output
echo ""
echo "===== Simulator Output ====="
cat simulator_output.txt
rm simulator_output.txt
