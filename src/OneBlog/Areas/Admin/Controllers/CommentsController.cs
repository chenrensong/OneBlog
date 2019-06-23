using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneBlog.Data;
using OneBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using OneBlog.Helpers;
using System.Globalization;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CommentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CommentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<CommentsResult> Get(int take = 10, int skip = 0)
        {
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            var items = new List<CommentItem>();
            var query = commentRepository.GetList(
                include: b => b.Include(a => a.Posts),
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
                })
                .Skip(skip);
            if (take > 0)
            {
                query = query.Take(take);
            }
            var list= await query.ToListAsync();
            var commentsResult = new CommentsResult();
            commentsResult.Items = list;
            commentsResult.SelectedItem = new CommentItem();
            commentsResult.Detail = new CommentItem();
            return commentsResult;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            var result = await commentRepository.GetFirstOrDefaultAsync(
                predicate: m => m.Id == id,
                include: x => x.Include(m => m.Posts),
                selector: c => new CommentItem()
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    PostId = c.Posts.Id,
                    Content = c.Content,
                    Ip = c.Ip
                });
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CommentItem item)
        {
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            var postRepository = _unitOfWork.GetRepository<Post>();
            try
            {
                var post = await postRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == item.PostId);
                var newItem = new Comment()
                {
                    Content = item.Content,
                    CreateDate = DateTime.UtcNow,
                    DisplayName = item.DisplayName,
                    Ip = AspNetCoreHelper.GetRequestIP(),
                    Posts = post,
                    Email = item.Email,
                    IsApproved = true,
                    ParentId = item.ParentId,
                };
                await commentRepository.InsertAsync(newItem);
                await _unitOfWork.SaveChangesAsync();
                item.Id = newItem.Id;
                item.HasChildren = false;
                item.IsApproved = true;
                item.DateCreated = newItem.CreateDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody]CommentItem item)
        {
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            try
            {
                var comment = await commentRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == item.Id);
                comment.Content = item.Content;
                comment.DisplayName = item.DisplayName;
                commentRepository.Update(comment);
                await _unitOfWork.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            var comment = await commentRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == id);
            if (comment != null)
            {
                var list = GetChildren(commentRepository, new List<Comment>() { comment });
                commentRepository.Delete(list);
                await _unitOfWork.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPut]
        [Route("processchecked/{id?}")]
        public async Task<IActionResult> ProcessChecked([FromBody]List<CategoryItem> items, string id)
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest();
            }
            var action = id.ToLowerInvariant();
            if (action == "delete")
            {
                foreach (var item in items)
                {
                    if (item.IsChecked)
                    {
                        await Delete(item.Id);
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> DeleteAll([FromBody]CommentItem item)
        {
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            var action = ControllerContext.RouteData.Values["id"].ToString().ToLowerInvariant();
            if (action.ToLower() == "pending" || action.ToLower() == "spam")
            {
                var all = commentRepository.GetAll();
                commentRepository.Delete(all);
            }
            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }


        private async Task<List<Comment>> GetChildren(IRepository<Comment> commentRepository, List<Comment> comments)
        {
            List<Comment> temp = new List<Comment>();
            foreach (var item in comments)
            {
                var newList = await commentRepository.GetListAsync(predicate: m => m.ParentId == item.Id);
                newList = await GetChildren(commentRepository, newList);
                temp.AddRange(newList);
            }
            comments.AddRange(temp);
            return comments;
        }
    }
}
