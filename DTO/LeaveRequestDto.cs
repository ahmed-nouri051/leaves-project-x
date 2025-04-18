using technicalTest.Models;

namespace technicalTest.DTO
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLeaveRequestDto
    {
        public int EmployeeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
    }

    public class UpdateLeaveRequestDto
    {
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
    }



    public class LeaveRequestFilterDto
    {
        public int? EmployeeId { get; set; }
        public LeaveType? LeaveType { get; set; }
        public RequestStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }

    public class PagedResponseDto<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
    
    public class LeaveReportDto
    {
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public int TotalLeaves { get; set; }
        public int AnnualLeaves { get; set; }
        public int SickLeaves { get; set; }
        public int OtherLeaves { get; set; }
    }

    
    public class LeaveReportFilterDto
    {
        public int? Year { get; set; }
        public string? Department { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
