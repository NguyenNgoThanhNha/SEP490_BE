using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class CreateOrderWithProductsAndServicesRequest
{
    [Required(ErrorMessage = "CustomerId là bắt buộc")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "BranchId là bắt buộc")]
    public int BranchId { get; set; }

    public int? VoucherId { get; set; }

    [Required(ErrorMessage = "Danh sách sản phẩm là bắt buộc")]
    public int[] ProductBranchIds { get; set; }

    [Required(ErrorMessage = "Số lượng sản phẩm là bắt buộc")]
    public int[] ProductQuantities { get; set; }

    [Required(ErrorMessage = "Danh sách dịch vụ là bắt buộc")]
    public int[] ServiceIds { get; set; }

    [Required(ErrorMessage = "Số lượng dịch vụ là bắt buộc")]
    public int[] ServiceQuantities { get; set; }

    [Required(ErrorMessage = "Danh sách nhân viên là bắt buộc")]
    public int[] StaffIds { get; set; }

    [Required(ErrorMessage = "Danh sách thời gian hẹn là bắt buộc")]
    public DateTime[] AppointmentDates { get; set; }

    [Required(ErrorMessage = "Tổng tiền là bắt buộc")]
    public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "IsAuto là bắt buộc")]
    public bool IsAuto { get; set; }
}
