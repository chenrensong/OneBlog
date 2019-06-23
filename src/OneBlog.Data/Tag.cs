using System;
using OneBlog.Helpers;
using System.Collections.Generic;
using SS.Toolkit.Helpers;
using System.ComponentModel.DataAnnotations;

namespace OneBlog.Data
{
    public class Tag
    {
        public Tag()
        {
            Id = GuidHelper.Gen().ToString();
        }

        [Key]
        [StringLength(100)]
        public string Id { get; set; }

        public string TagName { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public virtual IList<TagsInPosts> TagsInPosts { get; set; }
    }
}
