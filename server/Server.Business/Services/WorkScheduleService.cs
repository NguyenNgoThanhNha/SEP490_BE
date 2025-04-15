using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data;
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
        var staff = await _unitOfWork.StaffRepository.FindByCondition(x => x.StaffId == workSheduleRequest.StaffId)
            .Include(x => x.StaffInfo).FirstOrDefaultAsync();
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
            throw new BadRequestException(
                $"Nhân viên {staff.StaffInfo.FullName} đã có lịch làm việc trong ca {shift.ShiftName} vào các ngày: {string.Join(", ", duplicateDays)}.");
        }

        var workSchedule = _mapper.Map<List<WorkSchedule>>(scheduleListModel);
        await _unitOfWork.WorkScheduleRepository.AddRangeAsync(workSchedule);
    }

    public async Task CreateWorkScheduleMultiShiftAsync(MultiShiftWorkScheduleRequest request)
    {
        var staff = await _unitOfWork.StaffRepository
            .FindByCondition(x => x.StaffId == request.StaffId)
            .Include(x => x.StaffInfo)
            .FirstOrDefaultAsync();


        if (staff == null)
            throw new BadRequestException("Không tìm thấy nhân viên!");

        if (request.FromDate < DateTime.Today.AddDays(7))
            throw new BadRequestException("Lịch làm việc phải được đăng ký trước ít nhất 1 tuần.");

        var validShifts = await _unitOfWork.ShiftRepository
            .FindByCondition(x => request.ShiftIds.Contains(x.ShiftId))
            .ToListAsync();

        if (validShifts.Count != request.ShiftIds.Count)
            throw new BadRequestException("Một hoặc nhiều ca làm không tồn tại.");

        var existingSchedules = await _unitOfWork.WorkScheduleRepository
            .FindByCondition(ws => ws.StaffId == staff.StaffId
                                   && ws.WorkDate >= request.FromDate
                                   && ws.WorkDate <= request.ToDate)
            .ToListAsync();

        var scheduleListModel = new List<WorkScheduleModel>();
        var duplicateEntries = new List<string>();

        for (var date = request.FromDate; date <= request.ToDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Sunday) continue;

            foreach (var shift in validShifts)
            {
                if (existingSchedules.Any(ws => ws.WorkDate == date && ws.ShiftId == shift.ShiftId))
                {
                    duplicateEntries.Add($"{date:dd/MM/yyyy} (Ca: {shift.ShiftName})");
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

        if (duplicateEntries.Any())
        {
            throw new BadRequestException(
                $"Nhân viên {staff.StaffInfo.FullName} đã có lịch làm trong: {string.Join("; ", duplicateEntries)}");
        }

        var workSchedules = _mapper.Map<List<WorkSchedule>>(scheduleListModel);
        await _unitOfWork.WorkScheduleRepository.AddRangeAsync(workSchedules);
    }


    public async Task<bool> UpdateWorkScheduleForStaffLeaveAsync(
        WorkScheduleForStaffLeaveRequest workScheduleForStaffLeaveRequest)
    {
        var workSchedule = await _unitOfWork.WorkScheduleRepository
            .FirstOrDefaultAsync(ws => ws.StaffId == workScheduleForStaffLeaveRequest.StaffLeaveId
                                       && ws.ShiftId == workScheduleForStaffLeaveRequest.ShiftId
                                       && ws.WorkDate == workScheduleForStaffLeaveRequest.WorkDate
                                       && ws.DayOfWeek == workScheduleForStaffLeaveRequest.DayOfWeek);
        if (workSchedule == null)
        {
            throw new BadRequestException("Work schedule not found");
        }

        var workScheduleReplace = await _unitOfWork.WorkScheduleRepository
            .FirstOrDefaultAsync(ws => ws.StaffId == workScheduleForStaffLeaveRequest.StaffReplaceId
                                       && ws.ShiftId == workScheduleForStaffLeaveRequest.ShiftId
                                       && ws.WorkDate == workScheduleForStaffLeaveRequest.WorkDate
                                       && ws.DayOfWeek == workScheduleForStaffLeaveRequest.DayOfWeek);

        if (workScheduleReplace != null)
        {
            throw new BadRequestException("Work schedule for staff replace already exists");
        }

        workSchedule.Status = ObjectStatus.InActive.ToString();
        workSchedule.UpdatedDate = DateTime.Now;

        _unitOfWork.WorkScheduleRepository.Update(workSchedule);

        var createWorkSchedule = new WorkSchedule
        {
            StaffId = workScheduleForStaffLeaveRequest.StaffReplaceId,
            ShiftId = workScheduleForStaffLeaveRequest.ShiftId,
            DayOfWeek = workScheduleForStaffLeaveRequest.DayOfWeek,
            WorkDate = workScheduleForStaffLeaveRequest.WorkDate,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };
        await _unitOfWork.WorkScheduleRepository.AddAsync(createWorkSchedule);
        return await _unitOfWork.WorkScheduleRepository.Commit() > 0;
    }

    public async Task<IEnumerable<WorkScheduleModel>> GetWorkSchedulesByMonthYearAsync(int staffId, int month, int year)
    {
        var workSchedules = await _unitOfWork.WorkScheduleRepository
            .FindByCondition(ws => ws.StaffId == staffId
                                   && ws.WorkDate.Month == month
                                   && ws.WorkDate.Year == year)
            .Include(ws => ws.Shift) // Nạp thông tin ca làm việc
            .ToListAsync();

        return _mapper.Map<List<WorkScheduleModel>>(workSchedules);
    }

    public async Task<IEnumerable<ShiftModel>> GetShiftSlotsByMonthYearAsync(int staffId, int month, int year)
    {
        var shifts = await _unitOfWork.WorkScheduleRepository
            .FindByCondition(ws => ws.StaffId == staffId
                                   && ws.WorkDate.Month == month
                                   && ws.WorkDate.Year == year)
            .Include(ws => ws.Shift) // Nạp thông tin ca làm việc
            .Select(ws => ws.Shift)
            .Distinct()
            .ToListAsync();

        return _mapper.Map<List<ShiftModel>>(shifts);
    }
}