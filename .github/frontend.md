# Frontend Architecture

Complete documentation of the client-side architecture including JavaScript, View Components, and UI patterns.

## Table of Contents
- [Overview](#overview)
- [JavaScript Architecture](#javascript-architecture)
- [View Components](#view-components)
- [UI Framework](#ui-framework)
- [Event Handling](#event-handling)
- [State Management](#state-management)
- [Syntax Highlighting](#syntax-highlighting)

---

## Overview

The frontend is built with:
- **Razor Pages** for server-side rendering
- **Vanilla JavaScript** (740+ lines) for interactivity
- **Bootstrap 5** for responsive UI
- **PrettyPrint.js** for syntax highlighting

**Architecture Pattern**: Progressive enhancement with server-side rendering as the base and JavaScript adding interactivity.

---

## JavaScript Architecture

**File**: `wwwroot/js/site.js` (740+ lines)

### Main Object Structure

```javascript
var psw = psw || {
    controls: {
        // DOM element references (28+ controls)
        packages: document.getElementById('Packages'),
        templateName: document.getElementById('TemplateName'),
        templateVersion: document.getElementById('TemplateVersion'),
        includeStarterKit: document.getElementById('IncludeStarterKit'),
        // ... more controls
    },
    buttons: {
        // Button references (7 buttons)
        clearpackages: document.getElementById('clearpackages'),
        reset: document.getElementById('reset'),
        copy: document.getElementById('copy'),
        generate: document.getElementById('generate'),
        // ... more buttons
    },
    init: function() { },
    onPageLoad: function() { },
    addListeners: function() { },
    // ... 30+ methods
}
```

### Key Functions

#### Initialization

```javascript
init: function() {
    psw.onPageLoad();
    psw.addListeners();
    psw.setFromLocalStorage();
}
```

**Called**: On page load
**Purpose**: Sets up event listeners, loads saved state, initializes UI

---

#### Event Listeners

```javascript
addListeners: function() {
    // Save/Delete configuration
    psw.buttons.save.addEventListener('click', ...);
    psw.buttons.deletesave.addEventListener('click', ...);

    // Package management
    psw.buttons.clearpackages.addEventListener('click', ...);
    psw.buttons.reset.addEventListener('click', ...);

    // Search filter
    psw.controls.search.addEventListener('keyup', psw.filterPackages);

    // Form controls
    psw.controls.useUnattendedInstall.addEventListener('change', ...);
    psw.controls.onelinerOutput.addEventListener('change', ...);
    psw.controls.removeComments.addEventListener('change', ...);
    psw.controls.templateName.addEventListener('change', ...);
    psw.controls.templateVersion.addEventListener('change', ...);

    // Package selection
    psw.controls.packageCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', ...);
    });

    // Version dropdowns
    psw.controls.packageVersionDropdowns.forEach(dropdown => {
        dropdown.addEventListener('change', ...);
    });
}
```

---

#### URL Synchronization

```javascript
updateUrl: function() {
    var queryString = psw.buildQueryString();
    var newUrl = window.location.origin + window.location.pathname + queryString;

    if (window.history && window.history.pushState) {
        window.history.pushState(null, '', newUrl);
    }
}
```

**Purpose**: Syncs form state to URL for shareable configurations
**Trigger**: Any form change event
**Method**: Uses `history.pushState()` to update URL without page reload

---

#### Script Generation

```javascript
updateOutput: function() {
    var model = psw.buildModelFromForm();

    fetch('/api/scriptgeneratorapi/generatescript', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ model: model })
    })
    .then(response => response.json())
    .then(data => {
        psw.controls.codeBlock.textContent = data.script;
        PR.prettyPrint(); // Apply syntax highlighting
    })
    .catch(error => {
        console.error('Error:', error);
    });
}
```

**Purpose**: Calls API to generate script and updates UI
**Trigger**: Form changes, user clicks "Generate"
**Error Handling**: Logs errors to console

---

#### Package Version Loading

```javascript
loadPackageVersions: function(packageId, dropdown, includePrerelease) {
    fetch('/api/scriptgeneratorapi/getpackageversions', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            packageId: packageId,
            includePrerelease: includePrerelease
        })
    })
    .then(response => response.json())
    .then(data => {
        dropdown.innerHTML = '<option value="">Latest</option>';
        data.versions.forEach(version => {
            var option = document.createElement('option');
            option.value = version;
            option.textContent = version;
            dropdown.appendChild(option);
        });
    })
    .catch(error => {
        console.error('Error loading versions:', error);
    });
}
```

**Purpose**: Fetches and populates version dropdown for selected package
**Trigger**: Package checkbox is checked
**Caching**: Server-side caching (60 minutes)

---

#### Package Filtering

```javascript
filterPackages: function() {
    var searchTerm = psw.controls.search.value.toLowerCase();

    psw.controls.packageCards.forEach(function(card) {
        var packageName = card.getAttribute('data-package-name').toLowerCase();
        var packageDescription = card.getAttribute('data-package-description').toLowerCase();

        if (packageName.includes(searchTerm) || packageDescription.includes(searchTerm)) {
            card.style.display = '';
        } else {
            card.style.display = 'none';
        }
    });
}
```

**Purpose**: Real-time package search/filter
**Trigger**: Keyup event on search input
**Performance**: Debounced to prevent excessive filtering

---

#### Local Storage Management

```javascript
setFromLocalStorage: function() {
    var savedParams = window.localStorage.getItem("searchParams");
    if (savedParams) {
        var url = window.location.origin + window.location.pathname + savedParams;
        window.location.href = url;
    }
}

saveToLocalStorage: function() {
    window.localStorage.setItem("searchParams", window.location.search);
}

deleteFromLocalStorage: function() {
    window.localStorage.removeItem("searchParams");
}
```

**Purpose**: Persist and restore configuration between sessions
**Storage Key**: `searchParams`
**Data Format**: URL query string

---

#### Control Toggles

```javascript
toggleUnattendedInstallControls: function() {
    var isChecked = psw.controls.useUnattendedInstall.checked;

    // Enable/disable database fields
    psw.controls.databaseType.disabled = !isChecked;
    psw.controls.connectionString.disabled = !isChecked;
    psw.controls.userFriendlyName.disabled = !isChecked;
    psw.controls.userEmail.disabled = !isChecked;
    psw.controls.userPassword.disabled = !isChecked;
}

toggleDockerControls: function() {
    var isUmbracoTemplate = psw.controls.templateName.value === "Umbraco.Templates";
    var version = psw.controls.templateVersion.value;

    // Enable Docker options for appropriate versions
    psw.controls.canIncludeDocker.value = isUmbracoTemplate && version >= "10.0.0";
    psw.controls.includeDockerfile.disabled = !isUmbracoTemplate;
    psw.controls.includeDockerCompose.disabled = !isUmbracoTemplate;
}

toggleTemplateNameControls: function() {
    var isUmbracoTemplate = psw.controls.templateName.value === "Umbraco.Templates";

    // Show/hide Umbraco-specific options
    psw.controls.includeStarterKit.closest('.form-group').style.display =
        isUmbracoTemplate ? '' : 'none';
}
```

**Purpose**: Show/hide and enable/disable form controls based on selections
**Trigger**: Template, version, or option changes

---

#### Copy to Clipboard

```javascript
copyToClipboard: function() {
    var codeBlock = psw.controls.codeBlock;
    var textToCopy = codeBlock.textContent;

    navigator.clipboard.writeText(textToCopy).then(function() {
        psw.buttons.copy.textContent = 'Copied!';
        setTimeout(function() {
            psw.buttons.copy.textContent = 'Copy';
        }, 2000);
    }).catch(function(error) {
        console.error('Failed to copy:', error);
    });
}
```

**Purpose**: Copy generated script to clipboard
**API**: Uses modern `navigator.clipboard` API
**Feedback**: Changes button text to "Copied!" temporarily

---

## View Components

View Components are reusable, self-contained UI sections rendered server-side.

### Component List

| Component | File | Purpose |
|-----------|------|---------|
| SiteHeader | SiteHeaderViewComponent.cs | Site header and branding |
| TabNavigation | TabNavigationViewComponent.cs | Tab navigation UI |
| Package | PackageViewComponent.cs | Package selection cards |
| Options | OptionsViewComponent.cs | Configuration form |
| InstallScript | InstallScriptViewComponent.cs | Script output display |
| PopularScripts | PopularScriptsViewComponent.cs | Pre-configured templates |
| About | AboutViewComponent.cs | About section |
| UmbracoVersions | UmbracoVersionsViewComponent.cs | Version lifecycle table |
| Share | ShareViewComponent.cs | Share/save controls |
| SiteFooter | SiteFooterViewComponent.cs | Site footer |

---

### Component Implementation Pattern

**C# Class** (example):
```csharp
public class PackageViewComponent : ViewComponent
{
    private readonly IPackageService _packageService;
    private readonly IMemoryCache _memoryCache;

    public PackageViewComponent(
        IPackageService packageService,
        IMemoryCache memoryCache)
    {
        _packageService = packageService;
        _memoryCache = memoryCache;
    }

    public async Task<IViewComponentResult> InvokeAsync(PackagesViewModel model)
    {
        var packages = await _packageService.GetAllPackages(_memoryCache, 60);
        return View(packages);
    }
}
```

**Razor View** (example - `Shared/Components/Package/Default.cshtml`):
```razor
@model PagedPackages

<div class="row" id="packagelist">
    @foreach (var package in Model.Packages)
    {
        <div class="col-md-4 mb-3">
            <div class="card"
                 data-package-name="@package.Name"
                 data-package-description="@package.Description">
                <div class="card-body">
                    <h5 class="card-title">@package.Name</h5>
                    <p class="card-text">@package.Description</p>
                    <div class="form-check">
                        <input type="checkbox"
                               class="form-check-input"
                               id="package-@package.Id"
                               value="@package.Id">
                        <label class="form-check-label" for="package-@package.Id">
                            Include
                        </label>
                    </div>
                    <select class="form-control mt-2"
                            id="version-@package.Id"
                            disabled>
                        <option value="">Latest</option>
                    </select>
                </div>
            </div>
        </div>
    }
</div>
```

**Invocation** (in parent view):
```razor
@await Component.InvokeAsync("Package", new { model = Model })
```

---

### PackageViewComponent Details

**Purpose**: Displays package selection cards with checkboxes and version dropdowns

**Features**:
- Grid layout (3 columns on desktop)
- Searchable by name and description
- Version dropdown (populated on selection)
- Package metadata display (icon, owner, tags)

**Data Attributes**:
- `data-package-name` - For search filtering
- `data-package-description` - For search filtering
- `data-package-id` - For API calls

---

### OptionsViewComponent Details

**Purpose**: Configuration form for project settings

**Form Sections**:
1. **Template Selection**
   - Template dropdown (Umbraco.Templates, etc.)
   - Version dropdown (dynamically populated)

2. **Project Configuration**
   - Solution name
   - Project name
   - Create solution file checkbox

3. **Database Settings** (shown if unattended install)
   - Database type dropdown
   - Connection string input
   - User credentials (name, email, password)

4. **Docker Options** (shown for Umbraco templates)
   - Include Dockerfile
   - Include Docker Compose

5. **Output Options**
   - One-liner output checkbox
   - Remove comments checkbox

---

### InstallScriptViewComponent Details

**Purpose**: Displays generated script with syntax highlighting

**Features**:
- Syntax-highlighted code block
- Copy button
- Pre-formatted output

**HTML Structure**:
```html
<div class="script-output">
    <button id="copy" class="btn btn-primary">Copy</button>
    <pre class="prettyprint lang-bash"><code>
        # Generated script appears here
    </code></pre>
</div>
```

---

## UI Framework

### Bootstrap 5

**Version**: Bootstrap 5.x

**Components Used**:
- **Grid System**: Responsive layout (12-column grid)
- **Cards**: Package display cards
- **Forms**: Input controls, checkboxes, dropdowns
- **Buttons**: Primary, secondary, outline styles
- **Tabs**: Navigation between sections
- **Alerts**: Error/success messages
- **Modals**: Future enhancement for dialogs

**Customization**:
- Custom CSS in `wwwroot/css/site.css`
- Overrides for card spacing, colors, etc.

---

### Responsive Design

**Breakpoints**:
- **Mobile** (< 576px): Single column layout
- **Tablet** (576px - 768px): 2 column package grid
- **Desktop** (> 768px): 3 column package grid

**Mobile Optimizations**:
- Stacked form controls
- Full-width buttons
- Collapsible sections for options

---

## Event Handling

### Event Flow Diagram

See [Process Flows - Event Flow](process-flows.md#event-flow) for sequence diagram.

### Event Types

| Event | Target | Handler | Purpose |
|-------|--------|---------|---------|
| `change` | Template dropdown | `toggleTemplateNameControls()` | Show/hide template-specific options |
| `change` | Version dropdown | `updateOutput()` | Regenerate script |
| `change` | Package checkbox | `getPackageVersionsForCheckbox()` | Load versions |
| `change` | Version dropdown | `updateOutput()` | Update script with version |
| `keyup` | Search input | `filterPackages()` | Real-time search |
| `click` | Copy button | `copyToClipboard()` | Copy script |
| `click` | Save button | `saveToLocalStorage()` | Save configuration |
| `click` | Clear packages | `clearAllPackages()` | Uncheck all packages |
| `click` | Reset button | `reset()` | Reset form to defaults |

### Debouncing

Search input uses debouncing to improve performance:

```javascript
let searchTimeout;
psw.controls.search.addEventListener('keyup', function() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(psw.filterPackages, 300);
});
```

**Delay**: 300ms
**Benefit**: Reduces DOM operations during typing

---

## State Management

### State Sources

1. **URL Query String**: Primary source of truth
   - All form state encoded in URL
   - Enables shareable configurations
   - Updated on every form change

2. **Local Storage**: Secondary persistence
   - Stores last used configuration
   - Restored on page load
   - Key: `searchParams`

3. **DOM State**: Current form values
   - Checkbox states
   - Dropdown selections
   - Input values

### State Synchronization Flow

```
User Input → JavaScript Handler → Update DOM → Update URL → (Optional) Save to localStorage
    ↓
Load Page → Check localStorage → Load from URL → Populate Form → Generate Script
```

### Query String Format

Example:
```
?TemplateName=Umbraco.Templates
&TemplateVersion=14.3.0
&ProjectName=MyProject
&Packages=Package1|1.0.0,Package2|2.0.0
&UseUnattendedInstall=true
&DatabaseType=SQLite
```

**Encoding**: URL-encoded
**Delimiter**: `&` for parameters, `,` for package list, `|` for package/version separator

---

## Syntax Highlighting

### PrettyPrint.js

**Library**: Google Code Prettify
**Location**: `wwwroot/js/prettyprint/`

**Usage**:
```javascript
// After updating script output
psw.controls.codeBlock.textContent = generatedScript;
PR.prettyPrint(); // Apply syntax highlighting
```

**Supported Languages**:
- Bash/Shell
- PowerShell
- Generic code

**CSS Classes**:
- `.prettyprint` - Main class for code blocks
- `.lang-bash` - Language hint
- `.linenums` - Line numbers (optional)

**Customization**:
Custom styles in `prettyprint/prettify.css`:
- Comment color
- Keyword color
- String color
- Function color

---

## Performance Optimizations

### JavaScript Optimizations

1. **Event Delegation**: Use event delegation for dynamic elements
2. **Debouncing**: Delay search filter execution
3. **Lazy Loading**: Load package versions only when selected
4. **Minimal DOM Manipulation**: Batch DOM updates

### Caching Strategy

1. **API Response Caching**: Server-side (60 minutes)
2. **Browser Caching**: Static assets (CSS, JS)
3. **LocalStorage**: User configuration

### Bundle Size

| Asset | Size | Notes |
|-------|------|-------|
| site.js | ~30KB | Main JavaScript |
| Bootstrap | ~150KB | CSS + JS bundle |
| PrettyPrint | ~20KB | Syntax highlighting |
| **Total** | **~200KB** | Uncompressed |

**Optimization Opportunities**:
- Minification (reduce by ~40%)
- Compression (gzip/brotli)
- Code splitting
- Tree shaking unused Bootstrap components

---

## Browser Compatibility

### Supported Browsers

| Browser | Minimum Version |
|---------|----------------|
| Chrome | 90+ |
| Firefox | 88+ |
| Safari | 14+ |
| Edge | 90+ |

### Required APIs

- **Fetch API**: AJAX requests
- **Clipboard API**: Copy functionality
- **History API**: URL updates (`pushState`)
- **LocalStorage**: Configuration persistence
- **ES6**: Arrow functions, `const`/`let`, template literals

### Polyfills

None currently required. Consider adding for older browser support:
- `fetch` polyfill
- `clipboard` polyfill

---

## Future Enhancements

### Planned Features

1. **Dark Mode**: Toggle dark/light theme
2. **Template Preview**: Live preview of generated output
3. **Diff View**: Compare before/after when changing options
4. **Keyboard Shortcuts**: Ctrl+C to copy, Ctrl+G to generate
5. **Progressive Web App**: Offline support, installable
6. **WebSocket**: Real-time collaboration on configurations

### Framework Migration Considerations

Potential migration to modern framework:
- **React**: Component-based architecture
- **Vue**: Progressive enhancement friendly
- **Svelte**: Compile-time framework (smaller bundle)

**Pros**: Better state management, easier testing, modern tooling
**Cons**: Increased complexity, larger bundle size, rebuild required

---

[← Back to Documentation Index](documentation.md)
