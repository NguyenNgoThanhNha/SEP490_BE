using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities
{
    [Table("Schedule")]
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleId { get; set; }

        [ForeignKey("StaffSchedule")]
        public int StaffId { get; set; } 
        public Staff Staff { get; set; } 

        [ForeignKey("BranchSchedule")]
        public int BranchId { get; set; } 
        public Branch Branch { get; set; } 

        [Required]
        [MaxLength(50)]
        public string ShiftName { get; set; } 

        [Required]
        public TimeSpan StartTime { get; set; } 

        [Required]
        public TimeSpan EndTime { get; set; } 

        [Required]
        public DateTime WorkDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } 

        [Required]
        public DateTime UpdatedDate { get; set; } 
    }
}
