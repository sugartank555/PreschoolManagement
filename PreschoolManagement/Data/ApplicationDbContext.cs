using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Models;

namespace PreschoolManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<FeeInvoice> FeeInvoices => Set<FeeInvoice>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<ActivitySchedule> ActivitySchedules => Set<ActivitySchedule>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Student>()
                .HasIndex(s => s.Code)
                .IsUnique();

            builder.Entity<ClassRoom>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Student>()
                .HasOne(s => s.Parent)
                .WithMany()
                .HasForeignKey(s => s.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.Date })
                .IsUnique();

            builder.Entity<FeeInvoice>()
                .HasIndex(f => new { f.StudentId, f.Month })
                .IsUnique();
        }
    }
}
