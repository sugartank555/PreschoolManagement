using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Models;

namespace PreschoolManagement.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin")]
    public class ClassRoomsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const int DefaultPageSize = 12;

        public ClassRoomsController(ApplicationDbContext db) => _db = db;

        // GET: /Dashboard/ClassRooms
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = DefaultPageSize)
        {
            var qr = _db.ClassRooms
                        .Include(c => c.Teacher)
                        .Include(c => c.Students)
                        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                qr = qr.Where(c => c.Name.Contains(q) || (c.Grade ?? "").Contains(q) || (c.Room ?? "").Contains(q));
            }

            var total = await qr.CountAsync();
            var items = await qr.OrderBy(c => c.Name)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            return View(items);
        }

        // GET: /Dashboard/ClassRooms/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.ClassRooms
                                .Include(c => c.Teacher)
                                .Include(c => c.Students)
                                .FirstOrDefaultAsync(c => c.Id == id);
            return item == null ? NotFound() : View(item);
        }

        // GET: /Dashboard/ClassRooms/Create
        public async Task<IActionResult> Create()
        {
            await BindTeachersAsync();
            return View(new ClassRoom());
        }

        // POST: /Dashboard/ClassRooms/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassRoom model)
        {
            if (!ModelState.IsValid)
            {
                await BindTeachersAsync(model.TeacherId);
                return View(model);
            }

            _db.ClassRooms.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Dashboard/ClassRooms/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.ClassRooms.FindAsync(id);
            if (item == null) return NotFound();

            await BindTeachersAsync(item.TeacherId);
            return View(item);
        }

        // POST: /Dashboard/ClassRooms/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClassRoom model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await BindTeachersAsync(model.TeacherId);
                return View(model);
            }

            try
            {
                _db.Update(model);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.ClassRooms.AnyAsync(x => x.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Dashboard/ClassRooms/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.ClassRooms
                                .Include(c => c.Teacher)
                                .FirstOrDefaultAsync(c => c.Id == id);
            return item == null ? NotFound() : View(item);
        }

        // POST: /Dashboard/ClassRooms/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.ClassRooms.FindAsync(id);
            if (item != null)
            {
                _db.ClassRooms.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Nạp dropdown giáo viên: lọc user có role "Teacher"
        /// </summary>
        private async Task BindTeachersAsync(string? selectedId = null)
        {
            // Lấy danh sách user thuộc role Teacher
            var teacherRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Teacher");
            var teachers = new List<ApplicationUser>();

            if (teacherRole != null)
            {
                var teacherUserIds = await _db.UserRoles
                    .Where(ur => ur.RoleId == teacherRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                teachers = await _db.Users
                    .Where(u => teacherUserIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
            }

            ViewBag.Teachers = new SelectList(
                teachers.Select(t => new { t.Id, Display = string.IsNullOrWhiteSpace(t.FullName) ? t.Email : t.FullName + " (" + t.Email + ")" }),
                "Id", "Display", selectedId
            );
        }
    }
}
