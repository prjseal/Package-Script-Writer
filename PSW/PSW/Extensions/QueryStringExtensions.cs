namespace PSW.Extensions
{
    public static class QueryStringExtensions
    {
        public static QueryString AddValueIfNotEmpty(this QueryString queryString, string key, string val)
        {
            if (!string.IsNullOrWhiteSpace(val))
            {
                queryString = queryString.Add(key, val);
            }
            return queryString;
        }
    }
}
