namespace PSW.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) { return value; }

        if (value.Length > maxLength) { return value.Substring(0, maxLength) + "..."; }

        return value.Substring(0, value.Length);
    }
}