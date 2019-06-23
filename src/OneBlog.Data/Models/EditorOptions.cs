using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    /// <summary>
    /// Editor options
    /// </summary>
    public class EditorOptions
    {
        /// <summary>
        /// Option type
        /// </summary>
        public string OptionType { get; set; }
        /// <summary>
        /// Show slug
        /// </summary>
        public bool ShowSlug { get; set; }
        /// <summary>
        /// Show description
        /// </summary>
        public bool ShowDescription { get; set; }
        /// <summary>
        /// Show custom fields
        /// </summary>
        public bool ShowCustomFields { get; set; }

        public bool ShowAuthors { get; set; }
    }
}
