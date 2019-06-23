using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using OneBlog.Data.Contracts;
using System.Threading.Tasks;

namespace OneBlog.Controllers
{

    [Route("author")]
    public class AuthorController : Controller
    {
        private IPostService _postService;
        private IUsersRepository _usersRepository;
        private readonly IOptions<AppSettings> _appSettings;

        public AuthorController(IPostService postService, IUsersRepository usersRepository, 
            IOptions<AppSettings> appSettings)
        {
            _usersRepository = usersRepository;
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
            var userItem = _usersRepository.FindById(id.ToString());
            if (userItem != null && userItem.Profile != null)
            {
                ViewBag.UserProfile = userItem.Profile;
            }
            var result = await _postService.GetPagedList(_appSettings.Value.PostPerPage,
                _appSettings.Value.PostPerPage * (page - 1), null, null, null, id);
            ViewBag.ControllerName = "Author";
            ViewBag.Id = id.ToString();
            ViewBag.Title = $"{userItem.Profile.DisplayName}";
            return View("_List", result);
        }
    }
}