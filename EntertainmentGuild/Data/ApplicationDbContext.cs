using EntertainmentGuild.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EntertainmentGuild.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<CarouselTopProduct> CarouselTopProducts { get; set; }

        public DbSet<RecommendedTopProduct> RecommendedTopProducts { get; set; }

        public DbSet<DisabledUser> DisabledUsers { get; set; }

        public DbSet<Address> Addresses { get; set; }
    }
}
