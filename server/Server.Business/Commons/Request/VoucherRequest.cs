namespace Server.Business.Commons.Request;

public class VoucherRequest
{
    public string Status { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class CreateVoucherRequest
{
    public string Code { get; set; }
    
    public int Quantity { get; set; }
    
    public int RemainQuantity { get; set; }
    
    public string Status { get; set; }
    
    public string Description { get; set; }
    
    public decimal DiscountAmount { get; set; }

    
    public int RequirePoint { get; set; } = 10;
    
    public decimal? MinOrderAmount { get; set; } // giá trị đơn hàng tối thiểu để sử dụng voucher
    
    public DateTime ValidFrom { get; set; }
    
    public DateTime ValidTo { get; set; }
}

public class UpdateVoucherRequest : CreateVoucherRequest
{
    public int VoucherId { get; set; }
}
