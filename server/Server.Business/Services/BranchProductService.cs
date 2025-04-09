using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Dtos;
using Server.Business.Ultils;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Business.Services
{
    public class BranchProductService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;


        public BranchProductService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;           
            _mapper = mapper;           
        }

        public async Task<List<BranchProductDto>> GetAllAsync()
        {
            var entities = await _unitOfWorks.Branch_ProductRepository
                .GetAll()
                .Include(x => x.Product)
                .Include(x => x.Branch)
                .Include(x => x.Promotion)
                .ToListAsync();

            return _mapper.Map<List<BranchProductDto>>(entities);
        }
        
        public async Task<List<BranchProductDto>> GetAllProductInBranchAsync(int branchId)
        {
            var entities = await _unitOfWorks.Branch_ProductRepository
                .FindByCondition(x => x.BranchId == branchId)
                .Include(x => x.Product)
                .Include(x => x.Branch)
                .Include(x => x.Promotion)
                .ToListAsync();

            return _mapper.Map<List<BranchProductDto>>(entities);
        }


        public async Task<BranchProductDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWorks.Branch_ProductRepository.GetByIdWithIncludesAsync(id);
            return entity == null ? null : _mapper.Map<BranchProductDto>(entity);
        }

        public async Task<BranchProductDto> CreateAsync(CreateBranchProductDto dto)
        {
            var entity = _mapper.Map<Branch_Product>(dto);
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedDate = DateTime.Now;

            await _unitOfWorks.Branch_ProductRepository.AddAsync(entity);
            await _unitOfWorks.SaveChangesAsync();

            return _mapper.Map<BranchProductDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateBranchProductDto dto)
        {
            var entity = await _unitOfWorks.Branch_ProductRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = dto.Status;
            entity.StockQuantity = dto.StockQuantity;
            entity.UpdatedDate = DateTime.Now;
            await _unitOfWorks.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWorks.Branch_ProductRepository.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWorks.Branch_ProductRepository.Remove(entity.Id);
            await _unitOfWorks.SaveChangesAsync();
            return true;
        }

        public async Task<Product?> CheckProductExist(int productId)
        {
            return await _unitOfWorks.ProductRepository.GetByIdAsync(productId);
        }

        public async Task<Branch?> CheckBranchExist(int branchId)
        {
            return await _unitOfWorks.BranchRepository.GetByIdAsync(branchId);
        }


    }
}
