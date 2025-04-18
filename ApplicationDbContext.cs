// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using technicalTest.Models;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data
        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 1, FullName = "John Doe", Department = "IT", JoiningDate = new DateTime(2022, 1, 15) },
            new Employee { Id = 2, FullName = "Jane Smith", Department = "HR", JoiningDate = new DateTime(2021, 6, 10) }
        );

        modelBuilder.Entity<LeaveRequest>().HasData(
            new LeaveRequest
            {
                Id = 1,
                EmployeeId = 1,
                LeaveType = LeaveType.Annual,
                StartDate = new DateTime(2025, 4, 20), // Static value
                EndDate = new DateTime(2025, 4, 25), // Static value
                Status = RequestStatus.Pending,
                Reason = "Vacation",
                CreatedAt = new DateTime(2025, 4, 15)
            }
        );
    }
}