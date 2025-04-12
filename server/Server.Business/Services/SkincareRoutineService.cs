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
    public class SkincareRoutineService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public SkincareRoutineService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<GetAllSkincareRoutinePaginationResponse> GetAllPaginationAsync(int page, int pageSize)
        {
            var query = _unitOfWorks.SkincareRoutineRepository
                .GetAll()
                .OrderByDescending(x => x.UpdatedDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = _mapper.Map<List<SkincareRoutineDto>>(items);

            return new GetAllSkincareRoutinePaginationResponse
            {
                message = "Lấy danh sách skincare routine thành công",
                data = result,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }

        public async Task<SkincareRoutineDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<SkincareRoutineDto>(entity);
        }

        public async Task<SkincareRoutineDto?> CreateAsync(CreateSkincareRoutineDto dto)
        {
            var entity = _mapper.Map<SkincareRoutine>(dto);
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedDate = DateTime.Now;

            await _unitOfWorks.SkincareRoutineRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<SkincareRoutineDto>(entity);
        }

        public async Task<SkincareRoutineDto?> UpdateAsync(int id, UpdateSkincareRoutineDto dto)
        {
            var entity = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.UpdatedDate = DateTime.Now;

            _unitOfWorks.SkincareRoutineRepository.Update(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<SkincareRoutineDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWorks.SkincareRoutineRepository.Remove(id);
            await _unitOfWorks.SaveChangesAsync();
            return true;
        }
    }
}
