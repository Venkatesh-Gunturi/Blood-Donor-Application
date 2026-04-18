using BloodDonorApplication.Data;
using BloodDonorApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonorApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Show form
        public IActionResult Register()
        {
            return View(new User());
        }

        // POST: Handle form submission
        [HttpPost]
        public IActionResult Register(User user)
        {
            // ✅ 1. Validate first (avoid unnecessary DB calls)
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // ✅ 2. Normalize input (important for consistency)
            user.Email = user.Email?.Trim().ToLower();
            user.PhoneNumber = user.PhoneNumber?.Trim();

            // ✅ 3. Check duplicates separately (better control)
            bool emailExists = _context.Users.Any(u => u.Email == user.Email);
            bool phoneExists = _context.Users.Any(u => u.PhoneNumber == user.PhoneNumber);

            if (emailExists || phoneExists)
            {
                ViewBag.DuplicateMessage = "These details are already registered. Do you want to login?";
                return View(user);
            }

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "User Registered Successfully";
                return RedirectToAction("List");
            }
            catch (DbUpdateException)
            {
                // ✅ 4. DB-level safety net
                ModelState.AddModelError("", "Duplicate Email or Phone not allowed.");
            }

            return View(user);
        }

        // GET: List + Search
        public IActionResult List(string searchBloodGroup, string? searchLocation)
        {
            var users = _context.Users.AsQueryable();

            // ✅ 5. Trim input
            searchLocation = searchLocation?.Trim();

            // ✅ 6. Filter by blood group
            if (!string.IsNullOrEmpty(searchBloodGroup))
            {
                users = users.Where(u => u.BloodGroup == searchBloodGroup);
            }

            // ✅ 7. Case-insensitive search (real-world behavior)
            if (!string.IsNullOrEmpty(searchLocation))
            {
                users = users.Where(u =>
                    u.Location != null &&
                    u.Location.ToLower().Contains(searchLocation.ToLower()));
            }

            // ✅ 8. Show latest first (better UX)
            return View(users.OrderByDescending(u => u.Id).ToList());
        }


        [HttpGet]
        public IActionResult Search(string? searchBloodGroup, string? searchLocation)
        {
            var users = _context.Users.AsQueryable();

            searchLocation = searchLocation?.Trim();

            if (!string.IsNullOrEmpty(searchBloodGroup))
            {
                users = users.Where(u => u.BloodGroup == searchBloodGroup);
            }

            if (!string.IsNullOrWhiteSpace(searchLocation))
            {
                users = users.Where(u =>
                    u.Location != null &&
                    u.Location.ToLower().Contains(searchLocation.ToLower()));
            }

            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var result = users
                .Select(u => new
                {
                    u.Name,
                    u.Email,
                    u.PhoneNumber,
                    u.BloodGroup,
                    u.Location,
                    IsAvailable = (u.LastDonationDate == null || u.LastDonationDate < sixMonthsAgo)
                })
                .OrderByDescending(u => u.IsAvailable) // 🔥 KEY LINE
                .ToList();

            return Json(result);
        }
    }
}