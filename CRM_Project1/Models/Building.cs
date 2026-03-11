using System.ComponentModel.DataAnnotations;

namespace CRM_Project.Models
{
    public class Building
    {
        [Key]
        public int BuildingId { get; set; }

        [Required, MaxLength(200)]
        public string BuildingName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string BuildingType { get; set; } = string.Empty;

        public double SquareFootage { get; set; }

        [MaxLength(300)]
        public string StreetAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        public int NumberOfFloors { get; set; }

        [MaxLength(100)]
        public string OwnerFirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string OwnerLastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string OwnerEmail { get; set; } = string.Empty;

        [MaxLength(30)]
        public string OwnerPhoneNumber { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<BuildingFloorCustomer> FloorAssignments { get; set; } = new List<BuildingFloorCustomer>();
        public ICollection<CustomerServiceRequest> ServiceRequests { get; set; } = new List<CustomerServiceRequest>();
    }
}
