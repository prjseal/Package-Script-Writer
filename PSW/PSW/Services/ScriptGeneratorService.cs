using PSW.Models;

namespace PSW.Services;
public class ScriptGeneratorService : IScriptGeneratorService
{
    public string GenerateScript(PackagesViewModel model)
    {
        var outputList = new List<string>();

        var renderPackageName = !string.IsNullOrWhiteSpace(model.ProjectName);

        if (model.InstallUmbracoTemplate)
        {
            outputList.AddRange(GenerateUmbracoTemplatesSectionScript(model));

            outputList.AddRange(GenerateCreateSolutionFileScript(model));

            outputList.AddRange(GenerateCreateProjectScript(model));

            outputList.AddRange(GenerateAddProjectToSolutionScript(model));

            outputList.Add("");
        }

        outputList.AddRange(GenerateAddStarterKitScript(model, renderPackageName));

        outputList.AddRange(GenerateAddPackagesScript(model, renderPackageName));

        outputList.AddRange(GenerateRunProjectScript(model, renderPackageName));

        if (!model.OnelinerOutput)
        {
            return string.Join(Environment.NewLine, outputList);
        }
        
        outputList.RemoveAll(string.IsNullOrWhiteSpace);
        return string.Join(" && ", outputList);
    }

    public List<string> GenerateUmbracoTemplatesSectionScript(PackagesViewModel model)
    {
        
        var outputList = new List<string>();
        var forceTemplateInstallCommand = model.ForceTemplateInstall ? " --force" : "";
        var installCommand = model.UmbracoTemplateVersion?.StartsWith("9.") == true || model.UmbracoTemplateVersion?.StartsWith("10.") == true ? "-i" : "install";

        if (!model.OnelinerOutput)
        {
            outputList.Add("# Ensure we have the latest Umbraco templates");
        }
        
        if (!string.IsNullOrWhiteSpace(model.UmbracoTemplateVersion))
        {
            outputList.Add($"dotnet new {installCommand} Umbraco.Templates::{model.UmbracoTemplateVersion}{forceTemplateInstallCommand}");
        }
        else
        {
            outputList.Add($"dotnet new {installCommand} Umbraco.Templates{forceTemplateInstallCommand}");
        }


        outputList.Add("");

        return outputList;
    }

    public List<string> GenerateCreateSolutionFileScript(PackagesViewModel model)
    {
        var outputList = new List<string>();
        if (!model.CreateSolutionFile) return outputList;

        if (!model.OnelinerOutput)
        {
            outputList.Add("# Create solution/project");
        }

        if (!string.IsNullOrWhiteSpace(model.SolutionName))
        {
            outputList.Add($"dotnet new sln --name \"{model.SolutionName}\"");
        }

        return outputList;
    }

    public List<string> GenerateCreateProjectScript(PackagesViewModel model)
    {
        var outputList = new List<string>();

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

            outputList.Add($"dotnet new umbraco --force -n \"{model.ProjectName}\" --friendly-name \"{model.UserFriendlyName}\" --email \"{model.UserEmail}\" --password \"{model.UserPassword}\"{connectionString}{databasTypeSwitch}");

            if (model.DatabaseType == "SQLite" && isOldv10RCVersion)
            {
                outputList.Add("$env:Umbraco__CMS__Global__InstallMissingDatabase=\"true\"");
                outputList.Add("$env:ConnectionStrings__umbracoDbDSN_ProviderName=\"Microsoft.Data.SQLite\"");
            }
        }
        else
        {
            outputList.Add($"dotnet new umbraco --force -n \"{model.ProjectName}\"");
        }



        return outputList;
    }

    public List<string> GenerateAddProjectToSolutionScript(PackagesViewModel model)
    {
        var outputList = new List<string>();

        if (model.CreateSolutionFile && !string.IsNullOrWhiteSpace(model.SolutionName))
        {
            outputList.Add($"dotnet sln add \"{model.ProjectName}\"");
        }

        return outputList;
    }

    public List<string> GenerateAddStarterKitScript(PackagesViewModel model, bool renderPackageName)
    {
        var outputList = new List<string>();

        if (!model.IncludeStarterKit) return outputList;
        
        if (!model.OnelinerOutput)
        {
            outputList.Add("#Add starter kit");
        }

        if (renderPackageName)
        {
            outputList.Add($"dotnet add \"{model.ProjectName}\" package {model.StarterKitPackage}");
        }
        else
        {
            outputList.Add($"dotnet add package {model.StarterKitPackage}");
        }
        
        outputList.Add("");

        return outputList;
    }

    public List<string> GenerateAddPackagesScript(PackagesViewModel model, bool renderPackageName)
    {
        var outputList = new List<string>();

        if (string.IsNullOrWhiteSpace(model.Packages)) return outputList;
        
        var packages = model.Packages.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

        if (packages.Length > 0)
        {
            if (!model.OnelinerOutput)
            {
                outputList.Add("#Add Packages");
            }

            foreach (var package in packages)
            {
                var packageIdAndVersion = package.TrimEnd('|').Replace("|--prerelease", " --prerelease ").Replace("|", " --version ");
                if (renderPackageName)
                {
                    outputList.Add($"dotnet add \"{model.ProjectName}\" package {packageIdAndVersion}");
                }
                else
                {
                    outputList.Add($"dotnet add package {packageIdAndVersion}");
                }
            }
        }
        outputList.Add("");

        return outputList;
    }

    public List<string> GenerateRunProjectScript(PackagesViewModel model, bool renderPackageName)
    {
        var outputList = new List<string>();

        if (renderPackageName)
        {
            outputList.Add($"dotnet run --project \"{model.ProjectName}\"");
        }
        else
        {
            outputList.Add($"dotnet run");
        }

        if (!model.OnelinerOutput)
        {
            outputList.Add("#Running");
        }

        return outputList;
    }
}
