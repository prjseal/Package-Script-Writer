using PSW.Models;
using System.Text;

namespace PSW.Services
{
    public class ScriptGeneratorService : IScriptGeneratorService
    {
        public string GeneratePackageScript(PackagesViewModel model)
        {
            StringBuilder sb = new StringBuilder();

            var renderPackageName = !string.IsNullOrWhiteSpace(model.ProjectName);

            if (model.InstallUmbracoTemplate)
            {
                sb.AppendLine("# Ensure we have the latest Umbraco templates");
                if (!string.IsNullOrWhiteSpace(model.UmbracoTemplateVersion))
                {
                    sb.AppendLine($"dotnet new -i Umbraco.Templates::{model.UmbracoTemplateVersion}");
                }
                else
                {
                    sb.AppendLine("dotnet new -i Umbraco.Templates");
                }
                sb.AppendLine();

                if (model.CreateSolutionFile)
                {
                    sb.AppendLine("# Create solution/project");
                    if (!string.IsNullOrWhiteSpace(model.SolutionName))
                    {
                        sb.AppendLine($"dotnet new sln --name \"{model.SolutionName}\"");
                    }
                }
                //Data Source=|DataDirectory|/PaulsShinySQLIteDatabase.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True
                //                sb.AppendLine($"dotnet new umbraco -n {model.ProjectName} --friendly-name \"{model.UserFriendlyName}\" --email \"{model.UserEmail}\" --password \"{model.UserPassword}\" --connection-string \"Data Source = (localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Umbraco.mdf;Integrated Security=True\"");

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

                    sb.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\" --friendly-name \"{model.UserFriendlyName}\" --email \"{model.UserEmail}\" --password \"{model.UserPassword}\" --connection-string {connectionString}");

                    if (model.DatabaseType == "SQLite")
                    {
                        sb.AppendLine("$env:Umbraco__CMS__Global__InstallMissingDatabase=\"true\"");
                        sb.AppendLine("$env:ConnectionStrings__umbracoDbDSN_ProviderName=\"Microsoft.Data.SQLite\"");
                    }
                }
                else
                {
                    sb.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\"");
                }

                if (model.CreateSolutionFile && !string.IsNullOrWhiteSpace(model.SolutionName))
                {
                    sb.AppendLine($"dotnet sln add \"{model.ProjectName}\"");
                }
                sb.AppendLine();
            }

            if (model.IncludeStarterKit)
            {
                sb.AppendLine("#Add starter kit");
                if (renderPackageName)
                {
                    sb.AppendLine($"dotnet add \"{model.ProjectName}\" package {model.StarterKitPackage}");
                }
                else
                {
                    sb.AppendLine($"dotnet add package {model.StarterKitPackage}");
                }
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(model.Packages))
            {
                var packages = model.Packages.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

                if (packages != null && packages.Length > 0)
                {
                    sb.AppendLine("#Add Packages");

                    foreach (var package in packages)
                    {
                        if (renderPackageName)
                        {
                            sb.AppendLine($"dotnet add \"{model.ProjectName}\" package {package}");
                        }
                        else
                        {
                            sb.AppendLine($"dotnet add package {package}");
                        }
                    }
                }
                sb.AppendLine();
            }

            if (renderPackageName)
            {
                sb.AppendLine($"dotnet run --project \"{model.ProjectName}\"");
            }
            else
            {
                sb.AppendLine($"dotnet run");
            }

            sb.AppendLine("#Running");
            return sb.ToString();
        }
    }
}
