using Microsoft.AspNetCore.Mvc;
using OneBlog.Core.Services;
using OneBlog.Models.ManageViewModels;
using System.Threading.Tasks;

namespace OneBlog.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;
        public DashboardController(IPostService postService, ICategoryService categoryService)
        {
            _postService = postService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<DashboardViewModel> Get()
        {
            var vm = new DashboardViewModel();
            var latestPost = await _postService.Get(10);
            return vm;
        }
    }
}
