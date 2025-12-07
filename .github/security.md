# Security

Security measures and best practices implemented in the Package Script Writer application.

## Table of Contents
- [Security Overview](#security-overview)
- [Security Headers Middleware](#security-headers-middleware)
- [HTTPS and HSTS](#https-and-hsts)
- [Input Validation](#input-validation)
- [No Database = Reduced Attack Surface](#no-database--reduced-attack-surface)
- [Dependency Security](#dependency-security)
- [Best Practices](#best-practices)
- [Security Checklist](#security-checklist)

---

## Security Overview

The application implements multiple security layers:

1. **Security Headers** - Custom middleware adding protective headers
2. **HTTPS Enforcement** - All traffic forced to HTTPS
3. **HSTS** - HTTP Strict Transport Security enabled
4. **Input Validation** - Client and server-side validation
5. **No Database** - Stateless application reduces attack surface
6. **No Sensitive Data Storage** - User credentials not persisted

**Threat Model**: Web application without authentication, focused on script generation

---

## Security Headers Middleware

**File**: `Middleware/SecurityHeadersMiddleware.cs`

### Implementation

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("Referrer-Policy", "no-referrer");
        context.Response.Headers.Add(
            "Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()"
        );

        await _next(context);
    }
}
```

### Registration

```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();
```

---

### Security Headers Explained

#### X-Frame-Options: SAMEORIGIN

**Purpose**: Prevents clickjacking attacks by controlling iframe embedding.

**Values**:
- `DENY` - No iframe embedding allowed
- `SAMEORIGIN` - Only same-origin iframes allowed (current setting)
- `ALLOW-FROM uri` - Specific origin allowed

**Protection Against**:
- Clickjacking attacks
- UI redressing attacks
- Malicious iframe embedding

**Example Attack Prevented**:
```html
<!-- Attacker's site -->
<iframe src="https://psw.codeshare.co.uk"></iframe>
<!-- This iframe will be blocked -->
```

---

#### X-Content-Type-Options: nosniff

**Purpose**: Prevents MIME-sniffing attacks.

**Behavior**: Forces browser to respect `Content-Type` header rather than guessing.

**Protection Against**:
- MIME confusion attacks
- Serving malicious content as safe content type
- XSS via file uploads

**Example Attack Prevented**:
```
Attacker uploads: malicious.txt (contains JavaScript)
Content-Type: text/plain
Without nosniff: Browser might execute as JavaScript
With nosniff: Browser treats as text only
```

---

#### Referrer-Policy: no-referrer

**Purpose**: Prevents leaking referrer information to external sites.

**Values**:
- `no-referrer` - Never send referrer (current setting)
- `no-referrer-when-downgrade` - Send referrer only for HTTPS → HTTPS
- `origin` - Send only origin, not full URL
- `same-origin` - Send referrer only to same origin

**Protection Against**:
- Information leakage via URL parameters
- Tracking across sites
- Privacy concerns

**Example**:
```
User at: https://psw.codeshare.co.uk?packages=secret-package
Clicks external link
Without policy: External site sees full URL with "secret-package"
With no-referrer: External site sees no referrer
```

---

#### Permissions-Policy

**Purpose**: Disables browser features that aren't needed.

**Current Policy**:
```
accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()
```

**Features Disabled**:
| Feature | Reason |
|---------|--------|
| `accelerometer` | Not needed for script generation |
| `camera` | No camera access required |
| `geolocation` | No location tracking |
| `gyroscope` | Not needed |
| `magnetometer` | Not needed |
| `microphone` | No audio features |
| `payment` | No payment processing |
| `usb` | No USB device access |

**Benefits**:
- Reduced attack surface
- Better user privacy
- Clear security posture

---

## HTTPS and HSTS

### HTTPS Redirection

**Middleware**:
```csharp
app.UseHttpsRedirection();
```

**Behavior**:
- All HTTP requests → 301 redirect to HTTPS
- Forces secure connections
- Prevents man-in-the-middle attacks

**Example**:
```
Request: http://psw.codeshare.co.uk
Response: 301 Moved Permanently
Location: https://psw.codeshare.co.uk
```

---

### HTTP Strict Transport Security (HSTS)

**Middleware** (Production only):
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
```

**Header Sent**:
```
Strict-Transport-Security: max-age=2592000
```

**Configuration** (default):
- `max-age`: 30 days (2,592,000 seconds)
- `includeSubDomains`: Not enabled by default
- `preload`: Not enabled by default

**Custom Configuration**:
```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});
```

**Behavior**:
- Browser remembers to only use HTTPS
- Future requests automatically use HTTPS
- Protects against SSL stripping attacks

**HSTS Preload List**:
To be included in browser's built-in HSTS list:
1. Enable `includeSubDomains`
2. Enable `preload`
3. Set `max-age` >= 31536000 (1 year)
4. Submit to: https://hstspreload.org/

---

## Input Validation

### Client-Side Validation

**HTML5 Validation**:
```html
<input type="email" required />
<input type="password" minlength="10" required />
<input type="text" maxlength="100" required />
```

**JavaScript Validation**:
```javascript
// Password strength check
function validatePassword(password) {
    if (password.length < 10) {
        return "Password must be at least 10 characters";
    }
    return null;
}

// Email format check
function validateEmail(email) {
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
```

---

### Server-Side Validation

**Data Annotations**:
```csharp
public class PackagesViewModel
{
    [Required(ErrorMessage = "Project name is required")]
    [MaxLength(100, ErrorMessage = "Project name too long")]
    public string ProjectName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string UserEmail { get; set; }

    [MinLength(10, ErrorMessage = "Password must be at least 10 characters")]
    public string UserPassword { get; set; }
}
```

**Model State Validation**:
```csharp
[HttpPost]
public IActionResult GenerateScript([FromBody] GeneratorApiRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Process valid request
}
```

---

### Validation Rules

| Field | Validation | Error Message |
|-------|-----------|---------------|
| `ProjectName` | Required, MaxLength(100) | "Project name is required" / "Project name too long" |
| `UserEmail` | EmailAddress | "Invalid email format" |
| `UserPassword` | MinLength(10) | "Password must be at least 10 characters" |
| `TemplateName` | Required | "Template name is required" |
| `SolutionName` | Required (if CreateSolutionFile) | "Solution name required" |

---

## No Database = Reduced Attack Surface

### Stateless Architecture

**No Database Means**:
- ✅ No SQL injection vulnerabilities
- ✅ No database authentication attacks
- ✅ No stored XSS attacks
- ✅ No data breach risk (no user data stored)
- ✅ Simpler infrastructure
- ✅ Easier to scale horizontally

**Data Sources**:
1. **In-Memory Cache** - Temporary storage only
2. **External APIs** - Read-only access (NuGet.org, Umbraco Marketplace)
3. **Browser LocalStorage** - Client-side only

---

### No Sensitive Data Storage

**User Credentials**:
- Entered in form
- Sent to API
- Used in script generation
- **Never stored or logged**

**Generated Scripts**:
- Created on-demand
- Not persisted server-side
- Only visible to user

**Configuration URLs**:
- All state in query string
- No server-side session storage
- Shareable without authentication

---

## Dependency Security

### NuGet Package Auditing

**Check for Vulnerabilities**:
```bash
dotnet list package --vulnerable
```

**Update Packages**:
```bash
dotnet add package <PackageName> --version <NewVersion>
```

**Current Dependencies**:
- ASP.NET Core 10.0 (Microsoft-maintained, regular security updates)
- System.Text.Json (Microsoft-maintained)
- No third-party dependencies (minimal attack surface)

---

### Dependency Scanning

**Tools**:
1. **GitHub Dependabot** - Automatic PRs for vulnerable dependencies
2. **Snyk** - Continuous dependency monitoring
3. **OWASP Dependency-Check** - Open-source scanner

**Configuration** (`.github/dependabot.yml`):
```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/src/PSW"
    schedule:
      interval: "weekly"
```

---

## Best Practices

### 1. OWASP Top 10 Mitigation

| OWASP Risk | Mitigation |
|------------|-----------|
| **A01: Broken Access Control** | No authentication required, public API |
| **A02: Cryptographic Failures** | HTTPS enforced, HSTS enabled |
| **A03: Injection** | No database, input validation on all fields |
| **A04: Insecure Design** | Stateless design, minimal data handling |
| **A05: Security Misconfiguration** | Security headers, proper error handling |
| **A06: Vulnerable Components** | Regular dependency updates, minimal dependencies |
| **A07: Authentication Failures** | Not applicable (no authentication) |
| **A08: Software/Data Integrity** | Subresource Integrity for CDN assets (if used) |
| **A09: Logging Failures** | Structured logging, no sensitive data logged |
| **A10: SSRF** | No user-controlled URLs to external resources |

---

### 2. Content Security Policy (CSP)

**Recommendation**: Add CSP header for XSS protection.

**Implementation**:
```csharp
context.Response.Headers.Add(
    "Content-Security-Policy",
    "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' https://marketplace.umbraco.com"
);
```

**Benefits**:
- Prevents inline script execution (XSS protection)
- Controls resource loading
- Reports violations

---

### 3. Secure Coding Practices

**Avoid String Concatenation**:
```csharp
// Bad
var command = "dotnet new " + templateName;

// Good
var command = $"dotnet new {templateName}";
```

**Use Parameterized Queries** (if database added):
```csharp
// Bad
var sql = $"SELECT * FROM Users WHERE Id = {userId}";

// Good
var sql = "SELECT * FROM Users WHERE Id = @UserId";
command.Parameters.AddWithValue("@UserId", userId);
```

**Validate All Input**:
```csharp
// Always validate, even if UI validates
if (string.IsNullOrWhiteSpace(model.ProjectName))
{
    return BadRequest("Project name is required");
}
```

---

### 4. Error Handling

**Don't Leak Information**:
```csharp
// Bad - Exposes internal details
catch (Exception ex)
{
    return BadRequest(ex.ToString());
}

// Good - Generic error message
catch (Exception ex)
{
    _logger.LogError(ex, "Error generating script");
    return StatusCode(500, "An error occurred while generating the script");
}
```

**Production Error Pages**:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
```

---

### 5. Logging Security

**Never Log Sensitive Data**:
```csharp
// Bad
_logger.LogInformation("User password: {Password}", model.UserPassword);

// Good
_logger.LogInformation("User {Email} generated script for {ProjectName}",
    model.UserEmail, model.ProjectName);
```

**Log Security Events**:
- Failed validations
- Unusual request patterns
- API errors
- Cache clearing (admin action)

---

## Security Checklist

### Deployment Checklist

- [ ] HTTPS enabled with valid certificate
- [ ] HSTS enabled (`max-age >= 31536000`)
- [ ] Security headers middleware active
- [ ] Error handling configured (no stack traces in production)
- [ ] Logging configured (no sensitive data logged)
- [ ] Dependencies up to date (no known vulnerabilities)
- [ ] CSP header configured
- [ ] CORS configured (if API used cross-origin)
- [ ] Rate limiting implemented (if high traffic)
- [ ] Monitoring and alerting configured

---

### Regular Security Tasks

**Weekly**:
- [ ] Check for dependency updates
- [ ] Review error logs for anomalies

**Monthly**:
- [ ] Review security headers (use securityheaders.com)
- [ ] Run vulnerability scan (OWASP ZAP, Burp Suite)
- [ ] Update dependencies

**Quarterly**:
- [ ] Security audit
- [ ] Penetration testing
- [ ] Review and update security policies

---

## Security Tools

### Testing Tools

1. **OWASP ZAP** - Security scanner
   ```bash
   zap-cli quick-scan https://psw.codeshare.co.uk
   ```

2. **SecurityHeaders.com** - Header checker
   Visit: https://securityheaders.com/?q=psw.codeshare.co.uk

3. **SSL Labs** - SSL/TLS tester
   Visit: https://www.ssllabs.com/ssltest/

4. **Mozilla Observatory** - Security assessment
   Visit: https://observatory.mozilla.org/

---

### Monitoring Tools

1. **Application Insights** (Azure)
2. **Sentry** - Error tracking
3. **Datadog** - APM and security monitoring
4. **CloudFlare** - DDoS protection and WAF

---

## Incident Response

### Security Incident Plan

1. **Detect**: Monitor logs for suspicious activity
2. **Contain**: Isolate affected systems
3. **Eradicate**: Remove threat
4. **Recover**: Restore normal operations
5. **Learn**: Post-mortem and improvements

### Contact Information

**Security Issues**: Create a private security advisory on GitHub

**Reporting Format**:
- Description of vulnerability
- Steps to reproduce
- Potential impact
- Suggested mitigation

---

## Future Security Enhancements

### Planned Improvements

1. **Content Security Policy** - Add CSP header
2. **Rate Limiting** - Protect against abuse
3. **API Authentication** - Optional API keys for high-volume users
4. **Subresource Integrity** - For CDN assets
5. **Security Monitoring** - Real-time threat detection
6. **WAF Integration** - Web Application Firewall

---

[← Back to Documentation Index](documentation.md)
