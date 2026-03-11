using CRM_Project.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class EmployeeControllerDetails : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeControllerDetails(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsEmployee() => HttpContext.Session.GetString("Role") == "Employee";

        public async Task<IActionResult> Index()
        {
            if (!IsEmployee()) return RedirectToAction("Login", "Account");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var employee = await _context.Employees
                .Include(e => e.Building)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            var assignments = await _context.ServiceAssignments
                .Include(a => a.Request)
                    .ThenInclude(r => r.Service)
                .Include(a => a.Request)
                    .ThenInclude(r => r.Customer)
                .Include(a => a.Request)
                    .ThenInclude(r => r.Building)
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync();

            ViewBag.Employee = employee;
            ViewBag.Assignments = assignments;
            return View();
        }
    }
}
