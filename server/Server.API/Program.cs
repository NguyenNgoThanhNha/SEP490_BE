using MailKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Server.API.Extensions;
using Server.Business.Middlewares;
using Server.Business.Services;
using Server.Data.Entities;
using Server.Data.SeedData;
using Service.Business.Services;
using StackExchange.Redis;

namespace Server.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddTransient<IAIMLService, AIMLService>();

            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Mock API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });


            builder.Services.AddCors(option =>
                option.AddPolicy("CORS", builder =>
                    builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((host) => true)));

            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            builder.Services.AddElasticSearch(builder.Configuration);
            builder.Services.AddHttpClient("AIML", client =>
            {
                client.BaseAddress = new Uri("https://api.aimlapi.com/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["AIML:ApiKey"]}");
            });
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("RedisSetting:ConnectionString")));
            var app = builder.Build();
            // Hook into application lifetime events and trigger only application fully started 
            app.Lifetime.ApplicationStarted.Register(async () =>
            {
                // Database Initialiser
                await app.InitialiseDatabaseAsync();
            });

            // Configure the HTTP request pipeline.
            /*            if (app.Environment.IsDevelopment())
                        {
                            var connectionString = builder.Configuration.GetConnectionString("PgDbConnection");
                            Console.WriteLine($"PgDbConnection Program: {connectionString}");
                            await using (var scope = app.Services.CreateAsyncScope())
                            {
                                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                                await dbContext.Database.MigrateAsync();
                            }

                            app.UseSwagger();
                            app.UseSwaggerUI();
                        }*/

            var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
            Console.WriteLine($"MySQLDbConnection Program: {connectionString}");
            await using (var scope = app.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("CORS");

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionMiddleware>();


            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<ChatHubs>("/chat");
            
            app.Run();
        }
    }
}

