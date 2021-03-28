using Microsoft.EntityFrameworkCore;
using OneBlog.Data;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace OneBlog.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<CategoryItem>> Get(int take = 10, int skip = 0)
        {
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var items = new List<CategoryItem>();
            var query = categoryRepository.GetAll().Include(a => a.PostsInCategories).Select(c => DataMapper.Parse(c))
                .Skip(skip);
            if (take > 0)
            {
                query = query.Take(take);
            }
            var list = await query.ToListAsync();
            return list;
        }

        public async Task<CategoryItem> Get(string id)
        {
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var result = await categoryRepository.GetFirstOrDefaultAsync(
                predicate: m => m.Id == id,
                selector:
                c => DataMapper.Parse(c));
            return result;
        }

        public async Task<CategoryItem> Add(CategoryItem item)
        {
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var newItem = new Category()
            {
                Description = item.Description,
                ParentId = item.Parent?.OptionValue,
                Title = item.Title,
            };
            await categoryRepository.InsertAsync(newItem);
            await _unitOfWork.SaveChangesAsync();
            item.Id = newItem.Id;
            return item;

        }

        public async Task<bool> Update(CategoryItem item)
        {
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            try
            {
                var category = await categoryRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == item.Id);
                category.Title = item.Title;
                category.Description = item.Description;
                category.ParentId = item.Parent?.OptionValue;
                categoryRepository.Update(category);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Delete(string id)
        {

            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var postsInCategoriesRepository = _unitOfWork.GetRepository<PostsInCategories>();
            try
            {
                var category = await categoryRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == id, include: x => x.Include(c => c.PostsInCategories));
                if (category != null)
                {
                    if (category.PostsInCategories.Count > 0)
                    {
                        postsInCategoriesRepository.Delete(category.PostsInCategories);
                    }
                    categoryRepository.Delete(category);
                    await _unitOfWork.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<IList<CategoryItem>> GetAll()
        {
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var items = new List<CategoryItem>();
            var query = categoryRepository.GetAll().Include(a => a.PostsInCategories).Select(c => DataMapper.Parse(c));
            var list = await query.ToListAsync();
            return list;
        }
    }
}
