namespace Server.Business.Ultils;

public class ChatSession
{
    private List<string> chatHistory = new List<string>();

    // Thêm tin nhắn vào lịch sử chat
    public void AddToHistory(string message)
    {
        chatHistory.Add(message);
    }

    // Lấy toàn bộ lịch sử chat
    public List<string> GetHistory()
    {
        return chatHistory;
    }

    // Xóa lịch sử nếu bắt đầu một phiên trò chuyện mới
    public void ClearHistory()
    {
        chatHistory.Clear();
    }
}