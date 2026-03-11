using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Project.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "Employee";

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;

        public int BuildingId { get; set; }
        [ForeignKey(nameof(BuildingId))]
        public Building Building { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ServiceAssignment> Assignments { get; set; } = new List<ServiceAssignment>();
    }
}
