using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Voucher")]
public class Voucher
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VoucherId { get; set; }
    
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
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    public ICollection<UserVoucher>? UserVoucher { get; set; }

}