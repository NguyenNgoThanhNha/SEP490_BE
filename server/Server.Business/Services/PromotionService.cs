using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class PromotionService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public PromotionService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }

    public async Task<GetAllPromotionResponse> GetAllPromotion(int page = 1, int pageSize = 5)
    {
        var listPromotion = await _unitOfWorks.PromotionRepository.FindByCondition(x => x.Status == "Active").OrderByDescending(x => x.PromotionId).ToListAsync();
        if (listPromotion.Equals(null))
        {
            return null;
        }
        var totalCount = listPromotion.Count();
        
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var pagedServices = listPromotion.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        
        var promotionModels = _mapper.Map<List<PromotionModel>>(pagedServices);

        return new GetAllPromotionResponse()
        {
            data = promotionModels,
            pagination = new Pagination
            {
                page = page,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }

    public async Task<PromotionModel> GetPromotionById(int id)
    {
        var promotionExist = await _unitOfWorks.PromotionRepository.FirstOrDefaultAsync(x => x.PromotionId.Equals(id));
        if (promotionExist == null)
        {
            return null;
        }

        return _mapper.Map<PromotionModel>(promotionExist);
    }

    public async Task<PromotionModel> CreatePromotion(PromotionRequest request)
    {
        var createNewPromotion = new PromotionModel()
        {
            PromotionName = request.PromotionName,
            PromotionDescription = request.PromotionDescription,
            DiscountPercent = request.DiscountPercent,
            Status = request.Status,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        var promotionEntity = await _unitOfWorks.PromotionRepository.AddAsync(_mapper.Map<Promotion>(createNewPromotion));
        var result = await _unitOfWorks.PromotionRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<PromotionModel>(promotionEntity);
        }
        return null;
    }
    
    public async Task<PromotionModel> UpdatePromotion(PromotionModel promotionModel,PromotionRequest request)
    {
        if (!request.PromotionName.Equals(null))
        {
            promotionModel.PromotionName = request.PromotionName;
        }
        if (!request.PromotionDescription.Equals(null))
        {
            promotionModel.PromotionDescription = request.PromotionDescription;
        }
        if (!request.DiscountPercent.Equals(null))
        {
            promotionModel.DiscountPercent = request.DiscountPercent;
        }
        if (!request.StartDate.Equals(null))
        {
            promotionModel.StartDate = request.StartDate;
        }
        if (!request.EndDate.Equals(null))
        {
            promotionModel.EndDate = request.EndDate;
        }
        
        var promotionEntity = _unitOfWorks.PromotionRepository.Update(_mapper.Map<Promotion>(promotionModel));
        var result = await _unitOfWorks.PromotionRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<PromotionModel>(promotionEntity);
        }
        return null;
    }

    public async Task<PromotionModel> DeletePromotion(PromotionModel promotionModel)
    {
        promotionModel.Status = "InActive";
        var promotionEntity = _unitOfWorks.PromotionRepository.Update(_mapper.Map<Promotion>(promotionModel));
        var result = await _unitOfWorks.PromotionRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<PromotionModel>(promotionEntity);
        }

        return null;
    }
}