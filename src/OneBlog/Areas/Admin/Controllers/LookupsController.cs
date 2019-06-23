using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OneBlog.Core.Services;
using OneBlog.Data;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LookupsController
    {
        private readonly ICultureService _cultureService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICategoryService _categoryService;

        public LookupsController(UserManager<AppUser> userManager,
            ICultureService cultureService,
            ICategoryService categoryService)
        {
            _userManager = userManager;
            _cultureService = cultureService;
            _categoryService = categoryService;
        }
        //public IActionResult Lookups([FromServices]ILookupsRepository repository)
        [HttpGet]
        public async Task<Lookups> Get()
        {
            var result = new Lookups();
            var cats = new List<SelectOption>();
            var category = await _categoryService.GetAll();
            foreach (var cat in category)
            {
                cats.Add(new SelectOption { OptionName = cat.Title, OptionValue = cat.Id.ToString() });
            }
            result.CategoryList = cats;
            result.Cultures = _cultureService.Get();
            result.SelfRegisterRoles = new List<SelectOption>();
            var task = _userManager.GetUsersInRoleAsync("Administrator");
            var administrators = task.Result;
            Author author = new Author();
            bool isSelected = false;
            var items = new List<SelectOption>();
            foreach (var item in administrators)
            {
                author.Id = item.Id;
                author.DisplayName = item.DisplayName;
                author.Signature = item.Signature;
                author.Name = item.UserName;
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(author);
                items.Add(new SelectOption { OptionName = item.DisplayName, OptionValue = author.Id, IsSelected = isSelected });
            }
            result.AuthorList = items;
            var pages = new List<SelectOption>();
            result.PageList = pages;
            result.InstalledThemes = new List<SelectOption>();
            result.PostOptions = new EditorOptions
            {
                OptionType = "Post",
                ShowSlug = true,
                ShowDescription = true,
                ShowCustomFields = true,
                ShowAuthors = true
            };
            result.PageOptions = new EditorOptions
            {
                OptionType = "Page",
                ShowSlug = true,
                ShowDescription = true,
                ShowCustomFields = true
            };
            return result;
        }
    }
}
