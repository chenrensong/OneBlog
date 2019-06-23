using OneBlog.Data.Common;
using OneBlog.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OneBlog.Data
{
    public class BaseRepository
    {
        protected AppDbContext _ctx;

        protected int CalculatePages(int totalCount, int pageSize)
        {
            return ((int)(totalCount / pageSize)) + ((totalCount % pageSize) > 0 ? 1 : 0);
        }


        
    }
}