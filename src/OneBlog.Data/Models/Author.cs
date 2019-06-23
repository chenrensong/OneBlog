using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    [DataContract]
    public class Author
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Signature { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Avatar { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
    }
}
