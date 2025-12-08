using Microsoft.AspNetCore.Http;
using PSW.Shared.Models;

namespace PSW.Shared.Services;

public interface IQueryStringService
{
    public PackagesViewModel LoadModelFromQueryString(HttpRequest request);

    public QueryString GenerateQueryStringFromModel(PackagesViewModel model);
}