#!/bin/bash

set -e

echo "=========================================="
echo "Testing Shared Storage for Task Managers"
echo "=========================================="
echo ""

# Use the Forebay CLI to interact with the same storage key both apps use
CLI_PATH="./client/Forebay.Cli/bin/Debug/net9.0/Forebay.Cli"

echo "1. Creating initial task list..."
$CLI_PATH put tasks/list '{"tasks":[{"Id":1,"Text":"Test TUI application","Done":false},{"Id":2,"Text":"Test Avalonia GUI application","Done":false},{"Id":3,"Text":"Verify shared storage works","Done":false}]}'
echo ""

echo "2. Reading back the task list..."
$CLI_PATH get tasks/list
echo ""

echo "3. Updating a task (marking as done)..."
$CLI_PATH put tasks/list '{"tasks":[{"Id":1,"Text":"Test TUI application","Done":true},{"Id":2,"Text":"Test Avalonia GUI application","Done":false},{"Id":3,"Text":"Verify shared storage works","Done":false}]}'
echo ""

echo "4. Verifying the update..."
$CLI_PATH get tasks/list
echo ""

echo "=========================================="
echo "Shared Storage Test Complete!"
echo "=========================================="
echo ""
echo "Both the TUI and Avalonia apps use the same 'tasks/list' storage key."
echo "You can now run either application and they will see the same data:"
echo ""
echo "  TUI:      ./apps/Forebay.TaskManager.Tui/bin/Debug/net9.0/Forebay.TaskManager.Tui"
echo "  Avalonia: ./apps/Forebay.TaskManager.Avalonia/bin/Debug/net9.0/Forebay.TaskManager.Avalonia"
echo ""
