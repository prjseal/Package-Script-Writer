# Package Script Writer

<div align="center">

[![Live Site](https://img.shields.io/badge/Live-psw.codeshare.co.uk-blue?style=for-the-badge)](https://psw.codeshare.co.uk)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

**Generate customized installation scripts for Umbraco CMS projects**

[Live Demo](https://psw.codeshare.co.uk) · [Documentation](.github/documentation.md) · [Report Bug](https://github.com/prjseal/Package-Script-Writer/issues) · [Request Feature](https://github.com/prjseal/Package-Script-Writer/issues)

</div>

---

## 🎯 What is Package Script Writer?

Package Script Writer is a web-based tool that helps Umbraco developers quickly generate installation scripts for new projects. Simply select your template, choose packages, configure your settings, and get a ready-to-run script!

**Perfect for**:
- 🚀 Quick project setup
- 👥 Team onboarding
- 📚 Training and tutorials
- 🔄 Consistent project configurations

---

## ✨ Features

- **Template Selection** - Choose from Umbraco official templates and community packages
- **Package Browser** - Browse 150+ packages from the Umbraco Marketplace
- **Version Control** - Select specific package versions or use latest
- **Unattended Install** - Pre-configure database and admin credentials
- **Docker Support** - Optional Dockerfile and Docker Compose generation
- **Shareable URLs** - All configuration encoded in the URL for easy sharing
- **Multiple Formats** - Generate multi-line scripts or one-liners
- **Syntax Highlighting** - Clean, readable output with syntax highlighting
- **Swagger/OpenAPI** - Interactive API documentation with Swagger UI

---

## 🚀 Quick Start

### For Users

1. Visit **[psw.codeshare.co.uk](https://psw.codeshare.co.uk)**
2. Select your Umbraco template and version
3. Choose any packages you want to include
4. Configure your project settings
5. Click "Generate" and copy your script!

**Example Output**:
```bash
# Ensure we have the version specific Umbraco templates
dotnet new install Umbraco.Templates::14.3.0 --force

# Create solution/project
dotnet new sln --name "MySolution"
dotnet new umbraco --force -n "MyProject" --development-database-type SQLite
dotnet sln add "MyProject"

#Add Packages
dotnet add "MyProject" package Umbraco.Community.BlockPreview --version 1.6.0

dotnet run --project "MyProject"
```

### For Developers

**Prerequisites**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

```bash
# Clone the repository
git clone https://github.com/prjseal/Package-Script-Writer.git
cd Package-Script-Writer

# Run the application
dotnet watch run --project ./src/PSW/

# Open browser to https://localhost:5001
```

That's it! No database setup required - the application is completely stateless.

---

## 📖 Documentation

Comprehensive documentation is available in the [`.github/`](.github/) directory:

| Document | Description |
|----------|-------------|
| **[Documentation Index](.github/documentation.md)** | Main documentation hub with overview |
| [Architecture](.github/architecture.md) | System architecture and design patterns |
| [Process Flows](.github/process-flows.md) | Visual diagrams of all processes |
| [Services](.github/services.md) | Business logic layer documentation |
| [API Reference](.github/api-reference.md) | Complete REST API documentation |
| [Frontend](.github/frontend.md) | JavaScript and UI architecture |
| [Data Models](.github/data-models.md) | All data structures and models |
| [Configuration](.github/configuration.md) | Settings and configuration guide |
| [Security](.github/security.md) | Security measures and best practices |
| [Development Guide](.github/development-guide.md) | Setup, testing, and contributing |
| [Testing Guide](.github/testing.md) | Integration tests, API testing, and CI/CD |

**Start here**: [📚 Read the Documentation](.github/documentation.md)

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Language**: C# 13
- **Frontend**: Razor Pages + Vanilla JavaScript
- **UI**: Bootstrap 5
- **Caching**: In-memory (IMemoryCache)
- **APIs**: NuGet.org, Umbraco Marketplace
- **Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Testing**: xUnit, FluentAssertions, ASP.NET Core Testing
- **CI/CD**: GitHub Actions

**Why no database?** The application is intentionally stateless for simplicity, security, and easy deployment.

---

## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Raise an issue** - For bugs or features, create an issue first
2. **Discuss** - Let's agree on the approach before coding
3. **Fork & branch** - Fork the repo and create a feature branch
4. **Code** - Make your changes following our code style
5. **Test** - Test your changes thoroughly
6. **Submit PR** - Create a pull request with clear description

**See the [Development Guide](.github/development-guide.md) for detailed contributing instructions.**

### Development Commands

```bash
# Run with hot reload
dotnet watch run --project ./src/PSW/

# Run integration tests
dotnet test

# Build for production
dotnet publish ./src/PSW/PSW.csproj -c Release -o ./publish

# Format code
dotnet format ./src/PSW/PSW.csproj
```

---

## 🧪 Testing

### Integration Tests

The project includes comprehensive integration tests that validate all API endpoints using **xUnit**, **HttpClient**, and **FluentAssertions**.

```bash
# Run all integration tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Coverage**:
- ✅ Script generation with various configurations
- ✅ Package version retrieval
- ✅ Cache clearing functionality
- ✅ API health checks
- ✅ Error handling and validation

**See [Integration Tests README](src/PSW.IntegrationTests/README.md) for detailed testing documentation.**

---

### Continuous Integration

Every pull request automatically runs all tests via **GitHub Actions**:

- 🔄 Automated test execution on all PRs
- ✅ Build verification across multiple environments
- 📊 Test result reporting
- 🚫 PR merge blocked if tests fail

**GitHub Actions Workflow**: `.github/workflows/integration-tests.yml`

---

### API Testing

#### Swagger UI (Interactive Documentation)

The easiest way to explore and test the API is through the built-in Swagger UI:

```bash
# Start the application
dotnet watch run --project ./src/PSW/

# Open your browser to:
https://localhost:5001/api/docs
```

Swagger UI provides:
- 📖 Interactive API documentation with OpenAPI annotations
- 🧪 Built-in request testing
- 📝 Request/response examples
- 🔍 Schema exploration
- 📄 Complete API specification

#### REST Client (VS Code)

The repository includes a `Api Request/API Testing.http` file for testing with the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) VS Code extension.

```bash
# Start the application
dotnet watch run --project ./src/PSW/

# Open Api Request/API Testing.http in VS Code
# Click "Send Request" above each endpoint
```

**See [API Reference](.github/api-reference.md) for complete endpoint documentation.**

---

## 📦 Project Structure

```
Package-Script-Writer/
├── src/
│   ├── PSW/                        # Main application
│   │   ├── Components/            # View Components
│   │   ├── Controllers/           # MVC & API Controllers
│   │   ├── Services/              # Business logic
│   │   ├── Models/                # Data models
│   │   ├── Views/                 # Razor views
│   │   └── wwwroot/               # Static files (CSS, JS, images)
│   └── PSW.IntegrationTests/      # Integration test project
│       ├── ScriptGeneratorApiTests.cs
│       └── CustomWebApplicationFactory.cs
├── .github/
│   ├── workflows/                  # GitHub Actions workflows
│   └── *.md                        # Documentation
└── README.md                       # This file
```

**See [Architecture](.github/architecture.md) for detailed structure.**

---

## 🔒 Security

The application implements multiple security measures:

- ✅ **Security Headers** - X-Frame-Options, CSP, etc.
- ✅ **HTTPS/HSTS** - Forced HTTPS connections
- ✅ **Input Validation** - Client and server-side
- ✅ **No Data Storage** - Stateless, no user data stored
- ✅ **Regular Updates** - Dependencies kept current

**See [Security](.github/security.md) for complete security documentation.**

---

## 🐛 Troubleshooting

### Common Issues

**Port already in use**:
```bash
# Run on different port
dotnet run --urls "https://localhost:5555"
```

**Certificate errors**:
```bash
# Trust development certificate
dotnet dev-certs https --trust
```

**Cache issues**:
```bash
# Clear cache via API
curl https://localhost:5001/api/scriptgeneratorapi/clearcache
```

**See [Development Guide](.github/development-guide.md#troubleshooting) for more solutions.**

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Paul Seal**

- Website: [codeshare.co.uk](https://codeshare.co.uk)
- Twitter: [@codeshare](https://twitter.com/codeshare)
- GitHub: [@prjseal](https://github.com/prjseal)

---

## 🌟 Acknowledgments

- **Umbraco Community** - For the amazing CMS and package ecosystem
- **Microsoft** - For ASP.NET Core and .NET
- **Contributors** - Everyone who has contributed to this project

---

## 📊 Project Stats

![GitHub stars](https://img.shields.io/github/stars/prjseal/Package-Script-Writer?style=social)
![GitHub forks](https://img.shields.io/github/forks/prjseal/Package-Script-Writer?style=social)
![GitHub issues](https://img.shields.io/github/issues/prjseal/Package-Script-Writer)
![GitHub pull requests](https://img.shields.io/github/issues-pr/prjseal/Package-Script-Writer)

---

## 🔗 Useful Links

- **Live Site**: [psw.codeshare.co.uk](https://psw.codeshare.co.uk)
- **Documentation**: [Technical Documentation](.github/documentation.md)
- **Issues**: [GitHub Issues](https://github.com/prjseal/Package-Script-Writer/issues)
- **Umbraco**: [docs.umbraco.com](https://docs.umbraco.com)
- **Marketplace**: [marketplace.umbraco.com](https://marketplace.umbraco.com)

---

<div align="center">

**⭐ If this project helps you, consider giving it a star! ⭐**

Made with ❤️ for the Umbraco Community

</div>
