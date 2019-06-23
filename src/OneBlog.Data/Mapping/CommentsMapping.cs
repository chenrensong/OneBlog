using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Mappings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Data.Mapping
{

    public class CommentsMapping : BaseEntityMapping<Comment>
    {
        protected override void Execute(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.HasOne(x => x.Posts).WithMany(x => x.Comments);
        }
    }


}
