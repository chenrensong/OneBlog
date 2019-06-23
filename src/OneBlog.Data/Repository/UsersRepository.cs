using Microsoft.AspNetCore.Identity;
using OneBlog.Data.Contracts;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneBlog.Data
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {

        SignInManager<AppUser> _signInManager;
        UserManager<AppUser> _userManager;

        public UsersRepository(
            AppDbContext ctx,
            SignInManager<AppUser> signInManager, UserManager<AppUser> userManager
            )
        {
            _ctx = ctx;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public UserItem Add(UserItem user)
        {
            var appUser = new AppUser()
            {
                Email = user.Email,
                UserName = user.UserName,
                DisplayName = user.Profile.DisplayName,
                Signature = user.Profile.Signature
            };

            var userResult = _userManager.CreateAsync(appUser, user.Password).Result; // Temp Password

            if (!userResult.Succeeded) throw new InvalidProgramException("Failed to create user");

            foreach (var item in user.Roles)
            {
                if (!item.IsChecked)
                {
                    continue;
                }
                var roleResult = _userManager.AddToRoleAsync(appUser, item.RoleName).Result;
                if (!roleResult.Succeeded) throw new InvalidProgramException("Failed to create role");
            }

            return user;
        }


        public IEnumerable<UserItem> Find(int take = 10, int skip = 0, string filter = "", string order = "")
        {
            var users = new List<UserItem>();

            if (take == 0)
            {
                take = _ctx.Users.Count();
            }

            var members = _ctx.Users.Skip(skip)
                     .Take(take)
                     .ToList();

            foreach (var m in members)
            {
                users.Add(new UserItem
                {
                    IsChecked = false,
                    UserName = m.UserName,
                    Email = m.Email,
                    Profile = GetProfile(m),
                    Roles = GetRoles(m)
                });
            }
            return users;
        }

        public UserItem FindByName(string name)
        {
            var user = _userManager.FindByNameAsync(name).Result;
            if (user != null)
            {
                return new UserItem
                {
                    IsChecked = false,
                    UserName = user.UserName,
                    Email = user.Email,
                    Profile = GetProfile(user),
                    Roles = GetRoles(user)
                };
            }
            return null;
        }

        public UserItem FindById(string id)
        {
            var user = _userManager.FindByIdAsync(id).Result;
            if (user != null)
            {
                return new UserItem
                {
                    IsChecked = false,
                    UserName = user.UserName,
                    Email = user.Email,
                    Profile = GetProfile(user),
                    Roles = GetRoles(user)
                };
            }
            return null;
        }

        public bool Remove(string name)
        {
            var user = _userManager.FindByNameAsync(name).Result;
            if (user != null)
            {
                var result = _userManager.DeleteAsync(user).Result;
                return result.Succeeded;
            }

            return false;
        }

        public bool SaveProfile(UserItem user)
        {
            var appUser = _userManager.FindByNameAsync(user.UserName).Result;
            appUser.DisplayName = user.Profile.DisplayName;
            appUser.Signature = user.Profile.Signature;
            var result = _userManager.UpdateAsync(appUser).Result;
            return result.Succeeded;
        }

        public bool Update(UserItem user)
        {
            throw new NotImplementedException();
        }


        public Profile GetProfile(string name)
        {
            var user = _userManager.FindByNameAsync(name).Result;
            if (user != null)
            {
                return GetProfile(user);
            }
            return null;
        }


        Profile GetProfile(AppUser user)
        {
            return new Profile()
            {
                DisplayName = user.DisplayName,
                Signature = user.Signature,
                PhotoUrl = user.Avatar
            };

        }


        List<RoleItem> GetRoles(AppUser user)
        {
            var userRoles = new List<RoleItem>();
            var task = _userManager.GetRolesAsync(user);
            var roles = task.Result;

            foreach (var r in roles)
            {
                userRoles.Add(new RoleItem() { RoleName = r });
            }
            return userRoles;
        }


    }
}
