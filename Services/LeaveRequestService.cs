using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using technicalTest.DTO;
using technicalTest.Models;

namespace technicalTest.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private const int MAX_ANNUAL_LEAVE_DAYS = 20;

        public LeaveRequestService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResponseDto<LeaveRequestDto>> FilterLeaveRequestsAsync(LeaveRequestFilterDto filter)
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .AsQueryable();

           
            if (filter.EmployeeId.HasValue)
                query = query.Where(l => l.EmployeeId == filter.EmployeeId.Value);

            if (filter.LeaveType.HasValue)
                query = query.Where(l => l.LeaveType == filter.LeaveType.Value);

            if (filter.Status.HasValue)
                query = query.Where(l => l.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(l => l.StartDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(l => l.EndDate <= filter.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(l => l.Reason.Contains(filter.Keyword));

           
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                var sortDirection = string.IsNullOrWhiteSpace(filter.SortOrder) ||
                                    filter.SortOrder.ToLower() == "asc" ? "ascending" : "descending";

                
                if (ValidateSortField(filter.SortBy))
                {
                    query = query.OrderBy($"{filter.SortBy} {sortDirection}");
                }
            }
            else
            {
                query = query.OrderByDescending(l => l.CreatedAt);
            }

            
            var totalCount = await query.CountAsync();

           
            var leaveRequests = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var leaveRequestDtos = _mapper.Map<List<LeaveRequestDto>>(leaveRequests);

            return new PagedResponseDto<LeaveRequestDto>
            {
                Data = leaveRequestDtos,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<LeaveRequest> CreateLeaveRequestAsync(CreateLeaveRequestDto createLeaveRequestDto)
        {
            await ValidateLeaveRequestCreationAsync(createLeaveRequestDto);

            var leaveRequest = _mapper.Map<LeaveRequest>(createLeaveRequestDto);
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return leaveRequest;
        }

        public async Task<LeaveRequest> UpdateLeaveRequestAsync(int id, UpdateLeaveRequestDto updateLeaveRequestDto)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                throw new KeyNotFoundException($"Leave request with ID {id} not found.");
            }

            await ValidateLeaveRequestUpdateAsync(leaveRequest, updateLeaveRequestDto);

            _mapper.Map(updateLeaveRequestDto, leaveRequest);
            await _context.SaveChangesAsync();

            return leaveRequest;
        }

        private bool ValidateSortField(string sortField)
        {
            
            var validFields = new HashSet<string>
        {
            "Id", "EmployeeId", "LeaveType", "StartDate", "EndDate",
            "Status", "Reason", "CreatedAt"
        };
            return validFields.Contains(sortField);
        }

        private async Task ValidateLeaveRequestCreationAsync(CreateLeaveRequestDto dto)
        {
            
            var overlappingRequests = await _context.LeaveRequests
                .AnyAsync(lr => lr.EmployeeId == dto.EmployeeId &&
                               lr.Status != RequestStatus.Rejected &&
                               ((lr.StartDate <= dto.EndDate && lr.EndDate >= dto.StartDate)));

            if (overlappingRequests)
            {
                throw new InvalidOperationException("Employee already has approved or pending leave during these dates.");
            }

            
            if (dto.LeaveType == LeaveType.Annual)
            {
                var currentYear = DateTime.Now.Year;
                var totalAnnualDays = await _context.LeaveRequests
                    .Where(lr => lr.EmployeeId == dto.EmployeeId &&
                                lr.LeaveType == LeaveType.Annual &&
                                lr.Status != RequestStatus.Rejected &&
                                lr.StartDate.Year == currentYear)
                    .SumAsync(lr => (lr.EndDate - lr.StartDate).Days + 1);

                var requestedDays = (dto.EndDate - dto.StartDate).Days + 1;

                if (totalAnnualDays + requestedDays > MAX_ANNUAL_LEAVE_DAYS)
                {
                    throw new InvalidOperationException($"Annual leave exceeds maximum of {MAX_ANNUAL_LEAVE_DAYS} days per year.");
                }
            }

            
            if (dto.LeaveType == LeaveType.Sick && string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new InvalidOperationException("Sick leave requires a non-empty reason.");
            }
        }

        private async Task ValidateLeaveRequestUpdateAsync(LeaveRequest originalLeaveRequest, UpdateLeaveRequestDto dto)
        {
            
            if (dto.StartDate > dto.EndDate)
            {
                throw new InvalidOperationException("End date must be after start date.");
            }

            
            var overlappingRequests = await _context.LeaveRequests
                .AnyAsync(lr => lr.Id != originalLeaveRequest.Id &&
                                lr.EmployeeId == originalLeaveRequest.EmployeeId &&
                                lr.Status != RequestStatus.Rejected &&
                                (lr.StartDate <= dto.EndDate && lr.EndDate >= dto.StartDate));

            if (overlappingRequests)
            {
                throw new InvalidOperationException("Employee already has approved/pending leave during these dates.");
            }

            
            if (dto.LeaveType == LeaveType.Annual || originalLeaveRequest.LeaveType == LeaveType.Annual)
            {
               
                int originalDays = originalLeaveRequest.LeaveType == LeaveType.Annual
                    ? (originalLeaveRequest.EndDate - originalLeaveRequest.StartDate).Days + 1
                    : 0;

                
                int newDays = dto.LeaveType == LeaveType.Annual
                    ? (dto.EndDate - dto.StartDate).Days + 1
                    : 0;

               
                var currentYear = DateTime.Now.Year;
                var totalAnnualDays = await _context.LeaveRequests
                    .Where(lr => lr.EmployeeId == originalLeaveRequest.EmployeeId &&
                                lr.LeaveType == LeaveType.Annual &&
                                lr.Status != RequestStatus.Rejected &&
                                lr.Id != originalLeaveRequest.Id &&
                                lr.StartDate.Year == currentYear)
                    .SumAsync(lr => (lr.EndDate - lr.StartDate).Days + 1);

                
                totalAnnualDays = totalAnnualDays - originalDays + newDays;

                if (totalAnnualDays > MAX_ANNUAL_LEAVE_DAYS)
                {
                    throw new InvalidOperationException(
                        $"Annual leave exceeds maximum of {MAX_ANNUAL_LEAVE_DAYS} days per year.");
                }
            }

            
            if (dto.LeaveType == LeaveType.Sick && string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new InvalidOperationException("Sick leave requires a non-empty reason.");
            }
        }

        public async Task<LeaveRequest> DeleteLeaveRequest(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                throw new KeyNotFoundException($"Leave request with ID {id} not found.");
            }

            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();

            return leaveRequest;
        }

        public async Task<IEnumerable<LeaveReportDto>> GetLeaveReportAsync(LeaveReportFilterDto filter)
        {
            
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .Where(l => l.Status == RequestStatus.Approved)
                .AsQueryable();

            
            if (filter.Year.HasValue)
            {
                query = query.Where(l => l.StartDate.Year == filter.Year.Value);
            }

            
            if (filter.StartDate.HasValue)
            {
                query = query.Where(l => l.StartDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(l => l.EndDate <= filter.EndDate.Value);
            }

            
            if (!string.IsNullOrWhiteSpace(filter.Department))
            {
                query = query.Where(l => l.Employee.Department == filter.Department);
            }

           
            var report = await query
                .GroupBy(l => new { l.EmployeeId, l.Employee.FullName, l.Employee.Department })
                .Select(g => new LeaveReportDto
                {
                    EmployeeName = g.Key.FullName,
                    Department = g.Key.Department,
                    TotalLeaves = g.Sum(l => (l.EndDate - l.StartDate).Days + 1),
                    AnnualLeaves = g.Where(l => l.LeaveType == LeaveType.Annual)
                        .Sum(l => (l.EndDate - l.StartDate).Days + 1),
                    SickLeaves = g.Where(l => l.LeaveType == LeaveType.Sick)
                        .Sum(l => (l.EndDate - l.StartDate).Days + 1),
                    OtherLeaves = g.Where(l => l.LeaveType == LeaveType.Other)
                        .Sum(l => (l.EndDate - l.StartDate).Days + 1)
                })
                .ToListAsync();

            return report;
        }

        public async Task<LeaveRequestDto> ApproveLeaveRequestAsync(int id)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaveRequest == null)
            {
                throw new KeyNotFoundException($"Leave request with ID {id} not found.");
            }

            if (leaveRequest.Status != RequestStatus.Pending)
            {
                throw new InvalidOperationException("Only pending leave requests can be approved.");
            }

            leaveRequest.Status = RequestStatus.Approved;
            await _context.SaveChangesAsync();

            return _mapper.Map<LeaveRequestDto>(leaveRequest);
        }
    }
}
