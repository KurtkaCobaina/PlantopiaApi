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
        public DbSet<UserTask> UserTasks { get; set; } = null!; // ← ИСПРАВЛЕНО: UserTask вместо Task

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
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Experts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Consultation>(entity =>
            {
                entity.ToTable("consultations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

                // Связь с пользователем (кто заказал консультацию)
                entity.HasOne(c => c.User)
                    .WithMany(u => u.ConsultationsAsUser)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Связь с экспертом — без обратной навигации к User
                entity.HasOne(c => c.Expert)
                    .WithMany() // ← убрали указание на User.ConsultationsAsExpert
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
                entity.HasOne(d => d.Consultation)
                      .WithMany(c => c.Diagnoses)
                      .HasForeignKey(d => d.ConsultationId)
                      .OnDelete(DeleteBehavior.Cascade);
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
                      .WithMany(u => u.UserTasks) // ← согласовано с User.UserTasks
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}