using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OneBlog.Models
{
    /// <summary>
    /// Blog post detail
    /// </summary>
    [DataContract]
    public class PostDetail
    {

        public int ContentType { get; set; }
        /// <summary>
        /// If checked in the UI
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }
        /// <summary>
        /// Post ID
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// Post title
        /// </summary>
        [DataMember]
        public string Title { get; set; }
        /// <summary>
        /// Post author(DisplayName)
        /// </summary>
        [DataMember]
        public Author Author { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        public string Description { get; set; }
        /// <summary>
        /// Post content
        /// </summary>
        [DataMember]
        public string Content { get; set; }
        /// <summary>
        ///     Gets or sets the date portion of published date
        /// </summary>
        [DataMember]
        public string DateCreated { get; set; }
        /// <summary>
        /// Slub
        /// </summary>
        [DataMember]
        public string Slug { get; set; }
        /// <summary>
        /// Relative link
        /// </summary>
        [DataMember]
        public string RelativeLink { get; set; }
        /// <summary>
        /// Comma separated list of post categories
        /// </summary>
        [DataMember]
        public IList<CategoryItem> Categories { get; set; }
        /// <summary>
        /// Comma separated list of post tags
        /// </summary>
        [DataMember]
        public IList<TagItem> Tags { get; set; }
        /// <summary>
        /// Comment counts for the post
        /// </summary>
        [DataMember]
        public IList<string> Comments { get; set; }
        /// <summary>
        /// Post comments enabled
        /// </summary>
        [DataMember]
        public bool HasCommentsEnabled { get; set; }

        [DataMember]
        public bool HasRecommendEnabled { get; set; }
        /// <summary>
        /// Gets or sets post status
        /// </summary>
        [DataMember]
        public bool IsPublished { get; set; }
        /// <summary>
        /// If post marked for deletion
        /// </summary>
        [DataMember]
        public bool IsDeleted { get; set; }
        /// <summary>
        /// If the current user can delete this page.
        /// </summary>
        [DataMember]
        public bool CanUserDelete { get; set; }
        /// <summary>
        /// If the current user can edit this page.
        /// </summary>
        [DataMember]
        public bool CanUserEdit { get; set; }

        [DataMember]
        public string Cover1 { get; set; }
        [DataMember]
        public string Cover2 { get; set; }
        [DataMember]
        public string Cover3 { get; set; }

    }



}
