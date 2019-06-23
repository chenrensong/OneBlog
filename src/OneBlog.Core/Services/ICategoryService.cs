using System.Collections.Generic;
using System.Threading.Tasks;
using OneBlog.Models;

namespace OneBlog.Core.Services
{
    public interface ICategoryService
    {
        Task<CategoryItem> Add(CategoryItem item);
        Task<bool> Delete(string id);
        Task<IList<CategoryItem>> GetAll();
        Task<IList<CategoryItem>> Get(int take = 10, int skip = 0);
        Task<CategoryItem> Get(string id);
        Task<bool> Update(CategoryItem item);
    }
}