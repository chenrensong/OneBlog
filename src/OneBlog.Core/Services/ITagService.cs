using System.Collections.Generic;
using System.Threading.Tasks;
using OneBlog.Models;

namespace OneBlog.Core.Services
{
    public interface ITagService
    {
        Task<TagItem> Add(TagItem item);
        Task<bool> Delete(string name);
        Task<IList<TagItem>> Get(int take = 10, int skip = 0);
        Task<TagItem> Get(string id);
    }
}