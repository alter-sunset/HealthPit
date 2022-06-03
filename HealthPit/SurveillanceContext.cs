using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HealthPit
{
    public partial class SurveillanceContext : DbContext
    {
        public SurveillanceContext()
        {
            
        }
        public SurveillanceContext(DbContextOptions<SurveillanceContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Shift> Shifts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employees");

                entity.HasIndex(e => e.PositionId, "IX_employees_PositionId");

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.PositionId);

                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.FirstName).IsRequired();
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.ToTable("positions");

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.ToTable("shifts");

                entity.HasIndex(e => e.EmployeeId, "IX_shifts_EmployeeId");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Shifts)
                    .HasForeignKey(d => d.EmployeeId);

                //entity.Ignore(u => u.Employee);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
