using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Dtos;
using Server.Business.Dtos.Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Services
{
    public class ProductRoutineStepService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ProductRoutineStepService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ProductRoutineStepDto> AssignProductToRoutineStepAsync(AssignProductToProductRoutineStepDto dto)
        {
            if (dto.ProductId <= 0 || dto.StepId <= 0)
                throw new Exception("ProductId hoặc StepId không hợp lệ.");

            // Check duplicate
            var existed = await _unitOfWorks.ProductRoutineStepRepository
                .FindByCondition(x => x.ProductId == dto.ProductId && x.StepId == dto.StepId)
                .FirstOrDefaultAsync();

            if (existed != null)
                throw new Exception("Sản phẩm này đã được gán vào bước skincare này rồi.");

            // Check existence
            var product = await _unitOfWorks.ProductRepository.GetByIdAsync(dto.ProductId);
            var step = await _unitOfWorks.SkinCareRoutineStepRepository.GetByIdAsync(dto.StepId);

            if (product == null || step == null)
                throw new Exception("Không tìm thấy Product hoặc Step tương ứng.");

            // Add
            var entity = new ProductRoutineStep
            {
                ProductId = dto.ProductId,
                StepId = dto.StepId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWorks.ProductRoutineStepRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            // Return with includes
            var result = await _unitOfWorks.ProductRoutineStepRepository
                .FindByCondition(x => x.Id == entity.Id)
                .Include(x => x.Product)
                    .ThenInclude(p => p.Category)
                .Include(x => x.Product)
                    .ThenInclude(p => p.Company)
                .Include(x => x.Step)
                .FirstOrDefaultAsync();

            if (result == null)
                throw new Exception("Không thể load lại dữ liệu sau khi tạo.");

            return _mapper.Map<ProductRoutineStepDto>(result);
        }
    }
}
