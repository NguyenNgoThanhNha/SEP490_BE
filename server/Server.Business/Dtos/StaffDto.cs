namespace Server.Business.Dtos
{
    public class StaffDto
    {

        public int StaffId { get; set; }

        public int UserId { get; set; }

        public int BranchId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
