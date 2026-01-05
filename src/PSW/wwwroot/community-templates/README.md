# Community Templates

Welcome to the Package Script Writer community templates! This folder contains templates contributed by the community to help others get started quickly with common Umbraco setups.

## How It Works

Community templates are served through the PSW website API:

1. **Contributors** submit templates via pull requests to `src/PSW/wwwroot/community-templates/`
2. **Website** deploys templates and serves them via API at https://psw.codeshare.co.uk
3. **CLI** fetches templates from the API (not directly from GitHub)
4. **Users** can list, view, and use templates with simple commands

This architecture ensures templates are always available, even when offline after initial download (due to caching).

## Using Community Templates

### CLI Mode
```bash
# List all available community templates
psw --community-template list

# Use a community template with overrides
psw --community-template blog-with-usync -n "MyBlog"

# Use a template and auto-run
psw --community-template blog-with-usync -n "MyBlog" --auto-run
```

### Interactive Mode
```bash
psw
# Select: Create script from community template
# Choose your template from the list
# The script will be generated and you can Run, Edit, Copy, or Save it
```

## Contributing Templates

Share your Umbraco setup with the community! Follow these steps to contribute:

### 1. Create Your Template Locally

Use Package Script Writer to create and test your template:

```bash
psw
# Configure your ideal Umbraco setup
# Save it as a template (e.g., "my-awesome-setup")
```

### 2. Export Your Template

Export the template to a YAML file:

```bash
psw template export my-awesome-setup
# This creates my-awesome-setup.yaml in your current directory
```

### 3. Add Metadata

Edit the YAML file and ensure it has complete metadata:

```yaml
metadata:
  name: "My Awesome Setup"
  description: "Clear, concise description of what this template does"
  author: "Your Name or Email"
  created: 2024-12-16T00:00:00Z
  modified: 2024-12-16T00:00:00Z
  version: "1.0.0"
  tags:
    - relevant
    - tags
    - here
```

**Required Metadata:**
- `name`: Clear, descriptive name
- `description`: Explain what this template does and when to use it
- `author`: Your name or email
- `created`: Current date in ISO format
- `version`: Semantic version (start with "1.0.0")
- `tags`: 2-5 relevant tags for discoverability

### 4. Update index.json

Add your template to `src/PSW/wwwroot/community-templates/index.json`:

```json
{
  "name": "my-awesome-setup",
  "displayName": "My Awesome Setup",
  "description": "Clear, concise description of what this template does",
  "author": "Your Name",
  "tags": ["relevant", "tags", "here"],
  "fileName": "my-awesome-setup.yaml",
  "created": "2024-12-16"
}
```

**Important:** Also update the `lastUpdated` field in index.json to today's date.

### 5. Submit Pull Request

1. Fork the Package-Script-Writer repository
2. Create a new branch: `git checkout -b add-template-my-awesome-setup`
3. Add your template YAML file to `src/PSW/wwwroot/community-templates/`
4. Update `src/PSW/wwwroot/community-templates/index.json`
5. Commit your changes with a clear message
6. Push to your fork and create a Pull Request
7. In the PR description, explain:
   - What scenario this template is for
   - What packages are included and why
   - Any special configuration choices

**Note:** Templates are submitted to `src/PSW/wwwroot/community-templates/` so they can be deployed with the website. The CLI fetches templates from the PSW API at https://psw.codeshare.co.uk instead of directly from GitHub.

### 6. Template Review

Maintainers will review your submission for:
- ✅ **Quality**: Clear purpose and documentation
- ✅ **Security**: No hard-coded credentials or sensitive data
- ✅ **Packages**: Only publicly available NuGet packages
- ✅ **Testing**: Template has been tested and works
- ✅ **Metadata**: Complete and accurate information

## Template Naming Convention

### File Names
- Use kebab-case: `my-template-name.yaml`
- Be descriptive: `multi-site-blog.yaml` not `template1.yaml`
- Indicate purpose: `e-commerce-starter.yaml`, `corporate-website.yaml`

### Template Names (in metadata)
- Use Title Case: "My Template Name"
- Be clear and specific
- Keep it concise (under 50 characters)

## Quality Standards

### ✅ Good Template

A quality community template should:
- Have a clear, specific purpose
- Use current, stable package versions
- Include helpful tags for discovery
- Have a descriptive name and detailed description
- Be tested and verified to work
- Follow Umbraco best practices
- Include only necessary packages

### ❌ Avoid

Please don't submit templates that:
- Use outdated package versions
- Have vague or unclear descriptions
- Include hard-coded credentials or API keys
- Use uncommon or private packages
- Are overly generic ("basic setup")
- Haven't been tested

## Template Structure

All templates use the same YAML structure:

```yaml
metadata:
  name: "Template Name"
  description: "What this template does"
  author: "Your Name"
  created: 2024-12-16T00:00:00Z
  modified: 2024-12-16T00:00:00Z
  version: "1.0.0"
  tags:
    - tag1
    - tag2

configuration:
  template:
    name: "Umbraco.Templates"
    version: "14.3.0"

  project:
    name: "MyProject"
    createSolution: true
    solutionName: "MyProject"

  packages:
    - name: "PackageName"
      version: "1.0.0"

  starterKit:
    enabled: false
    package: null

  docker:
    dockerfile: false
    dockerCompose: false

  unattended:
    enabled: false
    database:
      type: "SQLite"
      connectionString: null
    admin:
      name: "Administrator"
      email: "admin@example.com"
      password: "<prompt>"

  output:
    oneliner: false
    removeComments: false
    includePrerelease: false

  execution:
    autoRun: false
    runDirectory: "."
```

## Template Ideas

Need inspiration? Here are some useful template ideas:

- **Blog Setup**: Basic blog with content management packages
- **E-commerce**: Shop setup with payment and inventory packages
- **Multi-site**: Configuration for managing multiple sites
- **Content Sync**: Templates with uSync for staging/production workflows
- **Headless CMS**: API-first setup with authentication
- **Forms & Workflow**: Heavy form processing with workflows
- **Multilingual**: Multi-language site configuration
- **SEO Optimized**: SEO-focused package selection
- **Developer Tools**: Debugging and development packages

## Security & Privacy

### Do NOT Include:
- ❌ API keys or tokens
- ❌ Passwords (use `<prompt>` instead)
- ❌ Database connection strings with credentials
- ❌ Email addresses (except example.com)
- ❌ Internal URLs or IP addresses
- ❌ License keys

### Safe Practices:
- ✅ Use `<prompt>` for passwords (user will be asked)
- ✅ Use `<env:VAR_NAME>` for environment variables
- ✅ Use example.com for email domains
- ✅ Use SQLite as default database (no credentials needed)
- ✅ Leave sensitive fields empty/null

## Getting Help

- **Questions**: Open a discussion in the repository
- **Issues**: Report problems with existing templates
- **Ideas**: Suggest new template ideas in discussions

## License

All community templates are shared under the same license as Package Script Writer. By contributing, you agree to license your template accordingly.

---

Thank you for contributing to the Package Script Writer community! 🎉
