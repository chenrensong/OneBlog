using Microsoft.EntityFrameworkCore;
using OneBlog.Data;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Core.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<TagItem>> Get(int take = 10, int skip = 0)
        {
            var tagRepository = _unitOfWork.GetRepository<Tag>();
            var items = new List<TagItem>();
            var query = tagRepository.GetAll().Include(a => a.TagsInPosts)
                .OrderByDescending(m => m.TagsInPosts.Count).Select(m => DataMapper.Parse(m)
                ).Skip(skip);
            if (take > 0)
            {
                query = query.Take(take);
            }
            return await query.ToListAsync();
        }

        public async Task<TagItem> Get(string id)
        {
            var tagRepository = _unitOfWork.GetRepository<Tag>();
            var result = await tagRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == id, selector: m => new TagItem { TagName = m.TagName, TagCount = m.TagsInPosts.Count });
            return result;
        }

        public async Task<TagItem> Add(TagItem item)
        {
            var tagRepository = _unitOfWork.GetRepository<Tag>();
            var newItem = new Tag()
            {
                TagName = item.TagName,
                CreateTime = DateTime.UtcNow
            };
            await tagRepository.InsertAsync(newItem);
            await _unitOfWork.SaveChangesAsync();
            return item;
        }

        public async Task<bool> Delete(string name)
        {
            var tagRepository = _unitOfWork.GetRepository<Tag>();
            var tagsInPostsRepository = _unitOfWork.GetRepository<TagsInPosts>();
            var category = await tagRepository.GetFirstOrDefaultAsync(predicate: m => m.TagName == name, include: x => x.Include(c => c.TagsInPosts));
            if (category != null)
            {
                if (category.TagsInPosts.Count > 0)
                {
                    tagsInPostsRepository.Delete(category.TagsInPosts);
                }
                tagRepository.Delete(category);
                await _unitOfWork.SaveChangesAsync();
            }
            return true;
        }



    }
}
