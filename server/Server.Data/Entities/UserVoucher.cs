using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("UserVoucher")]
public class UserVoucher
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserVoucherId { get; set; }
    
    [ForeignKey("UserVoucher_UserId")]
    public int UserId { get; set; }
    public User User { get; set; }
    
    [ForeignKey("UserVoucher_VoucherId")]
    public int VoucherId { get; set; }
    public Voucher Voucher { get; set; }
    
    public string Status { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}