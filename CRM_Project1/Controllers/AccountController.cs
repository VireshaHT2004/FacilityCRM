using CRM_Project.Data;
using CRM_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AccountController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // ---------------- LOGIN PAGE ----------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ---------------- LOGIN LOGIC ----------------
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            // ---------------- ADMIN LOGIN ----------------
            if (role == "Admin")
            {
                if (username == "q" && password == "q")
                {
                    var adminUser = new Models.User
                    {
                        Id = 0,
                        Username = "q",
                        Role = "Admin"
                    };

                    var token = _jwtService.GenerateToken(adminUser);

                    Response.Cookies.Append("jwt", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Strict
                    });

                    return RedirectToAction("Index", "Admin");
                }

                ViewBag.Error = "Invalid admin credentials.";
                return View();
            }

            // ---------------- DATABASE USER LOGIN ----------------
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Role == role);

            if (user != null)
            {
                // Verify hashed password
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    var token = _jwtService.GenerateToken(user);

                    Response.Cookies.Append("jwt", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Strict
                    });

                    return RedirectToDashboard(user.Role);
                }
            }

            // ---------------- FALLBACK EMPLOYEE LOGIN ----------------
            if (role == "Employee")
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Username == username);

                if (employee != null && BCrypt.Net.BCrypt.Verify(password, employee.Password))
                {
                    var empUser = new Models.User
                    {
                        Id = employee.EmployeeId,
                        Username = employee.Username,
                        Role = "Employee"
                    };

                    var token = _jwtService.GenerateToken(empUser);

                    Response.Cookies.Append("jwt", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Strict
                    });

                    return RedirectToAction("Index", "EmployeeControllerDetails");
                }
            }

            ViewBag.Error = "Invalid credentials or role mismatch.";
            return View();
        }

        // ---------------- LOGOUT ----------------
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }

        // ---------------- DASHBOARD REDIRECTION ----------------
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