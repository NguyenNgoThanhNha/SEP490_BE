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
                
                // ServiceCategory
                if (!_context.ServiceCategory.Any())
                {
                    await SeedServiceCategories();
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

                if (!_context.Vouchers.Any())
                {
                    await SeedVoucherData();
                }

                if (!_context.Orders.Any() && !_context.OrderDetails.Any())
                {
                    await SeedOrderData();
                }

                await Task.CompletedTask;

                if (!_context.Blogs.Any())
                {
                    await SeedBlogs();
                }
                
                if (!_context.SkincareRoutines.Any())
                {
                    await SeedSkincareRoutines();
                }
                
                if (!_context.BedType.Any())
                {
                    await SeedBedTypes();
                }
                
                if (!_context.Room.Any())
                {
                    await SeedRooms();
                }
                
                if (!_context.Bed.Any())
                {
                    await SeedBeds();
                }
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
                new Category
                {
                    Name = "Cleanser", 
                    Description = "Sản phẩm làm sạch da mặt, loại bỏ bụi bẩn và dầu thừa", 
                    Status = "Active", 
                    ImageUrl = "https://example.com/facial-treatment.jpg"
                },
                new Category
                {
                    Name = "Toner", 
                    Description = "Cân bằng độ pH cho da, giúp da mềm mại và sẵn sàng hấp thụ dưỡng chất", 
                    Status = "Active", 
                    ImageUrl = "https://example.com/anti-aging.jpg"
                },
                new Category
                {
                    Name = "Serum", 
                    Description = "Tinh chất cô đặc giúp điều trị các vấn đề về da như mụn và thâm", 
                    Status = "Active", 
                    ImageUrl = "https://example.com/acne-treatment.jpg"
                },
                new Category
                {
                    Name = "Moisturizer", 
                    Description = "Kem dưỡng ẩm giúp cung cấp độ ẩm cần thiết cho da", 
                    Status = "Active", 
                    ImageUrl = "https://example.com/whitening-therapy.jpg"
                },
                new Category
                {
                    Name = "Sun Cream", 
                    Description = "Kem chống nắng bảo vệ da khỏi tác hại của tia UV", 
                    Status = "Active",
                    ImageUrl = "https://example.com/skin-detox.jpg"
                },
                new Category
                {
                    Name = "Mask", 
                    Description = "Mặt nạ dưỡng da giúp cung cấp dưỡng chất và độ ẩm sâu", 
                    Status = "Active", 
                    ImageUrl = "https://example.com/moisturizing.jpg"
                },
                new Category
                {
                    Name = "Exfoliants", 
                    Description = "Tẩy tế bào chết, làm sạch lỗ chân lông và cải thiện kết cấu da", 

                    Status = "Active", 
                    ImageUrl = "https://example.com/eye-treatment.jpg"
                },
                new Category
                {
                    Name = "Body", 
                    Description = "Sản phẩm chăm sóc cơ thể giúp da mịn màng và săn chắc", 

                    Status = "Active", 
                    ImageUrl = "https://example.com/lifting-firming.jpg"
                },
                new Category
                {
                    Name = "Shampoo", 
                    Description = "Dầu gội giúp làm sạch tóc và da đầu", 
 
                    Status = "Active",
                    ImageUrl = "https://example.com/body-massage.jpg"
                },
                new Category
                {
                    Name = "Conditioner", 
                    Description = "Dầu xả giúp tóc mềm mượt và chắc khỏe", 
                    Status = "Active", 
                    ImageUrl = "https://example.com/hot-stone-therapy.jpg"
                },
            };

            // Thêm các danh mục vào cơ sở dữ liệu
            await _context.Categorys.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }
        
        private async Task SeedServiceCategories()
        {
            var serviceCategories = new List<ServiceCategory>
            {
                new ServiceCategory
                {
                    Name = "Cleanser", 
                    Description = "Sản phẩm làm sạch da mặt, loại bỏ bụi bẩn và dầu thừa", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/facial-treatment.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Toner", 
                    Description = "Cân bằng độ pH cho da, giúp da mềm mại và sẵn sàng hấp thụ dưỡng chất", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/anti-aging.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Serum", 
                    Description = "Tinh chất cô đặc giúp điều trị các vấn đề về da như mụn và thâm", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/acne-treatment.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Moisturizer", 
                    Description = "Kem dưỡng ẩm giúp cung cấp độ ẩm cần thiết cho da", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/whitening-therapy.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Sun Cream", 
                    Description = "Kem chống nắng bảo vệ da khỏi tác hại của tia UV", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/skin-detox.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Mask", 
                    Description = "Mặt nạ dưỡng da giúp cung cấp dưỡng chất và độ ẩm sâu", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/moisturizing.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Exfoliants", 
                    Description = "Tẩy tế bào chết, làm sạch lỗ chân lông và cải thiện kết cấu da", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/eye-treatment.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Body", 
                    Description = "Sản phẩm chăm sóc cơ thể giúp da mịn màng và săn chắc", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/lifting-firming.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Shampoo", 
                    Description = "Dầu gội giúp làm sạch tóc và da đầu", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/body-massage.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Conditioner", 
                    Description = "Dầu xả giúp tóc mềm mượt và chắc khỏe", 
                    Status = "Active", 
                    Thumbnail = "https://example.com/hot-stone-therapy.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
            };

            // Thêm các danh mục vào cơ sở dữ liệu
            await _context.ServiceCategory.AddRangeAsync(serviceCategories);
            await _context.SaveChangesAsync();
        }


        private async Task SeedProducts()
        {
            var products = new List<Product>
            {
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Remedy Cream To Oil",
                    ProductDescription = "Innovative, gentle cleanser with cream-to-oil transformative texture. Formulated with Marula Oil, it is ideal for sensible, sensitized skins prone to redness.",
                    Price = 1_050_000m, // Giá sản phẩm là 1.050.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Comfortzone
                    Dimension = "150ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Essential Face Wash",
                    ProductDescription = "A creamy cleanser that gently removes make-up while restoring a natural glow to the skin.",
                    Price = 990_000m, // Giá sản phẩm là 990.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Comfortzone
                    Dimension = "150ml",
                    Volume = 200m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Active Pureness Cleansing Gel",
                    ProductDescription = "This cleansing gel contains 3% gluconolactone, which deeply cleanses the epidermis, exfoliates dead skin, and clears the pores. Ideal for acne-prone and oily ​skin.",
                    Price = 1_120_000m, // Giá sản phẩm là 1.120.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Comfortzone
                    Dimension = "200ml",
                    Volume = 200m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Clearing Skin Wash",
                    ProductDescription = "Foaming cleanser helps clear skin and reduce visible skin aging. Salicylic Acid, a Beta Hydroxy Acid, stimulates natural exfoliation to help clear clogged follicles and smooth away dullness that contributes to visible skin aging. Menthol and Camphor help cool the skin. Contains extracts of Balm Mint, Eucalyptus and Tea Tree. Skin is left clean and prepped for optimal absorption of Active Clearing treatment ingredients.",
                    Price = 1_400_000m, // Giá sản phẩm là 1.400.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Dermalogica
                    Dimension = "250ml",
                    Volume = 250m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Oil To Foam Cleanser",
                    ProductDescription = "Transformative oil to foam cleanser removes make-up, sunscreen, and debris while cleansing skin in one step for ultra-clean, healthy-looking skin.",
                    Price = 1_950_000m, // Giá sản phẩm là 1.950.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Dermalogica
                    Dimension = "250ml",
                    Volume = 250m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Micellar Prebiotic PreCleanse",
                    ProductDescription = "Deep-cleansing oil melts make-up and impurities from skin. Achieve ultra clean and healthy-looking skin with the Double Cleanse regimen that begins with PreCleanse. Thoroughly melt away layers of excess sebum (oil), sunscreen, waterproof make-up, environmental pollutants and residual products that build up on skin throughout the day. Ideal even for oily skin.",
                    Price = 1_630_000m, // Giá sản phẩm là 1.630.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Dermalogica
                    Dimension = "150ml",
                    Volume = 150m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Kombucha Microbiome Foaming Cleanser",
                    ProductDescription = "Refresh and purify skin to reveal a healthy-looking glow. Crafted with targeted micellar technology, this liquid-to-foam cleanser gently removes impurities without over-stripping moisture from the skin. Kombucha, ginger, white tea and jasmine work in unison to refresh and balance the microbiome and look of the skin without compromising the moisture barrier. Suitable for all skin types.",
                    Price = 1_170_000m, // Giá sản phẩm là 1.170.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "150ml",
                    Volume = 150m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Monoi Age Corrective Exfoliating Cleanser",
                    ProductDescription = "Wash away impurities and remove surface debris to experience smooth skin like never before. Infused with exotic monoi oil and made with our unique Natural Retinol Alternative, this refining cleanser rejuvenates skin for a smooth and lifted appearance.",
                    Price = 1_200_000m, // Giá sản phẩm là 1.200.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Body
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "250ml",
                    Volume = 250m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Acne Advanced Cleansing Foam",
                    ProductDescription = "This unique liquid-to-foam cleanser provides lightweight acne cleansing action, effectively preventing acne breakouts and clearing blocked pores. Featuring time-release encapsulated salicylic acid combined with a natural herb blend, this cleanser soothes and tones to address the look of uneven skin.",
                    Price = 1_250_000m, // Giá sản phẩm là 1.250.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Shampoo
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "150ml",
                    Volume = 150m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Lemon Grass Cleanser",
                    ProductDescription = "The olive oil, sunflower and flax seed in our Lemon Grass Cleanser gently remove impurities from skin while organic and biodynamic herbal ingredients help calm and soothe. This hypoallergenic cream cleanser is perfect for sensitive or dehydrated skin.",
                    Price = 1_450_000m, // Giá sản phẩm là 1.450.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Conditioner
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "50ml",
                    Volume = 50m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Charcoal Exfoliating Gel Cleanser",
                    ProductDescription = "Formulated with charcoal, malachite gemstones and blue matcha, this supercharged purifying cleanser transforms from a gel to an exfoliating lather to wash away impurities and reveal a balanced complexion.",
                    Price = 1_490_000m, // Giá sản phẩm là 1.490.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Exfoliants
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "150ml",
                    Volume = 150m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Beplain Mung Bean pH-Balanced Cleansing Foam",
                    ProductDescription = "A gentle daily facial cleanser with 33% Mung bean extract that helps cleanse impurities while keeping the skin hydrated and comfortable.",
                    Price = 355_000m, // Giá sản phẩm là 355.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // K Beauty (Giả định CompanyId là 4)
                    Dimension = "150ml",
                    Volume = 150m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "ISNTREE Yam Root Vegan Milk Cleanser",
                    ProductDescription = "A gentle and nourishing beauty care product suitable for all skin types. Enriched with the goodness of Andong yam root extract to effectively remove impurities and dirt from your skin while maintaining its vital hydration level. This cleanser is packed with amino acids that work harmoniously to soothe your skin and create a protective barrier, locking in essential moisture.",
                    Price = 450_000m, // Giá sản phẩm là 450.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // K Beauty (Giả định CompanyId là 4)
                    Dimension = "220ml",
                    Volume = 220m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Normaderm Anti-Acne Purifying Gel Cleanser",
                    ProductDescription = "Unlike most acne cleansers, which strips the skin of natural oils and hydration, Vichy's phytosolution gel cleanser for acne is an oil-free and soap-free face cleanser that is gentle and non-irritating for acne-prone skin. Its formula is charged with active ingredients to not only target pimples, acne breakouts, pores and oily skin but also to reinforce your skin barrier function.",
                    Price = 720_000m, // Giá sản phẩm là 720.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Vichy (Giả định CompanyId là 5)
                    Dimension = "200ml",
                    Volume = 200m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Purete Thermale Fresh Cleansing Gel",
                    ProductDescription = "A rich, lathering face cleanser that effectively cleanses all impurities, makeup and pollution from the skin while counteracting the skin-damaging effects of hard water. Leaves skin feeling soft and fresh, without tightness.",
                    Price = 600_000m, // Giá sản phẩm là 600.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Vichy (Giả định CompanyId là 5)
                    Dimension = "200ml",
                    Volume = 200m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Purete Thermale Cleansing Foaming Cream",
                    ProductDescription = "A foaming, cream face cleanser that effectively cleanses impurities, makeup and pollution from the skin without rubbing or drying it out, while counteracting the skin-damaging effects of hard water. Leaves skin feeling soft and fresh, without tightness.",
                    Price = 650_000m, // Giá sản phẩm là 650.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // Vichy (Giả định CompanyId là 5)
                    Dimension = "125ml",
                    Volume = 125m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Foaming Cream Cleanser",
                    ProductDescription = "This luxurious cream formula that cleanses and gently exfoliates the skin while providing deep hydration through the infusion of amino acids. With the potent combination of tranexamic acid and niacinamide, this cleanser effectively brightens the skin, revealing a radiant, illuminated, and more balanced complexion.",
                    Price = 1_170_000m, // Giá sản phẩm là 1.170.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 6)
                    Dimension = "120ml",
                    Volume = 120m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Exfoliating Cleanser",
                    ProductDescription = "Infused with collagen-boosting and wrinkle-relaxing peptides, this instantly energizing cleanser features a moisture-rich lather and eco-friendly jojoba esters to leave skin feeling soft and clean without stripping healthy oils.",
                    Price = 1_250_000m, // Giá sản phẩm là 1.250.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 6)
                    Dimension = "200ml",
                    Volume = 200m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Cleansing Gel Face Wash",
                    ProductDescription = "This one-and-done face wash effectively removes makeup, dirt and excess oil with a gentle blend of peptides, antioxidants and calming anti-irritants that leave skin clean and refreshed.",
                    Price = 1_250_000m, // Giá sản phẩm là 1.250.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 1, // Cleanser
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 6)
                    Dimension = "200ml",
                    Volume = 200m
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Mangosteen Revitalizing Mist",
                    ProductDescription = "Escape to paradise with every spritz of this invigorating facial mist. A dreamy combination of antioxidant-packed mangosteen, energizing ribose, and pore-refining red clover work in perfect harmony to revitalize the skin.",
                    Price = 1_120_000m, // Giá sản phẩm là 1.120.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 7)
                    Dimension = "125ml",
                    Volume = 125m
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Pineapple Refining Tonique",
                    ProductDescription = "Refresh your routine with a sweetly scented pineapple tonique! PHA, bromelain and tranexamic acid team up to visibly renew dull, textured skin without irritation. This tonique is designed to exfoliate, brighten and hydrate while preparing skin for the next step in your daily ritual. This mild exfoliating tonique is great for all skin types and can be used daily.",
                    Price = 1_400_000m, // Giá sản phẩm là 1.400.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 7)
                    Dimension = "120ml",
                    Volume = 120m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Hawthorn Tonique",
                    ProductDescription = "For dehydrated, irritated, and sensitive skin, Eminence Organics Hawthorn Tonique will give your skin a revitalized appearance. Use the power of Hawthorn*, Chamomile*, and Marjoram** to reduce the appearance of skin irritation. Eucalyptus Oil*, protects and tones while Carrot Extract**, rich in vitamins, minerals and carotenoids heals irritation. Made with *Certified Organic Ingredients and **Biodynamic® ingredients from Demeter International Certified Biodynamic® farms.",
                    Price = 1_200_000m, // Giá sản phẩm là 1.200.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 7)
                    Dimension = "50ml",
                    Volume = 50m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Lime Refresh Tonique",
                    ProductDescription = "A refreshing toner for all skin types, particularly normal to oily skin. Rich in vitamin C, this invigorating mist tones and balances the skin’s appearance with freshly squeezed lime juice. Contains Eminence Organics proprietary Biocomplex2™: Euterpe Oleracea (Acai)*, Citrus Limon (Lemon)*, Malpighia Glabra (Barbados Cherry)*, Emblica Officinalis (Indian Gooseberry)*, Adansonia Digitata (Baobab)*, Myrciaria Dubia (Camu Camu)*, Daucus Carota Sativa (Carrot)*, Cocos Nucifera (Coconut) Water*, Lycium Barbarum (Goji) Berry*, Tapioca Starch (from Cassava Root)*, Thioctic Acid (Alpha Lipoic Acid) and Ubiquinone (Coenzyme Q10). *Certified Organic Ingredient",
                    Price = 990_000m, // Giá sản phẩm là 990.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 7)
                    Dimension = "120ml",
                    Volume = 120m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Soothing Chamomile Tonique",
                    ProductDescription = "This calming and soothing toner infuses your skin with comforting herbs to restore the skin’s balance. Chamomile, Comfrey Root, Licorice and Aloe Vera calm and moisturize the skin while Sodium Bicarbonate neutralizes and soothes. This toner can be applied after professional or home use peels to neutralize and restore the skin’s balance.",
                    Price = 990_000m, // Giá sản phẩm là 990.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 7)
                    Dimension = "120ml",
                    Volume = 120m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Multi-Acne Toner",
                    ProductDescription = "Light facial toner hydrates and refreshes. This ultra-light toner with moisture-binding humectants helps condition and prep skin for proper moisture absorption. Soothing Arnica, Balm Mint and Lavender refresh the skin, making this spritz ideal for skin hydration after cleansing and throughout the day.",
                    Price = 1_510_000m, // Giá sản phẩm là 1.510.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 5)
                    Dimension = "250ml",
                    Volume = 250m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Antioxidant Hydramist",
                    ProductDescription = "Refreshing antioxidant toner that helps firm and hydrate. Convenient mist-on formula supplements skin’s protective barrier by creating an active antioxidant shield to help fight free radical damage, and help prevent the signs of aging caused by Advanced Glycation End-products (AGEs) – a damaging by-product of sugar/protein reactions on the skin. Pea Extract helps firm skin, while Rose and Clove extracts comfort and refresh, making this mist ideal for use after cleansing or throughout the day.",
                    Price = 1_640_000m, // Giá sản phẩm là 1.640.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 5)
                    Dimension = "150ml",
                    Volume = 150m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "UltraCalming Mist",
                    ProductDescription = "A soothing, hydrating mist to help calm redness and sensitivity. Use post-cleanse to lock in hydration and prime skin for treatment with UltraCalming products. Lightweight mist quickly absorbs to support a functioning skin barrier, helping to minimize future flare-ups. Our exclusive UltraCalming Complex contains Oat and botanicals to help soothe and strengthen skin, and soothing Aloe helps calm while supporting a natural moisture balance in skin.",
                    Price = 1_510_000m, // Giá sản phẩm là 1.510.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 5)
                    Dimension = "177ml",
                    Volume = 177m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Hyaluronic Ceramide Mist",
                    ProductDescription = "Saturate skin with hydration and lock in moisture to help bounce back: long-lasting hydrating Hyaluronic Acid and ceramide mist helps to smooth fine lines and strengthen skin’s barrier. Skin-nourishing, hydration-rich mist utilizes four types of Hyaluronic Acid, helping skin hold on to water for long-lasting hydration. Moisture barrier-boosting Hyaluronic Acid and ceramide formula helps smooth the appearance of fine lines, sealing in hydration for supple skin that bounces back from stress. Calming, aromatic Rose Water with antioxidant-rich polyphenols and flavonoids helps to revive and refresh. Nourishing formula leaves skin feeling smooth and soft.",
                    Price = 1_800_000m, // Giá sản phẩm là 1.800.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 5)
                    Dimension = "150ml",
                    Volume = 150m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Remedy Toner",
                    ProductDescription = "Rose water spray with a fortifying and soothing effect, recommended for sensitive skin, skin showing signs of sensitivity and redness.",
                    Price = 990_000m, // Giá sản phẩm là 990.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 6)
                    Dimension = "200ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Essential Toner",
                    ProductDescription = "Alcohol-free skin balancing toner, ideal for completing cleansing, capable of helping to restore proper moisture and regenerate tissues.",
                    Price = 836_000m, // Giá sản phẩm là 836.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 6)
                    Dimension = "200ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Active Pureness Toner",
                    ProductDescription = "Toner helps smooth the skin with 3% gluconolactone which has an exfoliating effect.",
                    Price = 990_000m, // Giá sản phẩm là 990.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 6)
                    Dimension = "200ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Revitalizing Tonic",
                    ProductDescription = "Inspired by sunlight reflecting off the ocean, the new revitalizing toner is designed to deliver the ultimate level of radiance and luminosity to the skin.",
                    Price = 1_320_000m, // Giá sản phẩm là 1.320.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 6)
                    Dimension = "200ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Acwell Licorice pH Balancing Cleansing Toner",
                    ProductDescription = "Acwell Licorice pH Balancing Cleansing Toner has a pH level of 5.5 to effectively balance your skin. Peony extract and a high concentration of licorice water - both natural brighteners - seep into skin to add an extra dose of luminosity to your complexion. Green tea extract also helps calm and reduce pigmentation, including acne scars and dark spots. After use, skin feels clean and smooth, not dry or tight. Because it's so good at removing any impurities left on the skin post-cleanser, it helps the rest of the products in your routine absorb better.",
                    Price = 460_000m, // Giá sản phẩm là 460.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // K Beauty (Giả định CompanyId là 7)
                    Dimension = "150ml",
                    Volume = 150m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "COSRX AHA/BHA Clarifying Treatment Toner",
                    ProductDescription = "The formulation of AHA+BHA+Purifying Botanical ingredients help to improve skin texture, increase vitality, and control pores. Eliminate impurities, exfoliate, and hydrate all in one step.",
                    Price = 355_000m, // Giá sản phẩm là 355.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // K Beauty (Giả định CompanyId là 7)
                    Dimension = "150ml",
                    Volume = 150m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Sulwhasoo Concentrated Ginseng Renewing Water",
                    ProductDescription = "This luxe, supercharged anti-aging toner helps improve the look of wrinkles and elasticity while hydrating for a rejuvenated complexion.",
                    Price = 3_450_000m, // Giá sản phẩm là 3.450.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // K Beauty (Giả định CompanyId là 8)
                    Dimension = "150ml",
                    Volume = 150m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Pre-Treatment Toner",
                    ProductDescription = "Infused with potent antioxidants and peptides designed to relax wrinkles and support collagen production, this age-defying treatment toner balances the skin’s moisture level while visibly brightening and encouraging a more even tone and texture.",
                    Price = 1_120_000m, // Giá sản phẩm là 1.120.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 9)
                    Dimension = "200ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Hydraflora",
                    ProductDescription = "This essence is formulated with a pre- and probiotic complex to balance and strengthen the microflora on skin’s surface. A rich blend of botanical extracts packed with potent antioxidants brightens as well as protects the skin from free radical damage. Coconut water and blue agave refine the look of pores and provide a smooth complexion.",
                    Price = 1_750_000m, // Giá sản phẩm là 1.750.000 VND
                    Quantity = 100,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 9)
                    Dimension = "120ml",
                    Volume = 120m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Clarifying Toner Pads",
                    ProductDescription = "Packed with skin-renewing exfoliants, bacteria-fighting botanicals, and a powerful regenerating peptide, these pre-soaked toner pads help keep skin clear from blemishes while visibly brightening, smoothing uneven texture, and reducing inflammation.",
                    Price = 1_250_000m, // Giá sản phẩm là 1.250.000 VND
                    Quantity = 60,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 9)
                    Dimension = "60 pads",
                    Volume = 60m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Toner Vichy Aqualia Thermal Hydrating Refreshing Water",
                    ProductDescription = "Vichy Aqualia Thermal Hydrating Refreshing Water 200ml toner for combination and oily skin acts like a magnet to remove all dirt on the skin, nourishes the skin to be soft, smooth and firm, while balancing the skin's pH so that the moisturizer can penetrate faster.",
                    Price = 690_000m, // Giá sản phẩm là 690.000 VND
                    Quantity = 200,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Vichy (Giả định CompanyId là 10)
                    Dimension = "200ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Toner Vichy Normaderm acne-prone skin purifying pore-tightening lotion",
                    ProductDescription = "Balancing Water Helps Unclog, Reduce Pores Size And Reduce Oiliness For Oily, Acne-Prone Skin.",
                    Price = 780_000m, // Giá sản phẩm là 780.000 VND
                    Quantity = 200,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 2, // Toner
                    CompanyId = 1, // Vichy (Giả định CompanyId là 10)
                    Dimension = "200ml",
                    Volume = 200m,
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Sublime Skin Intensive Serum",
                    ProductDescription = "Multi-functional repair essence helps smooth, firm and protect the skin, visibly fills wrinkles, and gives a slimmer face.",
                    Price = 3_520_000m, // Giá sản phẩm là 3.520.000 VND
                    Quantity = 30,
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 12)
                    Dimension = "30ml",
                    Volume = 30m,
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Hydramemory Hydra & Glow Ampoules",
                    ProductDescription = "Intensive moisturizing & brightening essence with brightening complex (Niacinamide and NAG) and Polyglutamic Acid (PGA) with moisturizing effect to make skin healthier and skin becomes softer and smoother.",
                    Price = 1_350_000m, // Giá sản phẩm là 1.350.000 VND
                    Quantity = 7, // Số lượng ampoule là 7 x 2ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 12)
                    Dimension = "7 x 2ml",
                    Volume = 14m, // Tổng dung tích là 14ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Sublime Skin Lift & Firm Ampoule",
                    ProductDescription = "Intensive aesthetic treatment with anti-wrinkle peptides, epidermal growth factors (EGFs) and macro-hyaluronic acid to target skin firming and wrinkle reduction. Skin is instantly plumped and smoother.",
                    Price = 1_890_000m, // Giá sản phẩm là 1.890.000 VND
                    Quantity = 8, // Số lượng ampoule là 8 x 2ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 12)
                    Dimension = "8 x 2ml",
                    Volume = 16m, // Tổng dung tích là 16ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Biolumin-C Serum",
                    ProductDescription = "High-performance serum enhances skin’s natural defense system to brighten, firm and help dramatically reduce the appearance of fine lines and wrinkles. Advanced bio-technology and an ultra-stable Vitamin C complex work synergistically to enhance bioavailability of Vitamin C to fight oxidative stress and the appearance of skin aging before it starts. Optimized delivery system combined with a peptide and AHA renews for brighter, firmer, more radiant skin.",
                    Price = 3_320_000m, // Giá sản phẩm là 3.320.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 14)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Age Bright Clearing Serum",
                    ProductDescription = "This active two-in-one serum clears and helps prevent breakouts while reducing visible skin aging. Salicylic Acid reduces breakouts to clear skin. This highly-concentrated serum exfoliates to help prevent breakouts and accelerates cell turnover to reduce signs of skin aging. AGE Bright™ Complex works with the skin’s natural microbiome for clearer, brighter skin.",
                    Price = 2_620_000m, // Giá sản phẩm là 2.620.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 14)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Powerbright Dark Spot Serum",
                    ProductDescription = "Start fading the appearance of dark spots within days: advanced serum begins to diminish the appearance of uneven pigmentation fast, and keeps working to even skin tone over time.",
                    Price = 3_810_000m, // Giá sản phẩm là 3.810.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 14)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "UltraCalming Serum Concentrate",
                    ProductDescription = "The solution for skin sensitivity. This super-concentrated serum helps calm, restore and defend sensitized skin. Our exclusive UltraCalming™ Complex contains Oat and botanicals to ease sensitization, as peptides plus Oil of Evening Primrose, Sunflower Seed and Avocado extracts help defend against future assaults.",
                    Price = 2_310_000m, // Giá sản phẩm là 2.310.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 40ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 14)
                    Dimension = "40ml",
                    Volume = 40m, // Tổng dung tích là 40ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Circular Hydration Serum With Hyaluronic Acid",
                    ProductDescription = "Kick-start skin’s hydration cycle: long-lasting serum immediately floods skin with hydration, replenishes from within, and helps prevent future hydration evaporation. Full-circle hydrating serum utilizes an enhanced form of Hyaluronic Acid to penetrate skin’s surface for deep hydration and more supple, radiant skin over time. An Algae Extract-infused moisturizing matrix delivers quick and long-lasting hydration.",
                    Price = 2_230_000m, // Giá sản phẩm là 2.230.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Dermalogica (Giả định CompanyId là 14)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Strawberry Rhubarb Hyaluronic Serum",
                    ProductDescription = "Discover a radiant, youthful-looking complexion with the potent Strawberry Rhubarb Hyaluronic Serum. Our unique Botanical Hyaluronic Acid Complex combines with cica, succulent strawberry and rhubarb to deeply hydrate for visibly smoother, softer skin. Suitable for all skin types, especially dry or dehydrated.",
                    Price = 1_500_000m, // Giá sản phẩm là 1.500.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 15)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Citrus & Kale Potent C+E Serum",
                    ProductDescription = "Lightweight, advanced serum for all skin types. The potent Vitamin C in this serum is stabilized by botanically-derived ferulic acid delivering antioxidants to help brighten skin, improve the look of fine lines and wrinkles, and reduce the appearance of free radical damage.",
                    Price = 2_990_000m, // Giá sản phẩm là 2.990.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 15)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Marine Flower Peptide Serum",
                    ProductDescription = "This easily absorbed, potent gel serum delivers concentrated plant peptides and botanicals to diminish the appearance of fine lines and wrinkles for visibly smoother, plumper and more youthful-looking skin. Ideal for all skin types, especially aging skin, the Smart Collagen+ Complex rejuvenates the look of the complexion while unique algae extracts increase firmness and provide long-lasting hydration.",
                    Price = 2_990_000m, // Giá sản phẩm là 2.990.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 15)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Clear Skin Willow Bark Booster-Serum",
                    ProductDescription = "Help heal irritation and reduce the appearance of problem skin with this concentrated serum and product enhancer infused with willow bark and tea tree oil.",
                    Price = 1_680_000m, // Giá sản phẩm là 1.680.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 15)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Cornflower Recovery Serum",
                    ProductDescription = "Recover and detoxify with the power of cornflower, chamomile and hibiscus. This serum will improve the look of elasticity in your skin, giving you the look of age-defying results.",
                    Price = 1_480_000m, // Giá sản phẩm là 1.480.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 15ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Eminence Organic Skin Care (Giả định CompanyId là 15)
                    Dimension = "15ml",
                    Volume = 15m, // Tổng dung tích là 15ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Power Serum",
                    ProductDescription = "Our potent multitasking treatment delivers high-performance peptide complexes and collagen-boosting antioxidants that work in synergy to radically repair and improve the look of fine lines and wrinkles for a firmer, years-younger appearance.",
                    Price = 3_910_000m, // Giá sản phẩm là 3.910.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Firma-Bright Serum",
                    ProductDescription = "With a potent dose of stabilized vitamin C, free radical-fighting antioxidants, and radiance-boosting peptides, a few drops a day of this highly concentrated booster goes above and beyond brightening to visibly firm, sculpt, and illuminate skin.",
                    Price = 3_380_000m, // Giá sản phẩm là 3.380.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Hydrostem Serum",
                    ProductDescription = "This powerful age-defying treatment unites rejuvenating peptides with plant extracts that guard against UV and pollution damage. Antioxidant-rich botanical stem cells nourish skin cells and enhance skin vibrancy and luminosity to create more radiant, visibly firmed skin.",
                    Price = 4_270_000m, // Giá sản phẩm là 4.270.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Hydrostem Serum",
                    ProductDescription = "This powerful age-defying treatment unites rejuvenating peptides with plant extracts that guard against UV and pollution damage. Antioxidant-rich botanical stem cells nourish skin cells and enhance skin vibrancy and luminosity to create more radiant, visibly firmed skin.",
                    Price = 4_270_000m, // Giá sản phẩm là 4.270.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Minéral 89 Booster",
                    ProductDescription = "A breakthrough skincare formula: a high concentration of 89% of Vichy Volcanic Water, naturally charged with 15 Essential Minerals, enriched with pure Hyaluronic acid to help strengthen skin's moisture barrier and make it more resistant to daily aggressors. Replenished with moisture, the skin is hydrated, looks toned and plumped. Day after day, the skin radiates with a healthy glow.",
                    Price = 1_170_000m, // Giá sản phẩm là 1.170.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Vichy (Giả định CompanyId là 17)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Minéral 89 Probiotic Fractions",
                    ProductDescription = "Your stressed skin solution. Skin faces multiple sources of inner and outer exposome stressors, such as pollution, climate change, nutrition and psychological stress. Intense and repeated exposure to these factors alters the skin barrier and skin defense function, leading to dull, fragilized and tired-looking skin. Introducing Minéral 89 Probiotic Fractions, a regenerating and repairing booster powered by probiotic fractions grown in Vichy Volcanic Water and enriched with soothing niacinamide (Vitamin B3) to correct visible signs of stress on skin.",
                    Price = 1_300_000m, // Giá sản phẩm là 1.300.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 3, // Serum
                    CompanyId = 1, // Vichy (Giả định CompanyId là 17)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Barrier Builder",
                    ProductDescription = "A therapeutic barrier repair treatment consciously formulated to calm and renew inflamed and irritated skin. This rich, creamy formula penetrates deep into the epidermis, delivering a potent blend of clinically proven ingredients that repair, hydrate, and fortify skin. At the core of Barrier Builder is our exclusive patented peptide CellRenew-16, offering your skin a cellular toolkit to construct the essential structural proteins necessary for a healthy-looking complexion.",
                    Price = 1_480_000m, // Giá sản phẩm là 1.480.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Power Luxe",
                    ProductDescription = "HydroPeptide Power Luxe is the final step in your nighttime routine to lock in hydration and encourage cell turn-over while you sleep. Power Luxe works with your skin's circadian rhythm to balance and restore over time. Use nightly to protect your skin's natural lipid barrier and prevent dryness and sensitivity. Contains 4 different kinds of hyaluronic acid to restore hydration within every layer of the skin, Bakuchiol-a natural retinol alternative, Red Algae Extracts to assist in lipid barrier support and reduce the appearance of skin sagging and dryness and Peptides are designed to support your skin's structural integrity to fight skin sagging and encourage a firmer, plumper appearance.",
                    Price = 3_960_000m, // Giá sản phẩm là 3.960.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "AquaBoost Oil Free Face Moisturizer",
                    ProductDescription = "6 peptides including a Clearing Peptide, Retinoic-Like Peptide, Radiance Peptide, Anti-Redness, Protective Peptide, and Preservative Peptide offer superior hormonal aging benefits including clearer, more even, youthful-looking skin. Sacred Lily Dormin extract maintains more youthful looking skin, alleviates skin discomfort and sensitivity, and provides protection and pigment reduction.",
                    Price = 1_750_000m, // Giá sản phẩm là 1.750.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Face Lift Moisturizer",
                    ProductDescription = "Face Lift uses a powerful L22 lipid complex to increase your skin's age-preventing defenses to the levels they were at when your skin was in its prime. This cream also delivers just the perfect amount of hydration while antioxidants and peptides help protect from damaging free radicals.",
                    Price = 2_110_000m, // Giá sản phẩm là 2.110.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // HydroPeptide (Giả định CompanyId là 16)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Sublime Skin Fluid Cream",
                    ProductDescription = "Comfort Zone Fluid Cream that tones and firms. Made with an anti-aging botanical Achillea Millefolilum extract and Hyaluronic Acid, it plumps, smoothes, and brightens the skin. Use in combination with the Sublime Skin Intensive Serum. Contains 99% natural-origin ingredients without silicones, animal derivatives, mineral oils, artificial colorants, ethoxylates (PEG) and acrylates. Non-comedogenic.",
                    Price = 3_530_000m, // Giá sản phẩm là 3.530.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Comfortzone (Giả định CompanyId là 17)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Sacred Nature Nutrient Cream",
                    ProductDescription = "This remarkable cream, packed full of all natural and organic ingredients, will enhance the skin with immediate and long-term results for visibly younger looking skin. Made complete with the Sacred Nature’s patented Scientific Garden Extract™, a complex of Myrtle, Elderberry and Pomegranate, that immensely improves the skin’s overall health. Contains natural fragrance ingredients.",
                    Price = 1_780_000m, // Giá sản phẩm là 1.780.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Comfortzone (CompanyId mặc định là 1)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Active Pureness Fluid",
                    ProductDescription = "The unique combination of Vitamin C and mattifying powders makes this an ideal moisturizer or make-up primer for oily and breakout-prone skin.",
                    Price = 1_400_000m, // Giá sản phẩm là 1.400.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Comfortzone (CompanyId mặc định là 1)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Remedy Cream",
                    ProductDescription = "This light, silky moisturizer reinforces the skin’s barrier defense, providing a sensation of comfort and protection. Prebiotics derived from natural sugars protect the integrity of the skin’s natural flora. Ideal for skin that is sensitive, sensitized and prone to redness.",
                    Price = 1_960_000m, // Giá sản phẩm là 1.960.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Comfortzone (CompanyId mặc định là 1)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Strawberry Rhubarb Hyaluronic Hydrator",
                    ProductDescription = "With a fresh, dewy finish, this vegan gel-cream rejuvenates the appearance of dull skin. Lightweight in texture, this hydrator pairs our innovative Botanical Hyaluronic Acid Complex with panthenol, strawberry and rhubarb to lock in moisture and reveal radiant-looking skin. Ideal for all skin types.",
                    Price = 1_730_000m, // Giá sản phẩm là 1.730.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 35ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 1)
                    Dimension = "35ml",
                    Volume = 35m, // Tổng dung tích là 35ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Bakuchiol + Niacinamide Moisturizer",
                    ProductDescription = "Restore skin’s natural hydration with this gel-cream moisturizer, formulated with the unique combination of retinol alternative bakuchiol and niacinamide. This pairing smooths wrinkles while visibly firming skin, minimizing large pores and uneven texture, with no visible irritation.",
                    Price = 2_000_000m, // Giá sản phẩm là 2.000.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 1)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Acne Advanced Clarifying Hydrator",
                    ProductDescription = "This ultra-lightweight hydrating lotion reduces the appearance of oily skin, resulting in a soft, matte finish. Powerful lotus extract diminishes shine, while time-release encapsulated salicylic acid reduces blemishes and breakouts. In addition, zinc hyaluronate and arbutin reduce the look of irritation and scarring.",
                    Price = 2_000_000m, // Giá sản phẩm là 2.000.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 35ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 1)
                    Dimension = "35ml",
                    Volume = 35m, // Tổng dung tích là 35ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Echinacea Recovery Cream",
                    ProductDescription = "Echinacea, yarrow and evening primrose oil help repair the visible signs of aging without leaving behind a greasy feeling on your skin. This soothing fluid cream is perfect for dehydrated or irritated skin.",
                    Price = 1_990_000m, // Giá sản phẩm là 1.990.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 1)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "PowerBright Overnight Cream",
                    ProductDescription = "Dermalogica Powerbright Overnight Cream is a nourishing nighttime cream that optimizes skin moisture recovery and helps restore luminosity and fades dark spots while you sleep. Niacinamide and Hexylresorcinol help fade the appearance of dark spots along with Vitamin C. Pumpkin Enzyme smoothes and evens skin texture. Antioxidant-rich Cranberry and Raspberry Seed oils help protect against the damaging effects of free radicals from pollution and deliver essential fatty acids that moisturize to help reduce the appearance of fine lines. Licorice Extract helps soothe the skin.",
                    Price = 2_260_000m, // Giá sản phẩm là 2.260.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Skin Soothing Hydrating Lotion",
                    ProductDescription = "Say goodbye to dehydrated, breakout-irritated skin with this lightweight moisturizer! Sheer, easy-to-apply formula helps soothe discomfort and hydrate areas that feel dry. Also helps relieve the dryness often associated with some acne treatments.",
                    Price = 635_000m, // Giá sản phẩm là 635.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Skin Smoothing Cream",
                    ProductDescription = "Next-generation moisturizer with Active HydraMesh Technology™ infuses skin with 48 hours of continuous hydration and helps protect against environmental stress. This best-selling moisturizer features a state-of-the-art complex that works on a molecular level to help reduce Trans-Epidermal Water Loss (TEWL) and infuse skin with 48 hours of vital moisture. This advanced technology also helps shield skin’s natural microbiome from environmental stress. A dynamic Hyaluronic Acid Complex with Mallow, Cucumber and Arnica distributes hydration throughout the skin, helping to lock in moisture for lasting hydration. Also formulated with naturally-antioxidant Grape Seed Extract, Vitamin C and Vitamin E.",
                    Price = 1_930_000m, // Giá sản phẩm là 1.930.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 100ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "100ml",
                    Volume = 100m, // Tổng dung tích là 100ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Barrier Repair",
                    ProductDescription = "Velvety moisturizer helps fortify sensitized skin with a damaged barrier. Use this unique anhydrous (waterless) moisturizer after toning to help shield against environmental and internal triggers that cause skin stress. Our exclusive UltraCalming Complex contains Oat and botanical actives that work below the surface to interrupt inflammatory triggers that lead to sensitization, while helping to minimize discomfort, burning and itching. Help restore a healthy barrier function with Oil of Evening Primrose, Borage Seed Oil and silicones as vitamins C and E help combat free radicals that can lead to inflamed and irritated skin.",
                    Price = 1_430_000m, // Giá sản phẩm là 1.430.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Minéral 89 72H Moisture Boosting Fragrance Free Cream",
                    ProductDescription = "Vichy Minéral 89 72H Moisture Boosting Fragrance Free Cream is a lightweight, hydrating cream that provides long-lasting moisture to the skin. It's formulated with Hyaluronic Acid, Mineral-Rich Vichy Volcanic Water, Vitamins B3 & E, and Squalane to strengthen the skin's moisture barrier and protect it from environmental stressors. This cream is suitable for all skin types, especially those with dry or sensitive skin. It absorbs quickly, leaving skin feeling soft, supple, and hydrated.",
                    Price = 550_000m, // Giá sản phẩm là 550.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Vichy (CompanyId mặc định là 1)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Liftactiv B3 Tone Correcting Night Cream With Pure Retinol",
                    ProductDescription = "Liftactiv B3 Tone Correcting Night Cream with Pure Retinol is a night cream designed to target hyperpigmentation and uneven skin tone. It combines the power of Niacinamide (Vitamin B3) and Retinol to promote skin cell turnover, reduce the appearance of dark spots, and improve overall skin texture. This cream is suitable for all skin types, including sensitive skin, and helps to reveal a more radiant and even-toned complexion.",
                    Price = 1_670_000m, // Giá sản phẩm là 1.670.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // Vichy (CompanyId mặc định là 1)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Belif The True Cream Aqua Bomb",
                    ProductDescription = "A lightweight, gel-type moisturizer that provides instant hydration and a cooling sensation to the skin. It is formulated with a blend of herbs, including Lady's Mantle, known for its antioxidant properties. This moisturizer is suitable for all skin types, especially oily and combination skin. It absorbs quickly, leaving skin feeling refreshed and hydrated without a greasy residue.",
                    Price = 970_000m, // Giá sản phẩm là 970.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // K Beauty (CompanyId mặc định là 1)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Oat So Simple Water Cream",
                    ProductDescription = "A light-as-water moisturizer that feels like a burst of refreshing hydration on the face. Formulated with less than 10 ingredients including soothing Oat Extract, this no-fuss, just-right moisturizer gives your skin the gentle calming it wants and the essential hydration it needs. You and your skin will go from 'so over this' to 'glad that's over' all thanks to Oat So Simple Water Cream. Whew!",
                    Price = 700_000m, // Giá sản phẩm là 700.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 80ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // K Beauty (CompanyId mặc định là 1)
                    Dimension = "80ml",
                    Volume = 80m, // Tổng dung tích là 80ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "AESTURA A-CICA 365 Calming Cream",
                    ProductDescription = "A soothing moisturizer designed to calm irritated and sensitive skin. It's formulated with Centella Asiatica extract, known for its skin-soothing and healing properties. This cream provides long-lasting hydration and helps reduce redness and inflammation. It's suitable for those with dry, sensitive, or acne-prone skin.",
                    Price = 790_000m, // Giá sản phẩm là 790.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 4, // Moisturizer
                    CompanyId = 1, // K Beauty (CompanyId mặc định là 1)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Sun Soul Face Cream SPF30",
                    ProductDescription = "Sun Soulface Cream SPF3 is a lightweight, daily sunscreen designed to protect your skin from harmful UV rays. It provides broad-spectrum protection against both UVA and UVB rays, helping to prevent sunburn, premature aging, and skin cancer. This sunscreen is often formulated with gentle ingredients and is suitable for daily use on all skin types, including sensitive skin. It absorbs quickly, leaving a non-greasy finish and can be used as a base for makeup.",
                    Price = 880_000m, // Giá sản phẩm là 880.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Comfortzone (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    Volume = 200m, // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Skin Regimen Urban Shield SPF30",
                    ProductDescription = "Comfort Zone's Skin Regimen Urban Shield SPF30 is a lightweight, non-greasy sunscreen designed to protect your skin from the harmful effects of urban pollution and UV radiation.",
                    Price = 1_670_000m, // Giá sản phẩm là 1.670.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 40ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Comfortzone (CompanyId mặc định là 1)
                    Dimension = "40ml",
                    Volume = 40m, // Tổng dung tích là 40ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "PoreScreen SPF40",
                    ProductDescription = "Minimize the appearance of pores and protect against UVA + UV rays with Porescreen SPF40. Enhances skin with primer-like effect and hint of tint for sheer finish.",
                    Price = 1_390_000m, // Giá sản phẩm là 1.390.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Invisible Physical Defense SPF30",
                    ProductDescription = "Say goodbye to thick, white residue with this physical SPF formula that provides added blue light protection and helps soothe away the effects of environmental aggressors. Bio-active Mushroom Complex helps soothe skin, and reduce UV-induced redness and dryness. Antioxidant Green Tea helps defend skin against free radical damage. Ideal for all skin types, including sensitive.",
                    Price = 1_245_000m, // Giá sản phẩm là 1.245.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Protection 50 Sport SPF50",
                    ProductDescription = "This sheer solar protection treatment defends against prolonged skin damage from UV light and environmental assault. Oleosome microspheres help enhance SPF performance and counteract moisture loss triggered by extended daylight exposure. Lightweight formula helps neutralize damage and bind moisture to skin without a greasy after-feel.",
                    Price = 1_000_000m, // Giá sản phẩm là 1.000.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 150ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "150ml",
                    Volume = 150m, // Tổng dung tích là 150ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Oil Free Matte SPF30",
                    ProductDescription = "Broad spectrum sunscreen helps prevent shine and skin aging on oily, breakout-prone skin. Lightweight, ultra-sheer formula contains an advanced blend of Zinc Gluconate, Caffeine, Niacinamide, Biotin and Yeast Extract. Oil absorbers help maintain an all-day matte finish, preventing shine without any powdery residue. Sheer formula provides defense against skin-aging UV light.",
                    Price = 2_030_000m, // Giá sản phẩm là 2.030.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 1)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Radiant Protection SPF Fluid",
                    ProductDescription = "Help protect skin from the harsh effects of the sun while smoothing the look of fine lines and wrinkles. With a hydrating, nourishing feel and dewy finish, this SPF keeps skin looking ageless while offering broad spectrum SPF 30 protection for combination to dry skin.",
                    Price = 1_600_000m, // Giá sản phẩm là 1.600.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 1)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Lilikoi Daily Defense Moisturizer SPF 40",
                    ProductDescription = "An all-in-one lightweight daily moisturizer formulated with cocoa seed extract, satsuma mandarin peel and SPF 40 all-mineral protection to improve the appearance of skin exposed to blue-light stress and pollution. Suitable for all skin types.",
                    Price = 1_930_000m, // Giá sản phẩm là 1.930.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 2)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Lilikoi Mineral Defense Sport Sunscreen SPF 30",
                    ProductDescription = "An easy-to-apply sport formulation for face and body, this SPF 30 mineral sunscreen is non-greasy and water-resistant up to 40 minutes. Highly effective for outdoor activities like swimming and high-performance sports where perspiration can impact standard sunscreen efficacy, this zinc oxide sunscreen protects from head to toe.",
                    Price = 1_480_000m, // Giá sản phẩm là 1.480.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 147ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 2)
                    Dimension = "147ml",
                    Volume = 147m, // Tổng dung tích là 147ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Daily Defense Tinted SPF",
                    ProductDescription = "Achieve a natural finish with this lightweight, tinted SPF 50+ sunscreen. This all-mineral formula is non-comedogenic and enriched with antioxidants for hydration while offering broad-spectrum and blue light protection.",
                    Price = 1_750_000m, // Giá sản phẩm là 1.750.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 2)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Solar Dew Sheer Mineral Melt SPF 30",
                    ProductDescription = "This sunscreen serum combines your sun defense with skin care benefits. A patented peptide, CellRenew-16, hydrates aging skin and blocks out free radicals that cause visible aging signs. What you're left with is protected skin and a delightful glow without feeling greasy.",
                    Price = 1_730_000m, // Giá sản phẩm là 1.730.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 40ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // HydroPeptide (CompanyId mặc định là 3)
                    Dimension = "40ml",
                    Volume = 40m, // Tổng dung tích là 40ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Solar Defense Non-Tinted Sunscreen",
                    ProductDescription = "Elegant SPF 50 mineral sunscreen stops sun damage in its tracks with a combination of physical blockers, weightless hydration, and botanical antioxidants. The silky smooth formula leaves a beautiful sheer-matte finish while protecting against free radical damage that leads to fine lines and wrinkles.",
                    Price = 1_350_000m, // Giá sản phẩm là 1.350.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // HydroPeptide (CompanyId mặc định là 3)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Solar Defense Tinted SPF 30",
                    ProductDescription = "This self-adjusting tinted SPF adapts instantly to most skin tones, providing a healthy glow and broad-spectrum mineral protection from UVA, UVB, and infrared rays. Hyaluronic acid attracts and locks in moisture and a blend of antioxidant-rich extracts guards against damage from free radicals.",
                    Price = 1_400_000m, // Giá sản phẩm là 1.400.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // HydroPeptide (CompanyId mặc định là 3)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Vichy Capital Soleil UV Age Daily SPF50 PA++++",
                    ProductDescription = "A high-protection sunscreen designed to protect your skin from harmful UV rays and combat signs of aging. It offers broad-spectrum protection against both UVA and UVB rays, helping to prevent sunburn, premature aging, and skin cancer.",
                    Price = 570_000m, // Giá sản phẩm là 570.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 40ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Vichy (CompanyId mặc định là 4)
                    Dimension = "40ml",
                    Volume = 40m, // Tổng dung tích là 40ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Capital Soleil Ultra Light Face Sunscreen SPF 50",
                    ProductDescription = "A daily anti-aging face sunscreen lotion with broad spectrum UVA/UVB SPF 50 protection in an ultra-light formula that is oxybenzone-free, water resistant for up to 80 minutes and easily absorbed on skin.",
                    Price = 880_000m, // Giá sản phẩm là 880.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Vichy (CompanyId mặc định là 4)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Neogen Day-Light Protection Airy Sunscreen",
                    ProductDescription = "Neogen Day-Light Protection Airy Sunscreen is a lightweight, non-greasy sunscreen that provides broad-spectrum protection against UVA and UVB rays. It's formulated with gentle ingredients, making it suitable for sensitive skin. This sunscreen absorbs quickly, leaving a matte finish and can be used as a base for makeup. It often contains additional skincare benefits, such as antioxidants and hydrating ingredients, to protect and nourish the skin.",
                    Price = 815_000m, // Giá sản phẩm là 815.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Neogen (CompanyId mặc định là 6)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Round Lab Birch Juice Moisturizing Sunscreen",
                    ProductDescription = "Round Lab Birch Juice Moisturizing Sunscreen is a lightweight, hydrating sunscreen that provides broad-spectrum protection against UVA and UVB rays. It's formulated with birch juice, known for its hydrating properties, and other gentle ingredients. This sunscreen is suitable for all skin types, especially those with sensitive or dry skin. It absorbs quickly, leaving a dewy finish and can be used as a base for makeup.",
                    Price = 360_000m, // Giá sản phẩm là 360.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // Round Lab (CompanyId mặc định là 7)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Beet The Sun SPF 40 PA+++",
                    ProductDescription = "A lightweight chemical sunscreen that’s made to look and feel like your skin. Supercharged with beetroot extract — a powerful antioxidant that gives your skin additional protection against oxidative stress — this SPF defends against the sun and other environmental aggressors without any extra drama or a white cast! With its truly universal formula and velvety texture, Beet The Sun is our simple solution to once-complicated SPF protection that’ll have you reaching for it again and again.",
                    Price = 500_000m, // Giá sản phẩm là 500.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // K Beauty (CompanyId mặc định là 8)
                    Dimension = "50ml",
                    Volume = 50m, // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Klairs All-day Airy Mineral Sunscreen SPF50+ PA++++",
                    ProductDescription = "It's formulated with gentle, mineral-based active ingredients, making it suitable for sensitive skin. This sunscreen absorbs quickly, leaving a matte finish and doesn't clog pores. It's also known for its ability to minimize white cast, making it a popular choice for those who prefer a natural-looking finish.",
                    Price = 480_000m, // Giá sản phẩm là 480.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // K Beauty (CompanyId mặc định là 8)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Goongbe Waterful Sun Lotion Mild SPF50+ PA++++",
                    ProductDescription = "Goongbe Waterful Sun Lotion Mild SPF50+ PA++++ is a gentle, water-based sunscreen designed for sensitive skin, including babies and children. It offers broad-spectrum protection against UVA and UVB rays, helping to shield your skin from sunburn and premature aging.",
                    Price = 450_000m, // Giá sản phẩm là 450.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 80ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 5, // Sun Cream
                    CompanyId = 1, // K Beauty (CompanyId mặc định là 8)
                    Dimension = "80ml",
                    Volume = 80m, // Tổng dung tích là 80ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Eight Greens Phyto Masque – Hot",
                    ProductDescription = "The whole plants and seeds in our Eight Greens Phyto Masque – Hot are naturally high in phytoestrogens and antioxidants. This unique mask will help improve hydration, the look of elasticity, the appearance of signs of aging, normalization of oily skin and the appearance of breakouts —all to return your skin to its youthful-looking glow.",
                    Price = 1_580_000m, // Giá sản phẩm là 1.580.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 5)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Kombucha Microbiome Leave-On Masque",
                    ProductDescription = "Bring comfort and serenity to dry, dull-looking skin with a rich slow-absorbing masque. A luxurious step in your routine, this formula features ginger and pre, pro* and postbiotics to visibly renew the appearance of the skin while supporting your moisture barrier. Ideal for all skin types, this masque helps the skin appear hydrated and healthy.",
                    Price = 2_140_000m, // Giá sản phẩm là 2.140.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 5)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Citrus & Kale Potent C+E Masque",
                    ProductDescription = "Potent, cream-gel mask for all skin types. Harness the natural power of Vitamins C+E with a boost of vitamins to improve the appearance of skin. A blend of citrus, leafy greens and avocado oil that helps reduce the look of drying environmental damage, as well as fine lines and wrinkles.",
                    Price = 1_780_000m, // Giá sản phẩm là 1.780.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 5)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Stone Crop Masque",
                    ProductDescription = "Our Stone Crop Masque excels in increasing the moisture content and health of all skin types – it will leave your skin looking radiant and youthful. Stone Crop is a healing plant used by herbalists for centuries to heal a multitude of skin conditions, and now you can experience its wonderful effects.",
                    Price = 1_400_000m, // Giá sản phẩm là 1.400.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 5)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Calm Skin Arnica Masque",
                    ProductDescription = "Calm the appearance of sensitive skin with this naturally soothing Arnica Mask. Ingredients including calendula, ivy and arnica soothe and detoxify the skin while reducing the appearance of inflammation.",
                    Price = 1_650_000m, // Giá sản phẩm là 1.650.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Eminence Organic Skin Care (CompanyId mặc định là 5)
                    Dimension = "60ml",
                    Volume = 60m, // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Multivitamin Power Recovery Masque",
                    ProductDescription = "Ultra-replenishing masque helps rescue stressed, aging skin. Apply after cleansing as an ultimate remedy for dulling, dry, dehydrated, lackluster, photodamaged skin and skin aging. Powerful, concentrated vitamins A, C and E, and Linoleic Acid help restore skin showing signs of damage and aging while enhancing barrier properties. Antioxidant vitamins C and E help shield skin from Reactive Oxygen Species (free radicals). Nutrient-rich Algae Extract helps moisturize and soften skin while Pro-Vitamin B5 helps nourish damaged skin. Botanical extracts of Licorice, Comfrey and Burdock soothe and calm skin while increasing resilience.",
                    Price = 2_050_000m, // Giá sản phẩm là 2.050.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 75ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 6)
                    Dimension = "75ml",
                    Volume = 75m, // Tổng dung tích là 75ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Sebum Clearing Masque",
                    ProductDescription = "Soothing clay masque helps clear breakouts and minimize premature signs of skin aging. Oil-absorbing clays help detoxify skin as Salicylic Acid clears pore congestion. Calming botanicals such as Oat and Bisabolol help soothe aggravation brought on by breakouts. Safflower Oil helps counter fine dehydration lines. This masque, which contains Licorice and Niacinamide, also helps even skin tone.",
                    Price = 1_700_000m, // Giá sản phẩm là 1.700.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 75ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 6)
                    Dimension = "75ml",
                    Volume = 75m, // Tổng dung tích là 75ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Melting Moisture Masque",
                    ProductDescription = "Extremely moisturizing masque elegantly transforms from balm to oil to help restore dry skin. Activated by skin’s natural heat, our MeltingPoint Complex delivers a satisfying melting sensation as it penetrates skin’s surface layers to deeply nourish and rehydrate. Micro-algae’s hydrating properties help soothe and protect skin against the drying effects of pollution. Vitamin-rich, buttery formula delivers intense, lasting hydration for smooth skin. Linoleic Acid nourishes while Vitamin E helps protect against skin-damaging free radicals for healthier-looking skin.",
                    Price = 2_340_000m, // Giá sản phẩm là 2.340.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 75ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Dermalogica (CompanyId mặc định là 6)
                    Dimension = "75ml",
                    Volume = 75m, // Tổng dung tích là 75ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Miracle Mask",
                    ProductDescription = "This multi-functional face mask uses purifying clays to clear impurities while minimizing the appearance of pores, while peptides address the appearance of wrinkles and an advanced complex provides an immediate lift.",
                    Price = 1_200_000m, // Giá sản phẩm là 1.200.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // HydroPeptide (CompanyId mặc định là 7)
                    Dimension = "30ml",
                    Volume = 30m, // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Hydro-Lock Sleep Mask",
                    ProductDescription = "An overnight, pillow-proof treatment that smooths and perfects your complexion. Royal peptides boost cell turnover and infuse your skin with radiance-restoring ingredients.",
                    Price = 2_230_000m, // Giá sản phẩm là 2.230.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 75ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // HydroPeptide (CompanyId mặc định là 7)
                    Dimension = "75ml",
                    Volume = 75m, // Tổng dung tích là 75ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "PolyPeptide Collagel+ Mask",
                    ProductDescription = "Enhance facial results with this PolyPeptide Collagel+ Mask for Face. To promote a youthful complexion, Hydrogel technology is infused with key collagen supporting peptides and hydrating nutrients to reduce the appearance of fine lines and wrinkles while brightening age spots.",
                    Price = 1_320_000m, // Giá sản phẩm là 1.320.000 VND
                    Quantity = 2, // Sản phẩm có 2 miếng (pads)
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // HydroPeptide
                    Dimension = "2 pads",
                    Volume = null // Không áp dụng cho dung tích
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Balancing Mask",
                    ProductDescription = "This unique age-defying face mask provides skin with a protective shield that keeps it balanced, calm, hydrated, and youthful looking.",
                    Price = 1_200_000m, // Giá sản phẩm là 1.200.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // HydroPeptide
                    Dimension = "30ml",
                    Volume = 30m // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Rejuvenating Mask",
                    ProductDescription = "This calming, detoxifying face mask will pamper and restore sensitive or sensitized skin with cool, purifying clays and calming peptides.",
                    Price = 1_200_000m, // Giá sản phẩm là 1.200.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // HydroPeptide
                    Dimension = "30ml",
                    Volume = 30m // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Aqualia Thermal Night Spa",
                    ProductDescription = "An ultra-comfortable, deeply hydrating night cream and face mask with natural origin Hyaluronic acid and Vichy Volcanic Water to lock in moisture and boost hydration. Hydrates dry & dull skin overnight, when skin is most receptive to treatments. Reveals soft, soothed and supple skin by morning.",
                    Price = 1_320_000m, // Giá sản phẩm là 1.320.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Vichy
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Quenching Mineral Mask",
                    ProductDescription = "Vichy's first mineral hydrating face mask with 10% Vichy Volcanic Water and soothing Vitamin B3 to act as a hydration boost for dry and uncomfortable skin.",
                    Price = 970_000m, // Giá sản phẩm là 970.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Vichy
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Pore Purifying Clay Mask",
                    ProductDescription = "Our best clay mask combining two ultra-fine white clays with 15 Mineral-Rich Vichy Volcanic Water to help eliminate excess sebum and impurities for purified pores and softer skin.",
                    Price = 970_000m, // Giá sản phẩm là 970.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // Vichy
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Sulwhasoo Activating Mask",
                    ProductDescription = "A sheet mask infused with the essence of First Care Activating Serum VI. It is designed to provide intensive hydration, improve skin elasticity, and promote a healthy, radiant complexion.",
                    Price = 1_520_000m, // Giá sản phẩm là 1.520.000 VND
                    Quantity = 5, // Sản phẩm có 5 miếng, mỗi miếng 25ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // K Beauty
                    Dimension = "25ml x 5 sheets",
                    Volume = 125m // Tổng dung tích là 125ml (25ml x 5)
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "COSRX Ultimate Nourishing Rice Spa Overnight Mask",
                    ProductDescription = "This gentle, hydrating overnight mask is formulated with rice extract to nourish and brighten the skin. It helps to improve skin texture, reduce dullness, and hydrate the skin deeply. Its lightweight, non-greasy formula is suitable for all skin types, especially dry and sensitive skin.",
                    Price = 690_000m, // Giá sản phẩm là 690.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // K Beauty
                    Dimension = "60ml",
                    Volume = 60m // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Klairs Rich Moist Soothing Tencel Sheet Mask",
                    ProductDescription = "Ceramide on the skin surface prevents water loss, always maintains the ideal moisture level for the skin, helping the skin to be supple and healthy. Ceramide also has the effect of preventing the skin aging process, helping the skin to retain its youthful appearance.",
                    Price = 65_000m, // Giá sản phẩm là 65.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 25ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // K Beauty
                    Dimension = "25ml",
                    Volume = 25m // Tổng dung tích là 25ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Klairs Midnight Blue Calming Sheet Mask",
                    ProductDescription = "It is a pore care mask solution with Bamboo Charcoal Powder that helps deep clean thereby preventing the formation of blackheads. The golden ingredient Erirythtol in the product also helps to quickly lower skin temperature.",
                    Price = 65_000m, // Giá sản phẩm là 65.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 25ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // K Beauty
                    Dimension = "25ml",
                    Volume = 25m // Tổng dung tích là 25ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Klairs Freshly Juiced Vitamin E Mask",
                    ProductDescription = "The combination of Vitamin E and Niacinamide provides antioxidant and brightening effects, improves wrinkles and prevents signs of aging.",
                    Price = 545_000m, // Giá sản phẩm là 545.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 90ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 6, // Mask
                    CompanyId = 1, // K Beauty
                    Dimension = "90ml",
                    Volume = 90m // Tổng dung tích là 90ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Sacred Nature Exfoliant Mask",
                    ProductDescription = "Organic exfoliating mask with 9% gluconolactone gently exfoliates and instantly brightens the skin. With antioxidants from Garden of Science™ to enhance skin's resilience and vitality. For dull and impure skin.",
                    Price = 1_740_000m, // Giá sản phẩm là 1.740.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 110ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Comfortzone
                    Dimension = "110ml",
                    Volume = 110m // Tổng dung tích là 110ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Essential Scrub",
                    ProductDescription = "This exfoliating facial scrub removes impurities and refines the pores, reawakening skin luminosity and leaving the skin soft and smooth. For all skin types, especially congested, oily and acne-prone.",
                    Price = 1_295_000m, // Giá sản phẩm là 1.295.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Comfortzone
                    Dimension = "60ml",
                    Volume = 60m // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Liquid Resurfacing Solution",
                    ProductDescription = "This lightweight, leave-on exfoliant partners 2% salicylic acid and potent antioxidants with our patented CellRenew-16 technology to visibly improve skin tone and texture without compromising the skin's moisture barrier.",
                    Price = 1_245_000m, // Giá sản phẩm là 1.245.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 120ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // HydroPeptide
                    Dimension = "120ml",
                    Volume = 120m // Tổng dung tích là 120ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "5X Power Peel Face Exfoliator",
                    ProductDescription = "These gentle yet effective peel pads will break you down to build you right back up. With just the right amount of gentle exfoliation, they whisk away dull skin cells improving the appearance of fine lines and wrinkles while encouraging collagen and cell renewal. Unlike other peels that leave skin red and irritated, these are gentle enough for daily use—with no down time.",
                    Price = 1_850_000m, // Giá sản phẩm là 1.850.000 VND
                    Quantity = 30, // Sản phẩm có 30 miếng (25ml x 30 sheets)
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // HydroPeptide
                    Dimension = "25ml x 30 sheets",
                    Volume = 750m // Tổng dung tích là 750ml (25ml x 30 sheets)
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Daily Milkfoliant",
                    ProductDescription = "Vegan milky powder exfoliant with Oat and Coconut activates upon contact with water, releasing botanical extracts to help polish away dead skin cells. This gentle exfoliating powder and a blend with fruit-based Grape Extract and Arginine rich in Alpha Hydroxy Acids (AHAs) and Beta Hydroxy Acids (BHAs) work together with Coconut Milk for smoother, softer skin. Harvested from Papaya, Papain Extract provides exfoliating properties, helping to resurface skin.",
                    Price = 1_670_000m, // Giá sản phẩm là 1.670.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 74ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Dermalogica
                    Dimension = "74ml",
                    Volume = 74m // Tổng dung tích là 74ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Liquid Peelfoliant",
                    ProductDescription = "Professional-grade at-home peel with a blend of 30% acids and enzymes (Glycolic, Lactic, Salicylic, Phytic and Tranexamic acids plus Gluconolactone and fermented Pomegranate Enzyme) works at different layers of skin’s surface to thoroughly exfoliate, help unclog pores and reveal smoother, brighter skin. A lipid-rich blend with upcycled Cranberry Extract promotes long-lasting hydration.",
                    Price = 1_650_000m, // Giá sản phẩm là 1.650.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Dermalogica
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Daily Superfoliant",
                    ProductDescription = "This highly-active resurfacer delivers your smoothest skin ever, and helps fight the biochemical and environmental triggers known to accelerate skin aging. The advanced powder formula activates upon contact with water, releasing powerful enzymes, skin-smoothing alpha hydroxy acids and anti-pollution technology. Activated Binchotan Charcoal purifies the skin, helping to adsorb environmental toxins from deep within the pores, while Niacinamide, Red Algae and Tara Fruit Extract help guard against the damaging effects of pollution. Not recommended for users of medically-prescribed exfoliation products.",
                    Price = 1_670_000m, // Giá sản phẩm là 1.670.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Dermalogica
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Multivitamin Thermafoliant",
                    ProductDescription = "Thermal skin exfoliant infuses skin with age-fighting ingredients. This powerful skin polisher combines physical and chemical exfoliants to refine skin texture and enhance penetration of age-fighting vitamins into skin. Resurfacing microgranules gently polish off dulling skin cells to reveal smoother, fresher skin immediately. Unique thermal technology activates upon contact with water to stimulate penetration of skin-sloughing Salicylic Acid and Retinol, while Prickly Pear Extract accelerates skin's natural exfoliation process. White Tea suppresses the formation of MMPs while Licorice and vitamins C and E brighten skin tone, provide antioxidant defense against damaging free radicals (Reactive Oxygen Species) and help promote skin firmness. Skin feels and looks dramatically improved and smoother. Keep out of eyes.",
                    Price = 1_670_000m, // Giá sản phẩm là 1.670.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Dermalogica
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Daily Microfoliant",
                    ProductDescription = "Achieve brighter, smoother skin every day with this iconic exfoliating powder. Rice-based powder activates upon contact with water, releasing Papain, Salicylic Acid and Rice Enzymes to polish skin to perfection. A Skin Brightening Complex of Phytic Acid from Rice Bran, White Tea and Licorice helps balance uneven skin tone while a super-soothing blend of Colloidal Oatmeal and Allantoin calms skin. Gentle enough for daily use.",
                    Price = 1_670_000m, // Giá sản phẩm là 1.670.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Dermalogica
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Strawberry Rhubarb Dermafoliant",
                    ProductDescription = "Reveal bright, refined skin with the award-winning Strawberry Rhubarb Dermafoliant. Formulated with lactic acid, a blend of polishing flours and our Botanical Hyaluronic Acid Complex, this vegan exfoliant gently removes impurities and excess oil from the skin to reveal a visibly smooth, radiant complexion. Suitable for all skin types.",
                    Price = 1_350_000m, // Giá sản phẩm là 1.350.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 120ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "120ml",
                    Volume = 120m // Tổng dung tích là 120ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Turmeric Energizing Treatment",
                    ProductDescription = "Formulated with turmeric, citrine gemstones and zeolite, this spicy golden powder awakens the skin. As you slowly add water, activate your treatment into a fluffy mousse bursting with energy. Embrace the warm, exfoliating sensation and reveal silky, luminous skin.",
                    Price = 2_490_000m, // Giá sản phẩm là 2.490.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "60ml",
                    Volume = 60m // Tổng dung tích là 60ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Bright Skin Licorice Root Exfoliating Peel",
                    ProductDescription = "A results-oriented peel solution with cotton round pads that reduces the look of uneven pigmentation. Lactic and mandelic acids gently exfoliate dead skin cells while licorice root and our Natural Hydroquinone Alternative from African potato and tara tree brighten the appearance of dark spots and hyperpigmentation.",
                    Price = 2_260_000m, // Giá sản phẩm là 2.260.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Calm Skin Chamomile Exfoliating Peel",
                    ProductDescription = "A gentle exfoliating peel solution with cotton round pads for sensitive skin types. Renew sensitive skin without irritation with lactic and mandelic acids. Chamomile refreshes, calendula moisturizes and soothes the look of redness, and the arnica flower reduces the appearance of inflammation. Reveal a calm, balanced and more luminous complexion with this peel solution that is mild enough to use on sensitive skin two to three times a week.",
                    Price = 2_260_000m, // Giá sản phẩm là 2.260.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 50ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "50ml",
                    Volume = 50m // Tổng dung tích là 50ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Radish Seed Refining Peel",
                    ProductDescription = "This hypoallergenic peel helps prevent the appearance of breakouts with its detoxifying herbal extracts. Nettle, whole grain oat and willow bark stimulate skin renewal and smooth the appearance of lines.",
                    Price = 1_670_000m, // Giá sản phẩm là 1.670.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "30ml",
                    Volume = 30m // Tổng dung tích là 30ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Anua Heartleaf 77% Clear Pad",
                    ProductDescription = "Introducing Anua's Heartleaf 77 Clear Pad, your essential ally in achieving a balanced, refined complexion. Each pad is soaked in a potent serum, rich in natural botanical extracts such as Ulmus Davidiana, Pueraria Lobata root, and Pinus Palustris leaf, renowned for their sebum-regulating and pore-minimizing properties. The hero ingredient, Houttuynia Cordata extract, comprises 77% of the formula, providing intense hydration while soothing irritation, making it suitable even for sensitive skin types. Gluconolactone, a skin-friendly PHA, gently removes dead skin cells, revealing a smoother, brighter complexion underneath. The formula is lightweight and absorbs swiftly, making it an effortless addition to your daily skincare regimen.",
                    Price = 660_000m, // Giá sản phẩm là 660.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 180ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // K Beauty
                    Dimension = "180ml",
                    Volume = 180m // Tổng dung tích là 180ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "No.5 Vitamin-Niacinamide Concentrated Pad",
                    ProductDescription = "Contains niacinamide and vitamin C derivatives to help reduce the appearance of dark spots, even out skin tone, and improve overall skin texture. The pads are soaked in a gentle exfoliating solution that helps remove dead skin cells, revealing a brighter and smoother complexion.",
                    Price = 400_000m, // Giá sản phẩm là 400.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 180ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // K Beauty
                    Dimension = "180ml",
                    Volume = 180m // Tổng dung tích là 180ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Balanceful Cica Toner Pad",
                    ProductDescription = "The cotton pad contains vegan ingredients, 5% jojoba, olive, macadamia, baobab essential oils that provide superior moisture for soft, fresh skin. Combined with Centella Asiatic 5D complex and 5 derivatives extracted from Centella Asiatica, it has the effect of soothing, lowering temperature, minimizing redness and acne inflammation on the skin.",
                    Price = 506_000m, // Giá sản phẩm là 506.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 180ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // K Beauty
                    Dimension = "180ml",
                    Volume = 180m // Tổng dung tích là 180ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Pine Needle Pore Pad Clear Touch",
                    ProductDescription = "Contains pine needle extract with a content of 30,000ppm to help reduce oiliness, remove excess oil deep in pores to keep skin clear and smooth.",
                    Price = 590_000m, // Giá sản phẩm là 590.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 145ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // K Beauty
                    Dimension = "145ml",
                    Volume = 145m // Tổng dung tích là 145ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Krave Kale-lalu-yAHA",
                    ProductDescription = "A kind yet effective exfoliator that smooths out texture and fades the look of discoloration to reveal healthier, more radiant skin. Powered by 5.25% Glycolic Acid to gently nudge the skin’s natural renewal process without harming the barrier and microbiome.",
                    Price = 640_000m, // Giá sản phẩm là 640.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 7, // Exfoliant
                    CompanyId = 1, // K Beauty
                    Dimension = "200ml",
                    Volume = 200m // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Yuzu Solid Body Oil",
                    ProductDescription = "This transformative solid body oil infuses dry, dull skin with dreamy yuzu and vitamin-rich camu camu for radiant skin from every angle. Refining PHA and lush tropical oils enhance hydration for all skin types to leave you with supple, glowing skin.",
                    Price = 1_430_000m, // Giá sản phẩm là 1.430.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 150ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "150ml",
                    Volume = 150m // Tổng dung tích là 150ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Mangosteen Body Lotion",
                    ProductDescription = "Take a blissful approach to full body hydration with this heavenly mangosteen body lotion. Formulated with a unique Lactic Acid Complex, a proprietary blend of actives including lactic acid, ribose and red clover flower extract, this lightweight formula gently resurfaces to reveal bright, radiant skin.",
                    Price = 1_145_000m, // Giá sản phẩm là 1.145.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "250ml",
                    Volume = 250m // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Coconut Sugar Scrub",
                    ProductDescription = "Refine and hydrate your whole body with raw sugar cane granules, a source of natural Alpha Hydroxy Acids for exfoliation and virgin coconut oil to hydrate.",
                    Price = 1_120_000m, // Giá sản phẩm là 1.120.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "250ml",
                    Volume = 250m // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Stone Crop Contouring Body Cream",
                    ProductDescription = "The active ingredients in this naturally potent cream smooth and firm the skin. Use it as an all-over moisturizer or as a targeted treatment to tighten and tone where you need it most. Coffee and Microalgae Extracts, reduce the look of cellulite while Jojoba Oil, Stone Crop and Shea Butter moisturize and repair dry skin.",
                    Price = 1_730_000m, // Giá sản phẩm là 1.730.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 140ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "140ml",
                    Volume = 140m // Tổng dung tích là 140ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Stone Crop Body Oil",
                    ProductDescription = "Soothe and soften dry skin with a lightweight body oil that absorbs quickly, leaving a matte satin finish. The combination of stone crop and arnica makes this lightly scented oil ideal for massage, hand and foot treatments or daily moisturizing.",
                    Price = 940_000m, // Giá sản phẩm là 940.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 240ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Eminence Organic Skin Care
                    Dimension = "240ml",
                    Volume = 240m // Tổng dung tích là 240ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Sacred Nature Body Butter",
                    ProductDescription = "Eco-Cert Organic ingredients provide the most effective natural ingredients to encourage soft and supple skin. Concocted with Sacred Nature’s patented complex, Scientific Garden Extract™, a blend of Myrtle, Elderberry and Pomegranate. Contains natural fragrance ingredients.",
                    Price = 1_340_000m, // Giá sản phẩm là 1.340.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 220ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Comfortzone
                    Dimension = "220ml",
                    Volume = 220m // Tổng dung tích là 220ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Tranquillity Oil",
                    ProductDescription = "This bath and body oil contains nourishing amaranth oil and an exclusive blend of essential oils which provide an immediate sensation of wellbeing, alleviating the state of stress and tension. Its versatile formulation transforms from an oil to a milky fluid when in contact with water. Applied directly to the body, it is a nourishing oil which leaves skin feeling silky without an oily residue. For all skin types. Particularly recommended at the end of the day as a body treatment to calm and alleviate stress.",
                    Price = 2_590_000m, // Giá sản phẩm là 2.590.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Comfortzone
                    Dimension = "200ml",
                    Volume = 200m // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Body Strategist Peel Scrub",
                    ProductDescription = "The double mechanical and chemical exfoliating action ensure that the skin immediately looks smoother, softer, and brighter. For all skin types, especially dry and rough skin. Ideal all year round as a weekly treatment.",
                    Price = 1_500_000m, // Giá sản phẩm là 1.500.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Comfortzone
                    Dimension = "200ml",
                    Volume = 200m // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Body Strategist Oil",
                    ProductDescription = "A blend of precious natural oils that is ideal for dryness, stretch marks and loss of elasticity. Improves skin texture & tone with a blend of natural oils.",
                    Price = 1_680_000m, // Giá sản phẩm là 1.680.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 150ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Comfortzone
                    Dimension = "150ml",
                    Volume = 150m // Tổng dung tích là 150ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Body Strategist Contour Cream",
                    ProductDescription = "Lightweight, hydrating Body Strategist Contour Cream gives an immediate sense of firmness to the skin with the power of science + nature with key ingredient ACMELLA OLERACEA EXTRACT, known for its ability to reduce muscle contractions. Ideal for all skin types with loss of elasticity and firmness.",
                    Price = 1_680_000m, // Giá sản phẩm là 1.680.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Comfortzone
                    Dimension = "200ml",
                    Volume = 200m // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Body Hydrating Cream",
                    ProductDescription = "Hydrate, smooth and tone: this nourishing cream features a worldly collection of essential oils to benefit all skin conditions. Aromatic Orange Oil and Chinese Green Tea soothe and soften skin. French Lavender and Indonesian Patchouli oils calm the senses while naturally-derived Lactic Acid and hydroxy acid extracts from Cane Sugar and Apple smooth skin and relieve dryness – all in a silky, medium-weight formula that absorbs easily for immediate, glowing hydration.",
                    Price = 1_260_000m, // Giá sản phẩm là 1.260.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 295ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Dermalogica
                    Dimension = "295ml",
                    Volume = 295m // Tổng dung tích là 295ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Phyto Replenish Body Oil",
                    ProductDescription = "Replenish, calm and moisturize with this antioxidant-rich blend of skin-nourishing oils. French Plum Seed Oil, Avocado Oil and Sunflower Seed Oil are rich in skin-replenishing Omega Fatty Acids such as Linoleic, Linolenic and Oleic Acid plus Vitamin E to help protect skin’s lipid barrier. Infused with calming Fermented Red Ginseng – inspired by Korean skin care rituals – this body oil calms and nourishes to deliver glowing skin. Lightly infused with bright Bergamot, Neroli and Orange along with relaxing aromas of Patchouli and Sandalwood, it leaves skin soft and delicately scented.",
                    Price = 1_960_000m, // Giá sản phẩm là 1.960.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 125ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Dermalogica
                    Dimension = "125ml",
                    Volume = 125m // Tổng dung tích là 125ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Conditioning Body Wash",
                    ProductDescription = "Cleanse, condition and invigorate with this richly-sensorial, skin-nourishing body wash. Inspired by the essential oils diffused in Turkish hammams, this silky, gently cleansing formula features aromatic oils of French Rosemary and Chinese Eucalyptus alongside fresh Tea Tree and Lemon oils to cleanse skin and awaken the senses. Pro-Vitamin B5 and tranquil Sandalwood, Lavender and Clary Sage smooth and condition for a truly transformative finish.",
                    Price = 1_260_000m, // Giá sản phẩm là 1.260.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 295ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // Dermalogica
                    Dimension = "295ml",
                    Volume = 295m // Tổng dung tích là 295ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Nourishing Glow Body Oil",
                    ProductDescription = "Luxurious blend of pure plant oils and squalene soothe skin and drench it with lasting hydration. Antioxidant Vitamins C and E work to illuminate, protect and soften while fine, shimmering mica instantly illuminates leaving a finespun, dewy radiance. Nourishing garden-grown herbs and flowers, including rosemary, plumeria, sandalwood and jasmine nourish and impart an addictive, otherworldly scent.",
                    Price = 1_980_000m, // Giá sản phẩm là 1.980.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 100ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // HydroPeptide
                    Dimension = "100ml",
                    Volume = 100m // Tổng dung tích là 100ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Firming Body Moisturizer",
                    ProductDescription = "This rich, firming body cream will have you wanting to show some skin. Go ahead, be a showoff. This firming, tightening body cream is chock-full of ingredients that not only help reduce the appearance of stretch marks, scars and discoloration, but also fabulously tighten and hydrate.",
                    Price = 2_510_000m, // Giá sản phẩm là 2.510.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // HydroPeptide
                    Dimension = "200ml",
                    Volume = 200m // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Illiyoon Ceramide Ato Concentrate Cream",
                    ProductDescription = "ILLIYOON Ceramide Ato Concentrate Cream is a moisturizer from the ILLIYOON brand from Korea. With moisturizing ingredients combined with nutrients from nature, it nourishes the skin in a gentle, benign way, suitable for all ages and all skin types.",
                    Price = 295_000m, // Giá sản phẩm là 295.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // K Beauty
                    Dimension = "200ml",
                    Volume = 200m // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Dear Doer The Hidden Body Scrub & Wash",
                    ProductDescription = "The Hidden Body Scrub | Smoothing: The 2-in-1 body cleanser & scrub, with plant-derived ingredients, creates a rich lather to cleanse all over body. Fine Perlite from volcanic rocks and coarse Andes salt exfoliate dead skin cells with the tranquil scent.",
                    Price = 450_000m, // Giá sản phẩm là 450.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // K Beauty
                    Dimension = "300ml",
                    Volume = 300m // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Aestura Atobarrier 365 Ceramide Lotion",
                    ProductDescription = "Moisturizes, brightens and strengthens the skin's resistance, helping to keep skin healthy and prevent cracking and dryness.",
                    Price = 600_000m, // Giá sản phẩm là 600.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 150ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // K Beauty
                    Dimension = "150ml",
                    Volume = 150m // Tổng dung tích là 150ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Dr. Orga pH-balanced Houttuynia Cordata Red Spot Mist",
                    ProductDescription = "Antioxidant-rich heart leaf extract puts the Houttuynia Cordata in the Dr. Orga pH-balanced Houttuynia Cordata Red Spot Mist. This popular K-beauty ingredient is known for soothing and healing inflammation, as well as preventing skin dehydration. It’s joined by AHA, BHA, and PHA to chemically exfoliate skin and banish body acne.",
                    Price = 590_000m, // Giá sản phẩm là 590.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 150ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // K Beauty
                    Dimension = "150ml",
                    Volume = 150m // Tổng dung tích là 150ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Derma B Daily Moisture Body Oil",
                    ProductDescription = "This lightweight, long-lasting body oil, enriched with natural ingredients like sweet almond, argan, and camellia seed oil, deeply moisturizes and firms the skin. Its gentle formula is suitable for all skin types, including sensitive skin. The refreshing peach scent provides a delightful sensory experience.",
                    Price = 430_000m, // Giá sản phẩm là 430.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 8, // Body
                    CompanyId = 1, // K Beauty
                    Dimension = "200ml",
                    Volume = 200m // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Davines Volu Shampoo",
                    ProductDescription = "Davines Volu Shampoo is suitable for thin and flat hair, helping to increase volume for more fluffy and bouncy hair. The special formula helps create small, smooth foam particles that gently remove daily dirt, making hair noticeably lighter and softer. This shampoo has a light floral scent, which is the common scent of Volu hair care products.",
                    Price = 915_000m, // Giá sản phẩm là 915.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Davines
                    Dimension = "1000ml",
                    Volume = 1000m // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Davines Calming Shampoo",
                    ProductDescription = "Suitable for sensitive or easily irritated scalps, it provides a gentle and pleasant cleansing experience. It gently cleanses dirt from hair and scalp, leaving hair softer and smoother. With ingredients from blueberries, a nutrient-dense superfood, full of vitamins C and B and antioxidants, essential for the health and protection of hair and skin. They are also known for their effective anti-inflammatory properties.",
                    Price = 985_000m, // Giá sản phẩm là 985.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Davines
                    Dimension = "1000ml",
                    Volume = 1000m // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Davines Dede Shampoo",
                    ProductDescription = "Suitable for all scalp types, it gently removes daily dirt, leaving hair feeling softer and lighter. The product has a super-fine foam formula that helps to relax and refresh when used. This shampoo has a fresh lemon scent.",
                    Price = 915_000m, // Giá sản phẩm là 915.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Davines
                    Dimension = "1000ml",
                    Volume = 1000m // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Davines Melu Shampoo",
                    ProductDescription = "Davines Melu Shampoo is especially suitable for long or damaged hair. With its special formula, it helps to restore and prevent split ends effectively. The product has a soft cream form that gently removes daily dirt from the scalp. This shampoo has a floral and woody scent, which is the common scent of Melu hair care products.",
                    Price = 1_055_000m, // Giá sản phẩm là 1.055.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Davines
                    Dimension = "1000ml",
                    Volume = 1000m // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Bain Décalcifiant Réparateur Repairing Shampoo",
                    ProductDescription = "Water is good for you. But not so great for your hair. The calcium in your shower water can lead to persistent hair damage leaving it rigid, dull & brittle. This powerful system removes calcium build up that causes damage and restores up to 99% of hair’s original strength. It repairs persistent damage, reversing stiffness and dullness for 73% shinier hair, and 2x smoother hair. Hair is 93% stronger.",
                    Price = 1_780_000m, // Giá sản phẩm là 1.780.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 500ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Kerastase
                    Dimension = "500ml",
                    Volume = 500m // Tổng dung tích là 500ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Bain Densité Shampoo",
                    ProductDescription = "Cleanses hair build-up. Increases hair density. Strengthens hair. Creates shiny hair. Develops fullness through the ends. Distributes resilience beginning at the root.",
                    Price = 1_780_000m, // Giá sản phẩm là 1.780.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 500ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Kerastase
                    Dimension = "500ml",
                    Volume = 500m // Tổng dung tích là 500ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Bain Hydra-Fortifiant Shampoo",
                    ProductDescription = "Silicone-free. Gently removes sebum & pollution particles from scalp and hair. Provides oil-control. Prevents risk of hair-fall due to breakage from brushing. 97% less hair fall due to breakage from brushing.",
                    Price = 1_780_000m, // Giá sản phẩm là 1.780.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 500ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Kerastase
                    Dimension = "500ml",
                    Volume = 500m // Tổng dung tích là 500ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "L'Oreal Paris Elseve Total Repair 5 Repairing Shampoo",
                    ProductDescription = "Total Repair 5 is a care solution that helps reduce the 5 signs of damaged hair. With the new Ceramide ingredient similar to the natural keratin ingredient in hair fibers, it quickly penetrates weak and damaged hair areas, helping your hair look stronger, smoother, shinier, smoother, and reduce split ends.",
                    Price = 200_000m, // Giá sản phẩm là 200.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 650ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // L'Oreal
                    Dimension = "650ml",
                    Volume = 650m // Tổng dung tích là 650ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "L'Oreal Professional Hair Spa Deep Nourishing Shampoo",
                    ProductDescription = "Extracted from Tea Tree essential oil to clean dandruff, prevent odor and sweat on the scalp. Combined with massage to relax the scalp, helping hair and scalp stay healthy and refreshing.",
                    Price = 319_000m, // Giá sản phẩm là 319.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 600ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // L'Oreal
                    Dimension = "600ml",
                    Volume = 600m // Tổng dung tích là 600ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "L'Oreal Paris Elseve Fall Resist 3X Anti-Hairfall Shampoo",
                    ProductDescription = "Provides hair with essential nutrients, prevents hair loss, giving you more beautiful and stronger hair. Anti-hair loss formula with 3 effects: nourishes from the roots, regenerates hair structure, and makes hair grow stronger.",
                    Price = 200_000m, // Giá sản phẩm là 200.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 650ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // L'Oreal
                    Dimension = "650ml",
                    Volume = 650m // Tổng dung tích là 650ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Nº.4 Bond Maintenance Shampoo",
                    ProductDescription = "A highly concentrated shampoo that strengthens and adds shine to damage-prone hair as it gently cleanses. The rich lather nourishes and repairs to leave hair smoother and more manageable.",
                    Price = 760_000m, // Giá sản phẩm là 760.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Opalex
                    Dimension = "250ml",
                    Volume = 250m // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Gold Lust Repair & Restore Shampoo",
                    ProductDescription = "Reawaken your hair to its glossiest, healthiest prime. This rejuvenating cleanser combines centuries-old healing oils and extracts—cypress and argan—with our revolutionary bio-restorative complex to balance the scalp and reinforce the inner strength of each strand.",
                    Price = 1_350_000m, // Giá sản phẩm là 1.350.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Oribe
                    Dimension = "250ml",
                    Volume = 250m // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Supershine Hydrating Shampoo",
                    ProductDescription = "Lather in luxury. This illuminating shampoo richly hydrates and smoothes, transforming dull strands into glossy locks. So brilliant. Gently cleanses while maintaining hair’s natural oils, amplifies shine for radiant, glossy locks, optimally hydrates hair without weighing it down, softens and smoothes strands for enhanced shine and brilliance, nourishes hair and protects it from moisture loss.",
                    Price = 1_245_000m, // Giá sản phẩm là 1.245.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Oribe
                    Dimension = "250ml",
                    Volume = 250m // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Acidic Bonding Concentrate sulfate-free Shampoo",
                    ProductDescription = "Acidic Bonding Concentrate sulfate-free shampoo is Redken's most concentrated all-in-one formula for strength repair on all types of damaged hair.",
                    Price = 840_000m, // Giá sản phẩm là 840.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Redken
                    Dimension = "300ml",
                    Volume = 300m // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Redken All Soft Shampoo",
                    ProductDescription = "Shampoo for dry hair with Redken's Moisture Complex. Gently cleanses while adding hydration, silkiness and a healthy look to hair. Leaves hair silky soft with increased manageability, suppleness and shine.",
                    Price = 770_000m, // Giá sản phẩm là 770.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Redken
                    Dimension = "300ml",
                    Volume = 300m // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Izumi Tonic Strengthening Shampoo",
                    ProductDescription = "This strengthening shampoo gently cleanses, repairs, and thickens fragile hair. Formulated with rice water to strengthen and repair, Izumi Tonic Shampoo leaves hair 30x stronger* and reduces split ends by up to 91%*. This shampoo provides a deep yet gentle cleanse while strengthening and adding nourishment to hair. It features a lightweight formula that cleanses and thickens without stripping hair of its hydration.",
                    Price = 1_250_000m, // Giá sản phẩm là 1.250.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Shu uemura (CompanyId mặc định là 5)
                    Dimension = "300ml",
                    Volume = 300m, // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Ultimate Reset Extreme Repair Shampoo",
                    ProductDescription = "This shampoo can be used to repair and restore very damaged hair that has undergone frequent bleaching, coloring, or chemical treatments. It provides intense care without weighing hair down. The delicate cleanser helps to lift away any dirt, impurities, and product build-up while strengthening strands from root to tip to fortify against future hair breakage, leaving healthy hair feeling soft. Safe on colored hair so your hair color can thrive without sacrificing the health of your hair.",
                    Price = 1_340_000m, // Giá sản phẩm là 1.340.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Shu uemura (CompanyId mặc định là 1)
                    Dimension = "300ml",
                    Volume = 300m, // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Fusion Shampoo",
                    ProductDescription = "The shampoo’s Metal Purifier technology removes metallic impurities through antioxidant action. Micronized lipids provide instant wet conditioning and intense repair.",
                    Price = 965_000m, // Giá sản phẩm là 965.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    Volume = 250m, // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Ultimate Repair Shampoo",
                    ProductDescription = "A rich cream shampoo with a lightweight foaming formula that gently and effectively cleanses hair with a luxurious lather. Formulated with our Metal Purifier technology, this shampoo detoxifies and rebuilds heat- and bleach-damaged hair inside and out, leaving it looking healthy, shiny and smooth.",
                    Price = 660_000m, // Giá sản phẩm là 660.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1, // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    Volume = 250m, // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Davines Dede Conditioner",
                    ProductDescription = "The product has the ability to nourish the hair, preventing tangles and keeping the hair soft and smooth. In addition, when using the product, you will feel your hair softer, lighter and more bouncy.",
                    Price = 1_318_000m, // Giá sản phẩm là 1.318.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Davines Love Smoothing Conditioner",
                    ProductDescription = "Davines Love Smoothing Conditioner is specially designed for dry, frizzy, curly hair to help you have smoother hair. In addition, the product also helps balance the hair's moisture to make hair lighter, softer, and more bouncy.",
                    Price = 1_550_000m, // Giá sản phẩm là 1.550.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Davines Melu Conditioner",
                    ProductDescription = "The product has the ability to prevent split ends very well, in addition, the product also helps your hair become lighter and softer. The product also gently nourishes the hair shaft from deep inside, making your hair thicker and healthier. This conditioner has a floral and woody scent.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Davines Momo Conditioner",
                    ProductDescription = "This lightweight conditioner nourishes and hydrates hair, preventing tangles and leaving it soft and smooth. Its rich foam and pleasant floral scent enhance the hair care experience.",
                    Price = 1_318_000m, // Giá sản phẩm là 1.318.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Fondant Renforçateur Conditioner",
                    ProductDescription = "Silicone-free. Sulfate-free. Featuring anti-breakage action & fiber fortification. Lightweight. Provides immediate strength and softness. Provides moisture and shine. Detangles and adds body. 97% less hair fall due to breakage from brushing.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    Volume = 200m, // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Fondant Densité Conditioner",
                    ProductDescription = "Improves hair texture. Creates thick hair. Easily detangles hair.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    Volume = 200m, // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Fondant Fluidealiste Conditioner",
                    ProductDescription = "This luxurious conditioner is the perfect solution for dry, unruly hair. Its lightweight formula penetrates deep into the hair shaft, providing intense hydration and smoothing out the hair cuticle. Say goodbye to frizz and hello to silky, manageable hair.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    Volume = 200m, // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Ciment Anti-Usure Conditioner",
                    ProductDescription = "Revitalize your hair with this transformative treatment. It repairs damaged hair, adds a radiant shine, and strengthens the hair fiber to prevent breakage. Experience softer, healthier, and more vibrant hair.",
                    Price = 1_110_000m, // Giá sản phẩm là 1.110.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    Volume = 200m, // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Redken All Soft Conditioner",
                    ProductDescription = "Deeply conditions and hydrates dry, brittle hair, leaving it soft, smooth, and shiny.",
                    Price = 840_000m, // Giá sản phẩm là 840.000 VND
                    Quantity = 1, // Dung tích là 300ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Redken (CompanyId mặc định là 1)
                    Dimension = "300ml",
                    Volume = 300m, // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Redken Frizz Dismiss Conditioner",
                    ProductDescription = "Smooths frizz and flyaways, leaving hair silky and manageable.",
                    Price = 770_000m, // Giá sản phẩm là 770.000 VND
                    Quantity = 1, // Dung tích là 300ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Redken (CompanyId mặc định là 1)
                    Dimension = "300ml",
                    Volume = 300m, // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "L'Oréal Paris Elvive Total Repair 5 Conditioner",
                    ProductDescription = "This conditioner is designed to strengthen and repair damaged hair. Formulated with a blend of ceramides, it targets five signs of damage: split ends, weakness, roughness, dullness, and dehydration, leaving hair smooth and revitalized.",
                    Price = 145_000m, // Giá sản phẩm là 145.000 VND
                    Quantity = 1, // Dung tích là 355ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // L'Oreal (CompanyId mặc định là 1)
                    Dimension = "355ml",
                    Volume = 355m, // Tổng dung tích là 355ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "L'Oréal Paris EverPure Moisture Conditioner",
                    ProductDescription = "This sulfate-free conditioner is perfect for color-treated hair. It provides deep moisture and nourishment, enhancing color vibrancy while keeping hair soft and manageable. Infused with rosemary, it helps to maintain shine and hydration without weighing hair down.",
                    Price = 170_000m, // Giá sản phẩm là 170.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // L'Oreal (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    Volume = 250m, // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "L'Oréal Paris EverCurl Hydracharge Conditioner",
                    ProductDescription = "Specifically formulated for curly hair, this conditioner hydrates and defines curls while reducing frizz. Its lightweight formula enhances curl bounce and softness, providing lasting moisture and a touchable finish without sulfates or harsh salts.",
                    Price = 195_000m, // Giá sản phẩm là 195.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // L'Oreal (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    Volume = 250m, // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Invigo Nutri-Enrich Conditioner",
                    ProductDescription = "Formulated for dry or stressed hair, this conditioner nourishes and replenishes moisture. It contains goji berry extract and vitamin E, helping to improve hair elasticity and manageability while providing a soft, healthy look.",
                    Price = 460_000m, // Giá sản phẩm là 460.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Elements Renewing Conditioner",
                    ProductDescription = "This silicone-free conditioner revitalizes and nourishes all hair types. Infused with natural extracts, it provides deep moisture and helps to strengthen hair, leaving it soft and healthy without weighing it down.",
                    Price = 1_180_000m, // Giá sản phẩm là 1.180.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Moisture Velvet Nourishing Conditioner",
                    ProductDescription = "This luxurious conditioner deeply nourishes dry and coarse hair, providing essential moisture and enhancing softness. Formulated with Japanese Peony, it helps to detangle hair while leaving it smooth, healthy, and radiant.",
                    Price = 1_200_000m, // Giá sản phẩm là 1.200.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Shu uemura (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Ultimate Remedy Conditioner",
                    ProductDescription = "Designed for extremely damaged hair, this conditioner offers intense repair and hydration. Enriched with the goodness of natural ingredients, it restores hair’s strength and elasticity, making it look revitalized and manageable.",
                    Price = 1_320_000m, // Giá sản phẩm là 1.320.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Shu uemura (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    Volume = 1000m, // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "No. 5 Bond Maintenance Conditioner",
                    ProductDescription = "This conditioner is formulated to hydrate and repair damaged hair while reducing breakage. It works to maintain the health of hair after chemical treatments, leaving it soft, shiny, and manageable.",
                    Price = 675_000m, // Giá sản phẩm là 675.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Opalex (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    Volume = 250m, // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                    SkinTypeSuitable = "All",
                    ProductName = "Gold Lust Repair & Restore Conditioner",
                    ProductDescription = "This luxurious conditioner revitalizes and restores damaged hair while enhancing shine and softness. Infused with a blend of healing oils and extracts, it helps to strengthen hair and prevent future damage, making it perfect for all hair types.",
                    Price = 1_490_000m, // Giá sản phẩm là 1.490.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Oribe (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    Volume = 250m, // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",
                        SkinTypeSuitable = "All",
                    ProductName = "Signature Moisture Masque",
                    ProductDescription = "This deep conditioning mask provides intense hydration and nourishment for dry, brittle hair. Formulated with a rich blend of natural ingredients, it helps to improve elasticity and restore hair's natural moisture balance, resulting in soft, manageable strands.",
                    Price = 1_640_000m, // Giá sản phẩm là 1.640.000 VND
                    Quantity = 1, // Dung tích là 160ml
                    Discount = 0.0m, // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1, // Oribe (CompanyId mặc định là 1)
                    Dimension = "160ml",
                    Volume = 160m, // Tổng dung tích là 160ml
                },
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
                    Status = "Active",
                    Name = "Signature Facial", 
                    Duration = "60 phút", 
                    Price = 600_000m, 
                    ServiceCategoryId = 1,
                    Description = "A tailored facial to meet your skin’s needs.",
                    Steps = "1. Skin consultation.\n2. Cleansing.\n3. Exfoliation.\n4. Mask and moisturizer application.\n5. Relaxing facial massage."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Anti-Aging Facial",
                    Duration = "90 phút",
                    Price = 800_000m,
                    ServiceCategoryId = 2,
                    Description = "Reduces fine lines and restores youthful glow.",
                    Steps = "1. Cleansing.\n2. Serum application.\n3. Lifting massage.\n4. Anti-aging mask.\n5. SPF protection."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Hydrating Facial",
                    Duration = "60 phút",
                    Price = 500_000m,
                    ServiceCategoryId = 3,
                    Description = "Deeply hydrates and plumps your skin.",
                    Steps = "1. Cleansing.\n2. Hydration serum.\n3. Mask for hydration.\n4. Moisturizer application."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Brightening Facial",
                    Duration = "75 phút",
                    Price = 650_000m,
                    ServiceCategoryId = 4,
                    Description = "Brightens dull and uneven skin tones.",
                    Steps = "1. Cleansing.\n2. Exfoliation.\n3. Vitamin C serum.\n4. Brightening mask.\n5. SPF protection."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Acne Treatment Facial",
                    Duration = "90 phút",
                    Price = 700_000m,
                    ServiceCategoryId = 5,
                    Description = "Targets acne and reduces breakouts.",
                    Steps = "1. Skin analysis.\n2. Deep cleansing.\n3. Spot treatment.\n4. Calming mask.\n5. SPF application."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Soothing Facial",
                    Duration = "60 phút",
                    Price = 550_000m,
                    ServiceCategoryId = 6,
                    Description = "Calms and soothes sensitive skin.",
                    Steps = "1. Gentle cleansing.\n2. Anti-inflammatory mask.\n3. Relaxing facial massage.\n4. Moisturizer and SPF."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Green Tea Facial",
                    Duration = "60 phút",
                    Price = 500_000m,
                    ServiceCategoryId = 7,
                    Description = "Antioxidant-rich facial for youthful skin.",
                    Steps = "1. Cleansing.\n2. Green tea serum application.\n3. Antioxidant mask.\n4. Moisturizer and SPF."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Collagen Boost Facial",
                    Duration = "75 phút",
                    Price = 700_000m,
                    ServiceCategoryId = 8,
                    Description = "Boosts collagen production for firmer skin.",
                    Steps = "1. Cleansing.\n2. Collagen serum application.\n3. Facial massage.\n4. Collagen-infused mask.\n5. SPF protection."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Detox Facial",
                    Duration = "60 phút",
                    Price = 600_000m,
                    ServiceCategoryId = 9,
                    Description = "Purifies and detoxifies your skin.",
                    Steps = "1. Cleansing.\n2. Detoxifying mask.\n3. Serum application.\n4. Moisturizer and SPF."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Overnight Hydration Facial",
                    Duration = "90 phút",
                    Price = 750_000m,
                    ServiceCategoryId = 10,
                    Description = "Intense hydration for glowing morning skin.",
                    Steps = "1. Cleansing.\n2. Overnight hydration serum.\n3. Relaxing facial massage.\n4. Moisturizing overnight mask."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Swedish Massage",
                    Duration = "60 phút",
                    Price = 600_000m,
                    ServiceCategoryId = 1,
                    Description = "Classic massage for relaxation and stress relief.",
                    Steps = "1. Consultation.\n2. Gentle Swedish massage.\n3. Use of aromatic oils for relaxation."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Full Body Scrub",
                    Duration = "75 phút",
                    Price = 650_000m,
                    ServiceCategoryId = 2,
                    Description = "Exfoliates and renews your skin.",
                    Steps = "1. Application of body scrub.\n2. Gentle exfoliation.\n3. Rinse and hydrating lotion application."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Moisturizing Body Wrap",
                    Duration = "60 phút",
                    Price = 600_000m,
                    ServiceCategoryId = 3,
                    Description = "Deep hydration treatment for dry skin.",
                    Steps = "1. Body exfoliation.\n2. Application of hydrating wrap."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Aromatherapy Massage",
                    Duration = "90 phút",
                    Price = 750_000m,
                    ServiceCategoryId = 4,
                    Description = "Massage with essential oils for relaxation.",
                    Steps = "1. Consultation.\n2. Aromatherapy massage focusing on tension areas.\n3. Application of calming essential oils."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Foot Massage",
                    Duration = "45 phút",
                    Price = 400_000m,
                    ServiceCategoryId = 5,
                    Description = "Relaxes and soothes tired feet.",
                    Steps = "1. Warm foot soak.\n2. Relaxing foot massage.\n3. Moisturizing cream application."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Abdominal Massage",
                    Duration = "30 phút",
                    Price = 500_000m,
                    ServiceCategoryId = 6,
                    Description = "Eases abdominal discomfort and improves digestion.",
                    Steps = "1. Gentle abdominal massage.\n2. Use of essential oils for relaxation.\n3. Consultation for personalized care."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Detox Body Treatment",
                    Duration = "90 phút",
                    Price = 800_000m,
                    ServiceCategoryId = 7,
                    Description = "Detoxifies and purifies the body.",
                    Steps = "1. Body scrub.\n2. Detoxifying body mask.\n3. Relaxing massage.\n4. Hydrating lotion application."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Mud Bath",
                    Duration = "75 phút",
                    Price = 700_000m,
                    ServiceCategoryId = 9,
                    Description = "Full-body mud treatment for detoxification.",
                    Steps = "1. Application of warm mud mask.\n2. Relaxation period.\n3. Rinse and hydration with lotion."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Body Polish",
                    Duration = "60 phút",
                    Price = 600_000m,
                    ServiceCategoryId = 10,
                    Description = "Gently exfoliates and polishes your skin.",
                    Steps = "1. Application of body polish.\n2. Exfoliation and cleansing.\n3. Hydration with moisturizing cream."
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
                    CompanyId = 1,// ID của công ty đã seed trước đó
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

            // Danh sách URL hình ảnh thật
            var promotionImages = new[]
            {
                "https://snov.io/blog/wp-content/uploads/2021/08/Webp.net-resizeimage3-1024x512.png",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRHmC_hrFziWofmW8-MGF1KSFjhQ7-94FrfTTVpkhIWnUWZwaH-d2eddzpJ2KUhhmB0bIU&usqp=CAU",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTElAKcuC8I24BKiVbYWfHnrcjT7sfydre7uRMtzKCIWkpDbALSf4LyLhpTzHcB7CiGJ70&usqp=CAU",
                "https://www.rewardport.in/wp-content/uploads/2024/05/Latest-Consumer-Promotion-Trends-That-Are-Shaping-The-Future-of-Marketing-2.webp",
                "https://5.imimg.com/data5/EZ/BK/JV/SELLER-8510670/advertisement-and-sales-promotion-strategy.png",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQNVBx8qjAx-itp2-T8XLHt4FitkMh9HsTdbDC3WUj_cQRCgJXm8buAZ1usfJ_6VVdWMic&usqp=CAU",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR4a_LWSSRUOCxdLI2u1qyxKH1n3Tnpc2QRVVh-i4peRgGimaP5GSOl-9xE5lY8xFueh7s&usqp=CAU"
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
                    UpdatedDate = DateTime.Now,
                    Image = promotionImages[random.Next(promotionImages.Length)] // Random hình ảnh thật
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

    // Tạo danh sách các sản phẩm và hình ảnh của chúng
    var productImageData = new List<(string productName, List<string> imageUrls)>
    {
        // Remedy Cream To Oil Cleanser
        ("Remedy Cream To Oil", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/Remedy-1-01-1-1091x1200.png",
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/f9a79b2aadeddf3f7b904fe2fdd2bd390b786956_2000x-1091x1200.jpg",
            "https://eideal.com/cdn/shop/files/Cream-to-oil-Life-2.jpg?v=1703578773"
        }),

        // Essential Face Wash
        ("Essential Face Wash", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/12100-essential-face-wash-150ml_inner-1091x1200.png",
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/9ff738e7c2d747062d121d0fc4b563eff31ef198_2000x-1091x1200.jpg"
        }),

        // Active Pureness Cleasing Gel
        ("Active Pureness Cleasing Gel", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/Active-Pureness-Gel_San-pham-1091x1200.png",
            "https://icgroup.dk/resources/product/119/43/activec-pureness---gel.png?width=800&height=600",
            "https://www.michaelahann.at/wp-content/uploads/2022/08/Active-Pureness-Gel.jpg"
        }),

        // Clearing Skin Wash
        ("Clearing Skin Wash", new List<string>
        {
            "https://www.dermalogica.com/cdn/shop/files/clearing-skin-wash_8.4oz_front.jpg?v=1710455212&width=1946", 
            "https://vn-test-11.slatic.net/p/1c096bc4b5b03330c62465154db802d2.jpg", 
            "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/25/images/dermalogica-clearing-skin-wash-250ml-2.jpg"
        }),

        // Oil To Foam Cleanser
        ("Oil To Foam Cleanser", new List<string>
        {
            "https://sieuthilamdep.com/images/detailed/20/sua-rua-mat-tay-trang-2-trong-1-dermalogica-oil-to-foam-total-cleanser.jpg", 
            "https://www.thedermacompany.co.uk/wp-content/uploads/2023/06/Dermalogica-Oil-To-Foam-Cleanser-Lifestyle.jpg", 
            "https://skinmart.com.au/cdn/shop/products/DermalogicaOilToFoamCleanser_2_2560x.png?v=1680137764"
        }),

        // Micellar Prebiotic PreCleanse
        ("Micellar Prebiotic PreCleanse", new List<string>
        {
            "https://dermalogica.com.vn/cdn/shop/products/dermalogica-vietnam-cleansers-150ml-coming-soon-micellar-precleanse-s-a-t-y-trang-ch-a-prebiotic-danh-cho-m-i-lo-i-da-31640176623821.png?v=1717214352&width=1946", 
            "https://edbeauty.vn/wp-content/uploads/2024/08/image_2024_08_02T10_27_06_857Z.png", 
            "https://cdn1.parfuemerie-becker.de/media/cache/article/variation/109000/035020189_95635_1709027057.png"
        }),

        // Kmobucha Microbiome Foaming Cleaser
        ("Kmobucha Microbiome Foaming Cleaser", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-kombucha-microbiome-foaming-cleanser-pdp.jpg?itok=XFjrWeJc", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAs0nEmyqyQbx0hl4vz1MUoLloMBlBJTO4QQ&s", 
            "https://emstore.com/cdn/shop/products/cleanser-2_75951af1-f4c2-4d54-80dd-9936b699a5c8.png?v=1683718720"
        }),

        // Monoi Age Corrective Exfoliating Cleanser
        ("Monoi Age Corrective Exfoliating Cleanser", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-monoi-age-corrective-exfoliating-cleanser-pdp-compressed.jpg?itok=pRBEP6CO", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT7vbzeB98PqQkVa4vQKztlvJZTh1-H0DWPF62hO-Uo3_PGI5iz4E1jFv_p2VAD2LrgA-c&usqp=CAU", 
            "https://www.facethefuture.co.uk/cdn/shop/files/eminence-organics-monoi-age-corrective-exfoliating-cleanser-swatch-compressed.jpg?v=1695310185&width=600"
        }),

        // Acne Advanced Cleansing Foam
        ("Acne Advanced Cleansing Foam", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-acne-advanced-cleansing-foam-v2-400pix-compressor.jpg?itok=7eKH7RYv", 
            "https://wildflowerbeautystudio.ca/cdn/shop/products/acnecleansingfoam_300x300.jpg?v=1613861748", 
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-slide/eminence-organics-acne-advanced-cleansing-foam-swatch-400x400.jpg?itok=_451iEc7"
        }),
        
        // Lemon Grass Cleanser
        ("Lemon Grass Cleanser", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQg8RxjJM_IxA0iIIBfpofAqmoH7QLa7zIy2Q&s", 
            "https://images.squarespace-cdn.com/content/v1/5ea87ac4bf0b761180ffcfae/1626276341235-D5E9PCNK5OSRIP2RALJN/LemonGrassCleanser.jpg?format=1000w", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTcyaLelaqtVcZiyeWTM4QOoJVXWQveYuorDw&s"
        }),
        
        // Charcoal Exfoliating Gel Cleanser
        ("Charcoal Exfoliating Gel Cleanser", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQcNxhG4XNakMZOCjxT2ulB1i2cnWkTVq1qxw&s", 
            "https://naturalbeautygroup.com/cdn/shop/files/Eminence-Organics-Charcoal-Exfoliating-Gel-Cleanser-Lifestyle.jpg?v=1711740188&width=1080", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRbNf8GcQLHfBysKN1piBJLl5kX-ovUWTfWhw&s"
        }),
        // Beplain Mung Bean pH-Balanced Cleansing Foam
        ("Beplain Mung Bean pH-Balanced Cleansing Foam", new List<string>
        {
            "https://product.hstatic.net/200000773671/product/907d907dd544731a2a55_97fc43aed51040a58f76b2512b39f457_master.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT1ZAGpmlp5k_1VB64FLzA6HfAkRMDHXybYEw&s", 
            "https://product.hstatic.net/200000773671/product/ece36b53526af434ad7b_7e25a0c0d2f641e9a0fcfe8371587fca_master.jpg"
        }),
        // ISNTREE Yam Root Vegan Milk Cleanser
        ("ISNTREE Yam Root Vegan Milk Cleanser", new List<string>
        {
            "https://www.kanvasbeauty.com.au/cdn/shop/files/7_5a516b9f-1e89-47af-a0f3-c9e419cbee24_1200x.jpg?v=1711882846", 
            "https://www.skincupid.co.uk/cdn/shop/files/ISNTREEYamRootVeganMilkCleanser_220ml_5.png?v=1728904708&width=800", 
            "https://koreanskincare.nl/cdn/shop/files/452359592_470675092412851_8771590449487260742_n.jpg?v=1721894307"
        }),
        
        // Normaderm Anti-Acne Purifying Gel Cleanser
        ("Normaderm Anti-Acne Purifying Gel Cleanser", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAztQlHO_gcl1kdZRAKV5QSYVFcAVKnen6yg&s", 
            "https://sohaticare.com/cdn/shop/files/3337875663076_3_82da998a-a47b-4461-a110-036d702f4886_4000x@3x.progressive.jpg?v=1706877499", 
            "https://bng.com.pk/cdn/shop/files/e3906fe2-cb9a-47d8-8a03-73dc6d7f25ce_0df3ce49-2f5b-4d45-b6f7-7db99714a57e_2400x.jpg?v=1720805633"
        }),
        
        // Purete Thermale Fresh Cleansing Gel
        ("Purete Thermale Fresh Cleansing Gel", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTBGIU-XNebd-dJK6_u0DMg5l1MKiD1l4zCJg&s", 
            "https://www.binsina.ae/media/catalog/product/8/1/81414_2.jpg?optimize=medium&bg-color=255,255,255&fit=bounds&height=600&width=600&canvas=600:600", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSw-hWi1mf7kCZzWrthwsn-3Opba1kYnJcQmA&s"
        }),
        
        // Purete Thermale Cleansing Foaming Cream
        ("Purete Thermale Cleansing Foaming Cream", new List<string>
        {
            "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/other-products/purete-thermale---hydrating-and-cleansing-foaming-cream/hydrating-cleansing-foaming-cream-pack2.jpeg?rev=8a1f1df622f24bfe963a86befb0031b4&sc_lang=vi-vn&cx=0.47&cy=0.43&cw=525&ch=596&hash=D4ABC102BDEC8E803268FC703A80B685", 
            "https://images-na.ssl-images-amazon.com/images/I/81fpwzdjEgL.jpg", 
            "https://m.media-amazon.com/images/I/71e3vIZEiBL._AC_UF1000,1000_QL80_.jpg"
        }),
        
        // Foaming Cream Cleanser
        ("Foaming Cream Cleanser", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/021924_FoamingCleanser_Carousel-Hero_1024x1024.jpg?v=1709334047", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRhMF2XnUc1FGnqMiZFBkLTUJJLw8mCOGt05A&s", 
            "https://templeskincare.com.au/wp-content/uploads/2024/10/20231026_027-Edit_5000px-Edit-CreamCleanser-scaled.jpg"
        }),
        
        // Exfoliating Cleanser
        ("Exfoliating Cleanser", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/010924_Retail_ExfoliatingCleanser_PDP_1024x1024.jpg?v=1713463787", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRn6R3GviQQlSKoSH8nmK0q8PPuwSwUefS3Sg&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTNWmj3G1El9VvSp-muGnch-XOIXykwLaZa4w&s"
        }),
        
        // Cleansing Gel Face Wash
        ("Cleansing Gel Face Wash", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQkaX2LqQ8a7zqysEzX98U1KhUdN6kjc6i22Q&s", 
            "https://images.squarespace-cdn.com/content/v1/5badc17c797f743dc830bb95/1720164751711-2KJJ7A6MK6WH95C7JGG9/HydroPeptide+Cleansing+Gel+Perth.png?format=1000w", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ9BYzA_QpTeT5wLVuqIQEQ0x6fMYRUEEdbzTqiVagtp3fB2bZ44hGnXnVWD7bfsUBXpGE&usqp=CAU"
        }),
        
        // Mangosteen Revitalizing Mist
        ("Mangosteen Revitalizing Mist", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRhqpsm4vwoEDeWoXmYThy0w7GBLITExzxFkg&s", 
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-slide/eminence-organics-mangosteen-revitalizing_mist_swatch-400x400px-compressed.png?itok=9ia6NZD4", 
            "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Mangosteen-Revitalizing-Mist-Lifestyle.jpg?v=1711740433&width=1080"
        }),

        // Pineapple Refining Tonique
        ("Pineapple Refining Tonique", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSipnwI-Quyzdj8X2z555QL6Nv8dXGypcpFUw&s", 
            "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Pineapple-Refining-Tonique-lifestyle.jpg?v=1711743675&width=1080", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRXvTAya614P0QjS3xR5k9bA9QyUs8DLnn72Q&s"
        }),
        
        // Hawthorn Tonique
        ("Hawthorn Tonique", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQWgXY3ygvCD1JS1ZJM_UQB0DlglHmITznI7A&s", 
            "https://images.squarespace-cdn.com/content/v1/5ea87ac4bf0b761180ffcfae/1626276012659-MUTJNN1LVDNA49X09L55/Hawthorn+Tonique.jpg?format=1000w", 
        }),
        
        // Lime Refresh Tonique
        ("Lime Refresh Tonique", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-lime-refresh-tonique-400x400px.png?itok=75tgznFH", 
            "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Lime-Refresh-Tonique-Lifestyle.png?v=1711744338&width=1080", 
            "https://eminenceorganics.com/sites/default/files/article-image/eminence-organics-tonique.jpg"
        }),
        
        //Soothing Chamomile Tonique
        ("Soothing Chamomile Tonique", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR0OK4AjASttojjEQbjJa9vTR7cy-Vf1UcAag&s", 
            "https://blog.skin-beauty.com/wp-content/uploads/2020/08/soothing-chamomile-tonique__64126.1586549554.1280.1280.jpg", 
        }),
        
        // Multi-Acne Toner 
        ("Multi-Acne Toner", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/2-3.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQOyk2bFufKdc__jGiQJnFZxau1_kg7OmQUpg&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQK3uQghHwcl19vgzc-ZLG9e4hDV_aFbE4nwwwES3x6LrdmIl2zlBrhTBgg5lGrH6ST_5U&usqp=CAU"
        }),
        
        // Antioxidant Hydramist
        ("Antioxidant Hydramist", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/2-117.jpg", 
            "https://www.dermalogica.co.uk/cdn/shop/products/Antioxidant-Hydramist-pdp-2.jpg?v=1721383738&width=1946", 
            "https://bellelab.co/wp-content/uploads/2019/12/Dermalogica_Antioxidant_Hydramist_150ml_3.jpg"
        }),
        
        // UltraCalming Mist
        ("UltraCalming Mist", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2020/05/2.jpg", 
            "https://myvienhana.vn/wp-content/uploads/2022/03/xit-khoang-dermalogica-Ultracalming-Mist.jpg", 
            "https://116805005.cdn6.editmysite.com/uploads/1/1/6/8/116805005/s595587978454154049_p56_i2_w1080.jpeg"
        }),
        
        // Hyaluronic Ceramide Mist
        ("Hyaluronic Ceramide Mist", new List<string>
        {
            "https://www.dermalogica.com/cdn/shop/files/hyaluronic-ceramide-mist_front.jpg?v=1698103421&width=1946", 
            "https://sieuthilamdep.com/images/detailed/19/xit-khoang-cap-am-va-lam-diu-da-dermalogica-hyaluronic-ceramide-mist-2.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQV0eQsaz5GoUd5mI9LsEXP5mb0IEO7zlvz3A&s"
        }),
        
        // Remedy Toner 
        ("Remedy Toner", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR5fMbd6K1rQjSbxMXbFo36ZZHjthpqr4lpDQ&s", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/Remedy-1-02-1091x1200.png", 
            "https://vn-test-11.slatic.net/p/5fe09d9ffdf46df663e3dfcd64fcf4c5.jpg"
        }),
        
        // Essential Toner
        ("Essential Toner", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/Essential-Toner_San-pham-1091x1200.png", 
            "https://hadibeauty.com/wp-content/uploads/2023/03/334204861_942692140091925_7678139925329751463_n.webp", 
        }),
        
        // Active Pureness Toner
        ("Active Pureness Toner", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/Active-Pureness-Toner_San-pham-1091x1200.png", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/cdd49c856665c35cd1ca025ee14c26d5429d3d2c_2000x-1091x1200.jpg", 
        }),

        // Revitalizing Tonic
        ("Revitalizing Tonic", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTkXA3c63LygDEehGwwAJm2Y_SxgWwEt1vz8w&s", 
            "https://comfortzone.com.vn/wp-content/uploads/2023/01/8004608516330_4-1200x1200.jpg", 
            "https://cf.shopee.vn/file/vn-11134201-23030-p5amrfvba8nv3a"
        }),
        // Acwell Licorice pH Balancing Cleansing Toner
        ("Acwell Licorice pH Balancing Cleansing Toner", new List<string>
        {
            "https://sokoglam.com/cdn/shop/files/SokoGlamPDP_Acwell_Revamped_Licorice_pH_Balancing_Cleansing_Toner-4_860x.png?v=1729736947", 
            "https://www.mikaela-beauty.com/cdn/shop/files/AX6H2549w_1200x1200.jpg?v=1720903305", 
        }),
    
        // COSRX AHA/BHA Clarifying Treatment Toner
        ("COSRX AHA/BHA Clarifying Treatment Toner", new List<string>
        {
            "https://product.hstatic.net/1000006063/product/cosrx_ahabha_clarifying_treatment_toner_150ml_625a49f8074c41c59c9d185e582f0580_1024x1024.jpg", 
            "https://assets.aemi.vn/webp/CRX_TNR_150ml_001_img2.webp", 
        }),
        // Sulwhasoo Concentrated Ginseng Renewing Water
        ("Sulwhasoo Concentrated Ginseng Renewing Water", new List<string>
        {
            "https://cdn.shopify.com/s/files/1/0667/9416/0378/files/concentrated_ginseng_rejuvenating_water_kv_pc_vn_240819.jpg?v=1724121219", 
            "https://th.sulwhasoo.com/cdn/shop/files/TN-CGR-Water-1.jpg?v=1734325148", 
        }),
        // Pre-Treatment Toner
        ("Pre-Treatment Toner", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/011024_Pre-TreatmentToner_PDP_1024x1024.jpg?v=1711563597", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT0ITo2GuTRZWHrbdTatJFRzQcwsN5jDNfHOw&s", 
            "https://134449990.cdn6.editmysite.com/uploads/1/3/4/4/134449990/s148490656276618525_p144_i1_w550.jpeg"
        }),
        // Hydraflora
        ("Hydraflora", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_HydraFlora_PDP_eea6942c-bdcc-47a2-adf3-c2632906ffc3_grande.jpg?v=1730921087", 
            "https://hydropeptide.com/cdn/shop/files/020724_Swatch_Hydraflora_PDP_0f209cc8-b2dd-42ca-bd9d-06a83eb0d376_1024x1024.jpg?v=1730921090", 
        }),
        // Clarifying Toner Pads
        ("Clarifying Toner Pads", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_ClarifyingToner_PDP_grande.jpg?v=1715974145", 
            "https://metafields-manager-by-hulkapps.s3-accelerate.amazonaws.com/uploads/hydropeptide-canada.myshopify.com/1718388492-022624_ClarifyingToner_BENEFITS.jpg", 
        }),
        // Toner Vichy Aqualia Thermal Hydrating Refreshing Water
        ("Toner Vichy Aqualia Thermal Hydrating Refreshing Water", new List<string>
        {
            "https://trungsoncare.com/images/detailed/10/1_n7ix-l0.png", 
            "https://ordinaryvietnam.net/wp-content/uploads/2022/03/Nuoc-hoa-hong-Vichy-Aqualia-Thermal-Hydrating-Refreshing-Water-Ordinary-Viet-Nam-3-600x600.jpg", 
        }),
        // Toner Vichy Normaderm acne-prone skin purifying pore-tightening lotion
        ("Toner Vichy Normaderm acne-prone skin purifying pore-tightening lotion", new List<string>
        {
            "https://storage.beautyfulls.com/uploads-1/thanhhuong/2022/vichy/toner/vichy-normaderm-purifying-pore-tightening/nuoc-hoa-hong-vichy.jpg", 
            "https://escentual.com/cdn/shop/files/vichy_normaderm_purifying_pore-tightening_toning_lotion_200ml_2.png?v=1729191893", 
        }),
        // Sublime Skin Intensive Serum
        ("Sublime Skin Intensive Serum", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2023/05/sublime-skin-07-1-1091x1200.png", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5pQrlNdExrdzyuV56Zv4vX2yGLGl1VR6juQ&s", 
        }),
        // Hydramemory Hydra & Glow Ampoules
        ("Hydramemory Hydra & Glow Ampoules", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2023/04/8004608510871%E2%80%8B_2-1091x1200.jpg", 
            "https://vonpreen.com/wp-content/uploads/2023/02/duong-da-Comfort-Zone-Hyramemory-Hydra-Glow-Ampoule-7-ong-x-2ml.jpg", 
        }),
        // Subline Skin Lift & Firm Ampoule
        ("Subline Skin Lift & Firm Ampoule", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/ampollesublime_2000x-1091x1200.jpg", 
            "https://livelovespa.com/cdn/shop/products/Untitleddesign_33_626c334a-5cfe-4a98-9fd6-f1c4faf46476.png?v=1650075373&width=2048", 
        }),
        // Biolumin-C Serum
        ("Biolumin-C Serum", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2020/05/2-1.jpg", 
            "https://stralabeauty.com/wp-content/uploads/2022/05/111341-dermalogica-biolumin-c-serum-open.jpg", 
        }),
        // Age Bright Clearing Serum
        ("Age Bright Clearing Serum", new List<string>
        {
            "https://www.facethefuture.co.uk/cdn/shop/files/111342-lifestyle-1_1750x1750_7f55ce82-2824-4c25-aa9b-e14e9f84f1d2.jpg?v=1695286223&width=600", 
            "https://dermalogica.com.vn/cdn/shop/products/facial-oils-and-serums-facial-oils-and-serums-30ml-age-bright-clearing-serum-30198061039821.png?v=1718765966&width=1445", 
            "https://veevee.store/wp-content/uploads/2023/10/dermalogica-age-bright-clearing-serum-2.webp"
        }),
        // Powerbright Dark Spot Serum
        ("Powerbright Dark Spot Serum", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/dermalogica-vietnam-powerbright-dark-spot-serum-29677731414221_707x707.jpg", 
            "https://edbeauty.vn/wp-content/uploads/2023/08/Tinh-chat-duong-sang-da-Dermalogica-Powerbright-Dark-Spot-Serum-2.jpg", 
            "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/power-serum-2.jpg"
        }),
        // UltraCalming Serum Concentrate
        ("UltraCalming Serum Concentrate", new List<string>
        {
            "https://cdn.dangcapphaidep.vn/wp-content/uploads/2018/06/Dermalogica-Ultracalming%E2%84%A2-Serum-Concentrate-1.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSk6cqAfkxjQz5Mbmi6kkvBSj-ktU8a-Ufmng&s", 
        }),
        // Circular Hydration Serum With Hyaluronic Acid
        ("Circular Hydration Serum With Hyaluronic Acid", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2024/03/Huyet-Thanh-Cap-Am-Chuyen-Sau-Circular-Hydration-Serum-30ml.jpg", 
            "https://www.depmoingay.net.vn/wp-content/uploads/2023/08/Tinh-chat-cap-am-chuyen-sau-Dermalogica-Circular-Hydration-Serum.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQNmRYaTc9DUWC6Db28yQs8xl-cPbyLqsHfUQ&s"
        }),
        // Strawberry Rhubarb Hyaluronic Serum
        ("Strawberry Rhubarb Hyaluronic Serum", new List<string>
        {
            "https://beautyritual.ca/cdn/shop/products/eminence-organics-strawberry-rhubarb-hyaluronic-serum-swatch_6209ac7f-cf39-4122-b2a3-98a2d6c150ae.jpg?v=1722965085&width=480", 
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-strawberry-rhubarb-hyaluronic-serum.jpg?itok=MnXS_0td", 
        }),
        // Citrus & Kale Potent C+E Serum
        ("Citrus & Kale Potent C+E Serum", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-citrus-kale-potent-ce-serum-400x400px_0.png?itok=2SnWNB_z", 
            "https://store-cdn-media.dermpro.com/catalog/product/cache/10f519365b01716ddb90abc57de5a837/e/m/eminence_citrus_kale_potent_c_e_serum_2.jpg", 
            "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Citrus-Kale-Potent-C-E-Serum-Lifestyle.png?v=1711744368&width=1080"
        }),
        // Marine Flower Peptide Serum
        ("Marine Flower Peptide Serum", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQeNfYFOyzx2eItOl_4C-VnSTtOqdEH8_Bp9w&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSYaWbbx2Sz426MAirXQgrJ9YqBH75dQxB2gw&s", 
            "https://images.squarespace-cdn.com/content/v1/5b3ca5a2b98a782f09c815d6/1669510664302-2M38DWMSGWL2H9N0GR6I/Fall-2022-Social-Media-Package-2-1080x1080.jpg"
        }),
        // Clear Skin Willow Bark Booster-Serum
        ("Clear Skin Willow Bark Booster-Serum", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQN_Wauo_Rlilox29NSTEBgTFF9vxbY62PLZg&s", 
            "https://the-afternoon-prod.s3.ap-southeast-1.amazonaws.com/product/02644f3a-9594-42b3-b857-6a1753db3a84/block/k-t-602-copy1-min.jpg", 
        }),
        // Cornflower Recovery Serum
        ("Cornflower Recovery Serum", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-cornflower-recovery-serum-sq.jpg?itok=nIX5IZVH", 
            "https://www.dermstore.com/images?url=https://static.thcdn.com/productimg/original/11857248-1344866735395349.jpg&format=webp&auto=avif&width=1200&height=1200&fit=cover", 
        }),
        // Power Serum
        ("Power Serum", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQV7CdjpeMqLmybea_wuyU1YkJaN9H56D83bg&s", 
            "https://images.squarespace-cdn.com/content/v1/5badc17c797f743dc830bb95/1721113519111-GS0XBO7FD4FEU9TPD5QZ/HydroPeptide+Power+Serum+.png?format=1000w", 
            "https://img-cdn.heureka.group/v1/975eff79-baec-4106-87d5-641a98f2bc61.jpg?width=350&height=350"
        }),
        // Firma-Bright
        ("Firma-Bright", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR6XcpSDG4i6iLlhr1d8tah8jg9YvwaeHAJqg&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTxCwXA3JgUVHri_LMGzNtf2Xrkig-zmb6AvA&s", 
            "https://metafields-manager-by-hulkapps.s3-accelerate.amazonaws.com/uploads/hydropeptide-canada.myshopify.com/1726623141-082224_New_Retail_Packaging_Firma-Bright_BENEFITS.jpg"
        }),
        // Hydrostem Serum
        ("Hydrostem Serum", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSeehTZ70DIUnNzpsOuSC8V0BBSMtsJqSSa2w&s", 
            "https://cdn.shopify.com/s/files/1/0345/0444/1995/files/PDP_Hydrostem_Results.jpg?v=1660700668", 
        }),
        // Minéral 89 Booster
        ("Minéral 89 Booster", new List<string>
        {
            "https://product.hstatic.net/200000124701/product/ml_mb033500_serum_giup_sang_da_va_cang_muot_5309_609c_large_4eaece6dec_040dcc25119b4ba48c4f84eb5ea8eab8_master.jpg", 
            "https://product.hstatic.net/1000006063/product/2_9c06e1144bc44c31976a4726501e6936_1024x1024.jpg", 
            "https://assets-hebela.cdn.vccloud.vn/dict/1/rigigstrtssgtssmsm20221024161114mineral-89-fortifying-daily-booster-30ml/rrosatidinngitiins20221024161532image2.png"
        }),
        // Minéral 89 Probiotic Fractions
        ("Minéral 89 Probiotic Fractions", new List<string>
        {
            "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/skincare/m89/mineral89-probiotic-fractions-pack3.jpg?rev=8745dcf5b4fb41dbbf921950bebe6ae3&cx=0.53&cy=0.55&cw=525&ch=596&hash=3CB4E2F0FB27C99F34CEF2B4B60F94C5", 
            "https://product.hstatic.net/1000006063/product/2_9c06e1144bc44c31976a4726501e6936_1024x1024.jpg", 
            "https://product.hstatic.net/200000617989/product/tinh_chat_vichy_mineral_89_probiotic_fractions___2__fdb740bf564c4b6eb8ce809b420b1d4c.jpg"
        }),
        // Barrier Builder
        ("Barrier Builder", new List<string>
        {
            "https://hydropeptide.co.uk/cdn/shop/files/Barrier-Builder.jpg?v=1725351085", 
            "https://cdn.shopify.com/s/files/1/0345/0444/1995/files/how-to.jpg?v=1725358910", 
        }),
        // Power Luxe
        ("Power Luxe", new List<string>
        {
            "https://skinbeautifulrx.com/cdn/shop/products/PowerLuxe_1200x.jpg?v=1629145994", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSc1zwGe1F7e0YfxPgDTe8hUHB8978RAybE_Q&s", 
            "https://abeautybar.com.ua/content/images/35/1080x1071l80mc0/gidropitatelnyy-infuzionnyy-krem-hydropeptide-power-luxe4784-36045340977099.jpeg"
        }),
        // AquaBoost Oil Free Face Moisturizer
        ("AquaBoost Oil Free Face Moisturizer", new List<string>
        {
            "https://cdn.cosmostore.org/cache/front/shop/products/511/1558828/650x650.jpg", 
            "https://images.squarespace-cdn.com/content/v1/64930aaaf0c0fc7ee1e1ffeb/1713368640714-4KTAPH5GBPSDEBBFLW8P/Hydropeptide+Aquaboost+example.png?format=1500w", 
        }),
        // Face Lift Moisturizer
        ("Face Lift Moisturizer", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/010924_New_Retail_FaceLift_PDP_grande.jpg?v=1711563449", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR999M9m99s-z5kSEiW-HqQchDa2YY783A04w&s", 
        }),
        // Sublime Skin Fluid Cream
        ("Sublime Skin Fluid Cream", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2023/03/12201_1_SUBLIME_SKIN_Fluid_Cream_60ml_Comfort_Zone_2000x-1091x1200.webp", 
            "https://comfortzone.com.vn/wp-content/uploads/2023/03/sublime-skin-texture-fluid-cream.png", 
        }),
        // Sacred Nature Nutrient Cream
        ("Sacred Nature Nutrient Cream", new List<string>
        {
            "https://world.comfortzoneskin.com/cdn/shop/files/o3rmembdmxcpkfgj5rau_1600x.jpg?v=1718130042", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/6939f8f00a8a7ed77a04d50b9549311e267a48fe_2000x-1091x1200.jpg", 
        }),
        // Active Pureness Fluid
        ("Active Pureness Fluid", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2023/11/San-pham-134-1091x1200.jpg", 
            "https://www.organicpavilion.com/cdn/shop/products/PF8aa29d2d702ea16e288b6fbaad4bf3bd52092193_2000x_6c1ce0b0-1fa2-4ae4-98bc-fc537be81713_large.png?v=1677234394", 
        }),
        // Remedy Cream 
        ("Remedy Cream", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/Remedy-1-03-1091x1200.png", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/c9d0b89d8a9af00e2eb8909d8c768e15158b6d7e_2000x-1091x1200.jpg", 
        }),
        // Strawberry Rhubarb Hyaluronic Hydrator
        ("Strawberry Rhubarb Hyaluronic Hydrator", new List<string>
        {
            "https://www.everyoneblooms.com/wp-content/uploads/2024/07/Eminence-Organics-Strawberry-Rhubarb-Hyaluronic-Hydrator-SQ.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTdwtvJgu9013N_fBXR36JJrEmupTNpKqo17Q&s", 
        }),
        // Bakuchiol + Niacinamide Moisturizer
        ("Bakuchiol + Niacinamide Moisturizer", new List<string>
        {
            "https://pbs.twimg.com/media/GEFcv4YbQAAP1OE.jpg:large", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ2AJMC8ZAAzSqA09cJezP4jN79hpxNvKS56A&s", 
            "https://www.newbeauty.com/wp-content/uploads/2023/10/2023-Fall-Bakuchiol-And-Niacinamide-Collection-Lifestyle-High-38-1-scaled.jpg"
        }),
        // Acne Advanced Clarifying Hydrator
        ("Acne Advanced Clarifying Hydrator", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-acne-advanced-clarifying-hydrator-v2-400pix-compressor.jpg?itok=_ELq7sKM", 
            "https://buynaturalskincare.com/cdn/shop/products/Eminence-Organics-Acne-Advanced-Clarifying-Hydrator-Swatch.jpg?v=1665160459&width=1080", 
        }),
        // Echinacea Recovery Cream
        ("Echinacea Recovery Cream", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-echinacea-recovery-cream-400x400px.png?itok=LNRyYSeA", 
            "https://cdn.cosmostore.org/cache/front/shop/products/127/302585/650x650.jpg", 
        }),
        // PowerBright Overnight Cream
        ("PowerBright Overnight Cream", new List<string>
        {
            "https://edbeauty.vn/wp-content/uploads/2023/08/Kem-duong-Powerbright-Overnight-Cream-1.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR7Ul9pN39KXGUnc4ho3jvwm2hrKpmBZdKy-g&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS98UOJj0PdVmux2QYM31SSmllUTZkidUc9lA&s"
        }),
        // Skin Soothing Hydrating Lotion
        ("Skin Soothing Hydrating Lotion", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT1JQViTcVpNPKqP6Wn2e99hYZrczacbtnGtA&s", 
            "https://absoluteskin.com.au/cdn/shop/products/dermalogica-moisturisers-dermalogica-clear-start-skin-soothing-hydrating-lotion-59ml-28824281022558_300x.jpg?v=1711588930", 
            "https://www.dermalogica.ca/cdn/shop/products/3soothinglotionhands-min_2048x2048_9f7cd589-f542-4efb-9228-6d0fb802c954.webp?v=1660681593&width=1946"
        }),
        // Skin Smoothing Cream
        ("Skin Smoothing Cream", new List<string>
        {
            "https://hoaanhdao.vn/media/sanpham/1530205200/KEM_D%C6%AF%E1%BB%A0NG_%E1%BA%A8M_L%C3%80M_M%E1%BB%8AN_DA_DERMALOGICA_SKIN_SMOOTHING_CREAM.jpg", 
            "https://www.facethefuture.co.uk/cdn/shop/files/skin-smoothing-cream-pdp-3.jpg?v=1692712240&width=600", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRj4i7SJV0CSo8uab92n_4olFmOhtRlXD9DJg&s"
        }),
        // Barrier Repair
        ("Barrier Repair", new List<string>
        {
            "https://dangcapphaidep.vn/image-data/780-780/cdn/2021/11/Dermalogica-Barrier-Repair-1.jpg", 
            "https://veevee.store/wp-content/uploads/2023/10/dermalogica-barrier-repair-2.webp", 
        }),
        // Minéral 89 72H Moisture Boosting Fragrance Free Cream
        ("Minéral 89 72H Moisture Boosting Fragrance Free Cream", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTeDcDHtRmZHif3n-fLkzxuYnQIGuuaVoDVFg&s", 
            "https://media.superdrug.com//medias/sys_master/prd-images/h77/h30/10011961950238/prd-back-826896_600x600/prd-back-826896-600x600.jpg", 
        }),
        // Liftactiv B3 Tone Correcting Night Cream With Pure Retinol
        ("Liftactiv B3 Tone Correcting Night Cream With Pure Retinol", new List<string>
        {
            "https://www.vichy.ca/dw/image/v2/AATL_PRD/on/demandware.static/-/Sites-vichy-master-catalog/default/dwe39c018d/product/2024/3337875873086/3337875873086.mainA-EN.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQHvHgPHulcbHZbcvkjI6m1cmQsZFNyiX9Aaw&s", 
        }),
        // Belif The True Cream Aqua Bomb
        ("Belif The True Cream Aqua Bomb", new List<string>
        {
            "https://image.hsv-tech.io/1987x0/tfs/common/d221fe39-305d-492d-b089-44607a9285fc.webp", 
            "https://image.hsv-tech.io/1920x0/bbx/common/b6851887-fbe5-46a8-b189-e9be2b2d5556.webp", 
        }),
        // Oat So Simple Water Cream
        ("Oat So Simple Water Cream", new List<string>
        {
            "https://kravebeauty.com/cdn/shop/files/9.24_OSS_PDP3.png?v=1727744093&width=1200", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRw5DZUUIS5TX9dU3_h8FD70ChLCeiuZsuTIiniSZZ9udxobZfPTA7A46-KPczF9YiCGr8&usqp=CAU", 
        }),
        // AESTURA A-CICA 365 Calming Cream
        ("AESTURA A-CICA 365 Calming Cream", new List<string>
        {
            "https://down-vn.img.susercontent.com/file/sg-11134207-7rbka-ln1cvnbe1d8532", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ5-Iis6x4JWFAUiCEywTriRNgD9JBhu0P_9g&s", 
        }),
        // Sun Soul Face Cream SPF30
        ("Sun Soul Face Cream SPF30", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2023/03/8004608515975_1-1091x1200.jpg", 
            "https://shop.beautymanufactur.de/wp-content/uploads/2024/05/com-12163-comfort-zone-sun-soul-face-cream-spf30-texture-900x900.jpg.webp", 
        }),
        // Skin Regimen Urban Shield SPF30
        ("Skin Regimen Urban Shield SPF30", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/8004698186420_1-1091x1200.jpg", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/neni-urban-shield-spf-30-2-800x1200.jpg", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/apply-urban-shield-spf-30-800x1200.jpg"
        }),
        // PoreScreen SPF40
        ("PoreScreen SPF40", new List<string>
        {
            "https://dr-skincare.com/wp-content/uploads/2024/03/67.jpg", 
            "https://cdn11.bigcommerce.com/s-6c800/images/stencil/1280x1280/products/8001/28227/Dermalogica-Porescreen-Mineral-Sunscreen-SPF-40-DM111473_26769__87674.1729965251.jpg?c=2", 
            "https://dermalogica.in/cdn/shop/files/PoreScreen-09_6b178e28-1089-4645-88df-24015551d76a.jpg?v=1724661984&width=1445"
        }),
        // Invisible Physical Defense SPF30 
        ("Invisible Physical Defense SPF30", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2020/05/7-3-590x600.jpg", 
            "https://m.media-amazon.com/images/I/51HC+QLIrVL._AC_UF350,350_QL80_.jpg", 
        }),
        // Protection 50 Sport SPF50
        ("Protection 50 Sport SPF50", new List<string>
        {
            "https://edbeauty.vn/wp-content/uploads/2019/12/kem-chong-nang-Dermalogica-protection-sport-spf-50-156ml.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR9OSPWVkYd5yhmaP6lPYcO1RPlqwK5FYaU-Q&s", 
        }),
        // Oil Free Matte SPF30
        ("Oil Free Matte SPF30", new List<string>
        {
            "https://www.dermalogica.ie/cdn/shop/files/oil-free-matte-pdp_abddc3c6-01ee-4a08-9a22-3d93d66cc79e.jpg?v=1687444268", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQa4gMLYU2BQkxgljz_dQteQ7buLxR-1Q39yQ&s", 
        }),
        // Radiant Protection SPF Fluid
        ("Radiant Protection SPF Fluid", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQSIeOviukKjzkEkUyMUzgkB5Up-8o7XucjLw&s", 
            "https://naturalbeautygroup.com/cdn/shop/files/eminence-organics-Radiant-Protection-SPF-Fluid-Swatch.png?v=1708611969&width=1080", 
        }),
        // Lilikoi Daily Defense Moisturizer SPF 40
        ("Lilikoi Daily Defense Moisturizer SPF 40", new List<string>
        {
            "https://cdn.cosmostore.org/cache/front/shop/products/486/1464606/350x350.jpg", 
            "https://images.squarespace-cdn.com/content/v1/63d45207da7f2a5e82696fe2/1674859081420-WZPSXPO81QEKEFHHKB4Y/eminence-organics-lilikoi-moisturizer-spf40-swatch-circle-400x400.jpg?format=1000w", 
        }),
        // Lilikoi Mineral Defense Sport Sunscreen SPF 30
        ("Lilikoi Mineral Defense Sport Sunscreen SPF 30", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-llilikoi-mineral-defense-sport-sunscreen-spf-30-400x400.jpg?itok=0IXRhb-K", 
            "https://images.squarespace-cdn.com/content/v1/5d75abb04593a56ccb9161cb/1572814643401-Q0O24YSFHA4JBI09B5T5/eminence-organics-stone-crop-revitalizing-body-scrub-swatch-toscana.png?format=1000w", 
        }),
        // Daily Defense Tinted SPF
        ("Daily Defense Tinted SPF", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-daily-defense-tinted-spf-pdp.jpg?itok=DphokG8j", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS8Nl7TW6hlAMdQ9UBiRaTrH0dHNp476Ye3wQ&s", 
            "https://shop.vivadayspa.com/cdn/shop/files/EminenceOrganicDailyDefenseTintedSPF50_3_1800x1800.png?v=1719944530"
        }),
        // Solar Dew Sheer Mineral Melt SPF 30
        ("Solar Dew Sheer Mineral Melt SPF 30", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQOz514kGYGfzEyqZ4ItSevNqGTw4KzgJgkrQ&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAV2C0GAlWYxHpgCIWrRSvXA6JFAP0hbeHaA&s", 
            "https://www.purplebeautysupplies.com/cdn/shop/files/hydropeptide-solar-dew-spf-30-mineral-serum-135-oz-40-ml-478701.jpg?v=1715239028&width=1445"
        }),
        // Solar Defense Non-Tinted Sunscreen
        ("Solar Defense Non-Tinted Sunscreen", new List<string>
        {
            "https://bizweb.dktcdn.net/thumb/1024x1024/100/318/244/products/untitled-bd9e4f55-c942-4778-8da9-70005304d193.jpg?v=1607671087160", 
            "https://metafields-manager-by-hulkapps.s3-accelerate.amazonaws.com/uploads/hydropeptide-canada.myshopify.com/1726621522-022624_SolarDefenseNonTinted_BENEFITS.jpg", 
        }),
        // Solar Defense Tinted SPF 30
        ("Solar Defense Tinted SPF 30", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_SolarDefenseTinted_PDP_1024x1024.jpg?v=1724437418", 
            "https://hydropeptide.co.uk/cdn/shop/products/HP--PDP-CaroselImages-Solar-Defense-Tinted-video.jpg?v=1662163522", 
        }),
        // Vichy Capital Soleil UV Age Daily SPF50 PA++++ 
        ("Vichy Capital Soleil UV Age Daily SPF50 PA++++", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSspHWslG4DZpyDyDQNx2owKFNPb1_v0zeDFA&s", 
            "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/suncare/capital-soleil---uv-age/capital-soleil-uv-age-spf50-pack4.jpg?rev=02d0faa4daf344b7ae245ea718addbaa&cx=0.47&cy=0.53&cw=525&ch=596&hash=CFA022DF922328E3F8ABA2264ED86940", 
        }),
        // Capital Soleil Ultra Light Face Sunscreen SPF 50
        ("Capital Soleil Ultra Light Face Sunscreen SPF 50", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS9WRnfVxm2mABipRz9LHO1eBdH1oDlU6wC7A&s", 
            "https://exclusivebeautyclub.com/cdn/shop/products/vichy-capital-soleil-ultra-light-sunscreen-spf-50-vichy-shop-at-exclusive-beauty-club-515732.jpg?v=1698096439", 
        }),
        // Neogen Day-Light Protection Airy Sunscreen
        ("Neogen Day-Light Protection Airy Sunscreen", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTk5ssRRdBHGrHUnF87gc2M30_2EJcxarie1g&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTsD2FE2MUfeFeZlFXuvJgidm1iU8WgkkVU7Q&s", 
        }),
        // Round Lab Birch Juice Moisturizing Sunscreen
        ("Round Lab Birch Juice Moisturizing Sunscreen", new List<string>
        {
            "https://product.hstatic.net/200000150709/product/1_10a2d9a626da4c03ac81c93497bae020.png", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSiew88Sg_ffo50u6QcX4kuVHoG_fK9u3qtkA&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTKdt5IbBt21U-Ppg4c-ic4PawxrqvJUgAgtg&s"
        }),
        // Beet The Sun SPF 40 PA+++
        ("Beet The Sun SPF 40 PA+++", new List<string>
        {
            "https://www.dodoskin.com/cdn/shop/files/61J0umsAoGL_080fdcb3-37f3-4066-a1b9-8769b6fc1376_2048x2048.jpg?v=1707875322", 
            "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lpp4hgn0mhxw87", 
            "https://kravebeauty.com/cdn/shop/files/9.24_BTS_PDP3.png?v=1729803947&width=1200"
        }),
        // Klairs All-day Airy Mineral Sunscreen SPF50+ PA++++
        ("Klairs All-day Airy Mineral Sunscreen SPF50+ PA++++", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRxNHcQ2vQJhLia5CAIKeC0YLPPKi_KtyfcIg&s", 
            "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lz7ct301ck9t85", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSfMgba99CrRc61gzwD8wPnD1H7X-QlaB_tFw&s"
        }),
        // Goongbe Waterful Sun Lotion Mild SPF50+ PA++++
        ("Goongbe Waterful Sun Lotion Mild SPF50+ PA++++", new List<string>
        {
            "https://gomimall.vn/cdn/shop/files/6_55cd82b9-b3ba-438d-a9e3-d105ff8d9166.png?v=1727068956", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT_VhLOsDvcPtXOWARG0zxac-oDZExusVtWnw&s", 
            "https://www.ballagrio.com/cdn/shop/files/3_f0fb1c1d-ee7a-4762-a261-79a4e841791b_2048x.jpg?v=1734515800"
        }),
        // Eight Greens Phyto Masque – Hot
        ("Eight Greens Phyto Masque – Hot", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-eight-greens-phyto-masque-hot-pdp.jpg?itok=-pxuW5H5", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTQtndM3VeufQiiNA59YlFA--hDSXxxB9qMow&s", 
            "https://organicskincare.com/wp-content/uploads/2015/08/Eminence-Eight-Greens-Phyto-Masque-360x360.jpg"
        }),
        // Kombucha Microbiome Leave-On Masque
        ("Kombucha Microbiome Leave-On Masque", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-kombucha-mircobiome-leave-on-masque-pdp.jpg?itok=m8OxvyO3", 
            "https://beautyritual.ca/cdn/shop/products/eminence-organics-kombucha-microbiome-leave-on-masque-swatch-pdp.jpg?v=1722521123&width=480", 
            "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Kombucha-Microbiome-Leave-On-Masque-lifestyle.jpg?v=1717443755&width=1080"
        }),
        // Citrus & Kale Potent C+E Masque
        ("Citrus & Kale Potent C+E Masque", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/content/blog/product-picks/eminence-organics-citrus-kale-potent-ce-masque.png", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRpPxaCBWNH4MBUIBigH9RkoB_9ExSPGYKnzw&s", 
            "https://i0.wp.com/jessicasapothecary.com/wp-content/uploads/2020/04/vitaminc_masque.jpg?resize=840%2C1050&ssl=1"
        }),
        // Stone Crop Masque
        ("Stone Crop Masque", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSZoviYz-lQL1doxfPgl9PLZEtC_e6oKEhwnA&s", 
            "https://www.shophalosaltskinspa.com/cdn/shop/products/CAEB78BD-1992-4EEB-B4C3-C4CF73CFE794.jpg?v=1662576743&width=1445", 
            "https://oresta.ca/cdn/shop/products/eminence-organics-eminence-stone-crop-masque-803682.jpg?v=1706991974&width=2000"
        }),
        // Calm Skin Arnica Masque
        ("Calm Skin Arnica Masque", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTJwWf92eq1uUCwgZTan7qEZlq-TgQfTXFOZA&s", 
            "https://vanislebeautyco.com/cdn/shop/products/image_61ac7e19-35ab-4d54-979d-5ce1d8e44d56.jpg?v=1672155911&width=720", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTImXEm-NSJyUUmbxpk5EcSniUif5Q09WVDFw&s"
        }),
        // Multivitamin Power Recovery Masque
        ("Multivitamin Power Recovery Masque", new List<string>
        {
            "https://sieuthilamdep.com/images/detailed/15/mat-na-vitamin-chong-lao-hoa-dermalogica-multivitamin-power-recovery-masque.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRn48kip8eDY41wOLGcEU7hx4NvHyqrpzwPpA&s", 
            "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lowl5l8fp4631a"
        }),
        // Sebum Clearing Masque
        ("Sebum Clearing Masque", new List<string>
        {
            "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/30/images/dermalogica-sebum-clearing-masque-75ml%281%29.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQI_EgDwBMh0LKVO-pyPoV9JSyBhIAbVJLDSg&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRwazAQaO2dRExfe9aDgIYK2dlsywUuOlLByA&s"
        }),
        // Melting Moisture Masque
        ("Melting Moisture Masque", new List<string>
        {
            "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/30/images/dermalogica-melting-moisture-masque.jpg", 
            "https://edbeauty.vn/wp-content/uploads/2023/08/Mat-na-cap-am-Dercalogica-Melting-Moisture-Masque-1.jpg", 
            "https://www.depmoingay.net.vn/wp-content/uploads/2023/08/Mat-na-cap-am-chuyen-sau-Dercalogica-Melting-Moisture-Masque-1.jpg"
        }),
        // Miracle Mask
        ("Miracle Mask", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_MiracleMask_PDP_1024x1024.jpg?v=1715974173", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTw_gxtCWIU4_0n5EORj1Kl9XmFLdUwXkOVeg&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSwt8mTBHwwlIOOzAMW9avZFEcDHdAb96msRw&s"
        }),
        // Hydro-Lock Sleep Mask
        ("Hydro-Lock Sleep Mask", new List<string>
        {
            "https://hydropeptide.com.es/image/cache/catalog/Products/hydro-lock-sleep-mask/012224_New_Retail_Packaging_Hydro-LockSleepMask_PDP_950x_2x.progressive-1000x1000.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTOC_f_73gSfPV_aCpydJNdeAV0aJ18mz-VVg&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQRVLHWy0Ee-uIrQCXBvxrGXgF4b7X69GKfOg&s"
        }),
        // PolyPeptide Collagel+ Mask 
        ("PolyPeptide Collagel+ Mask", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/011124_PolypeptideCollagel_Face_PDP_1024x1024.jpg?v=1705108115", 
            "https://m.media-amazon.com/images/I/61kfwzxp9sL._AC_UF1000,1000_QL80_.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRpqGAgS_WZV8gv_OroncyYwbcWdujtINeEHg&s"
        }),
        // Balancing Mask
        ("Balancing Mask", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQfYIUlZvjKdCtWBk5N0Hjzr5rWg0f7c8NBdw&s", 
            "https://hydropeptide.asia/wp-content/uploads/2021/02/HP-PDP-BalancingMask-Hero-2.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTCNUS7D4oiAqWaQbCt8I4Ao1amYgwd9ojTGQ&s"
        }),
        // Rejuvenating Mask
        ("Rejuvenating Mask", new List<string>
        {
            "https://cdn.shopify.com/s/files/1/0345/0444/1995/files/PDP-RejuvenatingMask-Results.jpg?v=1661906989", 
            "https://hydropeptide.co.uk/cdn/shop/products/rejuvenating-mask_full-size.jpg?v=1662076519", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS5xMKeR40mJv14WObkfKV-EpCMvfT5jlPDlA&s"
        }),
        // Aqualia Thermal Night Spa
        ("Aqualia Thermal Night Spa", new List<string>
        {
            "https://product.hstatic.net/200000124701/product/00014081_vichy_mat_na_khoang_75ml_m9104500_6680_5c9e_large_88ade14c39_01d00c3a004a464b8b0e09f6bbc6ffb1_master.jpg", 
            "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/product-packshots---1/aqualia/vichy_aqualia_thermal_creme_nuit.png?rev=296ada59cb5e41109fb3b0d565c68596&cx=0.51&cy=0.54&cw=525&ch=596&hash=2D0C1F95950702CE1CC90F8691F193F5", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQK37Y_CeTdJPn4VyPC0fCvjfmImZwSQrOhrQdTNwu4hPPB7NwrdedizcNK0FDZEplJQTg&usqp=CAU"
        }),
        // Quenching Mineral Mask
        ("Quenching Mineral Mask", new List<string>
        {
            "https://product.hstatic.net/200000124701/product/00014081_vichy_mat_na_khoang_75ml_m9104500_6680_5c9e_large_88ade14c39_01d00c3a004a464b8b0e09f6bbc6ffb1_master.jpg", 
            "https://cf.shopee.vn/file/1674db822b2bc08d254590acabf64547", 
        }),
        // Pore Purifying Clay Mask
        ("Pore Purifying Clay Mask", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTmtvhbhIddJEIZPbAz6q8EFueP1vtX4HhqQw&s", 
            "https://down-vn.img.susercontent.com/file/244ad88d90d841ab5af1681edc2258a3", 
        }),
        // Sulwhasoo Activating Mask
        ("Sulwhasoo Activating Mask", new List<string>
        {
            "https://kallos.co/cdn/shop/files/12_33f1be98-3f05-421b-98cb-67be17d1e90b.jpg?v=1693728111&width=900", 
            "https://assets.aemi.vn/images/2024/5/1715574861200-0", 
            "https://product.hstatic.net/200000714339/product/z5219389220375_0b7217f34a936c85ae10f994e6bdda40_2c716e8ce65146fc938d37df2bbd805d.jpg"
        }),
        // COSRX Ultimate Nourishing Rice Spa Overnight Mask
        ("COSRX Ultimate Nourishing Rice Spa Overnight Mask", new List<string>
        {
            "https://image.hsv-tech.io/1920x0/bbx/products/0e8fbb74-e136-4ea3-9ef0-76f6d44bca3b.webp", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT8ESM2p1P_SgJXVC6VPdOIambFBoukzgAGdw&s", 
        }),
        // Klairs Rich Moist Soothing Tencel Sheet Mask
        ("Klairs Rich Moist Soothing Tencel Sheet Mask", new List<string>
        {
            "https://product.hstatic.net/1000006063/product/klairs_rich_moist_soothing_tencel_sheet_mask_94678ee3e2134354b5a039b569cff87e_1024x1024.jpg", 
            "https://product.hstatic.net/200000551679/product/dear__klairs_mat_na_giay_rich_moist___2__db38203e18c541a19031c3212db864d4_1024x1024.jpg", 
            "https://product.hstatic.net/200000714339/product/klairs-rich-moist-soothing-sheet_bee5a326d74642bab0e5862e663e09b8_1024x1024.jpg"
        }),
        // Klairs Midnight Blue Calming Sheet Mask
        ("Klairs Midnight Blue Calming Sheet Mask", new List<string>
        {
            "https://image.hsv-tech.io/1987x0/bbx/common/38079d73-de48-4721-88af-c0e72b28a471.webp", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS4SsU7xocAIl8x0h4bj_lrLO0felkJMgASmw&s", 
        }),
        // Dear, Klairs Freshly Juiced Vitamin E Mask
        ("Dear, Klairs Freshly Juiced Vitamin E Mask", new List<string>
        {
            "https://www.guardian.com.vn/media/catalog/product/cache/30b2b44eba57cd45fd3ef9287600968e/3/0/3020993_iifvm7d8r9crinym.jpg", 
            "https://image.hsv-tech.io/1920x0/bbx/common/12aecda8-804a-47d0-9205-b881a6ce3174.webp", 
        }),
        // Sacred Nature Exfoliant Mask
        ("Sacred Nature Exfoliant Mask", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/0d080a4d83fbeff70dd59e4585d73e1d76f7b92d_2000x-1091x1200.jpg", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/e8a1b4fb805fbdc0b8d1f659b595477ce4d7ff79_2000x.jpg", 
            "https://eideal.com/cdn/shop/files/Exfoliant-mask-Texture-1.jpg?v=1703580157"
        }),
        // Essential Scrub
        ("Essential Scrub", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2023/11/San-pham-112.jpg", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/bf8201b7fbba797ceca178721986f5de589c9eed_2000x-698x768.jpg", 
        }),
        // Liquid Resurfacing Solution
        ("Liquid Resurfacing Solution", new List<string>
        {
            "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_LiquidResufacingSolution_PDP_1024x1024.jpg?v=1718661002", 
            "https://hydropeptide.ca/cdn/shop/files/022024_LiquidResurfacingSolution_PDP.jpg?v=1712791423", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSuv41x-1M5vuU3jX5Ggnd6NfFwrKNUe_D56Q&s"
        }),
        // 5X Power Peel Face Exfoliator
        ("5X Power Peel Face Exfoliator", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS6Xgx3XAAFH8I5qf0piOxYUBFpx_wro35G6Q&s", 
            "https://hydropeptide.ca/cdn/shop/files/012224_Old_Retail_Packaging_5xPowerPeel_Sachets_PDP.jpg?v=1712790410", 
        }),
        // Daily Milkfoliant
        ("Daily Milkfoliant", new List<string>
        {
            "https://edbeauty.vn/wp-content/uploads/2023/08/Tay-te-bao-chet-Dermalogica-Daily-Milkfoliant-2.jpg", 
            "https://veevee.store/wp-content/uploads/2023/10/dermalogica-daily-milkfoliant-exfoliator-2.webp", 
        }),
        // Liquid Peelfoliant
        ("Liquid Peelfoliant", new List<string>
        {
            "https://healthygoods.com.vn/resource/images/2023/12/dermalogica-liquid-peelfoliant-59ml1.jpg", 
            "https://www.spacenk.com/on/demandware.static/-/Library-Sites-spacenk-global/default/dwb77b7327/dermalogica-liquid-peel-review-space-nk.jpg", 
        }),
        // Daily Superfoliant
        ("Daily Superfoliant", new List<string>
        {
            "https://dermalogica-vietnam.com/wp-content/uploads/2024/03/T%E1%BA%A9y-T%E1%BA%BF-B%C3%A0o-Ch%E1%BA%BFt-Than-Ho%E1%BA%A1t-T%C3%ADnh-Daily-Superfoliant.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWwLsIyzmrB8hOus3oKNIoufCNavvosd0Zog&s", 
        }),
        // Multivitamin Thermafoliant
        ("Multivitamin Thermafoliant", new List<string>
        {
            "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/30/images/dermalogica-multivitamin-thermafoliant-1.jpg", 
            "https://veevee.store/wp-content/uploads/2023/10/dermalogica-multivitamin-thermafoliant-1.webp", 
        }),
        // Daily Microfoliant
        ("Daily Microfoliant", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS6Xgx3XAAFH8I5qf0piOxYUBFpx_wro35G6Q&s", 
            "https://thesecretdayspa.co.uk/cdn/shop/products/US-PDP-How-To-Product-In-Hand---Daily-Microfoliant_1800x1800.webp?v=1678979333", 
        }),
        // Strawberry Rhubarb Dermafoliant
        ("Strawberry Rhubarb Dermafoliant", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-strawberry-rhubarb-dermafoliant.jpg?itok=lP2gq52k", 
            "https://anjouspa.com/wp-content/uploads/2023/01/anjou-spa-fresh-fruit-facial-27.jpg", 
        }),
        // Turmeric Energizing Treatment
        ("Turmeric Energizing Treatment", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTbvQKm1rsUl0wMFrKASBA5iBt0VX69eQIyjw&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTb2Zl2efrdTibFj1FlzpWE9n4qQaT7a-fe4w&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1TWpyNYBuXgZHao3xTL4Y4KWq-VYK_yFLNw&s"
        }),
        // Bright Skin Licorice Root Exfoliating Peel
        ("Bright Skin Licorice Root Exfoliating Peel", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTL9Oq6DD-KmiZGSPvBt6viuJI3wf059HorRQ&s", 
            "https://emstore.com/cdn/shop/files/licorice-root-exfoliating-peel-pads.jpg?v=1695202885", 
        }),
        // Calm Skin Chamomile Exfoliating Peel
        ("Calm Skin Chamomile Exfoliating Peel", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRFf6rWs4EsBBhwJBa458IvTlMEnUf1ZZqiXQ&s", 
            "https://static.thcdn.com/productimg/960/960/11370437-1344871983881681.jpg", 
        }),
        // Radish Seed Refining Peel
        ("Radish Seed Refining Peel", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-radish-seed-refining-peel-400x400px.png?itok=G7HyJYY5", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS7PLc_tx0I_lQDDPwITKuaqEAVUN7u7IsuMw&s", 
        }),
        // Anua Heartleaf 77% Clear Pad
        ("Anua Heartleaf 77% Clear Pad", new List<string>
        {
            "https://bizweb.dktcdn.net/100/525/087/products/56aa5b2e-5258-4122-beee-d53128f44c3c.jpg?v=1733139782937", 
            "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lrqzijemkds443", 
            "https://cdn.shopify.com/s/files/1/0560/7328/9826/files/IMG-4057.jpg?v=1714980820"
        }),
        // No.5 Vitamin-Niacinamide Concentrated Pad
        ("No.5 Vitamin-Niacinamide Concentrated Pad", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTkRt0Da2RE6ez4IpyF-9eVbo1rgj_6wA-4LQ&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQqjR310AXBDPsifkWK0rsXBxhDnTBDCW50Yg&s", 
            "https://www.mikaela-beauty.com/cdn/shop/files/AX6H2399w_1200x1200.jpg?v=1711843023"
        }),
        // Balanceful Cica Toner Pad
        ("Balanceful Cica Toner Pad", new List<string>
        {
            "https://product.hstatic.net/1000006063/product/bt_91f25bc0d4854817a2c94eff8a459e9e_1024x1024.jpg", 
            "https://product.hstatic.net/1000328823/product/_20__19__9352572e4c5245a4a9756cb089a6d047_master.png", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTxFuy4bq9_k4ZsMOIcLNhKFheC0oJnhQ7gJQ&s"
        }),
        // Pine Needle Pore Pad Clear Touch
        ("Pine Needle Pore Pad Clear Touch", new List<string>
        {
            "https://down-vn.img.susercontent.com/file/7876b204a9fcf65d0930190e274bb254", 
            "https://cdn.zochil.shop/bbafea58-b901-4841-9319-281f9b7c4be3_t1500.jpg", 
        }),
        // Krave Kale-lalu-yAHA
        ("Krave Kale-lalu-yAHA", new List<string>
        {
            "https://kravebeauty.com/cdn/shop/files/9.24_KLY_PDP1.png?v=1727740531", 
            "https://i.ebayimg.com/images/g/M18AAOSw9BpkHShE/s-l400.jpg", 
            "https://lilabeauty.com.au/cdn/shop/products/buy-krave-beauty-kale-lalu-yaha-200ml-at-lila-beauty-korean-and-japanese-beauty-skin-care-255400.jpg?v=1648860498"
        }),
        // Yuzu Solid Body Oil
        ("Yuzu Solid Body Oil", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTa6cZWEjkws8RPCsTXRPcfNM7GlmkJ86fuXg&s", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT091jFcMQELHhS2zqu8kDBc39zoRi4Sck4LQ&s", 
        }),
        // Mangosteen Body Lotion
        ("Mangosteen Body Lotion", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-mangosteen-body-lotion-400x400px-compressed.png?itok=Ofu_3Y-c", 
            "https://cdn11.bigcommerce.com/s-wcc90u14r8/images/stencil/1280x1280/products/9804/24894/skin-beauty-eminence-mangosteen-body-lotion-8.4__31975.1729024904.jpg?c=2", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSMES-hL9lyHa3KeoivJfChbejtJvNOfXyflQ&s"
        }),
        // Coconut Sugar Scrub
        ("Coconut Sugar Scrub", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTFrsNXy29hCQjfyrcStKLnNID2zv_9QuKvvQ&s", 
            "https://i5.walmartimages.com/seo/Eminence-Stone-Crop-Revitalizing-Body-Scrub-8-4-oz_f7168ae2-fd69-45f7-9c04-443ddcc24516.961abd73cdb34020becd439ede8416a6.jpeg", 
        }),
        // Stone Crop Contouring Body Cream
        ("Stone Crop Contouring Body Cream", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/stone-crop-body-contouring-cream-400x400.png?itok=gTrvgdf1", 
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-slide/eminence-organics-stone-crop-body-contouring-cream-swatch-400x400.jpg?itok=CW8Z9DlV", 
        }),
        // Stone Crop Body Oil
        ("Stone Crop Body Oil", new List<string>
        {
            "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/stone-crop-body-oil-400x400.png?itok=kCbSF1gV", 
            "https://i5.walmartimages.com/asr/3c096190-35c1-443b-876c-65b80976c79b.d68538517adb4174192b340277828d4e.jpeg?odnHeight=768&odnWidth=768&odnBg=FFFFFF", 
        }),
        // Sacred Nature Body Butter
        ("Sacred Nature Body Butter", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/9720a1e9b759cb104a72600261dc4320a1e31fe4_2000x.jpg", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/181a900228143e96fd981d63502195294ef76dfe_2000x-1091x1200.jpg", 
            "https://vonpreen.com/wp-content/uploads/2023/03/Comfort-Zone-Sacred-Nature-Body-Butter.jpg"
        }),
        // Tranquillity Oil
        ("Tranquillity Oil", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSz0kljZlGJ8sUGfVH9Q5GXUrVZd51g4A83yQ&s", 
            "https://down-vn.img.susercontent.com/file/sg-11134201-22110-4gj57hclpnjv4b", 
        }),
        // Body Strategist Peel Scrub
        ("Body Strategist Peel Scrub", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/4a34a3dd50e62994703db0d5db6c98c3ac321337_2000x.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRNq-zuTktcugPue1xZh1lJ_BagEEnHf-x0mQ&s", 
            "https://i.makeup.ae/600x600/qgmotujz0cvp.jpg"
        }),
        // Body Strategist Oil
        ("Body Strategist Oil", new List<string>
        {
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/61200985bf4310406e0a68b32f26e54ffad19bf5_2000x.jpg", 
            "https://oadep.com/wp-content/uploads/2023/03/duong-the-Comfort-Zone-Body-Strategist-Oi-100ml-chinh-hang.jpg", 
            "https://comfortzone.com.vn/wp-content/uploads/2022/10/c0c3698e5819ca297e7b8be9f087c36a4a31cef4_2000x-1091x1200.jpg"
        }),
        // Body Strategist Contour Cream
        ("Body Strategist Contour Cream", new List<string>
        {
            "https://world.comfortzoneskin.com/cdn/shop/files/myqk9504x4uzbbh0gz8u_5000x.jpg?v=1718128996", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRQ1zUHTv9Xso7yRlufR12dR9ahzcB1Po3M0LBxOdrtbIOrBmPDpaq-lVlUdo-Dkey4qsQ&usqp=CAU", 
        }),
        // Body Hydrating Cream
        ("Body Hydrating Cream", new List<string>
        {
            "https://dangcapphaidep.vn/image-data/780-780/cdn/2018/07/Dermalogica-Body-Hydrating-Cream-1.jpg", 
            "https://dermalogica-vietnam.com/wp-content/uploads/2021/06/2.1.jpg", 
            "https://bluemercury.com/cdn/shop/files/global_images-666151111103-2.jpg?v=1725057930&width=1500"
        }),
        // Phyto Replenish Body Oil
        ("Phyto Replenish Body Oil", new List<string>
        {
            "https://dr-skincare.com/wp-content/uploads/2024/03/64.png", 
            "https://product.hstatic.net/1000160964/product/phyto-replenish-body-oil-with-ingredients-400_2x_2b58ad161eca4e67851e178c2b38bfd4_master.jpg", 
            "https://dermalogica-vietnam.com/wp-content/uploads/2020/06/8.8.jpg"
        }),
        // Conditioning Body Wash
        ("Conditioning Body Wash", new List<string>
        {
            "https://hoaoaihuong.vn/upload/data/images/product/1587433351_ConditioningBodyWashwithRosemaryIllustration_1_700x800.jpg", 
            "https://www.depmoingay.net.vn/wp-content/uploads/2023/08/Sua-tam-min-da-Dermalogica-Conditioning-Hand-Body-Wash-1.jpg", 
            "https://www.dermalogica.ca/cdn/shop/products/conditioning-body-wash_84-01c_590x617_bf506ab2-c69d-47c3-8b74-6594dedaea1b.jpg?v=1600289356&width=1445"
        }),
        // Nourishing Glow Body Oil
        ("Nourishing Glow Body Oil", new List<string>
        {
            "https://hydropeptide.co.uk/cdn/shop/products/nourishing-glow_full-size.jpg?v=1660861721", 
            "https://cdn.shopify.com/s/files/1/0345/0444/1995/files/PDP-NourishingGlow-HowToUse.jpg?v=1660794950", 
            "https://i.makeup.ae/m/mu/muboadhvbzcg.jpg"
        }),
        // Firming Body Moisturizer
        ("Firming Body Moisturizer", new List<string>
        {
            "https://hydropeptide.co.uk/cdn/shop/products/firming-moisturizer_full-size.jpg?v=1681516762", 
            "https://www.fruitionskintherapy.ca/wp-content/uploads/hydropeptide-body-firming-body-moisturizer-2.jpg", 
        }),
        // Illiyoon Ceramide Ato Concentrate Cream
        ("Illiyoon Ceramide Ato Concentrate Cream", new List<string>
        {
            "https://seoulplus.com/wp-content/uploads/2023/07/Illiyoon-Ceramide-Ato-Concentrate-Cream-1.jpg", 
            "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-llzx6x3exbdrb8", 
        }),
        // Dear Doer The Hidden Body Scrub & Wash
        ("Dear Doer The Hidden Body Scrub & Wash", new List<string>
        {
            "https://m.media-amazon.com/images/I/71f+T8Po-bL.jpg", 
            "https://m.media-amazon.com/images/I/71qOFL2EeUL._AC_UF350,350_QL80_.jpg", 
            "https://www.ballagrio.com/cdn/shop/files/2_ff23dccd-b3b9-471c-b9af-0e44d9101f2a_2048x.jpg?v=1689571364"
        }),
        // Aestura Atobarrier 365 Ceramide Lotion
        ("Aestura Atobarrier 365 Ceramide Lotion", new List<string>
        {
            "https://m.media-amazon.com/images/I/51mD15TJzJL._AC_UF1000,1000_QL80_.jpg", 
            "https://i.ebayimg.com/images/g/2zwAAOSwPxpi~wUt/s-l1200.jpg", 
            "https://down-vn.img.susercontent.com/file/sg-11134207-7rbm3-lpm4wcfn4hbpfd"
        }),
        // Dr. Orga pH-balanced Houttuynia Cordata Red Spot Mist
        ("Dr. Orga pH-balanced Houttuynia Cordata Red Spot Mist", new List<string>
        {
            "https://sokoglam.com/cdn/shop/files/Soko-Glam-PDP-Dr-Orga-Houttuynia-Cordata-Body-Mist-01_860x.png?v=1705971958", 
            "https://sokoglam.com/cdn/shop/files/Soko-Glam-PDP-Dr-Orga-Houttuynia-Cordata-Body-Wash-03_860x.png?v=1705972007", 
        }),
        // Derma B Daily Moisture Body Oil
        ("Derma B Daily Moisture Body Oil", new List<string>
        {
            "https://sokoglam.com/cdn/shop/files/Soko-Glam-PDP-Derma-B-Daily-Moisture-Body-Oil-02_860x.png?v=1687301614", 
            "https://down-vn.img.susercontent.com/file/8b3b623e8eeeddd133584f44dd7a77e1", 
            "https://koreamarket.ru/wa-data/public/shop/products/04/90/19004/images/10900/10900.970.jpg"
        }),
        // Davines Energizing Shampoo
        ("Davines Energizing Shampoo", new List<string>
        {
            "https://davinesvietnam.com/wp-content/uploads/2019/07/dau-goi-Davines-Energizing-Shampoo-1000ml-1.jpg", 
            "https://www.planetbeauty.com/cdn/shop/files/Energize_shp_bk_x2000.jpg?v=1684540681", 
        }),
        // Davines Volu Shampoo
        ("Davines Volu Shampoo", new List<string>
        {
            "https://vn.davines.com/cdn/shop/files/rtqshfyvzhwofcdaxnm9_2000x_bf029392-1ae3-48bf-99f6-dab3cb178723.jpg?v=1721123829", 
            "https://i.ebayimg.com/images/g/1gwAAOSwfi5kQG9-/s-l1200.jpg", 
        }),
        // Davines Calming Shampoo
        ("Davines Calming Shampoo", new List<string>
        {
            "https://vn.davines.com/cdn/shop/products/71262_NATURALTECH_CALMING_Calming_Shampoo_250ml_Davines_2000x.jpg?v=1721118741", 
            "https://bizweb.dktcdn.net/100/141/195/products/1363f1c28cd32f8d76c2.jpg?v=1718777142610", 
        }),
        // Davines Dede Shampoo
        ("Davines Dede Shampoo", new List<string>
        {
            "https://vn.davines.com/cdn/shop/products/75019_ESSENTIAL_HAIRCARE_DEDE_Shampoo_250ml_Davines_2000x.jpg?v=1721121499", 
            "https://www.planetbeauty.com/cdn/shop/products/Davines_Dede_Shampoo_back_x2000.jpg?v=1683765042", 
        }),
        // Davines Melu Shampoo
        ("Davines Melu Shampoo", new List<string>
        {
            "https://vn.davines.com/cdn/shop/products/75097_ESSENTIAL_HAIRCARE_MELU_Shampoo_250ml_Davines_600x.jpg?v=1721121211", 
            "https://down-vn.img.susercontent.com/file/sg-11134201-7rd4y-lxae2wn24xn0cb", 
        }),
        // Bain Décalcifiant Réparateur Repairing Shampoo
        ("Bain Décalcifiant Réparateur Repairing Shampoo", new List<string>
        {
            "https://www.lmching.com/cdn/shop/files/ProductSize_1_2_-2024-08-09T092055.601_800x.jpg?v=1723170064", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRImTYO-AKUfNQ9nQUAVCCmGJqOD7DmC3NTuw&s", 
        }),
        // Bain Densité Shampoo
        ("Bain Densité Shampoo", new List<string>
        {
            "https://www.lmching.com/cdn/shop/files/ProductSize_1_2_-2024-08-09T092055.601_800x.jpg?v=1723170064", 
            "https://beautytribe.com/cdn/shop/files/kerastase-densifique-bain-densite-250ml-105115-467836.jpg?v=1732792066&width=1500", 
        }),
        // Bain Hydra-Fortifiant Shampoo
        ("Bain Hydra-Fortifiant Shampoo", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWOrdBrhw9Spr0jyNZMHmGhMMRTWI5QkzBJw&s", 
            "https://supernovasalon.com/cdn/shop/products/bain-hydra-fortifiant-lifestyle_900x.webp?v=1647475834", 
            "https://shopurbannirvana.com/cdn/shop/files/GENESIS-BAIN-HYDRA-500ML-2.jpg?v=1730219325&width=1445"
        }),
        // L'Oreal Paris Elseve Total Repair 5 Repairing
        ("L'Oreal Paris Elseve Total Repair 5 Repairing Shampoo", new List<string>
        {
            "https://product.hstatic.net/1000006063/product/l_oreal_elseve_total_repair_5_shampoo_650ml_cf374f7742d44e639abc23d1b63e3fcb_1024x1024.jpg", 
            "https://image.hsv-tech.io/1987x0/bbx/l_oreal_paris_elseve_total_repair_5_shampoo_330ml_d9cfd29c63b142db8bbf4201153ca0b7.png", 
        }),
        // L'Oreal Professional Hair Spa Deep Nourishing
        ("L'Oreal Professional Hair Spa Deep Nourishing Shampoo", new List<string>
        {
            "https://media.hcdn.vn/catalog/product/g/o/google-shopping-dau-goi-l-oreal-professionnel-cap-am-cho-toc-kho-600ml_img_680x680_d30c8d_fit_center.jpg", 
            "https://down-vn.img.susercontent.com/file/sg-11134201-7rd75-lty2ehqkzei8ac", 
        }),
        // L'Oreal Paris Elseve Fall Resist 3X Anti-Hairfall
        ("L'Oreal Paris Elseve Fall Resist 3X Anti-Hairfall Shampoo", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSNOiI0F3AwhpbFi1s-H6iAqE0Hy1y6Dv-L-w&s", 
            "https://m.media-amazon.com/images/I/612rxvwYs1L._AC_UF1000,1000_QL80_.jpg", 
        }),
        // Nº.4 Bond Maintenance Shampoo
        ("Nº.4 Bond Maintenance Shampoo", new List<string>
        {
            "https://image.hsv-tech.io/1987x0/bbx/common/3bb29575-9f8e-48eb-a709-f019f95cc65f.webp", 
            "https://image.hsv-tech.io/1987x0/bbx/common/1d335186-bd89-4e62-ab87-72d9bd03557a.webp", 
            "https://product.hstatic.net/1000172157/product/olaplex_cap_no4_5_250ml_04_b501890da8f440d6a4cf66d2069d1524_large.jpg"
        }),
        // Gold Lust Repair & Restore Shampoo
        ("Gold Lust Repair & Restore Shampoo", new List<string>
        {
            "https://www.sephora.com/productimages/sku/s2438166-main-zoom.jpg?imwidth=315", 
            "https://www.oribe.com/cdn/shop/files/1200Wx1200H-400172_1b27cf32-3e50-43b4-954b-1790ae536f2e.jpg?v=1698874898", 
            "https://images-na.ssl-images-amazon.com/images/I/71kp9RhJD3L.jpg"
        }),
        // Supershine Hydrating Shampoo
        ("Supershine Hydrating Shampoo", new List<string>
        {
            "https://www.oribe.com/cdn/shop/files/402453-0_b7d201cd-c3f9-4b48-84c5-f9e3b03a522a.jpg?v=1725462201", 
            "https://images.finncdn.no/dynamic/1280w/2024/12/vertical-0/13/8/384/925/188_a9edb849-aba8-4933-acad-827f251c0c4e.jpg", 
        }),
        // Acidic Bonding Concentrate sulfate-free Shampoo
        ("Acidic Bonding Concentrate sulfate-free Shampoo", new List<string>
        {
            "https://images-na.ssl-images-amazon.com/images/I/61T+3kBuHoL.jpg", 
            "https://images-na.ssl-images-amazon.com/images/I/71J1-1eoRbL.jpg", 
        }),
        // Redken All Soft Shampoo
        ("Redken All Soft Shampoo", new List<string>
        {
            "https://images-na.ssl-images-amazon.com/images/I/71xQ59SWkML.jpg", 
        }),
        // Izumi Tonic Strengthening Shampoo
        ("Izumi Tonic Strengthening Shampoo", new List<string>
        {
            "https://static.thcdn.com/productimg/original/14204021-6465063343389571.jpg", 
            "https://static.thcdn.com/productimg/original/14204021-9055063343445593.jpg", 
        }),
        // Ultimate Reset Extreme Repair Shampoo
        ("Ultimate Reset Extreme Repair Shampoo", new List<string>
        {
            "https://cdn.haarshop.ch/catalog/product/thumbnail/51ac8a89d88d4eed1e1f5a7566ce210ab624b84b71e44e4fa3b44063/image/0/570x570/111/99/7/0/70941222e739145ec75a7a705ff8f17b6f55fc15_3474636610181_bi_shu_uemura_ultimate_reset_shampoo.jpg", 
            "https://m.media-amazon.com/images/I/81j7Ha+ZN+L._AC_UF1000,1000_QL80_.jpg", 
        }),
        // Fusion Shampoo
        ("Fusion Shampoo", new List<string>
        {
            "https://www.wella.com/professional/m/Supernova/Fusion/PDP/Fusion-shampoo-supernova-slider-packshot-1_d.jpg", 
            "https://www.wella.com/professional/m/Supernova/Fusion/PDP/Fusion-shampoo-supernova-slider-packshot-6_d.jpg", 
        }),
        // Ultimate Repair Shampoo
        ("Ultimate Repair Shampoo", new List<string>
        {
            "https://www.wella.com/professional/m/care/Fire/Product_Packshots/Ultimate-Repair-Shampoo-slider-packshot-v2_d.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTXUItcEx4vQtgGnn4SBGez8xi8-68r49hSaQ&s", 
            "https://cdn.awsli.com.br/613/613406/produto/2389103400b84107f38.jpg"
        }),
        // Davines Dede Conditioner 
        ("Davines Dede Conditioner", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWFr_SKchvHJVFDWloohAIqYFse_GceySgQw&s", 
            "https://vn.davines.com/cdn/shop/products/PROSIZEWEBSITE-11_2084x.jpg?v=1721121409", 
        }),
        // Davines Love Smoothing Conditioner
        ("Davines Love Smoothing Conditioner", new List<string>
        {
            "https://davinesvietnam.com/wp-content/uploads/2019/07/dau-xa-Davines-Love-Smoothing-Conditioner-250ml-chinh-hang-gia-re.jpg", 
            "https://lizi.vn/wp-content/uploads/2020/02/dau-goi-davines-love-smoothing-3.jpeg", 
        }),
        // Davines Melu Conditioner
        ("Davines Melu Conditioner", new List<string>
        {
            "https://vn.davines.com/cdn/shop/products/75608_ESSENTIAL_HAIRCARE_MELU_Conditioner_250ml_Davines_2000x_4a9aa7af-bc07-4713-bd1f-4b8c9c5b4d53_2000x.jpg?v=1721121159", 
            "https://aslaboratory.com.ua/image/cache/catalog/import/001818-crop-600x750-product_thumb.jpg", 
        }),
        // Davines Momo Conditioner 
        ("Davines Momo Conditioner", new List<string>
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSlTOtpGa0eZH_hOfmHfed3ivvAl-PaNgDPpg&s", 
            "https://conceptcshop.com/cdn/shop/files/davines-revitalisant-hydratant-momo-1000-ml-130430.jpg?v=1728407522&width=2048", 
        }),
        // Fondant Renforçateur Conditioner
        ("Fondant Renforçateur Conditioner", new List<string>
        {
            "https://supernovasalon.com/cdn/shop/products/fondant-renforcateur-lifestyle_800x.webp?v=1647476539", 
            "https://millies.ie/cdn/shop/files/KerastaseGenesisFondantRenforcateur_3.jpg?v=1705681358&width=3024", 
        }),
        // Fondant Densité Conditioner
        ("Fondant Densité Conditioner", new List<string>
        {
            "https://emmakcreativestyling.ie/wp-content/uploads/2023/04/Kerastase-Densifique-Fondant-Densite_Gallery-Image-2.webp", 
            "https://img-cdn.heureka.group/v1/6cdbcbe4-8b14-535b-ad4c-34848f5dc065.jpg", 
        }),
        // Fondant Fluidealiste Conditioner
        ("Fondant Fluidealiste Conditioner", new List<string>
        {
            "https://cdn11.bigcommerce.com/s-f7ta3/images/stencil/1280x1280/products/3856/8706/kerastase-discipline-fondant-fluidealiste-conditioner-200ml__51586.1657021232.jpg?c=2", 
            "https://static.thcdn.com/productimg/960/960/10951828-6334927764118014.jpg", 
        }),
        // Ciment Anti-Usure Conditioner
        ("Ciment Anti-Usure Conditioner", new List<string>
        {
            "https://i0.wp.com/salonvenere.com/wp-content/uploads/2023/01/3474636397884.Main_.jpg?fit=930%2C930&ssl=1", 
            "https://www.kerastase.co.uk/dw/image/v2/AAQP_PRD/on/demandware.static/-/Sites-ker-master-catalog/en_GB/dwda75a874/product/resistance/3474636397884.pt01.jpg?sw=340&sh=340&sm=cut&sfrm=jpg&q=80", 
        }),
        // Redken All Soft Conditioner
        ("Redken All Soft Conditioner", new List<string>
        {
            "https://www.redken.com/dw/image/v2/AAFM_PRD/on/demandware.static/-/Sites-ppd-redken-master-catalog/default/dwf2db2623/images/pdp/all-soft-conditioner/redken-all-soft-conditioner-for-dry-hair.png", 
            "https://images-na.ssl-images-amazon.com/images/I/617nmDgkTfL.jpg", 
        }),
        // Redken Frizz Dismiss Conditioner
        ("Redken Frizz Dismiss Conditioner", new List<string>
        {
            "https://www.ubuy.vn/productimg/?image=aHR0cHM6Ly9tLm1lZGlhLWFtYXpvbi5jb20vaW1hZ2VzL0kvNjFSSTQ4VTMtV0wuX1NMMTUwMF8uanBn.jpg", 
            "https://m.media-amazon.com/images/I/61ERLLB3uSL.jpg", 
            "https://www.ozhairandbeauty.com/_next/image?url=https%3A%2F%2Fcdn.shopify.com%2Fs%2Ffiles%2F1%2F1588%2F9573%2Ffiles%2FRedken-Frizz-Dismiss-Sodium-Chloride-Free-Conditioner_7.jpg%3Fv%3D1714700684&w=3840&q=75"
        }),
        // L'Oréal Paris Elvive Total Repair 5 Conditioner
        ("L'Oréal Paris Elvive Total Repair 5 Conditioner", new List<string>
        {
            "https://i5.walmartimages.com/seo/L-Oreal-Paris-Elvive-Total-Repair-5-Repairing-Conditioner-For-Damaged-Hair-With-Protein-And-Ceramide-Strong-Silky-Shiny-Healthy-Renewed-28-Fl-Oz_cec04c0d-34eb-4fa9-9de8-3001f4a60d25.a403fb04c77a2dfe68657dcebd78d065.jpeg", 
            "https://images-na.ssl-images-amazon.com/images/I/81VWk+QHgyL.jpg", 
        }),
        // L'Oréal Paris EverPure Moisture Conditioner
        ("L'Oréal Paris EverPure Moisture Conditioner", new List<string>
        {
            "https://m.media-amazon.com/images/I/61wefZxO9gL.jpg", 
        }),
        // L'Oréal Paris EverCurl Hydracharge Conditioner
        ("L'Oréal Paris EverCurl Hydracharge Conditioner", new List<string>
        {
            "https://m.media-amazon.com/images/I/71I7nEWa6DL.jpg", 
        }),
        // Invigo Nutri-Enrich Conditioner
        ("Invigo Nutri-Enrich Conditioner", new List<string>
        {
            "https://mcgrocer.com/cdn/shop/files/aeb74ae52abdfdf7746a2a4a84d407ae.jpg?v=1710962092", 
            "https://www.capitalhairandbeauty.ie/Images/Product/Default/xlarge/867618.jpg", 
            "https://www.lookfantastic.com/images?url=https://static.thcdn.com/productimg/original/11711572-1815158483432062.jpg&format=webp&auto=avif&width=1200&height=1200&fit=cover"
        }),
        // Elements Renewing Conditioner
        ("Elements Renewing Conditioner", new List<string>
        {
            "https://www.modernhairbeauty.com/wp-content/uploads/2021/08/Elements-renewing-conditioner.jpg", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTnbpIdPoJxjZzQ0dJZc1DVy5NRqkHgT7iWDw&s", 
        }),
        // Moisture Velvet Nourishing Conditioner
        ("Moisture Velvet Nourishing Conditioner", new List<string>
        {
            "https://www.everythinghairandbeauty.com.au/assets/full/SHUUEMURA-3474630146693.jpg?20220420183416", 
        }),
        // Ultimate Remedy Conditioner
        ("Ultimate Remedy Conditioner", new List<string>
        {
            "https://www.shuuemuraartofhair-usa.com/dw/image/v2/AANG_PRD/on/demandware.static/-/Sites-shu-master-catalog/default/dw328561bc/2019/Full/Ultimate-Reset/shu-uemura-ultimate-reset-conditioner.jpg?sw=270&sfrm=png&q=70", 
            "https://images.squarespace-cdn.com/content/v1/5b688fd0f407b48b1e37b441/1610912816432-JL6OFF0M1CVE9Z7RWEI9/ultimate_remedy_travel_size_shampoo.jpg?format=1000w", 
        }),
        // No. 5 Bond Maintenance Conditioner
        ("No. 5 Bond Maintenance Conditioner", new List<string>
        {
            "https://arterashop.com/wp-content/uploads/2021/01/OLAP5-600x600.jpg", 
            "https://image.hsv-tech.io/1987x0/bbx/common/5e14cb4a-d1d0-449b-b27b-467e92b456ce.webp", 
        }),
        // Gold Lust Repair & Restore Conditioner
        ("Gold Lust Repair & Restore Conditioner", new List<string>
        {
            "https://cdn.vuahanghieu.com/unsafe/0x900/left/top/smart/filters:quality(90)/https://admin.vuahanghieu.com/upload/product/2024/03/dau-xa-oribe-gold-lust-repair-restore-conditioner-200ml-66077ad485b29-30032024093708.jpg", 
            "https://bizweb.dktcdn.net/100/445/245/products/3-1719469113736.png?v=1720865387960", 
            "https://www.oribe.com/cdn/shop/products/1200Wx1200H-400103-4_8b6f47b7-d8fe-4613-a565-91248acdafee.jpg?v=1704092392&width=3840"
        }),
        // Signature Moisture Masque
        ("Signature Moisture Masque", new List<string>
        {
            "https://www.oribe.com/cdn/shop/products/1200Wx1200H-400298-2_60a552ba-8f61-4315-9260-0da9ab2c01a2.jpg?v=1691604399&width=3840", 
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSovZKp21hY7eOHYioOmG8ONQ3o4BO6xu5bPw&s", 
        }),

    };

    // Duyệt qua danh sách các sản phẩm và thêm hình ảnh cho từng sản phẩm
    foreach (var productData in productImageData)
    {
        // Tìm sản phẩm trong cơ sở dữ liệu theo tên
        var product = products.FirstOrDefault(p => p.ProductName == productData.productName);

        if (product != null)
        {
            // Lặp qua danh sách các hình ảnh và thêm vào bảng ProductImages
            foreach (var imageUrl in productData.imageUrls)
            {
                var productImage = new ProductImages
                {
                    ProductId = product.ProductId,
                    image = imageUrl
                };

                // Thêm hình ảnh vào cơ sở dữ liệu
                await _context.ProductImages.AddAsync(productImage);
            }
        }
    }

    // Lưu tất cả thay đổi vào cơ sở dữ liệu
    await _context.SaveChangesAsync();
}


        private async Task SeedServiceImages()
        {
            // Lấy danh sách tất cả các dịch vụ hiện có
            var services = await _context.Services.ToListAsync();

            // Tạo danh sách các dịch vụ và hình ảnh của chúng
            var serviceImageData = new List<(string serviceName, List<string> imageUrls)>
            {
                // Signature Facial
                ("Signature Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/27/81/a7/2781a79854d1a98a16fde4f099bcc8a5.jpg",
                    "https://i.pinimg.com/736x/2c/87/b8/2c87b81f2775b10b59767a43fe28e20f.jpg",
                    "https://i.pinimg.com/736x/5f/fd/bb/5ffdbbdd837d0e1b597bf7b55608ecf9.jpg",
                    "https://i.pinimg.com/736x/7f/86/b9/7f86b9ae476a90f6e13706c96db78e9d.jpg",
                    "https://i.pinimg.com/736x/4c/d2/e4/4cd2e442ed27ad93414300ad5ce65bf9.jpg"
                }),
                // Anti-Aging Facial
                ("Anti-Aging Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/42/82/8d/42828de778bdc6d184fde7ef8de141d7.jpg",
                    "https://i.pinimg.com/736x/33/f5/2b/33f52b7c23657d2ac0cd8d9aef3826c7.jpg",
                    "https://i.pinimg.com/736x/c5/25/bc/c525bc3ce54da328c0d5a06226603ac9.jpg",
                    "https://i.pinimg.com/736x/5d/b2/30/5db2300075fe7591f1b7ddc140fb36c0.jpg",
                    "https://i.pinimg.com/736x/28/20/90/28209092643b28bd55e12004a5158fb9.jpg"
                }),
                // Hydrating Facial
                ("Hydrating Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/52/e2/c8/52e2c875539100b6fd36dfcb81cb334b.jpg",
                    "https://i.pinimg.com/736x/5e/87/da/5e87daadb4e52e5d9d048bf52b943988.jpg",
                    "https://i.pinimg.com/736x/64/bf/86/64bf86354531fd40bbad0fe5979bbe64.jpg",
                }),
                // Brightening Facial
                ("Brightening Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/ad/bd/b1/adbdb1e65aac64f3f44cbf7b7a548b30.jpg",
                    "https://i.pinimg.com/736x/66/51/44/665144d405e77c90658a184cc69f1728.jpg",
                    "https://i.pinimg.com/736x/c3/b2/c7/c3b2c74b916a8ca4ad5a111e46d62fe1.jpg",
                    "https://i.pinimg.com/736x/95/24/b1/9524b1129e39e0c4e8336ee825469156.jpg"
                }),
                // Acne Treatment Facial
                ("Acne Treatment Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/fc/c4/6b/fcc46b80915cd02cde8c8f7d8975c01b.jpg",
                    "https://i.pinimg.com/736x/c6/14/6d/c6146dba83a0356ede4f775a46b64a34.jpg",
                    "https://i.pinimg.com/736x/33/38/e3/3338e3a12f420ba6b75ef9097acc8329.jpg",
                    "https://i.pinimg.com/736x/dc/92/9f/dc929f3ff80c4f48be5fb81809426022.jpg",
                    "https://i.pinimg.com/736x/95/24/b1/9524b1129e39e0c4e8336ee825469156.jpg"
                }),
                // Soothing Facial
                ("Soothing Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/50/72/37/5072376f6e17d3219b0404e93a8cd89b.jpg",
                    "https://i.pinimg.com/736x/d8/62/97/d862974838bb5477234ddfe9130df3cb.jpg",
                    "https://i.pinimg.com/736x/90/e8/8b/90e88bf68152266da89017591b6cbed6.jpg",
                }),
                // Green Tea Facial
                ("Green Tea Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/7a/b9/40/7ab940119bbfd56905f68154084c73f6.jpg",
                    "https://i.pinimg.com/736x/35/78/64/357864ef765a43a798742ed02ebf3766.jpg",
                    "https://i.pinimg.com/736x/4f/94/81/4f9481757db772237b2f348d3f24d47a.jpg",
                }),
                // Collagen Boost Facial
                ("Collagen Boost Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/ac/ad/30/acad30ac766eb1e68002d8e45d60d7d3.jpg",
                    "https://i.pinimg.com/736x/63/b3/01/63b30170bfe60842ac58502f52b7f250.jpg",
                    "https://i.pinimg.com/736x/5b/77/92/5b7792126b2a90abe9b4bbf1eb0ef76b.jpg",
                    "https://i.pinimg.com/736x/3f/a4/30/3fa430040c0aa63d7682ef4457b8251d.jpg"
                }),
                // Detox Facial
                ("Detox Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/26/7c/46/267c46cac2f68d850351bb84f1a05aac.jpg",
                    "https://i.pinimg.com/736x/66/51/44/665144d405e77c90658a184cc69f1728.jpg",
                    "https://i.pinimg.com/736x/4f/94/81/4f9481757db772237b2f348d3f24d47a.jpg",
                    "https://i.pinimg.com/736x/8e/83/a6/8e83a64aac39e18f75acaf0efe8f0df9.jpg"
                }),
                // Overnight Hydration Facial
                ("Overnight Hydration Facial", new List<string>
                {
                    "https://i.pinimg.com/736x/58/b1/bf/58b1bf4c47a37a4f21b99a7a0bc35a8e.jpg",
                    "https://i.pinimg.com/736x/a1/6a/4b/a16a4b0cb8492406f5712927de9e2c05.jpg",
                    "https://i.pinimg.com/736x/bb/4f/14/bb4f14b2a54b5df40aaf29964618faf0.jpg",
                    "https://i.pinimg.com/736x/4a/a3/dc/4aa3dc07ca1938573d318717c437e0a6.jpg",
                }),
                // Swedish Massage
                ("Swedish Massage", new List<string>
                {
                    "https://i.pinimg.com/736x/a4/e5/c6/a4e5c65d43fc9a6aa54dd3160b9e839d.jpg",
                    "https://www.health.com/thmb/K_Vtfnh3Yu-Ceya3aETxfH72k9Q=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/GettyImages-1175433234-034014dc5b9c45edaeaf04c7b80ceafc.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRR32fYGDt-A-868QxQbHt0O3YK7ZP6Och_7Q&s",
                }),
                // Full Body Scrub
                ("Full Body Scrub", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcShtNFeG36aaJZsLyW4Jbq2mnJQq1EUVsb4CTkCP36InrBM9-LyRiyd_YZeutVVcuF1ub0&usqp=CAU",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ6DKTthrU1lo3VIdPKVfIu0IjVY7EfpO7KocRY6xFhpMG4dWfhcGSREJ-TMDZ9mog2O-o&usqp=CAU",
                    "https://i.pinimg.com/736x/aa/fc/b5/aafcb5c41918eac7a035334e26becc05.jpg",
                }),
                // Moisturizing Body Wrap
                ("Moisturizing Body Wrap", new List<string>
                {
                    "https://i.pinimg.com/736x/d9/7a/41/d97a41c66c2b8c27413e5b8386ee6734.jpg",
                    "https://i.pinimg.com/736x/bd/78/87/bd7887c01d97a59665f04f62616f78da.jpg",
                }),
                // Aromatherapy Massage
                ("Aromatherapy Massage", new List<string>
                {
                    "https://i.pinimg.com/736x/c6/c8/c6/c6c8c65052fb8f59d3c7b495f47aace6.jpg",
                    "https://i0.wp.com/www.absolute-aromas.com/wp-content/uploads/2022/11/aromatherapy_massage_techniques.jpg?fit=1280%2C848&ssl=1",
                    "https://i.pinimg.com/736x/25/fc/d0/25fcd0f1f682b5ae41eafcc0d1397b5c.jpg",
                    "https://i.pinimg.com/736x/04/09/be/0409be31db953306c4ba2021a8a10b84.jpg",
                }),
                // Foot Massage
                ("Foot Massage", new List<string>
                {
                    "https://i.pinimg.com/736x/b3/ee/14/b3ee141b5983c783a88d7160a30e734a.jpg",
                    "https://i.pinimg.com/736x/ad/55/f8/ad55f85f17ae9b07cd6c94bd67e28754.jpg",
                    "https://i.pinimg.com/736x/55/49/69/5549692df73157f1bf2c49a8fb6daf52.jpg",
                    "https://i.pinimg.com/736x/26/d2/b9/26d2b991409c50d6baf530cbc7397b77.jpg",
                    "https://i.pinimg.com/736x/37/e9/64/37e964a2263041df8131c138f9219d79.jpg"
                }),
                // Abdominal Massage
                ("Abdominal Massage", new List<string>
                {
                    "https://images.squarespace-cdn.com/content/v1/5f2864b6ee63644ee0b157d3/1716227683804-A6SSUR28MIBPHBSSXODP/stomach+massage+for+constipation.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQFO_Je7Wo2NTv7rKUoQNB2LCWShvPvie0FSVRE6zzkQjk92ika0RruE9bJa5YZAXa74pk&usqp=CAU",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRdxvYhv5difFnJ1XoXVyNoIdxHn08KX5ZEs5Zm_oXmJkmLiZM8mQFXKP6kH1YbHICY5JE&usqp=CAU",
                    "https://i.pinimg.com/736x/4e/f2/9b/4ef29b5b0d121283f8b07cd16ba8f562.jpg",
                }),
                // Detox Body Treatment
                ("Detox Body Treatment", new List<string>
                {
                    "https://i.pinimg.com/736x/c2/e0/42/c2e0422404b022ae69d9ba98cd659748.jpg",
                    "https://i.pinimg.com/736x/62/78/07/627807715ea10ff33f2b633e86fec023.jpg",
                    "https://i.pinimg.com/736x/fa/60/1b/fa601bea62f56aeae561dbf884d4e184.jpg",
                    "https://i.pinimg.com/736x/a4/16/c7/a416c7d0d39a76dec17f7fed548e65a7.jpg",
                }),
                // Mud Bath
                ("Mud Bath", new List<string>
                {
                    "https://i.pinimg.com/736x/a0/1c/41/a01c4134bff32537d93b0db8cfe4ba52.jpg",
                    "https://i.pinimg.com/736x/e5/94/0d/e5940d85b2ab0a370a05b2e15f6689e3.jpg",
                    "https://i.pinimg.com/736x/b1/c0/6f/b1c06f280faaac6beb3f3ea3cdb60063.jpg",
                    "https://i.pinimg.com/736x/35/8b/6a/358b6a8da772fb59bea486501845148c.jpg",
                }),
                // Body Polish
                ("Body Polish", new List<string>
                {
                    "https://www.bodycraft.co.in/wp-content/uploads/side-view-woman-getting-massaged-spa-1.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRLNJVEtH6cN5JTyKFp841LIzCUFEtTwWxT6-7po0PTr1EAcdMlkooFhlseDIFTyWO_WDU&usqp=CAU",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTtfba8M0gKvn6hM6qrBlRp6HueE6c83lcXQVvr-gNKy0FbJuU-YTcl-isOPV3C6ZJniTY&usqp=CAU",
                    "https://hillsbeauty.vn/uploads/baiviet/massage-body-la-gi-massage-body-bao-gom-nhung-phuong-phap-nao-1702451817-gvy9x.jpg",
                })
            };
            // Duyệt qua danh sách các dịch vụ và thêm hình ảnh cho từng dịch vụ
            foreach (var serviceData in serviceImageData)
            {
                // Tìm dịch vụ trong cơ sở dữ liệu theo tên
                var service = services.FirstOrDefault(p => p.Name == serviceData.serviceName);

                if (service != null)
                {
                    // Lặp qua danh sách các hình ảnh và thêm vào bảng ServiceImages
                    foreach (var imageUrl in serviceData.imageUrls)
                    {
                        var serviceImage = new ServiceImages()
                        {
                            ServiceId = service.ServiceId,
                            image = imageUrl
                        };

                        // Thêm hình ảnh vào cơ sở dữ liệu
                        await _context.ServiceImages.AddAsync(serviceImage);
                    }
                }
            }

            // Lưu tất cả thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();
        }

    

    private async Task SeedBlogs()
        {
            var blogs = new List<Blog>
    {
        new Blog { Title = "How to Relax at a Spa", Content = "Learn the best ways to enjoy a relaxing spa day.", AuthorId = 1, Status = "Accept", Note = "Popular blog" },
        new Blog { Title = "Benefits of Facial Treatments", Content = "Explore the advantages of getting a facial treatment.", AuthorId = 2, Status = "Published", Note = "Informative"},
        new Blog {Title = "Top 5 Spa Treatments for Stress Relief", Content = "A guide to the most effective spa treatments for stress relief.", AuthorId = 3, Status = "Draft", Note = "Needs review"},
        new Blog {Title = "Skincare Tips for Beginners", Content = "Simple and effective skincare tips for beginners.", AuthorId = 4, Status = "Accept", Note = "Great for beginners"},
        new Blog {Title = "The Art of Aromatherapy", Content = "Discover the benefits and techniques of aromatherapy.", AuthorId = 5, Status = "Pending", Note = "Awaiting approval"},
        new Blog {Title = "How to Choose the Right Spa Package", Content = "Tips on selecting the perfect spa package for your needs.", AuthorId = 1, Status = "Accept", Note = "Customer favorite"},
        new Blog {Title = "The Importance of Self-Care", Content = "Why self-care is essential for mental and physical health.", AuthorId = 2, Status = "Accept", Note = "Motivational"},
        new Blog {Title = "Exploring Hot Stone Therapy", Content = "Everything you need to know about hot stone therapy.", AuthorId = 3, Status = "Rejected", Note = "Needs additional content"},
        new Blog {Title = "Tips for Maintaining Healthy Skin", Content = "Best practices for keeping your skin healthy and radiant.", AuthorId = 4, Status = "Accept", Note = "Well-researched"},
        new Blog {Title = "The Science Behind Spa Therapies", Content = "Understanding how spa therapies benefit your body and mind.", AuthorId = 5, Status = "Pending", Note = "Detailed insights"}
    };

            await _context.Blogs.AddRangeAsync(blogs);
            await _context.SaveChangesAsync();
        }

        private async Task SeedVoucherData()
        {
            var random = new Random();

            // Create a list of Voucher objects
            var vouchers = new List<Voucher>();

            for (int i = 0; i < 10; i++) // Create 10 vouchers as an example
            {
                var voucher = new Voucher
                {
                    Code = "VOUCHER" + random.Next(1000, 9999), // Random code like "VOUCHER1234"
                    Quantity = random.Next(50, 100), // Random quantity between 50 and 100
                    RemainQuantity = random.Next(0, 50), // Random remaining quantity (less than or equal to Quantity)
                    Status = random.NextDouble() < 0.5 ? "Active" : "Inactive", // Random status (50% chance for active or inactive)
                    Description = "Discount voucher for various products", // Example description
                    Discount = random.Next(5, 50), // Random discount between 5% and 50%
                    ValidFrom = DateTime.Now.AddDays(-random.Next(30, 60)), // Random start date (between 30 and 60 days ago)
                    ValidTo = DateTime.Now.AddDays(random.Next(30, 60)), // Random expiration date (30 to 60 days from now)
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                vouchers.Add(voucher);
            }

            // Add the vouchers to the context
            await _context.Vouchers.AddRangeAsync(vouchers);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        private async Task SeedOrderData()
        {
            var random = new Random();

            // Step 1: Seed Orders first
            var orders = new List<Order>();

            for (int i = 0; i < 10; i++) // Create 10 orders
            {
                var order = new Order
                {
                    OrderCode = random.Next(1000, 9999), // Random order code
                    CustomerId = 1, // Assume CustomerId is 1 for simplicity; adjust as needed
                    VoucherId = 1, // Assume VoucherId is 1 for simplicity; adjust as needed
                    TotalAmount = random.Next(100, 500), // Random total amount
                    OrderType = i % 2 == 0 ? OrderTypeEnum.Product.ToString() : OrderTypeEnum.Appointment.ToString(), 
                    Status = i % 2 == 0 ? OrderStatusEnum.Pending.ToString() : OrderStatusEnum.Completed.ToString(), 
                    Note = "Note " + random.Next(1, 1000),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                orders.Add(order);
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync(); // Save Orders to DB

            var customers = await _context.Users.ToListAsync(); // Assuming 'Users' is for Customers
            var staffList = await _context.Staffs.ToListAsync();
            var services = await _context.Services.ToListAsync();
            var branches = await _context.Branchs.ToListAsync();
            var products = await _context.Products.ToListAsync();
            // Step 2: Seed OrderDetails with valid OrderId
            var orderDetails = new List<OrderDetail>();
            var appointments = new List<Appointments>();
            foreach (var order in orders)
            {
                if (order.OrderType == OrderTypeEnum.Product.ToString())
                {
                    for (int i = 0; i < random.Next(1, 5); i++) // Random number of order details (1 to 5)
                    {
                        var quantity = random.Next(1, 3);
                        var unitPrice = random.Next(10, 100);
                        var randomProduct = products[random.Next(products.Count)];
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId, 
                            Quantity = quantity, // Random số lượng từ 1 đến 2
                            UnitPrice = unitPrice, // Random giá từ 10 đến 100
                            SubTotal = quantity * unitPrice,
                            ProductId = randomProduct.ProductId,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };

                        orderDetails.Add(orderDetail);
                    }
                }
                else if (order.OrderType == OrderTypeEnum.Appointment.ToString())
                {
                    for (int i = 0; i < random.Next(1, 5); i++) // Random number of order details (1 to 5)
                    {
                        var quantity = random.Next(1, 3);
                        var unitPrice = random.Next(10, 100);
                        var appointment = new Appointments
                        {
                            OrderId = order.OrderId,
                            Quantity = quantity, // Random số lượng từ 1 đến 2
                            UnitPrice = unitPrice, // Random giá từ 10 đến 100
                            SubTotal = quantity * unitPrice,
                            CustomerId = customers[random.Next(customers.Count)].UserId, // Random CustomerId
                            StaffId = staffList[random.Next(staffList.Count)].StaffId,   // Random StaffId
                            ServiceId = services[random.Next(services.Count)].ServiceId, // Random ServiceId
                            BranchId = branches[random.Next(branches.Count)].BranchId,   // Random BranchId

                            // Random thời gian hẹn trong khoảng 30 ngày tới
                            AppointmentsTime = DateTime.Now.AddDays(random.Next(1, 31)).AddHours(random.Next(8, 18)),

                            Status = random.Next(2) == 0 ? "Pending" : "Confirmed", // Ngẫu nhiên trạng thái
                            Notes = "Note " + random.Next(1, 1000), // Ghi chú ngẫu nhiên
                            Feedback = "No feedback yet", // Feedback hoặc null
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };

                        appointments.Add(appointment);
                    }
                }
                
            }

            await _context.OrderDetails.AddRangeAsync(orderDetails);
            await _context.Appointments.AddRangeAsync(appointments);
            await _context.SaveChangesAsync(); // Save OrderDetails to DB
        }

    public async Task SeedSkincareRoutines()
    {
        var skincareRoutines = new List<SkincareRoutine>
        {
            new SkincareRoutine
            {
                Name = "Oily Skin",
                Description = "Routine for managing oily skin and reducing excess sebum.",
                Steps = "Cleanse, Tone, Moisturize, Protect",
                Frequency = "Daily",
                TargetSkinTypes = "Oily Skin"
            },
            new SkincareRoutine
            {
                Name = "Dry Skin",
                Description = "Routine for hydrating and nourishing dry skin.",
                Steps = "Cleanse, Hydrate, Moisturize, Protect",
                Frequency = "Daily",
                TargetSkinTypes = "Dry Skin"
            },
            new SkincareRoutine
            {
                Name = "Neutral Skin",
                Description = "Routine for maintaining neutral skin balance.",
                Steps = "Cleanse, Tone, Moisturize, Protect",
                Frequency = "Daily",
                TargetSkinTypes = "Neutral Skin"
            },
            new SkincareRoutine
            {
                Name = "Combination Skin",
                Description = "Routine for addressing both dry and oily areas.",
                Steps = "Cleanse, Tone, Moisturize, Protect",
                Frequency = "Daily",
                TargetSkinTypes = "Combination Skin"
            },
            new SkincareRoutine
            {
                Name = "Blackheads",
                Description = "Routine to clear and prevent blackheads.",
                Steps = "Cleanse, Exfoliate, Tone, Protect",
                Frequency = "Weekly",
                TargetSkinTypes = "Blackheads"
            },
            new SkincareRoutine
            {
                Name = "Acne",
                Description = "Routine for acne-prone skin to reduce breakouts.",
                Steps = "Cleanse, Treat, Moisturize, Protect",
                Frequency = "Daily",
                TargetSkinTypes = "Acne"
            },
            new SkincareRoutine
            {
                Name = "Dark Circles",
                Description = "Routine for reducing dark circles under the eyes.",
                Steps = "Cleanse, Treat, Moisturize, Protect",
                Frequency = "Daily",
                TargetSkinTypes = "Dark Circles"
            },
            new SkincareRoutine
            {
                Name = "Closed Comedones",
                Description = "Routine to manage closed comedones.",
                Steps = "Cleanse, Exfoliate, Moisturize, Protect",
                Frequency = "Weekly",
                TargetSkinTypes = "Closed Comedones"
            },
            new SkincareRoutine
            {
                Name = "Glabella Wrinkles",
                Description = "Routine to reduce glabella wrinkles.",
                Steps = "Cleanse, Treat, Moisturize, Protect",
                Frequency = "Daily",
                TargetSkinTypes = "Glabella Wrinkles"
            }
        };

        foreach (var routine in skincareRoutines)
        {
            if (!_context.SkincareRoutines.Any(r => r.Name == routine.Name))
            {
                _context.SkincareRoutines.Add(routine);
                await _context.SaveChangesAsync();

                var routineInDb = _context.SkincareRoutines.First(r => r.Name == routine.Name);

                // Define product and service IDs
                var routineProducts = routine.Name switch
                {
                    "Oily Skin" => new[] { 3, 5, 31, 44, 45, 46 },
                    "Dry Skin" => new[] { 1, 6, 13, 24, 28, 39, 42, 48 },
                    "Neutral Skin" => new[] { 2, 7, 20, 26, 30, 33, 44, 49, 50 },
                    "Combination Skin" => new[] { 10, 11, 15, 23, 32, 34, 43, 45, 48 },
                    "Blackheads" => new[] { 3, 9, 11, 25, 34, 38, 45, 46 },
                    "Acne" => new[] { 6, 9, 14, 19, 31, 33, 40, 44, 45 },
                    "Dark Circles" => new[] { 1, 7, 24, 26, 28, 43, 44, 50 },
                    "Closed Comedones" => new[] { 5, 10, 11, 23, 31, 34, 45, 46 },
                    "Glabella Wrinkles" => new[] { 2, 6, 8, 28, 32, 41, 42, 48 },
                    _ => Array.Empty<int>()
                };

                var routineServices = routine.Name switch
                {
                    "Oily Skin" => new[] { 4, 9 },
                    "Dry Skin" => new[] { 3, 6, 10 },
                    "Neutral Skin" => new[] { 1, 5, 8 },
                    "Combination Skin" => new[] { 2, 7 },
                    "Blackheads" => new[] { 4, 5, 9 },
                    "Acne" => new[] { 5, 6, 9 },
                    "Dark Circles" => new[] { 3, 4, 8 },
                    "Closed Comedones" => new[] { 4, 7 },
                    "Glabella Wrinkles" => new[] { 2, 3, 8 },
                    _ => Array.Empty<int>()
                };

                // Add ProductRoutine
                foreach (var productId in routineProducts)
                {
                    _context.ProductRoutines.Add(new ProductRoutine
                    {
                        RoutineId = routineInDb.SkincareRoutineId,
                        ProductId = productId,
                        Status = "Active"
                    });
                }

                // Add ServiceRoutine
                foreach (var serviceId in routineServices)
                {
                    _context.ServiceRoutine.Add(new ServiceRoutine
                    {
                        RoutineId = routineInDb.SkincareRoutineId,
                        ServiceId = serviceId,
                        Status = "Active"
                    });
                }

                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task SeedBedTypes()
{
    var bedTypes = new List<BedType>
    {
        new BedType { Name = "Standard Bed", Description = "A basic bed for standard use.", Thumbnail = "standard_bed.jpg" },
        new BedType { Name = "Deluxe Bed", Description = "A luxurious bed for enhanced comfort.", Thumbnail = "deluxe_bed.jpg" },
        new BedType { Name = "Hydrotherapy Bed", Description = "A bed designed for hydrotherapy treatments.", Thumbnail = "hydrotherapy_bed.jpg" },
        new BedType { Name = "Massage Bed", Description = "A specialized bed for massage services.", Thumbnail = "massage_bed.jpg" },
        new BedType { Name = "Facial Bed", Description = "A comfortable bed for facial treatments.", Thumbnail = "facial_bed.jpg" }
    };

    await _context.BedType.AddRangeAsync(bedTypes);
    await _context.SaveChangesAsync();
}

    private async Task SeedRooms()
    {
        var rooms = new List<Room>();

        // Duyệt qua mỗi chi nhánh (BranchId từ 1 đến 5)
        for (int branchId = 1; branchId <= 5; branchId++)
        {
            // Tạo 10 phòng cho mỗi chi nhánh
            for (int roomNumber = 1; roomNumber < 10; roomNumber++)
            {
                rooms.Add(new Room
                {
                    Name = $"Room {branchId}0{roomNumber}", // Tên phòng, ví dụ: Room 101, Room 201
                    Description = $"Room {branchId}0{roomNumber} is a comfortable and relaxing space.",
                    Thumbnail = $"room_{branchId}0{roomNumber}.jpg", // Tên file hình ảnh
                    Status = ObjectStatus.Active.ToString(),
                    BranchId = branchId
                });
            }
        }

        // Thêm danh sách phòng vào database
        await _context.Room.AddRangeAsync(rooms);
        await _context.SaveChangesAsync();
    }
    
    private async Task SeedBeds()
    {
        var rooms = await _context.Room.ToListAsync();
        var bedTypes = await _context.BedType.ToListAsync();

        var beds = new List<Bed>();
        var random = new Random();

        foreach (var room in rooms)
        {
            for (int i = 0; i < 5; i++) // Mỗi phòng có 5 giường
            {
                var bedType = bedTypes[random.Next(bedTypes.Count)]; // Chọn ngẫu nhiên BedType

                beds.Add(new Bed
                {
                    Name = $"{room.Name} - Bed {i + 1}",
                    Description = $"{bedType.Name} in {room.Name}.",
                    Thumbnail = bedType.Thumbnail,
                    RoomId = room.RoomId,
                    Status = ObjectStatus.Active.ToString(),
                    BedTypeId = bedType.BedTypeId
                });
            }
        }

        await _context.Bed.AddRangeAsync(beds);
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