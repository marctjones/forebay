# Forebay Demo

Interactive demonstration of the Forebay Queue-as-a-Service platform.

## Quick Start

Run the full interactive demo:

```bash
./demo.sh
```

The demo will automatically:
- Check prerequisites (.NET SDK, curl)
- Build the CLI
- Walk through 8 different use cases
- Clean up demo data afterwards

## What's Demonstrated

### 1. Health Check
- Worker availability monitoring
- System status verification

### 2. Authentication
- API key-based authentication
- Configuration management
- Identity verification with `whoami`

### 3. Basic Queue Operations
- Pushing messages to queues
- Pulling messages (FIFO order)
- Viewing queue statistics
- **Use Case:** Simple task queue for background processing

### 4. Job Queue Pattern
- Submitting background jobs
- Worker processing simulation
- Queue monitoring
- **Use Case:** Image processing pipeline

### 5. Event Streaming
- Publishing real-time events
- Event consumption for analytics
- Time-series data handling
- **Use Case:** User activity tracking

### 6. Multiple Queue Management
- Priority-based queue separation
- High/Normal/Low priority queues
- Selective queue processing
- **Use Case:** Task prioritization system

### 7. Queue Lifecycle
- Creating temporary queues
- Monitoring queue status
- Batch processing
- Queue cleanup and deletion
- **Use Case:** Batch job processing

### 8. Direct API Access
- REST API authentication
- HTTP-based queue operations
- Developer integration examples
- **Use Case:** Direct API integration

## Requirements

### System Requirements
- **Operating System:** Linux, macOS, or Windows with WSL
- **Shell:** bash 4.0+
- **.NET SDK:** 9.0 or higher
- **curl:** Any recent version
- **jq:** (Optional) For prettier JSON output

### Installation

```bash
# Install .NET SDK (if not installed)
# Visit: https://dotnet.microsoft.com/download

# Install jq (optional, for pretty JSON)
# Ubuntu/Debian:
sudo apt-get install jq

# macOS:
brew install jq

# Fedora:
sudo dnf install jq
```

## Configuration

The demo uses the production Forebay Worker by default:
```
https://forebay-worker.marc-t-jones.workers.dev
```

### Custom Worker URL

To use a different Worker (e.g., local development):

```bash
# Run with local worker
WORKER_URL=http://localhost:8787 ./demo.sh

# Run with custom deployment
WORKER_URL=https://your-worker.workers.dev ./demo.sh
```

### Custom CLI Path

If you built the CLI in a different location:

```bash
CLI_PATH=/path/to/Forebay.Cli ./demo.sh
```

## Running Individual Demos

The script is modular. You can run specific demos by calling the functions directly:

```bash
# Source the script
source demo.sh

# Run specific demo
demo_basic_queue_ops

# Run cleanup
cleanup_demo
```

## What Gets Created

The demo creates the following queues:
- `tasks` - General task queue
- `image-processing` - Job processing queue
- `user-events` - Event stream queue
- `high-priority` - High priority tasks
- `normal-priority` - Normal priority tasks
- `low-priority` - Low priority tasks
- `temp-batch` - Temporary batch queue (deleted during demo)
- `api-test` - API testing queue

**Note:** All queues are automatically cleaned up at the end of the demo.

## Demo Data

### Authentication
- **API Key:** `dev-alice`
- **Email:** `alice@example.com`
- **Config File:** `~/.config/forebay/config.toml`

### Sample Messages

The demo uses realistic sample messages:

```json
// Task Queue
{"task": "send-email", "to": "user@example.com"}
{"task": "process-payment", "amount": 99.99}

// Image Processing
{"job": "resize", "image": "photo.jpg", "width": 800}
{"job": "thumbnail", "image": "photo.jpg", "size": "200x200"}

// User Events
{"event": "page_view", "user_id": "user123", "page": "/home"}
{"event": "purchase", "user_id": "user123", "amount": 49.99}

// Alerts
{"alert": "system-down", "severity": "critical"}
```

## Expected Output

The demo provides:
- ✅ **Colored output** for better readability
- 📊 **Real-time status** for each operation
- 🔍 **JSON formatting** (if jq is installed)
- ⏸️ **Interactive pauses** between sections
- 📝 **Command examples** showing exact syntax

## Troubleshooting

### Demo fails with "CLI not found"

Make sure you've built the project first:
```bash
dotnet build client/Forebay.Cli/Forebay.Cli.csproj
```

### Demo fails with "Worker not responding"

Check if the Worker is accessible:
```bash
curl https://forebay-worker.marc-t-jones.workers.dev/health
```

### Demo fails with "dotnet: command not found"

Install the .NET SDK:
- Visit: https://dotnet.microsoft.com/download
- Or use your package manager (apt, brew, etc.)

### Permission denied when running ./demo.sh

Make the script executable:
```bash
chmod +x demo.sh
```

### Config file errors

Remove existing config and let the demo recreate it:
```bash
rm -rf ~/.config/forebay/config.toml
./demo.sh
```

## Extending the Demo

To add your own use case:

1. Create a new demo function:
```bash
demo_your_use_case() {
    print_header "Demo: Your Use Case"
    print_step "Description of what you're demonstrating"
    print_command "$CLI_PATH push your-queue '{\"data\":\"value\"}'"
    $CLI_PATH push your-queue '{"data":"value"}'
    wait_for_user
}
```

2. Add it to the main() function:
```bash
main() {
    # ... existing demos ...
    demo_your_use_case
    # ...
}
```

3. Add cleanup:
```bash
cleanup_demo() {
    # ... existing cleanup ...
    demo_queues+=("your-queue")
}
```

## Real-World Use Cases

### Background Job Processing
```bash
# Submit jobs
forebay push jobs '{"type":"send-email","recipient":"user@example.com"}'
forebay push jobs '{"type":"resize-image","file":"photo.jpg"}'

# Worker processes jobs
while true; do
  job=$(forebay pull jobs)
  process_job "$job"
done
```

### Event Streaming & Analytics
```bash
# Publisher: Send events
forebay push analytics '{"event":"page_view","user":"123","page":"/home"}'

# Consumer: Process events
forebay pull analytics | process_analytics
```

### Microservice Communication
```bash
# Service A sends message to Service B
forebay push service-b-inbox '{"action":"process","data":{...}}'

# Service B processes
response=$(forebay pull service-b-inbox)
process_and_respond "$response"
```

### Task Scheduling
```bash
# Scheduler adds tasks
forebay push scheduled-tasks '{"run_at":"2026-01-06T10:00:00Z","task":"backup"}'

# Worker checks and executes
while true; do
  task=$(forebay pull scheduled-tasks)
  if should_run_now "$task"; then
    execute_task "$task"
  fi
done
```

## Performance Characteristics

Based on testing with the production deployment:

- **Latency:** ~100-300ms per operation (including network)
- **Throughput:** Limited by Cloudflare Workers (100k requests/day on free tier)
- **Storage:** KV storage limits apply (1GB on free tier)
- **FIFO Guarantee:** Yes, messages are processed in order
- **Durability:** Durable storage via Cloudflare KV

## Next Steps

After running the demo:

1. **Explore the CLI:**
   ```bash
   client/Forebay.Cli/bin/Debug/net9.0/Forebay.Cli --help
   ```

2. **Try the API directly:**
   ```bash
   curl -H "Authorization: Bearer dev-alice" \
        https://forebay-worker.marc-t-jones.workers.dev/queues
   ```

3. **Deploy your own Worker:**
   ```bash
   cd worker
   wrangler deploy
   ```

4. **Integrate into your application:**
   - Use the C# client library
   - Use the REST API from any language
   - Build your own client using the API docs

## Feedback

Found a bug or have a suggestion? Please open an issue or submit a pull request!

## License

See the main project README for license information.
