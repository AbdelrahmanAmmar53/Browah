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
        public DbSet<Advertisment> Advertisements { get; set; }
        public DbSet<AdvertisementSpecification> AdvertisementSpecifications { get; set; }
        public DbSet<AdvertisementReview> AdvertisementReviews { get; set; }
        public DbSet<AdvertismentImage> AdvertismentImages { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<VisaCard> VisaCards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Advertisment>().ToTable("Advertisments");
            builder.Entity<User>(user =>
            {
                user.HasKey(u => u.Id);
                user.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
                user.Property(u => u.SecondName).IsRequired().HasMaxLength(50);
                user.Property(u => u.ThirdName).IsRequired().HasMaxLength(50);
                user.Property(u => u.LastName).IsRequired().HasMaxLength(50);
                user.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(15);
                user.Property(u => u.Email).IsRequired().HasMaxLength(100);
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
            builder.Entity<Advertisment>()
                 .Property(a => a.Price)
                 .HasColumnType("decimal(18,2)");
            builder.Entity<Image>()
                .HasOne(i => i.User)
                .WithMany(u => u.Images)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<AdvertismentImage>()
                .HasKey(ai => new { ai.AdvertismentId, ai.ImageId });

            builder.Entity<AdvertismentImage>()
                .HasOne(ai => ai.Advertisment)
                .WithMany(a => a.AdvertismentImages)
                .HasForeignKey(ai => ai.AdvertismentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AdvertismentImage>()
                .HasOne(ai => ai.Image)
                .WithMany(i => i.AdvertismentImages)
                .HasForeignKey(ai => ai.ImageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Advertisement Specifications configuration
            builder.Entity<AdvertisementSpecification>()
                .HasOne(s => s.Advertisement)
                .WithMany()
                .HasForeignKey(s => s.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AdvertisementReview>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.NoAction); 
            builder.Entity<AdvertisementReview>()
                .HasOne(r => r.Advertisement)
                .WithMany()
                .HasForeignKey(r => r.AdvertisementId)
                .OnDelete(DeleteBehavior.Cascade); 
            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .HasOne(o => o.Delivery)
                .WithMany()
                .HasForeignKey(o => o.DeliveryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SystemSetting>(entity =>
            {
                entity.Property(s => s.CompanyCommissionPercentage).HasPrecision(18, 2);
                entity.Property(s => s.MaxCashLimitForDelivery).HasPrecision(18, 2);
                entity.Property(s => s.MinWithdrawalAmount).HasPrecision(18, 2);
                entity.Property(s => s.WithdrawalFeeFixed).HasPrecision(18, 2);
            });
            builder.Entity<Transaction>()
               .Property(t => t.Amount)
               .HasColumnType("decimal(18,2)");

            builder.Entity<User>()
                .Property(u => u.MinimumOrder)
                .HasColumnType("decimal(18,2)");
        }
    }
}
