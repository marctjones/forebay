#!/bin/bash

# Forebay Demo Script
# Demonstrates all implemented features with real-world use cases

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
WORKER_URL="${WORKER_URL:-https://forebay-worker.marc-t-jones.workers.dev}"
CLI_PATH="${CLI_PATH:-./client/Forebay.Cli/bin/Debug/net9.0/Forebay.Cli}"

# Helper functions
print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_step() {
    echo -e "${GREEN}▶ $1${NC}"
}

print_command() {
    echo -e "${YELLOW}$ $1${NC}"
}

wait_for_user() {
    echo -e "\n${YELLOW}Press Enter to continue...${NC}"
    read -r
}

# Check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"

    # Check if dotnet is installed
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}Error: dotnet is not installed${NC}"
        exit 1
    fi
    print_step "✓ .NET SDK found: $(dotnet --version)"

    # Check if curl is installed
    if ! command -v curl &> /dev/null; then
        echo -e "${RED}Error: curl is not installed${NC}"
        exit 1
    fi
    print_step "✓ curl found"

    # Check if jq is installed (optional but nice)
    if command -v jq &> /dev/null; then
        HAS_JQ=true
        print_step "✓ jq found (JSON output will be formatted)"
    else
        HAS_JQ=false
        print_step "ℹ jq not found (install for prettier JSON output)"
    fi
}

# Build the CLI
build_cli() {
    print_header "Building Forebay CLI"

    print_step "Building .NET solution..."
    print_command "dotnet build client/Forebay.Cli/Forebay.Cli.csproj"
    dotnet build client/Forebay.Cli/Forebay.Cli.csproj > /dev/null 2>&1

    if [ ! -f "$CLI_PATH" ]; then
        echo -e "${RED}Error: CLI binary not found at $CLI_PATH${NC}"
        exit 1
    fi

    print_step "✓ CLI built successfully"
}

# Demo 1: Health Check
demo_health_check() {
    print_header "Demo 1: Health Check"

    print_step "Testing Worker availability and health status"
    print_command "curl -s $WORKER_URL/health"

    response=$(curl -s "$WORKER_URL/health")
    if [ "$HAS_JQ" = true ]; then
        echo "$response" | jq .
    else
        echo "$response"
    fi

    wait_for_user
}

# Demo 2: Authentication
demo_authentication() {
    print_header "Demo 2: Authentication with API Keys"

    print_step "Logging in with API key (dev-alice)"
    print_command "$CLI_PATH login dev-alice"
    $CLI_PATH login dev-alice

    # Update config to use correct worker URL
    print_step "Configuring Worker URL: $WORKER_URL"
    config_file="$HOME/.config/forebay/config.toml"
    if grep -q "worker_url" "$config_file"; then
        sed -i "s|worker_url = .*|worker_url = \"$WORKER_URL\"|" "$config_file"
    else
        echo "worker_url = \"$WORKER_URL\"" >> "$config_file"
    fi

    print_step "Verifying authentication"
    print_command "$CLI_PATH whoami"
    $CLI_PATH whoami

    wait_for_user
}

# Demo 3: Basic Queue Operations
demo_basic_queue_ops() {
    print_header "Demo 3: Basic Queue Operations"

    print_step "Use Case: Simple message queue for task processing"

    # Push messages
    print_step "1. Pushing messages to 'tasks' queue"
    print_command "$CLI_PATH push tasks '{\"task\":\"send-email\",\"to\":\"user@example.com\"}'"
    $CLI_PATH push tasks '{"task":"send-email","to":"user@example.com"}'

    print_command "$CLI_PATH push tasks '{\"task\":\"process-payment\",\"amount\":99.99}'"
    $CLI_PATH push tasks '{"task":"process-payment","amount":99.99}'

    print_command "$CLI_PATH push tasks '{\"task\":\"generate-report\",\"type\":\"monthly\"}'"
    $CLI_PATH push tasks '{"task":"generate-report","type":"monthly"}'

    # Check stats
    print_step "2. Checking queue statistics"
    print_command "$CLI_PATH stats tasks"
    $CLI_PATH stats tasks

    # Pull message
    print_step "3. Worker pulling task from queue (FIFO - First In, First Out)"
    print_command "$CLI_PATH pull tasks"
    $CLI_PATH pull tasks

    print_step "4. Checking updated queue stats"
    print_command "$CLI_PATH stats tasks"
    $CLI_PATH stats tasks

    wait_for_user
}

# Demo 4: Job Queue Pattern
demo_job_queue() {
    print_header "Demo 4: Job Queue Pattern"

    print_step "Use Case: Background job processing system"

    # Create multiple job types
    print_step "1. Submitting various background jobs"

    print_command "$CLI_PATH push image-processing '{\"job\":\"resize\",\"image\":\"photo.jpg\",\"width\":800}'"
    $CLI_PATH push image-processing '{"job":"resize","image":"photo.jpg","width":800}'

    print_command "$CLI_PATH push image-processing '{\"job\":\"thumbnail\",\"image\":\"photo.jpg\",\"size\":\"200x200\"}'"
    $CLI_PATH push image-processing '{"job":"thumbnail","image":"photo.jpg","size":"200x200"}'

    print_command "$CLI_PATH push image-processing '{\"job\":\"watermark\",\"image\":\"photo.jpg\",\"text\":\"© 2026\"}'"
    $CLI_PATH push image-processing '{"job":"watermark","image":"photo.jpg","text":"© 2026"}'

    # Stats
    print_step "2. Viewing job queue status"
    print_command "$CLI_PATH stats image-processing"
    $CLI_PATH stats image-processing

    # Simulate worker processing
    print_step "3. Simulating worker processing jobs..."
    for i in {1..3}; do
        echo -e "   Processing job $i..."
        result=$($CLI_PATH pull image-processing)
        if [ "$HAS_JQ" = true ]; then
            echo "$result" | head -1 | jq -c '.job' | sed 's/^/   Job: /'
        fi
        sleep 0.5
    done

    print_step "4. All jobs processed!"
    print_command "$CLI_PATH stats image-processing"
    $CLI_PATH stats image-processing

    wait_for_user
}

# Demo 5: Event Streaming
demo_event_streaming() {
    print_header "Demo 5: Event Streaming Pattern"

    print_step "Use Case: Real-time event processing and analytics"

    # Push events
    print_step "1. Publishing user activity events"

    events=(
        '{"event":"page_view","user_id":"user123","page":"/home","timestamp":"2026-01-05T02:10:00Z"}'
        '{"event":"button_click","user_id":"user123","button":"signup","timestamp":"2026-01-05T02:10:15Z"}'
        '{"event":"form_submit","user_id":"user123","form":"registration","timestamp":"2026-01-05T02:10:30Z"}'
        '{"event":"purchase","user_id":"user123","amount":49.99,"timestamp":"2026-01-05T02:10:45Z"}'
    )

    for event in "${events[@]}"; do
        echo "   Publishing: $(echo "$event" | jq -c '.event' 2>/dev/null || echo "$event")"
        $CLI_PATH push user-events "$event" > /dev/null
        sleep 0.3
    done

    print_step "2. Event stream status"
    print_command "$CLI_PATH stats user-events"
    $CLI_PATH stats user-events

    print_step "3. Analytics worker consuming events"
    print_command "$CLI_PATH pull user-events --pretty"
    $CLI_PATH pull user-events --pretty

    wait_for_user
}

# Demo 6: Multiple Queues
demo_multiple_queues() {
    print_header "Demo 6: Multiple Queue Management"

    print_step "Use Case: Separating different priority levels"

    # High priority queue
    print_step "1. Adding high priority tasks"
    print_command "$CLI_PATH push high-priority '{\"alert\":\"system-down\",\"severity\":\"critical\"}'"
    $CLI_PATH push high-priority '{"alert":"system-down","severity":"critical"}'

    # Normal priority queue
    print_step "2. Adding normal priority tasks"
    print_command "$CLI_PATH push normal-priority '{\"task\":\"daily-backup\",\"schedule\":\"midnight\"}'"
    $CLI_PATH push normal-priority '{"task":"daily-backup","schedule":"midnight"}'

    # Low priority queue
    print_step "3. Adding low priority tasks"
    print_command "$CLI_PATH push low-priority '{\"task\":\"cleanup-logs\",\"older_than\":\"30d\"}'"
    $CLI_PATH push low-priority '{"task":"cleanup-logs","older_than":"30d"}'

    print_step "4. Queue status overview"
    echo "   High Priority:"
    $CLI_PATH stats high-priority | grep "Length:"
    echo "   Normal Priority:"
    $CLI_PATH stats normal-priority | grep "Length:"
    echo "   Low Priority:"
    $CLI_PATH stats low-priority | grep "Length:"

    print_step "5. Processing high priority first"
    print_command "$CLI_PATH pull high-priority"
    $CLI_PATH pull high-priority

    wait_for_user
}

# Demo 7: Queue Management
demo_queue_management() {
    print_header "Demo 7: Queue Lifecycle Management"

    print_step "Use Case: Creating, monitoring, and cleaning up queues"

    # Create a temporary queue
    print_step "1. Creating a temporary processing queue"
    print_command "$CLI_PATH push temp-batch '{\"batch_id\":\"batch-001\",\"item\":1}'"
    $CLI_PATH push temp-batch '{"batch_id":"batch-001","item":1}'
    $CLI_PATH push temp-batch '{"batch_id":"batch-001","item":2}' > /dev/null
    $CLI_PATH push temp-batch '{"batch_id":"batch-001","item":3}' > /dev/null

    print_step "2. Monitoring queue"
    print_command "$CLI_PATH stats temp-batch"
    $CLI_PATH stats temp-batch

    print_step "3. Processing all items"
    $CLI_PATH pull temp-batch 2>/dev/null || true
    $CLI_PATH pull temp-batch 2>/dev/null || true
    $CLI_PATH pull temp-batch 2>/dev/null || true

    print_step "4. Verifying queue is empty"
    print_command "$CLI_PATH stats temp-batch"
    $CLI_PATH stats temp-batch

    print_step "5. Cleaning up empty queue"
    print_command "$CLI_PATH delete temp-batch --force"
    $CLI_PATH delete temp-batch --force

    wait_for_user
}

# Demo 8: Persistent Storage
demo_storage() {
    print_header "Demo 8: Persistent Document Storage"

    print_step "Use Case: Storing persistent data (task lists, notes, configuration)"

    # Store documents
    print_step "1. Storing a task list"
    print_command "$CLI_PATH put tasks/todo-list '{\"tasks\":[{\"id\":1,\"text\":\"Review PRs\",\"done\":false},{\"id\":2,\"text\":\"Deploy to prod\",\"done\":false}]}'"
    $CLI_PATH put tasks/todo-list '{"tasks":[{"id":1,"text":"Review PRs","done":false},{"id":2,"text":"Deploy to prod","done":false}]}'

    print_step "2. Storing a configuration document"
    print_command "$CLI_PATH put config/app-settings '{\"theme\":\"dark\",\"notifications\":true,\"apiEndpoint\":\"https://api.example.com\"}'"
    $CLI_PATH put config/app-settings '{"theme":"dark","notifications":true,"apiEndpoint":"https://api.example.com"}'

    print_step "3. Storing a note"
    print_command "$CLI_PATH put notes/meeting-2026-01-05 '{\"title\":\"Team Meeting\",\"date\":\"2026-01-05\",\"notes\":\"Discussed Q1 roadmap and sprint planning\"}'"
    $CLI_PATH put notes/meeting-2026-01-05 '{"title":"Team Meeting","date":"2026-01-05","notes":"Discussed Q1 roadmap and sprint planning"}'

    # List documents
    print_step "4. Listing all documents"
    print_command "$CLI_PATH list-docs"
    $CLI_PATH list-docs

    # Filter by prefix
    print_step "5. Filtering documents by prefix"
    print_command "$CLI_PATH list-docs --prefix tasks"
    $CLI_PATH list-docs --prefix tasks

    # Retrieve a document
    print_step "6. Retrieving a document"
    print_command "$CLI_PATH get tasks/todo-list --pretty"
    $CLI_PATH get tasks/todo-list --pretty

    # Update a document
    print_step "7. Updating a document (marking task as done)"
    print_command "$CLI_PATH put tasks/todo-list '{\"tasks\":[{\"id\":1,\"text\":\"Review PRs\",\"done\":true},{\"id\":2,\"text\":\"Deploy to prod\",\"done\":false}]}'"
    $CLI_PATH put tasks/todo-list '{"tasks":[{"id":1,"text":"Review PRs","done":true},{"id":2,"text":"Deploy to prod","done":false}]}'

    print_step "8. Viewing updated document"
    print_command "$CLI_PATH get tasks/todo-list --pretty"
    $CLI_PATH get tasks/todo-list --pretty

    wait_for_user
}

# Demo 9: Direct API Testing
demo_direct_api() {
    print_header "Demo 9: Direct API Access (for developers)"

    print_step "Use Case: Using the REST API directly with curl"

    print_step "1. Authentication with Bearer token"
    print_command "curl -H 'Authorization: Bearer dev-alice' $WORKER_URL/auth/whoami"
    response=$(curl -s -H "Authorization: Bearer dev-alice" "$WORKER_URL/auth/whoami")
    if [ "$HAS_JQ" = true ]; then
        echo "$response" | jq .
    else
        echo "$response"
    fi

    print_step "2. Push via REST API"
    print_command "curl -X POST -H 'Authorization: Bearer dev-alice' -H 'Content-Type: application/json' -d '{\"payload\":{\"api\":\"test\"}}' $WORKER_URL/queues/api-test/push"
    response=$(curl -s -X POST -H "Authorization: Bearer dev-alice" -H "Content-Type: application/json" -d '{"payload":{"api":"test"}}' "$WORKER_URL/queues/api-test/push")
    if [ "$HAS_JQ" = true ]; then
        echo "$response" | jq .
    else
        echo "$response"
    fi

    print_step "3. Stats via REST API"
    print_command "curl -H 'Authorization: Bearer dev-alice' $WORKER_URL/queues/api-test/stats"
    response=$(curl -s -H "Authorization: Bearer dev-alice" "$WORKER_URL/queues/api-test/stats")
    if [ "$HAS_JQ" = true ]; then
        echo "$response" | jq .
    else
        echo "$response"
    fi

    wait_for_user
}

# Cleanup demo data
cleanup_demo() {
    print_header "Cleanup"

    print_step "Cleaning up demo queues..."

    # List of demo queues to clean up
    demo_queues=(
        "tasks"
        "image-processing"
        "user-events"
        "high-priority"
        "normal-priority"
        "low-priority"
        "api-test"
    )

    for queue in "${demo_queues[@]}"; do
        echo "   Deleting $queue..."
        $CLI_PATH delete "$queue" --force 2>/dev/null || true
    done

    print_step "Cleaning up demo documents..."

    # List of demo documents to clean up
    demo_docs=(
        "tasks/todo-list"
        "config/app-settings"
        "notes/meeting-2026-01-05"
    )

    for doc in "${demo_docs[@]}"; do
        echo "   Deleting $doc..."
        $CLI_PATH delete-doc "$doc" --force 2>/dev/null || true
    done

    print_step "✓ Demo data cleaned up"
}

# Main demo flow
main() {
    clear

    print_header "Forebay Demo - Queue-as-a-Service"
    echo "This demo showcases the Forebay message queue system"
    echo "running on Cloudflare Workers with KV storage."
    echo ""
    echo "Worker URL: $WORKER_URL"
    echo ""
    wait_for_user

    check_prerequisites
    build_cli

    # Run demos
    demo_health_check
    demo_authentication
    demo_basic_queue_ops
    demo_job_queue
    demo_event_streaming
    demo_multiple_queues
    demo_queue_management
    demo_storage
    demo_direct_api

    # Cleanup
    cleanup_demo

    # Summary
    print_header "Demo Complete!"
    echo -e "${GREEN}You've seen:${NC}"
    echo "  ✓ Health checking and monitoring"
    echo "  ✓ API key authentication"
    echo "  ✓ Basic queue operations (push, pull, stats)"
    echo "  ✓ Job queue pattern for background processing"
    echo "  ✓ Event streaming for analytics"
    echo "  ✓ Multi-queue priority management"
    echo "  ✓ Queue lifecycle (create, monitor, delete)"
    echo "  ✓ Persistent document storage (put, get, list, delete)"
    echo "  ✓ Direct REST API access"
    echo ""
    echo -e "${BLUE}Next Steps:${NC}"
    echo "  • Run individual commands: $CLI_PATH --help"
    echo "  • API Documentation: Check worker/README.md"
    echo "  • Deploy your own: wrangler deploy"
    echo ""
    print_step "Thank you for trying Forebay!"
}

# Run the demo
main
