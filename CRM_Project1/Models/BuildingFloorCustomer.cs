using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Project.Models
{
    public class BuildingFloorCustomer
    {
        [Key]
        public int Id { get; set; }

        public int BuildingId { get; set; }
        [ForeignKey(nameof(BuildingId))]
        public Building Building { get; set; } = null!;

        public int FloorNumber { get; set; }

        public int CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;
    }
}
