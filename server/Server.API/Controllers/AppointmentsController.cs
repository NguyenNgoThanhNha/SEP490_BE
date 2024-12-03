using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Services;
using Server.Business.Ultils;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentsService _appointmentsService;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;

        public AppointmentsController(AppointmentsService appointmentsService, IMapper mapper, MailService mailService)
        {
            _appointmentsService = appointmentsService;
            _mapper = mapper;
            _mailService = mailService;
        }

        [Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAppointment([FromQuery] int page = 1, int pageSize = 5)
        {
            var listAppointment = await _appointmentsService.GetAllAppointments(page, pageSize);
            if (listAppointment.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Currently, there is no appointments!"
                }));
            }
            return Ok(ApiResult<GetAllAppointmentResponse>.Succeed(new GetAllAppointmentResponse()
            {
                message = "Get appointments successfully!",
                data = listAppointment.data,
                pagination = listAppointment.pagination
            }));
        }

        [Authorize]
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetByAppointmentId([FromRoute] int id)
        {
            var appointmentsModel = await _appointmentsService.GetAppointmentsById(id);
            if (appointmentsModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Appointment not found!"
                }));
            }
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Get appointments successfully!",
                data = _mapper.Map<AppointmentsDTO>(appointmentsModel)
            }));
        }

        [Authorize]
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

            var appointmentsModel = await _appointmentsService.CreateAppointments(request);
            if (appointmentsModel == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in create appointments!"
                }));
            }

            var appointmentExist = await _appointmentsService.GetAppointmentsById(appointmentsModel.AppointmentsId);

            // Thông tin chi tiết về lịch hẹn
            var customerName = appointmentExist.Customer.FullName ?? "Customer";
            var appointmentDate = appointmentExist.AppointmentsTime.ToString("dd/MM/yyyy");
            var appointmentTime = appointmentExist.AppointmentsTime.ToString(@"hh\:mm");
            var appointmentService = appointmentExist.Service.Name ?? "the service";

            // Tạo nội dung email xác nhận
            var mailData = new MailData()
            {
                EmailToId = appointmentExist.Customer.Email,
                EmailToName = customerName,
                EmailSubject = "Appointment Confirmation",
                EmailBody = $@"
        <div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
            <h2 style=""text-align: center; color: #3498db; font-weight: bold;"">Appointment Confirmation</h2>
            <p style=""font-size: 16px; color: #555;"">Dear {customerName},</p>
        <p style=""font-size: 16px; color: #555;"">Thank you for booking 
            <span style=""font-weight: bold; text-decoration: underline; color: #000;"">{appointmentService}</span> 
            with us.
        </p>
        <p style=""font-size: 16px; color: #555;"">Here are the details of your appointment:</p>

            <ul style=""font-size: 16px; color: #555; list-style-type: none; padding: 0;"">
                <li><strong>Date:</strong> {appointmentDate}</li>
                <li><strong>Time:</strong> {appointmentTime}</li>
                <li><strong>Service:</strong> {appointmentService}</li>
            </ul>
            <p style=""font-size: 16px; color: #555;"">If you have any questions or need to reschedule, please feel free to contact us.</p>
            <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
        </div>"
            };

            _ = Task.Run(async () =>
            {
                var emailResult = await _mailService.SendEmailAsync(mailData, false);
                if (!emailResult)
                {
                    Console.WriteLine("Failed to send confirmation email.");
                }
            });

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Appointment created successfully! Confirmation email has been sent.",
                data = _mapper.Map<AppointmentsDTO>(appointmentsModel)
            }));
        }


        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAppointment([FromRoute] int id, [FromBody] ApointmentRequest request)
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
                    message = "Appointment not found!"
                }));
            }

            var appointmentsModel = await _appointmentsService.UpdateAppointments(appointmentsExist, request);
            if (appointmentsModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in update appointments!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Update appointments successfully!",
                data = _mapper.Map<AppointmentsDTO>(appointmentsModel)
            }));
        }

        [Authorize]
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
                    message = "Appointment not found!"
                }));
            }

            var appointmentsModel = await _appointmentsService.DeleteAppointments(appointmentsExist);
            if (appointmentsModel.Equals(null))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Error in delete appointments!"
                }));
            }

            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Delete appointments successfully!",
                data = _mapper.Map<AppointmentsDTO>(appointmentsModel)
            }));
        }

        [Authorize]
        [HttpGet("history-booking/{id}")]
        public async Task<IActionResult> HistoryBooking([FromRoute] int id, [FromQuery] int page = 1, int pageSize = 5)
        {
            var appointments = await _appointmentsService.BookingAppointmentHistory(id, page, pageSize);
            if (appointments.data == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "History booking not found!"
                }));
            }

            return Ok(ApiResult<GetAllAppointmentResponse>.Succeed(new GetAllAppointmentResponse()
            {
                message = appointments.message,
                data = appointments.data,
                pagination = appointments.pagination
            }));
        }

        [Authorize]
        [HttpPut("cancel-booking/{id}")]
        public async Task<IActionResult> CancleBooking([FromRoute] int id)
        {
            var appointment = await _appointmentsService.GetAppointmentsById(id);
            if (appointment == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Booking not found!"
                }));
            }

            var cancelBooking = await _appointmentsService.CancelBookingAppointment(appointment);
            if (cancelBooking == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Failed to cancel booking!"
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
                message = "Booking cancelled and email confirmation sent successfully!"
            }));
        }

    }
}