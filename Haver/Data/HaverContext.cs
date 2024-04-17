using Haver.DraftModels;
using Haver.Models;
using Haver.Utilities;
using iTextSharp.text;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Haver.Data
{
    public class HaverContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string UserName
        {
            get; private set;
        }

        public HaverContext(DbContextOptions<HaverContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
                UserName ??= "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }

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
        public DbSet<Photo> QualityPhotos { get; set; }
        public DbSet<EmployeePhoto> EmployeePhotos { get; set; }
        public DbSet<EmployeeThumbnail> EmployeeThumbnails { get; set; }
        public DbSet<VideoLink> VideoLinks { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<ConfigurationVariable> ConfigurationVariables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //One to one relationship between employee and notification
            modelBuilder.Entity<Employee>()
                .HasMany(a => a.Notifications)
                .WithOne(b => b.Employee)
                .HasForeignKey(b => b.EmployeeID);


            //Add a unique index to the Employee Email
            modelBuilder.Entity<Employee>()
                .HasIndex(a => new { a.Email })
                .IsUnique();

            //Add a unique index to problem
            modelBuilder.Entity<Problem>()
                .HasIndex(a => new { a.ProblemDescription })
                .IsUnique();

            //Add a unique index to the Parts Number
            modelBuilder.Entity<Part>()
                .HasIndex(a => new { a.PartNumber })
                .IsUnique();

            //Add a unique index to the NCRNumber
            modelBuilder.Entity<NCRNumber>()
                .HasIndex(a => new { a.Year, a.Counter }) 
                .IsUnique();

            //Add a unique index to the NCRNumber in NCR
            modelBuilder.Entity<NCR>()
                .HasIndex(a => new { a.NCRNum })
                .IsUnique();

            //Add a unique index to the Supplier code
            modelBuilder.Entity<Supplier>()
                .HasIndex(a => new { a.SupplierCode })
                .IsUnique();

            // Configure cascade delete Employee to notifications
            modelBuilder.Entity<Employee>()
                .HasMany(p => p.Notifications)
                .WithOne(c => c.Employee)
                .OnDelete(DeleteBehavior.Cascade);

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

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            if(UserName != "Seed Data")
                            {
                                trackable.CreatedOn = now;
                            }
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }

    }
}
