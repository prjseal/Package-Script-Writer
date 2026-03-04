using PSW.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PackageCliTool.Extensions
{
    /// <summary>
    /// Provides extension methods for converting <see cref="Models.Api.ScriptModel"/> to <see cref="PackagesViewModel"/>.
    /// </summary>
    public static class ScriptModelExtensions
    {
        /// <summary>
        /// Converts a <see cref="Models.Api.ScriptModel"/> instance to a <see cref="PackagesViewModel"/>.
        /// </summary>
        /// <param name="scriptModel">The script model to convert.</param>
        /// <returns>A <see cref="PackagesViewModel"/> populated with values from the script model.</returns>
        public static PackagesViewModel ToViewModel(this Models.Api.ScriptModel scriptModel)
        {
            return new PackagesViewModel
            {
                TemplateName = scriptModel.TemplateName,
                TemplateVersion = scriptModel.TemplateVersion,
                CreateSolutionFile = scriptModel.CreateSolutionFile,
                SolutionName = scriptModel.SolutionName,
                ProjectName = scriptModel.ProjectName,
                UseUnattendedInstall = scriptModel.UseUnattendedInstall,
                DatabaseType = scriptModel.DatabaseType,
                ConnectionString = scriptModel.ConnectionString,
                UserFriendlyName = scriptModel.UserFriendlyName,
                UserEmail = scriptModel.UserEmail,
                UserPassword = scriptModel.UserPassword,
                Packages = scriptModel.Packages,
                IncludeStarterKit = scriptModel.IncludeStarterKit,
                StarterKitPackage = scriptModel.StarterKitPackage,
                IncludeDockerfile = scriptModel.IncludeDockerfile,
                IncludeDockerCompose = scriptModel.IncludeDockerCompose,
                EnableContentDeliveryApi = scriptModel.EnableContentDeliveryApi,
                SkipDotnetRun = scriptModel.SkipDotnetRun
            };
        }
    }
}
