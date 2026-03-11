using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Project.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public int BuildingId { get; set; }
        [ForeignKey(nameof(BuildingId))]
        public Building Building { get; set; } = null!;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public BuildingFloorCustomer? FloorAssignment { get; set; }
        public ICollection<CustomerServiceRequest> ServiceRequests { get; set; } = new List<CustomerServiceRequest>();
    }
}
