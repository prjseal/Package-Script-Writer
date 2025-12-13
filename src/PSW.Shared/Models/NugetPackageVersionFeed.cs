namespace PSW.Shared.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

public class NugetPackageVersionFeed
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/Atom", IsNullable = false)]
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    public partial class feed
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private feedTitle titleField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private feedSubtitle subtitleField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string idField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private System.DateTime updatedField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string logoField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private feedLink[] linkField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        public feedEntry[] entryField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public feedTitle title
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public feedSubtitle subtitle
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.subtitleField;
            }
            set
            {
                this.subtitleField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public string id
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
        public System.DateTime updated
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.updatedField;
            }
            set
            {
                this.updatedField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public string logo
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.logoField;
            }
            set
            {
                this.logoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("link")]
#pragma warning disable IDE1006 // Naming Styles
        public feedLink[] link
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.linkField;
            }
            set
            {
                this.linkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("entry")]
#pragma warning disable IDE1006 // Naming Styles
        public feedEntry[] entry
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.entryField;
            }
            set
            {
                this.entryField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedTitle
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string typeField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string valueField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string type
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedSubtitle
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string typeField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string valueField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string type
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedLink
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string relField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string typeField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string hrefField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string rel
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.relField;
            }
            set
            {
                this.relField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string type
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string href
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedEntry
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string idField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private feedEntryTitle titleField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private System.DateTime publishedField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private System.DateTime updatedField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private feedEntryAuthor authorField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private feedEntryLink linkField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private feedEntryContent contentField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public string id
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
        public feedEntryTitle title
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public System.DateTime published
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.publishedField;
            }
            set
            {
                this.publishedField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public System.DateTime updated
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.updatedField;
            }
            set
            {
                this.updatedField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public feedEntryAuthor author
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.authorField;
            }
            set
            {
                this.authorField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public feedEntryLink link
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.linkField;
            }
            set
            {
                this.linkField = value;
            }
        }

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public feedEntryContent content
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.contentField;
            }
            set
            {
                this.contentField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedEntryTitle
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string typeField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string valueField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string type
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedEntryAuthor
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string nameField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string uriField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
#pragma warning disable IDE1006 // Naming Styles
        public string name
#pragma warning restore IDE1006 // Naming Styles
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
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1056 // URI-like properties should not be strings
        public string uri
#pragma warning restore CA1056 // URI-like properties should not be strings
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.uriField;
            }
            set
            {
                this.uriField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedEntryLink
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string relField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string hrefField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string rel
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.relField;
            }
            set
            {
                this.relField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string href
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
#pragma warning disable IDE1006 // Naming Styles
    public partial class feedEntryContent
#pragma warning restore IDE1006 // Naming Styles
    {

#pragma warning disable IDE1006 // Naming Styles
        private string typeField;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private string valueField;
#pragma warning restore IDE1006 // Naming Styles

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
#pragma warning disable IDE1006 // Naming Styles
        public string type
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}

#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.