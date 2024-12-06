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
            /*var payOsSetting = configuration.GetSection(nameof(PayOSSetting)).Get<PayOSSetting>();
            services.Configure<PayOSSetting>(configuration.GetSection(nameof(PayOSSetting)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<PayOSSetting>>().Value);*/

            // mail
            services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

            // cloud
            services.Configure<CloundSettings>(configuration.GetSection(nameof(CloundSettings)));

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

            /*            Register UnitOfWorks*/
            services.AddScoped<UnitOfWorks>();

            /*            Init Data*/
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


            return services;
        }
    };
}
