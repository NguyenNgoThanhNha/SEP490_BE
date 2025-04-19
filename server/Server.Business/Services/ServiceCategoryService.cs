using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class ServiceCategoryService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly CloudianryService _cloudianryService;
    private readonly IMapper _mapper;

    public ServiceCategoryService(UnitOfWorks unitOfWorks, CloudianryService cloudianryService, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _cloudianryService = cloudianryService;
        _mapper = mapper;
    }

    public async Task<ServiceCategoryModel> CreateServiceCategoryAsync(ServiceCategoryCreateUpdateDto dto)
    {
        var thumbnail = "";
        if (dto.Thumbnail != null)
        {
            var result = await _cloudianryService.UploadImageAsync(dto.Thumbnail);
            if (result != null)
            {
                thumbnail = result.SecureUrl.ToString();
            }
        }

        var serviceCategory = new ServiceCategoryModel
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = ObjectStatus.Active.ToString(),
            Thumbnail = thumbnail,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };

        var serviceCategoryCreated = await _unitOfWorks.ServiceCategoryRepository.AddAsync(_mapper.Map<ServiceCategory>(serviceCategory));
        await _unitOfWorks.ServiceCategoryRepository.Commit();

        return _mapper.Map<ServiceCategoryModel>(serviceCategoryCreated);
    }

    // Get a ServiceCategory by ID
    public async Task<ServiceCategoryModel?> GetServiceCategoryByIdAsync(int id)
    {
        var entity = await _unitOfWorks.ServiceCategoryRepository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<ServiceCategoryModel>(entity);
    }

    // Search ServiceCategories by Name
    public async Task<GetAllServiceCategoryResponse> SearchServiceCategoriesAsync(string? keyword, int page, int pageSize)
    {
        var query = _unitOfWorks.ServiceCategoryRepository.GetAll();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(sc => sc.Name.Contains(keyword));
        }

        var result = await query.OrderByDescending(x => x.ServiceCategoryId).ToListAsync();
        var totalCount = result.Count();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedServices = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var serviceCategoryModels = _mapper.Map<List<ServiceCategoryModel>>(pagedServices);

        return new GetAllServiceCategoryResponse()
        {
            data = serviceCategoryModels,
            pagination = new Pagination
            {
                page = page,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }

    // Update an existing ServiceCategory
    public async Task<ServiceCategoryModel?> UpdateServiceCategoryAsync(int id, ServiceCategoryCreateUpdateDto dto)
    {
        var existingEntity = await _unitOfWorks.ServiceCategoryRepository.GetByIdAsync(id);
        if (existingEntity == null)
        {
            return null;
        }

        var thumbnail = existingEntity.Thumbnail;
        if (dto.Thumbnail != null)
        {
            var result = await _cloudianryService.UploadImageAsync(dto.Thumbnail);
            if (result != null)
            {
                existingEntity.Thumbnail = result.SecureUrl.ToString();
            }
        }

        if (dto.Name != null)
        {
            existingEntity.Name = dto.Name;
        }
        if (dto.Description != null)
        {
            existingEntity.Description = dto.Description;
        }
        if (dto.Status != null)
        {
            existingEntity.Status = dto.Status;
        }
        existingEntity.UpdatedDate = DateTime.Now;

        _unitOfWorks.ServiceCategoryRepository.Update(existingEntity);
        await _unitOfWorks.ServiceCategoryRepository.Commit();

        return _mapper.Map<ServiceCategoryModel>(existingEntity);
    }

    // Delete a ServiceCategory
    public async Task<bool> DeleteServiceCategoryAsync(int id)
    {
        var existingEntity = await _unitOfWorks.ServiceCategoryRepository.GetByIdAsync(id);
        if (existingEntity == null)
        {
            return false;
        }

        existingEntity.Status = ObjectStatus.InActive.ToString();
        _unitOfWorks.ServiceCategoryRepository.Update(existingEntity);
        await _unitOfWorks.ServiceCategoryRepository.Commit();

        return true;
    }

    public async Task<List<ServiceCategoryModel>> GetAllServiceCategoriesAsync()
    {
        var entities = await _unitOfWorks.ServiceCategoryRepository
            .FindByCondition(x => x.Status == ObjectStatus.Active.ToString())
            .OrderByDescending(x => x.ServiceCategoryId)
            .ToListAsync();

        return _mapper.Map<List<ServiceCategoryModel>>(entities);
    }

}