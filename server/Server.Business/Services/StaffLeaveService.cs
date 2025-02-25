using AutoMapper;
using MongoDB.Driver.Linq;
using Server.Business.Commons.Request;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class StaffLeaveService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public StaffLeaveService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }
    
    public async Task<StaffLeaveModel> CreateStaffLeaveAsync(StaffLeaveRequest staffLeaveRequest)
    {
        var staff = await _unitOfWorks.StaffRepository.GetByIdAsync(staffLeaveRequest.StaffId);
        if (staff == null)
        {
            throw new BadRequestException("Staff not found");
        }

        // Kiểm tra ngày nghỉ phải trước ít nhất 2 ngày
        var minLeaveDate = DateTime.Now.Date.AddDays(2);
        if (staffLeaveRequest.LeaveDate < minLeaveDate)
        {
            throw new BadRequestException("Leave request must be submitted at least 2 days in advance.");
        }

        // Kiểm tra xem đã tồn tại đơn xin nghỉ trong ngày đó chưa
        var existingLeave = await _unitOfWorks.StaffLeaveRepository
            .FirstOrDefaultAsync(sl =>
                sl.StaffId == staffLeaveRequest.StaffId && sl.LeaveDate == staffLeaveRequest.LeaveDate);

        if (existingLeave != null)
        {
            throw new BadRequestException("A leave request for this date already exists.");
        }

        var staffLeave = new StaffLeave
        {
            StaffId = staffLeaveRequest.StaffId,
            LeaveDate = staffLeaveRequest.LeaveDate,
            Reason = staffLeaveRequest.Reason
        };

        var staffLeaveCreated = await _unitOfWorks.StaffLeaveRepository.AddAsync(staffLeave);
        var result = await _unitOfWorks.StaffLeaveRepository.Commit();
    
        if (result > 0)
        {
            return _mapper.Map<StaffLeaveModel>(staffLeaveCreated);
        }

        return null;
    }

}