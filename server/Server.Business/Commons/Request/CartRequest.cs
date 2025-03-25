using Server.Data;

namespace Server.Business.Commons.Request
{
    public class CartRequest
    {
        public int ProductCartId { get; set; }
        public int ProductId { get; set; }
        public int CartId { get; set; }

        public int Quantity { get; set; } = 1;
        public string? Note { get; set; }
        public OrderStatusEnum Status { get; set; }
        public OperationTypeEnum Operation { get; set; }
    }

    public class AddToCartRequest
    {
        public int ProductBranchId { get; set; }
        public int Quantity { get; set; } = 1;
        public int UserId { get; set; }
        
        public OperationTypeEnum Operation { get; set; }
    }
}
