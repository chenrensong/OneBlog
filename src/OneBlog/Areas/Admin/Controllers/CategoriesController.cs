using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneBlog.Data;
using OneBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using OneBlog.Core.Services;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IList<CategoryItem>> Get(int take = 10, int skip = 0)
        {
            var result = await _categoryService.Get(take, skip);
            return result;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _categoryService.Get(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CategoryItem item)
        {
            try
            {
                var result = await _categoryService.Add(item);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody]CategoryItem item)
        {
            var result = await _categoryService.Update(item);
            if (result)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _categoryService.Delete(id);
            if (result)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("processchecked/{id?}")]
        public async Task<IActionResult> ProcessChecked([FromBody]List<CategoryItem> items, string id)
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest();
            }
            var action = id.ToLowerInvariant();
            if (action == "delete")
            {
                foreach (var item in items)
                {
                    if (item.IsChecked)
                    {
                        await _categoryService.Delete(item.Id);
                    }
                }
            }
            return Ok();
        }
    }
}
