using PackageCliTool.Models.Templates;
using PackageCliTool.Exceptions;

namespace PackageCliTool.Validation;

/// <summary>
/// Validates template configurations
/// </summary>
public static class TemplateValidator
{
    /// <summary>
    /// Validates a complete template
    /// </summary>
    public static List<string> Validate(Template template)
    {
        var errors = new List<string>();

        // Validate metadata
        errors.AddRange(ValidateMetadata(template.Metadata));

        // Validate configuration
        errors.AddRange(ValidateConfiguration(template.Configuration));

        return errors;
    }

    /// <summary>
    /// Validates template metadata
    /// </summary>
    private static List<string> ValidateMetadata(TemplateMetadata metadata)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(metadata.Name))
        {
            errors.Add("Template name cannot be empty");
        }
        else if (metadata.Name.Length > 100)
        {
            errors.Add("Template name is too long (maximum 100 characters)");
        }
        else if (metadata.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            errors.Add("Template name contains invalid characters");
        }

        if (string.IsNullOrWhiteSpace(metadata.Version))
        {
            errors.Add("Template version cannot be empty");
        }

        return errors;
    }

    /// <summary>
    /// Validates template configuration
    /// </summary>
    private static List<string> ValidateConfiguration(TemplateConfiguration config)
    {
        var errors = new List<string>();

        // Validate project name
        try
        {
            InputValidator.ValidateProjectName(config.Project.Name);
        }
        catch (ValidationException ex)
        {
            errors.Add($"Project name: {ex.Message}");
        }

        // Validate solution name if provided
        if (!string.IsNullOrWhiteSpace(config.Project.SolutionName))
        {
            try
            {
                InputValidator.ValidateSolutionName(config.Project.SolutionName);
            }
            catch (ValidationException ex)
            {
                errors.Add($"Solution name: {ex.Message}");
            }
        }

        // Validate packages
        foreach (var package in config.Packages)
        {
            if (string.IsNullOrWhiteSpace(package.Name))
            {
                errors.Add("Package name cannot be empty");
            }
            else
            {
                try
                {
                    InputValidator.ValidatePackageName(package.Name);
                }
                catch (ValidationException ex)
                {
                    errors.Add($"Package '{package.Name}': {ex.Message}");
                }
            }

            // Validate version format if not special keyword
            if (!string.IsNullOrWhiteSpace(package.Version) &&
                package.Version != "latest" &&
                package.Version != "prerelease")
            {
                try
                {
                    InputValidator.ValidateVersion(package.Version);
                }
                catch (ValidationException ex)
                {
                    errors.Add($"Package '{package.Name}' version: {ex.Message}");
                }
            }
        }

        // Validate database configuration
        if (config.Unattended.Enabled)
        {
            try
            {
                InputValidator.ValidateDatabaseType(config.Unattended.Database.Type);
            }
            catch (ValidationException ex)
            {
                errors.Add($"Database type: {ex.Message}");
            }

            try
            {
                InputValidator.ValidateEmail(config.Unattended.Admin.Email);
            }
            catch (ValidationException ex)
            {
                errors.Add($"Admin email: {ex.Message}");
            }

            // Validate password if it's a literal (not prompt or env var)
            var password = config.Unattended.Admin.Password;
            if (!string.IsNullOrWhiteSpace(password) &&
                password != "<prompt>" &&
                !password.StartsWith("${"))
            {
                try
                {
                    InputValidator.ValidatePassword(password);
                }
                catch (ValidationException ex)
                {
                    errors.Add($"Admin password: {ex.Message}");
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates and throws if template is invalid
    /// </summary>
    public static void ValidateAndThrow(Template template)
    {
        var errors = Validate(template);

        if (errors.Count > 0)
        {
            throw new ValidationException(
                "Template validation failed",
                "Template",
                string.Join(Environment.NewLine, errors)
            );
        }
    }
}
