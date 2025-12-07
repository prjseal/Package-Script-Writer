using System;
using System.Collections.Generic;
using System.Text;

namespace PackageCliTool.Models
{
    /// <summary>
    /// Represents a package category with an identifier and a name.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string name { get; set; }
    }
}
