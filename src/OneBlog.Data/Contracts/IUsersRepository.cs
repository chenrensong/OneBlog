using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Data.Contracts
{
    /// <summary>
    /// Users repository
    /// </summary>
    public interface IUsersRepository
    {
        /// <summary>
        /// Post list
        /// </summary>
        /// <param name="filter">Filter expression</param>
        /// <param name="order">Order expression</param>
        /// <param name="skip">Records to skip</param>
        /// <param name="take">Records to take</param>
        /// <returns>List of users</returns>
        IEnumerable<UserItem> Find(int take = 10, int skip = 0, string filter = "", string order = "");
        /// <summary>
        /// Get single post
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>User object</returns>
        UserItem FindById(string id);

        UserItem FindByName(string name);
        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="user">Blog user</param>
        /// <returns>Saved user</returns>
        UserItem Add(UserItem user);
        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="user">User to update</param>
        /// <returns>True on success</returns>
        bool Update(UserItem user);
        /// <summary>
        /// Save user profile
        /// </summary>
        /// <param name="user">Blog user</param>
        /// <returns>True on success</returns>
        bool SaveProfile(UserItem user);
        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>True on success</returns>
        bool Remove(string id);
    }
}
