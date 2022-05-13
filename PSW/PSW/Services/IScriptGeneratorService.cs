using PSW.Models;

namespace PSW.Services
{
    public interface IScriptGeneratorService
    {
        public string GeneratePackageScript(PackagesViewModel model);
    }
}
