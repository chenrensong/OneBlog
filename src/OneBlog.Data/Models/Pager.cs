using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    public class Pager<T>
    {

        public Pager(IEnumerable<T> items)
        {
            this.Items = items;
        }

        public IEnumerable<T> Items { get; set; }

        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int PagesLength { get; set; }
    }
}
