using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OneBlog.Models;

namespace OneBlog.Core.Services
{
    public interface IPostService
    {
        Task<PostDetail> Add(PostDetail postDetail, bool checkTitle = true);
        Task<IList<PostItem>> Get(int take = 10, int skip = 0, string term = null, string categoryId = null, string tag = null, string authorId = null);
        Task<IList<PostItem>> GetAll();
        Task<IPagedList<PostItem>> GetPagedList(int take = 10, int skip = 0, string term = null, string categoryId = null, string tag = null, string authorId = null);
        Task<bool> Update(PostDetail postDetail);
        Task<bool> Delete(string id);
        Task<long> AddReadCount(string id);
        Task<PostDetail> GetBySlug(string slug);
        Task<PostDetail> GetById(string id);


    }
}