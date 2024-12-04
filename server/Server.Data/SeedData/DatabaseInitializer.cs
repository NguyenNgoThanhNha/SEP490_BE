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
            try
            {
                // User Roles
                if (!_context.UserRoles.Any())
                {
                    await SeedUserRole();
                }

                // User
                if (!_context.Users.Any())
                {
                    await SeedUser();
                }

                // Company
                if (!_context.Companies.Any())
                {
                    await SeedCompany();
                }

                // Category
                if (!_context.Categorys.Any())
                {
                    await SeedCategories();
                }

                // Product
                if (!_context.Products.Any())
                {
                    await SeedProducts();
                }

                // Service
                if (!_context.Services.Any())
                {
                    await SeedServices();
                }

                // Promotion
                if (!_context.Promotions.Any())
                {
                    await SeedPromotions();
                }

                // Company
                if (!_context.Companies.Any())
                {
                    await SeedCompany();
                }

                // Branch
                if (!_context.Branchs.Any())
                {
                    await SeedBranches();
                }

                // Branch_Product
                if (!_context.Branch_Products.Any())
                {
                    await SeedBranchProducts();
                }

                // Branch_Service
                if (!_context.Branch_Services.Any())
                {
                    await SeedBranchServices();
                }

                // Branch_Promotion
                if (!_context.Branch_Promotions.Any())
                {
                    await SeedBranchPromotions();
                }

                // Staff
                if (!_context.Staffs.Any())
                {
                    await SeedStaff();
                }
                
                // Product Image
                if (!_context.ProductImages.Any())
                {
                    await SeedProductImages();
                }
                
                // Service Image
                if (!_context.ServiceImages.Any())
                {
                    await SeedServiceImages();
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Seed các role
        private async Task SeedUserRole()
        {
            var adminRole = new UserRole { RoleName = "Admin" };
            var managerRole = new UserRole { RoleName = "Manager" };
            var customerRole = new UserRole { RoleName = "Customer" };
            var staffRole = new UserRole { RoleName = "Staff" };

            List<UserRole> userRoles = new()
            {
                adminRole,
                managerRole,
                customerRole,
                staffRole
            };
            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();
        }

        // Seed các user
        private async Task SeedUser()
        {
            var users = new List<User>();

            var adminRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            var managerRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Manager");
            var customerRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            var staffRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Staff");

            // Admin
            var admin = new User
            {
                UserName = "Admin",
                Password = SecurityUtil.Hash("123456"),
                FullName = "Super Admin",
                Email = "admin@gmail.com",
                Gender = "Male",
                City = "HCM",
                Address = "HCM",
                PhoneNumber = "0123456780",
                BonusPoint = 0,
                BirthDate = DateTime.Now.AddYears(-42),
                CreateDate = DateTime.Now,
                Status = "Active",
                TypeLogin = "Normal",
                OTPCode = "0",
                UserRole = adminRole
            };
            users.Add(admin);

            // Manager
            // Thêm 5 người quản lý
            for (int i = 1; i <= 5; i++)
            {
                var manager = new User
                {
                    UserName = $"Manager{i}",
                    Password = SecurityUtil.Hash("123456"),
                    FullName = $"Business Manager {i}",
                    Email = $"manager{i}@gmail.com",
                    Gender = i % 2 == 0 ? "Female" : "Male", // Đổi giới tính cho mỗi người
                    City = "HCM",
                    Address = "HCM",
                    PhoneNumber = $"012345678{i}",
                    BonusPoint = 0,
                    BirthDate = DateTime.Now.AddYears(-35 + i), // Ngày sinh cách nhau 1 năm
                    CreateDate = DateTime.Now,
                    Status = "Active",
                    TypeLogin = "Normal",
                    OTPCode = "0",
                    UserRole = managerRole
                };
                users.Add(manager);
            }

            // Customer
            var customer = new User
            {
                UserName = "Customer",
                Password = SecurityUtil.Hash("123456"),
                FullName = "Regular Customer",
                Email = "customer@gmail.com",
                Gender = "Male",
                City = "HCM",
                Address = "HCM",
                PhoneNumber = "0123456782",
                BonusPoint = 0,
                BirthDate = DateTime.Now.AddYears(-30),
                CreateDate = DateTime.Now,
                Status = "Active",
                TypeLogin = "Google",
                OTPCode = "0",
                UserRole = customerRole
            };
            users.Add(customer);

            // Staff
            for (int i = 1; i <= 10; i++)
            {
                var staff = new User
                {
                    UserName = $"Staff{i}",
                    Password = SecurityUtil.Hash("123456"),
                    FullName = $"Service Staff {i}",
                    Email = $"staff{i}@gmail.com",
                    Gender = "Male",
                    City = "HCM",
                    Address = "HCM",
                    PhoneNumber = $"012345678{i}",
                    BonusPoint = 0,
                    BirthDate = DateTime.Now.AddYears(-25),
                    CreateDate = DateTime.Now,
                    Status = "Active",
                    TypeLogin = "Normal",
                    OTPCode = "0",
                    UserRole = staffRole
                };
                users.Add(staff);
            }

            // Thêm tất cả các user vào cơ sở dữ liệu
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCompany()
        {
            var company = new Company
            {
                Name = "ABC Corporation",
                Address = "123 Main St, HCM",
                Description = "Leading provider of tech solutions",
                PhoneNumber = "0123456789",
                Email = "contact@abccorp.com",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // Thêm công ty vào cơ sở dữ liệu
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCategories()
        {
            var categories = new List<Category>
            {
                // 10 danh mục dịch vụ chăm sóc sắc đẹp
                new Category
                {
                    Name = "Facial Treatment", Description = "Chăm sóc da mặt chuyên sâu", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/facial-treatment.jpg"
                },
                new Category
                {
                    Name = "Anti-Aging", Description = "Liệu pháp chống lão hóa", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/anti-aging.jpg"
                },
                new Category
                {
                    Name = "Acne Treatment", Description = "Điều trị mụn", SkinTypeSuitable = "Oily, Combination",
                    Status = "Active", ImageUrl = "https://example.com/acne-treatment.jpg"
                },
                new Category
                {
                    Name = "Whitening Therapy", Description = "Liệu pháp làm sáng da", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/whitening-therapy.jpg"
                },
                new Category
                {
                    Name = "Skin Detox", Description = "Thải độc da", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/skin-detox.jpg"
                },
                new Category
                {
                    Name = "Moisturizing", Description = "Dưỡng ẩm da", SkinTypeSuitable = "Dry, Normal",
                    Status = "Active", ImageUrl = "https://example.com/moisturizing.jpg"
                },
                new Category
                {
                    Name = "Eye Treatment", Description = "Chăm sóc vùng mắt", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/eye-treatment.jpg"
                },
                new Category
                {
                    Name = "Lifting & Firming", Description = "Nâng cơ và săn chắc", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/lifting-firming.jpg"
                },
                new Category
                {
                    Name = "Body Massage", Description = "Massage cơ thể", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/body-massage.jpg"
                },
                new Category
                {
                    Name = "Hot Stone Therapy", Description = "Liệu pháp đá nóng", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/hot-stone-therapy.jpg"
                },

                // 10 danh mục sản phẩm chăm sóc da
                new Category
                {
                    Name = "Face Cleanser", Description = "Sản phẩm làm sạch da mặt", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/face-cleanser.jpg"
                },
                new Category
                {
                    Name = "Toner", Description = "Toner dưỡng da", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/toner.jpg"
                },
                new Category
                {
                    Name = "Moisturizer", Description = "Kem dưỡng ẩm", SkinTypeSuitable = "Dry, Normal",
                    Status = "Active", ImageUrl = "https://example.com/moisturizer.jpg"
                },
                new Category
                {
                    Name = "Sunscreen", Description = "Kem chống nắng", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/sunscreen.jpg"
                },
                new Category
                {
                    Name = "Serum", Description = "Serum dưỡng da", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/serum.jpg"
                },
                new Category
                {
                    Name = "Exfoliator", Description = "Sản phẩm tẩy da chết", SkinTypeSuitable = "All",
                    Status = "Active", ImageUrl = "https://example.com/exfoliator.jpg"
                },
                new Category
                {
                    Name = "Eye Cream", Description = "Kem dưỡng mắt", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/eye-cream.jpg"
                },
                new Category
                {
                    Name = "Face Mask", Description = "Mặt nạ dưỡng da", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/face-mask.jpg"
                },
                new Category
                {
                    Name = "Lip Balm", Description = "Sản phẩm dưỡng môi", SkinTypeSuitable = "All", Status = "Active",
                    ImageUrl = "https://example.com/lip-balm.jpg"
                },
                new Category
                {
                    Name = "Night Cream", Description = "Kem dưỡng ban đêm", SkinTypeSuitable = "Dry, Normal",
                    Status = "Active", ImageUrl = "https://example.com/night-cream.jpg"
                }
            };

            // Thêm các danh mục vào cơ sở dữ liệu
            await _context.Categorys.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProducts()
        {
            var products = new List<Product>
            {
                // Các sản phẩm chăm sóc da
                new Product
                {
                    Status = "Active", ProductName = "Facial Cleanser",
                    ProductDescription = "Sản phẩm làm sạch sâu da mặt", Price = 150_000m, Quantity = 100,
                    Discount = 0.1m, CategoryId = 1, CompanyId = 1, Dimension = "200ml", Volume = 200m
                },
                new Product
                {
                    Status = "Active", ProductName = "Hydrating Toner", ProductDescription = "Toner dưỡng ẩm",
                    Price = 120_000m, Quantity = 150, Discount = 0.05m, CategoryId = 2, CompanyId = 1,
                    Dimension = "150ml", Volume = 150m
                },
                new Product
                {
                    Status = "Active", ProductName = "Anti-Aging Serum", ProductDescription = "Serum chống lão hóa",
                    Price = 250_000m, Quantity = 80, Discount = 0.15m, CategoryId = 3, CompanyId = 1,
                    Dimension = "50ml", Volume = 50m
                },
                new Product
                {
                    Status = "Active", ProductName = "Sunscreen SPF50+",
                    ProductDescription = "Kem chống nắng bảo vệ da SPF50+", Price = 200_000m, Quantity = 200,
                    Discount = 0.1m, CategoryId = 4, CompanyId = 1, Dimension = "100ml", Volume = 100m
                },
                new Product
                {
                    Status = "Active", ProductName = "Moisturizing Cream", ProductDescription = "Kem dưỡng ẩm sâu",
                    Price = 180_000m, Quantity = 120, Discount = 0.1m, CategoryId = 5, CompanyId = 1,
                    Dimension = "250g", Volume = 250m
                },

                // Các sản phẩm trị liệu
                new Product
                {
                    Status = "Active", ProductName = "Acne Treatment Gel",
                    ProductDescription = "Gel điều trị mụn hiệu quả", Price = 140_000m, Quantity = 90, Discount = 0.2m,
                    CategoryId = 6, CompanyId = 1, Dimension = "30ml", Volume = 30m
                },
                new Product
                {
                    Status = "Active", ProductName = "Skin Brightening Mask", ProductDescription = "Mặt nạ làm sáng da",
                    Price = 160_000m, Quantity = 110, Discount = 0.1m, CategoryId = 7, CompanyId = 1,
                    Dimension = "1 sheet", Volume = null
                },
                new Product
                {
                    Status = "Active", ProductName = "Exfoliating Scrub", ProductDescription = "Tẩy da chết nhẹ nhàng",
                    Price = 130_000m, Quantity = 100, Discount = 0.05m, CategoryId = 8, CompanyId = 1,
                    Dimension = "150ml", Volume = 150m
                },
                new Product
                {
                    Status = "Active", ProductName = "Eye Cream", ProductDescription = "Kem dưỡng vùng mắt",
                    Price = 210_000m, Quantity = 85, Discount = 0.1m, CategoryId = 9, CompanyId = 1,
                    Dimension = "30ml", Volume = 30m
                },
                new Product
                {
                    Status = "Active", ProductName = "Lip Balm", ProductDescription = "Sản phẩm dưỡng môi",
                    Price = 80_000m, Quantity = 200, Discount = 0.05m, CategoryId = 10, CompanyId = 1,
                    Dimension = "15g", Volume = 15m
                },

                // Các sản phẩm chăm sóc cơ thể
                new Product
                {
                    Status = "Active", ProductName = "Body Lotion", ProductDescription = "Sữa dưỡng thể dưỡng ẩm",
                    Price = 170_000m, Quantity = 140, Discount = 0.1m, CategoryId = 1, CompanyId = 1,
                    Dimension = "400ml", Volume = 400m
                },
                new Product
                {
                    Status = "Active", ProductName = "Hand Cream", ProductDescription = "Kem dưỡng da tay",
                    Price = 90_000m, Quantity = 200, Discount = 0.05m, CategoryId = 2, CompanyId = 1,
                    Dimension = "50ml", Volume = 50m
                },
                new Product
                {
                    Status = "Active", ProductName = "Foot Scrub", ProductDescription = "Tẩy da chết cho chân",
                    Price = 150_000m, Quantity = 130, Discount = 0.15m, CategoryId = 3, CompanyId = 1,
                    Dimension = "200ml", Volume = 200m
                },
                new Product
                {
                    Status = "Active", ProductName = "Massage Oil", ProductDescription = "Dầu massage dưỡng da",
                    Price = 200_000m, Quantity = 120, Discount = 0.1m, CategoryId = 4, CompanyId = 1,
                    Dimension = "300ml", Volume = 300m
                },
                new Product
                {
                    Status = "Active", ProductName = "Hot Stone Therapy Kit",
                    ProductDescription = "Bộ liệu pháp đá nóng", Price = 300_000m, Quantity = 50, Discount = 0.2m,
                    CategoryId = 5, CompanyId = 1, Dimension = "1 kit", Volume = null
                },

                // Các sản phẩm dưỡng da ban đêm
                new Product
                {
                    Status = "Active", ProductName = "Night Repair Cream",
                    ProductDescription = "Kem dưỡng phục hồi ban đêm", Price = 250_000m, Quantity = 90, Discount = 0.1m,
                    CategoryId = 6, CompanyId = 1, Dimension = "50ml", Volume = 50m
                },
                new Product
                {
                    Status = "Active", ProductName = "Overnight Mask", ProductDescription = "Mặt nạ dưỡng da ban đêm",
                    Price = 220_000m, Quantity = 75, Discount = 0.15m, CategoryId = 7, CompanyId = 1,
                    Dimension = "100ml", Volume = 100m
                },
                new Product
                {
                    Status = "Active", ProductName = "Night Serum", ProductDescription = "Serum dưỡng da ban đêm",
                    Price = 230_000m, Quantity = 85, Discount = 0.1m, CategoryId = 8, CompanyId = 1,
                    Dimension = "50ml", Volume = 50m
                },
                new Product
                {
                    Status = "Active", ProductName = "Night Eye Cream",
                    ProductDescription = "Kem dưỡng vùng mắt ban đêm", Price = 240_000m, Quantity = 70, Discount = 0.1m,
                    CategoryId = 9, CompanyId = 1, Dimension = "30ml", Volume = 30m
                },
                new Product
                {
                    Status = "Active", ProductName = "Rejuvenating Balm", ProductDescription = "Kem dưỡng phục hồi da",
                    Price = 260_000m, Quantity = 60, Discount = 0.2m, CategoryId = 10, CompanyId = 1,
                    Dimension = "50ml", Volume = 50m
                }
            };

            // Thêm sản phẩm vào cơ sở dữ liệu
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
        }

        private async Task SeedServices()
        {
            var services = new List<Service>
            {
                // Các dịch vụ chăm sóc da mặt
                new Service
                {
                    Status = "Active", Name = "Basic Facial", Description = "Dịch vụ làm sạch và dưỡng da mặt cơ bản",
                    Price = 300_000m, Duration = "60 phút", CategoryId = 1
                },
                new Service
                {
                    Status = "Active", Name = "Advanced Facial", Description = "Liệu pháp dưỡng da mặt chuyên sâu",
                    Price = 500_000m, Duration = "90 phút", CategoryId = 1
                },
                new Service
                {
                    Status = "Active", Name = "Acne Treatment", Description = "Điều trị mụn và phục hồi da",
                    Price = 400_000m, Duration = "75 phút", CategoryId = 2
                },
                new Service
                {
                    Status = "Active", Name = "Hydration Therapy", Description = "Liệu pháp cấp ẩm sâu cho da",
                    Price = 450_000m, Duration = "80 phút", CategoryId = 3
                },
                new Service
                {
                    Status = "Active", Name = "Anti-Aging Facial", Description = "Liệu pháp chống lão hóa da",
                    Price = 600_000m, Duration = "90 phút", CategoryId = 4
                },

                // Các dịch vụ chăm sóc cơ thể
                new Service
                {
                    Status = "Active", Name = "Body Scrub", Description = "Tẩy tế bào chết toàn thân", Price = 350_000m,
                    Duration = "60 phút", CategoryId = 5
                },
                new Service
                {
                    Status = "Active", Name = "Body Wrap", Description = "Liệu pháp quấn nóng toàn thân",
                    Price = 500_000m, Duration = "75 phút", CategoryId = 5
                },
                new Service
                {
                    Status = "Active", Name = "Aromatherapy Massage", Description = "Massage với tinh dầu thư giãn",
                    Price = 400_000m, Duration = "70 phút", CategoryId = 6
                },
                new Service
                {
                    Status = "Active", Name = "Hot Stone Massage", Description = "Massage bằng đá nóng",
                    Price = 600_000m, Duration = "90 phút", CategoryId = 6
                },
                new Service
                {
                    Status = "Active", Name = "Swedish Massage", Description = "Massage kiểu Thụy Điển thư giãn",
                    Price = 450_000m, Duration = "80 phút", CategoryId = 6
                },

                // Các dịch vụ chăm sóc móng
                new Service
                {
                    Status = "Active", Name = "Classic Manicure", Description = "Dịch vụ làm móng tay cơ bản",
                    Price = 200_000m, Duration = "45 phút", CategoryId = 7
                },
                new Service
                {
                    Status = "Active", Name = "Gel Manicure", Description = "Làm móng tay với sơn gel",
                    Price = 300_000m, Duration = "60 phút", CategoryId = 7
                },
                new Service
                {
                    Status = "Active", Name = "Classic Pedicure", Description = "Dịch vụ làm móng chân cơ bản",
                    Price = 250_000m, Duration = "50 phút", CategoryId = 8
                },
                new Service
                {
                    Status = "Active", Name = "Spa Pedicure", Description = "Chăm sóc móng chân và massage chân",
                    Price = 350_000m, Duration = "75 phút", CategoryId = 8
                },
                new Service
                {
                    Status = "Active", Name = "Nail Art Design", Description = "Trang trí móng nghệ thuật",
                    Price = 200_000m, Duration = "45 phút", CategoryId = 9
                },

                // Các dịch vụ chăm sóc tóc
                new Service
                {
                    Status = "Active", Name = "Hair Wash & Blow Dry", Description = "Gội và sấy tạo kiểu tóc",
                    Price = 150_000m, Duration = "40 phút", CategoryId = 10
                },
                new Service
                {
                    Status = "Active", Name = "Hair Treatment", Description = "Dưỡng và phục hồi tóc hư tổn",
                    Price = 300_000m, Duration = "60 phút", CategoryId = 10
                },
                new Service
                {
                    Status = "Active", Name = "Hair Cut", Description = "Cắt tóc và tạo kiểu", Price = 200_000m,
                    Duration = "45 phút", CategoryId = 10
                },
                new Service
                {
                    Status = "Active", Name = "Hair Color", Description = "Nhuộm tóc theo màu yêu thích",
                    Price = 400_000m, Duration = "90 phút", CategoryId = 10
                },
                new Service
                {
                    Status = "Active", Name = "Keratin Treatment", Description = "Phục hồi tóc bằng liệu pháp keratin",
                    Price = 500_000m, Duration = "100 phút", CategoryId = 10
                }
            };

            // Thêm dịch vụ vào cơ sở dữ liệu
            await _context.Services.AddRangeAsync(services);
            await _context.SaveChangesAsync();
        }

        private async Task SeedBranches()
        {
            var branches = new List<Branch>
            {
                new Branch
                {
                    BranchName = "ABC Spa - Chi nhánh Quận 1",
                    BranchAddress = "123 Đường Lê Lợi, Quận 1, TP.HCM",
                    BranchPhone = "0123456789",
                    LongAddress = "106.7009",
                    LatAddress = "10.7769",
                    Status = "Active",
                    ManagerId = 2, // ID của quản lý chi nhánh này
                    CompanyId = 1 // ID của công ty đã seed trước đó
                },
                new Branch
                {
                    BranchName = "ABC Spa - Chi nhánh Quận 3",
                    BranchAddress = "456 Đường Nguyễn Đình Chiểu, Quận 3, TP.HCM",
                    BranchPhone = "0123456790",
                    LongAddress = "106.6834",
                    LatAddress = "10.7757",
                    Status = "Active",
                    ManagerId = 3,
                    CompanyId = 1
                },
                new Branch
                {
                    BranchName = "ABC Spa - Chi nhánh Quận 5",
                    BranchAddress = "789 Đường Trần Hưng Đạo, Quận 5, TP.HCM",
                    BranchPhone = "0123456791",
                    LongAddress = "106.6665",
                    LatAddress = "10.7564",
                    Status = "Active",
                    ManagerId = 4,
                    CompanyId = 1
                },
                new Branch
                {
                    BranchName = "ABC Spa - Chi nhánh Thủ Đức",
                    BranchAddress = "321 Đường Kha Vạn Cân, TP. Thủ Đức, TP.HCM",
                    BranchPhone = "0123456792",
                    LongAddress = "106.7505",
                    LatAddress = "10.8414",
                    Status = "Active",
                    ManagerId = 5,
                    CompanyId = 1
                },
                new Branch
                {
                    BranchName = "ABC Spa - Chi nhánh Bình Thạnh",
                    BranchAddress = "654 Đường Xô Viết Nghệ Tĩnh, Quận Bình Thạnh, TP.HCM",
                    BranchPhone = "0123456793",
                    LongAddress = "106.7038",
                    LatAddress = "10.8039",
                    Status = "Active",
                    ManagerId = 6,
                    CompanyId = 1
                }
            };

            // Thêm các chi nhánh vào cơ sở dữ liệu
            await _context.Branchs.AddRangeAsync(branches);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPromotions()
        {
            var random = new Random();

            // Danh sách mẫu tên và mô tả khuyến mãi
            var promotionNames = new[]
            {
                "Holiday Sale",
                "Black Friday Discount",
                "New Year Promotion",
                "Summer Special Offer",
                "Flash Sale",
                "Clearance Discount",
                "Buy More Save More"
            };

            var promotionDescriptions = new[]
            {
                "Get amazing discounts this holiday season!",
                "Biggest sale of the year on Black Friday!",
                "Celebrate the new year with exclusive offers!",
                "Cool down this summer with hot discounts!",
                "Limited time offer – don't miss out!",
                "Clearance sale on selected items!",
                "Buy more and save more with our special offer!"
            };

            var promotions = new List<Promotion>();

            for (int i = 0; i < 20; i++) // Tạo 20 chương trình khuyến mãi
            {
                var isPercentage = random.Next(0, 2) == 1; // 50% xác suất là giảm giá theo % hoặc số tiền cố định
                var discountAmount =
                    isPercentage ? random.Next(5, 51) : random.Next(10000, 500001); // 5-50% hoặc 10,000 - 500,000

                var startDate =
                    DateTime.Now.AddDays(random.Next(-30, 1)); // Ngày bắt đầu trong khoảng 30 ngày trước đến hôm nay
                var endDate = startDate.AddDays(random.Next(10, 31)); // Ngày kết thúc từ 10-30 ngày sau ngày bắt đầu

                promotions.Add(new Promotion
                {
                    PromotionName = promotionNames[random.Next(promotionNames.Length)],
                    PromotionDescription = promotionDescriptions[random.Next(promotionDescriptions.Length)],
                    DiscountPercent = random.Next(10, 30),
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

            // Thêm danh sách promotions vào cơ sở dữ liệu
            await _context.Promotions.AddRangeAsync(promotions);
            await _context.SaveChangesAsync();
        }


        private async Task SeedBranchProducts()
        {
            var random = new Random();
            var branches = await _context.Branchs.ToListAsync();
            var products = await _context.Products.ToListAsync();

            var branchProducts = new List<Branch_Product>();

            foreach (var branch in branches)
            {
                // Chọn ngẫu nhiên số lượng sản phẩm từ 16 đến 20 cho mỗi chi nhánh
                int productCount = random.Next(16, 21);

                // Chọn ngẫu nhiên các sản phẩm cho chi nhánh này
                var selectedProducts = products.OrderBy(x => random.Next()).Take(productCount);

                foreach (var product in selectedProducts)
                {
                    var branchProduct = new Branch_Product
                    {
                        BranchId = branch.BranchId,
                        ProductId = product.ProductId,
                        Status = "Active",
                        StockQuantity = random.Next(5, 51), // Số lượng tồn kho từ 5 đến 50
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    branchProducts.Add(branchProduct);
                }
            }

            // Thêm danh sách branchProducts vào cơ sở dữ liệu
            await _context.Branch_Products.AddRangeAsync(branchProducts);
            await _context.SaveChangesAsync();
        }

        private async Task SeedBranchServices()
        {
            var random = new Random();
            var branches = await _context.Branchs.ToListAsync();
            var services = await _context.Services.ToListAsync();

            var branchServices = new List<Branch_Service>();

            foreach (var branch in branches)
            {
                // Chọn ngẫu nhiên số lượng dịch vụ từ 16 đến 21 cho mỗi chi nhánh
                int serviceCount = random.Next(16, 21);

                // Chọn ngẫu nhiên các dịch vụ cho chi nhánh này
                var selectedServices = services.OrderBy(x => random.Next()).Take(serviceCount);

                foreach (var service in selectedServices)
                {
                    var branchService = new Branch_Service
                    {
                        BranchId = branch.BranchId,
                        ServiceId = service.ServiceId,
                        Status = "Active",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    branchServices.Add(branchService);
                }
            }

            // Thêm danh sách branchServices vào cơ sở dữ liệu
            await _context.Branch_Services.AddRangeAsync(branchServices);
            await _context.SaveChangesAsync();
        }

        private async Task SeedBranchPromotions()
        {
            var random = new Random();

            // Lấy danh sách các chi nhánh và chương trình khuyến mãi
            var branches = await _context.Branchs.ToListAsync();
            var promotions = await _context.Promotions.ToListAsync();

            var branchPromotions = new List<Branch_Promotion>();

            foreach (var branch in branches)
            {
                // Chọn ngẫu nhiên số lượng chương trình khuyến mãi từ 5 đến 10 cho mỗi chi nhánh
                int promotionCount = random.Next(5, 11);

                // Chọn ngẫu nhiên các chương trình khuyến mãi cho chi nhánh này
                var selectedPromotions = promotions.OrderBy(x => random.Next()).Take(promotionCount);

                foreach (var promotion in selectedPromotions)
                {
                    var branchPromotion = new Branch_Promotion
                    {
                        BranchId = branch.BranchId,
                        PromotionId = promotion.PromotionId,
                        Status = "Active", // Có thể là "Pending" hoặc trạng thái khác
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    branchPromotions.Add(branchPromotion);
                }
            }

            // Thêm danh sách branchPromotions vào cơ sở dữ liệu
            await _context.Branch_Promotions.AddRangeAsync(branchPromotions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedStaff()
        {
            var staffUsers = await _context.Users.Where(u => u.RoleID == 4).ToListAsync();

            // Lấy danh sách các Branch có sẵn
            var branches = await _context.Branchs.ToListAsync();
            if (!branches.Any())
            {
                throw new Exception("No branches found in the database. Please seed branches first.");
            }

            // Ánh xạ Staff vào các Branch
            var branchIds = branches.Select(b => b.BranchId).ToList();
            var random = new Random();
            var staffList = new List<Staff>();

            foreach (var user in staffUsers)
            {
                staffList.Add(new Staff
                {
                    UserId = user.UserId, // Lấy ID của user tương ứng
                    BranchId = branchIds[random.Next(branchIds.Count)], // Gán Branch ngẫu nhiên
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

            // Lưu danh sách Staff vào cơ sở dữ liệu
            await _context.Staffs.AddRangeAsync(staffList);
            await _context.SaveChangesAsync();
        }
        
        private async Task SeedProductImages()
        {
            // Lấy danh sách tất cả các sản phẩm hiện có
            var products = await _context.Products.ToListAsync();

            // Khởi tạo danh sách ProductImages
            var productImages = new List<ProductImages>();

            // Danh sách URL ảnh ngẫu nhiên
            var randomUrls = new List<string>
            {
                "https://png.pngtree.com/png-vector/20240802/ourlarge/pngtree-tranquil-spa-products-png-image_13337692.png",
                "https://png.pngtree.com/png-vector/20240205/ourlarge/pngtree-spa-products-spa-concept-png-image_11547713.png",
                "https://png.pngtree.com/png-vector/20240205/ourlarge/pngtree-spa-products-spa-concept-png-image_11547710.png",
                "https://png.pngtree.com/png-vector/20240205/ourlarge/pngtree-spa-products-spa-concept-png-image_11547711.png",
                "https://png.pngtree.com/thumb_back/fw800/background/20230903/pngtree-all-natural-beauty-spa-products-image_13209386.jpg",
                "https://png.pngtree.com/png-vector/20240619/ourmid/pngtree-natural-spa-products-soaps-with-brushes-and-plants-png-image_12797870.png"
            };

            // Tạo 3 ảnh cho mỗi sản phẩm
            foreach (var product in products)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var randomImageUrl = randomUrls[new Random().Next(randomUrls.Count)]; // Chọn URL ngẫu nhiên
                    productImages.Add(new ProductImages
                    {
                        ProductId = product.ProductId,
                        image = randomImageUrl
                    });
                }
            }

            // Thêm dữ liệu vào bảng ProductImages
            await _context.ProductImages.AddRangeAsync(productImages);
            await _context.SaveChangesAsync();
        }

        private async Task SeedServiceImages()
        {
            // Lấy danh sách tất cả các dịch vụ hiện có
            var services = await _context.Services.ToListAsync();

            // Khởi tạo danh sách ServiceImages
            var serviceImages = new List<ServiceImages>();

            // Danh sách URL ảnh ngẫu nhiên
            var randomUrls = new List<string>
            {
                "https://diva.edu.vn/wp-content/uploads/2023/10/spa-gom-nhung-dich-vu-gi-1.jpeg",
                "https://posapp.vn/wp-content/uploads/2020/07/mun-2.jpg",
                "https://bizweb.dktcdn.net/100/385/697/files/spa-co-nhung-dich-vu-gi-5.jpg?v=1711425862233",
                "https://chefjob.vn/wp-content/uploads/2020/07/dich-vu-spa-trong-khach-san.jpg",
                "https://easysalon.vn/wp-content/uploads/2019/11/tim-hieu-cac-mo-hinh-spa-tren-the-gioi-5.png",
                "https://thanhtrucmed.com/wp-content/uploads/2024/05/spa-cham-soc-da-2_1684512418.jpg"
            };

            // Tạo 3 ảnh cho mỗi dịch vụ
            foreach (var service in services)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var randomImageUrl = randomUrls[new Random().Next(randomUrls.Count)]; // Chọn URL ngẫu nhiên
                    serviceImages.Add(new ServiceImages
                    {
                        ServiceId = service.ServiceId,
                        image = randomImageUrl
                    });
                }
            }

            // Thêm dữ liệu vào bảng ServiceImages
            await _context.ServiceImages.AddRangeAsync(serviceImages);
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