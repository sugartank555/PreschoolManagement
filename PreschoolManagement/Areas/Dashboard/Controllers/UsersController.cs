using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Models;

namespace PreschoolManagement.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
        {
            _db = db; _userManager = um; _roleManager = rm;
        }

        public async Task<IActionResult> Index(string? role)
        {
            var users = await _db.Users.OrderBy(u => u.Email).ToListAsync();
            var map = new Dictionary<string, IList<string>>();
            foreach (var u in users) map[u.Id] = await _userManager.GetRolesAsync(u);

            ViewBag.SelectedRole = role;
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            if (!string.IsNullOrWhiteSpace(role))
                users = users.Where(u => map[u.Id].Contains(role)).ToList();

            ViewBag.UserRoles = map;
            return View(users);
        }

        public IActionResult Create(string role) => View(new CreateUserVM { Role = role });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                FullName = vm.FullName
            };
            var res = await _userManager.CreateAsync(user, vm.Password ?? "User@123");
            if (!res.Succeeded)
            {
                foreach (var e in res.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }
            if (!string.IsNullOrWhiteSpace(vm.Role) && await _roleManager.RoleExistsAsync(vm.Role))
                await _userManager.AddToRoleAsync(user, vm.Role);

            return RedirectToAction(nameof(Index), new { role = vm.Role });
        }

        [HttpPost]
        public async Task<IActionResult> AddToRole(string id, string role)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();
            if (!await _roleManager.RoleExistsAsync(role)) return BadRequest("Role không tồn tại");
            await _userManager.AddToRoleAsync(u, role);
            return RedirectToAction(nameof(Index), new { role });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromRole(string id, string role)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();
            await _userManager.RemoveFromRoleAsync(u, role);
            return RedirectToAction(nameof(Index), new { role });
        }
    }

    public class CreateUserVM
    {
        public string? FullName { get; set; }
        public string Email { get; set; } = default!;
        public string? Password { get; set; }
        public string? Role { get; set; } // Admin/Teacher/Parent/Staff
    }
}
