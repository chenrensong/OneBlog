using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Mappings;
using Microsoft.EntityFrameworkCore.Providers;
using SS.Toolkit.Extensions;
using System.Reflection;

namespace OneBlog.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 文章
        /// </summary>
        public DbSet<Post> Posts { get; set; }
        /// <summary>
        /// Tag
        /// </summary>
        public DbSet<Tag> Tags { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public DbSet<Category> Categories { get; set; }
        /// <summary>
        /// 评论
        /// </summary>
        public DbSet<Comment> Comments { get; set; }
        /// <summary>
        /// 文章和分类关系
        /// </summary>
        public DbSet<PostsInCategories> PostsInCategories { get; set; }
        /// <summary>
        /// 分类和文章关系
        /// </summary>
        public DbSet<TagsInPosts> TagsInPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Post>().ToTable("Posts");
            builder.Entity<Tag>().ToTable("Tags");
            builder.Entity<Category>().ToTable("Categories");
            builder.Entity<PostsInCategories>().ToTable("PostsInCategories");
            builder.Entity<TagsInPosts>().ToTable("TagsInPosts");
            builder.Entity<Comment>().ToTable("Comments");
            //DataProviderFactory.OnModelCreating(builder);
            var currentAssembly = typeof(AppDbContext).GetTypeInfo().Assembly;
            var typesToRegister = currentAssembly.GetTypes<BaseEntityMapping>();//获取所有数据提供类型
            foreach (var type in typesToRegister)
            {
                type.Execute(builder);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            DataProviderFactory.OnConfiguring(optionsBuilder);
            base.OnConfiguring(optionsBuilder);
        }
    }
}