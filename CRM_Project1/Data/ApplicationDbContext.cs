using CRM_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM_Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<BuildingFloorCustomer> BuildingFloorCustomers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<CustomerServiceRequest> CustomerServiceRequests { get; set; }
        public DbSet<ServiceAssignment> ServiceAssignments { get; set; }
        public DbSet<ServiceWorkUpdate> ServiceWorkUpdates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Customer → User (one-to-one)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer → Building
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Building)
                .WithMany(b => b.Customers)
                .HasForeignKey(c => c.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            // BuildingFloorCustomer → unique floor per building
            modelBuilder.Entity<BuildingFloorCustomer>()
                .HasIndex(x => new { x.BuildingId, x.FloorNumber })
                .IsUnique();

            modelBuilder.Entity<BuildingFloorCustomer>()
                .HasOne(x => x.Building)
                .WithMany(b => b.FloorAssignments)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BuildingFloorCustomer>()
                .HasOne(x => x.Customer)
                .WithOne(c => c.FloorAssignment)
                .HasForeignKey<BuildingFloorCustomer>(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employee → Building
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Building)
                .WithMany(b => b.Employees)
                .HasForeignKey(e => e.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest → Customer
            modelBuilder.Entity<CustomerServiceRequest>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest → Building
            modelBuilder.Entity<CustomerServiceRequest>()
                .HasOne(r => r.Building)
                .WithMany(b => b.ServiceRequests)
                .HasForeignKey(r => r.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceRequest → Service
            modelBuilder.Entity<CustomerServiceRequest>()
                .HasOne(r => r.Service)
                .WithMany(s => s.ServiceRequests)
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceAssignment → Request
            modelBuilder.Entity<ServiceAssignment>()
                .HasOne(a => a.Request)
                .WithMany(r => r.Assignments)
                .HasForeignKey(a => a.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceAssignment → Employee
            modelBuilder.Entity<ServiceAssignment>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Assignments)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceWorkUpdate → Request
            modelBuilder.Entity<ServiceWorkUpdate>()
                .HasOne(w => w.Request)
                .WithMany(r => r.WorkUpdates)
                .HasForeignKey(w => w.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // ServiceWorkUpdate → Employee
            modelBuilder.Entity<ServiceWorkUpdate>()
                .HasOne(w => w.Employee)
                .WithMany()
                .HasForeignKey(w => w.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision
            modelBuilder.Entity<CustomerServiceRequest>()
                .Property(r => r.EstimatedCost)
                .HasPrecision(18, 2);
        }
    }
}
