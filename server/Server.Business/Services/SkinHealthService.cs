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
    
    public async Task<object> GetSkinHealthDataAsync(int userId)
    {
        var skinHealthData = await _unitOfWorks.SkinHealthRepository
            .FindByCondition(x => x.UserId==userId)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync() ?? new List<SkinHealth>();
        return _mapper.Map<List<SkinHealthModel>>(skinHealthData);
    }
}