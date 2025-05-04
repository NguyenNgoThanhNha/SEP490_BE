using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class SkinHealthService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public SkinHealthService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }
    
    public async Task<List<SkinHealthModel>> GetSkinHealthDataAsync(int userId)
    {
        // Lấy danh sách SkinHealth của user
        var skinHealthData = await _unitOfWorks.SkinHealthRepository
            .FindByCondition(x => x.UserId == userId)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync() ?? new List<SkinHealth>();

        // Lấy tất cả ảnh liên quan đến các SkinHealth của user
        var skinHealthIds = skinHealthData.Select(sh => sh.SkinHealthId).ToList();

        var skinHealthImages = await _unitOfWorks.SkinHealthImageRepository
            .FindByCondition(shi => skinHealthIds.Contains(shi.SkinHealthId))
            .ToListAsync();

        // Map dữ liệu sang SkinHealthModel
        var result = _mapper.Map<List<SkinHealthModel>>(skinHealthData);

        // Gán ảnh tương ứng vào mỗi model
        foreach (var item in result)
        {
            item.Images = skinHealthImages
                .Where(img => img.SkinHealthId == item.SkinHealthId)
                .Select(x =>x.ImageUrl)
                .FirstOrDefault();
        }

        return result;
    }

}