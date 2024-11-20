using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Branch_Product")]
public class Branch_Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ProductId { get; set; }
    public  Product Product { get; set; }
    
    public int BranchId { get; set; }
    public  Branch Branch { get; set; }
    
    public string? Status { get; set; }
    
    public int StockQuantity { get; set; } // Số lượng hàng tồn kho tại chi nhánh
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}