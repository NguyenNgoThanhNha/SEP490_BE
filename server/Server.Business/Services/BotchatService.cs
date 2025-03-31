using System.Text;
using Microsoft.Extensions.Options;
using Server.Business.Ultils;
using JsonElement = System.Text.Json.JsonElement;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Server.API.Ultils;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class BotchatService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly BotChatSetting _botChatSetting;
    private static readonly HttpClient HttpClient = new HttpClient();
    
    public BotchatService(IOptions<BotChatSetting> botChatSetting, UnitOfWorks unitOfWorks)
    {
        _unitOfWorks = unitOfWorks;
        _botChatSetting = botChatSetting.Value;
    }
public async Task<bool> SeedingDataChatbot()
{
    try
    {
        // Đọc nội dung file JSON
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SolaceFAQs.json");
        var jsonDataFromFile = await File.ReadAllTextAsync(filePath);
        
        // Giải mã JSON thành List<JsonObject> thay vì dynamic
        var faqData = JsonSerializer.Deserialize<List<JsonObject>>(jsonDataFromFile);

        // Lấy dữ liệu từ repository
        var products = await _unitOfWorks.ProductRepository.GetAll().ToListAsync();
        var services = await _unitOfWorks.ServiceRepository.GetAll().ToListAsync();

        // Kiểm tra và xóa Product nếu có, sau đó thêm lại toàn bộ sản phẩm
        var productIndex = faqData.FindIndex(f =>
        {
            try
            {
                var questionElement = f["question"]?.ToString();
                return questionElement == "Product";
            }
            catch (KeyNotFoundException)
            {
                // Trường hợp không tìm thấy thuộc tính "question"
                return false;
            }
        });

        if (productIndex >= 0)
        {
            faqData.RemoveAt(productIndex); // Xóa Product nếu có
        }

        // Thêm mới danh sách sản phẩm
        faqData.Add(new JsonObject
        {
            { "question", "Product" },
            { "answer", JsonSerializer.Serialize(products.Select(p => new
                {
                    name = p.ProductName,
                    description = p.ProductDescription,
                    price = p.Price
                }))
            }
        });

        // Kiểm tra và xóa Service nếu có, sau đó thêm lại toàn bộ dịch vụ
        var serviceIndex = faqData.FindIndex(f =>
        {
            var questionElement = f["question"]?.ToString();
            return questionElement == "Service";
        });

        if (serviceIndex >= 0)
        {
            faqData.RemoveAt(serviceIndex); // Xóa Service nếu có
        }

        // Thêm mới danh sách dịch vụ
        faqData.Add(new JsonObject
        {
            { "question", "Service" },
            { "answer", JsonSerializer.Serialize(services.Select(s => new
                {
                    name = s.Name,
                    description = s.Description,
                    price = s.Price
                }))
            }
        });

        // Ghi lại toàn bộ dữ liệu vào file JSON
        var updatedJsonData = JsonSerializer.Serialize(faqData, new JsonSerializerOptions
        {
            WriteIndented = true, // Format JSON dễ đọc
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Đảm bảo rằng ký tự đặc biệt (ví dụ: dấu tiếng Việt) không bị mã hóa sai
        });

        await File.WriteAllTextAsync(filePath, updatedJsonData);

        return true;
    }
    catch (Exception ex)
    {
        // Xử lý lỗi
        Console.WriteLine($"Error while updating Product and Service data: {ex.Message}");
        return false;
    }
}

      
    public async Task<string> SendChatMessageAsync(string userMessage)
    {
        try
        {
            // Đường dẫn đến file JSON
            var jsonFilePath = "SolaceFAQs.json";

            // Đọc và deserialize dữ liệu từ file JSON
            var jsonData = await File.ReadAllTextAsync(jsonFilePath);
            var faqs = System.Text.Json.JsonSerializer.Deserialize<List<FAQ>>(jsonData);

            if (faqs == null || faqs.Count == 0)
            {
                return "Không tìm thấy dữ liệu câu hỏi thường gặp.";
            }

            // Tìm câu trả lời phù hợp với câu hỏi của người dùng
            var matchedFAQ = faqs.FirstOrDefault(faq => faq.question.ToLower().Contains(userMessage.ToLower()));

            if (matchedFAQ != null)
            {
                return matchedFAQ.answer;
            }

            // Nếu không tìm thấy câu trả lời trong FAQ, gửi yêu cầu đến AI
            var modelName = "gemini-1.5-flash";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_botChatSetting.ApiKey}";

            var userMessageLower = userMessage.ToLower();
            

            bool containsSolace = userMessageLower.Contains("solace");
            bool containsProduct = userMessageLower.Contains("product") || userMessageLower.Contains("sản phẩm");
            bool containsService = userMessageLower.Contains("service") || userMessageLower.Contains("dịch vụ");

            // Xử lý câu hỏi về Solace
            if (containsSolace)
            {
                userMessage = userMessage + " " + faqs[0].question + " " + faqs[0].answer;
            }

            // Xử lý câu hỏi về sản phẩm
            if (containsProduct)
            {
                userMessage = userMessage + " lấy ra số lượng sản phẩm yêu cầu nếu có không thì chỉ lấy ra 5: " + faqs[1].question + " " + faqs[1].answer;
            }

            // Xử lý câu hỏi về dịch vụ
            if (containsService)
            {
                userMessage = userMessage + "lấy ra số lượng dịch vụ yêu cầu nếu có không thì chỉ lấy ra 5: " + faqs[2].question + " " + faqs[2].answer;
            }
            
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
                    string responseText = parts[0].GetProperty("text").GetString() ?? "";
                    responseText = responseText.Replace("Theo đoạn văn bạn cung cấp,", "").Trim();
                    responseText = responseText.Replace("Dựa trên đoạn văn bạn cung cấp,", "").Trim();
                    responseText = responseText.Replace("Dựa trên đoạn văn mô tả,", "").Trim();
                    responseText = responseText.Replace("Dựa trên thông tin được cung cấp,", "").Trim();
                    responseText = responseText.Replace("Dựa trên thông tin bạn cung cấp,", "").Trim();
                    return responseText;
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