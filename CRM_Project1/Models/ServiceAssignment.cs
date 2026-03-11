using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Project.Models
{
    public class ServiceAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        public int RequestId { get; set; }
        [ForeignKey(nameof(RequestId))]
        public CustomerServiceRequest Request { get; set; } = null!;

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = null!;

        public int AssignedByOwnerId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string EmployeeStatus { get; set; } = "Pending"; // Pending, Accepted, Rejected
    }
}
