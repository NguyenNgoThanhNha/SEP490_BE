using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;
using Server.Business.Ultils;
using Server.Data;
using Server.Data.UnitOfWorks;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayOsController : ControllerBase
    {
        private readonly PayOSSetting payOSSetting;
        private readonly UnitOfWorks _unitOfWorks;
        private readonly OrderDetailService _orderDetailService;

        public PayOsController(PayOSSetting payOSSetting, UnitOfWorks unitOfWorks, OrderDetailService orderDetailService)
        {
            this.payOSSetting = payOSSetting;
            _unitOfWorks = unitOfWorks;
            _orderDetailService = orderDetailService;
        }
        [HttpPost("create-payment-link")]
        public async Task<IActionResult> Create([FromBody] PayOsRequest req)
        {

            var payOS = new PayOS(payOSSetting.ClientId, payOSSetting.ApiKey, payOSSetting.ChecksumKey);

            var domain = payOSSetting.Domain;

            var paymentLinkRequest = new PaymentData(
                orderCode: int.Parse(DateTimeOffset.Now.ToString("ffffff")),
                amount: 20000,
                description: "Thanh toan don hang",
                items: [new("Mì tôm hảo hảo ly", 1, 2000)],
                returnUrl: $"{domain}/${req.returnUrl}",
                cancelUrl: $"{domain}/${req.cancelUrl}"
            );
            var response = await payOS.createPaymentLink(paymentLinkRequest);

            Response.Headers.Append("Location", response.checkoutUrl);
            return Ok(response.checkoutUrl);
        }
        
        [HttpPost("receive-webhook")]
        public async Task<IActionResult> GetResultPayOsOrder([FromBody] WebhookRequest req)
        {
            if (req.success)
            {
                var data = req.data; // Dữ liệu từ webhook request

                // Lấy OrderCode từ vị trí thứ 3 trong description
                string[] descriptionParts = data.description.Split(' ');
                if (descriptionParts.Length < 3 || !int.TryParse(descriptionParts[2], out int orderCode))
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Định dạng mã đơn hàng trong phần mô tả không hợp lệ!"
                    }));
                }

                // Tìm Order theo OrderCode
                var order = await _unitOfWorks.OrderRepository
                    .FindByCondition(o => o.OrderCode == orderCode)
                    .FirstOrDefaultAsync();

                var deposit = order?.StatusPayment;

                if (order == null)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Không tìm thấy đơn hàng!"
                    }));
                }

                // Cập nhật trạng thái dựa trên OrderType
                if (order.OrderType == "Appointment")
                {
                    var appointments = await _unitOfWorks.AppointmentsRepository
                        .FindByCondition(a => a.OrderId == order.OrderId)
                        .ToListAsync();

                    foreach (var appointment in appointments)
                    {
                        if(deposit == OrderStatusPaymentEnum.PendingDeposit.ToString())
                        {
                            appointment.StatusPayment = OrderStatusPaymentEnum.PaidDeposit.ToString();
                        }
                        else
                        {
                            appointment.StatusPayment = OrderStatusPaymentEnum.Paid.ToString();
                        }
                        appointment.UpdatedDate = DateTime.Now;
                        _unitOfWorks.AppointmentsRepository.Update(appointment);
                    }

                    await _unitOfWorks.AppointmentsRepository.Commit();
                }
                else if (order.OrderType == "Product")
                {
                    var orderDetails = await _unitOfWorks.OrderDetailRepository
                        .FindByCondition(od => od.OrderId == order.OrderId)
                        .ToListAsync();

                    foreach (var orderDetail in orderDetails)
                    {
                        if(deposit == OrderStatusPaymentEnum.PendingDeposit.ToString())
                        {
                            orderDetail.StatusPayment = OrderStatusPaymentEnum.PaidDeposit.ToString();
                        }
                        else
                        {
                            orderDetail.StatusPayment = OrderStatusPaymentEnum.Paid.ToString();
                        }
                        orderDetail.UpdatedDate = DateTime.Now;
                        _unitOfWorks.OrderDetailRepository.Update(orderDetail);
                    }

                    await _unitOfWorks.OrderDetailRepository.Commit();
                }

                // Kiểm tra và cập nhật ghi chú (Notes) của Order
                var percentPaidMatch = Regex.Match(order.Note ?? string.Empty, @"Đặt cọc (\d+)%");
                if (percentPaidMatch.Success && int.TryParse(percentPaidMatch.Groups[1].Value, out int percent))
                {
                    order.Note = $"Đã thanh toán thành công {percent}%";
                }
                else
                {
                    order.Note = "Đã thanh toán thành công";
                }

                // Cập nhật trạng thái Order
                if (order.StatusPayment == OrderStatusPaymentEnum.PendingDeposit.ToString())
                {
                    order.StatusPayment = OrderStatusPaymentEnum.PaidDeposit.ToString();
                }
                else
                {
                    order.StatusPayment = OrderStatusPaymentEnum.Paid.ToString();
                }
                order.UpdatedDate = DateTime.Now;

                _unitOfWorks.OrderRepository.Update(order);
                var result = await _unitOfWorks.OrderRepository.Commit();

                if (result > 0)
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                    {
                        message = $"Thanh toán thành công cho mã đơn hàng: {order.OrderCode}"
                    }));
                }
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lỗi khi cập nhật trạng thái đơn hàng."
            }));
        }

    }
}
