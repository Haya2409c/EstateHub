using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Models.Entities;

namespace RealtorsPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Agent> Agents { get; set; } = null!;
        public DbSet<News> NewsArticles { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<Region> Regions { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Area> Areas { get; set; } = null!;
        public DbSet<SellerProfile> SellerProfiles { get; set; } = null!;
        public DbSet<PropertyEnquiry> PropertyEnquiries { get; set; } = null!;
        public DbSet<SiteSetting> SiteSettings { get; set; } = null!;
        public DbSet<Package> Packages { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<EmailLog> EmailLogs { get; set; } = null!;
        public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(200);
                entity.Property(u => u.PhotoUrl).HasMaxLength(1000);
                entity.Property(u => u.AccountType).IsRequired().HasMaxLength(20).HasDefaultValue("Seller");
            });

            // SellerProfile
            modelBuilder.Entity<SellerProfile>(entity =>
            {
                entity.ToTable("SellerProfiles");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.CompanyName).HasMaxLength(300);
                entity.Property(s => s.PackageTier).IsRequired().HasMaxLength(50).HasDefaultValue("Basic");
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(s => s.User)
                      .WithOne(u => u.SellerProfile)
                      .HasForeignKey<SellerProfile>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Property
            modelBuilder.Entity<Property>(entity =>
            {
                entity.ToTable("Properties");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Title).IsRequired().HasMaxLength(250);
                entity.Property(p => p.Description).HasMaxLength(4000);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.PropertyType).HasMaxLength(100);
                entity.Property(p => p.Status).HasMaxLength(20);
                entity.Property(p => p.Amenities).HasMaxLength(500);
                entity.Property(p => p.Address).HasMaxLength(500);
                entity.Property(p => p.ListedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.SellerId).HasMaxLength(450); // matches ASP.NET Identity key length

                // Relations
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Properties)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Agent)
                      .WithMany(a => a.Properties)
                      .HasForeignKey(p => p.AgentId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Area)
                      .WithMany(a => a.Properties)
                      .HasForeignKey(p => p.AreaId)
                      .OnDelete(DeleteBehavior.SetNull);

                // SellerId is a plain FK — no nav property on Property to keep the entity lean
                entity.HasIndex(p => p.SellerId);
            });

            // PropertyImage
            modelBuilder.Entity<PropertyImage>(entity =>
            {
                entity.ToTable("PropertyImages");
                entity.HasKey(pi => pi.Id);
                entity.Property(pi => pi.Url).IsRequired().HasMaxLength(1000);
                entity.Property(pi => pi.Caption).HasMaxLength(250);

                entity.HasOne(pi => pi.Property)
                      .WithMany(p => p.Images)
                      .HasForeignKey(pi => pi.PropertyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
                entity.Property(c => c.Slug).HasMaxLength(200);
            });

            // Agent
            modelBuilder.Entity<Agent>(entity =>
            {
                entity.ToTable("Agents");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.FullName).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Email).HasMaxLength(200);
                entity.Property(a => a.Phone).HasMaxLength(50);
                entity.Property(a => a.PhotoUrl).HasMaxLength(1000);
                entity.Property(a => a.Bio).HasMaxLength(4000);
            });

            // News
            modelBuilder.Entity<News>(entity =>
            {
                entity.ToTable("News");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(300);
                entity.Property(n => n.Summary).HasMaxLength(1000);
                entity.Property(n => n.Content).IsRequired();
                entity.Property(n => n.PublishedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ContactMessage
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.ToTable("ContactMessages");
                entity.HasKey(cm => cm.Id);
                entity.Property(cm => cm.FullName).IsRequired().HasMaxLength(200);
                entity.Property(cm => cm.Email).IsRequired().HasMaxLength(200);
                entity.Property(cm => cm.Phone).HasMaxLength(50);
                entity.Property(cm => cm.Subject).IsRequired().HasMaxLength(250);
                entity.Property(cm => cm.Message).IsRequired().HasMaxLength(4000);
                entity.Property(cm => cm.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Country
            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Countries");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
                entity.Property(c => c.IsoCode).HasMaxLength(10);
            });

            // Region
            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("Regions");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(r => r.Country)
                      .WithMany(c => c.Regions)
                      .HasForeignKey(r => r.CountryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // City
            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("Cities");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(c => c.Region)
                      .WithMany(r => r.Cities)
                      .HasForeignKey(c => c.RegionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Area
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Areas");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(a => a.City)
                      .WithMany(c => c.Areas)
                      .HasForeignKey(a => a.CityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PropertyEnquiry
            modelBuilder.Entity<PropertyEnquiry>(entity =>
            {
                entity.ToTable("PropertyEnquiries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Message).HasMaxLength(2000);
                entity.Property(e => e.OwnerUserId).HasMaxLength(450);
                entity.Property(e => e.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Property)
                      .WithMany()
                      .HasForeignKey(e => e.PropertyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SiteSetting
            modelBuilder.Entity<SiteSetting>(entity =>
            {
                entity.ToTable("SiteSettings");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Key).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Value).HasMaxLength(2000);
                entity.Property(s => s.Group).HasMaxLength(50);
                entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(s => s.Key).IsUnique();
            });

            // Package
            modelBuilder.Entity<Package>(entity =>
            {
                entity.ToTable("Packages");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.TransactionId).IsRequired().HasMaxLength(200);
                entity.Property(p => p.UserId).IsRequired().HasMaxLength(450);
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Currency).IsRequired().HasMaxLength(10);
                entity.Property(p => p.Status).IsRequired().HasMaxLength(20);
                entity.Property(p => p.GatewayResponse).HasColumnType("nvarchar(max)");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(p => p.TransactionId).IsUnique();

                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Package)
                      .WithMany(pkg => pkg.Payments)
                      .HasForeignKey(p => p.PackageId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // EmailLog
            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.ToTable("EmailLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecipientUserId).HasMaxLength(450);
                entity.Property(e => e.ToEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Body).HasColumnType("nvarchar(max)");
                entity.Property(e => e.EmailType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.RecipientUser)
                      .WithMany()
                      .HasForeignKey(e => e.RecipientUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // SubscriptionHistory
            modelBuilder.Entity<SubscriptionHistory>(entity =>
            {
                entity.ToTable("SubscriptionHistories");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.UserId).IsRequired().HasMaxLength(450);
                entity.Property(s => s.Status).IsRequired().HasMaxLength(20);
                entity.Property(s => s.Notes).HasMaxLength(1000);
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Package)
                      .WithMany(p => p.SubscriptionHistories)
                      .HasForeignKey(s => s.PackageId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Payment)
                      .WithOne(p => p.SubscriptionHistory)
                      .HasForeignKey<SubscriptionHistory>(s => s.PaymentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Indexes
            modelBuilder.Entity<Property>().HasIndex(p => p.Slug).IsUnique(false);
            modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique(false);
            modelBuilder.Entity<News>().HasIndex(n => n.Slug).IsUnique(false);
        }
    }
}
