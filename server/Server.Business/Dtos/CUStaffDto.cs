namespace Server.Business.Dtos
{
    public class CUStaffDto
    {
        //public int StaffId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        //public int UserId { get; set; }
        
        public int RoleId { get; set; }
        public int BranchId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
