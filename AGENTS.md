# Agent Instructions

## Project Overview

# forebay

Describe your project vision here.

## IdlerGear Knowledge Management

This project uses [IdlerGear](https://github.com/marctjones/idlergear) for knowledge management.

### CRITICAL: Session Start

**ALWAYS run this at the start of EVERY session:**

```bash
idlergear context
```

This shows vision, current plan, open tasks, and recent notes. Do NOT skip this step.

### CRITICAL: Persist Your Discoveries

**When you learn something, STORE IT for future sessions:**

```bash
# Discovered API behavior
idlergear note create "Auth endpoint requires Bearer token prefix"

# Found a quirk or gotcha
idlergear note create "Parser fails on empty input - needs null check" --tag bug

# Had an architectural idea
idlergear note create "Could cache AST to improve performance" --tag idea
```

**This note WILL be available in your next session.** Without this, your learnings are lost.

### PREFER: IdlerGear Search Over File Search

**Before searching files for project context, try:**

```bash
idlergear search "authentication"
```

This searches tasks, notes, references, and plans - structured knowledge that persists.

### FORBIDDEN: File-Based Knowledge

**DO NOT create any of these files:**
- `TODO.md`, `TODO.txt`, `TASKS.md`
- `NOTES.md`, `SESSION_*.md`, `SCRATCH.md`
- `FEATURE_IDEAS.md`, `RESEARCH.md`, `BACKLOG.md`

**ALWAYS use IdlerGear commands instead.**

### FORBIDDEN: Inline TODOs

**DO NOT write inline TODO comments:**
- `// TODO: ...`
- `# FIXME: ...`
- `/* HACK: ... */`

**INSTEAD:** `idlergear task create "..." --label technical-debt`

### REQUIRED: Use IdlerGear Commands

| Instead of... | Use this command |
|---------------|------------------|
| Creating TODO.md | `idlergear task create "description"` |
| Writing notes to files | `idlergear note create "content"` |
| Adding TODO comments | `idlergear task create "..." --label technical-debt` |
| Creating VISION.md | `idlergear vision edit` |
| Documenting findings | `idlergear reference add "title" --body "..."` |

### During Development

| Action | Command |
|--------|---------|
| Discovered something | `idlergear note create "..."` |
| Found a bug | `idlergear task create "..." --label bug` |
| Had an idea | `idlergear note create "..." --tag idea` |
| Research question | `idlergear note create "..." --tag explore` |
| Completed work | `idlergear task close <id>` |
| Session end | Consider what to note for next time |

### Knowledge Promotion Flow

```
note → task or reference
```
- Quick thoughts: `idlergear note create "..."` (capture now, review later)
- Research threads: `idlergear note create "..." --tag explore` (open questions)
- Ideas: `idlergear note create "..." --tag idea` (future possibilities)
- Actionable work: `idlergear task create "..."` (clear completion criteria)
- Promote notes: `idlergear note promote <id> --to task` (convert to task or reference)

### Reference Documentation

- `idlergear reference list` - View reference documents
- `idlergear reference show "title"` - Read a specific reference
- `idlergear reference add "title" --body "..."` - Add documentation
- `idlergear search "query"` - Search across all knowledge types

### Protected Files

**DO NOT modify directly:**
- `.idlergear/` - Data files (use CLI commands)
- `.claude/` - Claude Code settings
- `.mcp.json` - MCP configuration
