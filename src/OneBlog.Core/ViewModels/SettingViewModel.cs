using System.ComponentModel.DataAnnotations;

namespace OneBlog.ViewModels
{
    public class SettingViewModel
    {
        [Display(Name = "邮箱")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "昵称")]
        [StringLength(16, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        public string DisplayName { get; set; }

        [Display(Name = "签名")]
        public string Signature { get; set; }

        [Display(Name = "头像")]
        public string Avatar { get; set; }

        [Display(Name = "年龄")]
        public int Age { get; set; }

        [Display(Name = "性别")]
        public int Sex { get; set; }
    }
}
