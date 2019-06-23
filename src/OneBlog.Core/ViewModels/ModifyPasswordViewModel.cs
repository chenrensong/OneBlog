using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.ViewModels
{
    public class ModifyPasswordViewModel
    {


        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "昵称")]
        public string DisplayName { get; set; }

        [Display(Name = "签名")]
        public string Signature { get; set; }

        [Display(Name = "头像")]
        public string Avatar { get; set; }

        [Required]
        [Display(Name = "原始密码")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [Display(Name = "密码")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
