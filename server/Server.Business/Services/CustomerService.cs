using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Services
{
    public class CustomerService
    {
        private readonly UnitOfWorks _unitOfWork;

        public CustomerService(UnitOfWorks unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // API 1: Check điểm hiện tại
        public async Task<CheckPointResponse> CheckBonusPoints(int customerId)
        {
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == customerId && u.RoleID==3);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return new CheckPointResponse
            {
                UserId = user.UserId,
                BonusPoint = user.BonusPoint
            };
        }

        public async Task<ExchangePointResponse> ExchangePoints(ExchangePointRequest request)
        {
            // 1. Kiểm tra User tồn tại
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // 2. Kiểm tra Promotion tồn tại
            var promotion = await _unitOfWork.PromotionRepository.FirstOrDefaultAsync(p => p.PromotionId == request.PromotionId);
            if (promotion == null || promotion.Status != "Active")
                throw new KeyNotFoundException("Promotion not found or inactive");

            // 3. Tính toán điểm cần thiết để đổi dựa trên DiscountPercent
            int pointsRequired = (int)(promotion.DiscountPercent * 10);

            // 4. Kiểm tra điểm thưởng của khách hàng
            if (user.BonusPoint < pointsRequired)
                throw new InvalidOperationException($"Not enough points. This promotion requires {pointsRequired} points.");

            // 5. Trừ điểm và cập nhật
            user.BonusPoint -= pointsRequired;
            user.UpdatedDate = DateTime.Now;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.UserRepository.Commit();

            // 6. Trả về kết quả
            return new ExchangePointResponse
            {
                Message = "Exchange successful!",
                RemainingPoints = user.BonusPoint,
                PromotionName = promotion.PromotionName
            };
        }


        public async Task<List<BusyTimeDto>> GetCustomerBusyTimesAsync(int customerId, DateTime date)
        {
            // Lấy tất cả Appointments của khách hàng trong ngày
            var appointments = await _unitOfWork.AppointmentsRepository
                .FindByCondition(a => a.CustomerId == customerId &&
                                      a.AppointmentsTime.Date == date.Date &&
                                      a.Status == "Confirmed") // Chỉ lấy trạng thái đã xác nhận
                .Include(a => a.Service) // Include Service để lấy duration
                .ToListAsync();

            // Tính toán thời gian bận
            var busyTimes = appointments
                .Where(a => a.Service != null) // Bỏ qua các bản ghi không có Service
                .Select(a =>
                {
                    // Parse duration từ Service (VD: "60 minutes")
                    var durationParts = a.Service.Duration.Split(' ');
                    int durationMinutes = int.TryParse(durationParts[0], out var minutes) ? minutes : 0;

                    return new BusyTimeDto
                    {
                        StartTime = a.AppointmentsTime,
                        EndTime = a.AppointmentsTime.AddMinutes(durationMinutes)
                    };
                })
                .OrderBy(bt => bt.StartTime) // Sắp xếp theo thời gian bắt đầu
                .ToList();

            return busyTimes;
        }


    }

}
