using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Data.Entities;
using Server.Data.Helpers;

namespace Server.Data.SeedData
{
    public interface IDataaseInitialiser
    {
        Task InitialiseAsync();
        Task SeedAsync();
        Task TrySeedAsync();
    }
    public class DatabaseInitializer : IDataaseInitialiser
    {
        private readonly AppDbContext _context;

        public DatabaseInitializer(AppDbContext context)
        {
            this._context = context;
        }
        public async Task InitialiseAsync()
        {
            try
            {
                // Migration Database - Create database if it does not exist
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            if (_context.UserRoles.Any() && _context.Users.Any())
            {
                return;
            }

            var adminRole = new UserRole { RoleName = "Admin" };
            var customerRole = new UserRole { RoleName = "Customer" };
            var driverRole = new UserRole { RoleName = "Driver" };
            List<UserRole> userRoles = new()
            {
                adminRole,
                customerRole,
                driverRole,
            };
            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();
            // Seed Users
            var users = new List<User>();

            for (int i = 1; i <= 10; i++)
            {
                var customer = new User
                {
                    UserName = $"Customer{i}",
                    Password = SecurityUtil.Hash("123456"),
                    FullName = $"Customer{i}",
                    Email = $"customer{i}@gmail.com",
                    Gender = "Male",
                    City = "HCM",
                    Address = "HCM",
                    PhoneNumber = $"012345678{i}",
                    BirthDate = DateTime.Now.AddYears(-20 - i),
                    CreateDate = DateTime.Now.AddMonths(-i),
                    Status = "Active",
                    TypeLogin = "Google",
                    OTPCode = "0",
                    UserRole = customerRole, // Ensure `customerRole` is defined
                };
                users.Add(customer);

                var driver = new User
                {
                    UserName = $"Driver{i}",
                    Password = SecurityUtil.Hash("123456"),
                    FullName = $"Driver{i}",
                    Email = $"driver{i}@gmail.com",
                    Gender = "Male",
                    City = "HCM",
                    Address = "HCM",
                    PhoneNumber = $"018765432{i}",
                    BirthDate = DateTime.Now.AddYears(-30 - i),
                    CreateDate = DateTime.Now.AddMonths(-i),
                    Status = "Active",
                    TypeLogin = "Normal",
                    OTPCode = "0",
                    UserRole = driverRole, // Ensure `driverRole` is defined
                };
                users.Add(driver);
            }
            var admin = new User()
            {
                UserName = "Admin",
                Password = SecurityUtil.Hash("123456"),
                FullName = "Super Admin",
                Email = "admin@gmail.com",
                Gender = "Male",
                City = "HCM",
                Address = "HCM",
                PhoneNumber = $"0135724680",
                BirthDate = DateTime.Now.AddYears(-42),
                CreateDate = DateTime.Now,
                Status = "Active",
                TypeLogin = "Normal",
                OTPCode = "0",
                UserRole = adminRole, // Ensure `admin` is defined
            };
            users.Add(admin);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }
    }
    public static class DatabaseInitialiserExtension
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            // Create IServiceScope to resolve service scope
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

            await initializer.InitialiseAsync();

            // Try to seeding data
            await initializer.SeedAsync();
        }
    }
}
