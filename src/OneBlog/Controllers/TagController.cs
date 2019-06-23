using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using OneBlog.Data.Contracts;
using System.Threading.Tasks;

namespace OneBlog.Controllers
{
    [Route("[controller]")]
    public class TagController : Controller
    {
        private readonly IPostService _postService;
        private readonly IOptions<AppSettings> _appSettings;

        public TagController(IPostService postService, IOptions<AppSettings> appSettings)
        {
            _postService = postService;
            _appSettings = appSettings;
        }

        [HttpGet("{tag}")]
        public async Task<IActionResult> Index(string tag)
        {
            return await Pager(tag, 1);
        }

        [HttpGet("{tag}/{page}")]
        public async Task<IActionResult> Pager(string tag, int page)
        {
            ViewBag.ControllerName = "Tag";
            var result = await _postService.GetPagedList(_appSettings.Value.PostPerPage,
             _appSettings.Value.PostPerPage * (page - 1), null, null, tag);
            return View("Index", result);
        }
    }
}