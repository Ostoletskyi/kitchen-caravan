# Codex Rules

## Scope
- Edit only `Assets/Scripts/**` and `Docs/**` unless explicitly asked otherwise.
- Do not modify `Scenes/`, `Prefabs/`, `ProjectSettings/`, or `Packages/`.

## Unity Code Practices
- No LINQ inside `Update()`.
- Prefer object pooling over frequent instantiation/destruction.

## Output Requirements
- Always include a list of changed files.
- Always include Unity smoke-test steps.
