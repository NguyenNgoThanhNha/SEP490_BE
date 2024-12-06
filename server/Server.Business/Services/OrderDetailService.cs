﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services
{
    public class OrderDetailService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public OrderDetailService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<ApiResult<OrderDetail>> CreateOrderDetailAsync(CUOrderDetailDto model)
        {
            // Kiểm tra sự tồn tại của Order
            var orderExists = await _unitOfWorks.Orders
                .AnyAsync(x => x.OrderId == model.OrderId);
            if (!orderExists)
            {
                return ApiResult<OrderDetail>.Error(null, "Order not found");
            }

            // Kiểm tra sự tồn tại của Product với trạng thái Active
            var productExists = await _unitOfWorks.Products
                .AnyAsync(x => x.ProductId == model.ProductId && x.Status == "Active");
            if (!productExists)
            {
                return ApiResult<OrderDetail>.Error(null, "Product not found");
            }

            // Kiểm tra sự tồn tại của Service với trạng thái Active
            var serviceExists = await _unitOfWorks.Services
                .AnyAsync(x => x.ServiceId == model.ServiceId && x.Status == "Active");
            if (!serviceExists)
            {
                return ApiResult<OrderDetail>.Error(null, "Service not found");
            }

            // Tạo OrderDetail từ DTO
            var orderDetail = _mapper.Map<OrderDetail>(model);

            try
            {
                // Thêm vào cơ sở dữ liệu qua UnitOfWork
                await _unitOfWorks.OrderDetails.AddAsync(orderDetail);
                await _unitOfWorks.Commit();

                // Lấy lại thông tin với các bảng liên kết
                var orderDetailWithIncludes = await _unitOfWorks.OrderDetails
                    .Include(od => od.Product)
                    .Include(od => od.Service)
                    .Include(od => od.Order)
                    .FirstOrDefaultAsync(od => od.OrderDetailId == orderDetail.OrderDetailId);

                if (orderDetailWithIncludes == null)
                {
                    return ApiResult<OrderDetail>.Error(null, "Failed to retrieve the created order detail.");
                }

                // Map sang DTO để trả về thông tin
                var orderDetailDto = _mapper.Map<OrderDetail>(orderDetailWithIncludes);
                return ApiResult<OrderDetail>.Succeed(orderDetailDto);
            }
            catch (Exception ex)
            {
                return ApiResult<OrderDetail>.Error(null, ex.Message);
            }
        }

    }
}
