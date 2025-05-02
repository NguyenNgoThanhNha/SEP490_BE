namespace Server.Business.Commons.Request;

public class BookCompoSkinCareRoutineRequest
{
    public int RoutineId { get; set; }
    public int BranchId { get; set; }
    public int UserId { get; set; }
    public int? VoucherId { get; set; }
    public string? Note { get; set; }
    public DateTime? AppointmentTime { get; set; } = DateTime.Now;
    
    public string? PaymentMethod { get; set; }
}