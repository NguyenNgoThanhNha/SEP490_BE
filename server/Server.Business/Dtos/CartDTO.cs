namespace Server.Business.Dtos
{
    public class CartDTO
    {
        public int ProductCartId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public int CartId { get; set; }

        public int Quantity { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; }
        
        public ProductDto Product { get; set; }
        public CategoryDto Category { get; set; }
    }
}
