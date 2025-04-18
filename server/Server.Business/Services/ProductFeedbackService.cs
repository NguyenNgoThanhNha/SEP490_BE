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
    public class ProductFeedbackService
    {

        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ProductFeedbackService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ProductFeedbackDetailDto?> GetByIdAsync(int id)
        {
            var feedback = await _unitOfWorks.ProductFeedbackRepository
                .FindByCondition(f => f.ProductFeedbackId == id)
                .Include(f => f.Product)
                .Include(f => f.User)
                .FirstOrDefaultAsync();

            return feedback == null ? null : _mapper.Map<ProductFeedbackDetailDto>(feedback);
        }

        public async Task<List<ProductFeedbackDetailDto>> GetAllAsync()
        {
            var feedbacks = await _unitOfWorks.ProductFeedbackRepository
                .GetAll()
                .Include(f => f.Product)
                .Include(f => f.User)
                .ToListAsync();

            return _mapper.Map<List<ProductFeedbackDetailDto>>(feedbacks);
        }

        public async Task<ProductFeedbackDetailDto> CreateAsync(ProductFeedbackCreateDto dto)
        {
            if (dto.ProductId <= 0 || dto.UserId <= 0 || dto.Rating <= 0)
                throw new ArgumentException("ProductId, UserId và Rating phải lớn hơn 0.");

            // 👉 Gán mặc định nếu không truyền Status
            if (string.IsNullOrWhiteSpace(dto.Status))
                dto.Status = "Pending";

            var entity = _mapper.Map<ProductFeedback>(dto);
            entity.CreatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = entity.CreatedBy;

            await _unitOfWorks.ProductFeedbackRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<ProductFeedbackDetailDto>(entity);
        }



        public async Task<ProductFeedbackDetailDto?> UpdateAsync(int id, ProductFeedbackUpdateDto dto)
        {
            var feedback = await _unitOfWorks.ProductFeedbackRepository
                .FindByCondition(f => f.ProductFeedbackId == id)
                .FirstOrDefaultAsync();

            if (feedback == null) return null;

            _mapper.Map(dto, feedback);
            feedback.UpdatedDate = DateTime.UtcNow;

            _unitOfWorks.ProductFeedbackRepository.Update(feedback);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<ProductFeedbackDetailDto>(feedback);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = _unitOfWorks.ProductFeedbackRepository.Remove(id); // synchronous như bạn yêu cầu
            if (entity == null) return false;

            await _unitOfWorks.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductFeedbackDetailDto>> GetByProductIdAsync(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("ProductId không hợp lệ.");

            var feedbacks = await _unitOfWorks.ProductFeedbackRepository
                .FindByCondition(f => f.ProductId == productId)
                .Include(f => f.Customer) // ✅ lấy thông tin người phản hồi (là Customer)
                .Include(f => f.Product)
                .ToListAsync();

            return _mapper.Map<List<ProductFeedbackDetailDto>>(feedbacks);
        }


    }

}
