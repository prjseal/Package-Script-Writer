using System.Text;

using PSW.Models;

namespace PSW.Services;
public interface IScriptGeneratorService
{
    public string GenerateScript(PackagesViewModel model);
    public List<string> GenerateUmbracoTemplatesSectionScript(PackagesViewModel model);
    public List<string> GenerateCreateSolutionFileScript(PackagesViewModel model);
    public List<string> GenerateCreateProjectScript(PackagesViewModel model);
    public List<string> GenerateAddProjectToSolutionScript(PackagesViewModel model);
    public List<string> GenerateAddStarterKitScript(PackagesViewModel model, bool renderPackageName);
    public List<string> GenerateAddPackagesScript(PackagesViewModel model, bool renderPackageName);
    public List<string> GenerateRunProjectScript(PackagesViewModel model, bool renderPackageName);
}