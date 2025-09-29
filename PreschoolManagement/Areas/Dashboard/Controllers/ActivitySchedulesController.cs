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
    public class ActivitySchedulesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ActivitySchedulesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? classId, DayOfWeek? dow)
        {
            var qr = _db.ActivitySchedules.Include(a => a.ClassRoom).AsQueryable();
            if (classId.HasValue) qr = qr.Where(a => a.ClassRoomId == classId.Value);
            if (dow.HasValue) qr = qr.Where(a => a.DayOfWeek == dow.Value);

            ViewBag.Classes = new SelectList(await _db.ClassRooms.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", classId);
            ViewBag.Dow = dow;
            return View(await qr.OrderBy(a => a.ClassRoom.Name).ThenBy(a => a.DayOfWeek).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Classes = new SelectList(await _db.ClassRooms.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View(new ActivitySchedule { DayOfWeek = DayOfWeek.Monday, Slot = "Sáng" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivitySchedule model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Classes = new SelectList(await _db.ClassRooms.ToListAsync(), "Id", "Name", model.ClassRoomId);
                return View(model);
            }
            _db.ActivitySchedules.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.ActivitySchedules.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.Classes = new SelectList(await _db.ClassRooms.ToListAsync(), "Id", "Name", item.ClassRoomId);
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivitySchedule model)
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

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.ActivitySchedules.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.ActivitySchedules.FindAsync(id);
            if (item != null) _db.ActivitySchedules.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
