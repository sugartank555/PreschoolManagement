using Microsoft.AspNetCore.Identity;

namespace PreschoolManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
