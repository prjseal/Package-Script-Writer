using PSW.Models;
using System.Text;

namespace PSW.Services;
public class ScriptGeneratorService : IScriptGeneratorService
{
    public string GenerateScript(PackagesViewModel model)
    {
        var output = new StringBuilder();

        var renderPackageName = !string.IsNullOrWhiteSpace(model.ProjectName);

        if (model.InstallUmbracoTemplate)
        {
            output.Append(GenerateUmbracoTemplatesSectionScript(model));

            output.Append(GenerateCreateSoltionFileScript(model));

            output.Append(GenerateCreateProjectScript(model));

            output.Append(GenerateAddProjectToSolutionScript(model));

            output.AppendLine();
        }

        output.Append(GenerateAddStarterKitScript(model, renderPackageName));

        output.Append(GenerateAddPackagesScript(model, renderPackageName));

        output.Append(GenerateRunProjectScript(model, renderPackageName));

        return output.ToString();
    }

    public string GenerateUmbracoTemplatesSectionScript(PackagesViewModel model)
    {
        var output = new StringBuilder();

        output.AppendLine("# Ensure we have the latest Umbraco templates");
        if (!string.IsNullOrWhiteSpace(model.UmbracoTemplateVersion))
        {
            output.AppendLine($"dotnet new -i Umbraco.Templates::{model.UmbracoTemplateVersion}");
        }
        else
        {
            output.AppendLine("dotnet new -i Umbraco.Templates");
        }
        output.AppendLine();

        return output.ToString();
    }

    public string GenerateCreateSoltionFileScript(PackagesViewModel model)
    {
        var output = new StringBuilder();

        if (model.CreateSolutionFile)
        {
            output.AppendLine("# Create solution/project");
            if (!string.IsNullOrWhiteSpace(model.SolutionName))
            {
                output.AppendLine($"dotnet new sln --name \"{model.SolutionName}\"");
            }
        }

        return output.ToString();
    }

    public string GenerateCreateProjectScript(PackagesViewModel model)
    {
        var output = new StringBuilder();

        var majorVersionNumberAsString = model.UmbracoTemplateVersion.Split('.').FirstOrDefault();
        var majorVersionNumber = 10;

        if (!string.IsNullOrWhiteSpace(majorVersionNumberAsString))
        {
            _ = int.TryParse(majorVersionNumberAsString, out majorVersionNumber);
        }

        var isOldv10RCVersion = model.UmbracoTemplateVersion == "10.0.0-rc1" || model.UmbracoTemplateVersion == "10.0.0-rc2" || model.UmbracoTemplateVersion == "10.0.0-rc3";
        var isV10OrAbove = majorVersionNumber >= 10;

        if (model.UseUnattendedInstall)
        {
            var connectionString = "";
            var databasTypeSwitch = "";
            switch (model.DatabaseType)
            {
                case "SQLCE":
                    if (majorVersionNumber == 9)
                    {
                        connectionString = "--connection-string \"Data Source=|DataDirectory|\\Umbraco.sdf;Flush Interval=1\" -ce";
                    }
                    break;
                case "LocalDb":
                    if (!isV10OrAbove || isOldv10RCVersion)
                    {
                        connectionString = " --connection-string \"Data Source = (localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Umbraco.mdf;Integrated Security=True\"";
                    }
                    else if (isV10OrAbove && !isOldv10RCVersion)
                    {
                        databasTypeSwitch = " --development-database-type LocalDB";
                    }
                    break;
                case "SQLite":
                    if (!isV10OrAbove || isOldv10RCVersion)
                    {
                        connectionString = " --connection-string \"Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True\"";
                    }
                    else if (isV10OrAbove && !isOldv10RCVersion)
                    {
                        databasTypeSwitch = " --development-database-type SQLite";
                    }
                    break;
                case "SQLAzure":
                case "SQLServer":
                    connectionString = $" --connection-string \"{model.ConnectionString}\" --connection-string-provider-name \"Microsoft.Data.SqlClient\"";
                    break;
                default:
                    break;
            }

            output.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\" --friendly-name \"{model.UserFriendlyName}\" --email \"{model.UserEmail}\" --password \"{model.UserPassword}\"{connectionString}{databasTypeSwitch}");

            if (model.DatabaseType == "SQLite" && isOldv10RCVersion)
            {
                output.AppendLine("$env:Umbraco__CMS__Global__InstallMissingDatabase=\"true\"");
                output.AppendLine("$env:ConnectionStrings__umbracoDbDSN_ProviderName=\"Microsoft.Data.SQLite\"");
            }
        }
        else
        {
            output.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\"");
        }



        return output.ToString();
    }

    public string GenerateAddProjectToSolutionScript(PackagesViewModel model)
    {
        var output = new StringBuilder();

        if (model.CreateSolutionFile && !string.IsNullOrWhiteSpace(model.SolutionName))
        {
            output.AppendLine($"dotnet sln add \"{model.ProjectName}\"");
        }

        return output.ToString();
    }

    public string GenerateAddStarterKitScript(PackagesViewModel model, bool renderPackageName)
    {
        var output = new StringBuilder();

        if (model.IncludeStarterKit)
        {
            output.AppendLine("#Add starter kit");
            if (renderPackageName)
            {
                output.AppendLine($"dotnet add \"{model.ProjectName}\" package {model.StarterKitPackage}");
            }
            else
            {
                output.AppendLine($"dotnet add package {model.StarterKitPackage}");
            }
            output.AppendLine();
        }

        return output.ToString();
    }

    public string GenerateAddPackagesScript(PackagesViewModel model, bool renderPackageName)
    {
        var output = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(model.Packages))
        {
            var packages = model.Packages.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            if (packages != null && packages.Length > 0)
            {
                output.AppendLine("#Add Packages");

                foreach (var package in packages)
                {
                    var packageIdAndVersion = package.TrimEnd('|').Replace("|--prerelease", " --prerelease ").Replace("|", " --version ");
                    if (renderPackageName)
                    {
                        output.AppendLine($"dotnet add \"{model.ProjectName}\" package {packageIdAndVersion}");
                    }
                    else
                    {
                        output.AppendLine($"dotnet add package {packageIdAndVersion}");
                    }
                }
            }
            output.AppendLine();
        }

        return output.ToString();
    }

    public string GenerateRunProjectScript(PackagesViewModel model, bool renderPackageName)
    {
        var output = new StringBuilder();

        if (renderPackageName)
        {
            output.AppendLine($"dotnet run --project \"{model.ProjectName}\"");
        }
        else
        {
            output.AppendLine($"dotnet run");
        }

        output.AppendLine("#Running");

        return output.ToString();
    }
}