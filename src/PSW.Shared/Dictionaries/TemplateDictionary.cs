namespace PSW.Shared.Dictionaries;

public class TemplateDictionary
{
    public Dictionary<string, string> ShortNames
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "Umbraco.Community.Templates.UmBootstrap", "umbootstrap" },
                { "UmbCheckout.StarterKit.Stripe", "umbcheckout.starterkit.stripe" }
            };
        }
    }

    public string GetShortName(string key)
    {
        if (ShortNames.ContainsKey(key))
        {
            return ShortNames[key];
        }
        else
        {
            return key.ToLower();
        }
    }
}