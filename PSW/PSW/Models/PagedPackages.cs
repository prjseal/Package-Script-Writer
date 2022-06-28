using System.ComponentModel.DataAnnotations;

namespace PSW.Models;
public class PackageFeed
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/OurUmbraco.Repository.Models")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.datacontract.org/2004/07/OurUmbraco.Repository.Models", IsNullable = false)]
    public partial class PagedPackages
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private PagedPackagesPackage[] packagesField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private int pagesField;

        private int totalField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Package", IsNullable = false)]
        public PagedPackagesPackage[] Packages
        {
            get
            {
                return this.packagesField;
            }
            set
            {
                this.packagesField = value;
            }
        }

        /// <remarks/>
        public int Pages
        {
            get
            {
                return this.pagesField;
            }
            set
            {
                this.pagesField = value;
            }
        }

        /// <remarks/>
        public int Total
        {
            get
            {
                return this.totalField;
            }
            set
            {
                this.totalField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/OurUmbraco.Repository.Models")]
    public partial class PagedPackagesPackage
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string categoryField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private bool certifiedToWorkOnUmbracoCloudField;

        private System.DateTime createdField;

        private int downloadsField;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string excerptField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string iconField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string idField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string imageField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private bool isNuGetFormatField;

        private bool isPromotedField;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string latestVersionField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private byte likesField;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string nameField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string nuGetPackageIdField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private PagedPackagesPackageOwnerInfo ownerInfoField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private int scoreField;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string summaryField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string urlField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string versionRangeField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <remarks/>
        public string Category
        {
            get
            {
                return this.categoryField;
            }
            set
            {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        public bool CertifiedToWorkOnUmbracoCloud
        {
            get
            {
                return this.certifiedToWorkOnUmbracoCloudField;
            }
            set
            {
                this.certifiedToWorkOnUmbracoCloudField = value;
            }
        }

        /// <remarks/>
        public System.DateTime Created
        {
            get
            {
                return this.createdField;
            }
            set
            {
                this.createdField = value;
            }
        }

        /// <remarks/>
        public int Downloads
        {
            get
            {
                return this.downloadsField;
            }
            set
            {
                this.downloadsField = value;
            }
        }

        /// <remarks/>
        public string Excerpt
        {
            get
            {
                return this.excerptField;
            }
            set
            {
                this.excerptField = value;
            }
        }

        /// <remarks/>
        public string Icon
        {
            get
            {
                return this.iconField;
            }
            set
            {
                this.iconField = value;
            }
        }

        /// <remarks/>
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string Image
        {
            get
            {
                return this.imageField;
            }
            set
            {
                this.imageField = value;
            }
        }

        /// <remarks/>
        public bool IsNuGetFormat
        {
            get
            {
                return this.isNuGetFormatField;
            }
            set
            {
                this.isNuGetFormatField = value;
            }
        }

        /// <remarks/>
        public bool IsPromoted
        {
            get
            {
                return this.isPromotedField;
            }
            set
            {
                this.isPromotedField = value;
            }
        }

        /// <remarks/>
        public string LatestVersion
        {
            get
            {
                return this.latestVersionField;
            }
            set
            {
                this.latestVersionField = value;
            }
        }

        /// <remarks/>
        public byte Likes
        {
            get
            {
                return this.likesField;
            }
            set
            {
                this.likesField = value;
            }
        }

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string NuGetPackageId
        {
            get
            {
                return this.nuGetPackageIdField;
            }
            set
            {
                this.nuGetPackageIdField = value;
            }
        }

        /// <remarks/>
        public PagedPackagesPackageOwnerInfo OwnerInfo
        {
            get
            {
                return this.ownerInfoField;
            }
            set
            {
                this.ownerInfoField = value;
            }
        }

        /// <remarks/>
        public int Score
        {
            get
            {
                return this.scoreField;
            }
            set
            {
                this.scoreField = value;
            }
        }

        /// <remarks/>
        public string Summary
        {
            get
            {
                return this.summaryField;
            }
            set
            {
                this.summaryField = value;
            }
        }

        /// <remarks/>
        public string Url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        public string VersionRange
        {
            get
            {
                return this.versionRangeField;
            }
            set
            {
                this.versionRangeField = value;
            }
        }

        public List<string> PackageVersions { get; set; }

        [Display(Name="Version")]
        public string SelectedVersion { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/OurUmbraco.Repository.Models")]
    public partial class PagedPackagesPackageOwnerInfo
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string[] contributorsField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private int karmaField;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string ownerField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string ownerAvatarField;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
        [System.Xml.Serialization.XmlArrayItemAttribute(Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays", IsNullable = false)]
        public string[] Contributors
        {
            get
            {
                return this.contributorsField;
            }
            set
            {
                this.contributorsField = value;
            }
        }

        /// <remarks/>
        public int Karma
        {
            get
            {
                return this.karmaField;
            }
            set
            {
                this.karmaField = value;
            }
        }

        /// <remarks/>
        public string Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }

        /// <remarks/>
        public string OwnerAvatar
        {
            get
            {
                return this.ownerAvatarField;
            }
            set
            {
                this.ownerAvatarField = value;
            }
        }
    }
}