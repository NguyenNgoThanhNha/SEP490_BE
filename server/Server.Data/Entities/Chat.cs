using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Entities;
[Table("Chat")]
public class Chat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ChatId { get; set; }
    
    [ForeignKey("SenderChat")]
    public int SenderId { get; set; }
    public virtual User Sender { get; set; }
    
    [ForeignKey("ReceiverChat")]
    public int ReceiverId { get; set; }
    public virtual User Receiver { get; set; }
    
    public string content { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}