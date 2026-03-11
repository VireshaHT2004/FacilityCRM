using CRM_Project.Data;
using CRM_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Controllers
{
    public class EmployeeServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsEmployee() => HttpContext.Session.GetString("Role") == "Employee";

        public async Task<IActionResult> AssignedServices()
        {
            if (!IsEmployee()) return RedirectToAction("Login", "Account");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var assignments = await _context.ServiceAssignments
                .Include(a => a.Request)
                    .ThenInclude(r => r.Service)
                .Include(a => a.Request)
                    .ThenInclude(r => r.Customer)
                .Include(a => a.Request)
                    .ThenInclude(r => r.Building)
                .Include(a => a.Request)
                    .ThenInclude(r => r.WorkUpdates)
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync();

            return View(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> RespondToAssignment(int assignmentId, string response)
        {
            if (!IsEmployee()) return RedirectToAction("Login", "Account");

            var assignment = await _context.ServiceAssignments
                .Include(a => a.Request)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null) return NotFound();

            // Normalize response
            var normalized = response?.Trim();
            if (string.IsNullOrEmpty(normalized)) return BadRequest();

            assignment.EmployeeStatus = normalized; // Accepted or Rejected
            assignment.Request.Status = normalized == "Accepted" ? "Accepted" : "Rejected";
            assignment.Request.AcceptedByEmployeeId = normalized == "Accepted" ? assignment.EmployeeId : null;

            // Log initial work update for visibility to customer and owner
            _context.ServiceWorkUpdates.Add(new ServiceWorkUpdate
            {
                RequestId = assignment.Request.RequestId,
                EmployeeId = assignment.EmployeeId,
                WorkStatus = normalized == "Accepted" ? "Accepted" : "Rejected",
                UpdateTime = DateTime.UtcNow,
                Notes = normalized == "Accepted" ? "Employee accepted the assignment." : "Employee rejected the assignment."
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Assignment {normalized}.";
            return RedirectToAction("AssignedServices");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateServiceStatus(int requestId)
        {
            if (!IsEmployee()) return RedirectToAction("Login", "Account");

            var request = await _context.CustomerServiceRequests
                .Include(r => r.Service)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null) return NotFound();

            var workUpdates = await _context.ServiceWorkUpdates
                .Where(w => w.RequestId == requestId)
                .OrderByDescending(w => w.UpdateTime)
                .ToListAsync();

            ViewBag.Request = request;
            ViewBag.WorkUpdates = workUpdates;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServiceStatus(int requestId, string workStatus, string notes)
        {
            if (!IsEmployee()) return RedirectToAction("Login", "Account");

            int employeeId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var request = await _context.CustomerServiceRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            // Validate allowed statuses
            var allowed = new[] { "Travelling", "WorkStarted", "Delayed", "Paused", "Completed" };
            if (!allowed.Contains(workStatus))
            {
                TempData["Error"] = "Invalid work status.";
                return RedirectToAction("UpdateServiceStatus", new { requestId });
            }

            // Update request status
            request.Status = workStatus;
            if (workStatus == "Completed")
                request.CompletedDate = DateTime.UtcNow;

            _context.CustomerServiceRequests.Update(request);

            // Log work update (visible to owner and customer)
            _context.ServiceWorkUpdates.Add(new ServiceWorkUpdate
            {
                RequestId = requestId,
                EmployeeId = employeeId,
                WorkStatus = workStatus,
                UpdateTime = DateTime.UtcNow,
                Notes = notes
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Status updated successfully.";
            return RedirectToAction("AssignedServices");
        }
    }
}
