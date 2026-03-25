# Package Script Writer - AI Integration Guide

## Overview

Package Script Writer (PSW) is officially recognized as an **Umbraco Backoffice Skill** for AI-assisted development workflows. This integration enables AI assistants like Claude Code to automatically set up Umbraco installations on behalf of developers.

---

## 🤖 What is an Umbraco Backoffice Skill?

Umbraco Backoffice Skills are standardized capabilities that AI assistants can use to interact with and manage Umbraco CMS installations. PSW is one of the official skills, enabling AI to:

- Generate Umbraco installation scripts
- Execute setup commands automatically
- Configure projects with specific packages
- Handle unattended installations
- Manage database configurations

**Official Repository**: https://github.com/umbraco/Umbraco-CMS-Backoffice-Skills

---

## 🎯 Use Cases

### 1. Conversational Development

**User**: *"Set up a new Umbraco site with uSync and SQLite"*

**AI Assistant**: *Uses PSW skill to automatically execute:*
```bash
export PATH="$PATH:$HOME/.dotnet/tools" && psw -d \
    -n MyProject -s MySolution -u \
    --database-type SQLite \
    -p "uSync" \
    --admin-email admin@test.com \
    --admin-password SecurePass1234 \
    --auto-run
```

### 2. Complex Multi-Step Workflows

**User**: *"Create three Umbraco sites: dev, staging, and production, each with different configurations"*

**AI Assistant**: Uses PSW skill to generate and execute three separate configurations automatically.

### 3. Learning and Experimentation

**User**: *"I want to try the Bootstrap Starter Kit. Set it up for me."*

**AI Assistant**: Uses PSW skill to install the template, configure the project, and start the server - all from a single request.

---

## 🔧 Technical Details

### Command Structure for AI Integration

The AI skill uses a specific command structure optimized for automated workflows:

```bash
export PATH="$PATH:$HOME/.dotnet/tools" && psw -d \
    -n ProjectName \
    -s SolutionName \
    -u \
    --database-type SQLite \
    --admin-email admin@test.com \
    --admin-password SecurePass1234 \
    --auto-run
```

### Critical Flags for AI Workflows

| Flag | Purpose | Required |
|------|---------|----------|
| `-d, --default` | Generate full installation script | ✅ Yes |
| `-u, --unattended-defaults` | Use unattended install mode | Recommended |
| `--auto-run` | Execute script immediately | For automation |
| `--database-type` | Specify database (SQLite, LocalDb, etc.) | Recommended |
| `--admin-email` | Set admin email | With `-u` |
| `--admin-password` | Set admin password | With `-u` |

### Background Execution

AI assistants run PSW with `run_in_background: true` because Umbraco operates as a long-running web server:

```
The AI monitors the output for:
- Listening port notification
- Server readiness
- Error messages
```

### PATH Management

The skill ensures PSW is available by adding the .NET tools directory to PATH:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

This handles systems where global .NET tools aren't in the default PATH.

---

## 📚 Official Documentation

The complete skill documentation is maintained in Umbraco's official repository:

**Direct Link**: https://github.com/umbraco/Umbraco-CMS-Backoffice-Skills/blob/main/plugins/umbraco-backoffice-skills/skills/package-script-writer/SKILL.md

### Key Sections in Official Docs:

1. **Installation Verification**: How AI checks if PSW is available
2. **Installation Process**: Steps to install PSW if not found
3. **Non-Interactive Usage**: Command structure for automation
4. **Package Installation**: Syntax for adding packages
5. **Monitoring**: How AI monitors server startup
6. **Additional Commands**: History, versions, cache management

---

## 🎭 Demo Talking Points

When presenting the AI integration:

### Opening
*"PSW isn't just a CLI tool you run manually - it's being integrated into AI-powered development workflows. Umbraco officially recognizes PSW as a backoffice skill."*

### The Big Picture
*"Imagine describing what you want to an AI, and it uses PSW to set up your entire Umbraco environment. That's not the future - it's happening now."*

### Technical Excellence
*"The skill handles everything: PATH management, installation verification, background execution, and server monitoring. It's a complete, production-ready integration."*

### Official Recognition
*"This is in Umbraco's official repository. PSW is recognized as a core capability for AI-assisted Umbraco development."*

### Future Vision
*"This is where development is heading. From natural language conversation to running production-ready Umbraco sites - all automated."*

---

## 🚀 Live Demo Script

### Option 1: Show the Command Structure

```bash
# This is what an AI assistant executes when you ask for an Umbraco site
export PATH="$PATH:$HOME/.dotnet/tools" && psw -d \
    -n AIGeneratedProject \
    -s AIGeneratedSolution \
    -u \
    --database-type SQLite \
    --admin-email admin@test.com \
    --admin-password SecurePass1234 \
    --auto-run
```

**Talking Points**:
- *"Notice the `-d` flag - required for full script generation"*
- *"The `--auto-run` flag means complete automation"*
- *"PATH management ensures it works everywhere"*
- *"Background execution handles long-running servers"*

### Option 2: Show the Documentation

Open the GitHub URL and walk through:

```
https://github.com/umbraco/Umbraco-CMS-Backoffice-Skills/blob/main/plugins/umbraco-backoffice-skills/skills/package-script-writer/SKILL.md
```

**Talking Points**:
- *"This is the official skill documentation"*
- *"Maintained by Umbraco in their official repo"*
- *"Used by AI assistants to automate Umbraco setup"*
- *"Complete workflow from verification to execution"*

### Option 3: Explain the Workflow

Walk through the three-step process:

1. **Verify Installation**
   - AI checks if PSW is available: `psw --version`
   - If not found, explains PATH setup
   - If still not found, installs it: `dotnet tool install --global PackageScriptWriter.Cli`

2. **Execute Command**
   - AI constructs the appropriate command with all flags
   - Runs it in the background
   - Monitors output for completion

3. **Monitor Startup**
   - AI watches for "Listening on port..." message
   - Verifies server responds on that port
   - Reports readiness to user

---

## 🌟 Why This Matters

### For Developers
- **Lower barrier to entry**: Describe what you want, AI handles the setup
- **Faster experimentation**: Try different configurations instantly
- **Learning tool**: See exactly what commands AI uses
- **Time savings**: No more remembering complex command syntax

### For Teams
- **Standardization**: AI always uses best practices
- **Onboarding**: New developers get set up instantly
- **Documentation**: Natural language requests are self-documenting
- **Consistency**: Same setup process every time

### For Umbraco Ecosystem
- **Innovation**: Pushes the boundary of AI-assisted development
- **Accessibility**: Makes Umbraco more approachable for beginners
- **Tooling**: Shows Umbraco's commitment to modern developer tools
- **Community**: Opens possibilities for more AI skills

---

## 🔗 Related Resources

### PSW Resources
- **PSW GitHub**: https://github.com/prjseal/Package-Script-Writer
- **PSW Website**: https://psw.codeshare.co.uk
- **PSW NuGet**: https://www.nuget.org/packages/PackageScriptWriter.Cli/

### Umbraco AI Resources
- **Backoffice Skills Repo**: https://github.com/umbraco/Umbraco-CMS-Backoffice-Skills
- **PSW Skill Doc**: Direct link to SKILL.md (see above)
- **Other Skills**: Browse other capabilities in the same repo

### AI Development
- **Claude Code**: Anthropic's AI-powered coding assistant
- **GitHub Copilot**: AI pair programming
- **Cursor**: AI-first code editor

---

## 💡 Ideas for Demo Enhancement

### 1. Side-by-Side Comparison

**Manual Process** (Left screen):
```bash
# Developer types all these commands manually
dotnet tool install -g PackageScriptWriter.Cli
psw -d -n MyProject -s MySolution -u ...
```

**AI-Assisted** (Right screen):
```
User: "Set up Umbraco with uSync"
AI: [Executes automatically]
```

### 2. Complex Scenario

Show how AI can handle multi-step requests:

**User Request**:
*"Create two Umbraco sites: one with the Clean Starter Kit and uSync, another with the Bootstrap Starter Kit and Diplo.GodMode. Use SQLite for both."*

**AI Response**:
- Parses the request
- Generates two separate PSW commands
- Executes both in sequence
- Reports when both are ready

### 3. Error Handling

Show how AI handles issues:

**Scenario**: PSW not installed

**AI Response**:
1. Detects PSW is missing
2. Installs it automatically
3. Continues with the original request
4. No user intervention needed

---

## 📊 Metrics & Benchmarks

### Time Savings

| Task | Manual | AI-Assisted | Savings |
|------|--------|-------------|---------|
| Basic setup | ~2 minutes | ~10 seconds | 92% |
| With packages | ~5 minutes | ~15 seconds | 95% |
| Complex config | ~10 minutes | ~20 seconds | 97% |
| First-time setup | ~15 minutes | ~30 seconds | 97% |

*Note: Assumes user knows exactly what they want*

### Developer Experience

| Metric | Before PSW+AI | After PSW+AI |
|--------|---------------|--------------|
| Commands to remember | 15+ | 0 |
| Setup errors | Common | Rare |
| Onboarding time | Hours | Minutes |
| Experimentation | Tedious | Effortless |

---

## 🎯 Key Messages for AI Integration

1. **Official Recognition**: PSW is an official Umbraco backoffice skill
2. **Production Ready**: Complete workflow from verification to execution
3. **Future of Development**: AI-assisted development is here now
4. **Developer Friendly**: Natural language to running code
5. **Ecosystem Leader**: Umbraco pushing boundaries of tooling

---

## 🚀 Future Possibilities

### What Could Come Next?

1. **More Skills**: Other Umbraco operations (deployment, backups, etc.)
2. **Smarter AI**: Learn from user preferences over time
3. **Voice Commands**: Literally talk to set up Umbraco
4. **IDE Integration**: Built into VS Code, JetBrains, etc.
5. **Team Contexts**: AI learns team's standard configurations
6. **Multi-Platform**: Works across different operating systems
7. **Cloud Integration**: Automatic deployment to Azure, AWS, etc.

---

## 📞 Questions & Feedback

If you're interested in the AI integration:

- **PSW Issues**: https://github.com/prjseal/Package-Script-Writer/issues
- **Umbraco Skills**: https://github.com/umbraco/Umbraco-CMS-Backoffice-Skills/issues
- **Discussions**: Join the conversation in either repo

---

## ✨ Summary

Package Script Writer's integration with Umbraco Backoffice Skills represents a significant step forward in AI-assisted development. It demonstrates:

- ✅ **Automation**: Complete setup from a single request
- ✅ **Intelligence**: AI understands context and requirements
- ✅ **Reliability**: Standardized, production-ready workflows
- ✅ **Innovation**: Pushing the boundaries of developer tooling
- ✅ **Accessibility**: Making Umbraco more approachable

**This isn't just a cool demo - it's the future of how we'll build with Umbraco.**

---

*Last Updated: 2024*
*For the latest information, check the official Umbraco Backoffice Skills repository*
