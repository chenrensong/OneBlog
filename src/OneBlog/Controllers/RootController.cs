using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using OneBlog.Data;
using OneBlog.Data.Common;
using OneBlog.Helpers;
using OneBlog.Models;
using OneBlog.RssSyndication;
using OneBlog.Services;
using OneBlog.ViewModels;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OneBlog.Controllers
{
    [Route("")]
    public class RootController : Controller
    {
        private IPostService _postService;
        private IMailService _mailService;
        private IMemoryCache _memoryCache;
        private ILogger<RootController> _logger;
        private IViewRenderService _viewRenderService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<AppSettings> _appSettings;
        private INodeServices _nodeService;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private readonly IUnitOfWork _unitOfWork;
        public RootController(IUnitOfWork unitOfWork, IMailService mailService, UserManager<AppUser> userManager,
                              IHttpContextAccessor httpContextAccessor,
                              IMemoryCache memoryCache,
                              IViewRenderService viewRenderService,
                              IOptions<AppSettings> appSettings,
                              INodeServices nodeService,
                              IPostService postService,
                              ILogger<RootController> logger)
        {
            _postService = postService;
            _unitOfWork = unitOfWork;
            _nodeService = nodeService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _viewRenderService = viewRenderService;
            _mailService = mailService;
            _memoryCache = memoryCache;
            _appSettings = appSettings;
            _logger = logger;

        }

        [Route("archives")]
        public async Task<IActionResult> Archives()
        {
            var archives = await _postService.GetAll();
            return View(archives);
        }


        [ResponseCache(VaryByHeader = "Accept-Encoding", Location = ResponseCacheLocation.Any, Duration = 10)]
        [HttpGet("")]
        [HttpPost("")]
        public async Task<IActionResult> Index()
        {
            return await Pager(1);
        }

        [HttpGet("{page:int?}")]
        public async Task<IActionResult> Pager(int page)
        {
            if (page <= 0)
            {
                page = 1;
            }
            ViewBag.ControllerName = "Root";
            var result = await _postService.GetPagedList(_appSettings.Value.PostPerPage, _appSettings.Value.PostPerPage * (page - 1));
            return View("_List", result);
        }

        [HttpGet("captcha")]
        public ActionResult Captcha()
        {
            string code = ValidateHelper.CreateValidateCode(5);
            _session.SetString("ValidateCode", code);
            byte[] bytes = ValidateHelper.CreateValidateGraphic(code);
            return File(bytes, @"image/jpeg");
        }

        /// <summary>
        /// 统计接口
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("postcount/{id?}")]
        public async Task<IActionResult> PostCount(string id)
        {
            try
            {
                var count = await _postService.AddReadCount(id);
                return Json(new { Count = count });
            }
            catch
            {
                return Json(new { Count = 0 });
            }
        }



        /// <summary>
        /// 文章详情页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{slug}")]
        public async Task<IActionResult> Post(string slug)
        {
            try
            {
                PostDetail post = null;
                if (Guid.TryParse(slug, out Guid id))
                {
                    post = await _postService.GetById(id.ToString());
                }
                else
                {
                    post = await _postService.GetBySlug(slug);
                }
                if (post != null)
                {
                    if (post.ContentType == (int)DataConst.ContentType.Markdown)
                    {
                        post.Content = Markdig.Markdown.ToHtml(post.Content);
                    }
                }
                return View(post);
            }
            catch
            {
                _logger.LogWarning($"Couldn't find the ${slug} post");
            }
            return Redirect("/");
        }


        /// <summary>
        /// 关于页面
        /// </summary>
        /// <returns></returns>
        [HttpGet("about")]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet("Error/{code:int}")]
        public IActionResult Error(int errorCode)
        {
            if (Response.StatusCode == (int)HttpStatusCode.NotFound ||
                errorCode == (int)HttpStatusCode.NotFound ||
                Request.Path.Value.EndsWith("404"))
            {
                return View("NotFound");
            }
            return View();
        }

        [HttpGet("Exception")]
        public IActionResult Exception()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var request = HttpContext.Features.Get<IHttpRequestFeature>();

            if (exception != null && request != null)
            {
                var message = $@"RequestUrl: ${request.Path} Exception: ${exception.Error}";

                ViewBag.Error = message;
            }
            return View();
        }

        [HttpGet("feed")]
        public async Task<IActionResult> Feed()
        {
            var feed = new RssFeed()
            {
                Title = _appSettings.Value.Title,
                Description = _appSettings.Value.Description,
                Link = new Uri($"https://{_appSettings.Value.Host}/feed"),
                Copyright = $"© {DateTime.UtcNow.Year} {_appSettings.Value.Host}"
            };

            var entries = await _postService.GetPagedList(_appSettings.Value.PostPerPage);

            foreach (var entry in entries.Items)
            {
                var item = new RssItem()
                {
                    Title = entry.Title,
                    Body = string.Concat(entry.Content),
                    Link = new Uri($"{Request.GetEncodedUrl()}post/{entry.Id}"),
                    Permalink = entry.Slug,
                    PublishDate = entry.DatePublished,
                    Author = new RssAuthor() { Name = entry.Author.Name, Email = entry.Author.Email }
                };

                foreach (var cat in entry.Tags)
                {
                    item.Categories.Add(cat.TagName);
                }
                feed.Items.Add(item);
            }

            return File(Encoding.UTF8.GetBytes(feed.Serialize()), "text/xml");

        }

        [HttpGet("calendar")]
        public IActionResult Calendar()
        {
            return View();
        }


        /// <summary>
        /// 评论
        /// </summary>
        /// <param name="model"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost("comment")]
        public async Task<IActionResult> Comment(CommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    Error = "提交的信息有误,请检查后再试"
                });
            }
            var validateCode = _session.GetString("ValidateCode");
            if (string.IsNullOrEmpty(validateCode))
            {
                return Json(new
                {
                    Error = "验证码过期，请刷新重试！",
                });
            }
            _session.Remove("ValidateCode");
            if (!string.Equals(validateCode, model.Captcha, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new
                {
                    Error = "提交的验证码错误！",
                });
            }
            var replyToCommentId = Request.Form["hiddenReplyTo"].ToString();
            var commentRepository = _unitOfWork.GetRepository<Comment>();
            var postRepository = _unitOfWork.GetRepository<Post>();
            var post = await postRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == model.PostId);
            var commentItem = new CommentItem() { PostId = model.PostId, DisplayName = model.UserName, Email = model.Email, Content = model.Content };
            if (!string.IsNullOrEmpty(replyToCommentId))
            {
                commentItem.ParentId = replyToCommentId;
            }
            var newItem = new Comment()
            {
                Content = commentItem.Content,
                CreateDate = DateTime.UtcNow,
                DisplayName = commentItem.DisplayName,
                Ip = AspNetCoreHelper.GetRequestIP(),
                Posts = post,
                Email = commentItem.Email,
                IsApproved = true,
                ParentId = commentItem.ParentId,
            };
            await commentRepository.InsertAsync(newItem);
            await _unitOfWork.SaveChangesAsync();
            commentItem.Id = newItem.Id;
            commentItem.HasChildren = false;
            commentItem.IsApproved = true;
            commentItem.DateCreated = newItem.CreateDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            var result = await _viewRenderService.RenderToStringAsync(this, "_Comment", commentItem);
            return Json(new
            {
                Error = "",
                CommentId = commentItem.Id,
                CommentCount = (post.Comments.Count + 1),
                Result = result,
                Content = model.Content
            });
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }
}
