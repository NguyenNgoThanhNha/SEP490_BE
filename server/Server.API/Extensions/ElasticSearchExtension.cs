using Nest;
using Server.Business.Dtos;

namespace Server.API.Extensions
{
    public static class ElasticSearchExtension
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = /*configuration["ElasticSettings:baseUrl"];*/ "https://localhost:9200/";
            var finger =/* configuration["ElasticSettings:finger"];*/ "cdefc270eec9957bb8dcfd2e1581f0d172229fc167089f2a86391df96a21161c";
            var password = /*configuration["ElasticSettings:password"];*/ "q66nd9kbqUiG*fQQrJKz";
            var settings = new ConnectionSettings(new Uri(baseUrl ?? ""))
                .PrettyJson()
                .CertificateFingerprint(finger)
                .BasicAuthentication("elastic", password);

            settings.EnableApiVersioningHeader();
            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
            //CreateIndex(client);
        }
        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings.DefaultMappingFor<ProductDto>(m => m
                .Ignore(p => p.CreatedDate)
                .Ignore(p => p.UpdatedDate));

            settings.DefaultMappingFor<BlogDTO>(m => m
                .Ignore(p => p.CreatedDate)
                .Ignore(p => p.UpdatedDate));

            settings.DefaultMappingFor<ServiceDto>(m => m
                .Ignore(p => p.CreatedDate)
                .Ignore(p => p.UpdatedDate));
        }


        //private static void CreateIndex(IElasticClient client)
        //{
        //    CreateIndexForType<ProductDto>(client, $"products");
        //    CreateIndexForType<BlogDTO>(client, $"blogs");
        //    CreateIndexForType<ServiceDto>(client, $"services");
        //}

        //private static void CreateIndexForType<T>(IElasticClient client, string indexName) where T : class
        //{
        //    var existsResponse = client.Indices.Exists(indexName);

        //    if (!existsResponse.Exists)
        //    {
        //        var createIndexResponse = client.Indices.Create(indexName, index => index
        //            .Map<T>(x => x.AutoMap()) // AutoMap tự động ánh xạ các thuộc tính
        //            .Settings(s => s
        //                .NumberOfReplicas(1) // Số lượng bản sao
        //                .NumberOfShards(1))  // Số lượng shards
        //        );

        //        if (!createIndexResponse.IsValid)
        //        {
        //            throw new Exception($"Failed to create index {indexName}: {createIndexResponse.ServerError?.Error.Reason}");
        //        }
        //    }
        //}
    }
}
