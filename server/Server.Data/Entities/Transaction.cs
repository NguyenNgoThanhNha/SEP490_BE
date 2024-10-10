using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Transaction")]
public class Transaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionId { get; set; }
    
    [ForeignKey("Order_Transaction")]
    public int OrderId { get; set; }
    public virtual Order Order { get; set; }
    
    [ForeignKey("Customer_Transaction")]
    public int CustomerId { get; set; }
    public virtual User Customer { get; set; }
    
    public int? TransactionCode { get; set; }
    
    public string? Reference { get; set; }
    
    public string? PaymentMethod { get; set; } = string.Empty;
    
    public string? TransactionDateTime { get; set; }
    
    public string Status { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}