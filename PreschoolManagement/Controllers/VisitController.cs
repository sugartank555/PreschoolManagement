using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PreschoolManagement.Data;
using PreschoolManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PreschoolManagement.Controllers
{
    public class VisitController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public VisitController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Đăng ký tham quan";

            await LoadDropdowns_All(); // <-- nạp tất cả học sinh + lớp
            var vm = new VisitRegistration { VisitDate = DateTime.Today.AddDays(1) };

            // Prefill nếu đã đăng nhập
            if (User.Identity?.IsAuthenticated == true)
            {
                var me = await _userManager.GetUserAsync(User);
                if (me != null)
                {
                    vm.ParentName = me.FullName ?? me.UserName ?? "";
                    vm.Email = me.Email ?? "";
                }
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VisitRegistration model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns_All();
                ViewData["Title"] = "Đăng ký tham quan";
                return View(model);
            }

            // (tuỳ chọn) đảm bảo ngày không ở quá khứ
            if (model.VisitDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.VisitDate), "Ngày tham quan không được trong quá khứ.");
                await LoadDropdowns_All();
                return View(model);
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var me = await _userManager.GetUserAsync(User);
                if (me != null) model.ParentId = me.Id;
            }

            _db.VisitRegistrations.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã nhận đăng ký, chúng tôi sẽ liên hệ sớm!";
            return RedirectToAction(nameof(Thanks), new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Thanks(int id)
        {
            var entity = await _db.VisitRegistrations
                .Include(v => v.ClassRoom)
                .Include(v => v.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return RedirectToAction(nameof(Create));
            ViewData["Title"] = "Đăng ký thành công";
            return View(entity);
        }

        /// <summary>
        /// Nạp dropdown: tất cả ClassRooms và tất cả Students (không lọc theo ParentId)
        /// </summary>
        private async Task LoadDropdowns_All()
        {
            var classes = await _db.ClassRooms
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            var students = await _db.Students
                .OrderBy(s => s.FullName)
                .Select(s => new { s.Id, s.FullName })
                .ToListAsync();

            ViewBag.ClassRooms = new SelectList(classes, "Id", "Name");
            ViewBag.MyStudents = new SelectList(students, "Id", "FullName");
        }
    }
}
