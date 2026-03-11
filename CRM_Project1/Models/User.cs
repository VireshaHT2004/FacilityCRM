using System.ComponentModel.DataAnnotations;

namespace CRM_Project.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Role { get; set; } = string.Empty; // Admin, BuildingOwner, Employee, Customer

        // Navigation
        public Customer? Customer { get; set; }
    }
}
