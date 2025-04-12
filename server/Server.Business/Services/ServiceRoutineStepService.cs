using AutoMapper;
using Server.Business.Commons.Response;
using Server.Business.Commons;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Server.Business.Services
{
    public class ServiceRoutineStepService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ServiceRoutineStepService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ApiResult<object>> AssignServiceToRoutineStepAsync(AssignServiceToRoutineStepDto dto)
        {
            if (dto.ServiceId <= 0 || dto.StepId <= 0)
                return ApiResult<object>.Error(new ApiResponse { message = "ServiceId hoặc StepId không hợp lệ." });

            var existed = await _unitOfWorks.ServiceRoutineStepRepository
                .FindByCondition(x => x.ServiceId == dto.ServiceId && x.StepId == dto.StepId)
                .FirstOrDefaultAsync();

            if (existed != null)
                return ApiResult<object>.Error(new ApiResponse { message = "Dịch vụ này đã được gán vào bước skincare này rồi." });

            var service = await _unitOfWorks.ServiceRepository.GetByIdAsync(dto.ServiceId);
            var step = await _unitOfWorks.SkinCareRoutineStepRepository.GetByIdAsync(dto.StepId);

            if (service == null || step == null)
                return ApiResult<object>.Error(new ApiResponse { message = "Không tìm thấy Service hoặc Step tương ứng." });

            var entity = new ServiceRoutineStep
            {
                ServiceId = dto.ServiceId,
                StepId = dto.StepId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWorks.ServiceRoutineStepRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            var fullEntity = await _unitOfWorks.ServiceRoutineStepRepository
                .FindByCondition(x => x.Id == entity.Id)
                .Include(x => x.Service)
                .Include(x => x.Step)
                .FirstOrDefaultAsync();

            if (fullEntity == null)
                return ApiResult<object>.Error(new ApiResponse { message = "Không thể load lại dữ liệu sau khi tạo." });

            var resultDto = _mapper.Map<ServiceRoutineStepDto>(fullEntity);

            return ApiResult<object>.Succeed(new ApiResponse
            {
                message = "Gán dịch vụ vào bước skincare thành công",
                data = resultDto
            });
        }
    }
}
