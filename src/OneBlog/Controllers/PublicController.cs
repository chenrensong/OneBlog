using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using OneBlog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OneBlog.Data.Common.DataConst;

namespace OneBlog.Controllers
{
    [Route("public")]
    public class PublicController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IPostService _postService;
        public PublicController(IPostService postService, IOptions<AppSettings> appSettings)
        {
            _postService = postService;
            _appSettings = appSettings;
        }

        [HttpPost("PushPost")]
        public async Task<IActionResult> PushPost(string title, string content, string description, string tags, string cover)
        {
            string authorId = "3f2ae09c-c32b-465b-ae29-a793bd4c0eb5";
            var postDetail = new PostDetail();
            postDetail.Author = new Author()
            {
                Id = authorId
            };
            postDetail.ContentType = (int)ContentType.Html;
            postDetail.Description = description;
            postDetail.Title = title;
            postDetail.Content = content;
            postDetail.Categories = new List<CategoryItem>()
            {
                new CategoryItem()
                {
                    Id = "b8fa1674-50ba-4f9b-a468-a96f0097796c"//好物
                }
            };
            if (cover != null)
            {
                postDetail.Cover1 = cover;
            }
            if (tags != null)
            {
                postDetail.Tags = tags.Split(",").Select(m => new TagItem() { TagName = m }).ToList();
            }
            postDetail.IsPublished = true;
            await _postService.Add(postDetail);
            return Ok();
        }
    }
}
