using PackageCliTool.Exceptions;
using System.Text.RegularExpressions;

namespace PackageCliTool.Validation;

/// <summary>
/// Validates user inputs
/// </summary>
public static class InputValidator
{
    /// <summary>
    /// Validates a project name
    /// </summary>
    public static void ValidateProjectName(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new ValidationException(
                "Project name cannot be empty",
                "ProjectName",
                "Provide a valid project name (e.g., 'MyUmbracoProject')"
            );
        }

        // Check for invalid characters in file/folder names
        var invalidChars = Path.GetInvalidFileNameChars();
        if (projectName.Any(c => invalidChars.Contains(c)))
        {
            throw new ValidationException(
                $"Project name contains invalid characters: {projectName}",
                "ProjectName",
                "Use only letters, numbers, underscores, and hyphens in project names"
            );
        }

        // Check length
        if (projectName.Length > 100)
        {
            throw new ValidationException(
                "Project name is too long (maximum 100 characters)",
                "ProjectName",
                "Use a shorter project name"
            );
        }
    }

    /// <summary>
    /// Validates a solution name
    /// </summary>
    public static void ValidateSolutionName(string? solutionName)
    {
        if (string.IsNullOrWhiteSpace(solutionName))
        {
            return; // Optional field
        }

        ValidateProjectName(solutionName); // Same rules apply
    }

    /// <summary>
    /// Validates a directory path
    /// </summary>
    public static void ValidateDirectoryPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(
                "Directory path cannot be empty",
                "DirectoryPath",
                "Provide a valid directory path"
            );
        }

        try
        {
            // Try to get full path - this validates the path format
            var fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            throw new ValidationException(
                $"Invalid directory path: {ex.Message}",
                "DirectoryPath",
                "Provide a valid directory path (e.g., '/home/user/projects' or 'C:\\Projects')"
            );
        }
    }

    /// <summary>
    /// Validates an email address
    /// </summary>
    public static void ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return; // Optional in some contexts
        }

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        if (!emailRegex.IsMatch(email))
        {
            throw new ValidationException(
                $"Invalid email address: {email}",
                "Email",
                "Provide a valid email address (e.g., 'admin@example.com')"
            );
        }
    }

    /// <summary>
    /// Validates a password
    /// </summary>
    public static void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return; // Optional in some contexts
        }

        if (password.Length < 10)
        {
            throw new ValidationException(
                "Password is too short (minimum 10 characters required)",
                "Password",
                "Use a password with at least 10 characters"
            );
        }
    }

    /// <summary>
    /// Validates a package name
    /// </summary>
    public static void ValidatePackageName(string packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ValidationException(
                "Package name cannot be empty",
                "PackageName",
                "Provide a valid package name (e.g., 'uSync')"
            );
        }

        // Package names should follow NuGet conventions
        var packageRegex = new Regex(@"^[A-Za-z0-9_\.\-]+$");
        if (!packageRegex.IsMatch(packageName))
        {
            throw new ValidationException(
                $"Invalid package name: {packageName}",
                "PackageName",
                "Package names should contain only letters, numbers, dots, hyphens, and underscores"
            );
        }
    }

    /// <summary>
    /// Checks if a string is a valid NuGet package ID
    /// </summary>
    /// <param name="packageId">The package ID to validate</param>
    /// <returns>True if the package ID is valid, false otherwise</returns>
    public static bool IsValidNuGetPackageId(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId))
        {
            return false;
        }

        // NuGet package IDs should follow these rules:
        // - Only contain letters, numbers, dots, hyphens, and underscores
        // - Must not start or end with a dot
        // - Must not contain consecutive dots
        var packageRegex = new Regex(@"^[A-Za-z0-9_\-]+([\.][A-Za-z0-9_\-]+)*$");
        return packageRegex.IsMatch(packageId);
    }

    /// <summary>
    /// Validates a version string
    /// </summary>
    public static void ValidateVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return; // Empty means latest
        }

        if (version == "--prerelease" || version == "LTS")
        {
            return; // Special keywords
        }

        // Basic semantic version check (major.minor.patch with optional pre-release)
        var versionRegex = new Regex(@"^\d+\.\d+\.\d+(-[A-Za-z0-9\.\-]+)?$");
        if (!versionRegex.IsMatch(version))
        {
            throw new ValidationException(
                $"Invalid version format: {version}",
                "Version",
                "Use semantic versioning format (e.g., '14.0.0', '13.3.2-beta')"
            );
        }
    }

    /// <summary>
    /// Validates database type
    /// </summary>
    public static void ValidateDatabaseType(string? databaseType)
    {
        if (string.IsNullOrWhiteSpace(databaseType))
        {
            return; // Optional
        }

        var validTypes = new[] { "SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE" };
        if (!validTypes.Contains(databaseType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                $"Invalid database type: {databaseType}",
                "DatabaseType",
                $"Use one of: {string.Join(", ", validTypes)}"
            );
        }
    }

    /// <summary>
    /// Validates a connection string (basic check)
    /// </summary>
    public static void ValidateConnectionString(string? connectionString, string? databaseType)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Connection string is required for SQL Server and SQL Azure
            if (databaseType == "SQLServer" || databaseType == "SQLAzure")
            {
                throw new ValidationException(
                    "Connection string is required for SQL Server and SQL Azure",
                    "ConnectionString",
                    "Provide a valid connection string for your database"
                );
            }
            return;
        }

        // Basic validation - should contain at least one semicolon or equals sign
        if (!connectionString.Contains(';') && !connectionString.Contains('='))
        {
            throw new ValidationException(
                "Invalid connection string format",
                "ConnectionString",
                "Provide a valid connection string (e.g., 'Server=localhost;Database=MyDb;...')"
            );
        }
    }
}
