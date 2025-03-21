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
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Server.Data.MongoDb.Repository;
using Server.Data.Repositories;
using Microsoft.AspNetCore.Cors.Infrastructure;

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

            //Add Mapper
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new ProfilesMapper());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // jwt
            var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

            // zalo pay
            /*var zaloPaySetting = configuration.GetSection(nameof(ZaloPaySetting)).Get<ZaloPaySetting>();
            services.Configure<ZaloPaySetting>(configuration.GetSection(nameof(ZaloPaySetting)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<ZaloPaySetting>>().Value);*/

            // payOs
            var payOsSetting = configuration.GetSection(nameof(PayOSSetting)).Get<PayOSSetting>();
            services.Configure<PayOSSetting>(configuration.GetSection(nameof(PayOSSetting)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<PayOSSetting>>().Value);

            // mail
            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

            // cloud
            services.Configure<CloundSettings>(configuration.GetSection(nameof(CloundSettings)));

            // botchat
            services.Configure<BotChatSetting>(configuration.GetSection(nameof(BotChatSetting)));

            // skin analysis AI
            services.Configure<AISkinSetting>(configuration.GetSection(nameof(AISkinSetting)));

            // ElasticSettings
            services.Configure<ElasticSettings>(configuration.GetSection(nameof(ElasticSettings)));
            
            // MongoDbSetting
            services.Configure<MongoDbSetting>(configuration.GetSection(nameof(MongoDbSetting)));
            
            /*services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
            });*/
            services.AddAuthorization();
            

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                    {
                        options.Authority = "https://dev-urvottxjuerzz313.us.auth0.com/";
                        options.Audience = "L&L";
                    }
                });

            services.Configure<CloundSettings>(configuration.GetSection(nameof(CloundSettings)));

            services.AddDbContext<AppDbContext>(opt =>
            {
                var connectionString = configuration.GetConnectionString("MySqlConnection");
                Console.WriteLine($"MySQLDbConnection DbConnect: {connectionString}");
                opt.UseMySQL(connectionString);
            });

            /*Config repository*/
            services.AddScoped(typeof(IRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped(typeof(IRepositoryMongoDB<>), typeof(RepositoryMongoDb<>));
            services.AddScoped<CustomerRepository>();
            services.AddScoped<MessageRepository>();
            services.AddScoped<ChannelsRepository>();
            services.AddScoped<ScheduleRepository>();

            /*Config SignalR*/
            services.AddSignalR();
            
            /* Register UnitOfWorks*/
            services.AddScoped<UnitOfWorks>();

            /* Init Data*/
            services.AddScoped<DatabaseInitializer>();

            /*Config Service*/
            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<MailService>();
            services.AddScoped<PromotionService>();
            services.AddScoped<BranchPromotionService>();
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
            services.AddScoped<BlogCommentService>();
            services.AddScoped<ServiceCategoryService>();
            services.AddScoped<RoutineService>();
            services.AddScoped<WorkScheduleService>();
            services.AddScoped<ShiftService>();
            services.AddScoped<StaffLeaveService>();
            services.AddScoped<MongoDbService>();
            services.AddScoped<FeedbackService>();
            services.AddScoped<VoucherService>();
            services.AddHttpContextAccessor();
            services.AddScoped<CartService>();
            return services;
        }
    };
}
