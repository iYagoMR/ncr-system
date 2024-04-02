using Haver.DraftModels;
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
        public DbSet<Operations> OperationsS { get; set; }
        public DbSet<QualityRepresentative> QualityRepresentatives { get; set; }
        public DbSet<DraftQualityRepresentative> DraftQualityRepresentatives { get; set; }
        public DbSet<DraftEngineering> DraftEngineerings { get; set; }
        public DbSet<DraftOperations> DraftOperationsS { get; set; }
        public DbSet<DraftProcurement> DraftProcurements { get; set; }
        public DbSet<DraftReinspection> DraftReinspections { get; set; }
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
        public DbSet<EmployeePhoto> EmployeePhotos { get; set; }
        public DbSet<EmployeeThumbnail> EmployeeThumbnails { get; set; }
        public DbSet<VideoLink> VideoLinks { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Add a unique index to the Employee Email
            modelBuilder.Entity<Employee>()
            .HasIndex(a => new { a.Email })
            .IsUnique();

            //Add a unique index to the Parts Number
            modelBuilder.Entity<Part>()
            .HasIndex(a => new { a.PartNumber })
            .IsUnique();

            //Add a unique index to the Supplier code
            modelBuilder.Entity<Supplier>()
            .HasIndex(a => new { a.SupplierCode })
            .IsUnique();

            // Configure cascading delete for DraftEngineering related entities
            modelBuilder.Entity<DraftQualityRepresentative>()
                .HasMany(draftQualityRepresentative => draftQualityRepresentative.QualityPhotos)
                .WithOne(qualityPhoto => qualityPhoto.DraftQualityRepresentative)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DraftQualityRepresentative>()
                .HasMany(draftQualityRepresentative => draftQualityRepresentative.VideoLinks)
                .WithOne(videoLink => videoLink.DraftQualityRepresentative)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascading delete for DraftEngineering related entities
            modelBuilder.Entity<DraftEngineering>()
                .HasMany(draftEngineering => draftEngineering.QualityPhotos)
                .WithOne(qualityPhoto => qualityPhoto.DraftEngineering)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DraftEngineering>()
                .HasMany(draftEngineering => draftEngineering.VideoLinks)
                .WithOne(videoLink => videoLink.DraftEngineering)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascading delete for DraftOperations related entities
            modelBuilder.Entity<DraftOperations>()
                .HasMany(draftOperations => draftOperations.QualityPhotos)
                .WithOne(qualityPhoto => qualityPhoto.DraftOperations)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DraftOperations>()
                .HasMany(draftOperations => draftOperations.VideoLinks)
                .WithOne(videoLink => videoLink.DraftOperations)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascading delete for DraftProcurement related entities
            modelBuilder.Entity<DraftProcurement>()
                .HasMany(draftProcurement => draftProcurement.QualityPhotos)
                .WithOne(qualityPhoto => qualityPhoto.DraftProcurement)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DraftProcurement>()
                .HasMany(draftProcurement => draftProcurement.VideoLinks)
                .WithOne(videoLink => videoLink.DraftProcurement)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascading delete for DraftReinspection related entities
            modelBuilder.Entity<DraftReinspection>()
                .HasMany(draftReinspection => draftReinspection.QualityPhotos)
                .WithOne(qualityPhoto => qualityPhoto.DraftReinspection)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DraftReinspection>()
                .HasMany(draftReinspection => draftReinspection.VideoLinks)
                .WithOne(videoLink => videoLink.DraftReinspection)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascading delete for DraftEngineering related entities
            modelBuilder.Entity<DraftEngineering>()
                .HasMany(draftEngineering => draftEngineering.QualityPhotos)
                .WithOne(qualityPhoto => qualityPhoto.DraftEngineering)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DraftEngineering>()
                .HasMany(draftEngineering => draftEngineering.VideoLinks)
                .WithOne(videoLink => videoLink.DraftEngineering)
                .OnDelete(DeleteBehavior.Cascade);

        }

        public DbSet<Haver.Models.Procurement> Procurement { get; set; }

        public DbSet<Haver.Models.Reinspection> Reinspection { get; set; }
    }
}
