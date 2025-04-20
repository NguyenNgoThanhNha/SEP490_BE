using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data;
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
    
    public async Task<bool> ApproveStaffLeaveAsync(int staffLeaveId)
    {
        var staffLeave = await _unitOfWorks.StaffLeaveRepository.GetByIdAsync(staffLeaveId);
        if (staffLeave == null)
        {
            throw new BadRequestException("Staff leave not found");
        }
        
        var workingSchedules = await _unitOfWorks.WorkScheduleRepository
            .FindByCondition(ws => ws.StaffId == staffLeave.StaffId && ws.WorkDate == staffLeave.LeaveDate)
            .ToListAsync();
        if (workingSchedules != null)
        {
            foreach (var workSchedule in workingSchedules)
            {
                workSchedule.Status = ObjectStatus.InActive.ToString();
                workSchedule.UpdatedDate = DateTime.Now;
                _unitOfWorks.WorkScheduleRepository.Update(workSchedule);
            }
        }

        staffLeave.Status = StaffLeaveStatus.Approved.ToString();
        staffLeave.UpdatedDate = DateTime.Now;

        _unitOfWorks.StaffLeaveRepository.Update(staffLeave);
        var result = await _unitOfWorks.SaveChangesAsync();
        return result > 0;
    }
    
    public async Task<bool> RejectStaffLeaveAsync(int staffLeaveId)
    {
        var staffLeave = await _unitOfWorks.StaffLeaveRepository.GetByIdAsync(staffLeaveId);
        if (staffLeave == null)
        {
            throw new BadRequestException("Staff leave not found");
        }

        staffLeave.Status = StaffLeaveStatus.Rejected.ToString();
        staffLeave.UpdatedDate = DateTime.Now;

        _unitOfWorks.StaffLeaveRepository.Update(staffLeave);
        var result = await _unitOfWorks.StaffLeaveRepository.Commit();
        return result > 0;
    }

}