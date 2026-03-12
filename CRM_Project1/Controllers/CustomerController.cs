using CRM_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM_Project.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get UserId from JWT token
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            var customer = await _context.Customers
                .Include(c => c.Building)
                .Include(c => c.FloorAssignment)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return RedirectToAction("Login", "Account");

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