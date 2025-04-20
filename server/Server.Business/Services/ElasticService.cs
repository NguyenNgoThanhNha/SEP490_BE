//using Microsoft.AspNetCore.Http;
//using Nest;
//using Newtonsoft.Json;
//using Server.Business.Dtos;

//namespace Server.Business.Services
//{
//    public class ElasticService<T> where T : class
//    {
//        private readonly IElasticClient _elasticClient;
//        private readonly string _indexName;

//        public ElasticService(IElasticClient elasticClient, string indexName)
//        {
//            _elasticClient = elasticClient;
//            _indexName = indexName;
//        }

//        public async Task<string> ImportFromJsonFileAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                throw new ArgumentException("Please upload a valid JSON file.");

//            try
//            {
//                // Xóa toàn bộ dữ liệu cũ
//                await DeleteAllDocumentsAsync();

//                using var streamReader = new StreamReader(file.OpenReadStream());
//                var jsonData = await streamReader.ReadToEndAsync();

//                var documentList = JsonConvert.DeserializeObject<List<T>>(jsonData);
//                if (documentList == null || !documentList.Any())
//                    return "The JSON file is empty or has an invalid format.";

//                foreach (var document in documentList)
//                {
//                    await _elasticClient.IndexAsync(document, i => i.Index(_indexName).Refresh(Elasticsearch.Net.Refresh.WaitFor));
//                }

//                return $"{documentList.Count} documents were successfully imported.";
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"An error occurred while importing data: {ex.Message}");
//            }
//        }




//        private bool IsValid(T document)
//        {
//            if (document is ProductDto product)
//            {
//                return !string.IsNullOrEmpty(product.ProductName)
//                    && product.Price >= 0
//                    && product.Quantity >= 0

//                    && product.CompanyId > 0;
//            }
//            else if (document is ServiceDto service)
//            {
//                return !string.IsNullOrEmpty(service.Name)
//                    && !string.IsNullOrEmpty(service.Description)
//                    && service.Price >= 0
//                    && !string.IsNullOrEmpty(service.Duration);
//            }
//            else if (document is BlogDTO blog)
//            {
//                return !string.IsNullOrEmpty(blog.Title)
//                    && !string.IsNullOrEmpty(blog.Content)
//                    && blog.AuthorId > 0;
//            }
//            else
//            {
//                return false;
//            }
//        }


//        public async Task IndexDocumentAsync(T document)
//        {
//            var response = await _elasticClient.IndexAsync(document, i => i.Index(_indexName));
//            if (!response.IsValid)
//            {
//                throw new Exception($"Failed to index document: {response.ServerError?.Error?.Reason}");
//            }
//        }

//        public async Task UpdateDocumentAsync(T document, string id)
//        {
//            var getResponse = await _elasticClient.GetAsync<T>(id, g => g.Index(_indexName));
//            if (!getResponse.Found)
//            {
//                throw new Exception($"Document with id {id} not found");
//            }

//            var response = await _elasticClient.UpdateAsync<T>(id, u => u.Index(_indexName).Doc(document));
//            if (!response.IsValid)
//            {
//                throw new Exception($"Failed to update document: {response.ServerError?.Error?.Reason}");
//            }
//        }

//        public async Task DeleteDocumentAsync(string id)
//        {
//            var response = await _elasticClient.DeleteAsync<T>(id, d => d.Index(_indexName));
//            if (!response.IsValid)
//            {
//                throw new Exception($"Failed to delete document: {response.ServerError?.Error?.Reason}");
//            }
//        }

//        public async Task<IEnumerable<T>> SearchAsync(string keyword, int size = 5000)
//        {
//            var response = await _elasticClient.SearchAsync<T>(s => s
//                .Index(_indexName)
//                .Query(q => q.QueryString(qs => qs.Query($"*{keyword}*")))
//                .Size(size));
//            if (!response.IsValid)
//            {
//                throw new Exception($"Failed to search: {response.ServerError?.Error?.Reason}");
//            }
//            return response.Documents
//                .GroupBy(doc => doc.GetType().GetProperty("Name")?.GetValue(doc)?.ToString())
//                .Select(g => g.First())
//                .ToList();
//        }

//        public async Task<IEnumerable<T>> GetAllAsync()
//        {
//            var response = await _elasticClient.SearchAsync<T>(s => s.Index(_indexName).MatchAll().Size(1000));
//            if (!response.IsValid)
//            {
//                throw new Exception($"Failed to retrieve documents: {response.ServerError?.Error?.Reason}");
//            }
//            return response.Documents;
//        }

//        public async Task DeleteAllDocumentsAsync()
//        {
//            var response = await _elasticClient.DeleteByQueryAsync<T>(d => d
//                .Index(_indexName)
//                .Query(q => q.MatchAll())
//            );

//            if (!response.IsValid)
//            {
//                throw new Exception($"Xóa tất cả documents thất bại: {response.ServerError?.Error?.Reason}");
//            }
//        }


//    }
//}
using Microsoft.AspNetCore.Http;
using Nest;
using Newtonsoft.Json;
using Server.Business.Dtos;

namespace Server.Business.Services
{
    public class ElasticService<T> where T : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _indexName;

        public ElasticService(IElasticClient elasticClient, string indexName)
        {
            _elasticClient = elasticClient;
            _indexName = indexName;
        }

        public async Task<string> ImportFromJsonFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Vui lòng chọn file JSON hợp lệ.");

            try
            {
                using var streamReader = new StreamReader(file.OpenReadStream());
                var jsonData = await streamReader.ReadToEndAsync();
                var documentList = JsonConvert.DeserializeObject<List<T>>(jsonData);

                if (documentList == null || !documentList.Any())
                    return "File JSON trống hoặc sai định dạng.";

                // Bulk import
                var bulkDescriptor = new BulkDescriptor();
                foreach (var doc in documentList)
                {
                    if (IsValid(doc))
                    {
                        bulkDescriptor.Index<T>(op => op
                            .Index(_indexName)
                            .Document(doc));
                    }
                }

                var bulkResponse = await _elasticClient.BulkAsync(bulkDescriptor);

                if (!bulkResponse.IsValid)
                    throw new Exception($"Lỗi khi import: {bulkResponse.ServerError?.Error?.Reason}");

                return $"Đã import thành công {bulkResponse.Items.Count} documents.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi import dữ liệu: {ex.Message}");
            }
        }

        private bool IsValid(T document)
        {
            if (document is ProductDto product)
            {
                return !string.IsNullOrEmpty(product.ProductName)
                    && product.Price >= 0
                    && product.Quantity >= 0
                    && product.CompanyId > 0;
            }
            if (document is ServiceDto service)
            {
                return !string.IsNullOrEmpty(service.Name)
                    && !string.IsNullOrEmpty(service.Description)
                    && service.Price >= 0
                    && !string.IsNullOrEmpty(service.Duration);
            }
            if (document is BlogDTO blog)
            {
                return !string.IsNullOrEmpty(blog.Title)
                    && !string.IsNullOrEmpty(blog.Content)
                    && blog.AuthorId > 0;
            }

            return false;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var response = await _elasticClient.SearchAsync<T>(s => s
                .Index(_indexName)
                .MatchAll()
                .Size(1000));

            if (!response.IsValid)
                throw new Exception($"Không thể lấy dữ liệu: {response.ServerError?.Error?.Reason}");

            return response.Documents;
        }

        public async Task<IEnumerable<T>> SearchAsync(string keyword, int size = 5000)
        {
            var response = await _elasticClient.SearchAsync<T>(s => s
                .Index(_indexName)
                .Query(q => q.QueryString(qs => qs.Query($"*{keyword}*")))
                .Size(size));

            if (!response.IsValid)
                throw new Exception($"Tìm kiếm thất bại: {response.ServerError?.Error?.Reason}");

            return response.Documents;
        }

        public async Task IndexDocumentAsync(T document)
        {
            var response = await _elasticClient.IndexAsync(document, i => i.Index(_indexName));
            if (!response.IsValid)
                throw new Exception($"Index thất bại: {response.ServerError?.Error?.Reason}");
        }

        public async Task DeleteAllDocumentsAsync()
        {
            var response = await _elasticClient.DeleteByQueryAsync<T>(q => q
                .Index(_indexName)
                .Query(q => q.MatchAll()));

            if (!response.IsValid)
                throw new Exception($"Xóa tất cả documents thất bại: {response.ServerError?.Error?.Reason}");
        }
    }
}


