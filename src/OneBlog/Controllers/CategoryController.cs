using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using System.Threading.Tasks;

namespace OneBlog.Controllers
{

    [Route("category")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;
        private readonly IOptions<AppSettings> _appSettings;
        public CategoryController(IPostService postService, ICategoryService categoryService, IOptions<AppSettings> appSettings)
        {
            _categoryService = categoryService;
            _postService = postService;
            _appSettings = appSettings;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            return await Pager(id, 1);
        }

        [HttpGet("{id}/{page}")]
        public async Task<IActionResult> Pager(string id, int page)
        {
            var category = await _categoryService.Get(id);
            if (category == null)
            {
                return View("Index", null);
            }
            var result = await _postService.GetPagedList(_appSettings.Value.PostPerPage,
             _appSettings.Value.PostPerPage * (page - 1), null, id);
            ViewBag.ControllerName = "category";
            ViewBag.Id = id.ToString();
            ViewBag.Title = $"{category.Title}";
            ViewBag.Category = category.Title;
            return View("Index", result);
        }
    }
}