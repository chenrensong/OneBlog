using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneBlog.Core.Services;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TagsController : Controller
    {
        private readonly ITagService _tagService;
        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<IList<TagItem>> Get(int take = 10, int skip = 0)
        {
            var result = await _tagService.Get(take, skip);
            return result;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _tagService.Get(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TagItem item)
        {
            try
            {
                var result = await _tagService.Add(item);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string name)
        {
            var result = await _tagService.Delete(name);
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
        public async Task<IActionResult> ProcessChecked([FromBody]List<TagItem> items, string id)
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
                        await _tagService.Delete(item.TagName);
                    }
                }
            }
            return Ok();
        }


    }
}
