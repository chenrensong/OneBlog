using SS.Toolkit.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneBlog.Data
{
    /// <summary>
    /// 评论
    /// </summary>
    public class Comment
    {
        public Comment()
        {
            Id = GuidHelper.Gen().ToString();
        }

        [Key]
        [StringLength(100)]
        public string Id { get; set; }

        /// <summary>
        /// 父Id
        /// </summary>
        [StringLength(100)]
        public string ParentId { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Comment is approved.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// 是否是作弊
        /// </summary>
        public bool IsSpam { get; set; }

        public string Email { get; set; }

        public string SiteUrl { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// 评论时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 作者Id (支持匿名）
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// 关联文章
        /// </summary>
        public virtual Post Posts { get; set; }


    }
}
