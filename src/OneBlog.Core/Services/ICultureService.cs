using System.Collections.Generic;
using OneBlog.Models;

namespace OneBlog.Core.Services
{
    public interface ICultureService
    {
        List<SelectOption> Get();
    }
}