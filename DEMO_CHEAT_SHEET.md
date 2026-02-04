# PSW CLI Demo - Quick Reference Cheat Sheet

**For: Umbraco Sydney Meetup**

---

## 🎯 Demo Order (10-12 minutes total)

| # | Scenario | Time | Command |
|---|----------|------|---------|
| 1 | 30-Second Win | 30s | `psw --default` |
| 2 | Interactive Magic | 3-4min | `psw` → Create new script |
| 3 | Power User | 2min | CLI automation examples |
| 4 | Team Templates | 2min | `psw template save/load` |
| 5 | History Replay | 1min | `psw history list/rerun` |

---

## 📋 Commands Ready to Copy-Paste

### Scenario 1: Quick Win
```bash
psw --default
```

### Scenario 2: Interactive
```bash
psw
```
Then select: Create new script → Clean/Bootstrap Kit → Add packages (uSync, Diplo.GodMode, BlockPreview)

### Scenario 3: Power User

**Quick Setup:**
```bash
psw -p "uSync|17.0.0,Diplo.GodMode" -n UmbracoSydney -s SydneyMeetup --database-type SQLite
```

**Full Automation:**
```bash
psw -p "uSync,Diplo.GodMode,Umbraco.Community.BlockPreview" \
    -n ProductionSite -s MySolution -u \
    --database-type SQLite \
    --admin-email "admin@example.com" \
    --admin-password "SecurePass123!" \
    --add-docker --auto-run
```

**Client Project:**
```bash
psw --template "Bootstrap Starter Kit" -p "uSync,Diplo.GodMode" \
    -n ClientWebsite -u --database-type SQLite \
    --admin-email "dev@agency.com" --admin-password "TempPass123!" \
    --auto-run
```

### Scenario 4: Templates

**Save:**
```bash
psw -p "uSync|17.0.0,Diplo.GodMode,Umbraco.Community.BlockPreview" \
    -n MyProject -s MySolution -u --database-type SQLite \
    --admin-email "admin@company.com" \
    template save TeamStandard \
    --template-description "Company standard setup" \
    --template-tags "company,standard"
```

**List & Load:**
```bash
psw template list
psw template load TeamStandard
psw template load TeamStandard -n NewProject --auto-run
```

**Community:**
```bash
psw community list
psw community load "Full-Featured Blog"
```

### Scenario 5: History
```bash
psw history list
psw history show 5
psw history rerun 5
psw history rerun 5 -n NewProjectName --auto-run
```

### Bonus: Versions Table
```bash
psw versions
```

---

## 💡 Key Talking Points

| Scenario | Key Message |
|----------|-------------|
| **Default** | "Zero to Umbraco in 30 seconds" |
| **Interactive** | "Beautiful UI, 500+ packages, production-ready" |
| **CLI** | "Perfect for CI/CD, fully automated, reproducible" |
| **Templates** | "Standardize teams, share configs, onboard faster" |
| **History** | "Never lose a configuration, replay anytime" |

---

## 🎤 Opening Line

*"How many of you have forgotten to install your favorite packages until after you'd started coding? Yeah, me too. That's why I built this."*

---

## 🎬 Demo Tips

- ✅ Cache primed: Run `psw` once before demo
- ✅ Terminal size: Big enough for tables
- ✅ Verbose mode: `PSW_VERBOSE=1 psw` to show details
- ✅ Backup: Have pre-generated scripts ready
- ✅ Network: Test connection beforehand

---

## ⚠️ Common Pitfalls

- 🚫 Don't type passwords live (copy-paste!)
- 🚫 Don't typo package names (copy-paste!)
- 🚫 Watch terminal size for tables
- 🚫 Narrate during API waits

---

## 📦 Popular Packages to Mention

- uSync (deployment/sync)
- Diplo.GodMode (debugging)
- Umbraco.Community.BlockPreview (content editors)
- Our.Umbraco.GMaps (Google Maps)
- Umbraco.Community.Contentment (content apps)

---

## 🌐 Share at End

```
Installation:
dotnet tool install -g PackageScriptWriter.Cli

Website: https://psw.codeshare.co.uk
GitHub: https://github.com/prjseal/Package-Script-Writer
```

---

## 🎊 Closing Line

*"Whether you're learning Umbraco, building production sites, or managing a team, PSW has you covered. It's free, open-source, and available now. Try it and let me know what you think!"*

---

## 📱 Emergency Backup

If demo fails:
1. Show pre-generated scripts from files
2. Walk through code on GitHub
3. Show web version: https://psw.codeshare.co.uk
4. Have slides ready

---

## ✨ Demo Passwords (For Convenience)

Use these for demo (don't use in production!):
- `SecurePass123!`
- `TempPass123!`
- `DemoPass123!`

---

## 🎯 Success Metrics

Demo is successful if attendees:
- [ ] See the speed (30-second default)
- [ ] Experience the beauty (interactive UI)
- [ ] Understand automation (CLI mode)
- [ ] Get the team value (templates)
- [ ] Want to try it themselves!

---

**You've got this! 🚀**
