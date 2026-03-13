using CRM_Project.Data;
using CRM_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    [Authorize(Roles = "BuildingOwner")]
    public class BuildingOwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuildingOwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- DASHBOARD ----------------
        public async Task<IActionResult> Index()
        {
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

        // ═══════════════════════════════════════════════════════════════════
        //  CUSTOMER CRUD
        // ═══════════════════════════════════════════════════════════════════

        // ---------------- CREATE CUSTOMER PAGE ----------------
        [HttpGet]
        public async Task<IActionResult> CreateCustomer()
        {
            ViewBag.Buildings = new SelectList(
                await _context.Buildings.ToListAsync(),
                "BuildingId",
                "BuildingName");

            return View();
        }

        // ---------------- CREATE CUSTOMER ----------------
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer, string username, string password, int buildingId, int floorNumber)
        {
            bool floorTaken = await _context.BuildingFloorCustomers
                .AnyAsync(f => f.BuildingId == buildingId && f.FloorNumber == floorNumber);

            if (floorTaken)
            {
                ViewBag.Error = "This floor is already assigned to another customer.";
                ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
                return View(customer);
            }

            // Ensure Title is never NULL
            if (string.IsNullOrEmpty(customer.Title))
                customer.Title = "Mr";

            // Create login user with hashed password
            var user = new User
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            customer.UserId = user.Id;
            customer.BuildingId = buildingId;
            customer.CreatedDate = DateTime.UtcNow;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _context.BuildingFloorCustomers.Add(new BuildingFloorCustomer
            {
                BuildingId = buildingId,
                FloorNumber = floorNumber,
                CustomerId = customer.CustomerId
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Customer registered successfully.";
            return RedirectToAction("Customers");
        }

        // ---------------- VIEW CUSTOMERS ----------------
        public async Task<IActionResult> Customers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Building)
                .Include(c => c.FloorAssignment)
                .ToListAsync();

            return View(customers);
        }

        // ---------------- CUSTOMER DETAILS ----------------
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Building)
                .Include(c => c.FloorAssignment)
                .Include(c => c.ServiceRequests)
                    .ThenInclude(r => r.Service)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null) return NotFound();
            return View(customer);
        }

        // ---------------- EDIT CUSTOMER PAGE ----------------
        [HttpGet]
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Building)
                .Include(c => c.FloorAssignment)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null) return NotFound();

            ViewBag.Buildings = new SelectList(
                await _context.Buildings.ToListAsync(),
                "BuildingId", "BuildingName",
                customer.BuildingId);

            return View(customer);
        }

        // ---------------- EDIT CUSTOMER ----------------
        [HttpPost]
        public async Task<IActionResult> EditCustomer(int customerId, string firstName, string lastName,
            string email, string phoneNumber, string title, string? newPassword)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) return NotFound();

            customer.FirstName = firstName;
            customer.LastName = lastName;
            customer.Email = email;
            customer.PhoneNumber = phoneNumber;
            customer.Title = string.IsNullOrEmpty(title) ? "Mr" : title;

            // Only update password if a new one was provided — hash it
            if (!string.IsNullOrWhiteSpace(newPassword))
                customer.User.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction("Customers");
        }

        // ---------------- DELETE CUSTOMER ----------------
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.FloorAssignment)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer != null)
            {
                if (customer.FloorAssignment != null)
                    _context.BuildingFloorCustomers.Remove(customer.FloorAssignment);

                _context.Customers.Remove(customer);

                if (customer.User != null)
                    _context.Users.Remove(customer.User);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer deleted successfully.";
            }

            return RedirectToAction("Customers");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  EMPLOYEE CRUD
        // ═══════════════════════════════════════════════════════════════════

        // ---------------- CREATE EMPLOYEE PAGE ----------------
        [HttpGet]
        public async Task<IActionResult> CreateEmployee()
        {
            ViewBag.Buildings = new SelectList(
                await _context.Buildings.ToListAsync(),
                "BuildingId",
                "BuildingName");

            return View();
        }

        // ---------------- CREATE EMPLOYEE ----------------
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Username) || string.IsNullOrWhiteSpace(employee.Password))
            {
                ViewBag.Error = "Username and password are required.";
                ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
                return View(employee);
            }

            // Check username uniqueness across both Users and Employees tables
            bool usernameTaken =
                await _context.Users.AnyAsync(u => u.Username == employee.Username) ||
                await _context.Employees.AnyAsync(e => e.Username == employee.Username);

            if (usernameTaken)
            {
                ViewBag.Error = "Username already exists.";
                ViewBag.Buildings = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
                return View(employee);
            }

            // Also create a User record so employee can log in via Users table if needed
            var user = new User
            {
                Username = employee.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(employee.Password),
                Role = "Employee"
            };
            _context.Users.Add(user);

            // Hash the password stored directly on Employee too
            employee.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);
            employee.Role = "Employee";
            employee.CreatedDate = DateTime.UtcNow;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee registered successfully.";
            return RedirectToAction("Employees");
        }

        // ---------------- VIEW EMPLOYEES ----------------
        public async Task<IActionResult> Employees()
        {
            var employees = await _context.Employees
                .Include(e => e.Building)
                .ToListAsync();

            return View(employees);
        }

        // ---------------- EMPLOYEE DETAILS ----------------
        public async Task<IActionResult> EmployeeDetails(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Building)
                .Include(e => e.Assignments)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r!.Service)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();
            return View(employee);
        }

        // ---------------- EDIT EMPLOYEE PAGE ----------------
        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Building)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();

            ViewBag.Buildings = new SelectList(
                await _context.Buildings.ToListAsync(),
                "BuildingId", "BuildingName",
                employee.BuildingId);

            return View(employee);
        }

        // ---------------- EDIT EMPLOYEE ----------------
        [HttpPost]
        public async Task<IActionResult> EditEmployee(int employeeId, string firstName, string lastName,
            string email, string phone, int buildingId, string? newPassword)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return NotFound();

            employee.FirstName = firstName;
            employee.LastName = lastName;
            employee.Email = email;
            employee.Phone = phone;
            employee.BuildingId = buildingId;

            // Only update password if a new one was provided — hash it
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                string hashed = BCrypt.Net.BCrypt.HashPassword(newPassword);
                employee.Password = hashed;

                // Also update the matching User record if it exists
                var userRecord = await _context.Users.FirstOrDefaultAsync(u => u.Username == employee.Username);
                if (userRecord != null)
                {
                    userRecord.Password = hashed;
                    _context.Users.Update(userRecord);
                }
            }

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction("Employees");
        }

        // ---------------- DELETE EMPLOYEE ----------------
        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                // Also remove the matching User record if it exists
                var userRecord = await _context.Users.FirstOrDefaultAsync(u => u.Username == employee.Username);
                if (userRecord != null)
                    _context.Users.Remove(userRecord);

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee deleted successfully.";
            }

            return RedirectToAction("Employees");
        }

        // ---------------- FLOOR API ----------------
        [HttpGet]
        public async Task<IActionResult> GetAvailableFloors(int buildingId)
        {
            var building = await _context.Buildings.FindAsync(buildingId);

            if (building == null)
                return Json(new List<int>());

            var assignedFloors = await _context.BuildingFloorCustomers
                .Where(f => f.BuildingId == buildingId)
                .Select(f => f.FloorNumber)
                .ToListAsync();

            var available = Enumerable.Range(1, building.NumberOfFloors)
                .Where(f => !assignedFloors.Contains(f))
                .ToList();

            return Json(available);
        }
    }
}
