using CRM_Project.Data;
using CRM_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class BuildingOwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuildingOwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsOwner() => HttpContext.Session.GetString("Role") == "BuildingOwner";

        public async Task<IActionResult> Index()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");

            var customers = await _context.Customers
                .Include(c => c.Building)
                .Include(c => c.FloorAssignment)
                .ToListAsync();

            var employees = await _context.Employees
                .Include(e => e.Building)
                .ToListAsync();

            var requests = await _context.CustomerServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Service)
                .Include(r => r.Building)
                .Include(r => r.WorkUpdates)
                .OrderByDescending(r => r.RequestDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Customers = customers;
            ViewBag.Employees = employees;
            ViewBag.RecentRequests = requests;
            return View();
        }

        // ─── Create Customer ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CreateCustomer()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer, string username, string password, int buildingId, int floorNumber)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");

            // Check floor availability
            bool floorTaken = await _context.BuildingFloorCustomers
                .AnyAsync(f => f.BuildingId == buildingId && f.FloorNumber == floorNumber);
            if (floorTaken)
            {
                ViewBag.Error = "This floor is already assigned to another customer.";
                ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
                return View(customer);
            }

            // Create user account
            var user = new User { Username = username, Password = password, Role = "Customer" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create customer
            customer.UserId = user.Id;
            customer.BuildingId = buildingId;
            customer.CreatedDate = DateTime.UtcNow;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Assign floor
            _context.BuildingFloorCustomers.Add(new BuildingFloorCustomer
            {
                BuildingId = buildingId,
                FloorNumber = floorNumber,
                CustomerId = customer.CustomerId
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Customer registered successfully.";
            return RedirectToAction("Index");
        }

        // ─── Create Employee ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CreateEmployee()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");

            // Validate credentials
            if (string.IsNullOrWhiteSpace(employee.Username) || string.IsNullOrWhiteSpace(employee.Password))
            {
                ViewBag.Error = "Username and password are required.";
                ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
                return View(employee);
            }

            // Check for existing username in Users or Employees
            bool usernameTaken = await _context.Users.AnyAsync(u => u.Username == employee.Username)
                || await _context.Employees.AnyAsync(e => e.Username == employee.Username);
            if (usernameTaken)
            {
                ViewBag.Error = "Username already exists.";
                ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
                return View(employee);
            }

            // Create corresponding User record
            var user = new User
            {
                Username = employee.Username,
                Password = employee.Password,
                Role = "Employee"
            };
            _context.Users.Add(user);

            // Create employee record
            employee.Role = "Employee";
            employee.CreatedDate = DateTime.UtcNow;
            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Employee registered successfully.";
            return RedirectToAction("Index");
        }

        // ─── Floor API ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAvailableFloors(int buildingId)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building == null) return Json(new List<int>());

            var assignedFloors = await _context.BuildingFloorCustomers
                .Where(f => f.BuildingId == buildingId)
                .Select(f => f.FloorNumber)
                .ToListAsync();

            var available = Enumerable.Range(1, building.NumberOfFloors)
                .Where(f => !assignedFloors.Contains(f))
                .ToList();

            return Json(available);
        }

        // ─── View Customers ────────────────────────────────────────────────
        public async Task<IActionResult> Customers()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var customers = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Building)
                .Include(c => c.FloorAssignment)
                .ToListAsync();
            return View(customers);
        }

        // ─── View Employees ────────────────────────────────────────────────
        public async Task<IActionResult> Employees()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var employees = await _context.Employees.Include(e => e.Building).ToListAsync();
            return View(employees);
        }
    }
}
