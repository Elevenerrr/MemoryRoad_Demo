# AGENTS.md

## Skills Reference

Load the appropriate skill before starting each workflow:

| Workflow | Skill |
|----------|-------|
| Using unityMCP | `unity-mcp-orchestrator` |
| Using unity-test-framework | `unity-test-framework` |

---

## Development Workflow

### Operation Requirements

- Check both `message` and `success` from MCP call results to determine success
- When MCP calls fail, adjust parameters based on the returned `message` and retry
- Use `batch_execute` as much as possible to reduce MCP call times
- Ensure the editor is not in Play mode before executing editor scripts
- Execute editor scripts automatically via `unityMCP` (avoid manual execution)
- Use git for version control, add proper `.gitignore` before `git init`
- Add Tooltip attributes to editor-configurable variables
- Check Unity Console log first when encountering bugs

### Plan Mode Orchestration

- Non-trivial tasks (3+ steps or architecture decisions) must enter plan mode
- If things go off track, stop and replan—don't push through
- If bugs or frustration accumulate, stop and replan
- Write detailed specs as input to reduce ambiguity
- Use numbered steps

### Task Management

1. Plan first: write plan to `tasks/todo.md` with checkboxes
2. Validate plan before starting implementation
3. Implement incrementally: check off each step as completed
4. Explain changes: provide high-level summary for each step
5. Document results: add review section in `tasks/todo.md`
6. Record lessons: update `tasks/lessons.md` after corrections

### Subagent Strategy

- Use subagents heavily to keep the main context window clean
- Outsource research, exploration, and parallel analysis to subagents
- Reduce cognitive load and separate concerns via subagents
- One task per subagent, focused execution

### Code Quality Principles

**Self-Improvement Loop:**

- After user correction: update `tasks/lessons.md` to record patterns
- Write rules for yourself to prevent the same mistakes
- Iterate improvements until error rate drops
- Verify improvements each session, apply to relevant projects

**Verification Defaults:**

- Never mark a task complete until you've proven it works
- Compare main branch to your changes when relevant
- Always be ready to push to production, verify
- Ask yourself "Would a senior engineer approve this?"
- Do assertions, logs, test suite corrections

**Elegant Code:**

- For non-trivial changes: pause and ask "Is there a more elegant way?"
- If a fix feels like a patch: "Now that I know everything, implement an elegant solution"
- Skip this step for simple obvious fixes, don't over-engineer
- Challenge your own work before committing

**Core Principles:**

- Simplicity first: each change should be as simple as possible
- Don't be lazy: find root causes, no band-aid fixes
- Minimal impact: changes should only involve what's necessary

**Autonomous Bug Fixing:**

- When you find a bug: fix it directly, don't ask for help
- Point out logs, errors, failing tests, then resolve
- User doesn't need to switch context
- Proactively fix failing CI tests

---

## Testing

### Test-Driven Development (TDD)

- Follow the **Red-Green-Refactor** cycle
- Run tests through `run_tests` in `unityMCP`
- After tests pass, refactor code through code-review

---

## Preferences

### Input System

- Use the legacy input system

### Git

- Do not use `git worktree`

---

## GameObjects Setup

- Design the size and materials of different objects carefully

---

## Editor Scripts

**Running Editor Scripts:**

1. Use `[MenuItem("YourMenuPath")]` attribute on a static method
2. Refresh Unity with `mcp_unityMCP_refresh_unity` to compile
3. Execute the menu item with `mcp_unityMCP_execute_menu_item`
4. The script will run in Unity and perform configured actions

---

## Troubleshooting

### Common Issues

- When using unity-test-framework, forgetting to call the `unity-test-framework` skill causes compilation errors