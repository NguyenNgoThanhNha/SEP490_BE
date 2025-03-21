﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ProductCart")]
public class ProductCart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductCartId { get; set; }
    
    [ForeignKey("User_Cart_Product")]
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }
    
    [ForeignKey("User_Cart_Cart")]
    public int CartId { get; set; }
    public virtual Cart Cart { get; set; }
    
    public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
    public string Status { get; set; } = OrderStatusEnum.Pending.ToString();
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}