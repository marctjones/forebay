---
description: REQUIRED rules for IdlerGear knowledge management
alwaysApply: true
---

# IdlerGear Usage Rules

## CRITICAL: Session Start

**ALWAYS run this at the start of EVERY conversation:**

```bash
idlergear context
```

This provides vision, current plan, and open tasks. Do NOT skip this step.

## CRITICAL: Persist Your Discoveries

**When you learn something, STORE IT:**

```bash
# Discovered API behavior
idlergear note create "Auth endpoint requires Bearer token prefix"

# Found a quirk or gotcha
idlergear note create "Parser fails on empty input - needs null check" --tag bug

# Had an architectural idea
idlergear note create "Could cache AST to improve performance" --tag idea
```

**This note WILL be available in your next session.** Without this, your learnings are lost.

## FORBIDDEN: File-Based Knowledge

**DO NOT create any of these files:**
- `TODO.md`, `TODO.txt`, `TASKS.md`
- `NOTES.md`, `SESSION_*.md`, `SCRATCH.md`
- `FEATURE_IDEAS.md`, `RESEARCH.md`, `BACKLOG.md`

**ALWAYS use IdlerGear commands instead.**

## FORBIDDEN: Inline TODOs

**DO NOT write inline TODO comments:**
- `// TODO: ...`
- `# FIXME: ...`
- `/* HACK: ... */`

**INSTEAD:** `idlergear task create "..." --label technical-debt`

## PREFER: IdlerGear Search Over File Search

**Before searching files for project context, try:**

```bash
idlergear search "authentication"
```

This searches tasks, notes, references, and plans - structured knowledge that persists.

## REQUIRED: Use IdlerGear Commands

| Instead of... | Use this command |
|---------------|------------------|
| Creating TODO.md | `idlergear task create "description"` |
| Writing notes to files | `idlergear note create "content"` |
| Adding TODO comments | `idlergear task create "..." --label technical-debt` |
| Creating VISION.md | `idlergear vision edit` |
| Documenting findings | `idlergear reference add "title" --body "..."` |

## Workflow

1. **Session start**: Run `idlergear context`
2. **Discovered something**: `idlergear note create "..."`
3. **Found a bug**: `idlergear task create "..." --label bug`
4. **Had an idea**: `idlergear note create "..." --tag idea`
5. **Research question**: `idlergear note create "..." --tag explore`
6. **Completed work**: `idlergear task close <id>`
7. **Session end**: Consider what should be noted for next time

## Knowledge Promotion Flow

```
note → task or reference
```
- Quick thoughts go to notes (capture now, review later)
- Use `--tag explore` for research questions, `--tag idea` for ideas
- Actionable work goes to tasks (clear completion criteria)
- Use `idlergear note promote <id> --to task` to convert notes to tasks

## MCP Tools

The IdlerGear MCP server provides direct tool access. PREFER these over file operations when available.
