using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneBlog.Data;
using OneBlog.Models;
using OneBlog.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.ViewComponents
{
    public class CommentViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            var comments = await commentRepository.GetListAsync(
                predicate: m => m.Posts.Id == id,
                include: x => x.Include(m => m.Posts),
                orderBy: x => x.OrderBy(m => m.CreateDate),
                selector: c => new CommentItem
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    PostId = c.Posts.Id,
                    Content = c.Content,
                    IsApproved = c.IsApproved,
                    DisplayName = c.DisplayName,
                    Email = c.Email,
                    DateCreated = c.CreateDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                    HasChildren = (commentRepository.Count(m => m.ParentId == c.Id, null) > 0),
                    Ip = c.Ip
                });

            // instantiate object
            var nestedComments = new List<CommentItem>();
            // temporary ID/Comment table
            var commentTable = new Dictionary<string, CommentItem>();
            foreach (var comment in comments)
            {
                // add to hashtable for lookup
                commentTable.Add(comment.Id, comment);

                // check if this is a child comment
                if (string.IsNullOrEmpty(comment.ParentId))
                {
                    // root comment, so add it to the list
                    nestedComments.Add(comment);
                }
                else
                {
                    // child comment, so find parent
                    var parentComment = commentTable[comment.ParentId] as CommentItem;
                    if (parentComment != null)
                    {
                        // double check that this sub comment has not already been added
                        if (parentComment.Comments.IndexOf(comment) == -1)
                        {
                            parentComment.Comments.Add(comment);
                        }
                        //parentComment.Comments = parentComment.Comments.OrderByDescending(m => m.DateCreated).ToList();
                    }
                    else
                    {
                        // just add to the base to prevent an error
                        nestedComments.Add(comment);
                    }
                }
            }
            var model = new CommentViewModel();
            model.PostId = id;
            model.Comments = nestedComments.OrderBy(m => m.DateCreated).ToList();
            return View(model);
        }
    }
}
