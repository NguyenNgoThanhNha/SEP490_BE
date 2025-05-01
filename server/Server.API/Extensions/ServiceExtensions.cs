using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Business.Mappers;
using Server.Business.Middlewares;
using Server.Business.Services;
using Server.Business.Ultils;
using Server.Data.Base;
using Server.Data.Entities;
using Server.Data.SeedData;
using Server.Data.UnitOfWorks;
using Server.Data.MongoDb.Repository;
using Server.Data.Repositories;
using Server.Business.Worker;
using Nest;
using System.Text;

namespace Server.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ExceptionMiddleware>();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Add AutoMapper
            var mapperConfig = new MapperConfiguration(mc => mc.AddProfile(new ProfilesMapper()));
            services.AddSingleton(mapperConfig.CreateMapper());

            // JWT Settings
            var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

            // PayOS Settings
            services.Configure<PayOSSetting>(configuration.GetSection(nameof(PayOSSetting)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<PayOSSetting>>().Value);

            // Other settings
            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
            services.Configure<CloundSettings>(configuration.GetSection(nameof(CloundSettings)));
            services.Configure<BotChatSetting>(configuration.GetSection(nameof(BotChatSetting)));
            services.Configure<AISkinSetting>(configuration.GetSection(nameof(AISkinSetting)));
            services.Configure<ElasticSettings>(configuration.GetSection(nameof(ElasticSettings)));
            services.Configure<MongoDbSetting>(configuration.GetSection(nameof(MongoDbSetting)));
            services.Configure<RedisSetting>(configuration.GetSection(nameof(RedisSetting)));

            services.AddAuthorization();

            // JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
                options.Authority = "https://dev-urvottxjuerzz313.us.auth0.com/";
                options.Audience = "L&L";
            });

            // Database MySQL
            services.AddDbContext<AppDbContext>(opt =>
            {
                var connectionString = configuration.GetConnectionString("MySqlConnection");
                Console.WriteLine($"MySQLDbConnection DbConnect: {connectionString}");
                opt.UseMySQL(connectionString);
            });

            // Config Repositories
            services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped(typeof(IRepositoryMongoDB<>), typeof(RepositoryMongoDb<>));
            services.AddScoped<CustomerRepository>();
            services.AddScoped<MessageRepository>();
            services.AddScoped<ChannelsRepository>();

            // Config SignalR
            services.AddSignalR();

            // Unit of Work
            services.AddScoped<UnitOfWorks>();

            // Database Initializer
            services.AddScoped<DatabaseInitializer>();

            // Register Services
            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<MailService>();
            services.AddScoped<PromotionService>();
            services.AddScoped<BranchPromotionService>();
            services.AddScoped<BranchProductService>();
            services.AddScoped<BranchServiceService>();
            services.AddScoped<CloudianryService>();
            services.AddScoped<ProductService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<AppointmentsService>();
            services.AddScoped<BlogService>();
            services.AddScoped<StaffService>();
            services.AddScoped<CustomerService>();
            services.AddScoped<ServiceService>();
            services.AddScoped<OrderService>();
            services.AddScoped<BranchService>();
            services.AddScoped<OrderDetailService>();
            services.AddScoped<BotchatService>();
            services.AddScoped<SkinAnalyzeService>();
            services.AddScoped<AppointmentReminderWorker>();
            services.AddScoped<RedisKeepAliveService>();
            services.AddScoped<BlogCommentService>();
            services.AddScoped<ServiceCategoryService>();
            services.AddScoped<RoutineService>();
            services.AddScoped<WorkScheduleService>();
            services.AddScoped<ShiftService>();
            services.AddScoped<StaffLeaveService>();
            services.AddScoped<MongoDbService>();
            services.AddScoped<FeedbackService>();
            services.AddScoped<VoucherService>();
            services.AddScoped<CartService>();
            services.AddScoped<AppointmentFeedbackService>();
            services.AddScoped<ProductFeedbackService>();
            services.AddScoped<ServiceFeedbackService>();
            services.AddScoped<SkincareRoutineService>();
            services.AddScoped<SkinCareRoutineStepService>();
            services.AddScoped<ServiceRoutineService>();
            services.AddScoped<ServiceRoutineStepService>();
            services.AddScoped<ProductRoutineService>();
            services.AddScoped<ProductRoutineStepService>();
            services.AddScoped<UserRoutineStepStatusUpdateService>();
            services.AddScoped<SkinHealthService>();
            services.AddScoped<UserRoutineLoggerService>();
            services.AddScoped<NotificationServices>();

            services.AddHttpContextAccessor();

            return services;
        }           

    }
}
