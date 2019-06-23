using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Services
{
    public interface IViewRenderService
    {

        Task<string> RenderToStringAsync(Controller controller, string viewName, object model);

        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
