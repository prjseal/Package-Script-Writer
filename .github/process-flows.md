# Process Flows

This document contains all the process flow diagrams that illustrate how the Package Script Writer application works.

## Table of Contents
- [User Interaction Flow](#user-interaction-flow)
- [Script Generation Flow](#script-generation-flow)
- [Package Version Retrieval Flow](#package-version-retrieval-flow)
- [URL Query String Synchronization](#url-query-string-synchronization)
- [Application Startup & Dependency Injection](#application-startup--dependency-injection)
- [Script Generation Logic](#script-generation-logic)
- [Event Flow](#event-flow)
- [External API Integration](#external-api-integration)
- [Middleware Pipeline](#middleware-pipeline)
- [Error Handling Flow](#error-handling-flow)

---

## User Interaction Flow

This sequence diagram shows the complete user journey from visiting the site to generating and copying a script.

```mermaid
sequenceDiagram
    participant User
    participant Browser
    participant JavaScript
    participant API
    participant Service
    participant Cache
    participant External

    User->>Browser: Visit site
    Browser->>API: Load packages
    API->>Cache: Check cache
    alt Cache hit
        Cache-->>API: Return cached data
    else Cache miss
        API->>Service: Get packages
        Service->>External: Fetch from Marketplace
        External-->>Service: Package data
        Service->>Cache: Store in cache (60 min)
        Service-->>API: Return packages
    end
    API-->>Browser: Display packages

    User->>Browser: Select options
    JavaScript->>JavaScript: Update URL query string

    User->>Browser: Click "Generate"
    JavaScript->>API: POST /generatescript
    API->>Service: GenerateScript(model)
    Service->>Service: Build script commands
    Service-->>API: Return script string
    API-->>JavaScript: JSON response
    JavaScript->>Browser: Display formatted script

    User->>Browser: Click "Copy"
    Browser->>Browser: Copy to clipboard
```

---

## Script Generation Flow

This flowchart shows the detailed logic for generating installation scripts based on user selections.

```mermaid
flowchart TD
    Start([User Clicks Generate]) --> A{Template Selected?}
    A -->|Yes| B[Generate Template Install Command]
    A -->|No| Skip1[Skip Template Section]

    B --> C{Create Solution File?}
    C -->|Yes| D[Generate Solution Creation Command]
    C -->|No| E[Generate Project Creation Command]

    D --> E
    E --> F[Add Project to Solution]

    F --> G{Umbraco Template?}
    G -->|Yes| H[Generate Docker Compose if enabled]
    G -->|No| Skip2[Skip Docker]

    H --> I{Include Starter Kit?}
    Skip2 --> I
    I -->|Yes| J[Add Starter Kit Package Command]
    I -->|No| K[Process Package List]

    J --> K
    K --> L{Has Packages?}
    L -->|Yes| M[Loop Through Packages]
    L -->|No| N[Generate Run Command]

    M --> M1[Parse Package Name & Version]
    M1 --> M2{Is Starter Kit?}
    M2 -->|Yes| M3[Skip - Already Added]
    M2 -->|No| M4[Add Package Install Command]
    M4 --> M5{More Packages?}
    M5 -->|Yes| M1
    M5 -->|No| N

    N --> O{Remove Comments?}
    O -->|Yes| P[Filter Out # Lines]
    O -->|No| Q{One-liner Output?}

    P --> Q
    Q -->|Yes| R[Join with &&]
    Q -->|No| S[Join with Newlines]

    R --> End([Return Script])
    S --> End

    style Start fill:#e1f5ff
    style End fill:#e8f5e9
    style B fill:#fff4e1
    style E fill:#fff4e1
    style K fill:#fff4e1
```

---

## Package Version Retrieval Flow

This flowchart illustrates how package versions are fetched from NuGet.org with caching.

```mermaid
flowchart TD
    Start([API Request: Get Package Versions]) --> A[Extract Package ID from Request]

    A --> B{Check Cache}
    B -->|Hit| C[Return Cached Versions]
    B -->|Miss| D[Call NuGet.org API]

    D --> E{Include Prerelease?}
    E -->|Yes| F["GET /v3-flatcontainer/(id)/index.json"]
    E -->|No| G["GET /query?q=(id)&prerelease=false"]

    F --> H[Parse JSON Response]
    G --> H

    H --> I[Extract Version Array]
    I --> J{Valid Response?}
    J -->|Yes| K[Store in Cache - 60 min TTL]
    J -->|No| L[Return Empty Array]

    K --> M[Return Version List]
    L --> End([Return to Client])
    M --> End

    C --> End

    style Start fill:#e1f5ff
    style End fill:#e8f5e9
```

---

## URL Query String Synchronization

This flowchart shows how form state is synchronized with the browser URL for shareable configurations.

```mermaid
flowchart LR
    A[User Changes Form] --> B[JavaScript Event Listener]
    B --> C{Update Type?}

    C -->|Package Selection| D[Serialize Package IDs & Versions]
    C -->|Template Change| E[Update Template Parameters]
    C -->|Options Change| F[Update Option Parameters]

    D --> G[Build Query String]
    E --> G
    F --> G

    G --> H[Update Browser URL - history.pushState]
    H --> I{User Clicks Save?}
    I -->|Yes| J[Store to localStorage]
    I -->|No| K[Keep in URL Only]

    J --> End([Shareable Configuration])
    K --> End

    style A fill:#e1f5ff
    style End fill:#e8f5e9
```

---

## Application Startup & Dependency Injection

This flowchart shows the application initialization process and service registration.

```mermaid
flowchart TD
    Start([Application Start]) --> A[Load Program.cs]
    A --> B[Create WebApplicationBuilder]

    B --> C[Register Services]
    C --> C1[AddControllersWithViews]
    C1 --> C2[AddHttpClient]
    C2 --> C3[AddScoped: IScriptGeneratorService]
    C3 --> C4[AddScoped: IPackageService]
    C4 --> C5[AddScoped: IQueryStringService]
    C5 --> C6[AddScoped: IUmbracoVersionService]

    C6 --> D[Configure PSWConfig from appsettings.json]
    D --> E[Build Application]

    E --> F{Environment Check}
    F -->|Production| G[UseExceptionHandler]
    F -->|Development| H[Skip Error Handler]

    G --> I[UseHsts]
    H --> I
    I --> J[UseHttpsRedirection]
    J --> K[UseMiddleware: SecurityHeadersMiddleware]
    K --> L[UseStaticFiles]
    L --> M[UseRouting]
    M --> N[UseAuthorization]
    N --> O[MapControllerRoute - Default Pattern]
    O --> End([Application Running])

    style Start fill:#e1f5ff
    style End fill:#e8f5e9
    style C3 fill:#fff4e1
    style C4 fill:#fff4e1
    style C5 fill:#fff4e1
    style C6 fill:#fff4e1
```

---

## Script Generation Logic

This flowchart shows the internal logic flow within the ScriptGeneratorService.

```mermaid
flowchart TD
    A[GenerateScript] --> B{Template Name Exists?}
    B -->|Yes| C[GenerateUmbracoTemplatesSectionScript]
    B -->|No| J[Skip Templates]

    C --> D[GenerateCreateSolutionFileScript]
    D --> E[GenerateCreateProjectScript]
    E --> F[GenerateAddProjectToSolutionScript]

    F --> G{Is Umbraco Template?}
    G -->|Yes| H[GenerateAddDockerComposeScript]
    G -->|No| K[GenerateAddPackagesScript]

    H --> I[GenerateAddStarterKitScript]
    I --> K
    J --> K

    K --> L[GenerateRunProjectScript]
    L --> M{RemoveComments?}
    M -->|Yes| N[Filter Comment Lines]
    M -->|No| O{OnelinerOutput?}

    N --> O
    O -->|Yes| P[Join with &&]
    O -->|No| Q[Join with Newlines]

    P --> R[Return Script String]
    Q --> R

    style A fill:#fff4e1
    style R fill:#e8f5e9
```

---

## Event Flow

This sequence diagram shows the detailed event flow when a user changes form controls.

```mermaid
sequenceDiagram
    participant User
    participant DOM
    participant psw.js
    participant API
    participant Cache

    User->>DOM: Change template dropdown
    DOM->>psw.js: 'change' event
    psw.js->>psw.js: toggleTemplateNameControls()
    psw.js->>psw.js: toggleDockerControls()
    psw.js->>psw.js: updateUrl()
    psw.js->>API: POST /api/scriptgeneratorapi/generatescript
    API->>API: GenerateScript(model)
    API-->>psw.js: { script: "dotnet new..." }
    psw.js->>DOM: Update <pre> with script
    psw.js->>DOM: Apply syntax highlighting

    User->>DOM: Check package checkbox
    DOM->>psw.js: 'change' event
    psw.js->>psw.js: getPackageVersionsForCheckbox()
    psw.js->>API: POST /api/scriptgeneratorapi/getpackageversions
    API->>Cache: Check cache
    alt Cache hit
        Cache-->>API: Return versions
    else Cache miss
        API->>API: Call NuGet.org
        API->>Cache: Store versions
    end
    API-->>psw.js: { versions: ["1.0", "2.0"] }
    psw.js->>DOM: Populate version dropdown
    psw.js->>psw.js: updateOutput()
```

---

## External API Integration

This sequence diagram shows how the application integrates with external APIs.

```mermaid
sequenceDiagram
    participant PSW as Package Script Writer
    participant Cache as IMemoryCache
    participant Marketplace as Umbraco Marketplace
    participant NuGet as NuGet.org

    PSW->>Cache: Check all-packages cache
    alt Cache Miss
        PSW->>Marketplace: GET /api/marketplaceapi/getallpackages
        Marketplace-->>PSW: JSON with 150+ packages
        PSW->>Cache: Store for 60 minutes
    end
    Cache-->>PSW: Return packages

    Note over PSW: User selects package

    PSW->>Cache: Check package-versions-(id) cache
    alt Cache Miss
        PSW->>NuGet: GET /v3-flatcontainer/(id)/index.json
        NuGet-->>PSW: JSON with version array
        PSW->>Cache: Store for 60 minutes
    end
    Cache-->>PSW: Return versions
```

---

## Middleware Pipeline

This diagram shows the HTTP request processing pipeline.

```mermaid
graph LR
    A[Request] --> B{Environment}
    B -->|Production| C[ExceptionHandler]
    B -->|Development| D[Developer Exception Page]
    C --> E[HSTS]
    D --> E
    E --> F[HTTPS Redirection]
    F --> G[SecurityHeadersMiddleware]
    G --> H[Static Files]
    H --> I[Routing]
    I --> J[Authorization]
    J --> K[MVC Controller]
    K --> L[Response]

    style A fill:#e1f5ff
    style G fill:#fff4e1
    style L fill:#e8f5e9
```

---

## Error Handling Flow

This diagram illustrates how errors are handled based on the environment.

```mermaid
graph LR
    A[Request] --> B{Environment}
    B -->|Development| C[Developer Exception Page]
    B -->|Production| D[Custom Error Handler]
    C --> E[Detailed Stack Trace]
    D --> F[Generic Error Page]
    E --> G[Response]
    F --> G

    style A fill:#e1f5ff
    style D fill:#fff4e1
    style G fill:#e8f5e9
```

**Error Handling Strategy**:
- **Development**: Full stack traces for debugging (`/Home/Error` with detailed info)
- **Production**: User-friendly error pages without sensitive information
- **API Errors**: JSON responses with error messages
- **Validation Errors**: Model state errors returned to client

---

## Data Flow Summary

Here's a high-level view of how data flows through the system:

1. **User Input** → JavaScript captures form state
2. **URL Sync** → Query string updated via `history.pushState()`
3. **API Call** → POST to `/api/scriptgeneratorapi/generatescript`
4. **Service Layer** → `ScriptGeneratorService.GenerateScript()`
5. **External APIs** → Fetch package data if needed (with caching)
6. **Response** → JSON with generated script
7. **Display** → Update DOM with syntax-highlighted output

## Performance Considerations

### Caching Impact on Flow

```mermaid
graph TD
    A[Request] --> B{Cache Hit?}
    B -->|Yes| C[Return in ~5ms]
    B -->|No| D[API Call - ~500ms]
    D --> E[Store in Cache]
    E --> C

    style C fill:#e8f5e9
    style D fill:#fff4e1
```

**Performance Metrics**:
- Cached requests: ~5ms
- Non-cached requests: ~500ms
- Cache duration: 60 minutes
- Performance improvement: 100x

---

[← Back to Documentation Index](documentation.md)
