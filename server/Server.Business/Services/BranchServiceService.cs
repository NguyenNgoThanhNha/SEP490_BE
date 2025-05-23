﻿using AutoMapper;
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
    public class BranchServiceService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public BranchServiceService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<List<BranchServiceDto>> GetAllAsync()
        {
            var entities = await _unitOfWorks.Branch_ServiceRepository
                .GetAll()
                .Include(x => x.Branch)
                .Include(x => x.Service)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return _mapper.Map<List<BranchServiceDto>>(entities);
        }

        public async Task<GetAllBranchServicePaginationResponse> GetAllServiceInBranchAsync(int branchId, int page, int pageSize)
        {
            var query = _unitOfWorks.Branch_ServiceRepository
                .FindByCondition(x => x.BranchId == branchId)
                .Include(x => x.Branch)
                .Include(x => x.Service)
                .ThenInclude(s => s.ServiceImages);;

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<BranchServiceDto>>(items);
            
            for (int i = 0; i < items.Count; i++)
            {
                var serviceImages = items[i].Service.ServiceImages;
                dtos[i].Service.images = serviceImages
                    .Select(img => img.image) // giả sử image là chuỗi string
                    .ToArray();
            }

            return new GetAllBranchServicePaginationResponse
            {
                message = "Lấy danh sách dịch vụ trong chi nhánh thành công",
                data = dtos,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }

        public async Task<BranchServiceDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWorks.Branch_ServiceRepository
                .GetAll()
                .Include(x => x.Branch)
                .Include(x => x.Service)
                .FirstOrDefaultAsync(x => x.Id == id);

            return entity == null ? null : _mapper.Map<BranchServiceDto>(entity);
        }

        public async Task<BranchServiceDto?> CreateAsync(CreateBranchServiceDto dto)
        {
            if (dto.BranchId <= 0 || dto.ServiceId <= 0)
                return null;

            var branch = await _unitOfWorks.BranchRepository.GetByIdAsync(dto.BranchId);
            var service = await _unitOfWorks.ServiceRepository.GetByIdAsync(dto.ServiceId);
            if (branch == null || service == null)
                return null;

            // ✅ Check trùng
            var exists = await _unitOfWorks.Branch_ServiceRepository
                .FindByCondition(x => x.BranchId == dto.BranchId && x.ServiceId == dto.ServiceId)
                .AnyAsync();

            if (exists) return null;

            var entity = _mapper.Map<Branch_Service>(dto);
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedDate = DateTime.Now;

            await _unitOfWorks.Branch_ServiceRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<BranchServiceDto>(entity);
        }


        public async Task<bool> UpdateAsync(int id, UpdateBranchServiceDto dto)
        {
            var entity = await _unitOfWorks.Branch_ServiceRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = dto.Status;
            entity.UpdatedDate = DateTime.Now;

            _unitOfWorks.Branch_ServiceRepository.Update(entity);
            await _unitOfWorks.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWorks.Branch_ServiceRepository.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWorks.Branch_ServiceRepository.Remove(entity.Id);
            await _unitOfWorks.SaveChangesAsync();
            return true;
        }

    }
}
