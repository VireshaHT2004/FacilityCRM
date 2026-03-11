using CRM_Project.Models;

namespace CRM_Project.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Seed Services if none exist
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { ServiceNumber = "MAC-001", ServiceName = "MAC Facilities Add - Chair", Description = "Add chair to facility", IsActive = true },
                    new Service { ServiceNumber = "MAC-002", ServiceName = "MAC Facilities Add - Light", Description = "Add light fixture to facility", IsActive = true },
                    new Service { ServiceNumber = "MOV-001", ServiceName = "Move - Chair Move Only", Description = "Move chair within facility", IsActive = true },
                    new Service { ServiceNumber = "PLM-001", ServiceName = "Plumbing", Description = "Plumbing repair and maintenance", IsActive = true },
                    new Service { ServiceNumber = "MNT-001", ServiceName = "Maintenance", Description = "General maintenance services", IsActive = true },
                    new Service { ServiceNumber = "SEC-001", ServiceName = "Security Access Card", Description = "Security access card issuance or replacement", IsActive = true }
                );
                context.SaveChanges();
            }

            // Seed a sample building and building owner user
            if (!context.Buildings.Any())
            {
                context.Buildings.Add(new Building
                {
                    BuildingName = "HQ Tower",
                    BuildingType = "Commercial",
                    SquareFootage = 50000,
                    StreetAddress = "123 Main Street",
                    City = "New York",
                    State = "NY",
                    Country = "USA",
                    PostalCode = "10001",
                    NumberOfFloors = 10,
                    OwnerFirstName = "John",
                    OwnerLastName = "Smith",
                    OwnerEmail = "john.smith@hqtower.com",
                    OwnerPhoneNumber = "+1-212-555-0100",
                    CreatedDate = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            // Seed Building Owner user
            if (!context.Users.Any(u => u.Role == "BuildingOwner"))
            {
                context.Users.Add(new User
                {
                    Username = "owner1",
                    Password = "owner1",
                    Role = "BuildingOwner"
                });
                context.SaveChanges();
            }
        }
    }
}
