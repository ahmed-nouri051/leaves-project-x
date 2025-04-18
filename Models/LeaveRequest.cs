namespace technicalTest.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Employee Employee { get; set; }
    }
}
