using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OneBlog.Helpers;
using SS.Toolkit.Helpers;
using System.Linq;

namespace OneBlog.Data
{
    /// <summary>
    /// 文章列表
    /// </summary>
    public class Post
    {
        public Post()
        {
            Id = GuidHelper.Gen().ToString();
        }

        [Key]
        [StringLength(100)]
        public string Id { get; set; }
        /// <summary>
        /// 封面图片
        /// </summary>
        public string CoverImage { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 摘要
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 内容格式（0 markdown 1 html）
        /// </summary>
        public int ContentType { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime DatePublished { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPublished { get; set; }
        /// <summary>
        /// Slug
        /// </summary>
        public string Slug { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Tags 
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 坐标
        /// </summary>
        public string Coordinate { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 是否开启评论
        /// </summary>
        public bool HasCommentsEnabled { get; set; }
        /// <summary>
        /// 是否推荐
        /// </summary>
        public bool HasRecommendEnabled { get; set; }
        /// <summary>
        /// 阅读数
        /// </summary>
        public long ReadCount { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public virtual AppUser Author { get; set; }

        //public ICollection<Category> Categories { get; } = new List<Category>();

        [NotMapped]
        public IEnumerable<Category> Categories => PostsInCategories.Select(e => e.Categories);

        //[NotMapped]
        //public IEnumerable<Tag> Tags => TagsInPosts.Select(e => e.Tags);

        public virtual ICollection<PostsInCategories> PostsInCategories { get; set; }

        public virtual IList<Comment> Comments { get; set; }

        public virtual IList<TagsInPosts> TagsInPosts { get; set; }

    }
}