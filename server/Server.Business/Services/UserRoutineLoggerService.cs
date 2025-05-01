using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class UserRoutineLoggerService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public UserRoutineLoggerService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }

    public async Task<GetAlUserRoutineLoggerPaginationResponse> GetAllUserRoutineLoggersAsync(int? userRoutineId,
        int pageIndex, int pageSize)
    {
        var query = _unitOfWorks.UserRoutineLoggerRepository
            .FindByCondition(x => x.Status == ObjectStatus.Active.ToString())
            .Include(x => x.Staff).ThenInclude(x => x.StaffInfo)
            .Include(x => x.User)
            .Include(x => x.UserRoutineStep)
            .ThenInclude(x => x.UserRoutine) // đảm bảo EF load được UserRoutine nếu cần filter
            .AsQueryable();

        if (userRoutineId.HasValue)
        {
            query = query.Where(x => x.UserRoutineStep.UserRoutineId == userRoutineId.Value);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pageUserRoutineLoggers = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new GetAlUserRoutineLoggerPaginationResponse()
        {
            message = "Lấy danh sách thành công",
            data = _mapper.Map<List<UserRoutineLoggerModel>>(pageUserRoutineLoggers),
            pagination = new Pagination
            {
                page = pageIndex,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }


    public async Task<bool> CreateUserRoutineLogger(UserRoutineLoggerRequest request)
    {
        var hasUser = request.UserId.HasValue;
        var hasStaff = request.StaffId.HasValue;

        // Validate: phải có đúng một trong hai
        if ((hasUser && hasStaff) || (!hasUser && !hasStaff))
        {
            throw new BadRequestException("Phải có đúng một trong hai: UserId hoặc StaffId.");
        }

        // Kiểm tra Step có tồn tại không
        var stepExists =
            await _unitOfWorks.UserRoutineStepRepository.FindByCondition(s =>
                    s.UserRoutineStepId == request.StepId)
                .Include(x => x.UserRoutine)
                .FirstOrDefaultAsync();
        if (stepExists == null)
        {
            throw new BadRequestException($"Step với Id = {request.StepId} không tồn tại.");
        }

        // Nếu có UserId → kiểm tra User có tồn tại
        if (hasUser)
        {
            var userExists = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if (userExists == null)
            {
                throw new BadRequestException($"User với Id = {request.UserId} không tồn tại.");
            }

            // Kiểm tra User có thuộc SkinCareRoutine không
            var userRoutineExists = await _unitOfWorks.UserRoutineRepository
                .FirstOrDefaultAsync(u =>
                    u.UserId == request.UserId && u.RoutineId == stepExists.UserRoutine.RoutineId);
            if (userRoutineExists == null)
            {
                throw new BadRequestException(
                    $"User với Id = {request.UserId} không thuộc SkinCareRoutine với Id = {stepExists.UserRoutine.RoutineId}.");
            }
        }

        // Nếu có StaffId → kiểm tra Staff có tồn tại
        if (hasStaff)
        {
            var staffExists = await _unitOfWorks.StaffRepository.FirstOrDefaultAsync(s => s.StaffId == request.StaffId);
            if (staffExists == null)
            {
                throw new BadRequestException($"Staff với Id = {request.StaffId} không tồn tại.");
            }
        }

        var userRoutineLogger = new UserRoutineLogger
        {
            StepId = stepExists.UserRoutineStepId,
            StaffId = request.StaffId,
            UserId = request.UserId,
            ActionDate = request.ActionDate,
            Step_Logger = request.Step_Logger,
            Notes = request.Notes,
            Status = ObjectStatus.Active.ToString(),
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };

        await _unitOfWorks.UserRoutineLoggerRepository.AddAsync(userRoutineLogger);
        return await _unitOfWorks.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateUserRoutineLoggerAsync(int id, UserRoutineLoggerRequest request)
    {
        var logger =
            await _unitOfWorks.UserRoutineLoggerRepository.FirstOrDefaultAsync(l => l.UserRoutineLoggerId == id);
        if (logger == null)
            throw new NotFoundException($"Không tìm thấy logger với Id = {id}");

        var hasUser = request.UserId.HasValue;
        var hasStaff = request.StaffId.HasValue;

        if ((hasUser && hasStaff) || (!hasUser && !hasStaff))
        {
            throw new BadRequestException("Phải có đúng một trong hai: UserId hoặc StaffId.");
        }

        var stepExists = await _unitOfWorks.SkinCareRoutineStepRepository
            .FirstOrDefaultAsync(s => s.SkinCareRoutineStepId == request.StepId);
        if (stepExists == null)
        {
            throw new BadRequestException($"Step với Id = {request.StepId} không tồn tại.");
        }

        if (hasUser)
        {
            var userExists = await _unitOfWorks.UserRepository.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if (userExists == null)
            {
                throw new BadRequestException($"User với Id = {request.UserId} không tồn tại.");
            }

            var userRoutineExists = await _unitOfWorks.UserRoutineRepository
                .FirstOrDefaultAsync(u => u.UserId == request.UserId && u.RoutineId == stepExists.SkincareRoutineId);
            if (userRoutineExists == null)
            {
                throw new BadRequestException(
                    $"User với Id = {request.UserId} không thuộc SkinCareRoutine với Id = {stepExists.SkincareRoutineId}.");
            }
        }

        if (hasStaff)
        {
            var staffExists = await _unitOfWorks.StaffRepository.FirstOrDefaultAsync(s => s.StaffId == request.StaffId);
            if (staffExists == null)
            {
                throw new BadRequestException($"Staff với Id = {request.StaffId} không tồn tại.");
            }
        }

        logger.StepId = request.StepId;
        logger.StaffId = request.StaffId;
        logger.UserId = request.UserId;
        logger.ActionDate = request.ActionDate;
        logger.Step_Logger = request.Step_Logger;
        logger.Notes = request.Notes;
        logger.UpdatedDate = DateTime.Now;

        _unitOfWorks.UserRoutineLoggerRepository.Update(logger);
        return await _unitOfWorks.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteUserRoutineLoggerAsync(int id)
    {
        var logger =
            await _unitOfWorks.UserRoutineLoggerRepository.FirstOrDefaultAsync(l => l.UserRoutineLoggerId == id);
        if (logger == null)
            throw new NotFoundException($"Không tìm thấy logger với Id = {id}");

        logger.Status = ObjectStatus.InActive.ToString();

        _unitOfWorks.UserRoutineLoggerRepository.Update(logger);
        return await _unitOfWorks.SaveChangesAsync() > 0;
    }
}