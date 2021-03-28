
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Mappings
{

    public abstract class BaseEntityMapping
    {
        internal virtual string GetKey()
        {
            return "Default";
        }

        public abstract void Execute(ModelBuilder builder);
    }


    public abstract class BaseEntityMapping<T> : BaseEntityMapping where T : class
    {
        public BaseEntityMapping()
        {
        }

        public override void Execute(ModelBuilder builder)
        {
            Execute(builder.Entity<T>());
        }

        protected abstract void Execute(EntityTypeBuilder<T> builder);
    }

}