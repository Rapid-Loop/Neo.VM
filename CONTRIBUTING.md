# Contributing to neo-platform

This is a C#/.NET blockchain infrastructure project focused on NeoVM, binary/JSON serialization, CLI tooling, protocol features, and high-performance core components. All development uses **only the official `dotnet` SDK and Microsoft libraries**.

We follow strict **Framework Design Guidelines** to ensure our public APIs are intuitive, consistent, maintainable, and self-documenting.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Coding Guidelines](#coding-guidelines)
- [Testing](#testing)
- [Commit Messages](#commit-messages)
- [License](#license)

## Code of Conduct

Please read and follow our [Code of Conduct](CODE_OF_CONDUCT.md). We expect respectful, professional, and inclusive behavior at all times.

## How Can I Contribute?

### Reporting Bugs
- Search existing issues first.
- Provide a clear title, reproduction steps, expected vs. actual behavior, .NET version, OS, and (ideally) a minimal reproducible example.

### Suggesting Enhancements / New Features
- Open an issue with the **enhancement** label.
- Describe the use case, especially performance, correctness, or developer experience improvements.

### Submitting Code
- Fork the repo → create a feature branch → make changes → ensure tests pass → open a Pull Request.

## Development Setup

```bash
git clone https://github.com/Rapid-Loop/neo-platform.git
cd neo-platform
dotnet restore
dotnet build
dotnet test
```

Recommended tools:
- .NET 10 SDK (or newer)
- Visual Studio 2026+ or VS Code + C# Dev Kit

## Pull Request Process

1. Keep your branch up-to-date with `master`.
2. `dotnet build` must succeed with zero warnings.
3. All tests must pass (`dotnet test`).
4. Update or add documentation / XML comments where appropriate.
5. Open a Pull Request with a clear description and link to related issues.
6. Be responsive to review feedback.

## Coding Guidelines

We combine **project-specific rules** with the official **.NET Framework Design Guidelines**. All public and protected APIs must follow these rules.

### Project-Specific Rules (neo-platform)

- **Language & Runtime**: C# only. Target .NET 10+.
- **Dependencies**: Strictly limited to official Microsoft / `dotnet` libraries. No third-party packages unless explicitly approved by maintainers.
- **Performance Focus**: VM execution, serialization, memory handling, and hot paths must be allocation-efficient. Prefer `Span<T>`, `Memory<T>`, `MemoryMarshal`, `Unsafe`, `ref struct`, and `unsafe` code when justified.
- **Low-level Code**: Complex VM opcode logic, binary serialization, and crypto must include thorough tests (happy path + fault/edge cases).
- **Style**: Follow the rules below + the included `.editorconfig`.

### Framework Design Guidelines

#### General Design Principles
- **Test usability** of your API with developers unfamiliar with it.
- **Self-documenting API** — developers should be able to complete main scenarios without reading docs. Choose intuitive names.
- **Understand your customer** — design for the majority of users, not just experts in your team.

#### Naming Guidelines
- **DO** use `PascalCasing` for all public/protected identifiers (except parameters).
- **DO** use `camelCasing` for parameter names. Prefix generic type parameters with `T` (use `T` alone for single type parameters when appropriate).
- **DO** use `PascalCasing` or `camelCasing` for acronyms longer than two characters (`HtmlButton`, not `HTMLButton`).
- **DO NOT** use non-standard acronyms, Hungarian notation, underscores, hyphens, or contractions (`GetWindow`, not `GetWin`).
- **DO** name types and properties with **nouns or noun phrases**.
- **DO** name methods and events with **verbs or verb phrases**.
  - Events: Use present participle for “before” (`Closing`) and simple past for “after” (`Closed`).
  - **DO NOT** use `Before` or `After` prefixes.
- **DO** use these prefixes/postfixes:
  - `I` for interfaces
  - `T` for generic type parameters
  - `Exception`, `Collection`, `Dictionary`, `EventArgs`, `EventHandler`, `Attribute` as appropriate
- **DO** use plural names for flag enums and singular for non-flag enums.
- **DO** apply `FlagsAttribute` to flag enums.
- Namespace template: `<Company>.<Technology>[.<Feature>]` (e.g. `Neo.Platform.VM` or `RapidLoop.NeoPlatform.VM`).

#### General API Design
- **DO** use the most derived type for return values and the least derived type for input parameters (e.g. accept `IEnumerable<T>`, return `Collection<T>` or `ReadOnlyCollection<T>`).
- **DO** provide clear **Aggregate Components** — one or two main entry-point types per feature area.
- **DO** support **Create-Set-Call** style: default constructor + properties + simple methods.
- **DO NOT** require extensive initialization before use.
- **DO** prefer **classes over interfaces**.
- **DO NOT** seal types unless you have a strong reason.
- **DO NOT** create mutable value types unless you have a strong reason.
- **DO NOT** ship abstractions without at least one concrete implementation **and** at least one consuming API.
- **AVOID** public nested types.
- **DO** strongly prefer collections over arrays in public APIs.
- **DO NOT** use `ArrayList`, `List<T>`, `Hashtable`, or `Dictionary<K,V>` in public APIs — use `Collection<T>`, `ReadOnlyCollection<T>`, etc.
- **DO NOT** use error codes — use exceptions.
- **DO NOT** throw `Exception` or `SystemException`. Prefer specific exceptions (`ArgumentNullException`, `ArgumentOutOfRangeException`, `InvalidOperationException`, etc.).
- **DO** ensure exception messages are clear and actionable.
- **DO** use `EventHandler<T>` for events.
- **DO** prefer event-based APIs over raw delegates.
- **DO** prefer constructors over factory methods.
- **DO NOT** expose public fields — use properties.
- **DO** prefer properties, except when the operation is a conversion, expensive, has side effects, or returns different results on successive calls.
- **DO** allow properties to be set in any order.
- **DO NOT** make members virtual unless extensibility is required.
- **AVOID** finalizers.
- **DO** implement `IDisposable` on types that acquire native resources or have finalizers.
- **DO** keep parameter order and naming consistent across overloads.
- **AVOID** `out` and `ref` parameters in public APIs.

#### Additional Project Notes
- Run `dotnet analyzers` / Roslyn analyzers on your code.
- Apply `[CLSCompliant(true)]` to assemblies where appropriate.
- Write sample code for top scenarios — the first type used should be an Aggregate Component.
- Model higher-level concepts (`File`, `Directory`, `Transaction`, `Block`) rather than low-level tasks when designing public APIs.

## Testing

- Use the project’s test framework (currently `MSTest`).
- Mirror source folder structure under `tests/`.
- Cover both success paths and fault/edge cases (especially for VM opcodes, serialization, and CLI commands).
- Example:
  ```bash
  dotnet test Tests/Neo.Platform.VM.Tests
  ```

## Commit Messages

Use conventional commits:

```
<type>(<scope>): <short description>

- Optional longer explanation
- Closes #123
```

Common types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`.

## License

By contributing, you agree that your contributions will be licensed under the same terms as the project (see `LICENSE` file).

---

**Questions?**  
Open a [Discussion](https://github.com/Rapid-Loop/neo-platform/discussions) or contact the maintainers.

We appreciate high-quality, well-tested contributions that follow these guidelines. Thank you!
