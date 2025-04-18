using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Dtos;
using Server.Data;
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
        private readonly CloudianryService _cloudinaryService;

        public AppointmentFeedbackService(UnitOfWorks unitOfWorks, IMapper mapper, CloudianryService cloudinaryService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<AppointmentFeedbackDetailDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWorks.AppointmentFeedbackRepository
                .FindByCondition(f => f.AppointmentFeedbackId == id)
                .Include(f => f.Appointment)
                .Include(f => f.Customer)
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
                .Include(f => f.Customer)
                .ToListAsync();

            return _mapper.Map<List<AppointmentFeedbackDetailDto>>(feedbacks);
        }

        // Tạo phản hồi mới
        public async Task<object> CreateAsync(AppointmentFeedbackCreateDto dto)
        {
            var entity = _mapper.Map<AppointmentFeedback>(dto);
            entity.CreatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = entity.CreatedBy;
            entity.Status = "Pending";

            await _unitOfWorks.AppointmentFeedbackRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            return new { Id = entity.AppointmentFeedbackId };
        }



        // Cập nhật phản hồi
        public async Task<object?> PatchUpdateAsync(int id, AppointmentFeedbackUpdateFormDto dto)
        {
            var feedback = await _unitOfWorks.AppointmentFeedbackRepository
                .FindByCondition(f => f.AppointmentFeedbackId == id)
                .FirstOrDefaultAsync();

            if (feedback == null) return null;

            //if (!string.IsNullOrEmpty(dto.Comment))
            //    feedback.Comment = dto.Comment;

            //if (dto.Rating.HasValue)
            //    feedback.Rating = dto.Rating.Value;

            //if (!string.IsNullOrEmpty(dto.Status))
            //    feedback.Status = dto.Status;

            //if (dto.ImageBefore != null)
            //{
            //    var imageBeforeUpload = await _cloudinaryService.UploadImageAsync(dto.ImageBefore);
            //    if (imageBeforeUpload != null)
            //        feedback.ImageBefore = imageBeforeUpload.SecureUrl.ToString();
            //}

            if (dto.ImageAfter != null)
            {
                var imageAfterUpload = await _cloudinaryService.UploadImageAsync(dto.ImageAfter);
                if (imageAfterUpload != null)
                    feedback.ImageAfter = imageAfterUpload.SecureUrl.ToString();
            }

            feedback.UpdatedDate = DateTime.UtcNow;

            _unitOfWorks.AppointmentFeedbackRepository.Update(feedback);
            await _unitOfWorks.SaveChangesAsync();

            return new { AppointmentFeedbackId = feedback.AppointmentFeedbackId };
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
                .Include(f => f.Customer)
                .ToListAsync();

            return _mapper.Map<List<AppointmentFeedbackDetailDto>>(feedbacks);
        }


    }
}
