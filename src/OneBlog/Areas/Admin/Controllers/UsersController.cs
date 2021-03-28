using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OneBlog.Data;
using OneBlog.Data.Contracts;
using OneBlog.Models;
using OneBlog.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OneBlog.Areas.Admin.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IUsersRepository repository;
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager, IUsersRepository repository, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
            this.repository = repository;
        }

        [HttpGet]
        public IEnumerable<UserItem> Get(int take = 10, int skip = 0, string filter = "1 == 1", string order = "UserName")
        {
            return repository.Find(take, skip, filter, order);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var result = repository.FindById(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok();
        }
        [HttpPost]
        public IActionResult Post([FromBody] UserItem item)
        {
            var result = repository.Add(item);
            if (result == null)
            {
                return NotFound();
            }
            return Ok();
        }
        [HttpPut]
        public IActionResult Update([FromBody] UserItem item)
        {
            repository.Update(item);
            return Ok();
        }


        /// <summary>
        /// 移除头像
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPut]
        [HttpPost]
        [HttpGet]
        [Route("removeAvatar/{username?}")]
        public async Task<IActionResult> RemoveAvatar(string username)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                user.Avatar = $"/account/avatar/{SS.Toolkit.Helpers.SecurityHelper.MD5(username)}";
                await _userManager.UpdateAsync(user);
            }
            return Ok(user.Avatar);
        }

        [Route("profile/{username?}")]

        public IActionResult Profile(string username)
        {
            var user = repository.FindByName(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }


        [HttpPut]
        [Route("saveprofile")]
        public IActionResult SaveProfile([FromBody] UserItem item)
        {
            repository.SaveProfile(item);
            return Ok();
        }

        [HttpPut]
        [Route("processchecked/delete")]
        public IActionResult ProcessChecked([FromBody] IList<UserItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest();
            }
            foreach (var item in items)
            {
                if (item.IsChecked)
                {
                    repository.Remove(item.UserName);
                }
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            repository.Remove(id);
            return Ok();
        }
    }
}
