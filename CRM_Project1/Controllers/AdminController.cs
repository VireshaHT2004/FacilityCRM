using CRM_Project.Data;
using CRM_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- DASHBOARD ----------------
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Buildings.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.BuildingName.Contains(search) ||
                    b.City.Contains(search));
            }

            ViewBag.Search = search;

            return View(await query
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync());
        }

        // ---------------- CREATE BUILDING ----------------
        [HttpGet]
        public IActionResult CreateBuilding()
        {
            return View(new Building());
        }

        [HttpPost]
        public async Task<IActionResult> CreateBuilding(Building building, string ownerUsername, string ownerPassword)
        {
            if (!ModelState.IsValid)
                return View(building);

            building.CreatedDate = DateTime.UtcNow;

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            // Create building owner login
            if (!string.IsNullOrWhiteSpace(ownerUsername))
            {
                bool exists =
                    await _context.Users.AnyAsync(u => u.Username == ownerUsername) ||
                    await _context.Employees.AnyAsync(e => e.Username == ownerUsername);

                if (!exists)
                {
                    var user = new User
                    {
                        Username = ownerUsername,
                        Password = BCrypt.Net.BCrypt.HashPassword(ownerPassword),
                        Role = "BuildingOwner"
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Building created successfully.";

            return RedirectToAction("Index");
        }

        // ---------------- EDIT BUILDING ----------------
        [HttpGet]
        public async Task<IActionResult> EditBuilding(int id)
        {
            var building = await _context.Buildings.FindAsync(id);

            if (building == null)
                return NotFound();

            return View(building);
        }

        [HttpPost]
        public async Task<IActionResult> EditBuilding(Building building)
        {
            if (!ModelState.IsValid)
                return View(building);

            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Building updated successfully.";

            return RedirectToAction("Index");
        }

        // ---------------- DELETE BUILDING ----------------
        [HttpPost]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Buildings
                .FirstOrDefaultAsync(b => b.BuildingId == id);

            if (building == null)
                return RedirectToAction("Index");

            // Remove service requests
            var requests = await _context.CustomerServiceRequests
                .Where(r => r.BuildingId == id)
                .ToListAsync();

            if (requests.Any())
                _context.CustomerServiceRequests.RemoveRange(requests);

            // Remove floor assignments
            var floors = await _context.BuildingFloorCustomers
                .Where(f => f.BuildingId == id)
                .ToListAsync();

            if (floors.Any())
                _context.BuildingFloorCustomers.RemoveRange(floors);

            // Remove customers
            var customers = await _context.Customers
                .Where(c => c.BuildingId == id)
                .ToListAsync();

            if (customers.Any())
                _context.Customers.RemoveRange(customers);

            // Remove employees
            var employees = await _context.Employees
                .Where(e => e.BuildingId == id)
                .ToListAsync();

            if (employees.Any())
                _context.Employees.RemoveRange(employees);

            // Finally remove building
            _context.Buildings.Remove(building);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Building and related records deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}