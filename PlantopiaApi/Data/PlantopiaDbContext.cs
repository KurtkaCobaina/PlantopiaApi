// Data/PlantopiaDbContext.cs
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Models;

namespace PlantopiaApi.Data
{
    public class PlantopiaDbContext : DbContext
    {
        public PlantopiaDbContext(DbContextOptions<PlantopiaDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Expert> Experts { get; set; } = null!;
        public DbSet<Consultation> Consultations { get; set; } = null!;
        public DbSet<Diagnosis> Diagnoses { get; set; } = null!;
        public DbSet<FertilizerCalculation> FertilizerCalculations { get; set; } = null!;
        public DbSet<NdviMap> NdviMaps { get; set; } = null!;
        public DbSet<SoilTest> SoilTests { get; set; } = null!;
        public DbSet<UserTask> UserTasks { get; set; } = null!;
        public DbSet<Crop> Crops { get; set; } = null!;
        public DbSet<SoilType> SoilTypes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<Expert>(entity =>
            {
                entity.ToTable("experts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                
            });

            modelBuilder.Entity<Consultation>(entity =>
            {
                entity.ToTable("consultations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

                entity.HasOne(c => c.User)
                    .WithMany(u => u.ConsultationsAsUser)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Expert)
                    .WithMany()
                    .HasForeignKey(c => c.ExpertId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Diagnosis>(entity =>
            {
                entity.ToTable("diagnoses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasOne(d => d.User)
                      .WithMany(u => u.Diagnoses)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Ignore("Consultation");
                entity.Ignore("ConsultationId");
                entity.Ignore("ConsultationId1");
            });

            modelBuilder.Entity<FertilizerCalculation>(entity =>
            {
                entity.ToTable("fertilizer_calculations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CalculatedAt).HasDefaultValueSql("now()");
                entity.HasOne(f => f.User)
                      .WithMany(u => u.FertilizerCalculations)
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Добавляем связи с Crop и SoilType
                entity.HasOne<Crop>()
                      .WithMany()
                      .HasForeignKey(f => f.CropId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne<SoilType>()
                      .WithMany()
                      .HasForeignKey(f => f.SoilId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<NdviMap>(entity =>
            {
                entity.ToTable("ndvi_maps");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasOne(n => n.User)
                      .WithMany(u => u.NdviMaps)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SoilTest>(entity =>
            {
                entity.ToTable("soil_tests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasOne(s => s.User)
                      .WithMany(u => u.SoilTests)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserTask>(entity =>
            {
                entity.ToTable("tasks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
                entity.HasOne(t => t.User)
                      .WithMany(u => u.UserTasks)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Crop>(entity =>
            {
                entity.ToTable("crops");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.ScientificName).HasColumnName("scientific_name");
                entity.Property(e => e.OptimalNG1m2Min).HasColumnName("optimal_n_g_1m2_min");
                entity.Property(e => e.OptimalNG1m2Max).HasColumnName("optimal_n_g_1m2_max");
                entity.Property(e => e.OptimalPG1m2Min).HasColumnName("optimal_p_g_1m2_min");
                entity.Property(e => e.OptimalPG1m2Max).HasColumnName("optimal_p_g_1m2_max");
                entity.Property(e => e.OptimalKG1m2Min).HasColumnName("optimal_k_g_1m2_min");
                entity.Property(e => e.OptimalKG1m2Max).HasColumnName("optimal_k_g_1m2_max");
                entity.Property(e => e.TypicalYieldKg1m2).HasColumnName("typical_yield_kg_1m2");
                entity.Property(e => e.GrowthPeriodDays).HasColumnName("growth_period_days");
            });

            modelBuilder.Entity<SoilType>(entity =>
            {
                entity.ToTable("soil_types");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.PhLevelMin).HasColumnName("ph_level_min");
                entity.Property(e => e.PhLevelMax).HasColumnName("ph_level_max");
                entity.Property(e => e.NCorrectionFactor).HasColumnName("n_correction_factor").HasDefaultValue(1.0m);
                entity.Property(e => e.PCorrectionFactor).HasColumnName("p_correction_factor").HasDefaultValue(1.0m);
                entity.Property(e => e.KCorrectionFactor).HasColumnName("k_correction_factor").HasDefaultValue(1.0m);
            });
        }
    }
}