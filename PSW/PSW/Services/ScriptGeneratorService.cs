using PSW.Models;
using System.Text;

namespace PSW.Services;
public class ScriptGeneratorService : IScriptGeneratorService
{
    public string GeneratePackageScript(PackagesViewModel model)
    {
        var output = new StringBuilder();

        var renderPackageName = !string.IsNullOrWhiteSpace(model.ProjectName);

        if (model.InstallUmbracoTemplate)
        {
            UmbracoTemplatesSection(model, output);

            CreateSoltionFile(model, output);

            CreateProject(model, output);

            AddProjectToSolution(model, output);

            output.AppendLine();
        }

        AddStarterKit(model, output, renderPackageName);

        AddPackages(model, output, renderPackageName);

        RunProject(model, output, renderPackageName);

        return output.ToString();
    }

    private static void RunProject(PackagesViewModel model, StringBuilder output, bool renderPackageName)
    {
        if (renderPackageName)
        {
            output.AppendLine($"dotnet run --project \"{model.ProjectName}\"");
        }
        else
        {
            output.AppendLine($"dotnet run");
        }

        output.AppendLine("#Running");
    }

    private static void AddPackages(PackagesViewModel model, StringBuilder output, bool renderPackageName)
    {
        if (!string.IsNullOrWhiteSpace(model.Packages))
        {
            var packages = model.Packages.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            if (packages != null && packages.Length > 0)
            {
                output.AppendLine("#Add Packages");

                foreach (var package in packages)
                {
                    if (renderPackageName)
                    {
                        output.AppendLine($"dotnet add \"{model.ProjectName}\" package {package}");
                    }
                    else
                    {
                        output.AppendLine($"dotnet add package {package}");
                    }
                }
            }
            output.AppendLine();
        }
    }

    private static void AddStarterKit(PackagesViewModel model, StringBuilder output, bool renderPackageName)
    {
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
    }

    private static void AddProjectToSolution(PackagesViewModel model, StringBuilder output)
    {
        if (model.CreateSolutionFile && !string.IsNullOrWhiteSpace(model.SolutionName))
        {
            output.AppendLine($"dotnet sln add \"{model.ProjectName}\"");
        }
    }

    private static void CreateProject(PackagesViewModel model, StringBuilder output)
    {
        if (model.UseUnattendedInstall)
        {
            var connectionString = "";
            switch (model.DatabaseType)
            {
                case "LocalDb":
                    connectionString = "\"Data Source = (localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Umbraco.mdf;Integrated Security=True\"";
                    break;
                case "SQLCE":
                    connectionString = "\"Data Source=|DataDirectory|\\Umbraco.sdf;Flush Interval=1\" -ce";
                    break;
                case "SQLite":
                    connectionString = "\"Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True\"";
                    break;
                default:
                    break;
            }

            output.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\" --friendly-name \"{model.UserFriendlyName}\" --email \"{model.UserEmail}\" --password \"{model.UserPassword}\" --connection-string {connectionString}");

            if (model.DatabaseType == "SQLite")
            {
                output.AppendLine("$env:Umbraco__CMS__Global__InstallMissingDatabase=\"true\"");
                output.AppendLine("$env:ConnectionStrings__umbracoDbDSN_ProviderName=\"Microsoft.Data.SQLite\"");
            }
        }
        else
        {
            output.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\"");
        }
    }

    private static void CreateSoltionFile(PackagesViewModel model, StringBuilder output)
    {
        if (model.CreateSolutionFile)
        {
            output.AppendLine("# Create solution/project");
            if (!string.IsNullOrWhiteSpace(model.SolutionName))
            {
                output.AppendLine($"dotnet new sln --name \"{model.SolutionName}\"");
            }
        }
    }

    private static void UmbracoTemplatesSection(PackagesViewModel model, StringBuilder output)
    {
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
    }
}