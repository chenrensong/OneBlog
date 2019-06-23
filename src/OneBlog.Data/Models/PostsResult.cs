using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OneBlog.Models
{

    [DataContract]
    public class PostsResult : BasePager
    {
        [DataMember]
        public IEnumerable<PostItem> Posts { get; set; }

        [DataMember]
        public IEnumerable<PostItem> RecommendPosts { get; set; }

        [DataMember]
        public string Category { get; set; }
    }
}