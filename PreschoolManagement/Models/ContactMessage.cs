using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreschoolManagement.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Subject { get; set; }

        [Required, StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /* ---------- Quan hệ ---------- */

        // Người gửi nếu là user đăng nhập
        [StringLength(450)]
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        // Liên quan đến bé nào (nếu phụ huynh chọn)
        public int? RelatedStudentId { get; set; }
        public Student? RelatedStudent { get; set; }
    }
}
