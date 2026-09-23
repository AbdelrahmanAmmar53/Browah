using Berwah.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.App
{
    public class ApplicationDbContext: IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductViewed> RecentlyViewedProducts { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PlatFormSetting> PlatFormSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(user =>
            {
                user.HasKey(u => u.Id);
                user.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                user.Property(u => u.SecondName).IsRequired().HasMaxLength(50);
                user.Property(u => u.ThirdName).IsRequired().HasMaxLength(50);
                user.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                user.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(15);
                user.Property(u => u.Email).IsRequired().HasMaxLength(100);
                //user.Property(u => u.Password).IsRequired().HasMaxLength(100);
                //user.Property(u => u.BirthDay).IsRequired();
                //user.Property(u => u.Gender).IsRequired().HasMaxLength(10);
                user.Property(u => u.Address).IsRequired().HasMaxLength(200);
            });
            builder.Entity<Role>(role =>
            {
                role.HasKey(r => r.Id);
                role.Property(r => r.Name).IsRequired().HasMaxLength(50);
            });
            builder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });
                userRole.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRole)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRole)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });
            builder.Entity<Image>()
                    .HasOne(i => i.User)
                    .WithMany(u => u.Images)
                    .HasForeignKey(i => i.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
