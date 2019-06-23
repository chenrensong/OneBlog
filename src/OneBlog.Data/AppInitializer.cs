using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace OneBlog.Data
{
    /// <summary>
    /// 数据初始化
    /// </summary>
    public class AppInitializer
    {
        private AppDbContext _ctx;
        private UserManager<AppUser> _userMgr;
        private RoleManager<IdentityRole> _roleMgr;

        private static string DefaultName = "admin@chenrensong.com";
        private static string DefaultPassword = "admin@chenrensong";


        public AppInitializer(AppDbContext ctx, UserManager<AppUser> userMgr, RoleManager<IdentityRole> roleMgr)
        {
            _ctx = ctx;
            _userMgr = userMgr;
            _roleMgr = roleMgr;
        }

        public async Task SeedAsync()
        {
            _ctx.Database.Migrate();
            // Seed User
            if (await _userMgr.FindByNameAsync(DefaultName) == null)
            {
                // 添加系统角色
                if (!await _roleMgr.RoleExistsAsync("Administrator"))
                {
                    await _roleMgr.CreateAsync(new IdentityRole("Administrator"));
                }
                if (!await _roleMgr.RoleExistsAsync("Anonymous"))
                {
                    await _roleMgr.CreateAsync(new IdentityRole("Anonymous"));
                }
                if (!await _roleMgr.RoleExistsAsync("Editor"))
                {
                    await _roleMgr.CreateAsync(new IdentityRole("Editor"));
                }
                // 创建账号

                var defaultUser = new AppUser()
                {
                    Email = DefaultName,
                    UserName = DefaultName,
                    DisplayName = "Admin",
                    Signature = "OneBlog Signature",
                    EmailConfirmed = true
                };

                var userResult = await _userMgr.CreateAsync(defaultUser, DefaultPassword);

                if (!userResult.Succeeded)
                {
                    throw new InvalidProgramException("Failed to create seed user");
                }

                // 为账号分配角色
                var roleResult = await _userMgr.AddToRoleAsync(defaultUser, "Administrator");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidProgramException("Failed to create seed role");
                }

            }
        }
    }
}
