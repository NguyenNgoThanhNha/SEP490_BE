using AutoMapper;
using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class RoutineService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public RoutineService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }
    
    public async Task<SkincareRoutine> GetSkincareRoutineDetails(int id)
    {
        var routine =  await _unitOfWorks.SkincareRoutineRepository.FindByCondition(x => x.SkincareRoutineId == id)
            .Include(x => x.ProductRoutines)
            .ThenInclude(x => x.Products)
            .Include(x => x.ServiceRoutines)
            .ThenInclude(x => x.Service)
            .FirstOrDefaultAsync();
        if (routine == null) return null;
        return routine;
    }
}