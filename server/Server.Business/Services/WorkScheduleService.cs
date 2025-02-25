using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

public class WorkScheduleService
{
    private readonly UnitOfWorks _unitOfWork;
    private readonly IMapper _mapper;

    public WorkScheduleService(UnitOfWorks unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<WorkScheduleModel>> GetWorkSchedulesAsync()
    {
        var listWorkSchedules = await _unitOfWork.WorkScheduleRepository.GetAll().ToListAsync();
        return _mapper.Map<List<WorkScheduleModel>>(listWorkSchedules);
    }
    
    public async Task<WorkScheduleModel> GetWorkScheduleByIdAsync(int id)
    {
        var workSchedule = await _unitOfWork.WorkScheduleRepository.GetByIdAsync(id);
        return _mapper.Map<WorkScheduleModel>(workSchedule);
    }
    
    public async Task CreateWorkScheduleAsync(WorkSheduleRequest workSheduleRequest)
    {
        var staff = await _unitOfWork.StaffRepository.FindByCondition(x=> x.StaffId == workSheduleRequest.StaffId).Include(x => x.StaffInfo).FirstOrDefaultAsync();
        var shift = await _unitOfWork.ShiftRepository.GetByIdAsync(workSheduleRequest.ShiftId);
        if (staff == null || shift == null)
        {
            throw new BadRequestException("Staff or Shift not found");
        }
        
        // Kiểm tra nếu ngày đăng ký nhỏ hơn 7 ngày so với hôm nay
        if (workSheduleRequest.FromDate < DateTime.Today.AddDays(7))
        {
            throw new BadRequestException("Lịch làm việc phải được đăng ký trước ít nhất 1 tuần.");
        }
        
        var existingSchedules = await _unitOfWork.WorkScheduleRepository
            .FindByCondition(ws => ws.StaffId == staff.StaffId 
                                && ws.WorkDate >= workSheduleRequest.FromDate 
                                && ws.WorkDate <= workSheduleRequest.ToDate)
            .ToListAsync();

        var scheduleListModel = new List<WorkScheduleModel>();
        var duplicateDays = new List<string>();

        for (var date = workSheduleRequest.FromDate; date <= workSheduleRequest.ToDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Sunday) // Chỉ làm từ Thứ 2 - Thứ 7
            {
                // Kiểm tra trong danh sách đã truy vấn thay vì gọi DB từng ngày
                if (existingSchedules.Any(ws => ws.WorkDate == date && ws.ShiftId == shift.ShiftId))
                {
                    duplicateDays.Add(date.ToShortDateString());
                    continue; 
                }

                scheduleListModel.Add(new WorkScheduleModel
                {
                    StaffId = staff.StaffId,
                    ShiftId = shift.ShiftId,
                    DayOfWeek = (int)date.DayOfWeek,
                    WorkDate = date,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }
        }

        if (duplicateDays.Any())
        {
            throw new BadRequestException($"Nhân viên {staff.StaffInfo.FullName} đã có lịch làm việc trong ca {shift.ShiftName} vào các ngày: {string.Join(", ", duplicateDays)}.");
        }

        var workSchedule = _mapper.Map<List<WorkSchedule>>(scheduleListModel);
        await _unitOfWork.WorkScheduleRepository.AddRangeAsync(workSchedule);
    }

    
}