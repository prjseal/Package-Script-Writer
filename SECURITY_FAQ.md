# Package Script Writer - Security FAQ

Addressing security concerns about the PSW CLI tool and the `--auto-run` feature.

---

## 🔒 Quick Security Summary

**Is PSW safe to use?**

✅ **Yes** - Open source, command validation, community verified, and you control execution.

**Four Security Pillars:**
1. **Open Source Transparency** - All code visible on GitHub
2. **Command Validation** - Strict allowlist blocks dangerous operations
3. **User Control** - Choose auto-run or manual review
4. **Battle Tested** - Same code as website used by thousands

---

## 🎯 Frequently Asked Questions

### Q1: Is `--auto-run` safe?

**Short Answer**: Yes, because every command is validated before execution.

**Detailed Answer**:
- PSW validates all commands against a strict allowlist
- Only safe `dotnet` CLI commands are allowed
- Dangerous commands (file deletion, system modifications) are blocked
- If any command fails validation, the entire script is blocked
- You can review the validation logic: `src/PackageCliTool/Validation/CommandValidator.cs`

**Alternatives if you're concerned**:
```bash
# Review before running
psw -d -n MyProject -s MyProject -u --database-type SQLite \
    --admin-email admin@test.com --admin-password Pass123! \
    -o install-script.sh

# Check the contents
cat install-script.sh

# Run manually when satisfied
bash install-script.sh
```

---

### Q2: What commands are allowed?

**Allowed Commands** (Safe):
- ✅ `dotnet new install` - Install templates
- ✅ `dotnet new sln` - Create solution
- ✅ `dotnet new [template]` - Create project
- ✅ `dotnet sln add` - Add project to solution
- ✅ `dotnet add package` - Add NuGet packages
- ✅ `dotnet run` - Run the project
- ✅ `dotnet build` - Build the project
- ✅ `dotnet restore` - Restore dependencies
- ✅ `cd` - Change directory (to project folders only)
- ✅ `mkdir` - Create directories (for project structure)

**Blocked Commands** (Dangerous):
- ❌ `rm -rf` / `del /f` - File deletion
- ❌ `format` / `mkfs` - Disk formatting
- ❌ `sudo` / `runas` - Privilege escalation
- ❌ `curl | bash` - Piped remote execution
- ❌ `chmod 777` - Permission changes
- ❌ `mv /system` - System file moves
- ❌ Any command not explicitly allowed

**How It Works**:
```csharp
// Each command is matched against regex patterns
// Example from CommandValidator.cs:
patterns.Add(new Regex(@"^dotnet\s+new\s+sln"));
patterns.Add(new Regex(@"^dotnet\s+add.*package\s+[\w\.\-]+"));

// If no pattern matches → command is blocked
```

---

### Q3: How can I verify the scripts are safe?

**Four Ways to Verify**:

**1. Read the Generated Script**:
```bash
# Generate without auto-run
psw -d -n MyProject -s MyProject -u \
    --database-type SQLite \
    --admin-email admin@test.com \
    --admin-password Pass123!

# Output shows the full script
# Review every line before running
```

**2. Save to File**:
```bash
# Save to file for detailed review
psw -d -n MyProject -s MyProject -u \
    --database-type SQLite \
    --admin-email admin@test.com \
    --admin-password Pass123! \
    -o install-script.sh

# Open in your editor
code install-script.sh
# or
vim install-script.sh
```

**3. Compare with Web Version**:
```bash
# CLI version
psw -d -n MyProject -s MyProject -u --database-type SQLite

# Web version: https://psw.codeshare.co.uk
# Use same settings, compare output
# Should be identical
```

**4. Review Source Code**:
```bash
# Clone the repository
git clone https://github.com/prjseal/Package-Script-Writer.git

# Review the validation logic
cat src/PackageCliTool/Validation/CommandValidator.cs

# Review script generation logic
cat src/PSW.Shared/Services/ScriptGeneratorService.cs
```

---

### Q4: Does PSW collect my data?

**No. Zero data collection.**

**What PSW Does**:
- ✅ Generates scripts locally on your machine
- ✅ Caches package lists locally (in `~/.psw/cache/`)
- ✅ Saves history locally (in `~/.psw/history/`)
- ✅ Saves templates locally (in `~/.psw/templates/`)

**What PSW Does NOT Do**:
- ❌ No telemetry sent to any server
- ❌ No analytics tracking
- ❌ No user identification
- ❌ No phone home
- ❌ No third-party tracking services
- ❌ No cloud storage of your data

**Network Calls** (Only These):
1. **NuGet.org API** - To fetch package versions (same as `dotnet` CLI)
2. **Umbraco Marketplace API** - To fetch package list (public API)

Both are read-only operations, no data sent about you.

**Proof**:
```bash
# Monitor network activity while using PSW
# macOS/Linux:
sudo tcpdump -i any -n host api.nuget.org
sudo tcpdump -i any -n host marketplace.umbraco.com

# You'll see GET requests only, no POST data
```

---

### Q5: Can PSW install malware?

**No. Here's why:**

**1. Packages Come from Trusted Sources**:
- NuGet.org (Microsoft's official package repository)
- Umbraco Marketplace (Umbraco HQ's official repository)
- No custom repositories
- No direct downloads from URLs

**2. Same Packages as Manual Install**:
```bash
# PSW generates this:
dotnet add package uSync --version 17.0.0

# Which is identical to what you'd type manually
# Same source, same verification, same security
```

**3. Package Verification**:
- NuGet packages are signed by publishers
- Umbraco Marketplace packages are vetted
- Both use the same security as `dotnet` CLI

**4. No Custom Executables**:
- PSW doesn't download or run custom executables
- Only uses the `dotnet` CLI that's already on your system
- No bundled binaries, no external scripts

**If you're still concerned**:
- Research the packages before adding them (check NuGet, GitHub, ratings)
- Use `-o` to save the script, review package names
- Run packages through your own security scanning
- Same due diligence as any package installation

---

### Q6: What if there's a bug in the validator?

**Defense in Depth Approach:**

**Layer 1: Code Review**
- Open source on GitHub
- Community can review and report issues
- Pull requests are reviewed before merge

**Layer 2: Testing**
- Unit tests for validator logic
- Integration tests for script generation
- Manual testing before releases

**Layer 3: User Control**
```bash
# You can always review before running
psw -d ... -o script.sh
cat script.sh  # Manual inspection
bash script.sh  # Run only if satisfied
```

**Layer 4: Standard Commands Only**
- Even if validator had a bug, scripts only use `dotnet` CLI
- No custom executables that could be malicious
- Worst case: invalid dotnet command that would fail

**Layer 5: No Sudo Required**
- Everything runs in user space
- Can't modify system files without your permission
- Limited blast radius

**Report Security Issues**:
If you find a security concern, report it:
- **Email**: [maintainer's email]
- **GitHub Issues**: https://github.com/prjseal/Package-Script-Writer/issues
- Mark as security issue for priority handling

---

### Q7: Why should I trust PSW?

**Trust Factors**:

**1. Open Source**
- All code visible: https://github.com/prjseal/Package-Script-Writer
- MIT License - permissive and transparent
- No hidden functionality

**2. Community Verified**
- [X] GitHub stars
- [Y] Contributors
- Used by developers worldwide
- Active issue tracking and resolution

**3. Same as Website**
- CLI uses identical code as https://psw.codeshare.co.uk
- Website has been used by thousands of developers
- Proven track record

**4. Standard Tooling**
- Only uses official `dotnet` CLI
- Only accesses official package repositories
- No proprietary or custom tooling

**5. Transparent Operation**
- Use `--verbose` to see exactly what's happening
- All cache files are readable YAML/JSON
- History is stored in plain text

**6. Your Control**
- You decide: auto-run vs. manual review
- You can modify generated scripts
- You can audit the source code
- No lock-in, no dependencies

---

### Q8: How does PSW compare to running commands manually?

**Security Comparison**:

| Aspect | Manual Commands | PSW |
|--------|----------------|-----|
| Command Source | You type them | Generated & validated |
| Typo Risk | High | Zero (validated) |
| Dangerous Commands | Possible | Blocked by validator |
| Review Ability | Must remember each | Single script to review |
| Reproducibility | Hard to recreate | Saved in history |
| Transparency | As good as your memory | Fully documented |

**PSW is actually SAFER than manual because**:
- You can't accidentally type a dangerous command
- The complete workflow is reviewable
- It's reproducible and auditable
- Validation prevents mistakes

**Example**:
```bash
# Manual risk - typo disaster:
rm -rf / home/user/myproject  # Oops, space after /

# PSW - impossible:
# The validator would block any rm command
# Even if somehow generated, it wouldn't run
```

---

### Q9: What about the `--admin-password` in commands?

**Concern**: Passwords in command history

**Answer**: Valid concern - here's how to handle it:

**Option 1: Use Environment Variables** (Not yet implemented):
```bash
# Feature request - vote for it on GitHub!
export UMBRACO_ADMIN_PASSWORD="SecurePass123!"
psw -d ... --admin-password-env UMBRACO_ADMIN_PASSWORD
```

**Option 2: Generate Without Password**:
```bash
# Generate script without unattended install
psw -d -n MyProject -s MyProject --database-type SQLite

# Script will prompt for password during Umbraco setup
# No password in history
```

**Option 3: Edit the Script**:
```bash
# Generate with placeholder
psw -d ... -o script.sh

# Edit the script
vim script.sh
# Replace password with secure value
# Run manually
bash script.sh
```

**Option 4: Clear History After**:
```bash
# Use the password
psw -d ... --admin-password "SecurePass123!"

# Clear shell history immediately
history -c  # bash
history clear  # PowerShell
```

**Best Practice**:
- Use strong, unique passwords for demos
- Don't reuse production passwords in demos
- Consider the script as documentation, not secure storage

---

### Q10: Can I use PSW in production?

**Yes, with best practices:**

**✅ Good Use Cases**:
- **CI/CD Pipelines**: Reproducible, automated setups
- **Development Environments**: Fast, consistent provisioning
- **Team Onboarding**: Standardized configurations
- **Testing**: Spin up clean environments
- **Documentation**: Script serves as setup guide

**Best Practices for Production Use**:

**1. Version Pin Everything**:
```bash
# Specify exact versions for reproducibility
psw -d -n MyProject -s MyProject \
    -p "uSync|17.0.0,Umbraco.Forms|17.0.1" \
    --umbraco-version 17.0.0
```

**2. Review Scripts Before Running**:
```bash
# Always review first
psw -d ... -o production-setup.sh
# Review, test in staging, then run in production
```

**3. Store Scripts in Version Control**:
```bash
# Commit generated scripts to Git
git add production-setup.sh
git commit -m "Umbraco production setup script"
# Track changes, review diffs
```

**4. Use Templates for Consistency**:
```bash
# Create team template
psw -d ... template save ProductionStandard
# Share with team via Git
# Load and use: psw template load ProductionStandard
```

**5. Separate Sensitive Data**:
```bash
# Don't commit passwords
# Use environment variables or secret management
# Load from Azure Key Vault, AWS Secrets Manager, etc.
```

**6. Test in Staging First**:
```bash
# Generate script
psw -d ... -o staging-setup.sh
# Run in staging environment
# Verify everything works
# Then use same script for production
```

---

## 🛡️ Security Features Summary

### What Makes PSW Secure

| Feature | Benefit |
|---------|---------|
| **Open Source** | Transparent, auditable, community-reviewed |
| **Command Validation** | Blocks dangerous operations automatically |
| **Allowlist System** | Only approved commands can execute |
| **No Data Collection** | Complete privacy, no telemetry |
| **Local Operation** | Everything runs on your machine |
| **Standard Tools Only** | Uses official dotnet CLI |
| **User Control** | Choose automation level (auto vs manual) |
| **Reproducible** | History and templates ensure consistency |
| **No Sudo Required** | Limited permissions, user-space only |
| **Community Verified** | Active GitHub community, issue tracking |

---

## 📚 Additional Resources

### Security Documentation
- **Validator Source**: `src/PackageCliTool/Validation/CommandValidator.cs`
- **Security Guide**: `.github/cli/security.md`
- **GitHub Repo**: https://github.com/prjseal/Package-Script-Writer

### Report Security Issues
- **GitHub Issues**: https://github.com/prjseal/Package-Script-Writer/issues
- Label: `security`
- Priority handling for security concerns

### Community Discussion
- **GitHub Discussions**: Ask questions, share concerns
- **Open Issues**: Review existing security discussions

---

## 💡 Security Tips for Users

### Before First Use
1. ✅ Review the source code on GitHub
2. ✅ Read the CommandValidator.cs file
3. ✅ Star the repo to track updates
4. ✅ Check for recent security issues

### During Use
1. ✅ Use `--verbose` to see what's happening
2. ✅ Review generated scripts before running
3. ✅ Start with `-o` until you're comfortable
4. ✅ Test in non-production environments first

### Best Practices
1. ✅ Pin package versions for reproducibility
2. ✅ Store scripts in version control
3. ✅ Use templates for standardization
4. ✅ Review history before rerunning
5. ✅ Keep PSW updated for latest security fixes

---

## 🎯 Bottom Line

**PSW is designed with security as a priority:**

- **Transparent**: Open source, auditable code
- **Validated**: Strict command checking
- **Controlled**: You choose automation level
- **Trusted**: Same code as popular website
- **Standard**: Only uses official tools

**You have three options:**

1. **Trust & Automate**: Use `--auto-run` for speed (recommended after review)
2. **Verify Then Run**: Use `-o` to review before running (recommended for new users)
3. **Manual Control**: Copy output and run commands yourself

**Choose what fits your security requirements and comfort level.**

---

**Questions not answered here?**
- Open an issue: https://github.com/prjseal/Package-Script-Writer/issues
- Start a discussion: https://github.com/prjseal/Package-Script-Writer/discussions

---

*Last Updated: 2024*
*For the latest security information, check the GitHub repository*
