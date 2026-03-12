using CRM_Project.Data;
using CRM_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM_Project.Controllers
{
    [Authorize(Roles = "BuildingOwner")]
    public class OwnerServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- VIEW ALL SERVICE REQUESTS ----------------
        public async Task<IActionResult> ServiceRequests()
        {
            var requests = await _context.CustomerServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Service)
                .Include(r => r.Building)
                .Include(r => r.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(r => r.WorkUpdates)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // ---------------- ASSIGN SERVICE PAGE ----------------
        [HttpGet]
        public async Task<IActionResult> AssignService(int id)
        {
            var request = await _context.CustomerServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Service)
                .Include(r => r.Building)
                .Include(r => r.WorkUpdates)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
                return NotFound();

            ViewBag.Request = request;

            ViewBag.Employees = new SelectList(
                await _context.Employees
                    .Where(e => e.BuildingId == request.BuildingId)
                    .ToListAsync(),
                "EmployeeId",
                "FirstName"
            );

            return View();
        }

        // ---------------- ASSIGN EMPLOYEE TO SERVICE ----------------
        [HttpPost]
        public async Task<IActionResult> AssignService(int requestId, int employeeId, decimal? estimatedCost, string? estimatedTime)
        {
            // Get owner id from JWT token
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int ownerId = int.Parse(userIdClaim.Value);

            var request = await _context.CustomerServiceRequests.FindAsync(requestId);
            if (request == null)
                return NotFound();

            // Update request
            request.Status = "Assigned";
            request.EstimatedCost = estimatedCost;
            request.EstimatedTime = estimatedTime;

            _context.CustomerServiceRequests.Update(request);

            // Create assignment
            var assignment = new ServiceAssignment
            {
                RequestId = requestId,
                EmployeeId = employeeId,
                AssignedByOwnerId = ownerId,
                AssignedDate = DateTime.UtcNow,
                EmployeeStatus = "Pending"
            };

            _context.ServiceAssignments.Add(assignment);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee assigned successfully.";

            return RedirectToAction("ServiceRequests");
        }
    }
}