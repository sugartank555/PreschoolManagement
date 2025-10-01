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
    public class FeeInvoicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public FeeInvoicesController(ApplicationDbContext db) => _db = db;

        // LIST
        // LIST + Pagination
        public async Task<IActionResult> Index(int? studentId, string? status, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 10;

            // Base query (lọc trước)
            var baseQuery = _db.FeeInvoices
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .Include(f => f.Parent)
                .AsQueryable();

            if (studentId.HasValue)
                baseQuery = baseQuery.Where(f => f.StudentId == studentId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                baseQuery = baseQuery.Where(f => f.Status == status);

            // Auto mark Overdue (chỉ chạy trên tập đang xem để tránh quá nặng)
            var today = DateTime.UtcNow.Date;
            var overdueMonthStart = new DateTime(today.Year, today.Month, 1);
            var toUpdate = await baseQuery
                .Where(f => f.Status != "Paid" && f.Month < overdueMonthStart)
                .ToListAsync();
            foreach (var inv in toUpdate) inv.Status = "Overdue";
            if (toUpdate.Count > 0) await _db.SaveChangesAsync();

            // Đếm tổng sau khi lọc
            var totalCount = await baseQuery.CountAsync();

            // Lấy trang
            var items = await baseQuery
                .OrderByDescending(f => f.Month).ThenBy(f => f.Student.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Dropdown học sinh và giữ lại filter để render
            ViewBag.Students = new SelectList(
                await _db.Students.OrderBy(s => s.FullName).ToListAsync(), "Id", "FullName", studentId);

            ViewBag.Status = status;
            ViewBag.StudentId = studentId;

            // Thông tin phân trang
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(items);
        }

        // CREATE (GET)
        public async Task<IActionResult> Create()
        {
            await BindStudentsAsync();
            await BindParentsAsync();

            return View(new FeeInvoice
            {
                Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Status = "Pending"
            });
        }

        // CREATE (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,ParentId,Month,Status,Amount,Paid")] FeeInvoice model)
        {
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                await BindStudentsAsync();
                await BindParentsAsync();
                return View(model);
            }

            // nếu chưa chọn Parent → gán theo Student
            if (string.IsNullOrEmpty(model.ParentId))
            {
                var st = await _db.Students.AsNoTracking()
                                           .FirstOrDefaultAsync(s => s.Id == model.StudentId);
                model.ParentId = st?.ParentId;
            }

            // chặn trùng (Student + Month)
            bool exists = await _db.FeeInvoices.AnyAsync(f =>
                f.StudentId == model.StudentId &&
                f.Month.Year == model.Month.Year &&
                f.Month.Month == model.Month.Month);
            if (exists)
            {
                ModelState.AddModelError("", "Hóa đơn tháng này cho học sinh đã tồn tại.");
                await BindStudentsAsync();
                await BindParentsAsync();
                return View(model);
            }

            _db.FeeInvoices.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.FeeInvoices
                                .Include(f => f.Student)
                                .Include(f => f.Parent)
                                .FirstOrDefaultAsync(f => f.Id == id);
            if (item == null) return NotFound();

            await BindStudentsAsync();
            await BindParentsAsync(item.ParentId);
            return View(item);
        }

        // EDIT (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,ParentId,Month,Status,Amount,Paid")] FeeInvoice model)
        {
            if (id != model.Id) return BadRequest();
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                await BindStudentsAsync();
                await BindParentsAsync(model.ParentId);
                return View(model);
            }

            // nếu để trống Parent → suy ra từ Student
            if (string.IsNullOrEmpty(model.ParentId))
            {
                var st = await _db.Students.AsNoTracking()
                                           .FirstOrDefaultAsync(s => s.Id == model.StudentId);
                model.ParentId = st?.ParentId;
            }

            try
            {
                // duy nhất Student+Month
                bool dup = await _db.FeeInvoices.AnyAsync(f =>
                    f.Id != model.Id &&
                    f.StudentId == model.StudentId &&
                    f.Month == model.Month);
                if (dup)
                {
                    ModelState.AddModelError("", "Hóa đơn tháng này cho học sinh đã tồn tại.");
                    await BindStudentsAsync();
                    await BindParentsAsync(model.ParentId);
                    return View(model);
                }

                _db.Update(model);
                await _db.SaveChangesAsync();
            }
            catch
            {
                await BindStudentsAsync();
                await BindParentsAsync(model.ParentId);
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // MARK PAID
        [HttpPost]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var inv = await _db.FeeInvoices.FindAsync(id);
            if (inv == null) return NotFound();

            inv.Paid = inv.Amount;
            inv.Status = "Paid";
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.FeeInvoices
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .Include(f => f.Parent)
                .FirstOrDefaultAsync(f => f.Id == id);
            return item == null ? NotFound() : View(item);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.FeeInvoices
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .Include(f => f.Parent)
                .FirstOrDefaultAsync(f => f.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.FeeInvoices.FindAsync(id);
            if (item != null)
            {
                _db.FeeInvoices.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==== Helpers =========================================================

        // Danh sách học sinh (kèm ParentId xuất qua data-attribute ở view)
        private async Task BindStudentsAsync()
        {
            ViewBag.StudentOpts = await _db.Students
                .Include(s => s.ClassRoom)
                .OrderBy(s => s.FullName)
                .Select(s => new
                {
                    s.Id,
                    s.ParentId,
                    Text = s.FullName + " - " + (s.ClassRoom != null ? s.ClassRoom.Name : "Chưa có lớp")
                })
                .ToListAsync();
        }

        // Danh sách phụ huynh (chỉ user role Parent)
        private async Task BindParentsAsync(string? selected = null)
        {
            var parentRoleId = await _db.Roles
                .Where(r => r.Name == "Parent")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var parents = await _db.Users
                .Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == parentRoleId))
                .OrderBy(u => u.FullName ?? u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Display = (u.FullName ?? u.UserName) + (u.Email != null ? $" ({u.Email})" : "")
                })
                .ToListAsync();

            ViewBag.Parents = new SelectList(parents, "Id", "Display", selected);
        }

        private static void NormalizeModel(FeeInvoice m)
        {
            var monthStart = new DateTime(m.Month.Year, m.Month.Month, 1);
            m.Month = DateTime.SpecifyKind(monthStart, DateTimeKind.Utc);

            if (m.Amount > 0 && m.Paid >= m.Amount) m.Status = "Paid";
            else if (m.Status != "Pending" && m.Status != "Paid" && m.Status != "Overdue")
                m.Status = "Pending";
        }
    }
}
