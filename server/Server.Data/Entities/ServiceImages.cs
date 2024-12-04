using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("ServiceImages")]
public class ServiceImages
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceImagesId { get; set; }
    
    [ForeignKey("Service_Images")]
    public int ServiceId { get; set; }
    public virtual Service Service { get; set; }

    public string image { get; set; }
}