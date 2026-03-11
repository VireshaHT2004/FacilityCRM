using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Project.Models
{
    public class ServiceWorkUpdate
    {
        [Key]
        public int UpdateId { get; set; }

        public int RequestId { get; set; }
        [ForeignKey(nameof(RequestId))]
        public CustomerServiceRequest Request { get; set; } = null!;

        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = null!;

        [MaxLength(30)]
        public string WorkStatus { get; set; } = string.Empty;
        // Travelling, WorkStarted, Delayed, Paused, Completed

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }
}
