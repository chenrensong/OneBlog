using OneBlog.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneBlog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneBlog.Security;
using System.Reflection;

namespace OneBlog.Data
{

    public class RolesRepository : BaseRepository,IRolesRepository
    {
        private RoleManager<IdentityRole> _roleMgr;

        public RolesRepository(RoleManager<IdentityRole> roleMgr, AppDbContext ctx)
        {
            _roleMgr = roleMgr;
            _ctx = ctx;
        }

        public async Task<RoleItem> Add(RoleItem role)
        {
            var result = await _roleMgr.CreateAsync(new IdentityRole(role.RoleName));

            if (result.Succeeded)
            {
                return role;
            }
            return null;
        }

        public IEnumerable<RoleItem> Find(int take = 10, int skip = 0, string filter = "", string order = "")
        {
            var userRoles = new List<RoleItem>();

            if (take == 0)
            {
                take = _ctx.Roles.Count();
            }

            var roles = _ctx.Roles.Skip(skip)
                     .Take(take)
                     .ToList();

            foreach (var m in roles)
            {
                userRoles.Add(new RoleItem
                {
                    IsChecked = false,
                    RoleName = m.Name
                });
            }
            return userRoles;
        }

        public async Task<RoleItem> FindById(string id)
        {
            var role = await _roleMgr.FindByNameAsync(id);
            return new RoleItem() { RoleName = role.Name };
        }

        public async Task<IEnumerable<Group>> GetRoleRights(string name)
        {
            var role = await _roleMgr.FindByNameAsync(name);

            var groups = new List<Group>();
            // store the category for each Rights.
            var rightCategories = new Dictionary<Rights, string>();
            var roleRights = await _roleMgr.GetClaimsAsync(role);

            foreach (FieldInfo fi in typeof(Rights).GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
            {
                var right = (Rights)fi.GetValue(null);

                if (right != Rights.None)
                {
                    RightDetailsAttribute rightDetails = null;

                    foreach (Attribute attrib in fi.GetCustomAttributes(true))
                    {
                        if (attrib is RightDetailsAttribute)
                        {
                            rightDetails = (RightDetailsAttribute)attrib;
                            break;
                        }
                    }

                    var category = rightDetails == null ? RightCategory.General : rightDetails.Category;

                    var group = groups.FirstOrDefault(g => g.Title == category.ToString());

                    var prm = new Permission();
                    var rt = Right.GetRightByName(right.ToString());

                    prm.Id = right.ToString();
                    prm.Title = rt.DisplayName;
                    prm.IsChecked = roleRights.FirstOrDefault(m => m.Value == rt.FlagName) != null;

                    if (group == null)
                    {
                        var newGroup = new Group(category.ToString());
                        newGroup.Permissions.Add(prm);
                        groups.Add(newGroup);
                    }
                    else
                    {
                        group.Permissions.Add(prm);
                    }
                }
            }

            return groups;
        }

        public IEnumerable<RoleItem> GetUserRoles(string id)
        {
            var roles = new List<RoleItem>();

            roles.AddRange(_ctx.Roles
                .Select(r => new RoleItem
                {
                    RoleName = r.Name,
                    IsChecked = _ctx.UserRoles.FirstOrDefault(m => m.RoleId == r.Id) != null
                }));

            roles.Sort((r1, r2) => string.Compare(r1.RoleName, r2.RoleName));
            return roles;
        }

        public async Task<bool> Remove(string name)
        {
            try
            {
                var role = _roleMgr.FindByNameAsync(name).Result;

                var claims = _roleMgr.GetClaimsAsync(role).Result;
                foreach (var item in claims)
                {
                    await _roleMgr.RemoveClaimAsync(role, item);
                }
                await _roleMgr.DeleteAsync(role);
                //role = _context.Roles.FirstOrDefault(m => m.Id == role.Id);
                //_context.Roles.Remove(role);
                //await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
            return true;

        }

        public async Task<bool> SaveRights(List<Group> rights, string name)
        {
            try
            {
                var role = _roleMgr.FindByNameAsync(name).Result;
                var claims = _roleMgr.GetClaimsAsync(role).Result;
                var rightsCollection = new Dictionary<string, bool>();

                foreach (var g in rights)
                {
                    foreach (var r in g.Permissions)
                    {
                        if (r.IsChecked)
                        {
                            rightsCollection.Add(r.Id, r.IsChecked);
                        }
                    }
                }

                foreach (var right in Right.GetAllRights())
                {
                    if (right.CurFlag != Rights.None)
                    {
                        if (rightsCollection.ContainsKey(right.FlagName))
                        {
                            var claim = claims.FirstOrDefault(m => m.Value == right.FlagName);
                            if (claim == null)
                            {
                                await _roleMgr.AddClaimAsync(role, new System.Security.Claims.Claim(right.FlagName, right.FlagName));
                            }
                        }
                        else
                        {
                            var claim = claims.FirstOrDefault(m => m.Value == right.FlagName);
                            if (claim != null)
                            {
                                await _roleMgr.RemoveClaimAsync(role, claim);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return true;
        }


    }
}
