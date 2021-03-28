using Microsoft.EntityFrameworkCore;
using OneBlog.Data;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Collections;

namespace OneBlog.Core.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private IQueryable<PostItem> Query(int take = 10, int skip = 0, string term = null, string categoryId = null, string tag = null, string authorId = null)
        {
            var query = Query(term, categoryId, tag, authorId);
            if (take > 0)
            {
                query = query.Take(take);
            }
            query = query.Skip(skip);
            return query;
        }

        private IQueryable<PostItem> Query(string term = null, string categoryId = null, string tag = null, string authorId = null)
        {
            var postRepository = _unitOfWork.GetRepository<Post>();
            var query = postRepository.GetAll().Include(m => m.Author)
                .Include(m => m.PostsInCategories).ThenInclude(m => m.Categories)
                .OrderByDescending(m => m.DatePublished).Select(m => m);

            if (!string.IsNullOrEmpty(authorId))
            {
                query = query.Where(m => m.Author.Id == authorId);
            }

            if (!string.IsNullOrEmpty(categoryId))
            {
                query = query.Where(m => m.PostsInCategories.Any(x => x.CategoriesId == categoryId));
            }

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(m => m.Title.Contains(term) || m.Description.Contains(term));
            }

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(m => m.Tags.Contains(tag));
            }

            var newQuery = query.Select(m => DataMapper.Parse(m));
            return newQuery;
        }

        /// <summary>
        /// 获取整个列表（大数据网站慎用）
        /// </summary>
        /// <param name="isPublished"></param>
        /// <returns></returns>
        public async Task<IList<PostItem>> GetAll()
        {
            var postRepository = _unitOfWork.GetRepository<Post>();
            var query = postRepository.GetAll().Include(m => m.Author).Include(m => m.PostsInCategories)
                .OrderByDescending(m => m.DatePublished).Select(m => DataMapper.Parse(m));
            return await query.ToListAsync();
        }

        public async Task<IList<PostItem>> Get(int take = 10, int skip = 0, string term = null, string categoryId = null, string tag = null, string authorId = null)
        {
            return await Query(take, skip, term, categoryId, tag, authorId).ToListAsync();
        }

        public async Task<IPagedList<PostItem>> GetPagedList(int take = 10, int skip = 0, string term = null, string categoryId = null, string tag = null, string authorId = null)
        {
            var pageIndex = skip / (take <= 0 ? 1 : take);
            var pageSize = take <= 0 ? 10 : take;
            return await Query(term, categoryId, tag, authorId).ToPagedListAsync(pageIndex, pageSize);
        }


        public async Task<PostDetail> Add(PostDetail postDetail, bool checkTitle = true)
        {
            var postRepository = _unitOfWork.GetRepository<Post>();
            var appUesrRepository = _unitOfWork.GetRepository<AppUser>();
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var tagsInPostsRepository = _unitOfWork.GetRepository<TagsInPosts>();
            var postsInCategoriesRepository = _unitOfWork.GetRepository<PostsInCategories>();
            var tagRepository = _unitOfWork.GetRepository<Tag>();
            if (checkTitle)
            {
                var count = postRepository.Count(predicate: m => m.Title == postDetail.Title);
                if (count > 0)
                {
                    return null;
                }
            }

            var author = await appUesrRepository.FindAsync(postDetail.Author.Id);//predicate: m => m.Id == postDetail.Author.Id, disableTracking: false
            if (author == null)
            {
                return null;
            }

            var uniqueTags = postDetail.Tags.Distinct(new TagItemCompare());
            var tags = string.Join(",", uniqueTags.Select(m => m.TagName));
            var covers = new List<string> { postDetail.Cover1, postDetail.Cover2, postDetail.Cover3 }
            .Where(m => !string.IsNullOrEmpty(m)).ToList();
            var coversJson = Newtonsoft.Json.JsonConvert.SerializeObject(covers);
            if (string.IsNullOrEmpty(postDetail.DateCreated))
            {
                postDetail.DateCreated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
            }
            //content type
            var post = new Post()
            {
                ContentType = postDetail.ContentType,
                Description = postDetail.Description,
                Slug = postDetail.Slug,
                Content = postDetail.Content,
                HasCommentsEnabled = postDetail.HasCommentsEnabled,
                HasRecommendEnabled = postDetail.HasRecommendEnabled,
                IsPublished = postDetail.IsPublished,
                DatePublished = DateTime.ParseExact(postDetail.DateCreated, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Title = postDetail.Title,
                CoverImage = coversJson,
                Author = author,
                Tags = tags,
            };
            await postRepository.InsertAsync(post);
            foreach (var item in postDetail.Categories)
            {
                var category = await categoryRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == item.Id);
                await postsInCategoriesRepository.InsertAsync(new PostsInCategories()
                {
                    CategoriesId = category.Id,
                    PostsId = post.Id
                });
            }
            foreach (var item in uniqueTags)
            {
                var tag = await tagRepository.GetFirstOrDefaultAsync(predicate: m => m.TagName == item.TagName);
                if (tag == null)
                {
                    tag = new Tag() { CreateTime = DateTime.Now, TagName = item.TagName };
                    await tagRepository.InsertAsync(tag);
                }
                await tagsInPostsRepository.InsertAsync(new TagsInPosts()
                {
                    PostId = post.Id,
                    TagId = tag.Id
                });
            }
            postDetail.Id = post.Id;
            await _unitOfWork.SaveChangesAsync();
            return postDetail;
        }

        public async Task<bool> Update(PostDetail postDetail)
        {
            try
            {
                var postRepository = _unitOfWork.GetRepository<Post>();
                var appUesrRepository = _unitOfWork.GetRepository<AppUser>();
                var categoryRepository = _unitOfWork.GetRepository<Category>();
                var tagsInPostsRepository = _unitOfWork.GetRepository<TagsInPosts>();
                var postsInCategoriesRepository = _unitOfWork.GetRepository<PostsInCategories>();
                var tagRepository = _unitOfWork.GetRepository<Tag>();
                //var author = await appUesrRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == postDetail.Author.Id,
                //    disableTracking: false);
                var author = await appUesrRepository.FindAsync(postDetail.Author.Id);
                if (author == null)
                {
                    return false;
                }
                var uniqueTags = postDetail.Tags.Distinct(new TagItemCompare());
                var tags = string.Join(",", uniqueTags.Select(m => m.TagName));
                var covers = new List<string> { postDetail.Cover1, postDetail.Cover2, postDetail.Cover3 }
                .Where(m => !string.IsNullOrEmpty(m)).ToList();
                var coversJson = Newtonsoft.Json.JsonConvert.SerializeObject(covers);
                //content type
                var post = await postRepository.GetFirstOrDefaultAsync(
                    predicate: m => m.Id == postDetail.Id,
                    include: m => m.Include(x => x.TagsInPosts).Include(x => x.PostsInCategories));
                post.Description = postDetail.Description;
                post.Slug = postDetail.Slug;
                post.Content = postDetail.Content;
                post.HasCommentsEnabled = postDetail.HasCommentsEnabled;
                post.HasRecommendEnabled = postDetail.HasRecommendEnabled;
                post.IsPublished = postDetail.IsPublished;
                post.DatePublished = DateTime.ParseExact(postDetail.DateCreated, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                post.Title = postDetail.Title;
                post.CoverImage = coversJson;
                post.Author = author;
                post.Tags = tags;
                postRepository.Update(post);
                postsInCategoriesRepository.Delete(post.PostsInCategories);
                tagsInPostsRepository.Delete(post.TagsInPosts);
                foreach (var item in postDetail.Categories)
                {
                    var category = await categoryRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == item.Id);
                    await postsInCategoriesRepository.InsertAsync(new PostsInCategories()
                    {
                        CategoriesId = category.Id,
                        PostsId = post.Id
                    });
                }
                foreach (var item in uniqueTags)
                {
                    var tag = await tagRepository.GetFirstOrDefaultAsync(predicate: m => m.TagName == item.TagName);
                    if (tag == null)
                    {
                        tag = new Tag() { CreateTime = DateTime.Now, TagName = item.TagName };
                        await tagRepository.InsertAsync(tag);
                    }
                    await tagsInPostsRepository.InsertAsync(new TagsInPosts()
                    {
                        PostId = post.Id,
                        TagId = tag.Id
                    });
                }
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

            var postRepository = _unitOfWork.GetRepository<Post>();
            var appUesrRepository = _unitOfWork.GetRepository<AppUser>();
            var categoryRepository = _unitOfWork.GetRepository<Category>();
            var tagsInPostsRepository = _unitOfWork.GetRepository<TagsInPosts>();
            var postsInCategoriesRepository = _unitOfWork.GetRepository<PostsInCategories>();
            try
            {
                var post = await postRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == id, include: x => x.Include(c => c.PostsInCategories).Include(c => c.TagsInPosts));
                if (post != null)
                {
                    if (post.PostsInCategories != null && post.PostsInCategories.Count > 0)
                    {
                        postsInCategoriesRepository.Delete(post.PostsInCategories);
                    }
                    if (post.TagsInPosts != null && post.TagsInPosts.Count > 0)
                    {
                        tagsInPostsRepository.Delete(post.TagsInPosts);
                    }
                    postRepository.Delete(post);
                    await _unitOfWork.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PostDetail> GetById(string id)
        {
            var postRepository = _unitOfWork.GetRepository<Post>();
            var result = await postRepository.GetFirstOrDefaultAsync(predicate: m => m.Id == id,
                include: x => x.Include(f => f.Author).Include(m => m.PostsInCategories).ThenInclude(m => m.Categories),
                selector: post => DataMapper.ParsePostDetail(post));
            return result;
        }



        public async Task<PostDetail> GetBySlug(string slug)
        {
            var postRepository = _unitOfWork.GetRepository<Post>();
            var result = await postRepository.GetFirstOrDefaultAsync(predicate: m => m.Slug == slug,
                include: x => x.Include(f => f.Author).Include(m => m.PostsInCategories).ThenInclude(m => m.Categories),
                selector: post => DataMapper.ParsePostDetail(post));
            return result;
        }

        /// <summary>
        /// 添加阅读数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<long> AddReadCount(string id)
        {
            var postRepository = _unitOfWork.GetRepository<Post>();
            var post = await postRepository.FindAsync(id);
            post.ReadCount += 1;
            postRepository.Update(post);
            await _unitOfWork.SaveChangesAsync();
            return post.ReadCount;
        }


    }




}
