using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    [DataContract]
    public abstract class BasePager
    {
        [DataMember]
        public int TotalResults { get; set; }
        [DataMember]
        public int TotalPages { get; set; }
        [DataMember]
        public int CurrentPage { get; set; }
    }
}
