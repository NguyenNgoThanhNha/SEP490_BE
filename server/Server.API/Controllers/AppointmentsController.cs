using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Business.Ultils;
using Server.Data.UnitOfWorks;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentsService _appointmentsService;
        private readonly AuthService _authService;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;
        private readonly UserService _userService;
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AppointmentsController(AppointmentsService appointmentsService, AuthService authService, IMapper mapper, 
            MailService mailService, UserService userService, UnitOfWorks unitOfWorks)
        {
            _appointmentsService = appointmentsService;
            _authService = authService;
            _mapper = mapper;
            _mailService = mailService;
            _userService = userService;
            _unitOfWorks = unitOfWorks;
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAppointment([FromQuery] int page = 1, int pageSize = 5)
        {
            var listAppointment = await _appointmentsService.GetAllAppointments(page, pageSize);
            if (listAppointment.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Hiện tại chưa có cuộc hẹn nào!"
                }));
            }
            return Ok(ApiResult<GetAllAppointmentResponse>.Succeed(new GetAllAppointmentResponse()
            {
                message = "Đặt lịch hẹn thành công!",
                data = listAppointment.data,
                pagination = listAppointment.pagination
            }));
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetByAppointmentId([FromRoute] int id)
        {
            var appointmentsModel = await _appointmentsService.GetAppointmentsById(id);
            if (appointmentsModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy cuộc hẹn!"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đặt lịch hẹn thành công!",
                data = _mapper.Map<AppointmentsDTO>(appointmentsModel)
            }));
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] ApointmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var customer = await _userService.GetCustomerById(request.UserId);

            // Tạo appointments và lấy order tương ứng
            var appointmentsList = await _appointmentsService.CreateAppointments(customer.UserId, request);
            if (appointmentsList == null || !appointmentsList.Any())
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi tạo cuộc hẹn!"
                }));
            }

            // Lấy order dựa trên OrderId của một appointment
            var firstAppointment = appointmentsList.First();
            var order = await _unitOfWorks.OrderRepository.FirstOrDefaultAsync(o => o.OrderId == firstAppointment.OrderId);
            if (order == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy đơn hàng!"
                }));
            }

            // Chuẩn bị thông tin cho email
            var customerName = customer.FullName ?? "Customer";
            var appointmentDate = firstAppointment.AppointmentsTime.ToString("dd/MM/yyyy");
            var appointmentTime = firstAppointment.AppointmentsTime.ToString(@"hh\:mm tt");
            var servicesDetails = string.Join("", appointmentsList.Select(a =>
            {
                var service = _unitOfWorks.ServiceRepository.FirstOrDefaultAsync(s => s.ServiceId == a.ServiceId).Result;
                // Dùng định dạng tiền tệ mà không cần văn hóa cụ thể, thay thế "vi-VN" bằng "VND"
                return $"<li>{service.Name} - {service.Price.ToString("C0", System.Globalization.CultureInfo.InvariantCulture)}</li>";
            }));

            var mailData = new MailData()
            {
                EmailToId = customer.Email,
                EmailToName = customerName,
                EmailSubject = "Order & Appointment Confirmation",
                EmailBody = $@"
                <div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                    <h2 style=""text-align: center; color: #3498db; font-weight: bold;"">Order & Appointment Confirmation</h2>
                    <p style=""font-size: 16px; color: #555;"">Dear {customerName},</p>
                    <p style=""font-size: 16px; color: #555;"">
                        Thank you for placing an order with us. Below are the details of your order and appointments:
                    </p>
                    <p style=""font-size: 16px; color: #555;""><strong>Order Code:</strong> {order.OrderCode}</p>
                    <p style=""font-size: 16px; color: #555;"">
                        <strong>Total Amount:</strong> {order.TotalAmount.ToString("C0", System.Globalization.CultureInfo.InvariantCulture)} VND
                    </p>

                    <p style=""font-size: 16px; color: #555;""><strong>Appointment Date:</strong> {appointmentDate}</p>
                    <p style=""font-size: 16px; color: #555;""><strong>Appointment Time:</strong> {appointmentTime}</p>
                    <p style=""font-size: 16px; color: #555;""><strong>Services:</strong></p>
                    <ul style=""font-size: 16px; color: #555; list-style-type: none; padding: 0;"">
                        {servicesDetails}
                    </ul>
                    <p style=""font-size: 16px; color: #555;"">If you have any questions or need to reschedule, please feel free to contact us.</p>
                    <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
                </div>"
            };

            // Gửi email thông báo
            _ = Task.Run(async () =>
            {
                var emailResult = await _mailService.SendEmailAsync(mailData, false);
                if (!emailResult)
                {
                    Console.WriteLine("Không gửi được email xác nhận.");
                }
            });

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã tạo cuộc hẹn và đơn hàng thành công! Email xác nhận đã được gửi.",
                data = order.OrderId
            }));
        }



        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAppointment([FromRoute] int id, [FromBody] AppointmentUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            // Lấy token từ header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            // Chia tách token
            var tokenValue = token.ToString().Split(' ')[1];
            // accessUser
            var currentUser = await _authService.GetUserInToken(tokenValue);

            if (currentUser == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy thông tin khách hàng!"
                }));
            }

            var appointmentsExist = await _appointmentsService.GetAppointmentsById(id);
            if (appointmentsExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy cuộc hẹn!"
                }));
            }

            var appointmentsModel = await _appointmentsService.UpdateAppointments(appointmentsExist, request);
            if (appointmentsModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi cập nhật lịch hẹn!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cập nhật cuộc hẹn thành công!",
                data = _mapper.Map<AppointmentsDTO>(appointmentsModel)
            }));
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeleteAppointment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            var appointmentsExist = await _appointmentsService.GetAppointmentsById(id);
            if (appointmentsExist.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy cuộc hẹn!"
                }));
            }

            var appointmentsModel = await _appointmentsService.DeleteAppointments(appointmentsExist);
            if (appointmentsModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Lỗi khi xóa cuộc hẹn!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Xóa cuộc hẹn thành công!",
                data = _mapper.Map<AppointmentsDTO>(appointmentsModel)
            }));
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPut("cancel-booking/{id}")]
        public async Task<IActionResult> CancleBooking([FromRoute] int id)
        {
            var appointment = await _appointmentsService.GetAppointmentsById(id);
            if (appointment == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không tìm thấy lịch hẹn!"
                }));
            }

            var cancelBooking = await _appointmentsService.CancelBookingAppointment(appointment);
            if (cancelBooking == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Không thể hủy lịch hẹn!"
                }));
            }

            var appointmentDate = appointment.AppointmentsTime.ToString("dd/MM/yyyy");
            var appointmentTime = appointment.AppointmentsTime.ToString(@"hh\:mm");
            // Gửi email xác nhận hủy booking
            var mailData = new MailData
            {
                EmailToId = appointment.Customer.Email,
                EmailToName = appointment.Customer.FullName,
                EmailSubject = "Booking Cancellation Confirmation",
                EmailBody = $@"
        <div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
            <h2 style=""text-align: center; color: #e74c3c; font-weight: bold;"">Booking Cancellation</h2>
            <p style=""font-size: 16px; color: #555;"">Dear {appointment.Customer.FullName},</p>
            <p style=""font-size: 16px; color: #555;"">We are writing to confirm that your booking has been successfully canceled.</p>
            <ul style=""font-size: 16px; color: #555; list-style-type: none; padding: 0;"">
                <li><strong>Service:</strong> {appointment.Service.Name}</li>
                <li><strong>Date:</strong> {appointmentDate}</li>
                <li><strong>Time:</strong> {appointmentTime}</li>
            </ul>
            <p style=""font-size: 16px; color: #555;"">If this cancellation was a mistake, please contact us as soon as possible to reschedule.</p>
            <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
        </div>"
            };

            _ = Task.Run(async () =>
            {
                var emailResult = await _mailService.SendEmailAsync(mailData, false);
                if (!emailResult)
                {
                    Console.WriteLine("Failed to send cancellation email for appointment ID: {id}", id);
                }
            });

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Đã hủy lịch hẹn và đã gửi email xác nhận thành công!"
            }));
        }

        [HttpGet("by-branch")]
        public async Task<IActionResult> GetAppointmentsByBranch([FromQuery] AppointmentFilterRequest request)
        {
            var result = await _appointmentsService.GetAppointmentsByBranchAsync(request);

            return Ok(new
            {
                success = true,
                result = new
                {
                    message = "Lấy danh sách cuộc hẹn theo chi nhánh thành công.",
                    data = result.data,
                    pagination = result.pagination
                }
            });
        }

    }
}