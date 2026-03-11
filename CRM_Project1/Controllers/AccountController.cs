using CRM_Project.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToDashboard(HttpContext.Session.GetString("Role")!);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            // Admin hardcoded check (kept for compatibility)
            if (role == "Admin")
            {
                if (username == "q" && password == "q")
                {
                    HttpContext.Session.SetString("Username", username);
                    HttpContext.Session.SetString("Role", "Admin");
                    HttpContext.Session.SetInt32("UserId", 0);
                    return RedirectToAction("Index", "Admin");
                }
                ViewBag.Error = "Invalid admin credentials.";
                return View();
            }

            // Primary: look up in Users table for any role created
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password && u.Role == role);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", user.Role);

                if (user.Role == "Employee")
                {
                    // Map to EmployeeId for employee-specific dashboards
                    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == username);
                    if (employee != null)
                    {
                        HttpContext.Session.SetInt32("UserId", employee.EmployeeId);
                    }
                    else
                    {
                        // Fallback to user id if employee record missing
                        HttpContext.Session.SetInt32("UserId", user.Id);
                    }
                }
                else
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                }

                return RedirectToDashboard(user.Role);
            }

            // Fallback: legacy employee records stored only in Employees table
            if (role == "Employee")
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Username == username && e.Password == password);
                if (employee != null)
                {
                    HttpContext.Session.SetString("Username", username);
                    HttpContext.Session.SetString("Role", "Employee");
                    HttpContext.Session.SetInt32("UserId", employee.EmployeeId);
                    return RedirectToAction("Index", "EmployeeControllerDetails");
                }
            }

            ViewBag.Error = "Invalid credentials or role mismatch.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToDashboard(string role) => role switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "BuildingOwner" => RedirectToAction("Index", "BuildingOwner"),
            "Employee" => RedirectToAction("Index", "EmployeeControllerDetails"),
            "Customer" => RedirectToAction("Index", "Customer"),
            _ => RedirectToAction("Login")
        };
    }
}
