using CRM_Project.Data;
using CRM_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsCustomer() => HttpContext.Session.GetString("Role") == "Customer";

        [HttpGet]
        public async Task<IActionResult> CreateServiceRequest()
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");

            ViewBag.Services = new SelectList(
                await _context.Services.Where(s => s.IsActive).ToListAsync(),
                "ServiceId", "ServiceName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequest(int serviceId, string priority, string details)
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var customer = await _context.Customers
                .Include(c => c.FloorAssignment)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                TempData["Error"] = "Customer record not found.";
                return RedirectToAction("Index", "Customer");
            }

            var request = new CustomerServiceRequest
            {
                CustomerId = customer.CustomerId,
                BuildingId = customer.BuildingId,
                FloorNumber = customer.FloorAssignment?.FloorNumber ?? 0,
                ServiceId = serviceId,
                Priority = priority,
                Details = details ?? string.Empty,
                RequestDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.CustomerServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service request submitted successfully.";
            return RedirectToAction("MyServiceRequests");
        }

        public async Task<IActionResult> MyServiceRequests()
        {
            if (!IsCustomer()) return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return RedirectToAction("Login", "Account");

            var requests = await _context.CustomerServiceRequests
                .Include(r => r.Service)
                .Include(r => r.Building)
                .Where(r => r.CustomerId == customer.CustomerId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }
    }
}
