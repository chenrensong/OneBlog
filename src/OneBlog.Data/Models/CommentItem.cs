using System.Collections.Generic;

namespace OneBlog.Models
{
    public class CommentItem
    {
        /// <summary>
        /// If checked in the UI
        /// </summary>
        public bool IsChecked { get; set; }
        /// <summary>
        ///     Gets or sets the Comment Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 父Id
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        ///文章 ID
        /// </summary>
        public string PostId { get; set; }
        /// <summary>
        ///     If comment is pending
        /// </summary>
        public bool IsPending { get; set; }
        /// <summary>
        ///     If comment is approved
        /// </summary>
        public bool IsApproved { get; set; }
        /// <summary>
        ///     Whether comment is spam
        /// </summary>
        public bool IsSpam { get; set; }
        /// <summary>
        ///     Gets or sets the Author
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        ///  Email 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        ///     Whether this comment has nested comments
        /// </summary>
        public bool HasChildren { get; set; }
        /// <summary>
        ///     Gets or sets the date published
        /// </summary>
        public string DateCreated { get; set; }
        /// <summary>
        /// Content 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 用户Ip数据
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// The comments.
        /// </summary>
        private List<CommentItem> comments;

        public List<CommentItem> Comments
        {
            get
            {
                return comments ?? (comments = new List<CommentItem>());
            }
            set
            {
                comments = value;
            }
        }
    }
}
