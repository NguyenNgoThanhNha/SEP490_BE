using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Services
{
    public class AppointmentFeedbackService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public AppointmentFeedbackService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<AppointmentFeedbackDetailDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWorks.AppointmentFeedbackRepository
                .FindByCondition(f => f.AppointmentFeedbackId == id)
                .Include(f => f.Appointment)
                .Include(f => f.User)
                .FirstOrDefaultAsync();

            if (feedback == null) return null;

            var dto = _mapper.Map<AppointmentFeedbackDetailDto>(feedback);
            return dto;
        }
        public async Task<List<AppointmentFeedbackDetailDto>> GetAllAsync()
        {
            var feedbacks = await _unitOfWorks.AppointmentFeedbackRepository
                .GetAll()
                .Include(f => f.Appointment)
                .Include(f => f.User)
                .ToListAsync();

            return _mapper.Map<List<AppointmentFeedbackDetailDto>>(feedbacks);
        }

        // Tạo phản hồi mới
        public async Task<AppointmentFeedbackDetailDto> CreateAsync(AppointmentFeedbackCreateDto dto)
        {
            var entity = _mapper.Map<AppointmentFeedback>(dto);
            entity.CreatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow; // Cập nhật luôn nếu muốn đồng bộ
            entity.UpdatedBy = entity.CreatedBy;  // Gán giống CreatedBy nếu cần

            await _unitOfWorks.AppointmentFeedbackRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<AppointmentFeedbackDetailDto>(entity);
        }


        // Cập nhật phản hồi
        public async Task<AppointmentFeedbackDetailDto?> UpdateAsync(int id, AppointmentFeedbackUpdateDto dto)
        {
            var feedback = await _unitOfWorks.AppointmentFeedbackRepository
                .FindByCondition(f => f.AppointmentFeedbackId == id)
                .FirstOrDefaultAsync();

            if (feedback == null) return null;

            _mapper.Map(dto, feedback);
            feedback.UpdatedDate = DateTime.UtcNow;

            _unitOfWorks.AppointmentFeedbackRepository.Update(feedback);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<AppointmentFeedbackDetailDto>(feedback);
        }

        // Xóa phản hồi
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = _unitOfWorks.AppointmentFeedbackRepository.Remove(id); // KHÔNG await vì Remove là sync
            if (entity == null) return false;

            await _unitOfWorks.SaveChangesAsync(); // vẫn dùng async như bình thường
            return true;
        }

        public async Task<List<AppointmentFeedbackDetailDto>> GetByAppointmentIdAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new ArgumentException("AppointmentId không hợp lệ.");

            var feedbacks = await _unitOfWorks.AppointmentFeedbackRepository
                .FindByCondition(f => f.AppointmentId == appointmentId)
                .Include(f => f.Appointment)
                .Include(f => f.User)
                .ToListAsync();

            return _mapper.Map<List<AppointmentFeedbackDetailDto>>(feedbacks);
        }


    }
}
