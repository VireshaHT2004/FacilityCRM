using CRM_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeControllerDetails : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeControllerDetails(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get logged in username
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            // Find employee using username
            var employee = await _context.Employees
                .Include(e => e.Building)
                .FirstOrDefaultAsync(e => e.Username == username);

            if (employee == null)
                return Unauthorized();

            int employeeId = employee.EmployeeId;

            var assignments = await _context.ServiceAssignments
             .Include(a => a.Request)
                 .ThenInclude(r => r.Service)
             .Include(a => a.Request)
                 .ThenInclude(r => r.Customer)
             .Include(a => a.Request)
                 .ThenInclude(r => r.Building)
             .Include(a => a.Request)
                 .ThenInclude(r => r.WorkUpdates)   // important
             .Where(a => a.EmployeeId == employee.EmployeeId)
             .OrderByDescending(a => a.AssignedDate)
             .ToListAsync();

            ViewBag.Employee = employee;
            ViewBag.Assignments = assignments;

            return View();
        }
    }
}