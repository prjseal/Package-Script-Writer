using PSW.Models;

namespace PSW.Services;

public interface IQueryStringService
{
    public PackagesViewModel LoadModelFromQueryString(HttpRequest request);

    public QueryString GenerateQueryStringFromModel(PackagesViewModel model);
}