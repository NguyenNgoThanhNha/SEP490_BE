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
using Server.Business.Exceptions;
using Server.Data;

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
                .FindByCondition(x => x.Status == ObjectStatus.Active.ToString())
                .OrderByDescending(x => x.SkincareRoutineId)
                .AsQueryable();

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
            var skinConcerns = new List<SkinConcern>();

            foreach (var targetSkinType in dto.TargetSkinTypes)
            {
                var skinConcern = await _unitOfWorks.SkinConcernRepository
                    .FirstOrDefaultAsync(x => x.Name.ToLower().Contains(targetSkinType.ToLower()));

                if (skinConcern == null)
                {
                    throw new BadRequestException($"Không tìm thấy mục tiêu của gói liệu trình: {targetSkinType}");
                }

                skinConcerns.Add(skinConcern);
            }

            var entity = new SkincareRoutine()
            {
                Name = dto.Name,
                Description = dto.Description,
                TotalPrice = dto.TotalPrice,
                TotalSteps = dto.TotalSteps,
                TargetSkinTypes = string.Join(", ", dto.TargetSkinTypes),
                Status = ObjectStatus.Active.ToString(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var createdObject = await _unitOfWorks.SkincareRoutineRepository.AddAsync(entity);
            await _unitOfWorks.SkincareRoutineRepository.Commit();

            // Gán tất cả skinConcern liên quan vào routine
            foreach (var skinConcern in skinConcerns)
            {
                var existed = await _unitOfWorks.SkinCareConcernRepository
                    .FirstOrDefaultAsync(x =>
                        x.SkinConcernId == skinConcern.SkinConcernId &&
                        x.SkincareRoutineId == createdObject.SkincareRoutineId);

                if (existed == null)
                {
                    var skinConcernRoutineEntity = new SkincareRoutineConcern()
                    {
                        SkinConcernId = skinConcern.SkinConcernId,
                        SkincareRoutineId = createdObject.SkincareRoutineId
                    };
                    await _unitOfWorks.SkinCareConcernRepository.AddAsync(skinConcernRoutineEntity);
                }
            }

            await _unitOfWorks.SkinCareConcernRepository.Commit();

            return _mapper.Map<SkincareRoutineDto>(createdObject);
        }


        public async Task<SkincareRoutineDto?> UpdateAsync(int id, UpdateSkincareRoutineDto dto)
        {
            var skincareRoutine = await _unitOfWorks.SkincareRoutineRepository
                .FirstOrDefaultAsync(x => x.SkincareRoutineId == id)
                ?? throw new BadRequestException("Không tìm thấy gói liệu trình");

            // Lấy tất cả concern cũ đang gắn với routine này
            var existingConcerns = await _unitOfWorks.SkinCareConcernRepository
                .FindByCondition(x => x.SkincareRoutineId == id).ToListAsync();

            // Lấy danh sách concern mới từ dto.TargetSkinTypes
            var newSkinConcernIds = new List<int>();
            foreach (var targetSkinType in dto.TargetSkinTypes)
            {
                var skinConcern = await _unitOfWorks.SkinConcernRepository
                    .FirstOrDefaultAsync(x => x.Name.ToLower().Contains(targetSkinType.ToLower()));

                if (skinConcern == null)
                {
                    throw new BadRequestException($"Không tìm thấy mục tiêu của gói liệu trình: {targetSkinType}");
                }

                newSkinConcernIds.Add(skinConcern.SkinConcernId);

                var existed = existingConcerns.FirstOrDefault(x =>
                    x.SkinConcernId == skinConcern.SkinConcernId &&
                    x.SkincareRoutineId == skincareRoutine.SkincareRoutineId);

                // Nếu chưa tồn tại, thì tạo mới
                if (existed == null)
                {
                    var skinConcernRoutineEntity = new SkincareRoutineConcern()
                    {
                        SkinConcernId = skinConcern.SkinConcernId,
                        SkincareRoutineId = skincareRoutine.SkincareRoutineId
                    };
                    await _unitOfWorks.SkinCareConcernRepository.AddAsync(skinConcernRoutineEntity);
                }
            }

            // Xóa những concern không còn nằm trong danh sách mới
            foreach (var concern in existingConcerns)
            {
                if (!newSkinConcernIds.Contains(concern.SkinConcernId))
                {
                    _unitOfWorks.SkinCareConcernRepository.Remove(concern.Id);
                }
            }

            // Cập nhật thông tin routine
            skincareRoutine.Name = dto.Name;
            skincareRoutine.Description = dto.Description;
            skincareRoutine.TotalPrice = dto.TotalPrice;
            skincareRoutine.TotalSteps = dto.TotalSteps;
            skincareRoutine.TargetSkinTypes = string.Join(", ", dto.TargetSkinTypes);
            skincareRoutine.UpdatedDate = DateTime.Now;

            _unitOfWorks.SkincareRoutineRepository.Update(skincareRoutine);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<SkincareRoutineDto>(skincareRoutine);
        }



        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = ObjectStatus.InActive.ToString();
            entity.UpdatedDate = DateTime.Now;
            _unitOfWorks.SkincareRoutineRepository.Update(entity);
            var result = await _unitOfWorks.SkincareRoutineRepository.Commit();
            return result > 0;
        }

        public async Task<List<string>> GetTargetSkinTypesAsync()
        {
            var skinConcerns = await _unitOfWorks.SkinConcernRepository.GetAll().ToListAsync(); 
            var skinTypes = skinConcerns
                .Select(st => st.Name)
                .Distinct()
                .ToList();
            return skinTypes;
        }
    }
}
