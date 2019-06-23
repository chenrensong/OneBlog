using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Data
{
    public class PostsInCategories
    {
        public PostsInCategories()
        {
        }

        public string PostsId { get; set; }

        public string CategoriesId { get; set; }

        public virtual Post Posts { get; set; }

        public virtual Category Categories { get; set; }
    }
}
