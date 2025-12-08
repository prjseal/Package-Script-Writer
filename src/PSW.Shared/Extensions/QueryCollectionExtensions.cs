using Microsoft.AspNetCore.Http;

namespace PSW.Shared.Extensions;

public static class QueryCollectionExtensions
{
    public static string? GetStringValue(this IQueryCollection query, string keyName, string fallbackValue)
    {
        var returnValue = fallbackValue;

        var rawValue = query[keyName];
        if (!string.IsNullOrWhiteSpace(rawValue))
        {
            returnValue = rawValue;
        }

        return returnValue;
    }

    public static int GetIntValue(this IQueryCollection query, string keyName, int fallbackValue)
    {
        var rawValue = query[keyName];

        return !string.IsNullOrWhiteSpace(rawValue) && int.TryParse(rawValue, out var parsedValue) ? parsedValue : fallbackValue;
    }

}