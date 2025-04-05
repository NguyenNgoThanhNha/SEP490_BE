using System.ComponentModel.DataAnnotations;

namespace Server.Business.Commons.Request;

public class VoucherRequest
{
    public string Status { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class CreateVoucherRequest
{
    [Required]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Mã voucher phải từ 3 đến 20 ký tự.")]
    public string Code { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng còn lại không hợp lệ.")]
    public int RemainQuantity { get; set; }

    [Required]
    [RegularExpression("^(Active|Inactive|Expired)$", ErrorMessage = "Trạng thái không hợp lệ.")]
    public string Status { get; set; }

    [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự.")]
    public string Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền giảm giá phải lớn hơn 0.")]
    public decimal DiscountAmount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Điểm yêu cầu phải lớn hơn hoặc bằng 0.")]
    public int RequirePoint { get; set; } = 10;

    [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu không hợp lệ.")]
    public decimal? MinOrderAmount { get; set; }

    [DataType(DataType.DateTime)]
    [Required(ErrorMessage = "Ngày bắt đầu hiệu lực là bắt buộc.")]
    public DateTime ValidFrom { get; set; }

    [DataType(DataType.DateTime)]
    [Required(ErrorMessage = "Ngày hết hạn là bắt buộc.")]
    [DateGreaterThan("ValidFrom", ErrorMessage = "Ngày hết hạn phải lớn hơn ngày bắt đầu.")]
    public DateTime ValidTo { get; set; }
}


public class UpdateVoucherRequest : CreateVoucherRequest
{
    [Required(ErrorMessage = "ID voucher là bắt buộc.")]
    public int VoucherId { get; set; }
}


public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreaterThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var currentValue = (DateTime?)value;

        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
        if (property == null)
            return new ValidationResult($"Unknown property: {_comparisonProperty}");

        var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

        if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
