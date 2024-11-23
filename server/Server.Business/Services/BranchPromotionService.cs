using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class BranchPromotionService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public BranchPromotionService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }

    public async Task<GetAllBranchPromotionResponse> GetAllBranchPromotion(int page = 1, int pageSize = 5)
    {
        var listBranchPromotion = await _unitOfWorks.BranchPromotionRepository.GetAll().Include(x =>x.Promotion).Include(x=>x.Branch).OrderByDescending(x =>x.Id).ToListAsync();
        if (listBranchPromotion.Equals(null))
        {
            return null;
        }
        var totalCount = listBranchPromotion.Count();
        
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var pagedServices = listBranchPromotion.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        
        var branchPromotionModels = _mapper.Map<List<BranchPromotionModel>>(pagedServices);

        return new GetAllBranchPromotionResponse()
        {
            data = branchPromotionModels,
            pagination = new Pagination
            {
                page = page,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }

    public async Task<GetAllBranchPromotionResponse> GetAllPromotionOfBranch(int branchId, int page = 1, int pageSize = 5)
    {
        var listBranchPromotion = await _unitOfWorks.BranchPromotionRepository
            .FindByCondition(x => x.BranchId.Equals(branchId)).Include(x =>x.Promotion)
            .Include(x=>x.Branch).OrderByDescending(x => x.Id).ToListAsync();
        var totalCount = listBranchPromotion.Count();
        
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var pagedServices = listBranchPromotion.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        
        var branchPromotionModels = _mapper.Map<List<BranchPromotionModel>>(pagedServices);

        return new GetAllBranchPromotionResponse()
        {
            data = branchPromotionModels,
            pagination = new Pagination
            {
                page = page,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }

    public async Task<BranchPromotionModel> GetBranchPromotionById(int id)
    {
        var branchPromotion = await _unitOfWorks.BranchPromotionRepository.FindByCondition(x => x.Id.Equals(id))
            .Include(x => x.Branch)
            .Include(x => x.Promotion).FirstOrDefaultAsync();
        return _mapper.Map<BranchPromotionModel>(branchPromotion);
    }

    public async Task<BranchPromotionModel> CreateBranchPromotion(BranchPromotionRequest request)
    {
        var branch = await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId.Equals(request.BranchId));
        var promotion =
            await _unitOfWorks.PromotionRepository.FirstOrDefaultAsync(x => x.PromotionId.Equals(request.PromotionId));
        if (branch == null || promotion == null)
        {
            throw new BadRequestException("Branch or Promotion not found!");
        }
        var branchPromotionModel = new BranchPromotionModel()
        {
            BranchId = request.BranchId,
            PromotionId = request.PromotionId,
            Status = !request.Status.Equals(null) ? request.Status : "Active",
            StockQuantity = request.StockQuantity
        };
        var branchPromotionEntity =
            await _unitOfWorks.BranchPromotionRepository.AddAsync(_mapper.Map<Branch_Promotion>(branchPromotionModel));
        var result = await _unitOfWorks.BranchPromotionRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<BranchPromotionModel>(branchPromotionEntity);
        }
        return null;
    }
    
    public async Task<BranchPromotionModel> UpdateBranchPromotion(BranchPromotionModel branchPromotionModel, BranchPromotionRequest request)
    {
        var branch = await _unitOfWorks.BranchRepository.FirstOrDefaultAsync(x => x.BranchId.Equals(request.BranchId));
        var promotion =
            await _unitOfWorks.PromotionRepository.FirstOrDefaultAsync(x => x.PromotionId.Equals(request.PromotionId));
        if (branch == null || promotion == null)
        {
            throw new BadRequestException("Branch or Promotion not found!");
        }
        if (!request.BranchId.Equals(null))
        {
            branchPromotionModel.BranchId = request.BranchId;
        }
        if (!request.PromotionId.Equals(null))
        {
            branchPromotionModel.PromotionId = request.PromotionId;
        }
        if (!request.StockQuantity.Equals(null))
        {
            branchPromotionModel.StockQuantity = request.StockQuantity;
        }
        if (!request.Status.Equals(null))
        {
            branchPromotionModel.Status = request.Status;
        }
        
        var branchPromotionEntity =
            _unitOfWorks.BranchPromotionRepository.Update(_mapper.Map<Branch_Promotion>(branchPromotionModel));
        var result = await _unitOfWorks.BranchPromotionRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<BranchPromotionModel>(branchPromotionEntity);
        }
        return null;
    }

    public async Task<BranchPromotionModel> DeleteBranchPromotion(BranchPromotionModel branchPromotionModel)
    {
        branchPromotionModel.Status = "InActive";
        var branchPromotionUpdate =
            _unitOfWorks.BranchPromotionRepository.Update(_mapper.Map<Branch_Promotion>(branchPromotionModel));
        var result = await _unitOfWorks.BranchPromotionRepository.Commit();
        if (result > 0)
        {
            return _mapper.Map<BranchPromotionModel>(branchPromotionUpdate);
        }

        return null;
    }
}