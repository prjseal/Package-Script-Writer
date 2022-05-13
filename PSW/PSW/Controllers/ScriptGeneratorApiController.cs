using PSW.Models;
using PSW.Services;
using System.Web.Http;

namespace PSW.Controllers
{
    public class ScriptGeneratorApiController : ApiController
    {
        private readonly IScriptGeneratorService _scriptGeneratorService;

        public ScriptGeneratorApiController(IScriptGeneratorService scriptGeneratorService)
        {
            _scriptGeneratorService = scriptGeneratorService;
        }

        [HttpPost]
        public IHttpActionResult GenerateScript([FromBody] PackagesViewModel model)
        {
            return Ok(_scriptGeneratorService.GeneratePackageScript(model));
        }
    }
}
