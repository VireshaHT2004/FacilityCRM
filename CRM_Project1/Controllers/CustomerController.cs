using CRM_Project.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsCustomer() => HttpContext.Session.GetString("Role") == "Customer";

        public async Task<IActionResult> Index()
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var customer = await _context.Customers
                .Include(c => c.Building)
                .Include(c => c.FloorAssignment)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null) return RedirectToAction("Login", "Account");

            var requests = await _context.CustomerServiceRequests
                .Include(r => r.Service)
                .Include(r => r.WorkUpdates)
                .Where(r => r.CustomerId == customer.CustomerId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewBag.Customer = customer;
            ViewBag.Requests = requests;
            return View();
        }
    }
}
