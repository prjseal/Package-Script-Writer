namespace PSW.Extensions
{
    public static class QueryCollectionExtensions
    {
        public static string GetStringValue(this IQueryCollection query, string keyName, string fallbackValue)
        {
            var returnValue = fallbackValue;

            var rawValue = query[keyName];
            if (!string.IsNullOrWhiteSpace(rawValue))
            {
                returnValue = rawValue;
            }

            return returnValue;
        }
    }
}
