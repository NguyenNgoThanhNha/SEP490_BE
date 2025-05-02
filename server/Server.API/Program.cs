using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Server.API.Extensions;
using Server.Business.Middlewares;
using Server.Business.Worker;
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

            builder.Services.AddHostedService<OrderStatusUpdateService>();
            builder.Services.AddSingleton<OrderStatusUpdateService>();
            builder.Services.AddHostedService<UserRoutineStepStatusUpdateService>();
            builder.Services.AddSingleton<UserRoutineStepStatusUpdateService>();
            builder.Services.AddHostedService<UserRoutineStatusUpdateService>();
            builder.Services.AddSingleton<UserRoutineStatusUpdateService>();
            builder.Services.AddHostedService<AppointmentReminderNoRealStaffWorker>();
            builder.Services.AddSingleton<AppointmentReminderNoRealStaffWorker>();




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
            builder.Services.AddElasticSearchT(builder.Configuration);


            builder.Services.AddHttpClient("AIML", client =>
            {
                client.BaseAddress = new Uri("https://api.aimlapi.com/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["AIML:ApiKey"]}");
            });
            builder.Services.AddHttpContextAccessor();

            /*var redisConnectionString = builder.Configuration.GetSection("RedisSetting:RedisConnection").Value; */
            var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");           
            Console.WriteLine($"RedisDbConnection Program: {redisConnectionString}");

            builder.Services.AddStackExchangeRedisCache(option =>
            {
                option.InstanceName = "RedisCache_";
                option.Configuration = redisConnectionString;
            });

            var parts = redisConnectionString.Split(':', 2);
            var firstPart = parts[0];
            var secondPart = parts.Length > 1 ? parts[1] : "";


            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                if (string.IsNullOrEmpty(redisConnectionString))
                {
                    throw new Exception("❌ Redis connection string is missing!");
                }

                var configOptions = new ConfigurationOptions
                {
                    EndPoints = { firstPart, secondPart },
                    ConnectRetry = 5,
                    ReconnectRetryPolicy = new ExponentialRetry(5000),
                    Ssl = false,
                    AbortOnConnectFail = false,
                    ConnectTimeout = 5000,
                    SyncTimeout = 5000,
                    DefaultDatabase = 0,
                    KeepAlive = 500
                };

                return ConnectionMultiplexer.Connect(configOptions);
            });

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
            app.MapHub<NotificationHub>("/notification");

            app.Run();
        }
    }
}
