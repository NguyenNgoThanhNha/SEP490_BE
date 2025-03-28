using Server.Data.Entities;

namespace Server.Business.Dtos
{
    public class CartDTO
    {
        public int ProductCartId { get; set; }
        public int CartId { get; set; }      
        public ProductDetailInCartDto Product { get; set; }  
        public int Quantity {  get; set; }
    }
}
