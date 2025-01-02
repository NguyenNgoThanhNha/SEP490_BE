using System.Text;
using Microsoft.Extensions.Options;
using Server.Business.Ultils;
using JsonElement = System.Text.Json.JsonElement;
using System.Text.Json;
namespace Server.Business.Services;

public class BotchatService
{
    private readonly BotChatSetting _botChatSetting;
    private static readonly HttpClient HttpClient = new HttpClient();
    
    public BotchatService(IOptions<BotChatSetting> botChatSetting)
    {
        _botChatSetting = botChatSetting.Value;
    }
    
    public async Task<string> SendChatMessageAsync(string userMessage)
    {
        try
        {
        // Kiểm tra xem câu hỏi có liên quan đến "Solace là gì" không
        if (userMessage.ToLower().Contains("solace") && userMessage.ToLower().Contains("là gì"))
        {
            // Câu hỏi về Solace, trả lời giới thiệu về Solace
            var predefinedResponse = new[]
            {
                new { text = "Solace là gì?" },
                new { text = "Solace là một ứng dụng SPA tích hợp AI, giúp phân tích tình trạng da của người dùng và đề xuất sản phẩm, liệu trình chăm sóc da phù hợp." },
                new { text = "Ứng dụng này sử dụng AI để nhận diện các vấn đề da như mụn, tàn nhang, vết nám, và đưa ra gợi ý về các sản phẩm phù hợp." },
                new { text = "Ứng dụng này không chỉ giúp người dùng cải thiện làn da mà còn đề xuất các liệu trình và sản phẩm phù hợp, mang lại hiệu quả chăm sóc da lâu dài." }
            };

            return string.Join("\n", predefinedResponse.Select(r => r.text));
        }
        
        if (userMessage.ToLower().Contains("sản phẩm") || userMessage.ToLower().Contains("product"))
        {
            // Câu hỏi về sản phẩm, trả lời chỉ về sản phẩm của Solace
            var productResponse = new[]
            {
                new { text = "Solace cung cấp các sản phẩm chăm sóc da như: Remedy Cream To Oil, Essential Face Wash, Active Pureness Cleasing Gel, Clearing Skin Wash, Oil To Foam Cleanser." },
                new { text = "Các sản phẩm này giúp cải thiện làn da, làm sạch, dưỡng ẩm và hỗ trợ điều trị các vấn đề da." }
            };

            return string.Join("\n", productResponse.Select(r => r.text));
        }
        
        if (userMessage.ToLower().Contains("dịch vụ") || userMessage.ToLower().Contains("service"))
        {
            // Câu hỏi về dịch vụ, trả lời chỉ về dịch vụ của Solace
            var serviceResponse = new[]
            {
                new { text = "Solace cung cấp các liệu trình chăm sóc da chuyên sâu như: Signature Facial, Anti-Aging Facial, Hydrating Facial, Brightening Facial, Acne Treatment Facial." },
                new { text = "Những liệu trình này giúp cải thiện làn da, làm sáng da, giảm lão hóa và điều trị các vấn đề như mụn, da khô, và lão hóa." }
            };

            return string.Join("\n", serviceResponse.Select(r => r.text));
        }

        // Nếu không phải câu hỏi về Solace hay sản phẩm, gửi yêu cầu đến AI
        var modelName = "gemini-1.5-flash";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_botChatSetting.ApiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = userMessage }
                    }
                }
            }
        };

            // Serialize request body
            var requestContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Gửi request
            var response = await HttpClient.PostAsync(url, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response
                var result = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                if (result.TryGetProperty("candidates", out var candidates) &&
                    candidates.ValueKind == JsonValueKind.Array &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var content) &&
                    content.ValueKind == JsonValueKind.Object &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0)
                {
                    return parts[0].GetProperty("text").GetString() ?? "No output from the AI model.";
                }

                return "Invalid response structure from the AI model.";
            }
            else
            {
                // Log lỗi chi tiết từ server
                var errorMessage = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode}\nDetails: {errorMessage}";
            }
        }
        catch (HttpRequestException httpEx)
        {
            return $"HTTP Request Error: {httpEx.Message}";
        }
        catch (Exception ex)
        {
            return $"An unexpected error occurred: {ex.Message}";
        }
    }
}