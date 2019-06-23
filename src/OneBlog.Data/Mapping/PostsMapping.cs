using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Mappings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Data.Mapping
{
    public class PostsMapping : BaseEntityMapping<Post>
    {
        protected override void Execute(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.Content).IsRequired();
            builder.HasOne(x => x.Author).WithMany(x => x.Posts);
        }
    }


}
