using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Project.Models
{
    public class CustomerServiceRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        public int BuildingId { get; set; }
        [ForeignKey(nameof(BuildingId))]
        public Building Building { get; set; } = null!;

        public int FloorNumber { get; set; }

        public int ServiceId { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; } = null!;

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High

        [MaxLength(1000)]
        public string Details { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [MaxLength(30)]
        public string Status { get; set; } = "Pending";
        // Pending, Assigned, Accepted, Rejected, Travelling, WorkStarted, Delayed, Completed

        public decimal? EstimatedCost { get; set; }

        [MaxLength(100)]
        public string? EstimatedTime { get; set; }

        public int? AcceptedByEmployeeId { get; set; }

        public DateTime? CompletedDate { get; set; }

        // Navigation
        public ICollection<ServiceAssignment> Assignments { get; set; } = new List<ServiceAssignment>();
        public ICollection<ServiceWorkUpdate> WorkUpdates { get; set; } = new List<ServiceWorkUpdate>();
    }
}
