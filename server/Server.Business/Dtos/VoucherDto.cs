namespace Server.Business.Dtos;

public class VoucherDto
{
    public int VoucherId { get; set; }
    
    public string Code { get; set; }
    
    public int Quantity { get; set; }
    
    public int RemainQuantity { get; set; }
    
    public string Status { get; set; }
    
    public string Description { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal MinOrderAmount { get; set; } // giá trị đơn hàng tối thiểu để sử dụng voucher
    
    public DateTime ValidFrom { get; set; }
    
    public DateTime ValidTo { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}