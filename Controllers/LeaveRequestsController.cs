using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using technicalTest.DTO;
using technicalTest.Models;
using technicalTest.Services;

namespace technicalTest.Controllers
{
    [ApiController]
    [Route("api/leaverequests")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

       
        [HttpGet("filter")]
        public async Task<ActionResult<PagedResponseDto<LeaveRequestDto>>> GetLeaveRequests(
            [FromQuery] LeaveRequestFilterDto filter)
        {
            var result = await _leaveRequestService.FilterLeaveRequestsAsync(filter);
            return Ok(result);
        }

        
        [HttpPost]
        public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest(
            [FromBody] CreateLeaveRequestDto createDto)
        {
            try
            {
                var result = await _leaveRequestService.CreateLeaveRequestAsync(createDto);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/LeaveRequests/5
        [HttpPut("{id}")]
        public async Task<ActionResult<LeaveRequest>> UpdateLeaveRequest(
            int id, [FromBody] UpdateLeaveRequestDto updateDto)
        {
            try
            {
                var result = await _leaveRequestService.UpdateLeaveRequestAsync(id, updateDto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/LeaveRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveRequest(int id)
        {
            try
            {
                await _leaveRequestService.DeleteLeaveRequest(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<LeaveRequestDto>> ApproveLeaveRequest(int id)
        {
            try
            {
                var result = await _leaveRequestService.ApproveLeaveRequestAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
