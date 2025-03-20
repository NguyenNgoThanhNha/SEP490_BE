using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Dtos;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class VoucherService
{
    private readonly UnitOfWorks _unitOfWork;
    private readonly IMapper _mapper;

    public VoucherService(UnitOfWorks unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<VoucherDto>> GetAllVouchers(VoucherRequest request)
    {
        var vouchers = await _unitOfWork.VoucherRepository.FindByCondition(x =>
            (string.IsNullOrEmpty(request.Status) || x.Status == request.Status) &&
            (request.ValidFrom == DateTime.MinValue || x.ValidFrom >= request.ValidFrom) &&
            (request.ValidTo == DateTime.MinValue || x.ValidTo <= request.ValidTo)
            && x.Status == "Active")
            .ToListAsync();
        return _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
    }
    
    public async Task<IEnumerable<VoucherDto>> GetVoucherByDate(DateTime dateTime)
    {
        var vouchers = await _unitOfWork.VoucherRepository
            .FindByCondition(x => x.ValidFrom <= dateTime && x.ValidTo >= dateTime && x.Status == "Active")
            .ToListAsync();
        return _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
    }

}