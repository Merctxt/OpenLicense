# Changelog

Generate a release changelog from the last N commits since the previous release.

## Instructions

1. Use `git log --oneline` to list the last N commits (N = number of commits since last release)
2. For each commit, read the diff with `git show --stat <hash>` to understand what changed
3. Write concise descriptions in **English only** (even if commits are in another language)
4. Group commits into **Features** and **Fixes** categories
5. Use this exact format:

```
### Features

- <short_hash> — <one-line description of what changed>
- <short_hash> — <one-line description>
- <short_hash> — <one-line description>

### Fixes

- <short_hash> — <one-line description of what changed>
- <short_hash> — <one-line description>
```

## Rules

- Always use short hashes (7 chars, matching git log --oneline output)
- Use a dash `-` before each hash: `- <hash> — <description>`
- Description is one line per commit, max ~120 chars
- Use present tense for the action (e.g. "Add...", "Fix...", "Refactor...", "Remove...", "Update...")
- Keep it factual — no fluff, no marketing language
- If a commit is purely documentation, include it under Features
- If a commit is a fix (bugfix, typo, error), include it under Fixes
- If a commit is a refactor without behavioral change, include it under Features
- Always output inside a markdown code block (````md ... ````)
- Never wrap the short hash in backticks, quotes, or any other characters — output it raw (e.g. `- 03d75c3 Enhance modals`, NOT `- \`03d75c3\` Enhance modals`)
