using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Server.Business.Commons.Request;
using Server.Business.Dtos;
using Server.Business.Exceptions;
using Server.Data;
using Server.Data.Entities;
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
                && x.Status == "Active" && x.RemainQuantity > 0)
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

    public async Task<bool> ExchangePointToVoucher(int point, int voucherId, int userId)
    {
        var user = await _unitOfWork.UserRepository
                       .FirstOrDefaultAsync(x => x.UserId == userId)
                   ?? throw new BadRequestException("User not found!");

        var voucher = await _unitOfWork.VoucherRepository
                          .FirstOrDefaultAsync(x => x.VoucherId == voucherId)
                      ?? throw new BadRequestException("Voucher not found!");

        if (point <= 0)
        {
            throw new BadRequestException("Point of user must be larger than 0");
        }

        if (voucher.RequirePoint > point)
        {
            throw new BadRequestException($"Point of user must be larger than {voucher.RequirePoint} to exchange");
        }

        // Kiểm tra số điểm thực tế của user
        if (user.BonusPoint < voucher.RequirePoint)
        {
            throw new BadRequestException("User does not have enough points to exchange for this voucher");
        }

        // Kiểm tra số lượng voucher còn lại
        if (voucher.RemainQuantity <= 0)
        {
            throw new BadRequestException("This voucher is out of stock");
        }

        var userVoucher = await _unitOfWork.UserVoucherRepository
            .FirstOrDefaultAsync(x => x.UserId == user.UserId
                                      && x.VoucherId == voucher.VoucherId
                                      && x.Status == ObjectStatus.Active.ToString());

        if (userVoucher == null)
        {
            var newUserVoucher = new UserVoucher()
            {
                UserId = userId,
                VoucherId = voucherId,
                Status = ObjectStatus.Active.ToString(),
                Quantity = 1,
                CreatedDate = DateTime.Now
            };
            await _unitOfWork.UserVoucherRepository.AddAsync(newUserVoucher);
        }
        else
        {
            userVoucher.Quantity += 1;
            userVoucher.UpdatedDate = DateTime.Now;
            _unitOfWork.UserVoucherRepository.Update(userVoucher);
        }

        // Cập nhật điểm của user
        user.BonusPoint -= voucher.RequirePoint;
        _unitOfWork.UserRepository.Update(user);

        // Giảm số lượng voucher còn lại
        voucher.RemainQuantity -= 1;
        if (voucher.RemainQuantity < 0)
        {
            throw new BadRequestException("Invalid voucher quantity after update");
        }

        _unitOfWork.VoucherRepository.Update(voucher);

        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<List<UserVoucher>> GetListVoucherOfUser(int userId)
    {
        var user = await _unitOfWork.UserRepository
                       .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == ObjectStatus.Active.ToString())
                   ?? throw new BadRequestException("User not found!");
        var listUserVoucher = await _unitOfWork.UserVoucherRepository
            .FindByCondition(x => x.UserId == user.UserId)
            .Include(x => x.Voucher)
            .ToListAsync();

        return _mapper.Map<List<UserVoucher>>(listUserVoucher);
    }
}