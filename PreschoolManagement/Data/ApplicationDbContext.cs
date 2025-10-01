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
        public DbSet<VisitRegistration> VisitRegistrations { get; set; } = default!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = default!;

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

            // ===== FeeInvoice =====
            builder.Entity<FeeInvoice>()
                .HasIndex(f => new { f.StudentId, f.Month })   // duy nhất Student + Tháng
                .IsUnique();

            builder.Entity<FeeInvoice>()
                .HasOne(f => f.Student)
                .WithMany(s => s.FeeInvoices)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FeeInvoice>()
                .Property(f => f.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<FeeInvoice>()
                .Property(f => f.Paid)
                .HasColumnType("decimal(18,2)");

            // ===== VisitRegistration =====
            builder.Entity<VisitRegistration>()
                .HasOne(v => v.Parent)
                .WithMany()
                .HasForeignKey(v => v.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<VisitRegistration>()
                .HasOne(v => v.Student)
                .WithMany()
                .HasForeignKey(v => v.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<VisitRegistration>()
                .HasOne(v => v.ClassRoom)
                .WithMany()
                .HasForeignKey(v => v.ClassRoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== ContactMessage =====
            builder.Entity<ContactMessage>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ContactMessage>()
                .HasOne(c => c.RelatedStudent)
                .WithMany()
                .HasForeignKey(c => c.RelatedStudentId)
                .OnDelete(DeleteBehavior.SetNull);
            // ===== FeeInvoice =====
            builder.Entity<FeeInvoice>()
                .HasIndex(f => new { f.StudentId, f.Month })   // duy nhất Student + Tháng
                .IsUnique();

            builder.Entity<FeeInvoice>()
                .HasOne(f => f.Student)
                .WithMany(s => s.FeeInvoices)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // NEW: liên kết Parent
            builder.Entity<FeeInvoice>()
                .HasOne(f => f.Parent)
                .WithMany()   // hoặc .WithMany(u => u.FeeInvoices) nếu bạn khai báo ICollection<FeeInvoice> trong ApplicationUser
                .HasForeignKey(f => f.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<FeeInvoice>()
                .Property(f => f.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<FeeInvoice>()
                .Property(f => f.Paid)
                .HasColumnType("decimal(18,2)");


        }

    }
}
