using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OneBlog.Configuration;
using OneBlog.Data.Contracts;
using OneBlog.Uploader;
using SS.Toolkit.Helpers;
using System;
using System.Threading.Tasks;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IHostingEnvironment _env;

        public AdminController(IHostingEnvironment env, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _env = env;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("editpost")]
        public IActionResult EditPost()
        {
            return View();
        }


        [Route("customfields")]
        public IActionResult CustomFields()
        {
            return Ok();
        }


        [HttpPost]
        [Route("coverimage")]
        public async Task<IActionResult> CoverImage()
        {
            var fileUploader = IocContainer.Get<IFileUploder>();
            string url = string.Empty;
            if (Request.Form.ContainsKey("croppedImage"))
            {
                var source = Request.Form["croppedImage"][0];
                string base64 = source.Substring(source.IndexOf(',') + 1);
                base64 = base64.Trim('\0');
                var buffer = Convert.FromBase64String(base64);
                var uploadResult = await fileUploader.InvokeAsync("", "ugc/" + GuidHelper.Gen().ToString().Replace("-", "") + ".jpg", buffer);
                url = uploadResult.Url;
            }
            else
            {
                if (Request.Form == null || Request.Form.Files == null
                    || Request.Form.Files.Count == 0)
                {
                    NotFound();
                }
                var file = Request.Form.Files[0];

                var filename = ContentDispositionHeaderValue
                .Parse(file.ContentDisposition)
                .FileName
                .Trim();
                var uploadFileBytes = new byte[file.Length];
                try
                {
                    await file.OpenReadStream().ReadAsync(uploadFileBytes, 0, (int)file.Length);
                }
                catch (Exception ex)
                {

                }
                var uploadResult = await fileUploader.InvokeAsync("", "/ugc/" + GuidHelper.Gen().ToString().Replace("-", ""), uploadFileBytes);
                url = uploadResult.Url;
            }
            return Ok(url);
        }



        //public IActionResult Resource()
        //{
        //    var lang = BlogSettings.Instance.Culture;
        //    var sb = new StringBuilder();
        //    var cacheKey = "admin.resource.axd - " + lang;
        //    var script = (string)Blog.CurrentInstance.Cache[cacheKey];

        //    if (String.IsNullOrEmpty(script))
        //    {
        //        System.Globalization.CultureInfo culture;
        //        try
        //        {
        //            culture = new System.Globalization.CultureInfo(lang);
        //        }
        //        catch (Exception)
        //        {
        //            culture = OneBlog.Core.WebUtils.GetDefaultCulture();
        //        }

        //        var jc = new BlogCulture(culture, BlogCulture.ResourceType.Admin);

        //        // add SiteVars used to pass server-side values to JavaScript in admin UI
        //        var sbSiteVars = new StringBuilder();

        //        sbSiteVars.Append("ApplicationRelativeWebRoot: '" + OneBlog.Core.WebUtils.ApplicationRelativeWebRoot + "',");
        //        sbSiteVars.Append("RelativeWebRoot: '" + OneBlog.Core.WebUtils.RelativeWebRoot + "',");
        //        sbSiteVars.Append("AbsoluteWebRoot:  '" + OneBlog.Core.WebUtils.AbsoluteWebRoot + "',");

        //        sbSiteVars.Append("IsPrimary: '" + Blog.CurrentInstance.IsPrimary + "',");
        //        sbSiteVars.Append("BlogInstanceId: '" + Blog.CurrentInstance.Id + "',");
        //        sbSiteVars.Append("BlogStorageLocation: '" + Blog.CurrentInstance.StorageLocation + "',");
        //        sbSiteVars.Append("BlogFilesFolder: '" + OneBlog.Core.WebUtils.FilesFolder + "',");

        //        sbSiteVars.Append("GenericPageSize:  '" + BlogConfig.GenericPageSize.ToString() + "',");
        //        sbSiteVars.Append("GalleryFeedUrl:  '" + BlogConfig.GalleryFeedUrl + "',");
        //        sbSiteVars.Append("Version: 'OneBlog.NET " + BlogSettings.Instance.Version() + "'");

        //        sb.Append("SiteVars = {" + sbSiteVars.ToString() + "}; BlogAdmin = { i18n: " + jc.ToJsonString() + "};");
        //        script = sb.ToString();

        //        Blog.CurrentInstance.Cache.Insert(cacheKey, script, null, Cache.NoAbsoluteExpiration, new TimeSpan(3, 0, 0, 0));

        //    }

        //    return JavaScript(script);
        //}



    }
}