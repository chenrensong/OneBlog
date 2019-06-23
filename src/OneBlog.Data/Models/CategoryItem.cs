using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    //https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api
    [DataContract]
    public class CategoryItem
    {
        /// <summary>
        /// If checked in the UI
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }
        /// <summary>
        /// Unique Id
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        [DataMember]
        public string Title { get; set; }
        /// <summary>
        /// Parent
        /// </summary>
        [DataMember]
        public SelectOption Parent { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public string Description { get; set; }
        /// <summary>
        /// Counter
        /// </summary>
        [DataMember]
        public int Count { get; set; }
    }
}
