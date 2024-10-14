﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;

[Table("Product")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }
    
    public string ProductName { get; set; }
    
    public string ProductDescription { get; set; }
    
    public decimal Price { get; set; }
    
    public int Quantity { get; set; } // Số lượng hàng tồn kho tại công ty

    public decimal? Discount { get; set; }
    
    [ForeignKey("Product_Category")]
    public int CategoryId  { get; set; }
    public virtual Category Category { get; set; }
    
    [ForeignKey("Product_Company")]
    public int CompanyId  { get; set; }
    public virtual Company Company { get; set; }
    
    public ICollection<Branch_Product> Branch_Products { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}