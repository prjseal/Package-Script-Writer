#!/bin/bash

# Package Script Writer CLI - Demo Preparation Script
# Run this before your Umbraco Sydney meetup demo to ensure everything is ready

set -e  # Exit on error

echo "🎯 Package Script Writer CLI - Demo Preparation"
echo "=============================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

print_step() {
    echo -e "\n${BLUE}▶${NC} $1"
}

# Step 1: Check if PSW CLI is installed
print_step "Step 1: Checking PSW CLI installation..."
if command -v psw &> /dev/null; then
    VERSION=$(psw --version 2>&1 | head -n 1)
    print_success "PSW CLI is installed: $VERSION"
else
    print_error "PSW CLI is not installed"
    echo "Install with: dotnet tool install -g PackageScriptWriter.Cli"
    exit 1
fi

# Step 2: Check .NET SDK
print_step "Step 2: Checking .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_success ".NET SDK is installed: $DOTNET_VERSION"
else
    print_error ".NET SDK is not installed"
    exit 1
fi

# Step 3: Prime the cache
print_step "Step 3: Priming the package cache (this may take a moment)..."
print_info "This ensures the demo runs smoothly without API delays"

# Clear existing cache first
psw --clear-cache > /dev/null 2>&1 || true
print_success "Cleared existing cache"

# Run a quick command to populate cache
echo "Fetching package list..."
timeout 60s psw --default > /dev/null 2>&1 || true
print_success "Package cache primed"

# Step 4: Create demo directory
print_step "Step 4: Setting up demo workspace..."
DEMO_DIR="$HOME/psw-demo-sydney"
if [ -d "$DEMO_DIR" ]; then
    print_warning "Demo directory already exists: $DEMO_DIR"
    read -p "Remove it and start fresh? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        rm -rf "$DEMO_DIR"
        print_success "Removed old demo directory"
    fi
fi

if [ ! -d "$DEMO_DIR" ]; then
    mkdir -p "$DEMO_DIR"
    print_success "Created demo directory: $DEMO_DIR"
fi

cd "$DEMO_DIR"

# Step 5: Generate sample scripts for backup
print_step "Step 5: Generating backup scripts..."

# Generate a default script
print_info "Generating default script..."
psw --default > "$DEMO_DIR/backup-default-script.sh" 2>&1 || true
if [ -f "$DEMO_DIR/backup-default-script.sh" ]; then
    print_success "Default script saved: backup-default-script.sh"
fi

# Generate a full-featured script
print_info "Generating full-featured script..."
psw -p "uSync,Diplo.GodMode,Umbraco.Community.BlockPreview" \
    -n DemoProject -s DemoSolution -u \
    --database-type SQLite \
    --admin-email "admin@demo.com" \
    --admin-password "DemoPass123!" \
    --add-docker > "$DEMO_DIR/backup-full-script.sh" 2>&1 || true

if [ -f "$DEMO_DIR/backup-full-script.sh" ]; then
    print_success "Full-featured script saved: backup-full-script.sh"
fi

# Step 6: Create a sample template
print_step "Step 6: Creating sample team template..."
psw -p "uSync|17.0.0,Diplo.GodMode" \
    -n SampleProject -s SampleSolution \
    -u --database-type SQLite \
    --admin-email "admin@company.com" \
    template save DemoTeamStandard \
    --template-description "Demo template for Sydney meetup" \
    --template-tags "demo,sydney,umbraco" > /dev/null 2>&1 || true

if psw template list 2>&1 | grep -q "DemoTeamStandard"; then
    print_success "Sample template created: DemoTeamStandard"
else
    print_warning "Could not create sample template (this is okay)"
fi

# Step 7: Populate history with some entries
print_step "Step 7: Populating history with sample entries..."
print_info "This will give you something to show in the history demo"

# Generate a few scripts to populate history
for i in {1..3}; do
    psw --default > /dev/null 2>&1 || true
done

HISTORY_COUNT=$(psw history list 2>&1 | grep -c "│" || echo "0")
if [ "$HISTORY_COUNT" -gt "0" ]; then
    print_success "History populated with sample entries"
else
    print_warning "Could not populate history (this is okay)"
fi

# Step 8: Test network connectivity
print_step "Step 8: Testing network connectivity..."
if curl -s --head --request GET https://marketplace.umbraco.com > /dev/null; then
    print_success "Umbraco Marketplace is reachable"
else
    print_warning "Cannot reach Umbraco Marketplace (check your network)"
fi

if curl -s --head --request GET https://api.nuget.org > /dev/null; then
    print_success "NuGet API is reachable"
else
    print_warning "Cannot reach NuGet API (check your network)"
fi

# Step 9: Create quick reference files
print_step "Step 9: Creating quick reference files..."

cat > "$DEMO_DIR/DEMO_COMMANDS.txt" << 'EOF'
Package Script Writer CLI - Demo Commands
==========================================

SCENARIO 1: 30-Second Win
-------------------------
psw --default

SCENARIO 2: Interactive Experience
-----------------------------------
psw
(Then navigate through the menus)

SCENARIO 3: Power User
----------------------
# Quick setup
psw -p "uSync|17.0.0,Diplo.GodMode" -n UmbracoSydney -s SydneyMeetup --database-type SQLite

# Full automation
psw -p "uSync,Diplo.GodMode,Umbraco.Community.BlockPreview" \
    -n ProductionSite -s MySolution -u \
    --database-type SQLite \
    --admin-email "admin@example.com" \
    --admin-password "SecurePass123!" \
    --add-docker --auto-run

# Client project
psw --template "Bootstrap Starter Kit" -p "uSync,Diplo.GodMode" \
    -n ClientWebsite -u --database-type SQLite \
    --admin-email "dev@agency.com" --admin-password "TempPass123!" \
    --auto-run

SCENARIO 4: Team Templates
---------------------------
psw template list
psw template load DemoTeamStandard
psw template load DemoTeamStandard -n NewProject --auto-run

SCENARIO 5: History
-------------------
psw history list
psw history show 1
psw history rerun 1

BONUS: Versions Table
---------------------
psw versions

EOF

print_success "Created command reference: DEMO_COMMANDS.txt"

# Step 10: System check summary
print_step "Step 10: Final system check..."

echo ""
echo "================================"
echo "Demo Preparation Complete! ✨"
echo "================================"
echo ""
echo "Demo workspace: $DEMO_DIR"
echo ""
echo "Ready to demo:"
print_success "PSW CLI installed and working"
print_success "Package cache primed"
print_success "Backup scripts generated"
print_success "Sample template created"
print_success "History populated"
print_success "Quick reference created"
echo ""

print_info "Demo checklist:"
echo "  [ ] Terminal size is large enough (at least 120x30)"
echo "  [ ] Terminal colors are working"
echo "  [ ] Network connection is stable"
echo "  [ ] Demo commands ready: cat $DEMO_DIR/DEMO_COMMANDS.txt"
echo "  [ ] Backup scripts ready in: $DEMO_DIR"
echo ""

print_info "Optional: Test the interactive mode now"
echo "  cd $DEMO_DIR && psw"
echo ""

print_info "During demo, if anything fails:"
echo "  - Use backup scripts from: $DEMO_DIR"
echo "  - Show web version: https://psw.codeshare.co.uk"
echo "  - Show pre-generated history entries"
echo ""

print_success "Good luck with your demo! 🚀"
echo ""
