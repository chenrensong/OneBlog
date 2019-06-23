using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Data
{
    public class TagsInPosts
    {
        public TagsInPosts()
        {
        }

        public string TagId { get; set; }

        public string PostId { get; set; }

        public virtual Post Posts { get; set; }

        public virtual Tag Tags { get; set; }
    }
}
