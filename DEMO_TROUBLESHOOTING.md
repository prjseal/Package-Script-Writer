# PSW CLI Demo - Troubleshooting Guide

Quick solutions for common issues during the live demo at Umbraco Sydney meetup.

---

## 🚨 Quick Fixes for Common Issues

### Issue: "psw: command not found"

**Symptoms**: Terminal doesn't recognize `psw` command

**Quick Fix**:
```bash
# Reinstall the tool
dotnet tool install -g PackageScriptWriter.Cli

# Or update if already installed
dotnet tool update -g PackageScriptWriter.Cli
```

**Prevention**: Test the command before the demo starts

---

### Issue: Slow API Response / Hanging

**Symptoms**: Command hangs for 30+ seconds, or times out

**Cause**: Network issues, IPv6 timeout, or API unavailable

**Quick Fix Option 1** (Use cache):
```bash
# The cache should work even if API is slow
# Just wait it out, the cache will load
```

**Quick Fix Option 2** (Use backup):
```bash
# Show pre-generated script from backup
cd ~/psw-demo-sydney
cat backup-default-script.sh
# Or open in editor
code backup-default-script.sh
```

**Quick Fix Option 3** (Verbose mode):
```bash
# Show what's happening
PSW_VERBOSE=1 psw --default
# Or on Windows
$env:PSW_VERBOSE="1"; psw --default
```

**Talking Point While Waiting**:
- "The tool is fetching 500+ packages from the Umbraco Marketplace..."
- "Normally this is fast because of caching, but looks like we hit a network hiccup"
- "This is why the tool has caching built-in - second run is always instant"

---

### Issue: Network Completely Unavailable

**Symptoms**: Cannot reach any external APIs

**Quick Fix**:
```bash
# Use the backup scripts you generated earlier
cd ~/psw-demo-sydney

# Show default script
cat backup-default-script.sh

# Show full-featured script
cat backup-full-script.sh
```

**Talking Points**:
- "I generated these beforehand, but this is exactly what PSW would create"
- "Let me walk through what this script does..."
- "The beauty of PSW is that these scripts are portable and shareable"

**Alternative**: Switch to the web version at https://psw.codeshare.co.uk

---

### Issue: Interactive Mode Crashes / Freezes

**Symptoms**: Interactive menu becomes unresponsive

**Quick Fix**:
```bash
# Press Ctrl+C to restart (PSW handles this gracefully)
# The tool will restart automatically

# Or exit and use CLI mode instead
psw -p "uSync,Diplo.GodMode" -n DemoProject -s DemoSolution
```

**Talking Point**:
- "Notice how PSW handles Ctrl+C gracefully and restarts"
- "But for automation, we have CLI mode which is perfect for scripts"

---

### Issue: Terminal Too Small for Tables

**Symptoms**: Tables look broken, wrapped text, unreadable output

**Quick Fix**:
```bash
# Resize terminal window
# Minimum recommended: 120 columns x 30 rows

# Or use a simpler command that doesn't output tables
psw --default
```

**Prevention**: Set terminal size before demo (check with `tput cols`)

---

### Issue: Colors Not Showing

**Symptoms**: No colors, just plain text

**Quick Fix**:
```bash
# Check if terminal supports colors
echo $TERM

# Try forcing colors (if your terminal supports it)
export TERM=xterm-256color

# On Windows PowerShell, ensure Windows Terminal is used
```

**Talking Point**:
- "The CLI has a beautiful colored interface, but it gracefully degrades"
- "Even without colors, all functionality works perfectly"

---

### Issue: Package Not Found

**Symptoms**: Error when trying to add a specific package

**Quick Fix**:
```bash
# Use a different, well-known package
# Safe bets that always work:
- uSync
- Diplo.GodMode
- Umbraco.Community.BlockPreview
- Umbraco.Community.Contentment

# Or just use default
psw --default
```

**Talking Point**:
- "Package names must match exactly as they appear in the Marketplace"
- "The interactive mode has fuzzy search to help with this"

---

### Issue: Template Command Fails

**Symptoms**: Cannot save or load templates

**Quick Fix**:
```bash
# Check if template directory exists
ls ~/.psw/templates/  # macOS/Linux
dir %USERPROFILE%\.psw\templates\  # Windows

# Create it manually if needed
mkdir -p ~/.psw/templates/

# Or skip template demo and show history instead
psw history list
```

---

### Issue: History Empty

**Symptoms**: `psw history list` shows no entries

**Quick Fix**:
```bash
# Generate a quick script to populate history
psw --default

# Now show history
psw history list

# Or use pre-populated history from prepare-demo script
```

---

### Issue: Auto-run Flag Doesn't Work

**Symptoms**: `--auto-run` flag doesn't execute the script

**Cause**: Security settings, execution policy, or permissions

**Quick Fix**:
```bash
# Generate without auto-run
psw -p "uSync" -n Demo

# Show the script was generated
cat Demo/install-script.sh  # or .ps1 on Windows

# Explain you would normally run it manually
```

**Talking Point**:
- "In production, you'd run this script manually or in your CI/CD pipeline"
- "The --auto-run flag is a convenience, but manual execution gives you control"

---

### Issue: Demo Password Visible

**Symptoms**: You accidentally showed a password on screen

**Quick Fix**:
```bash
# Acknowledge it with humor
"And yes, that's a demo password - never use DemoPass123! in production!"

# For next command, use a different demo password to show variety
--admin-password "AnotherDemoPass!"
```

**Prevention**: Use copy-paste for passwords, not typing

---

### Issue: Typo in Package Name

**Symptoms**: Package name misspelled in CLI mode

**Quick Fix**:
```bash
# Use up arrow to recall command
# Edit the mistake
# Re-run

# Or switch to interactive mode
psw
# Use the package selector with search
```

**Talking Point**:
- "This is why the interactive mode is great - it has fuzzy search"
- "For automation, you'd test these commands once and then they're reliable"

---

## 🎬 General Demo Recovery Strategies

### Strategy 1: Pivot to Backup Material

If live demo fails:
1. Show pre-generated scripts from `~/psw-demo-sydney/`
2. Walk through the code on GitHub
3. Show screenshots/videos (if you prepared them)
4. Switch to the web version at https://psw.codeshare.co.uk

### Strategy 2: Turn It Into a Teaching Moment

If something breaks:
- "This is a great example of why caching is important"
- "This shows why we validate commands before execution"
- "Real-world demo, real-world issues - this is development!"

### Strategy 3: Have a Backup Plan Ready

Before the demo:
- [x] Run `prepare-demo.sh` to create backups
- [x] Test all commands once
- [x] Have the web version open in a browser tab
- [x] Have GitHub repo open in another tab
- [x] Know where your backup scripts are

---

## 🔍 Pre-Demo Checklist to Avoid Issues

Run through this 5 minutes before your demo:

```bash
# 1. Test basic command
psw --version

# 2. Test network
curl -I https://marketplace.umbraco.com
curl -I https://api.nuget.org

# 3. Test cache
psw --default > /dev/null

# 4. Check terminal size
tput cols  # Should be >= 120
tput lines # Should be >= 30

# 5. Verify demo workspace
ls ~/psw-demo-sydney/

# 6. Test one interactive command
echo "Just testing..." | psw history list
```

---

## 💡 Audience Engagement During Issues

While you're fixing an issue, keep the audience engaged:

### Ask Questions
- "Who here has used Umbraco before?"
- "What packages do you typically install?"
- "Has anyone tried the web version at psw.codeshare.co.uk?"

### Share Context
- "Let me explain what PSW is doing behind the scenes..."
- "The tool actually caches everything locally for offline use"
- "This script uses Spectre.Console for the beautiful UI"

### Show Related Content
- Open GitHub repository
- Show the CLAUDE.md file
- Show documentation
- Discuss architecture

### Take Questions Early
- "While this loads, any questions?"
- "Want to see any specific features?"

---

## 🆘 Emergency Contacts & Resources

If you need help during the demo:

- **GitHub Issues**: https://github.com/prjseal/Package-Script-Writer/issues
- **Discussions**: https://github.com/prjseal/Package-Script-Writer/discussions
- **Web Version**: https://psw.codeshare.co.uk
- **NuGet Package**: https://www.nuget.org/packages/PackageScriptWriter.Cli/

---

## 🎯 Remember

**The demo doesn't have to be perfect!**

What matters:
- ✅ Showing the value of the tool
- ✅ Demonstrating the key features
- ✅ Getting people excited to try it
- ✅ Handling issues gracefully

A demo with a minor hiccup that you handle well is often more memorable than a perfect demo!

---

## 📞 Quick Reference Commands

Keep these handy for quick recovery:

```bash
# Reset everything
psw --clear-cache
rm -rf ~/psw-demo-sydney
./prepare-demo.sh

# Quick test
psw --version
psw --default

# Safe fallback commands (always work)
psw --help
psw versions
psw history list
psw template list

# Backup demo location
cd ~/psw-demo-sydney
ls -la
```

---

**You've got this! Remember: Confidence + Humor + Preparation = Great Demo! 🚀**
