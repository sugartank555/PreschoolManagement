using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PreschoolManagement.Data;
using PreschoolManagement.Models;

namespace PreschoolManagement.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Contact
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Liên hệ";
            await LoadStudentsDropdown_All();   // <-- nạp TẤT CẢ học sinh

            var vm = new ContactMessage();

            // Nếu có đăng nhập: prefill tên/email
            if (User.Identity?.IsAuthenticated == true)
            {
                var me = await _userManager.GetUserAsync(User);
                if (me != null)
                {
                    vm.Name = me.FullName ?? me.UserName ?? "";
                    vm.Email = me.Email ?? "";
                }
            }

            return View(vm);
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage model)
        {
            await LoadStudentsDropdown_All();   // nạp lại dropdown khi lỗi validate

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Liên hệ";
                return View(model);
            }

            // Gắn người tạo nếu có đăng nhập
            if (User.Identity?.IsAuthenticated == true)
            {
                var me = await _userManager.GetUserAsync(User);
                if (me != null)
                {
                    model.CreatedById = me.Id;
                }
            }

            // Lúc này RelatedStudentId (nếu có) là ID hợp lệ bất kể thuộc phụ huynh nào
            _db.ContactMessages.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn! Chúng tôi đã nhận được liên hệ.";
            return RedirectToAction(nameof(Success));
        }

        // GET: /Contact/Success
        [HttpGet]
        public IActionResult Success()
        {
            ViewData["Title"] = "Gửi liên hệ thành công";
            return View();
        }

        /// <summary>
        /// Nạp dropdown tất cả học sinh (không lọc theo ParentId)
        /// </summary>
        private async Task LoadStudentsDropdown_All()
        {
            var kids = await _db.Students
                .OrderBy(s => s.FullName)
                .Select(s => new { s.Id, s.FullName })
                .ToListAsync();

            ViewBag.MyStudents = new SelectList(kids, "Id", "FullName");
        }
    }
}
