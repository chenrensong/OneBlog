using OneBlog.Data;
using OneBlog.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OneBlog.Core
{
    public class DataMapper
    {
        public static List<CategoryItem> Parse(IList<Category> categories)
        {
            if (categories == null || categories.Count == 0)
            {
                return null;
            }
            var categoryList = new List<CategoryItem>();
            foreach (var c in categories)
            {
                var item = Parse(c);
                categoryList.Add(item);
            }
            return categoryList;
        }

        public static TagItem Parse(Tag tag)
        {
            var item = new TagItem();
            item.TagName = tag.TagName;
            if (tag.TagsInPosts != null)
            {
                item.TagCount = tag.TagsInPosts.Count;
            }
            return item;
        }

        public static CategoryItem Parse(Category category)
        {
            var item = new CategoryItem();
            item.Id = category.Id;
            item.Title = category.Title;
            item.Description = category.Description;
            item.Parent = new SelectOption
            {
                OptionName = category.ParentName,
                OptionValue = category.ParentId
            };
            if (category.PostsInCategories != null)
            {
                item.Count = category.PostsInCategories.Count;
            }
            return item;
        }

        public static IList<CommentItem> Parse(string postId, IList<Comment> comments)
        {
            IList<CommentItem> list = new List<CommentItem>();
            if (comments != null)
            {
                foreach (var item in comments)
                {
                    list.Add(Parse(postId, item));
                }
            }
            return list;
        }

        public static CommentItem Parse(string postId, Comment comment)
        {
            var item = new CommentItem();
            item.PostId = postId;
            item.Id = comment.Id;
            item.ParentId = comment.ParentId;
            item.Content = comment.Content;
            item.Ip = comment.Ip;
            return item;
        }

        /// <summary>
        /// Post2PostItem
        /// </summary>
        /// <param name="post"></param>
        /// <param name="commentCount">不使用关联查询性能较差</param>
        /// <returns></returns>
        public static PostItem Parse(Post post)
        {
            var categories = post.Categories.ToList();
            var author = new Author();
            if (post.Author != null)
            {
                author.Id = post.Author.Id;
                author.Avatar = post.Author.Avatar;
                author.Signature = post.Author.Signature;
                author.DisplayName = post.Author.DisplayName;
                author.Name = post.Author.UserName;
            }
            var postitem = new PostItem
            {
                Id = post.Id,
                Author = author,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                RelativeLink = "/post/" + post.Id,
                ReadCount = post.ReadCount,
                DatePublished = post.DatePublished,
                DateCreated = post.DatePublished.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Categories = Parse(categories),
                Tags = post.Tags.Split(',').Where(m => !string.IsNullOrEmpty(m)).Select(m => new TagItem() { TagName = m }).ToList(),
                //CommentsCount = commentCount.HasValue ? commentCount.Value : 0,//影响性能
                //Comments = Parse(post.Id, post.Comments),
                IsPublished = post.IsPublished,
            };
            if (!string.IsNullOrEmpty(post.CoverImage))
            {
                var covers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(post.CoverImage);
                postitem.Cover1 = covers.Count > 0 ? covers[0] : null;
                postitem.Cover2 = covers.Count > 1 ? covers[1] : null;
                postitem.Cover3 = covers.Count > 2 ? covers[2] : null;
            }
            return postitem;
        }

        public static PostDetail ParsePostDetail(Post post)
        {
            var categories = post.Categories.ToList();
            var author = new Author();
            if (post.Author != null)
            {
                author.Id = post.Author.Id;
                author.Avatar = post.Author.Avatar;
                author.Signature = post.Author.Signature;
                author.DisplayName = post.Author.DisplayName;
                author.Name = post.Author.UserName;
            }
            var postitem = new PostDetail
            {
                Id = post.Id,
                ContentType = post.ContentType,
                Author = author,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                RelativeLink = "/post/" + post.Id,
                //ReadCount = post.ReadCount,
                //DatePublished = post.DatePublished,
                DateCreated = post.DatePublished.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Categories = Parse(categories),
                Tags = post.Tags.Split(',').Select(m => new TagItem() { TagName = m }).ToList(),
                //CommentsCount = commentCount.HasValue ? commentCount.Value : 0,//影响性能
                //Comments = Parse(post.Id, post.Comments),
                IsPublished = post.IsPublished,
            };
            if (!string.IsNullOrEmpty(post.CoverImage))
            {
                var covers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(post.CoverImage);
                postitem.Cover1 = covers.Count > 0 ? covers[0] : null;
                postitem.Cover2 = covers.Count > 1 ? covers[1] : null;
                postitem.Cover3 = covers.Count > 2 ? covers[2] : null;
            }
            return postitem;
        }
    }
}
