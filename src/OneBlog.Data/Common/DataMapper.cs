using OneBlog.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OneBlog.Data.Common
{
    /// <summary>
    /// 数据映射类
    /// </summary>
    public class DataMapper
    {
        private readonly AppDbContext _ctx;

        public DataMapper(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        #region Mapper
        /// <summary>
        /// Get post converted to Json
        /// </summary>
        /// <param name="post">Post</param>
        /// <returns>Json post</returns>
        public PostItem GetPost(Post post)
        {
            var categories = _ctx.PostsInCategories.Where(m => m.PostsId == post.Id).Select(m => m.Categories).ToList();
            var author = new Author();
            author.Id = post.Author?.Id;
            author.Avatar = post.Author?.Avatar;
            author.Signature = post.Author?.Signature;
            author.DisplayName = post.Author?.DisplayName;
            author.Name = post.Author?.UserName;
            var postitem = new PostItem
            {
                Id = post.Id,
                Author = author,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                RelativeLink = "/post/" + post.Id,
                CommentsCount = post.Comments != null ? post.Comments.Count : 0,
                ReadCount = post.ReadCount,
                DatePublished = post.DatePublished,
                DateCreated = post.DatePublished.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Categories = GetCategories(categories),
                Tags = GetTags(post.Tags.Split(',')),
                IsPublished = post.IsPublished,
            };
            if (!string.IsNullOrEmpty(post.CoverImage))
            {
                var covers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(post.CoverImage); //TODO
                if (covers.Count > 0)
                {
                    postitem.Cover1 = covers[0];
                }
                if (covers.Count > 1)
                {
                    postitem.Cover2 = covers[1];
                }
                if (covers.Count > 2)
                {
                    postitem.Cover3 = covers[2];
                }
            }
            return postitem;
        }
        /// <summary>
        /// Get detailed post
        /// </summary>
        /// <param name="post">Post</param>
        /// <returns>Json post detailed</returns>
        public PostDetail GetPostDetail(Post post)
        {
            var categories = _ctx.PostsInCategories.Where(m => m.PostsId == post.Id).Select(m => m.Categories).ToList();
            //if (post.ContentType == (int)DataConst.ContentType.Markdown)
            //{
            //    post.Content = Markdig.Markdown.ToHtml(post.Content);
            //}
            var author = new Author();
            author.Id = post.Author?.Id;
            author.Signature = post.Author?.Signature;
            author.DisplayName = post.Author?.DisplayName;
            author.Name = post.Author?.UserName;
            author.Avatar = post.Author?.Avatar;
            author.SiteUrl = post.Author?.SiteUrl;
            var postDetail = new PostDetail
            {
                ContentType = post.ContentType,
                Id = post.Id,
                Author = author,
                Title = post.Title,
                Slug = post.Slug,
                Description = post.Description,
                RelativeLink = "/post/" + post.Id,
                Content = post.Content,
                DateCreated = post.DatePublished.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Categories = GetCategories(categories),
                Tags = GetTags(post.Tags.Split(',')),
                Comments = GetComments(post),
                HasCommentsEnabled = post.HasCommentsEnabled,
                HasRecommendEnabled = post.HasRecommendEnabled,
                IsPublished = post.IsPublished,
                IsDeleted = false,
                CanUserEdit = true,
                CanUserDelete = true
            };
            if (!string.IsNullOrEmpty(post.CoverImage))
            {
                var covers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(post.CoverImage);
                if (covers.Count > 0)
                {
                    postDetail.Cover1 = covers[0];
                }
                if (covers.Count > 1)
                {
                    postDetail.Cover2 = covers[1];
                }
                if (covers.Count > 2)
                {
                    postDetail.Cover2 = covers[2];
                }
            }
            return postDetail;
        }

        private SelectOption ItemParent(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            var item = _ctx.Categories.Where(c => c.Id == id).FirstOrDefault();
            return new SelectOption { OptionName = item.Title, OptionValue = item.Id.ToString() };
        }


        public List<CategoryItem> GetCategories(IList<Category> categories)
        {
            if (categories == null || categories.Count == 0)
            {
                return null;
            }
            //var html = categories.Aggregate("", (current, cat) => current + string.Format
            //("<a href='#' onclick=\"ChangePostFilter('Category','{0}','{1}')\">{1}</a>, ", cat.Id, cat.Title));
            var categoryList = new List<CategoryItem>();
            foreach (var c in categories)
            {
                var item = new CategoryItem();
                item.Id = c.Id;
                item.Title = c.Title;
                item.Description = c.Description;
                item.Parent = ItemParent(c.ParentId);
                categoryList.Add(item);
            }
            return categoryList;
        }


        /// <summary>
        /// todo
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        internal string[] GetComments(Post post)
        {
            if (post.Comments == null || post.Comments.Count == 0)
            {
                return null;
            }

            string[] comments = new string[3];
            comments[0] = "0";
            comments[1] = post.Comments.Count.ToString();
            comments[2] = "0";
            //comments[0] = post.NotApprovedComments.Count.ToString();
            //comments[1] = post.ApprovedComments.Count.ToString();
            //comments[2] = post.SpamComments.Count.ToString();
            return comments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        internal IList<TagItem> GetTags(IList<string> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return null;
            }
            var items = new List<TagItem>();
            foreach (var item in tags)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                items.Add(new TagItem { TagName = item });
            }
            return items;
        }


        /// <summary>
        /// Get json comment
        /// </summary>
        /// <param name="c">Comment</param>
        /// <param name="postComments">List of comments</param>
        /// <returns>Json comment</returns>
        public CommentItem GetComment(Comment c, List<Comment> postComments)
        {
            var jc = new CommentItem();
            jc.Id = c.Id;
            jc.IsApproved = c.IsApproved;
            jc.IsSpam = c.IsSpam;
            jc.IsPending = !c.IsApproved && !c.IsSpam;
            jc.DisplayName = c.DisplayName;
            jc.Email = c.Email;
            jc.Content = c.Content;
            jc.DateCreated = c.CreateDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            jc.HasChildren = postComments.Where(pc => pc.ParentId == c.Id).FirstOrDefault() != null;
            return jc;
        }


        /// <summary>
        ///     Get comment detail
        /// </summary>
        /// <param name="c">Comment</param>
        /// <returns>Json comment detail</returns>
        public CommentItem GetCommentItem(Comment c)
        {
            var item = new CommentItem();
            item.Id = c.Id;
            item.ParentId = c.ParentId;
            item.PostId = c.Posts.Id;
            item.Content = c.Content;
            item.Ip = c.Ip;
            return item;
        }

        #endregion

    }
}
