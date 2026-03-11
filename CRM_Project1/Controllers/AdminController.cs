using CRM_Project.Data;
using CRM_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public async Task<IActionResult> Index(string? search)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _context.Buildings.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.BuildingName.Contains(search) || b.City.Contains(search));

            ViewBag.Search = search;
            return View(await query.OrderByDescending(b => b.CreatedDate).ToListAsync());
        }

        [HttpGet]
        public IActionResult CreateBuilding()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(new Building());
        }

        [HttpPost]
        public async Task<IActionResult> CreateBuilding(Building building, string ownerUsername, string ownerPassword)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            building.CreatedDate = DateTime.UtcNow;
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            // Create building owner user
            if (!string.IsNullOrWhiteSpace(ownerUsername))
            {
                // Check if username already exists
                bool exists = await _context.Users.AnyAsync(u => u.Username == ownerUsername)
                    || await _context.Employees.AnyAsync(e => e.Username == ownerUsername);
                if (!exists)
                {
                    var user = new User
                    {
                        Username = ownerUsername,
                        Password = ownerPassword,
                        Role = "BuildingOwner"
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Building created successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditBuilding(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return NotFound();
            return View(building);
        }

        [HttpPost]
        public async Task<IActionResult> EditBuilding(Building building)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Building updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var building = await _context.Buildings.FindAsync(id);
            if (building != null)
            {
                _context.Buildings.Remove(building);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Building deleted.";
            }
            return RedirectToAction("Index");
        }
    }
}
