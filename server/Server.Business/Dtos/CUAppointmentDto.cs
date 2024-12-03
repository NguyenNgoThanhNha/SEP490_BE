namespace Server.Business.Dtos
{
    public class CUAppointmentDto
    {
        public int AppointmentsId { get; set; }
        public int CustomerId { get; set; }
        public int StaffId { get; set; }
        public int ServiceId { get; set; }
        public int BranchId { get; set; }
        public DateTime AppointmentsTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string Feedback { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
