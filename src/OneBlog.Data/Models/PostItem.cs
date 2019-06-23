using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    /// <summary>
    /// Post item
    /// </summary>
    public class PostItem
    {
        /// <summary>
        /// If checked in the UI
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Post ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Post title
        /// </summary>
        public string Title { get; set; }

        public string Cover1 { get; set; }
        public string Cover2 { get; set; }
        public string Cover3 { get; set; }
        public string Content { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// Post author
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        ///     Gets or sets the date portion of published date
        /// </summary>
        public string DateCreated { get; set; }

        public DateTime DatePublished { get; set; }
        /// <summary>
        /// Slub
        /// </summary>
        public string Slug { get; set; }
        /// <summary>
        /// Relative link
        /// </summary>
        public string RelativeLink { get; set; }
        /// <summary>
        /// List of post categories
        /// </summary>
        public IList<CategoryItem> Categories { get; set; }
        /// <summary>
        /// Comma separated list of post tags
        /// </summary>
        public IList<TagItem> Tags { get; set; }

        /// <summary>
        /// Comment counts for the post
        /// </summary>
        public long CommentsCount { get; set; }

        public long ReadCount { get; set; }

        /// <summary>
        /// Gets or sets post status
        /// </summary>
        public bool IsPublished { get; set; }

        //加入CommentItem影响性能
        //public IList<CommentItem> Comments { get; set; }
    }
}
