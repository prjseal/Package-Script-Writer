using static PSW.Shared.Models.PackageFeed;

namespace PSW.Shared.Models;

public class PackageViewModel
{
    public PagedPackagesPackage Package { get; set; } = new();
    public List<string> PickedPackageIds { get; set; } = new();
    public int Downloads { get; set; }
    public bool IsChecked { get; set; }
    public int PackageId { get; set; }

    public PackageViewModel(PagedPackagesPackage package, List<string> pickedPackageIds, int packageId)
    {
        Package = package;
        PickedPackageIds = pickedPackageIds;
        Downloads = package?.Downloads ?? 0;
        IsChecked = pickedPackageIds != null && pickedPackageIds.Contains(package.PackageId);
        PackageId = packageId;
    }
}