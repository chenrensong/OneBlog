using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OneBlog.Models.ManageViewModels
{
    public class DisplayRecoveryCodesViewModel
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }

    }
}
