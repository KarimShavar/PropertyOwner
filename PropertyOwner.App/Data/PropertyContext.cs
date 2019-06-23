using Microsoft.EntityFrameworkCore;
using PropertyOwner.App.Data.Entities;

namespace PropertyOwner.App.Data
{
    public class PropertyContext : DbContext
    {
        public PropertyContext(DbContextOptions<PropertyContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
    }
}