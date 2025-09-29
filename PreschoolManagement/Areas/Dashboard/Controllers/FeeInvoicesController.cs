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

        // GET: /Dashboard/FeeInvoices
        public async Task<IActionResult> Index(int? studentId, string? status)
        {
            var query = _db.FeeInvoices
                .Include(f => f.Student)
                .ThenInclude(s => s.ClassRoom)
                .AsQueryable();

            if (studentId.HasValue) query = query.Where(f => f.StudentId == studentId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(f => f.Status == status);

            // Gợi ý: tự đánh dấu Overdue theo tháng < tháng hiện tại và chưa Paid
            var today = DateTime.UtcNow.Date;
            var toUpdate = await query.Where(f => f.Status != "Paid" && f.Month < new DateTime(today.Year, today.Month, 1))
                                      .ToListAsync();
            foreach (var inv in toUpdate) inv.Status = "Overdue";
            if (toUpdate.Count > 0) await _db.SaveChangesAsync();

            ViewBag.Students = new SelectList(await _db.Students.OrderBy(s => s.FullName).ToListAsync(), "Id", "FullName", studentId);
            ViewBag.Status = status;
            var data = await query.OrderByDescending(f => f.Month).ThenBy(f => f.Student.FullName).ToListAsync();
            return View(data);
        }

        // GET: /Dashboard/FeeInvoices/Create
        public async Task<IActionResult> Create()
        {
            await BindStudentsAsync();
            return View(new FeeInvoice
            {
                Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Status = "Pending"
            });
        }

        // POST: /Dashboard/FeeInvoices/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeeInvoice model)
        {
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                await BindStudentsAsync(model.StudentId);
                return View(model);
            }

            // Unique (StudentId, Month)
            var exists = await _db.FeeInvoices.AnyAsync(f => f.StudentId == model.StudentId && f.Month == model.Month);
            if (exists)
            {
                ModelState.AddModelError("", "Hóa đơn tháng này cho học sinh đã tồn tại.");
                await BindStudentsAsync(model.StudentId);
                return View(model);
            }

            _db.FeeInvoices.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Dashboard/FeeInvoices/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.FeeInvoices.Include(f => f.Student).FirstOrDefaultAsync(f => f.Id == id);
            if (item == null) return NotFound();
            await BindStudentsAsync(item.StudentId);
            return View(item);
        }

        // POST: /Dashboard/FeeInvoices/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FeeInvoice model)
        {
            if (id != model.Id) return BadRequest();
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                await BindStudentsAsync(model.StudentId);
                return View(model);
            }

            try
            {
                _db.Update(model);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // check duplicate key student+month
                var dup = await _db.FeeInvoices.AnyAsync(f => f.Id != id && f.StudentId == model.StudentId && f.Month == model.Month);
                if (dup)
                {
                    ModelState.AddModelError("", "Hóa đơn tháng này cho học sinh đã tồn tại.");
                    await BindStudentsAsync(model.StudentId);
                    return View(model);
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Dashboard/FeeInvoices/MarkPaid/5
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

        // GET: /Dashboard/FeeInvoices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.FeeInvoices
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .FirstOrDefaultAsync(f => f.Id == id);
            return item == null ? NotFound() : View(item);
        }

        // GET: /Dashboard/FeeInvoices/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.FeeInvoices
                .Include(f => f.Student).ThenInclude(s => s.ClassRoom)
                .FirstOrDefaultAsync(f => f.Id == id);
            return item == null ? NotFound() : View(item);
        }

        // POST: /Dashboard/FeeInvoices/Delete/5
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

        private async Task BindStudentsAsync(int? selected = null)
        {
            var students = await _db.Students
                .OrderBy(s => s.FullName)
                .Select(s => new { s.Id, Display = s.FullName + " - " + (s.ClassRoom != null ? s.ClassRoom.Name : "Chưa có lớp") })
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "Id", "Display", selected);
        }

        private static void NormalizeModel(FeeInvoice m)
        {
            // Month: chuẩn về ngày 1 + UTC
            var dt = new DateTime(m.Month.Year, m.Month.Month, 1);
            m.Month = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            // Trạng thái: nếu đã trả đủ thì Paid
            if (m.Paid >= m.Amount && m.Amount > 0) m.Status = "Paid";
            else if (m.Status != "Pending" && m.Status != "Paid" && m.Status != "Overdue")
                m.Status = "Pending";
        }
    }
}
