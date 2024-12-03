namespace Server.Business.Dtos
{
    public class CUOrderDto
    {
        public int OrderId { get; set; }
        public int OrderCode { get; set; }
        public int CustomerId { get; set; }
        public int? VoucherId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
