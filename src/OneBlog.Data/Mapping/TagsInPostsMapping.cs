using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Mappings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Data.Mapping
{

    public class TagsInPostsMapping : BaseEntityMapping<TagsInPosts>
    {

        protected override void Execute(EntityTypeBuilder<TagsInPosts> builder)
        {
            builder.HasKey(t => new { t.PostId, t.TagId });

            builder.HasOne(pt => pt.Tags)
                .WithMany(p => p.TagsInPosts)
                .HasForeignKey(pt => pt.TagId);

            builder.HasOne(pt => pt.Posts)
                .WithMany(t => t.TagsInPosts)
                .HasForeignKey(pt => pt.PostId);
        }
    }


}
