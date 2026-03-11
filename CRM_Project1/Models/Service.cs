using System.ComponentModel.DataAnnotations;

namespace CRM_Project.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [MaxLength(50)]
        public string ServiceNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<CustomerServiceRequest> ServiceRequests { get; set; } = new List<CustomerServiceRequest>();
    }
}
