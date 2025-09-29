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
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public StudentsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, int? classId)
        {
            var qr = _db.Students.Include(s => s.ClassRoom).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) qr = qr.Where(s => s.FullName.Contains(q) || s.Code.Contains(q));
            if (classId.HasValue) qr = qr.Where(s => s.ClassRoomId == classId.Value);

            ViewBag.Classes = new SelectList(await _db.ClassRooms.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", classId);
            ViewBag.Keyword = q;
            return View(await qr.OrderBy(s => s.FullName).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Classes = new SelectList(await _db.ClassRooms.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View(new Student { BirthDate = DateTime.Today.AddYears(-4) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Classes = new SelectList(await _db.ClassRooms.ToListAsync(), "Id", "Name", model.ClassRoomId);
                return View(model);
            }
            _db.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var s = await _db.Students.FindAsync(id);
            if (s == null) return NotFound();
            ViewBag.Classes = new SelectList(await _db.ClassRooms.ToListAsync(), "Id", "Name", s.ClassRoomId);
            return View(s);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Classes = new SelectList(await _db.ClassRooms.ToListAsync(), "Id", "Name", model.ClassRoomId);
                return View(model);
            }
            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var s = await _db.Students.Include(x => x.ClassRoom).FirstOrDefaultAsync(x => x.Id == id);
            return s == null ? NotFound() : View(s);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Students.FindAsync(id);
            return s == null ? NotFound() : View(s);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var s = await _db.Students.FindAsync(id);
            if (s != null) _db.Students.Remove(s);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
