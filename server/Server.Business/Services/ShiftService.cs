using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class ShiftService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;

    public ShiftService(UnitOfWorks unitOfWorks, IMapper mapper)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<ShiftModel>> GetShiftsAsync()
    {
        var listShifts = await _unitOfWorks.ShiftRepository.GetAll().ToListAsync();
        return _mapper.Map<List<ShiftModel>>(listShifts);
    }
    
    public async Task<Shifts> GetShiftByIdAsync(int id)
    {
        return await _unitOfWorks.ShiftRepository.GetByIdAsync(id);
    }
}