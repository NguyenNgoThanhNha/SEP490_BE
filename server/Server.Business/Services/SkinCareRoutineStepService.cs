using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Response;
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
    public class SkinCareRoutineStepService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public SkinCareRoutineStepService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<GetAllSkinCareRoutineStepPaginationResponse> GetAllPaginationAsync(int page, int pageSize)
        {
            var query = _unitOfWorks.SkinCareRoutineStepRepository
                .GetAll()
                .OrderBy(x => x.SkincareRoutineId)
                .ThenBy(x => x.Step);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = _mapper.Map<List<SkinCareRoutineStepDto>>(items);

            return new GetAllSkinCareRoutineStepPaginationResponse
            {
                message = "Lấy danh sách bước skincare thành công",
                data = result,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }


        public async Task<SkinCareRoutineStepDto?> GetByIdAsync(int id)
        {
            var step = await _unitOfWorks.SkinCareRoutineStepRepository.GetByIdAsync(id);
            return step == null ? null : _mapper.Map<SkinCareRoutineStepDto>(step);
        }

        public async Task<SkinCareRoutineStepDto> CreateAsync(CreateSkinCareRoutineStepDto dto)
        {
            if (dto.SkincareRoutineId <= 0)
                throw new Exception("SkincareRoutineId phải lớn hơn 0.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Tên bước (Name) là bắt buộc.");

            if (dto.Step < 1 || dto.Step > 10)
                throw new Exception("Step phải nằm trong khoảng từ 1 đến 10.");

            if (dto.IntervalBeforeNextStep.HasValue && dto.IntervalBeforeNextStep < TimeSpan.Zero)
                throw new Exception("Khoảng thời gian chờ không được nhỏ hơn 0.");

            var routine = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(dto.SkincareRoutineId);
            if (routine == null)
                throw new Exception("SkincareRoutineId không tồn tại.");

            var entity = _mapper.Map<SkinCareRoutineStep>(dto);
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedDate = DateTime.Now;

            await _unitOfWorks.SkinCareRoutineStepRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<SkinCareRoutineStepDto>(entity);
        }



        public async Task<SkinCareRoutineStepDto> UpdateAsync(int id, UpdateSkinCareRoutineStepDto dto)
        {
            var entity = await _unitOfWorks.SkinCareRoutineStepRepository.GetByIdAsync(id);
            if (entity == null)
                throw new Exception("Không tìm thấy bước skincare với ID được cung cấp.");

            if (dto.SkincareRoutineId <= 0)
                throw new Exception("SkincareRoutineId phải lớn hơn 0.");

            var routine = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(dto.SkincareRoutineId);
            if (routine == null)
                throw new Exception("SkincareRoutineId không tồn tại.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Tên bước (Name) là bắt buộc.");

            if (dto.Step < 1 || dto.Step > 10)
                throw new Exception("Step phải nằm trong khoảng từ 1 đến 10.");

            if (dto.IntervalBeforeNextStep.HasValue && dto.IntervalBeforeNextStep < TimeSpan.Zero)
                throw new Exception("Khoảng thời gian chờ không được nhỏ hơn 0.");

            _mapper.Map(dto, entity);
            entity.UpdatedDate = DateTime.Now;

            _unitOfWorks.SkinCareRoutineStepRepository.Update(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<SkinCareRoutineStepDto>(entity);
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWorks.SkinCareRoutineStepRepository.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWorks.SkinCareRoutineStepRepository.Remove(id);
            await _unitOfWorks.SaveChangesAsync();
            return true;
        }
    }
}
