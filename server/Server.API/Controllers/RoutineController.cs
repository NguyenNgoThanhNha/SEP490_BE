using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;
using Server.Business.Ultils;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutineController : ControllerBase
    {
        private readonly RoutineService _routineService;
        private readonly UserService _userService;
        private readonly OrderService _orderService;

        public RoutineController(RoutineService routineService, UserService userService, OrderService orderService)
        {
            _routineService = routineService;
            _userService = userService;
            _orderService = orderService;
        }

        [HttpGet("get-list-skincare-routines")]
        public async Task<IActionResult> GetListSkincareRoutines()
        {
            var routines = await _routineService.GetListSkincareRoutine();
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy danh sách liệu trình thành công!",
                data = routines
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSkincareRoutineDetails(int id)
        {
            var routine = await _routineService.GetSkincareRoutineDetails(id);
            if (routine == null)
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy liệu trình!"
                }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết liệu trình!",
                data = routine
            }));
        }

        [HttpGet("get-list-skincare-routines-step/{routineId}")]
        public async Task<IActionResult> GetListSkincareRoutineSteps(int routineId)
        {
            var steps = await _routineService.GetListSkincareRoutineStepByRoutineId(routineId);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công các bước của liệu trình!",
                data = steps
            }));
        }

        [HttpGet("get-list-routine-by/{userId}/{status}")]
        public async Task<IActionResult> GetListSkincareRoutineByUserId(int userId, string status)
        {
            var routines = await _routineService.GetListSkincareRoutineByUserId(userId, status);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công liệu trình!",
                data = routines
            }));
        }

        [HttpPost("book-compo-skin-care-routine")]
        public async Task<IActionResult> BookCompoSkinCareRoutine(BookCompoSkinCareRoutineRequest request)
        {
            var result = await _routineService.BookCompoSkinCareRoutine(request);
            if (result == 0)
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Đặt liệu trình thất bại!"
                }));

            // Lấy thông tin khách hàng
            var customer = await _userService.GetCustomerById(request.UserId);
            if (customer == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }

            var order = await _orderService.GetDetailOrder(result, customer.UserId);

            // Lấy thông tin liệu trình đã đặt (ví dụ: tên liệu trình, thời gian, giá, v.v.)
            var routineInfo = await _routineService.GetSkincareRoutineDetails(order.data.RoutineId ?? 1);
            if (routineInfo == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin liệu trình!"
                }));
            }

            var mailData = new MailData()
            {
                EmailToId = customer.Email,
                EmailToName = customer.FullName ?? "Khách hàng",
                EmailSubject = "Xác nhận đặt liệu trình chăm sóc da",
                EmailBody = $@"
        <div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
            <h2 style=""text-align: center; color: #e67e22; font-weight: bold;"">Xác nhận đặt liệu trình chăm sóc da</h2>
            <p style=""font-size: 16px; color: #555;"">Xin chào {customer.FullName},</p>
            <p style=""font-size: 16px; color: #555;"">
                Cảm ơn bạn đã đặt liệu trình tại spa của chúng tôi. Dưới đây là thông tin chi tiết:
            </p>
            <p style=""font-size: 16px; color: #555;""><strong>Tên liệu trình:</strong> {routineInfo.Name}</p>
            <p style=""font-size: 16px; color: #555;""><strong>Thời gian đặt:</strong> {routineInfo.CreatedDate:dd/MM/yyyy HH:mm}</p>
                    <p style=""font-size: 16px; color: #555;"">
                        <strong>Total Amount:</strong> {order.data.TotalAmount.ToString("C0", System.Globalization.CultureInfo.InvariantCulture)} VND
                    </p>
            <p style=""font-size: 16px; color: #555;"">Nếu bạn có bất kỳ câu hỏi nào hoặc muốn thay đổi lịch, vui lòng liên hệ với chúng tôi.</p>
            <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
        </div>"
            };

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đặt liệu trình thành công!",
                data = result
            }));
        }

        [HttpGet("tracking-user-routine/{routineId}/{userId}")]
        public async Task<IActionResult> TrackingUserRoutine(int routineId, int userId)
        {
            var routine = await _routineService.TrackingUserRoutineByRoutineId(routineId, userId);
            if (routine == null)
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy liệu trình!"
                }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết lịệu trình!",
                data = routine
            }));
        }

        [HttpGet("get-routine-of-user-newest/{userId}")]
        public async Task<IActionResult> GetRoutineOfUserNewest(int userId)
        {
            var routine = await _routineService.GetInfoRoutineOfUserNew(userId);
            if (routine == null)
                return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
                {
                    message = "Không tìm thấy liệu trình!",
                    data = null
                }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết liệu trình!",
                data = routine
            }));
        }

        [HttpGet("get-detail-order-routine/{orderId}/{userId}")]
        public async Task<IActionResult> GetDetailOrderRoutine(int orderId, int userId)
        {
            var routine = await _routineService.GetDetailOrderRoutine(userId, orderId);
            if (routine == null)
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy đơn hàng"
                }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công chi tiết đơn hàng!",
                data = routine
            }));
        }

        [HttpGet("get-list-branches-by-routine/{routineId}")]
        public async Task<IActionResult> GetListBranchesByRoutine(int routineId)
        {
            var branches = await _routineService.GetListBranchByRoutineId(routineId);
            if (branches == null)
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy chi nhánh nào!"
                }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công danh sách chi nhánh!",
                data = branches
            }));
        }

        [Authorize]
        [HttpGet("get-list-service-and-product-rcm/{userId}")]
        public async Task<IActionResult> GetListServiceAndProductRcm(int userId)
        {
            var result = await _routineService.GetListServiceAndProductRcm(userId);
            if (result == null)
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy dịch vụ và sản phẩm nào!"
                }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy thành công danh sách dịch vụ và sản phẩm!",
                data = result
            }));
        }

        [HttpPatch("update-start-time-of-routine")]
        public async Task<IActionResult> UpdateStartTimeOfRoutine(UpdateStartTimeOfRoutineRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var result =
                await _routineService.UpdateStartTimeOfRoutine(request.OrderId, request.FromStep, request.StartTime);
            if (result == null)
                return NotFound(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy liệu trình!"
                }));
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cập nhật thời gian bắt đầu liệu trình thành công!",
                data = result
            }));
        }
    }
}