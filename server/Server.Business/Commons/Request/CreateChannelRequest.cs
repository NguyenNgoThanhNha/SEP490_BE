namespace Server.Business.Commons.Request;

public class CreateChannelRequest
{
    public int AdminId { get; set; }
    public string ChannelName { get; set; }
    
    public int AppointmentId { get; set; }
}