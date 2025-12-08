using PSW.Shared.Configuration;

namespace PSW.Shared.Models;

public class TabNavigationViewModel
{
    public bool HasQueryString { get; set; }
    public int NumberOfPackages { get; set; }
    public TabNavigationViewModel(bool hasQueryString, int numberOfPackages)
    {
        HasQueryString = hasQueryString;
        NumberOfPackages = numberOfPackages;
    }
}