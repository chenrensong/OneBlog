using OneBlog.Core.Services;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneBlog.Tests.Fakes
{
    class FakeCategoryRepository : ICategoryService
    {
        Task<CategoryItem> ICategoryService.Add(CategoryItem item)
        {
            return Task.FromResult(item);
        }

        public Task<bool> Delete(string id)
        {
            return Task.FromResult(true);
        }

        public Task<IList<CategoryItem>> GetAll()
        {
            IList<CategoryItem> items = new List<CategoryItem>();
            items.Add(new CategoryItem()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "test"
            });
            return Task.FromResult(items);
        }

        public Task<IList<CategoryItem>> Get(int take = 10, int skip = 0)
        {
            IList<CategoryItem> items = new List<CategoryItem>();
            items.Add(new CategoryItem()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "test"
            });
            return Task.FromResult(items);
        }

        public Task<CategoryItem> Get(string id)
        {
            var item = new CategoryItem()
            {
                Id = id,
                Title = "test"
            };
            return Task.FromResult(item);
        }

        Task<bool> ICategoryService.Update(CategoryItem item)
        {
            return Task.FromResult(true);
        }
    }
}
