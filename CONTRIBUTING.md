# Contributing

## Development workflow

1. Fork the repo and create a feature branch from `main`:
   ```
   git checkout -b feat/my-change
   ```
2. Make your changes with **atomic commits** following [Conventional Commits](https://www.conventionalcommits.org/):
   - `feat:` new user-facing feature
   - `fix:` bug fix
   - `chore:` tooling, build, deps
   - `docs:` documentation only
   - `test:` adding/refactoring tests
   - `refactor:` code change that neither fixes a bug nor adds a feature
3. Run tests locally before pushing:
   ```
   dotnet test
   ```
4. Open a PR against `main`. The CI workflow must pass (build + test).

## Code style

- Follow [.editorconfig](.editorconfig). Most IDEs apply it automatically.
- Treat warnings as actionable.
- Public APIs need XML doc comments.
- New endpoints must have at least one test.

## Branch naming

- `feat/<short-name>` — new features
- `fix/<short-name>` — bug fixes
- `chore/<short-name>` — non-code changes
- `docs/<short-name>` — docs only

## Reporting bugs

Open an issue with:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Stack trace if applicable
- .NET / OS version

Thanks for contributing!
