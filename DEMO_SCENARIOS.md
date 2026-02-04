# Package Script Writer CLI - Demo Scenarios for Umbraco Sydney Meetup

This document outlines compelling demo scenarios that showcase the Package Script Writer CLI tool's capabilities in both interactive and non-interactive modes.

---

## 🎯 Demo Flow Overview

The demo progresses from quick wins to advanced features, showing real-world scenarios that resonate with Umbraco developers.

**Suggested Flow**:
1. **Quick Win** - Default script (30 seconds)
2. **Interactive Magic** - Full interactive workflow (3-4 minutes)
3. **Power User** - CLI automation (2 minutes)
4. **Team Collaboration** - Templates (2 minutes)
5. **Time Saver** - History feature (1 minute)

**Total Demo Time**: ~10-12 minutes

---

## 🚀 Scenario 1: "The 30-Second Win" (Default Script)

**Story**: *"I need an Umbraco site, and I need it NOW!"*

**Why it's compelling**: Shows immediate value with zero configuration.

### Demo Commands

```bash
# Just type 'psw' with --default flag
psw --default

# Watch it generate a complete script in seconds
# Script includes: template installation, project creation, ready to run!
```

### Key Talking Points

- "This is the fastest way to get started with Umbraco"
- "No questions, no configuration, just a working script"
- "Uses latest Umbraco version with SQLite (perfect for prototyping)"
- Shows the generated script with `cat` or your editor
- Optional: Actually execute the script to show it works!

### Expected Output

A complete bash/PowerShell script that:
- Installs Umbraco.Templates
- Creates a new solution and project
- Uses SQLite (no SQL Server required)
- Ready to run immediately

---

## 🎨 Scenario 2: "The Interactive Experience" (Full Guided Workflow)

**Story**: *"Let me build a proper blog with all the tools I need"*

**Why it's compelling**: Showcases the beautiful Spectre.Console UI and guides users through options they didn't know existed.

### Demo Flow

```bash
psw
```

Then navigate through:

1. **Main Menu** - Select "Create new script"

2. **Template Selection**
   - Choose "Clean Starter Kit" (or "Bootstrap Starter Kit")
   - Select latest version
   - *"Look at how it auto-fetches available versions!"*

3. **Project Configuration**
   - Project Name: `UmbracoSydneyBlog`
   - Solution Name: `SydneyBlog`

4. **Package Selection** - *This is where it gets exciting!*
   - Search for packages
   - Select:
     - `uSync` (for deployment)
     - `Diplo.GodMode` (for debugging)
     - `Umbraco.Community.BlockPreview` (for content editors)
   - *"500+ packages available from the Marketplace!"*

5. **Options Configuration**
   - Database: SQLite (for simplicity)
   - Unattended Install: Yes
     - Admin Email: `admin@sydneyumbraco.com`
     - Password: (use a demo password)
   - Docker: Yes
     - *"Perfect for containerized deployments!"*

6. **Generate & Execute**
   - Generate script
   - View it
   - Option to save and run immediately

### Key Talking Points

- "Beautiful terminal UI with fuzzy search"
- "Auto-completion and version selection"
- "All 500+ Marketplace packages at your fingertips"
- "Unattended install means no manual setup - perfect for CI/CD"
- "Docker support out of the box"

### Why This Scenario Wins

- Shows the **breadth** of options without overwhelming
- Demonstrates **real packages** that Sydney devs know and love
- The **visual appeal** of Spectre.Console is impressive
- Builds a **production-ready** configuration

---

## ⚡ Scenario 3: "The Power User" (CLI Automation)

**Story**: *"I'm scripting our onboarding process - no time for interactive prompts!"*

**Why it's compelling**: Shows how PSW fits into professional workflows, CI/CD, and team automation.

### Demo Commands

#### Example A: Quick Package Setup

```bash
# Install Umbraco with uSync and Diplo.GodMode in one command
psw -p "uSync|17.0.0,Diplo.GodMode" \
    -n UmbracoSydney \
    -s SydneyMeetup \
    --database-type SQLite
```

**Talking Point**: *"This is your Friday afternoon setup - specific versions, no questions asked."*

#### Example B: Full CI/CD Pipeline Script

```bash
# Production-ready setup with everything configured
psw -p "uSync,Our.Umbraco.GMaps,Umbraco.Community.BlockPreview" \
    -n ProductionSite \
    -s MySolution \
    -u \
    --database-type SQLServer \
    --admin-email "admin@example.com" \
    --admin-password "SecurePass123!" \
    --connection-string "Server=localhost;Database=UmbracoDb;..." \
    --add-docker \
    --auto-run
```

**Talking Points**:
- *"Perfect for GitHub Actions, Azure DevOps, or any CI/CD pipeline"*
- *"Fully automated - no human intervention needed"*
- *"Version pinning ensures consistency across environments"*
- *"The --auto-run flag even executes the script for you!"*

#### Example C: Bootstrap a New Client Project

```bash
# Use the Bootstrap Starter Kit for a quick client site
psw --template "Bootstrap Starter Kit" \
    -p "uSync,Diplo.GodMode" \
    -n ClientWebsite \
    -u --database-type SQLite \
    --admin-email "dev@agency.com" \
    --admin-password "TempPass123!" \
    --auto-run
```

**Talking Point**: *"Agency workflow - spin up a new client site in under 2 minutes."*

### Why This Scenario Wins

- Shows **professional use cases** that solve real problems
- Demonstrates **reproducibility** and **automation**
- Perfect for **DevOps-minded** developers
- One-liner commands are **shareable** and **documentable**

---

## 👥 Scenario 4: "The Team Template" (Template System)

**Story**: *"Our team has a standard setup - let's make it reusable!"*

**Why it's compelling**: Shows how teams can standardize and share configurations.

### Demo Flow

#### Step 1: Create Your Team Template

```bash
# Create your ideal setup first (using CLI or interactive mode)
psw -p "uSync|17.0.0,Diplo.GodMode,Umbraco.Community.BlockPreview" \
    -n MyProject \
    -s MySolution \
    -u --database-type SQLite \
    --admin-email "admin@company.com" \
    template save TeamStandard \
    --template-description "Company standard Umbraco setup with essential packages" \
    --template-tags "company,standard,umbraco14"
```

**Talking Point**: *"Now every developer in your company can use this exact configuration."*

#### Step 2: Share with Team

Show the saved template file location:

```bash
# Templates are saved in your profile
# Windows: %USERPROFILE%\.psw\templates\
# macOS/Linux: ~/.psw/templates/

psw template list
```

**Talking Point**: *"Templates are just YAML files - check them into Git, share via Slack, email them to contractors."*

#### Step 3: Load Template Later

```bash
# New team member joins - instant setup!
psw template load TeamStandard

# Or in CLI mode
psw template load TeamStandard --project-name NewProject --auto-run
```

**Talking Point**: *"Onboarding new developers? One command and they're running the company standard."*

#### Step 4: Override Template Values

```bash
# Load template but customize project name
psw template load TeamStandard \
    -n ClientProjectAlpha \
    -s ClientAlpha \
    --admin-email "dev@client.com"
```

**Talking Point**: *"Templates are starting points - override anything you need per project."*

### Advanced: Community Templates

```bash
# Browse community-shared templates
psw community list

# Load a community template
psw community load "Full-Featured Blog"

# Or in one command:
psw --community-template "Full-Featured Blog" -n MyBlog --auto-run
```

**Talking Point**: *"Learn from the community - see how others are configuring their Umbraco projects."*

### Why This Scenario Wins

- Addresses **real pain point**: team standardization
- Shows **collaboration features**
- Templates are **portable** and **versionable**
- Community templates demonstrate **ecosystem benefits**

---

## 📊 Scenario 5: "The History Replay" (History Feature)

**Story**: *"What was that script I generated last week? I need it again!"*

**Why it's compelling**: Shows that PSW remembers everything and makes it reusable.

### Demo Flow

#### View History

```bash
# List all previously generated scripts
psw history list
```

Shows a beautiful table with:
- Entry number
- Project name
- Packages used
- Timestamp
- Template used

#### Show Script Details

```bash
# View details of a specific entry
psw history show 5
```

**Talking Point**: *"Every script is automatically saved - never lose your configuration."*

#### Rerun a Previous Script

```bash
# Exactly replicate a previous setup
psw history rerun 5

# Or rerun with modifications
psw history rerun 5 -n NewProjectName --auto-run
```

**Talking Point**: *"Client wants the same setup as Project X? Just replay it."*

### Why This Scenario Wins

- Shows **attention to developer workflow**
- Eliminates **"I forgot what I did"** problems
- Makes **experimentation safe** (you can always go back)
- Automatic **documentation** of all your setups

---

## 🎭 Bonus Scenarios (If Time Permits)

### Scenario 6: "The Umbraco Versions Expert"

Show the versions table feature:

```bash
psw versions
```

**Talking Point**: *"Need to know which Umbraco versions are LTS vs STS? PSW has you covered."*

Shows a beautiful table with:
- Version numbers
- Support type (LTS/STS)
- Release dates
- Support end dates

### Scenario 7: "The Offline Developer"

Demonstrate caching:

```bash
# First run - fetches from API
psw

# Second run - uses cache (much faster!)
psw

# Clear cache if needed
psw --clear-cache
```

**Talking Point**: *"Works offline after first fetch - perfect for trains, planes, and coffee shops with bad Wi-Fi."*

### Scenario 8: "The Security Conscious"

Show command validation:

```bash
# Try to generate a script with dangerous commands
# (PSW's command validator will catch it)
```

**Talking Point**: *"PSW validates every command - no rm -rf surprises!"*

---

## 🎬 Demo Tips & Tricks

### Before the Demo

1. **Pre-cache packages**: Run `psw` once before demo to cache the package list
2. **Prepare terminals**: Have 2-3 terminal windows ready
3. **Clear history**: `psw history clear` for a clean slate (or keep some entries to show the feature)
4. **Test auto-run**: Make sure `--auto-run` works in your environment

### During the Demo

1. **Use verbose mode**: `psw --verbose` or `PSW_VERBOSE=1 psw` to show what's happening
2. **Show the files**: Use `cat` or `code` to show generated scripts
3. **Compare outputs**: Show bash vs PowerShell output side by side
4. **Highlight colors**: The Spectre.Console colors look amazing - let them shine!

### Common Demo Pitfalls to Avoid

1. **Network issues**: Have a cached run ready as backup
2. **Typos in package names**: Use copy-paste for CLI demos
3. **Long waits**: While API calls happen, narrate what's happening
4. **Terminal size**: Make sure your terminal is big enough for tables

### Backup Plan

If live demo fails:
1. Show pre-recorded terminal recording (use `asciinema`)
2. Walk through generated scripts from file system
3. Show the code on GitHub
4. Show the web version at https://psw.codeshare.co.uk

---

## 🎯 Key Messages to Drive Home

1. **Speed**: "From zero to Umbraco in under 60 seconds"
2. **Flexibility**: "Interactive when learning, automated when you know what you want"
3. **Team-Friendly**: "Share templates, standardize setups, onboard faster"
4. **Production-Ready**: "CI/CD pipelines, Docker support, unattended installs"
5. **Safety**: "Command validation prevents mistakes, history keeps you safe"
6. **Community**: "500+ packages, community templates, growing ecosystem"

---

## 📋 Demo Checklist

- [ ] CLI tool installed and working (`dotnet tool install -g PackageScriptWriter.Cli`)
- [ ] Cache primed (run `psw` once)
- [ ] Terminal configured (size, colors, font)
- [ ] Backup slides/recordings ready
- [ ] Example scripts pre-generated (as backup)
- [ ] Network connection stable
- [ ] Demo project names chosen (Sydney-themed!)
- [ ] Passwords prepared (don't type them live!)
- [ ] Community templates reviewed
- [ ] History populated with some examples

---

## 🎤 Suggested Narrative Arc

### Opening (30 seconds)
"How many of you have set up a new Umbraco project in the last month? Show of hands? Okay, and how many of you forgot to install your favorite packages until after you'd already started coding? Yeah, me too. That's why I built this."

### Act 1: The Quick Win (1 minute)
"Let's start with the absolute fastest way to get Umbraco running..."
[Demo Scenario 1]

### Act 2: The Beauty (3-4 minutes)
"But what if you want more control? Let me show you the interactive mode..."
[Demo Scenario 2]

### Act 3: The Power (2 minutes)
"Now, interactive is great, but what about automation? What about CI/CD pipelines?"
[Demo Scenario 3]

### Act 4: The Team Play (2 minutes)
"Here's where it gets really useful for teams..."
[Demo Scenario 4]

### Act 5: The Safety Net (1 minute)
"And finally, because we all forget things..."
[Demo Scenario 5]

### Closing (30 seconds)
"So whether you're learning Umbraco, building production sites, or managing a team, PSW has you covered. It's free, open-source, and available now on NuGet. Give it a try and let me know what you think!"

---

## 🌐 Resources to Share

At the end of demo, share:

- **NuGet Package**: `dotnet tool install -g PackageScriptWriter.Cli`
- **Website**: https://psw.codeshare.co.uk
- **GitHub**: https://github.com/prjseal/Package-Script-Writer
- **Documentation**: Full docs in the repo
- **Community**: GitHub Discussions for questions

---

## ✨ Demo Script Quick Reference

```bash
# Scenario 1: 30-Second Win
psw --default

# Scenario 2: Interactive Experience
psw
# Then navigate through menus

# Scenario 3: Power User Examples
psw -p "uSync|17.0.0,Diplo.GodMode" -n UmbracoSydney -s SydneyMeetup
psw --template "Bootstrap Starter Kit" -p "uSync" -n ClientSite --auto-run

# Scenario 4: Team Templates
psw template save TeamStandard
psw template list
psw template load TeamStandard -n NewProject

# Scenario 5: History
psw history list
psw history show 5
psw history rerun 5

# Bonus: Versions Table
psw versions
```

---

## 🎊 After the Demo

Encourage attendees to:

1. **Try it**: Give them 5 minutes to install and run it
2. **Contribute**: Show them how to add community templates
3. **Feedback**: Direct them to GitHub Issues
4. **Share**: Tweet/post about their experience

---

## 📝 Notes Section

Use this space to add notes specific to your audience:

- Sydney Umbraco community size:
- Common packages used in Sydney:
- Local pain points to address:
- Attendee questions from chat:

---

**Good luck with your demo! You've got this! 🚀**

*Remember: The tool is impressive, but your enthusiasm and storytelling will make it memorable.*
