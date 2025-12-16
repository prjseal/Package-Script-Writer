# Community Templates Feature Design

## Overview
Add the ability for users to share and use community-contributed templates stored in the GitHub repository, making it easy to get started with common Umbraco configurations.

## Architecture

### 1. Storage Structure

```
community-templates/
├── README.md                           # Submission guidelines
├── index.json                          # Template catalog with metadata
├── blog-with-usync.yaml               # Example: Blog setup with uSync
├── multi-site-setup.yaml              # Example: Multi-site configuration
├── e-commerce-starter.yaml            # Example: E-commerce setup
└── corporate-website.yaml             # Example: Corporate site
```

**Index File Format:** `index.json` contains metadata for all templates
```json
{
  "templates": [
    {
      "name": "blog-with-usync",
      "displayName": "Blog with uSync",
      "description": "Complete blog setup with uSync, Forms, and SEO packages",
      "author": "John Doe",
      "tags": ["blog", "usync", "seo"],
      "fileName": "blog-with-usync.yaml",
      "created": "2024-12-15"
    },
    {
      "name": "e-commerce-starter",
      "displayName": "E-Commerce Starter",
      "description": "E-commerce site setup with payment packages",
      "author": "Jane Smith",
      "tags": ["commerce", "shop"],
      "fileName": "e-commerce-starter.yaml",
      "created": "2024-12-16"
    }
  ],
  "lastUpdated": "2024-12-16"
}
```

**Template Format:** Same YAML structure as existing user templates
```yaml
metadata:
  name: "Blog with uSync"
  description: "Complete blog setup with uSync, Forms, and SEO packages"
  author: "Community Contributor"
  created: "2024-12-15"
  tags: ["blog", "usync", "seo"]

configuration:
  template:
    name: "Umbraco.Templates"
    version: "14.3.0"
  packages:
    - name: "uSync"
      version: "17.0.0"
    - name: "Umbraco.Forms"
      version: "14.0.0"
  # ... rest of configuration
```

---

## 2. CLI Mode (Non-Interactive) Flags

### New Flags to Add

| Flag | Description | Example |
|------|-------------|---------|
| `--community-template list` | List all available community templates | `psw --community-template list` |
| `--community-template <name>` | Use a specific community template and apply overrides the same as with local templates| `psw --community-template blog-with-usync -p "ProjectName"` |

### Usage Examples

```bash
# List all community templates
psw --list-community

# Use a community template directly and apoly overrides same as using loval template
psw --community-template blog-with-usync -n "ProjectName"
```

---

## 3. Interactive Mode Integration

### New Menu Option

Update `InteractiveModeWorkflow.cs` main menu:

```
┌─────────────────────────────────────────────────┐
│ Welcome to Package Script Writer               │
├─────────────────────────────────────────────────┤
│ 1. Create script from scratch                  │
│ 2. Create script from defaults                 │
│ 3. Create script from template                 │
│ 4. Create script from community template │
│ 5. Create script from history                  │
│ 6. See Umbraco versions table                  │
│ 7. See templates                                │
│ 8. See help                                     │
│ 9. See version                                  │
│ 10. Clear cache                                 │
└─────────────────────────────────────────────────┘
```

### Interactive Workflow

1. **Select "Create script from community template"**

2. **Fetch and Display Community Templates**
   ```
   ┌─────────────────────────────────────────────────────────┐
   │ Available Community Templates                           │
   ├─────────────────────────────────────────────────────────┤
   │ > Blog with uSync                                       │
   │   Author: John Doe | Tags: blog, usync, seo            │
   │   Complete blog setup with uSync, Forms, SEO packages   │
   │                                                          │
   │   Multi-Site Setup                                      │
   │   Author: Jane Smith | Tags: multi-site, advanced      │
   │   Configuration for managing multiple sites             │
   │                                                          │
   │   E-Commerce Starter                                    │
   │   Author: Commerce Team | Tags: commerce, shop         │
   │   E-commerce site with payment and inventory packages   │
   └─────────────────────────────────────────────────────────┘
   ```

3. **Generate and display Script and show the menu for what would you like to do? (same as currebt local template one. Save at this point will just create it as a normal local template)**
   ```
   What would you like to do?
   > Run
     Edit
     Copy
     Save
     Start Over
   ```
---

## 4. Technical Implementation

### 4.1 New Service: `CommunityTemplateService.cs`

**Location:** `src/PackageCliTool/Services/CommunityTemplateService.cs`

**Responsibilities:**
- Fetch templates from GitHub repository
- Parse and validate community templates
- Cache template list (1 hour TTL like packages)
- Download specific templates
- Convert to local template format

**Key Methods:**
```csharp
public class CommunityTemplateService
{
    Task<List<CommunityTemplateMetadata>> GetAllTemplatesAsync();
    Task<ScriptModel> GetTemplateAsync(string templateName);
    Task<bool> SaveToLocalTemplatesAsync(string templateName);
    Task<string> GetTemplateContentAsync(string templateName);
    void ClearCache();
}
```

### 4.2 GitHub Raw Content Integration

**No GitHub API Required** - Uses direct raw file access for simplicity and reliability.

**URLs Used:**

1. **Fetch Template Index:**
   ```
   GET https://raw.githubusercontent.com/prjseal/Package-Script-Writer/main/community-templates/index.json
   ```
   Returns: JSON catalog of all available templates with metadata

2. **Fetch Template Content:**
   ```
   GET https://raw.githubusercontent.com/prjseal/Package-Script-Writer/main/community-templates/{fileName}
   ```
   Returns: Raw YAML template content

**Implementation:**
```csharp
public class CommunityTemplateService
{
    private const string DefaultRepo = "prjseal/Package-Script-Writer";
    private const string DefaultBranch = "main";

    private string GetIndexUrl(string repo = DefaultRepo, string branch = DefaultBranch)
        => $"https://raw.githubusercontent.com/{repo}/{branch}/community-templates/index.json";

    private string GetTemplateUrl(string fileName, string repo = DefaultRepo, string branch = DefaultBranch)
        => $"https://raw.githubusercontent.com/{repo}/{branch}/community-templates/{fileName}";

    public async Task<TemplateIndex> GetIndexAsync()
    {
        // Check cache first
        var cached = _cacheService.Get<TemplateIndex>(CommunityTemplatesCacheKey);
        if (cached != null) return cached;

        // Fetch from GitHub
        var url = GetIndexUrl(_options.CommunityRepo);
        var index = await _httpClient.GetFromJsonAsync<TemplateIndex>(url);

        // Cache for 1 hour
        _cacheService.Set(CommunityTemplatesCacheKey, index, TimeSpan.FromHours(1));

        return index;
    }
}
```

**Benefits:**
- ✅ No API rate limits (raw content is CDN-served)
- ✅ No authentication required
- ✅ Simple HTTP GET requests
- ✅ Fast CDN delivery
- ✅ Works with forks by changing repo URL
- ✅ Reliable and predictable

### 4.3 Update CommandLineOptions.cs

Add new options:
```csharp
[Option("list-community", Required = false, HelpText = "List all available community templates")]
public bool ListCommunityTemplates { get; set; }

[Option("community-template", Required = false, HelpText = "Use a community template by name")]
public string CommunityTemplate { get; set; }

[Option("save-community-template", Required = false, HelpText = "Save the community template to local templates")]
public bool SaveCommunityTemplate { get; set; }

[Option("show-community", Required = false, HelpText = "Show details of a community template")]
public string ShowCommunityTemplate { get; set; }

[Option("community-repo", Required = false, Default = "prjseal/Package-Script-Writer",
    HelpText = "GitHub repository for community templates")]
public string CommunityRepo { get; set; }
```

### 4.4 Update CliModeWorkflow.cs

Add handling for community template flags:
```csharp
if (options.ListCommunityTemplates)
{
    await ListCommunityTemplatesAsync();
    return;
}

if (!string.IsNullOrEmpty(options.ShowCommunityTemplate))
{
    await ShowCommunityTemplateAsync(options.ShowCommunityTemplate);
    return;
}

if (!string.IsNullOrEmpty(options.CommunityTemplate))
{
    var scriptModel = await _communityTemplateService.GetTemplateAsync(options.CommunityTemplate);

    if (options.SaveCommunityTemplate)
    {
        await _communityTemplateService.SaveToLocalTemplatesAsync(options.CommunityTemplate);
    }

    // Apply any CLI overrides to the template
    ApplyCommandLineOverrides(scriptModel, options);

    // Generate script
    await GenerateAndExecuteScriptAsync(scriptModel, options);
}
```

### 4.5 Update InteractiveModeWorkflow.cs

Add new menu option and workflow:
```csharp
private async Task CreateScriptFromCommunityTemplateAsync()
{
    // 1. Fetch community templates
    var templates = await _communityTemplateService.GetAllTemplatesAsync();

    if (!templates.Any())
    {
        AnsiConsole.MarkupLine("[yellow]No community templates available[/]");
        return;
    }

    // 2. Display selection prompt
    var selectedTemplate = AnsiConsole.Prompt(
        new SelectionPrompt<CommunityTemplateMetadata>()
            .Title("Select a [green]community template[/]:")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more templates)[/]")
            .AddChoices(templates)
            .UseConverter(t => $"{t.Name} - {t.Description}")
    );

    // 3. Ask what to do
    var action = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What would you like to do?")
            .AddChoices("Use template now (one-time)",
                       "Save to my templates and use",
                       "Just save to my templates",
                       "Cancel")
    );

    // 4. Execute based on selection
    // ... implementation
}
```

### 4.6 Caching Strategy

Leverage existing `CacheService.cs`:
```csharp
// Cache key
private const string CommunityTemplatesCacheKey = "community_templates_index";

// Cache template index for 1 hour (like packages)
var cached = _cacheService.Get<TemplateIndex>(CommunityTemplatesCacheKey);
if (cached != null) return cached;

// Fetch index from raw.githubusercontent.com and cache
var index = await FetchIndexFromGitHubAsync();
_cacheService.Set(CommunityTemplatesCacheKey, index, TimeSpan.FromHours(1));
```

**What Gets Cached:**
- Template index (1 hour TTL)
- Individual template content can be cached on-demand
- Clear cache with `psw --clear-cache` (existing functionality)

---

## 5. User Experience Flow

### Scenario 1: New User Wants a Blog Setup

**Interactive Mode:**
```
$ psw
> Create script from community template
> Blog with uSync
> Use template now
> Override project name: "MyAwesomeBlog"
> Script generated and displayed
```

**CLI Mode:**
```bash
psw --community-template blog-with-usync -n "MyAwesomeBlog" --auto-run
```

### Scenario 2: User Wants to Customize Community Template

**Interactive Mode:**
```
> Create script from community template
> E-Commerce Starter
> Save to my templates and use
> (Template saved as "e-commerce-starter" in ~/.psw/templates/)
> Continue with configuration...
> Later: Can edit ~/. psw/templates/e-commerce-starter.yaml
```

**CLI Mode:**
```bash
# Save to local first
psw --community-template e-commerce-starter --save-community-template

# Edit locally
nano ~/.psw/templates/e-commerce-starter.yaml

# Use customized version
psw template load e-commerce-starter -n "MyShop"
```

### Scenario 3: Exploring Available Templates

**CLI Mode:**
```bash
# List all
psw --list-community

# Output:
# Available Community Templates:
#
# 1. blog-with-usync
#    Description: Complete blog setup with uSync, Forms, and SEO packages
#    Author: John Doe
#    Tags: blog, usync, seo
#
# 2. multi-site-setup
#    ...

# View details
psw --show-community blog-with-usync

# Output: (Full YAML content formatted nicely)
```

---

## 6. Template Submission Process

### For Contributors

**Create `community-templates/README.md`:**

```markdown
# Community Templates

Share your Umbraco installation templates with the community!

## Submission Guidelines

1. **Create Your Template Locally**
   ```bash
   psw
   # Configure your ideal setup
   # Save as template: my-awesome-template
   ```

2. **Export Template**
   ```bash
   psw template export my-awesome-template
   # Exports to my-awesome-template.yaml
   ```

3. **Add Metadata**
   Edit the YAML file and ensure it has:
   - Clear `name` and `description`
   - Your name in `author`
   - Relevant `tags`
   - Current `created` date

4. **Update Index File**
   Add your template to `community-templates/index.json`:
   ```json
   {
     "name": "your-template-name",
     "displayName": "Your Template Name",
     "description": "Brief description of what this template does",
     "author": "Your Name",
     "tags": ["tag1", "tag2"],
     "fileName": "your-template-name.yaml",
     "created": "2024-12-16"
   }
   ```

5. **Submit Pull Request**
   - Fork the repository
   - Add your template YAML to `community-templates/`
   - Update `community-templates/index.json` with your template entry
   - Update `lastUpdated` field in index.json
   - Name template descriptively (e.g., `blog-with-usync.yaml`)
   - Create PR with description of the template use case

6. **Template Review**
   - Maintainers will review for quality and security
   - Templates must use publicly available packages
   - No credentials or sensitive data
   - Clear documentation of purpose
   - Index entry matches template metadata

## Template Naming Convention

- Use kebab-case: `my-template-name.yaml`
- Be descriptive: `multi-tenant-blog.yaml` not `template1.yaml`
- Indicate purpose: `e-commerce-starter.yaml`, `corporate-website.yaml`

## Quality Standards

✅ **Good Template:**
- Clear purpose and description
- Up-to-date package versions
- Well-documented with tags
- Tested and working configuration

❌ **Avoid:**
- Outdated package versions
- Vague descriptions
- Hard-coded credentials
- Uncommon or private packages
```

---

## 7. Security Considerations

### Template Validation

Before using community templates, validate:
1. **YAML Structure:** Ensure valid template format
2. **Package Sources:** Only allow NuGet packages
3. **No Executable Code:** Templates are configuration only
4. **Sanitize Inputs:** Clean all user-provided values
5. **Show Preview:** Display what will be installed

### Offline and Network Handling

- **Network Failures:** Gracefully handle when GitHub is unreachable
- **Cache Strategy:** Use 1-hour cache to reduce requests and enable offline access
- **User Feedback:** Clear error messages when templates are unavailable
- **Fallback:** Suggest using local templates if community templates unavailable

### PR Review Process

All community templates should be:
- Manually reviewed by maintainers
- Tested before merging
- Checked for malicious configurations
- Validated for package availability

---

## 8. Testing Strategy

### Unit Tests

```csharp
// Test: CommunityTemplateService
- GetAllTemplatesAsync_ReturnsListOfTemplates
- GetTemplateAsync_WithValidName_ReturnsTemplate
- GetTemplateAsync_WithInvalidName_ThrowsException
- SaveToLocalTemplatesAsync_CreatesLocalFile
- ClearCache_RemovesCachedTemplates

// Test: CLI Options
- CommunityTemplateFlag_IsRecognized
- ListCommunityFlag_IsRecognized
- CommunityRepoFlag_UsesCustomRepo
```

### Integration Tests

```csharp
- FetchFromGitHub_ReturnsActualTemplates
- DownloadTemplate_ContainsValidYAML
- LocalSave_CreatesFileInTemplatesDirectory
```

### Manual Testing Checklist

- [ ] List community templates (interactive)
- [ ] List community templates (CLI)
- [ ] Use template without saving
- [ ] Use template and save locally
- [ ] Override template values
- [ ] View template details
- [ ] Handle offline scenarios
- [ ] Handle network failures gracefully
- [ ] Validate malformed YAML handling
- [ ] Verify cache expiration (1 hour)
- [ ] Test with custom repository URL

---

## 9. Documentation Updates

### Files to Update

1. **README.md** - Add community templates section
2. **Help Text** - Update CLI help output
3. **Interactive Menu** - Add help text for new option
4. **CHANGELOG.md** - Document new feature

### README.md Section

```markdown
## Community Templates

Get started quickly with community-contributed templates for common Umbraco scenarios.

### List Available Templates
```bash
psw --list-community
```

### Use a Community Template
```bash
# One-time use
psw --community-template blog-with-usync

# Save for repeated use
psw --community-template blog-with-usync --save-community-template

# Interactive mode
psw
# Select: Create script from community template
```

### Contribute Your Template

Have a great Umbraco setup? Share it with the community!

1. Create your template using `psw template save`
2. Export it: `psw template export my-template`
3. Submit a PR to the `community-templates/` folder
4. See [Community Templates Guide](community-templates/README.md)
```

---

## 10. Implementation Phases

### Phase 1: Core Infrastructure (MVP)
- [ ] Create `community-templates/` folder structure
- [ ] Create `index.json` with template catalog structure
- [ ] Implement `CommunityTemplateService` with raw URL fetching
- [ ] Add caching support (1 hour TTL)
- [ ] Create 2-3 example templates
- [ ] Write community-templates/README.md with submission guidelines

### Phase 2: CLI Integration
- [ ] Add command-line flags
- [ ] Update `CliModeWorkflow`
- [ ] Add `--list-community` support
- [ ] Add `--community-template` support
- [ ] Add `--save-community-template` support

### Phase 3: Interactive Mode
- [ ] Update main menu
- [ ] Create community template workflow
- [ ] Add template selection UI
- [ ] Add preview/details display
- [ ] Add save options

### Phase 4: Documentation & Polish
- [ ] Write community-templates/README.md
- [ ] Update main README.md
- [ ] Add help text
- [ ] Create contribution guide
- [ ] Add error handling and user feedback

### Phase 5: Testing & Launch
- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing
- [ ] Beta testing with community
- [ ] Official release

---

## 11. Future Enhancements

### Possible Future Features

1. **Template Ratings/Stars**
   - Track downloads and usage
   - Community voting system

2. **Template Search**
   - Search by tags
   - Filter by package/technology

3. **Template Versioning**
   - Support multiple versions of same template
   - Umbraco version compatibility matrix

4. **Template Collections**
   - Group related templates
   - "Starter Packs" for specific industries

5. **Template Analytics**
   - Track downloads in index.json
   - Popular templates highlighted
   - Usage statistics

6. **Private Template Repositories**
   - Support for private forks with authentication
   - `--github-token` flag for private repos
   - Team/organization template sharing

---

## Summary

This design provides:

✅ **Non-intrusive:** Builds on existing template system
✅ **User-friendly:** Works in both interactive and CLI modes
✅ **Flexible:** Use one-time or save locally for customization
✅ **Community-driven:** Easy submission via PR with index.json updates
✅ **Simple & Reliable:** No API dependencies, just raw file fetching
✅ **No Rate Limits:** Uses GitHub's CDN for raw content
✅ **Fast:** Cached with 1-hour TTL, CDN-delivered
✅ **Secure:** Validation and manual review process
✅ **Extensible:** Foundation for future enhancements

The feature integrates seamlessly with existing workflows while opening up community collaboration and knowledge sharing. The index-based approach ensures reliability without API complexity.
