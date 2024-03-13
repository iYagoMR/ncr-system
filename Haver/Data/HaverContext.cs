using Haver.Models;
using Microsoft.EntityFrameworkCore;

namespace Haver.Data
{
    public class HaverContext : DbContext
    {
        public HaverContext(DbContextOptions<HaverContext> options) : base(options)
        {

        }

        public DbSet<Engineering> Engineerings { get; set; }
        public DbSet<Purchasing> Purchasings { get; set; }
        public DbSet<QualityRepresentative> QualityRepresentatives { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<NCR> NCRs { get; set; }
        public DbSet<NCRNumber> NCRNumbers { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<PrelDecision> PrelDecisions { get; set; }
        public DbSet<EngReview> EngReviews { get; set; }
        public DbSet<ProcessApplicable> ProcessesApplicable { get; set; }
        public DbSet<Procurement> Procurements { get; set; }
        public DbSet<Reinspection> Reinspections { get; set; }
        public DbSet<QualityPhoto> QualityPhotos { get; set; }
        public DbSet<VideoLink> VideoLinks { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Add a unique index to the Employee Email
            modelBuilder.Entity<Employee>()
            .HasIndex(a => new { a.Email })
            .IsUnique();

        }

        public DbSet<Haver.Models.Procurement> Procurement { get; set; }

        public DbSet<Haver.Models.Reinspection> Reinspection { get; set; }
    }
}
