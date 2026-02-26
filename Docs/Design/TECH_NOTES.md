# Tech Notes

## Codex + Unity Checklist
- Confirm the task only touches `Assets/Scripts/**` or `Docs/**` unless explicitly approved.
- Avoid edits in `Scenes/`, `Prefabs/`, `ProjectSettings/`, and `Packages/`.
- Keep `Update()` free of LINQ and per-frame allocations.
- Use pooling for frequently spawned objects.
- Run Unity smoke-test steps and record the result.
