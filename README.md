# NEO Platform - C# Blockchain SDK

[![License](https://img.shields.io/github/license/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/blob/master/LICENSE)
[![Stars](https://img.shields.io/github/stars/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/stargazers)
[![Forks](https://img.shields.io/github/forks/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/forks)
[![Issues](https://img.shields.io/github/issues/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/issues)
[![Last Commit](https://img.shields.io/github/last-commit/Rapid-Loop/neo-platform)](https://github.com/Rapid-Loop/neo-platform/commits/master)
[![Verify Pull Request](https://github.com/Rapid-Loop/neo-platform/actions/workflows/verify.yml/badge.svg)](https://github.com/Rapid-Loop/neo-platform/actions/workflows/verify.yml)

**Official Rapid Loop fork and enterprise-enhanced rebuild of the NEO Blockchain Smart Economy Platform.**

A modern, high-performance C# library for building on the **Neo blockchain** — designed for developers and enterprises who need reliability, scalability, and seamless integration.

---

## 🌟 About This Project

**neo-platform** is Rapid Loop's actively maintained fork of the Neo blockchain C# ecosystem. We are rebuilding and enhancing the Neo platform with enterprise-grade improvements for:

- Better performance and scalability
- Modern developer experience
- Seamless integration with existing business systems
- Enhanced security and tooling

Whether you're building decentralized applications (dApps), smart contracts, wallets, or enterprise blockchain solutions, this SDK provides the core building blocks.

**Official Neo Website**: [neo.org](https://neo.org)

---

## ✨ Key Features

- **Full Neo Compatibility** — Core blockchain, VM, cryptography, and wallet functionality
- **Enterprise Ready** — Robust configuration, logging, and integration tools
- **Modular Architecture** — Clean separation across packages:
  - `Neo.Core` — Core blockchain logic
  - `Neo.VM` — Virtual Machine
  - `Neo.Cryptography` — Secure cryptographic primitives
  - `Neo.Wallet` — Wallet management
  - And more...
- **High Performance** — Optimized for real-world usage
- **Comprehensive Testing** — Full test suite included
- **NuGet Ready** — Easy package distribution

---

## 📁 Repository Structure

```bash
neo-platform/
├── src/                  # Source code (multiple projects)
│   ├── Neo.Core/
│   ├── Neo.VM/
│   ├── Neo.Cryptography/
│   ├── Neo.Wallet/
│   └── ...
├── tests/                # Unit and integration tests
├── pkgs/                 # Package distribution
├── .github/workflows/    # CI/CD pipelines
├── .images/              # Logos and assets
└── All.slnx              # Solution file
```

---

## 🚀 Quick Start

### 1. Clone the repository
```bash
git clone https://github.com/Rapid-Loop/neo-platform.git
cd neo-platform
```

### 2. Build the solution
```bash
dotnet build All.slnx
```

### 3. Run tests
```bash
dotnet test
```

---

## 🛠️ Tech Stack

- **Language**: `C# (.NET)`
- **Build System**: `dotnet`
- **CI/CD**: `GitHub Actions`

---

## 📖 Documentation

- [Neo Official Documentation](https://developers.neo.org/)
- Check individual project folders for more details.

---

## 🤝 Contributing

We welcome contributions! See our [Contributing Guidelines](CONTRIBUTING.md) (create this file if needed).

---

## 📜 License

This project is licensed under the **BSD-2-Clause License** — see the [LICENSE](LICENSE) file for details.

---
