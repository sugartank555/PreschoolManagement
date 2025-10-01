using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Models;

namespace PreschoolManagement.Controllers
{
   
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SchedulesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? classId, DayOfWeek? dow)
        {
            var userId = User.GetUserId(); // extension ở dưới

            // Lấy danh sách lớp “liên quan” tới user
            var relatedClassIds = new HashSet<int>();

            // Nếu là phụ huynh: lớp của con
            var childrenClasses = await _db.Students
                .AsNoTracking()
                .Where(s => s.ParentId == userId && s.ClassRoomId != null)
                .Select(s => s.ClassRoomId!.Value)
                .ToListAsync();
            foreach (var id in childrenClasses) relatedClassIds.Add(id);

            // Nếu là giáo viên: lớp chủ nhiệm
            var teacherClasses = await _db.ClassRooms
                .AsNoTracking()
                .Where(c => c.TeacherId == userId)
                .Select(c => c.Id)
                .ToListAsync();
            foreach (var id in teacherClasses) relatedClassIds.Add(id);

            // Nếu không có lớp nào “liên quan” → cho chọn tất cả
            var classQuery = _db.ClassRooms.AsNoTracking().OrderBy(c => c.Name);
            var selectable = relatedClassIds.Any()
                ? await classQuery.Where(c => relatedClassIds.Contains(c.Id)).ToListAsync()
                : await classQuery.ToListAsync();

            ViewBag.Classes = new SelectList(selectable, "Id", "Name", classId);

            // Nếu user có đúng 1 lớp mà không truyền classId → auto chọn
            if (!classId.HasValue && relatedClassIds.Count == 1)
                classId = relatedClassIds.First();

            var schedules = _db.ActivitySchedules
                .AsNoTracking()
                .Include(a => a.ClassRoom)
                .AsQueryable();

            if (classId.HasValue) schedules = schedules.Where(a => a.ClassRoomId == classId.Value);
            if (dow.HasValue) schedules = schedules.Where(a => a.DayOfWeek == dow.Value);

            var data = await schedules
                .OrderBy(a => a.ClassRoom!.Name).ThenBy(a => a.DayOfWeek).ThenBy(a => a.Slot)
                .ToListAsync();

            ViewBag.SelectedDow = dow;
            ViewData["Title"] = "Thời khóa biểu";
            return View(data);
        }
    }

    internal static class ClaimsExt
    {
        public static string? GetUserId(this System.Security.Claims.ClaimsPrincipal user) =>
            user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
}
