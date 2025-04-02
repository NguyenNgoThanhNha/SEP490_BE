using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Services;
using Server.Business.Ultils;

namespace Server.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkScheduleController : ControllerBase
    {
        private readonly WorkScheduleService _workScheduleService;
        private readonly AppointmentsService _appointmentsService;
        private readonly MailService _mailService;
        private readonly StaffService _staffService;
        private readonly MongoDbService _mongoDbService;
        private readonly AuthService _authService;
        private readonly UserService _userService;

        public WorkScheduleController(WorkScheduleService workScheduleService, AppointmentsService appointmentsService,
            MailService mailService, StaffService staffService, MongoDbService mongoDbService, AuthService authService,
            UserService userService)
        {
            _workScheduleService = workScheduleService;
            _appointmentsService = appointmentsService;
            _mailService = mailService;
            _staffService = staffService;
            _mongoDbService = mongoDbService;
            _authService = authService;
            _userService = userService;
        }

        [Authorize]
        [HttpPost("create-work-schedule")]
        public async Task<IActionResult> CreateWorkScheduleAsync(WorkSheduleRequest workSheduleRequest)
        {
            await _workScheduleService.CreateWorkScheduleAsync(workSheduleRequest);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Tạo lịch làm việc thành công",
            }));
        }


        [Authorize]
        [HttpPatch("update-work-schedules-for-staff-leave")]
        public async Task<IActionResult> UpdateWorkSchedulesAsync(WorkScheduleForStaffLeaveRequest workScheduleRequest)
        {
            // 1. Kiểm tra Authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Authorization header is missing."
                }));
            }

            var tokenValue = token.ToString().Split(' ')[1];
            var admin = await _authService.GetUserInToken(tokenValue);

            // 2. Lấy danh sách lịch hẹn của nhân viên nghỉ
            var listAppointments = await _appointmentsService
                .GetListAppointmentsByStaffId(workScheduleRequest.StaffLeaveId, workScheduleRequest.ShiftId,
                    workScheduleRequest.WorkDate);

            if (listAppointments == null)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Nhân viên không có cuộc hẹn nào."
                }));
            }


            // 3. Cập nhật lịch làm việc
            var result = await _workScheduleService.UpdateWorkScheduleForStaffLeaveAsync(workScheduleRequest);
            if (!result)
            {
                return BadRequest(ApiResult<ApiResponse>.Error(new ApiResponse()
                {
                    message = "Cập nhật lịch trình làm việc không thành công",
                }));
            }

            // 4. Gửi email thông báo cho Specialist và Customer
            if (listAppointments != null && listAppointments.Count > 0)
            {
                var newStaff = await _staffService.GetStaffById(workScheduleRequest.StaffReplaceId);

                foreach (var appointment in listAppointments)
                {
                    var customer = appointment.Customer;
                    var specialist = appointment.Staff?.StaffInfo;
                    var appointmentDate = appointment.AppointmentsTime.Date.ToString("dd/MM/yyyy");
                    var appointmentTime = appointment.AppointmentsTime.ToString(@"hh\:mm");
                    var servicesDetails = string.Join("<br/>", appointment.Service.Name);

                    // Email thông báo cho Specialist (Nhân viên nghỉ)
                    var specialistEmailData = new MailData()
                    {
                        EmailToId = specialist?.Email,
                        EmailToName = specialist?.FullName,
                        EmailSubject = "Thông báo nghỉ làm và điều chỉnh lịch hẹn",
                        EmailBody = $@"
                    <div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                        <h2 style=""text-align: center; color: #e74c3c; font-weight: bold;"">Thông báo nghỉ làm</h2>
                        <p style=""font-size: 16px; color: #555;"">Xin chào {specialist?.FullName},</p>
                        <p style=""font-size: 16px; color: #555;"">
                            Lịch làm việc của bạn vào ngày <strong>{appointmentDate}</strong> từ <strong>{appointmentTime}</strong> đã bị thay đổi.
                        </p>
                        <p style=""font-size: 16px; color: #555;"">Vui lòng kiểm tra lại lịch trình của bạn.</p>
                        <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
                    </div>"
                    };


                    // Email thông báo cho Customer
                    var customerEmailData = new MailData()
                    {
                        EmailToId = customer.Email,
                        EmailToName = customer.FullName,
                        EmailSubject = "Thông báo thay đổi lịch hẹn",
                        EmailBody = $@"
                    <div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                        <h2 style=""text-align: center; color: #f39c12; font-weight: bold;"">Thông báo thay đổi lịch hẹn</h2>
                        <p style=""font-size: 16px; color: #555;"">Xin chào {customer.FullName},</p>
                        <p style=""font-size: 16px; color: #555;"">
                            Lịch hẹn của bạn vào ngày <strong>{appointmentDate}</strong> lúc <strong>{appointmentTime}</strong> đã bị thay đổi do nhân viên có lịch nghỉ.
                        </p>
                        <p style=""font-size: 16px; color: #555;"">
                            Vui lòng liên hệ với chúng tôi để đặt lại lịch nếu cần.
                        </p>
                        <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
                    </div>"
                    };


                    // Email thông báo cho Staff thay thế
                    var newStaffEmailData = new MailData()
                    {
                        EmailToId = newStaff.StaffInfo.Email,
                        EmailToName = newStaff.StaffInfo.FullName,
                        EmailSubject = "Thông báo lịch làm việc mới",
                        EmailBody = $@"
                    <div style=""max-width: 600px; margin: 20px auto; padding: 20px; background-color: #f9f9f9; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                        <h2 style=""text-align: center; color: #2ecc71; font-weight: bold;"">Thông báo lịch làm việc mới</h2>
                        <p style=""font-size: 16px; color: #555;"">Xin chào {newStaff.StaffInfo.FullName},</p>
                        <p style=""font-size: 16px; color: #555;"">
                            Bạn đã được chỉ định thay thế cho nhân viên <strong>{specialist?.FullName}</strong> vào ngày <strong>{appointmentDate}</strong> lúc <strong>{appointmentTime}</strong>.
                        </p>
                        <p style=""font-size: 16px; color: #555;"">
                            Vui lòng chuẩn bị để đảm bảo dịch vụ tốt nhất cho khách hàng.
                        </p>
                        <p style=""text-align: center; color: #888; font-size: 14px;"">Powered by Team Solace</p>
                    </div>"
                    };
                    // get specialist MySQL
                    var specialistMySQL = await _staffService.GetStaffById(workScheduleRequest.StaffReplaceId);

                    // get admin, specialist, customer from MongoDB
                    var adminMongo = await _mongoDbService.GetCustomerByIdAsync(admin.UserId);
                    var specialistMongo = await _mongoDbService.GetCustomerByIdAsync(specialistMySQL.StaffInfo.UserId);
                    var customerMongo = await _mongoDbService.GetCustomerByIdAsync(customer.UserId);

                    // create channel
                    var channel = await _mongoDbService.CreateChannelAsync($"Channel {appointment.AppointmentId} {appointment.Service.Name}", adminMongo!.Id, appointment.AppointmentId);

                    // add member to channel
                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, specialistMongo!.Id);
                    await _mongoDbService.AddMemberToChannelAsync(channel.Id, customerMongo!.Id);

                    // Gửi email thông báo
                    _ = Task.Run(async () =>
                    {
                        await _mailService.SendEmailAsync(specialistEmailData, false);
                        await _mailService.SendEmailAsync(customerEmailData, false);
                        await _mailService.SendEmailAsync(newStaffEmailData, false);
                    });
                }
            }

            // 5. Trả về kết quả thành công
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Cập nhật lịch làm việc thành công",
            }));
        }

        [HttpGet("staff/{staffId}/work-schedules")]
        public async Task<IActionResult> GetWorkSchedulesByMonthYear(int staffId, [FromQuery] int month,
            [FromQuery] int year)
        {
            var result = await _workScheduleService.GetWorkSchedulesByMonthYearAsync(staffId, month, year);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy lịch làm việc thành công",
                data = result
            }));
        }

        [HttpGet("staff/{staffId}/shifts")]
        public async Task<IActionResult> GetShiftSlotsByMonthYear(int staffId, [FromQuery] int month,
            [FromQuery] int year)
        {
            var result = await _workScheduleService.GetShiftSlotsByMonthYearAsync(staffId, month, year);
            return Ok(ApiResult<ApiResponse>.Succeed(new ApiResponse()
            {
                message = "Lấy ca làm việc thành công",
                data = result
            }));
        }
    }
}