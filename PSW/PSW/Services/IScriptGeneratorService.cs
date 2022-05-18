using PSW.Models;
using System.Text;

namespace PSW.Services;
public interface IScriptGeneratorService
{
    public string GenerateScript(PackagesViewModel model);
    public string GenerateUmbracoTemplatesSectionScript(PackagesViewModel model);
    public string GenerateCreateSoltionFileScript(PackagesViewModel model);
    public string GenerateCreateProjectScript(PackagesViewModel model);
    public string GenerateAddProjectToSolutionScript(PackagesViewModel model);
    public string GenerateAddStarterKitScript(PackagesViewModel model, bool renderPackageName);
    public string GenerateAddPackagesScript(PackagesViewModel model, bool renderPackageName);
    public string GenerateRunProjectScript(PackagesViewModel model, bool renderPackageName);
}
