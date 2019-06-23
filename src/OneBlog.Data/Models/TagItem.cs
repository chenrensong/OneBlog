using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    /// <summary>
    /// The tag item
    /// </summary>
    [DataContract]
    public class TagItem
    {
        /// <summary>
        /// If checked in the UI
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }
        /// <summary>
        /// Tag Name
        /// </summary>
        [DataMember]
        public string TagName { get; set; }
        /// <summary>
        /// Tag Count
        /// </summary>
        [DataMember]
        public int TagCount { get; set; }
    }

    public class TagItemCompare : IEqualityComparer<TagItem>
    {
        public bool Equals(TagItem x, TagItem y)
        {
            return string.Equals(x.TagName, y.TagName);
        }

        public int GetHashCode(TagItem obj)
        {
            return obj.GetHashCode();
        }
    }
}
