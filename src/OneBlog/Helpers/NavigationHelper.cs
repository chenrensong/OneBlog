using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OneBlog.Data;
using OneBlog.Data.Contracts;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Helpers
{
    public class NavigationHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _contextAccessor;

        public NavigationHelper(IUnitOfWork unitOfWork,
            IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _contextAccessor = contextAccessor;

        }

        public async Task<IList<TagItem>> GetTopTags()
        {
            var tagRepository = _unitOfWork.GetRepository<Tag>();
            var model = await tagRepository.GetAll().Select(m => new TagItem { TagName = m.TagName, TagCount = m.TagsInPosts.Count }).ToListAsync();
            return model;
        }

        public List<MenuItem> GetMenu(bool hasIndex = false)
        {
            //string url = ((string)linkMenuItemPart.Url).Replace("~/", Context.Request.PathBase);
            //var match = Context.Request.Path.Value.Equals("/" + url, StringComparison.OrdinalIgnoreCase);

            var menus = new List<MenuItem>();


            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var category = categoryRepository.GetList(
            include: b => b.Include(a => a.PostsInCategories)
            ).Select(c => new CategoryItem
            {
                Id = c.Id,
                Parent = new SelectOption() { OptionName = c.ParentName, OptionValue = c.ParentId },
                Title = c.Title,
                Description = c.Description,
                Count = c.PostsInCategories.Count
            });

            var urlHelper = AspNetCoreHelper.UrlHelper;


            var table = new Dictionary<string, MenuItem>();
            if (hasIndex)
            {
                var index = new MenuItem()
                {
                    Id = string.Empty,
                    Title = "首页",
                    Url = urlHelper.Action("index", "root"),
                    Route = new List<Models.RouteData>() {
                        new Models.RouteData() { Action = "index", Controller = "root" },
                        new Models.RouteData() { Action = "pager", Controller = "root" }
                }
                };
                table.Add(string.Empty, index);
                menus.Add(index);
            }

            var commonRoute = new List<Models.RouteData>() { new Models.RouteData() { Action = "index", Controller = "category" } };

            foreach (var item in category)
            {
                var url = urlHelper.Action("index", "category", new { id = item.Id });

                var menu = new MenuItem() { Id = item.Id, Title = item.Title, Url = url, Route = commonRoute };
                table.Add(item.Id, menu);

                // check if this is a child comment
                if (item.Parent == null || item.Parent.IsEmpty())
                {
                    menus.Add(menu);
                }
                else
                {
                    var guid = item.Parent.OptionValue;
                    var parentMenu = table[guid];
                    if (parentMenu != null)
                    {
                        // double check that this sub comment has not already been added
                        if (parentMenu.Menus.IndexOf(menu) == -1)
                        {
                            parentMenu.Menus.Add(menu);
                        }
                    }
                    else
                    {
                        menus.Add(menu);
                    }
                }
            }

            var routeData = _contextAccessor.HttpContext.GetRouteData().Values;
            var controller = (string)routeData["controller"];
            var action = (string)routeData["action"];
            string id = null;

            if (routeData.ContainsKey(nameof(id)))
            {
                id = (string)routeData[nameof(id)];
            }

            foreach (var item in menus)
            {
                var url = item.Url.Replace("~/", _contextAccessor.HttpContext.Request.PathBase);
                var match = _contextAccessor.HttpContext.Request.Path.Value.Equals("/" + url, StringComparison.OrdinalIgnoreCase)
                    || _contextAccessor.HttpContext.Request.Path.Value.Equals(url, StringComparison.OrdinalIgnoreCase);

                if (!match)
                {
                    match = item.Route.Any(m => m.Action.Equals(action, StringComparison.OrdinalIgnoreCase) && m.Controller.Equals(controller, StringComparison.OrdinalIgnoreCase));
                    if (match && id != null)
                    {
                        match = (id == item.Id.ToString());
                    }
                }

                if (!match && item.Menus.Count > 0)
                {
                    match = IsActive(item.Menus);
                }

                if (match)
                {
                    item.IsActive = true;
                    break;
                }

            }
            return menus;
        }


        private bool IsActive(List<MenuItem> menus)
        {
            foreach (var item in menus)
            {
                var url = item.Url.Replace("~/", _contextAccessor.HttpContext.Request.PathBase);
                var match = _contextAccessor.HttpContext.Request.Path.Value.Equals("/" + url, StringComparison.OrdinalIgnoreCase)
                    || _contextAccessor.HttpContext.Request.Path.Value.Equals(url, StringComparison.OrdinalIgnoreCase);
                if (match)
                {
                    return true;
                }
            }
            return false;
        }


    }
}
