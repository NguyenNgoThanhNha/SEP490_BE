using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using Nest;
using Server.Business.Dtos;
using Server.Business.Ultils;

namespace Server.API.Extensions
{
    public static class ElasticSearchExtension
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind ElasticSettings from configuration
            var elasticSettings = new ElasticSettings();
            configuration.GetSection("ElasticSettings").Bind(elasticSettings);

            if (string.IsNullOrWhiteSpace(elasticSettings.baseUrl))
                throw new ArgumentException("ElasticSearch BaseUrl cannot be null or empty");

            if (string.IsNullOrWhiteSpace(elasticSettings.finger))
                throw new ArgumentException("ElasticSearch CertificateFingerprint cannot be null or empty");

            if (string.IsNullOrWhiteSpace(elasticSettings.password))
                throw new ArgumentException("ElasticSearch Password cannot be null or empty");

            Console.WriteLine(elasticSettings.baseUrl);
            // Set up connection settings
            var settings = new ConnectionSettings(new Uri(elasticSettings.baseUrl))
                .PrettyJson()
                .CertificateFingerprint(elasticSettings.finger)
                .BasicAuthentication("elastic", elasticSettings.password)
                .EnableApiVersioningHeader();

            // Add default mappings (if needed)
            AddDefaultMappings(settings);

            // Create ElasticClient and register as a singleton
            var client = new ElasticClient(settings);
            var pingResponse = client.Ping();
            if (!pingResponse.IsValid)
            {
                Console.WriteLine("Error connect ElasticSearch: ", pingResponse.DebugInformation);
                return;
            }
            services.AddSingleton<IElasticClient>(client);

            // Create index if needed
            CreateIndex(client);
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


        private static void CreateIndex(IElasticClient client)
        {
            CreateIndexForType<ProductDto>(client, $"products");
            CreateIndexForType<BlogDTO>(client, $"blogs");
            CreateIndexForType<ServiceDto>(client, $"services");
        }

        private static void CreateIndexForType<T>(IElasticClient client, string indexName) where T : class
        {
            var existsResponse = client.Indices.Exists(indexName);

            if (!existsResponse.Exists)
            {
                var createIndexResponse = client.Indices.Create(indexName, index => index
                    .Map<T>(x => x.AutoMap()) // AutoMap tự động ánh xạ các thuộc tính
                    .Settings(s => s
                        .NumberOfReplicas(1) // Số lượng bản sao
                        .NumberOfShards(1))  // Số lượng shards
                );

                if (!createIndexResponse.IsValid)
                {
                    throw new Exception($"Failed to create index {indexName}: {createIndexResponse.ServerError?.Error.Reason}");
                }
            }
        }
    }
}
