# CLI Tool Release Process

This document describes the automated release process for publishing the Package Script Writer CLI tool to NuGet.org.

## Overview

The CLI tool (`PackageScriptWriter.Cli`) is automatically packaged and published to NuGet.org whenever a new GitHub release is created. This is handled by the `release-cli-nuget.yml` GitHub Actions workflow.

## Prerequisites

Before creating a release, ensure the following secret is configured in your GitHub repository:

- **`NUGET_API_KEY`**: Your NuGet.org API key with push permissions
  - Go to Settings → Secrets and variables → Actions → New repository secret
  - Get your API key from https://www.nuget.org/account/apikeys

## Creating a Release

### 1. Determine the Version Number

Follow [Semantic Versioning](https://semver.org/) (MAJOR.MINOR.PATCH):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes (backwards compatible)

For pre-releases, append a suffix: `1.0.0-beta`, `1.0.0-rc.1`, etc.

### 2. Create the Release in GitHub

1. Go to your repository on GitHub
2. Click on "Releases" → "Draft a new release"
3. Click "Choose a tag" and create a new tag:
   - Format: `v{VERSION}` (e.g., `v1.0.0`, `v1.0.0-beta`)
   - The 'v' prefix is optional but recommended
4. Fill in the release details:
   - **Release title**: Descriptive name (e.g., "v1.0.0 - Initial Release")
   - **Description**: Changelog, features, bug fixes, breaking changes, etc.
5. Check "Set as a pre-release" if applicable (for beta, rc, etc.)
6. Click "Publish release"

### 3. Automated Workflow

Once you publish the release, the GitHub Actions workflow will automatically:

1. ✅ Extract the version from the release tag
2. ✅ Restore dependencies
3. ✅ Build the CLI tool with the specified version
4. ✅ Pack the CLI tool as a NuGet package
5. ✅ Upload the package as a workflow artifact
6. ✅ Publish the package to NuGet.org
7. ✅ Attach the `.nupkg` file to the GitHub release
8. ✅ Display a release summary

### 4. Monitor the Workflow

1. Go to the "Actions" tab in your GitHub repository
2. Click on the "Release - Publish CLI Tool to NuGet.org" workflow run
3. Monitor the progress and check for any errors
4. The workflow typically takes 2-5 minutes to complete

### 5. Verify Publication

After the workflow completes successfully:

1. **Check NuGet.org**: Visit https://www.nuget.org/packages/PackageScriptWriter.Cli
   - The new version should appear (may take a few minutes to index)
2. **Test Installation**:
   ```bash
   dotnet tool install --global PackageScriptWriter.Cli --version {VERSION}
   ```
3. **Check GitHub Release**: The `.nupkg` file should be attached to the release

## Version Management

The workflow automatically:

- Extracts the version from the Git tag (removing the 'v' prefix if present)
- Validates the version format (must match `X.Y.Z` or `X.Y.Z-suffix`)
- Applies the version to the NuGet package
- Detects if it's a prerelease based on:
  - The "Set as a pre-release" checkbox in GitHub
  - Version suffix (e.g., `-beta`, `-rc.1`)

## Troubleshooting

### Workflow Fails

1. Check the workflow logs in the Actions tab
2. Common issues:
   - **Missing NuGet API Key**: Ensure `NUGET_API_KEY` secret is set
   - **Invalid Version Format**: Tag must follow `X.Y.Z` or `X.Y.Z-suffix` format
   - **Build Errors**: Check for compilation issues
   - **Duplicate Package**: Version already exists on NuGet (use `--skip-duplicate` flag)

### Package Not Appearing on NuGet.org

- NuGet.org indexing can take 5-15 minutes
- Check the workflow logs to confirm successful publication
- Verify your API key has push permissions

### Manual Publishing (Fallback)

If the automated workflow fails, you can publish manually:

```bash
# Navigate to the CLI tool project
cd src/PackageCliTool

# Pack the tool with a specific version
dotnet pack --configuration Release /p:Version=1.0.0

# Publish to NuGet
dotnet nuget push bin/Release/PackageScriptWriter.Cli.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Example Release Workflow

1. Finish development and merge all changes to main
2. Update changelog/documentation if needed
3. Decide on version: `1.2.0` (new feature release)
4. Create GitHub release with tag `v1.2.0`
5. Add release notes describing the changes
6. Publish release
7. Wait for workflow to complete (~3 minutes)
8. Verify on NuGet.org
9. Announce the new version to users

## Best Practices

- **Test Before Release**: Ensure all tests pass before creating a release
- **Semantic Versioning**: Follow SemVer strictly for version numbers
- **Detailed Release Notes**: Include changelog, breaking changes, and migration guides
- **Pre-releases**: Use pre-release tags (`-beta`, `-rc`) for testing before stable releases
- **Version Consistency**: Don't manually edit version in `.csproj` - let the workflow handle it

## Related Files

- **Workflow**: `.github/workflows/release-cli-nuget.yml`
- **Project File**: `src/PackageCliTool/PackageCliTool.csproj`
- **Package Metadata**: Defined in `PackageCliTool.csproj`

## Support

For issues with the release process:
1. Check the workflow logs in GitHub Actions
2. Review this documentation
3. Open an issue in the repository with the workflow run URL
