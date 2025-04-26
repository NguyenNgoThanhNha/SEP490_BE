using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Server.Business.Dtos;
using Server.Business.Ultils;
using System;

namespace Server.API.Extensions
{
    public static class ServiceExtensionst
    {
        public static IServiceCollection AddElasticSearchT(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticSettings = configuration.GetSection(nameof(ElasticSettings)).Get<ElasticSettings>();

            if (string.IsNullOrWhiteSpace(elasticSettings.baseUrl))
                throw new ArgumentException("ElasticSearch BaseUrl cannot be null or empty");

            if (string.IsNullOrWhiteSpace(elasticSettings.finger))
                throw new ArgumentException("ElasticSearch CertificateFingerprint cannot be null or empty");

            if (string.IsNullOrWhiteSpace(elasticSettings.password))
                throw new ArgumentException("ElasticSearch Password cannot be null or empty");

            var settings = new ConnectionSettings(new Uri(elasticSettings.baseUrl))
                .DefaultIndex(elasticSettings.defaultIndex ?? "default-index")
                .PrettyJson()
                .CertificateFingerprint(elasticSettings.finger)
                .BasicAuthentication("elastic", elasticSettings.password)
                .EnableApiVersioningHeader();

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            var pingResponse = client.Ping();
            if (!pingResponse.IsValid)
            {
                Console.WriteLine("❌ ElasticSearch Ping failed:");
                Console.WriteLine(pingResponse.DebugInformation);
                throw new Exception("ElasticSearch ping failed. See logs for details.");
            }

            services.AddSingleton<IElasticClient>(client);

            CreateIndices(client);

            return services;
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

        private static void CreateIndices(IElasticClient client)
        {
            CreateIndexIfNotExists<ProductDto>(client, "products");
            CreateIndexIfNotExists<BlogDTO>(client, "blogs");
            CreateIndexIfNotExists<ServiceDto>(client, "services");
        }

        private static void CreateIndexIfNotExists<T>(IElasticClient client, string indexName) where T : class
        {
            var exists = client.Indices.Exists(indexName);
            if (!exists.Exists)
            {
                var response = client.Indices.Create(indexName, c => c
                    .Map<T>(m => m.AutoMap())
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(1)));

                if (!response.IsValid)
                {
                    throw new Exception($"❌ Failed to create index '{indexName}': {response.ServerError?.Error.Reason}");
                }
            }
        }
    }
}
