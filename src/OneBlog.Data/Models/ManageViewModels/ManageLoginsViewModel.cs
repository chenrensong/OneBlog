using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace OneBlog.Models.ManageViewModels
{
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        //public IList<AuthenticationDescription> OtherLogins { get; set; }
    }
}
