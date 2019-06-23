using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneBlog.Data.Contracts;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class RolesController : Controller
    {
        readonly IRolesRepository repository;

        public RolesController(IRolesRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public IEnumerable<RoleItem> Get(int take = 10, int skip = 0, string filter = "", string order = "")
        {
            return repository.Find(take, skip, filter, order);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await repository.FindById(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RoleItem role)
        {
            var result = await repository.Add(role);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody]List<RoleItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return NotFound();
            }

            foreach (var item in items)
            {
                await repository.Remove(item.RoleName);
            }
            return Ok();
        }

        public async Task<IActionResult> Delete(string id)
        {
            await repository.Remove(id);
            return Ok();
        }

        [HttpPut]
        [Route("processchecked/{id?}")]
        public async Task<IActionResult> ProcessChecked([FromBody]List<RoleItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return NotFound();
            }

            var action = ControllerContext.RouteData.Values["id"].ToString().ToLowerInvariant();

            if (action.ToLower() == "delete")
            {
                foreach (var item in items)
                {
                    if (item.IsChecked)
                    {
                        await repository.Remove(item.RoleName);
                    }
                }
            }
            return Ok();
        }



        [HttpGet]
        [Route("getuserroles/{id?}")]
        public IActionResult GetUserRoles(string id)
        {
            var result = repository.GetUserRoles(id);
            return Ok(result);
        }


        [HttpGet]
        [Route("getrights/{id?}")]
        public async Task<IActionResult> GetRights(string id)
        {
            var result = await repository.GetRoleRights(id);
            return Ok(result);
        }

        [HttpPut]
        [Route("saverights/{id?}")]
        public async Task<IActionResult> SaveRights([FromBody]List<Group> rights, string id)
        {
            await repository.SaveRights(rights, id);
            return Ok();
        }
    }
}
