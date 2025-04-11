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
    public class ProductRoutineService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ProductRoutineService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ProductRoutineDto> AssignProductToRoutineAsync(AssignProductToRoutineDto dto)
        {
            if (dto.ProductId <= 0 || dto.RoutineId <= 0)
                throw new Exception("ProductId hoặc RoutineId không hợp lệ.");

            var existed = await _unitOfWorks.ProductRoutineRepository
                .FindByCondition(x => x.ProductId == dto.ProductId && x.RoutineId == dto.RoutineId)
                .FirstOrDefaultAsync();

            if (existed != null)
                throw new Exception("Sản phẩm này đã được gán vào routine này rồi.");

            var product = await _unitOfWorks.ProductRepository.GetByIdAsync(dto.ProductId);
            var routine = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(dto.RoutineId);

            if (product == null || routine == null)
                throw new Exception("Không tìm thấy Product hoặc Routine.");

            var entity = new ProductRoutine
            {
                ProductId = dto.ProductId,
                RoutineId = dto.RoutineId,
                Status = "Active",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWorks.ProductRoutineRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            // Include các bảng liên quan
            var fullEntity = await _unitOfWorks.ProductRoutineRepository
                .FindByCondition(x => x.ProductRoutineId == entity.ProductRoutineId)
                .Include(x => x.Products)
                    .ThenInclude(p => p.ProductImages)
                .Include(x => x.Products)
                    .ThenInclude(p => p.Category)
                .Include(x => x.Products)
                    .ThenInclude(p => p.Company)
                .Include(x => x.Routine)
                .FirstOrDefaultAsync();

            if (fullEntity == null)
                throw new Exception("Không thể load lại dữ liệu sau khi tạo.");

            return _mapper.Map<ProductRoutineDto>(fullEntity);
        }
    }
}
