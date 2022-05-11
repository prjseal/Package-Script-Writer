using Microsoft.AspNetCore.Mvc;
using PSW.Extensions;
using PSW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSW.Controllers
{
    public class PackagesController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> SubmitForm(PackagesViewModel model)
        {
            var queryString = new QueryString();

            queryString = queryString.Add("InstallUmbracoTemplate", model.InstallUmbracoTemplate.ToString());
            queryString = queryString.AddValueIfNotEmpty("UmbracoTemplateVersion", model.UmbracoTemplateVersion);
            queryString = queryString.Add("IncludeStarterKit", model.IncludeStarterKit.ToString());
            queryString = queryString.AddValueIfNotEmpty("StarterKitPackage", model.StarterKitPackage);
            queryString = queryString.Add("UseUnattendedInstall", model.UseUnattendedInstall.ToString());
            queryString = queryString.AddValueIfNotEmpty("DatabaseType", model.DatabaseType);
            queryString = queryString.AddValueIfNotEmpty("UserEmail", model.UserEmail);
            queryString = queryString.AddValueIfNotEmpty("UserFriendlyName", model.UserFriendlyName);
            queryString = queryString.AddValueIfNotEmpty("UserPassword", model.UserPassword);
            queryString = queryString.AddValueIfNotEmpty("ProjectName", model.ProjectName);
            queryString = queryString.Add("CreateSolutionFile", model.CreateSolutionFile.ToString());
            queryString = queryString.AddValueIfNotEmpty("SolutionName", model.SolutionName);
            queryString = queryString.AddValueIfNotEmpty("Packages", model.Packages);

            return RedirectToRoute("/",queryString);

        }
    }
}
