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
                        new { text = "Solace là gì?" }, 
                        new { text = "Solace là một ứng dụng SPA tích hợp AI, giúp phân tích tình trạng da của người dùng và đề xuất sản phẩm, liệu trình chăm sóc da phù hợp." },
                        new { text = "Ứng dụng này sử dụng AI để nhận diện các vấn đề da như mụn, tàn nhang, vết nám, và đưa ra gợi ý về các sản phẩm phù hợp." },
                        new { text = "Solace cung cấp các sản phẩm chăm sóc da như: Remedy Cream To Oil, Essential Face Wash, Active Pureness Cleasing Gel, Clearing Skin Wash, Oil To Foam Cleanser." },
                        new { text = "Solace còn hỗ trợ người dùng với các liệu trình chăm sóc da chuyên sâu như: Signature Facial, Anti-Aging Facial, Hydrating Facial, Brightening Facial, Acne Treatment Facial." },
                        new { text = "Ứng dụng này không chỉ giúp người dùng cải thiện làn da mà còn đề xuất các liệu trình và sản phẩm phù hợp, mang lại hiệu quả chăm sóc da lâu dài." },
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

            // Định dạng trả ve từ response
        /*{
            "candidates": [
            {
                "content": {
                    "parts": [
                    {
                        "text": "Hello there! How can I help you today?\n"
                    }
                    ],
                    "role": "model"
                },
                "finishReason": "STOP",
                "avgLogprobs": -0.00063246636736122048
            }
            ],
            "usageMetadata": {
                "promptTokenCount": 1,
                "candidatesTokenCount": 11,
                "totalTokenCount": 12
            },
            "modelVersion": "gemini-1.5-flash"
        }*/
            
            if (result.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.Object &&
                content.TryGetProperty("parts", out var parts) &&
                parts.ValueKind == JsonValueKind.Array &&
                parts.GetArrayLength() > 0)
            {
                // Trả về text từ response
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