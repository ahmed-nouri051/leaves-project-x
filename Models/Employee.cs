namespace technicalTest.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public DateTime JoiningDate { get; set; }

        // Navigation property
        public ICollection<LeaveRequest> LeaveRequests { get; set; }
    }
}
