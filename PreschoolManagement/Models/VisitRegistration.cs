using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreschoolManagement.Models
{
    public class VisitRegistration
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string ParentName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Ngày tham quan"), DataType(DataType.Date)]
        [Required]
        public DateTime VisitDate { get; set; }

        [Display(Name = "Khung giờ")]
        [StringLength(20)]
        public string? VisitSlot { get; set; } // Sáng/Chiều (nếu muốn)

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /* ---------- Quan hệ ---------- */

        // Cha/Mẹ đang đăng ký (nếu là tài khoản đã đăng nhập)
        [StringLength(450)]
        public string? ParentId { get; set; }
        public ApplicationUser? Parent { get; set; }

        // Bé nào (nếu đã có trong hệ thống)
        public int? StudentId { get; set; }
        public Student? Student { get; set; }

        // Muốn tham quan lớp nào
        public int? ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

    }
}
