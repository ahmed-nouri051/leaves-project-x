using AutoMapper;
using technicalTest.DTO;
using technicalTest.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.LeaveType,
                opt => opt.MapFrom(src => src.LeaveType.ToString()))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateLeaveRequestDto, LeaveRequest>()
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(_ => RequestStatus.Pending));

        CreateMap<UpdateLeaveRequestDto, LeaveRequest>();
    }
}
