using OneBlog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneBlog.Data.Contracts
{
    /// <summary>
    /// Roles repository
    /// </summary>
    public interface IRolesRepository
    {
        /// <summary>
        /// Post list
        /// </summary>
        /// <param name="filter">Filter expression</param>
        /// <param name="order">Order expression</param>
        /// <param name="skip">Records to skip</param>
        /// <param name="take">Records to take</param>
        /// <returns>List of roles</returns>
        IEnumerable<RoleItem> Find(int take = 10, int skip = 0, string filter = "", string order = "");
        /// <summary>
        /// Get single role
        /// </summary>
        /// <param name="id">Role name</param>
        /// <returns>User object</returns>
        Task<RoleItem> FindById(string id);
        /// <summary>
        /// Add new role
        /// </summary>
        /// <param name="role">Blog user</param>
        /// <returns>Saved user</returns>
        Task<RoleItem> Add(RoleItem role);
        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>True on success</returns>
        Task<bool> Remove(string id);
        /// <summary>
        /// Get role rights
        /// </summary>
        /// <param name="role">Role</param>
        /// <returns>Collection of rights</returns>
        Task<IEnumerable<Group>> GetRoleRights(string role);
        /// <summary>
        /// Roles for user
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Roles</returns>
        IEnumerable<RoleItem> GetUserRoles(string id);
        /// <summary>
        /// Save rights
        /// </summary>
        /// <param name="rights">Rights</param>
        /// <param name="id">Role id</param>
        /// <returns>True if success</returns>
        Task<bool> SaveRights(List<Group> rights, string id);
    }
}
