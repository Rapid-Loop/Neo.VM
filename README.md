# NEO Platform

[![License](https://img.shields.io/github/license/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/blob/master/LICENSE)
[![Stars](https://img.shields.io/github/stars/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/stargazers)
[![Forks](https://img.shields.io/github/forks/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/forks)
[![Issues](https://img.shields.io/github/issues/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/issues)
[![Last Commit](https://img.shields.io/github/last-commit/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/commits/master)
[![Verify Pull Request](https://github.com/Rapid-Loop/neo-platform/actions/workflows/verify.yml/badge.svg)](https://github.com/Rapid-Loop/neo-platform/actions/workflows/verify.yml)

**Modular Neo blockchain platform for C# / .NET** — maintained by [Rapid Loop](https://github.com/Rapid-Loop).

A high-performance library set for building on **Neo**: core types, virtual machine, wallets, storage, and hosting integration. Aimed at developers and enterprises that need reliable packaging and a modern .NET experience.

This repository is **compatible with Neo protocol semantics**. It is Rapid Loop’s platform packaging and engineering work — **not** an “official Neo Foundation” product unless separately stated by Neo.

**Neo docs:** [developers.neo.org](https://developers.neo.org/) · **Neo site:** [neo.org](https://neo.org)

---

## About

**neo-platform** is Rapid Loop’s actively developed Neo C# stack: modular packages, enterprise-friendly configuration and logging hooks, and a cleaner project layout for real systems.

Use it when you need:

- Core blockchain types and serialization
- Neo VM execution
- Wallet formats (e.g. NEP-6) and account management
- RocksDB-backed blockchain storage
- .NET Generic Host / CLI integration

Led by a [neo-project](https://github.com/neo-project) contributor with core Neo development experience. Rapid Loop commercial work (integration, review, support) is separate from open-source contribution — see [Rapid-Loop](https://github.com/Rapid-Loop).

---

## Packages

| Project | Role |
|---------|------|
| `Neo.Core` | Protocol types, crypto, P2P messages, serialization, VM opcodes / limits |
| `Neo.VM` | Execution engine, stack types, middleware pipeline |
| `Neo.Wallet` | Wallet abstractions, NEP-6 / dev wallets |
| `Neo.IO` | I/O helpers (hashing, bloom filter) |
| `Neo.Configuration` | Options and JSON converters for protocol / store settings |
| `Neo.Platform.Storage` | Blockchain store, snapshots, backups (RocksDB) |
| `Neo.Platform.Hosting` | Hosting, configuration, logging, command-line wiring |
| `Neo.Platform.CLI` | Sample / entry CLI host |
| `Neo.Localization` | Localized VM messages |

---

## Repository layout

```text
neo-platform/
├── src/                   # Library and host projects
├── tests/                 # Unit and integration tests
├── pkgs/                  # NuGet package output
├── .github/workflows/     # CI
├── .images/               # Package / branding assets
└── All.slnx               # Solution
```

---

## Quick start

### Clone

```bash
git clone https://github.com/Rapid-Loop/neo-platform.git
cd neo-platform
```

### Build

```bash
dotnet build All.slnx
```

### Test

```bash
dotnet test All.slnx
```

---

## Tech stack

- **Language:** C# (.NET)
- **Build:** `dotnet` / `All.slnx`
- **CI:** GitHub Actions

---

## Documentation

- [Neo developer documentation](https://developers.neo.org/)
- Public APIs use XML documentation comments (`///`) in source under `src/`
- Contribution guide: [CONTRIBUTING.md](CONTRIBUTING.md)

---

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) and the [Code of Conduct](CODE_OF_CONDUCT.md).

---

## License

**BSD-2-Clause** — see [LICENSE](LICENSE).

---

## Contact

- **Org:** [github.com/Rapid-Loop](https://github.com/Rapid-Loop)
- **Website:** [rapidloop.net](https://rapidloop.net)
- **Email:** [github@rapidloop.net](mailto:github@rapidloop.net)
