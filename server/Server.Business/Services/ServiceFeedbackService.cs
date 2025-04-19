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
    public class ServiceFeedbackService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ServiceFeedbackService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ServiceFeedbackDetailDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWorks.FeedbackServiceRepository
                .FindByCondition(f => f.ServiceFeedbackId == id)
                .Include(f => f.Customer)
                .Include(f => f.Service)
                .FirstOrDefaultAsync();

            return feedback == null ? null : _mapper.Map<ServiceFeedbackDetailDto>(feedback);
        }

        public async Task<List<ServiceFeedbackDetailDto>> GetAllAsync()
        {
            var feedbacks = await _unitOfWorks.ServiceFeedbackRepository
                .GetAll()
                .Include(f => f.Service)
                .Include(f => f.Customer)
                .OrderByDescending(f => f.ServiceFeedbackId)
                .ToListAsync();

            return _mapper.Map<List<ServiceFeedbackDetailDto>>(feedbacks);
        }

        public async Task<ServiceFeedbackDetailDto> CreateAsync(ServiceFeedbackCreateDto dto)
        {
            if (dto.ServiceId <= 0 || dto.UserId <= 0 || dto.Rating <= 0)
                throw new ArgumentException("ServiceId, UserId và Rating phải lớn hơn 0.");

            if (string.IsNullOrWhiteSpace(dto.Status))
                dto.Status = "Pending";

            var entity = _mapper.Map<ServiceFeedback>(dto);
            entity.CreatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = entity.CreatedBy;

            await _unitOfWorks.ServiceFeedbackRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            // 👉 Load lại entity có include User (customer)
            var createdEntity = await _unitOfWorks.ServiceFeedbackRepository
                .FindByCondition(x => x.ServiceFeedbackId == entity.ServiceFeedbackId)
                .Include(x => x.Customer) // Include User
                .FirstOrDefaultAsync();

            return _mapper.Map<ServiceFeedbackDetailDto>(createdEntity);
        }


        public async Task<ServiceFeedbackDetailDto?> UpdateAsync(int id, ServiceFeedbackUpdateDto dto)
        {
            var feedback = await _unitOfWorks.ServiceFeedbackRepository
                .FindByCondition(f => f.ServiceFeedbackId == id)
                .FirstOrDefaultAsync();

            if (feedback == null) return null;

            _mapper.Map(dto, feedback);
            feedback.UpdatedDate = DateTime.UtcNow;

            _unitOfWorks.ServiceFeedbackRepository.Update(feedback);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<ServiceFeedbackDetailDto>(feedback);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = _unitOfWorks.ServiceFeedbackRepository.Remove(id);
            if (entity == null) return false;

            await _unitOfWorks.SaveChangesAsync();
            return true;
        }

        public async Task<List<ServiceFeedbackDetailDto>> GetByServiceIdAsync(int serviceId)
        {
            if (serviceId <= 0)
                throw new ArgumentException("ServiceId không hợp lệ.");

            var feedbacks = await _unitOfWorks.ServiceFeedbackRepository
                .FindByCondition(f => f.ServiceId == serviceId)
                .Include(f => f.Service)
                .Include(f => f.Customer)
                .ToListAsync();

            return _mapper.Map<List<ServiceFeedbackDetailDto>>(feedbacks);
        }

    }
}
