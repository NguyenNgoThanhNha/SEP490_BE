﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Data.Entities;
using System.Linq.Expressions;

namespace Server.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PurcharseController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly OrderDetailService _orderDetailService;

        public PurcharseController(OrderService orderService, OrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int? orderCode,
            string? customerName,
            string? voucherCode,
            int pageIndex = 0,
            int pageSize = 10)
        {
            try
            {
                Expression<Func<Order, bool>> filter = c => (orderCode == null || c.OrderCode == orderCode)
                    && (string.IsNullOrEmpty(customerName) || c.Customer.FullName.ToLower().Contains(customerName.ToLower()))
                    && (string.IsNullOrEmpty(voucherCode) || c.Voucher.Code.ToLower().Contains(voucherCode.ToLower()));

                var response = await _orderService.GetListAsync(
                    filter: filter,
                    includeProperties: "Customer,Voucher,OrderDetails",
                    pageIndex: pageIndex,
                    pageSize: pageSize);

                if (response == null || response.Data == null || !response.Data.Any())
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy đơn hàng."
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã lấy đơn hàng thành công.",
                    data = response
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                }));
            }
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrderAsync(CUOrderDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Xác thực không thành công.",
                        data = errors
                    }));
                }

                var orderResult = await _orderService.CreateOrderAsync(model);
                if (!orderResult.Success)
                    return BadRequest(orderResult);

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã tạo đơn hàng thành công.",
                    data = orderResult.Result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                }));
            }
        }

        [HttpPost("create-order-detail")]
        public async Task<IActionResult> CreateOrderDetailAsync(CUOrderDetailDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Xác thực không thành công.",
                        data = errors
                    }));
                }

                var orderDetailResult = await _orderDetailService.CreateOrderDetailAsync(model);
                if (!orderDetailResult.Success)
                    return BadRequest(orderDetailResult);

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã tạo chi tiết đơn hàng thành công.",
                    data = orderDetailResult.Result
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                }));
            }
        }
        
        [HttpGet("get-order-detail-by-branchId/{branchId}")]
        public async Task<IActionResult> GetOrderDetailByBranchId(int branchId)
        {
            try
            {
                var orderDetails = await _orderDetailService.GetOrderDetailByBranchId(branchId);
                if (orderDetails == null || !orderDetails.Any())
                    return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Không tìm thấy chi tiết đơn hàng."
                    }));

                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Đã lấy chi tiết đơn hàng thành công.",
                    data = orderDetails
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                }));
            }
        }
        
    }
}
