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
            // Check if the request indicates a successful payment
            /*if (req.success)
            {
                var data = req.data; // Assuming Data is of type DataObject

                // Log or handle the description as needed
                Console.WriteLine(data.description);

                // Extract order detail code from the description
                var orderDetailCode = data.description.Split(' ')[1];

                // Combine fetching and updating order details
                if (!int.TryParse(orderDetailCode, out int code))
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Invalid order detail code format!"
                    }));
                }

                var orderDetail = await _unitOfWorks.OrderDetailRepository
                    .FindByCondition(x => x.OrderDetailCode == code)
                    .Include(x => x.OrderInfo)
                    .FirstOrDefaultAsync();

                var orderInfo = orderDetail?.OrderInfo;
                if (orderInfo != null)
                {
                    var driver = await _unitOfWorks.UserRepository.FindByCondition(x => x.UserId == orderInfo.DriverId)
                        .FirstOrDefaultAsync();
                    if (driver != null)
                    {
                        driver.AccountBalance = orderInfo.DriverAmount.ToString();
                        _unitOfWorks.UserRepository.Update(driver);
                        await _unitOfWorks.UserRepository.Commit();
                    }
                }
                
                if (orderDetail == null)
                {
                    return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                    {
                        message = "Order detail not found!"
                    }));
                }

                // Update order properties
                orderDetail.PaymentMethod = "Payment via PayOS";
                orderDetail.Status = StatusEnums.Paid.ToString();
                orderDetail.reference = data.reference;
                orderDetail.transactionDateTime = data.transactionDateTime;

                // Update order details in the database
                _unitOfWorks.OrderDetailRepository.Update(orderDetail);
                var result = await _unitOfWorks.OrderDetailRepository.Commit();

                if (result > 0)
                {
                    return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                    {
                        message = $"Payment successful for order code: {orderDetail.OrderDetailCode}"
                    }));
                }
            }*/

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Error updating status for order detail."
            }));
        }
    }
}
