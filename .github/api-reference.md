# API Reference

Complete reference documentation for all API endpoints in the Package Script Writer application.

## Table of Contents
- [Interactive Documentation (Swagger UI)](#interactive-documentation-swagger-ui)
- [Base URL](#base-url)
- [API Endpoints](#api-endpoints)
  - [Generate Script](#generate-script)
  - [Get Package Versions](#get-package-versions)
  - [Clear Cache](#clear-cache)
  - [Health Check](#health-check)
- [Request/Response Examples](#requestresponse-examples)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)

---

## Interactive Documentation (Swagger UI)

The **easiest way** to explore and test the API is through the built-in Swagger UI interface. The API includes comprehensive **OpenAPI annotations** on all controllers and endpoints, providing rich, interactive documentation.

### Key Features

- 📖 **View all endpoints**: Complete API documentation with detailed descriptions
- 🧪 **Test API calls**: Execute requests directly from your browser
- 📝 **Request/response examples**: See realistic data samples
- 🔍 **Explore schemas**: Detailed structure of all data models
- 📄 **OpenAPI specification**: Download the complete API spec
- 🏷️ **Operation tags**: Organized endpoints by category
- 📋 **Parameter descriptions**: Detailed information for each parameter
- ✅ **Response codes**: All possible HTTP status codes with descriptions

### Accessing Swagger UI

**Production**: [https://psw.codeshare.co.uk/api/docs](https://psw.codeshare.co.uk/api/docs)
**Development**: [https://localhost:5001/api/docs](https://localhost:5001/api/docs)

### OpenAPI Annotations

All API controllers are fully annotated with OpenAPI attributes for comprehensive documentation:

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Script Generator")]
public class ScriptGeneratorApiController : ControllerBase
{
    [HttpPost("generatescript")]
    [ProducesResponseType(typeof(ScriptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GenerateScript([FromBody] ScriptRequest request)
    {
        // Implementation
    }
}
```

**Benefits**:
- Rich, accurate documentation generated directly from code
- Type-safe request/response definitions
- Clear HTTP status code documentation
- Automatic schema validation
- Version tracking and change detection

### Technology Stack

The Swagger/OpenAPI implementation uses:
- **Swashbuckle.AspNetCore** - OpenAPI document generation
- **OpenAPI annotations** - Controller and method-level documentation
- **XML documentation** - Enhanced descriptions from code comments
- **JSON Schema** - Automatic model schema generation

### Using Swagger UI

1. **Start the application**:
   ```bash
   dotnet watch run --project ./src/PSW/
   ```

2. **Open Swagger UI** in your browser:
   ```
   https://localhost:5001/api/docs
   ```

3. **Explore endpoints**:
   - Click on any endpoint to expand it
   - View parameters, request body, and response schemas
   - See all possible response codes

4. **Test an endpoint**:
   - Click "Try it out" button
   - Fill in the parameters or request body
   - Click "Execute"
   - View the response with status code, headers, and body

5. **Download OpenAPI specification**:
   - Click the link at the top to download the JSON specification
   - Use with tools like Postman, Insomnia, or code generators

---

## Base URL

**Production**: `https://psw.codeshare.co.uk`
**Development**: `https://localhost:5001`

All API endpoints are prefixed with `/api/scriptgeneratorapi`

---

## API Endpoints

### Generate Script

Generates a complete installation script based on provided configuration.

**Endpoint**: `POST /api/scriptgeneratorapi/generatescript`

**Content-Type**: `application/json`

**Request Body**:
```json
{
  "model": {
    "templateName": "string",
    "templateVersion": "string",
    "createSolutionFile": boolean,
    "solutionName": "string",
    "projectName": "string",
    "useUnattendedInstall": boolean,
    "databaseType": "string",
    "connectionString": "string",
    "userFriendlyName": "string",
    "userEmail": "string",
    "userPassword": "string",
    "packages": "string",
    "includeStarterKit": boolean,
    "starterKitPackage": "string",
    "canIncludeDocker": boolean,
    "includeDockerfile": boolean,
    "includeDockerCompose": boolean,
    "onelinerOutput": boolean,
    "removeComments": boolean
  }
}
```

**Request Parameters**:

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `templateName` | string | Yes | Template package name | `"Umbraco.Templates"` |
| `templateVersion` | string | No | Specific version or "LTS" | `"14.3.0"` or `"LTS"` |
| `createSolutionFile` | boolean | No | Create solution file | `true` |
| `solutionName` | string | No | Solution name | `"MySolution"` |
| `projectName` | string | Yes | Project name | `"MyProject"` |
| `useUnattendedInstall` | boolean | No | Use unattended install | `true` |
| `databaseType` | string | No | Database type | `"SQLite"`, `"LocalDb"`, `"SQLServer"`, `"SQLAzure"`, `"SQLCE"` |
| `connectionString` | string | No | Database connection string | `""` |
| `userFriendlyName` | string | No | Admin user name | `"Administrator"` |
| `userEmail` | string | No | Admin email | `"admin@example.com"` |
| `userPassword` | string | No | Admin password (10+ chars) | `"Password123"` |
| `packages` | string | No | Comma-separated packages | `"Package1|1.0.0,Package2|2.0.0"` |
| `includeStarterKit` | boolean | No | Include starter kit | `false` |
| `starterKitPackage` | string | No | Starter kit package name | `"Umbraco.TheStarterKit"` |
| `canIncludeDocker` | boolean | No | Can include Docker | `true` |
| `includeDockerfile` | boolean | No | Include Dockerfile | `false` |
| `includeDockerCompose` | boolean | No | Include Docker Compose | `false` |
| `onelinerOutput` | boolean | No | Output as one-liner | `false` |
| `removeComments` | boolean | No | Remove comment lines | `false` |

**Response**:
```json
{
  "script": "# Ensure we have the version specific Umbraco templates\ndotnet new install Umbraco.Templates::14.3.0 --force\n\n# Create solution/project\ndotnet new sln --name \"MySolution\"\ndotnet new umbraco --force -n \"MyProject\" --friendly-name \"Administrator\" --email \"admin@example.com\" --password \"Password123\" --development-database-type SQLite\ndotnet sln add \"MyProject\"\n\n#Add Packages\ndotnet add \"MyProject\" package Umbraco.Community.BlockPreview --version 1.6.0\n\ndotnet run --project \"MyProject\"\n#Running"
}
```

**Status Codes**:
- `200 OK` - Script generated successfully
- `400 Bad Request` - Invalid model data
- `500 Internal Server Error` - Server error during generation

**Example Request**:
```bash
curl -X POST https://psw.codeshare.co.uk/api/scriptgeneratorapi/generatescript \
  -H "Content-Type: application/json" \
  -d '{
    "model": {
      "templateName": "Umbraco.Templates",
      "templateVersion": "14.3.0",
      "projectName": "MyBlog",
      "useUnattendedInstall": true,
      "databaseType": "SQLite",
      "userFriendlyName": "Admin",
      "userEmail": "admin@example.com",
      "userPassword": "SuperSecret123",
      "packages": "Umbraco.Community.BlockPreview|1.6.0"
    }
  }'
```

---

### Get Package Versions

Retrieves available versions for a specific NuGet package.

**Endpoint**: `POST /api/scriptgeneratorapi/getpackageversions`

**Content-Type**: `application/json`

**Request Body**:
```json
{
  "packageId": "string",
  "includePrerelease": boolean
}
```

**Request Parameters**:

| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `packageId` | string | Yes | NuGet package ID | `"Umbraco.Community.BlockPreview"` |
| `includePrerelease` | boolean | No | Include prerelease versions | `false` |

**Response**:
```json
{
  "versions": [
    "1.6.0",
    "1.5.0",
    "1.4.0",
    "1.3.0",
    "1.2.0",
    "1.1.0",
    "1.0.0"
  ]
}
```

**Status Codes**:
- `200 OK` - Versions retrieved successfully
- `400 Bad Request` - Invalid package ID
- `404 Not Found` - Package not found on NuGet.org
- `500 Internal Server Error` - Error fetching versions

**Caching**: Results are cached for 60 minutes

**Example Request**:
```bash
curl -X POST https://psw.codeshare.co.uk/api/scriptgeneratorapi/getpackageversions \
  -H "Content-Type: application/json" \
  -d '{
    "packageId": "Umbraco.Community.BlockPreview",
    "includePrerelease": false
  }'
```

**Example Response**:
```json
{
  "versions": [
    "1.6.0",
    "1.5.0",
    "1.4.0"
  ]
}
```

---

### Clear Cache

Clears all cached data (packages, versions, templates).

**Endpoint**: `GET /api/scriptgeneratorapi/clearcache`

**Method**: GET

**Authentication**: None (should be protected in production)

**Response**:
```json
{
  "message": "Cache cleared successfully"
}
```

**Status Codes**:
- `200 OK` - Cache cleared successfully
- `500 Internal Server Error` - Error clearing cache

**Example Request**:
```bash
curl https://psw.codeshare.co.uk/api/scriptgeneratorapi/clearcache
```

**Use Cases**:
- Manual cache refresh after NuGet package updates
- Troubleshooting stale data issues
- Forced refresh of Marketplace packages

**Note**: This endpoint should be restricted to administrators in production environments.

---

### Health Check

Simple endpoint to verify API availability.

**Endpoint**: `GET /api/scriptgeneratorapi/test`

**Method**: GET

**Response**:
```json
{
  "status": "ok",
  "timestamp": "2025-12-04T12:00:00Z"
}
```

**Status Codes**:
- `200 OK` - API is operational

**Example Request**:
```bash
curl https://psw.codeshare.co.uk/api/scriptgeneratorapi/test
```

**Use Cases**:
- Health monitoring
- Load balancer health checks
- Integration testing

---

## Request/Response Examples

### Example 1: Basic Umbraco Project

**Request**:
```json
{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "14.3.0",
    "projectName": "BasicProject"
  }
}
```

**Response**:
```json
{
  "script": "# Ensure we have the version specific Umbraco templates\ndotnet new install Umbraco.Templates::14.3.0 --force\n\ndotnet new umbraco --force -n \"BasicProject\"\n\ndotnet run --project \"BasicProject\"\n#Running"
}
```

---

### Example 2: Full Featured Project with Solution

**Request**:
```json
{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "LTS",
    "createSolutionFile": true,
    "solutionName": "MyCompany.Web",
    "projectName": "MyCompany.Web.Site",
    "useUnattendedInstall": true,
    "databaseType": "SQLite",
    "userFriendlyName": "Site Administrator",
    "userEmail": "admin@mycompany.com",
    "userPassword": "P@ssw0rd123!",
    "packages": "Umbraco.Community.BlockPreview|1.6.0,Diplo.GodMode|3.0.3,uSync|12.0.0",
    "includeDockerfile": true,
    "includeDockerCompose": true,
    "canIncludeDocker": true
  }
}
```

**Response** (abbreviated):
```json
{
  "script": "# Ensure we have the version specific Umbraco templates\ndotnet new install Umbraco.Templates::14.3.0 --force\n\n# Create solution/project\ndotnet new sln --name \"MyCompany.Web\"\ndotnet new umbraco --force -n \"MyCompany.Web.Site\" --add-docker --friendly-name \"Site Administrator\" --email \"admin@mycompany.com\" --password \"P@ssw0rd123!\" --development-database-type SQLite\ndotnet sln add \"MyCompany.Web.Site\"\n\n#Add Docker Compose\ndotnet new umbraco-compose -P \"MyCompany.Web.Site\"\n\n#Add Packages\ndotnet add \"MyCompany.Web.Site\" package Umbraco.Community.BlockPreview --version 1.6.0\ndotnet add \"MyCompany.Web.Site\" package Diplo.GodMode --version 3.0.3\ndotnet add \"MyCompany.Web.Site\" package uSync --version 12.0.0\n\ndotnet run --project \"MyCompany.Web.Site\"\n#Running"
}
```

---

### Example 3: One-liner Output without Comments

**Request**:
```json
{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "14.3.0",
    "projectName": "QuickSetup",
    "onelinerOutput": true,
    "removeComments": true
  }
}
```

**Response**:
```json
{
  "script": "dotnet new install Umbraco.Templates::14.3.0 --force && dotnet new umbraco --force -n \"QuickSetup\" && dotnet run --project \"QuickSetup\""
}
```

---

### Example 4: Project with Starter Kit

**Request**:
```json
{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "14.3.0",
    "projectName": "BlogSite",
    "includeStarterKit": true,
    "starterKitPackage": "Umbraco.TheStarterKit"
  }
}
```

**Response**:
```json
{
  "script": "# Ensure we have the version specific Umbraco templates\ndotnet new install Umbraco.Templates::14.3.0 --force\n\ndotnet new umbraco --force -n \"BlogSite\"\n\n#Add starter kit\ndotnet add \"BlogSite\" package Umbraco.TheStarterKit\n\ndotnet run --project \"BlogSite\"\n#Running"
}
```

---

## Error Handling

### Error Response Format

```json
{
  "error": "string",
  "message": "string",
  "statusCode": number
}
```

### Common Error Scenarios

#### 400 Bad Request - Invalid Model

**Cause**: Missing required fields or invalid data

**Response**:
```json
{
  "error": "Bad Request",
  "message": "ProjectName is required",
  "statusCode": 400
}
```

#### 404 Not Found - Package Not Found

**Cause**: Package ID doesn't exist on NuGet.org

**Response**:
```json
{
  "error": "Not Found",
  "message": "Package 'NonExistentPackage' not found",
  "statusCode": 404
}
```

#### 500 Internal Server Error

**Cause**: Server-side error during processing

**Response**:
```json
{
  "error": "Internal Server Error",
  "message": "An error occurred while generating the script",
  "statusCode": 500
}
```

---

## Rate Limiting

Currently, there are **no rate limits** enforced on the API. However, the following best practices are recommended:

### Recommended Usage Patterns

1. **Cache Responses**: Cache generated scripts client-side
2. **Debounce Requests**: Wait for user to finish typing before calling API
3. **Batch Version Requests**: Request versions only when package is selected

### Future Rate Limiting Plans

Potential rate limits for future implementation:

| Endpoint | Limit | Window |
|----------|-------|--------|
| `/generatescript` | 100 requests | 1 hour |
| `/getpackageversions` | 200 requests | 1 hour |
| `/clearcache` | 10 requests | 1 hour |

---

## API Testing

### Using Swagger UI (Recommended)

The easiest way to test the API is through Swagger UI:

1. Start the application: `dotnet watch run --project ./src/PSW/`
2. Navigate to: [https://localhost:5001/api/docs](https://localhost:5001/api/docs)
3. Click on any endpoint to expand it
4. Click "Try it out" button
5. Fill in the parameters
6. Click "Execute" to send the request
7. View the response directly in the browser

### Using REST Client (VS Code Extension)

Create a file `api-tests.http`:

```http
### Generate Basic Script
POST https://psw.codeshare.co.uk/api/scriptgeneratorapi/generatescript
Content-Type: application/json

{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "14.3.0",
    "projectName": "TestProject"
  }
}

### Get Package Versions
POST https://psw.codeshare.co.uk/api/scriptgeneratorapi/getpackageversions
Content-Type: application/json

{
  "packageId": "Umbraco.Community.BlockPreview",
  "includePrerelease": false
}

### Health Check
GET https://psw.codeshare.co.uk/api/scriptgeneratorapi/test

### Clear Cache
GET https://psw.codeshare.co.uk/api/scriptgeneratorapi/clearcache
```

### Using Postman

1. Import the API collection
2. Set base URL variable: `{{baseUrl}}`
3. Create requests for each endpoint
4. Use environment variables for different environments

### Using cURL

**PowerShell**:
```powershell
$body = @{
    model = @{
        templateName = "Umbraco.Templates"
        templateVersion = "14.3.0"
        projectName = "TestProject"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://psw.codeshare.co.uk/api/scriptgeneratorapi/generatescript" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

**Bash**:
```bash
curl -X POST https://psw.codeshare.co.uk/api/scriptgeneratorapi/generatescript \
  -H "Content-Type: application/json" \
  -d '{"model":{"templateName":"Umbraco.Templates","templateVersion":"14.3.0","projectName":"TestProject"}}'
```

---

## API Versioning

Currently, the API is **unversioned**. All endpoints are under `/api/scriptgeneratorapi`.

### Future Versioning Strategy

Planned versioning approach:

- **v1**: `/api/v1/scriptgeneratorapi`
- **v2**: `/api/v2/scriptgeneratorapi` (with breaking changes)

**Backward Compatibility**: v1 will remain available for at least 12 months after v2 release.

---

## CORS Configuration

**Current Policy**: CORS is not explicitly configured (same-origin by default)

**Cross-Origin Requests**: Not currently supported

**Future Enhancement**: Configure CORS to allow specific origins for API-only usage.

---

## Authentication & Authorization

**Current Status**: No authentication required

**Future Plans**:
- API key authentication for high-volume users
- Rate limiting per API key
- Admin-only endpoints (e.g., cache clearing)

---

[← Back to Documentation Index](documentation.md)
