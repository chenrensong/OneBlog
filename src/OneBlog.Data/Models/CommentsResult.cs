using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Models
{
    /// <summary>
    /// Comments view model
    /// </summary>
    public class CommentsResult
    {
        public List<CommentItem> Items { get; set; }
        public CommentItem SelectedItem { get; set; }
        public CommentItem Detail { get; set; }
    }
}
