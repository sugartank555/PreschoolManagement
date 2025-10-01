using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Models;

namespace PreschoolManagement.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ---------- ViewModel cho trang Index ----------
        public sealed class StudentsIndexVm
        {
            public List<Student> Items { get; set; } = new();
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int TotalPages => (int)System.Math.Ceiling((double)System.Math.Max(0, TotalItems) / System.Math.Max(1, PageSize));
            public int? ClassId { get; set; }
            public string? Keyword { get; set; }
        }

        // ================= Helpers =====================

        private async Task LoadLookupsAsync(int? selectedClassId = null, string? selectedParentId = null)
        {
            var classes = await _db.ClassRooms
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            ViewBag.ClassRooms = new SelectList(classes, "Id", "Name", selectedClassId);

            // danh sách user ở role Parent
            var parents = await _userManager.GetUsersInRoleAsync("Parent");
            var parentItems = parents
                .OrderBy(p => p.FullName ?? p.UserName ?? p.Email)
                .Select(p => new SelectListItem
                {
                    Value = p.Id,
                    Text = (p.FullName ?? p.UserName ?? p.Email)
                           + (string.IsNullOrEmpty(p.Email) ? "" : $" ({p.Email})"),
                    Selected = (p.Id == selectedParentId)
                })
                .ToList();

            ViewBag.Parents = parentItems;
        }

        private bool CanWrite()
            => User.IsInRole("Admin") || User.IsInRole("Staff");

        private static int NormalizePageSize(int pageSize)
        {
            // chỉ cho phép 5/10/20/50; mặc định 10
            return pageSize switch
            {
                5 or 10 or 20 or 50 => pageSize,
                _ => 10
            };
        }

        // ================= Index (Paging + Filter) =====================

        // /Dashboard/Students?classId=1&keyword=an&page=1&pageSize=10
        public async Task<IActionResult> Index(int? classId, string? keyword, int page = 1, int pageSize = 10)
        {
            pageSize = NormalizePageSize(pageSize);
            if (page < 1) page = 1;

            var q = _db.Students
                .AsNoTracking()
                .Include(s => s.ClassRoom)
                .Include(s => s.Parent)
                .AsQueryable();

            if (classId.HasValue)
                q = q.Where(s => s.ClassRoomId == classId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(s => s.FullName.Contains(keyword) || s.Code.Contains(keyword));
            }

            var total = await q.CountAsync();

            // nếu page vượt quá tổng trang thì kéo về trang cuối
            var totalPages = (int)System.Math.Ceiling((double)System.Math.Max(1, total) / pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = await q
                .OrderBy(s => s.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await LoadLookupsAsync(classId, null);
            ViewData["Title"] = "Học sinh";

            var vm = new StudentsIndexVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                ClassId = classId,
                Keyword = keyword
            };

            return View(vm);
        }

        // ================= Details =====================

        public async Task<IActionResult> Details(int id)
        {
            var s = await _db.Students
                .AsNoTracking()
                .Include(x => x.ClassRoom)
                .Include(x => x.Parent)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) return NotFound();

            ViewData["Title"] = "Chi tiết học sinh";
            return View(s);
        }

        // ================= Create =====================

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create()
        {
            await LoadLookupsAsync();
            ViewData["Title"] = "Thêm học sinh";
            return View(new Student { BirthDate = DateTime.Today.AddYears(-5) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([Bind("Code,FullName,BirthDate,Gender,ClassRoomId,ParentId")] Student model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(model.ClassRoomId, model.ParentId);
                ViewData["Title"] = "Thêm học sinh";
                return View(model);
            }

            var exists = await _db.Students.AnyAsync(s => s.Code == model.Code);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Code), "Mã học sinh đã tồn tại.");
                await LoadLookupsAsync(model.ClassRoomId, model.ParentId);
                return View(model);
            }

            _db.Students.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm học sinh.";
            return RedirectToAction(nameof(Index));
        }

        // ================= Edit =====================

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var s = await _db.Students.FindAsync(id);
            if (s == null) return NotFound();

            await LoadLookupsAsync(s.ClassRoomId, s.ParentId);
            ViewData["Title"] = "Sửa học sinh";
            return View(s);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,FullName,BirthDate,Gender,ClassRoomId,ParentId")] Student model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(model.ClassRoomId, model.ParentId);
                ViewData["Title"] = "Sửa học sinh";
                return View(model);
            }

            var duplicate = await _db.Students
                .AnyAsync(s => s.Id != model.Id && s.Code == model.Code);
            if (duplicate)
            {
                ModelState.AddModelError(nameof(model.Code), "Mã học sinh đã tồn tại.");
                await LoadLookupsAsync(model.ClassRoomId, model.ParentId);
                return View(model);
            }

            _db.Attach(model);
            _db.Entry(model).Property(x => x.Code).IsModified = true;
            _db.Entry(model).Property(x => x.FullName).IsModified = true;
            _db.Entry(model).Property(x => x.BirthDate).IsModified = true;
            _db.Entry(model).Property(x => x.Gender).IsModified = true;
            _db.Entry(model).Property(x => x.ClassRoomId).IsModified = true;
            _db.Entry(model).Property(x => x.ParentId).IsModified = true;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật học sinh.";
            return RedirectToAction(nameof(Index));
        }

        // ================= Delete =====================

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Students
                .AsNoTracking()
                .Include(x => x.ClassRoom)
                .Include(x => x.Parent)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            ViewData["Title"] = "Xóa học sinh";
            return View(s);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var s = await _db.Students.FindAsync(id);
            if (s == null) return NotFound();

            _db.Students.Remove(s);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa học sinh.";
            return RedirectToAction(nameof(Index));
        }
    }
}
