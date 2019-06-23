using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IPostService _postService;
        public PostsController(IPostService postService, IOptions<AppSettings> appSettings)
        {
            _postService = postService;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IPagedList<PostItem>> Get(int take = 10, int skip = 0)
        {
            var result = await _postService.GetPagedList(take, skip);
            return result;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _postService.GetById(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]PostDetail item)
        {
            try
            {
                item.ContentType = (int)_appSettings.Value.EditorType;
                var newItem = await _postService.Add(item);
                item.Id = newItem.Id;
                return Ok(item);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody]PostDetail item)
        {
            try
            {
                await _postService.Update(item);
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
            var result = await _postService.Delete(id);
            if (result)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("processchecked/{id?}")]
        public async Task<IActionResult> ProcessChecked([FromBody]List<PostItem> items, string id)
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
                        await _postService.Delete(item.Id);
                    }
                }
            }
            return Ok();
        }

        //[HttpPut]
        //public async Task<IActionResult> DeleteAll([FromBody]CategoryItem item)
        //{
        //    var postRepository = _unitOfWork.GetRepository<Category>();
        //    var action = ControllerContext.RouteData.Values["id"].ToString().ToLowerInvariant();
        //    if (action.ToLower() == "pending" || action.ToLower() == "spam")
        //    {
        //        var all = postRepository.GetAll();
        //        postRepository.Delete(all);
        //    }
        //    await _unitOfWork.SaveChangesAsync();
        //    return Ok();
        //}
    }
}
