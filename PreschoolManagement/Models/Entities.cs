namespace PreschoolManagement.Models
{
    public class ClassRoom // Lớp học
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Grade { get; set; } // Mầm/Chồi/Lá...
        public string? Room { get; set; }
        public string? TeacherId { get; set; } // ApplicationUser (Giáo viên chủ nhiệm)
        public ApplicationUser? Teacher { get; set; }
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }

    public class Student // Học sinh
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = "N/A";
        public int? ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }
        public string? ParentId { get; set; } // ApplicationUser (Phụ huynh)
        public ApplicationUser? Parent { get; set; }
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }

    public class Attendance // Điểm danh
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = default!;
        public DateTime Date { get; set; }
        public bool Present { get; set; }
        public string? Note { get; set; }
    }

    public class FeeInvoice // Hóa đơn học phí
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = default!;
        public DateTime Month { get; set; } // 2025-09-01 -> Tháng 9/2025
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Paid/Overdue
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Announcement // Thông báo
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public string Audience { get; set; } = "All"; // All/Teachers/Parents
    }

    public class ActivitySchedule // Thời khoá biểu/hoạt động
    {
        public int Id { get; set; }
        public int ClassRoomId { get; set; }
        public ClassRoom ClassRoom { get; set; } = default!;
        public DayOfWeek DayOfWeek { get; set; }
        public string Slot { get; set; } = default!; // Sáng/Chiều/Tiết 1-5,...
        public string Activity { get; set; } = default!;
    }
}
