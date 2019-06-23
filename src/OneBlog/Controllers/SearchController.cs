using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using System.Threading.Tasks;

namespace OneBlog.Controllers
{
    [Route("[controller]")]
    public class SearchController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IPostService _postService;

        public SearchController(IPostService postService, IOptions<AppSettings> appSettings)
        {
            _postService = postService;
            _appSettings = appSettings;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect("/");//没有关键字，跳回首页。
        }

        [HttpGet("{term}/{page:int?}")]
        public async Task<IActionResult> Pager(string term, int page = 1)
        {
            if (string.IsNullOrEmpty(term))
            {
                return View("Index");
            }
            ViewBag.ControllerName = "search";
            ViewBag.Term = term;
            var result = await _postService.GetPagedList(_appSettings.Value.PostPerPage,
                _appSettings.Value.PostPerPage * (page - 1), term);
            return View("Index", result);
        }
    }
}
