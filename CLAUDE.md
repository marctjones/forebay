# forebay

# forebay

Describe your project vision here.

## IdlerGear Integration

This project uses IdlerGear for knowledge management. IdlerGear provides:
- **Tasks** - Track work items (syncs to GitHub Issues)
- **Notes** - Quick capture (use `--tag explore` for research, `--tag idea` for ideas)
- **Reference** - Technical documentation
- **Plans** - Implementation roadmaps
- **Vision** - Project goals and direction
- **Runs** - Process output tracking

### Before Starting Work

Always check context first:
```bash
idlergear vision show    # Understand project goals
idlergear plan show      # See current plan
idlergear task list      # Review open tasks
idlergear note list      # Check recent notes
```

### During Development

Capture knowledge as you work:
```bash
idlergear note create "discovered X while working on Y"
idlergear note create "should we try Z?" --tag explore
idlergear task create "need to fix Z" --label bug
idlergear reference add "API Design" --body "..."
```

### MCP Tools Available

The IdlerGear MCP server provides direct tool access. Use tools like:
- `idlergear_task_list` - List tasks
- `idlergear_note_create` - Create notes
- `idlergear_vision_show` - Get project vision

## Protected Files

Do NOT modify files in `.idlergear/` directly. Use idlergear commands instead.
The `.claude/` directory contains Claude Code settings - avoid modifying these too.

## IdlerGear Usage

**ALWAYS run at session start:**
```bash
idlergear context
```

**FORBIDDEN files:** `TODO.md`, `NOTES.md`, `SESSION_*.md`, `SCRATCH.md`
**FORBIDDEN comments:** `// TODO:`, `# FIXME:`, `/* HACK: */`

**Use instead:**
- `idlergear task create "..."` - Create actionable tasks
- `idlergear note create "..."` - Capture quick thoughts
- `idlergear explore create "..."` - Research questions
- `idlergear vision show` - Check project goals

See AGENTS.md for full command reference.
