using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneBlog.ViewModels
{
    /// <summary>
    /// 评论页数据
    /// </summary>
    public class CommentViewModel
    {
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "评论")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "验证码")]
        public string Captcha { get; set; }

        [Required]
        public string PostId { get; set; }

        /// <summary>
        /// 不是提交字段
        /// </summary>
        public List<CommentItem> Comments
        {
            get; set;
        }
    }


}
