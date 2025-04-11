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
    public class ServiceRoutineService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public ServiceRoutineService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }
        public async Task<ServiceRoutineDto> AssignServiceToRoutineAsync(AssignServiceToRoutineDto dto)
        {
            // Validate ID hợp lệ
            if (dto.ServiceId <= 0 || dto.RoutineId <= 0)
                throw new Exception("ServiceId hoặc RoutineId không hợp lệ.");

            // Kiểm tra trùng lặp
            var existed = await _unitOfWorks.ServiceRoutineRepository
                .FindByCondition(x => x.ServiceId == dto.ServiceId && x.RoutineId == dto.RoutineId)
                .FirstOrDefaultAsync();

            if (existed != null)
                throw new Exception("Dịch vụ đã được gán vào routine này rồi.");

            // Kiểm tra tồn tại Service và Routine
            var service = await _unitOfWorks.ServiceRepository.GetByIdAsync(dto.ServiceId);
            var routine = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(dto.RoutineId);

            if (service == null || routine == null)
                throw new Exception("Không tìm thấy Service hoặc Routine.");

            // Tạo mới bản ghi
            var entity = new ServiceRoutine
            {
                ServiceId = dto.ServiceId,
                RoutineId = dto.RoutineId,
                Status = "Active",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWorks.ServiceRoutineRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            // Lấy lại bản ghi có include Service & Routine
            var fullEntity = await _unitOfWorks.ServiceRoutineRepository
                .FindByCondition(x => x.ServiceRoutineId == entity.ServiceRoutineId)
                .Include(x => x.Service)
                .Include(x => x.Routine)
                .FirstOrDefaultAsync();

            if (fullEntity == null)
                throw new Exception("Không thể load lại dữ liệu sau khi tạo.");

            return _mapper.Map<ServiceRoutineDto>(fullEntity);
        }
    }
}
