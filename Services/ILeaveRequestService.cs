using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using technicalTest.DTO;
using technicalTest.Models;

namespace technicalTest.Services
{
    public interface ILeaveRequestService
    {
        Task<PagedResponseDto<LeaveRequestDto>> FilterLeaveRequestsAsync(LeaveRequestFilterDto filter);
        Task<LeaveRequest> CreateLeaveRequestAsync(CreateLeaveRequestDto createLeaveRequestDto);
        Task<LeaveRequest> UpdateLeaveRequestAsync(int id, UpdateLeaveRequestDto updateLeaveRequestDto);
         Task<LeaveRequest> DeleteLeaveRequest(int id);
        Task<IEnumerable<LeaveReportDto>> GetLeaveReportAsync(LeaveReportFilterDto filter);
        Task<LeaveRequestDto> ApproveLeaveRequestAsync(int id);

    }
}
