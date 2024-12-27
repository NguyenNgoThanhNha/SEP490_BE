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

                if (!_context.Appointments.Any())
                {
                    await SeedAppointments();
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
                    SkinTypeSuitable = "All",
                    Status = "Active", 
                    ImageUrl = "https://example.com/facial-treatment.jpg"
                },
                new Category
                {
                    Name = "Toner", 
                    Description = "Cân bằng độ pH cho da, giúp da mềm mại và sẵn sàng hấp thụ dưỡng chất", 
                    SkinTypeSuitable = "All",
                    Status = "Active", 
                    ImageUrl = "https://example.com/anti-aging.jpg"
                },
                new Category
                {
                    Name = "Serum", 
                    Description = "Tinh chất cô đặc giúp điều trị các vấn đề về da như mụn và thâm", 
                    SkinTypeSuitable = "Oily, Combination",
                    Status = "Active", 
                    ImageUrl = "https://example.com/acne-treatment.jpg"
                },
                new Category
                {
                    Name = "Moisturizer", 
                    Description = "Kem dưỡng ẩm giúp cung cấp độ ẩm cần thiết cho da", 
                    SkinTypeSuitable = "All",
                    Status = "Active", 
                    ImageUrl = "https://example.com/whitening-therapy.jpg"
                },
                new Category
                {
                    Name = "Sun Cream", 
                    Description = "Kem chống nắng bảo vệ da khỏi tác hại của tia UV", 
                    SkinTypeSuitable = "All", 
                    Status = "Active",
                    ImageUrl = "https://example.com/skin-detox.jpg"
                },
                new Category
                {
                    Name = "Mask", 
                    Description = "Mặt nạ dưỡng da giúp cung cấp dưỡng chất và độ ẩm sâu", 
                    SkinTypeSuitable = "Dry, Normal",
                    Status = "Active", 
                    ImageUrl = "https://example.com/moisturizing.jpg"
                },
                new Category
                {
                    Name = "Exfoliants", 
                    Description = "Tẩy tế bào chết, làm sạch lỗ chân lông và cải thiện kết cấu da", 
                    SkinTypeSuitable = "All",
                    Status = "Active", 
                    ImageUrl = "https://example.com/eye-treatment.jpg"
                },
                new Category
                {
                    Name = "Body", 
                    Description = "Sản phẩm chăm sóc cơ thể giúp da mịn màng và săn chắc", 
                    SkinTypeSuitable = "All",
                    Status = "Active", 
                    ImageUrl = "https://example.com/lifting-firming.jpg"
                },
                new Category
                {
                    Name = "Shampoo", 
                    Description = "Dầu gội giúp làm sạch tóc và da đầu", 
                    SkinTypeSuitable = "All", 
                    Status = "Active",
                    ImageUrl = "https://example.com/body-massage.jpg"
                },
                new Category
                {
                    Name = "Conditioner", 
                    Description = "Dầu xả giúp tóc mềm mượt và chắc khỏe", 
                    SkinTypeSuitable = "All",
                    Status = "Active", 
                    ImageUrl = "https://example.com/hot-stone-therapy.jpg"
                },
            };

            // Thêm các danh mục vào cơ sở dữ liệu
            await _context.Categorys.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProducts()
        {
            var products = new List<Product>
            {
                new Product
                {
                    Status = "Active",
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
                    Status = "Active", Name = "Basic Facial", Description = "Dịch vụ làm sạch và dưỡng da mặt cơ bản",
                    Price = 300_000m, Duration = "60 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Advanced Facial", Description = "Liệu pháp dưỡng da mặt chuyên sâu",
                    Price = 500_000m, Duration = "90 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Acne Treatment", Description = "Điều trị mụn và phục hồi da",
                    Price = 400_000m, Duration = "75 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Hydration Therapy", Description = "Liệu pháp cấp ẩm sâu cho da",
                    Price = 450_000m, Duration = "80 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Anti-Aging Facial", Description = "Liệu pháp chống lão hóa da",
                    Price = 600_000m, Duration = "90 phút",                 },

                // Các dịch vụ chăm sóc cơ thể
                new Service
                {
                    Status = "Active", Name = "Body Scrub", Description = "Tẩy tế bào chết toàn thân", Price = 350_000m,
                    Duration = "60 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Body Wrap", Description = "Liệu pháp quấn nóng toàn thân",
                    Price = 500_000m, Duration = "75 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Aromatherapy Massage", Description = "Massage với tinh dầu thư giãn",
                    Price = 400_000m, Duration = "70 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Hot Stone Massage", Description = "Massage bằng đá nóng",
                    Price = 600_000m, Duration = "90 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Swedish Massage", Description = "Massage kiểu Thụy Điển thư giãn",
                    Price = 450_000m, Duration = "80 phút",                 },

                // Các dịch vụ chăm sóc móng
                new Service
                {
                    Status = "Active", Name = "Classic Manicure", Description = "Dịch vụ làm móng tay cơ bản",
                    Price = 200_000m, Duration = "45 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Gel Manicure", Description = "Làm móng tay với sơn gel",
                    Price = 300_000m, Duration = "60 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Classic Pedicure", Description = "Dịch vụ làm móng chân cơ bản",
                    Price = 250_000m, Duration = "50 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Spa Pedicure", Description = "Chăm sóc móng chân và massage chân",
                    Price = 350_000m, Duration = "75 phút",                 },
                new Service
                {
                    Status = "Active", Name = "Nail Art Design", Description = "Trang trí móng nghệ thuật",
                    Price = 200_000m, Duration = "45 phút",                 },

                // Các dịch vụ chăm sóc tóc
                new Service
                {
                    Status = "Active", Name = "Hair Wash & Blow Dry", Description = "Gội và sấy tạo kiểu tóc",
                    Price = 150_000m, Duration = "40 phút", 
                },
                new Service
                {
                    Status = "Active", Name = "Hair Treatment", Description = "Dưỡng và phục hồi tóc hư tổn",
                    Price = 300_000m, Duration = "60 phút", 
                },
                new Service
                {
                    Status = "Active", Name = "Hair Cut", Description = "Cắt tóc và tạo kiểu", Price = 200_000m,
                    Duration = "45 phút", 
                },
                new Service
                {
                    Status = "Active", Name = "Hair Color", Description = "Nhuộm tóc theo màu yêu thích",
                    Price = 400_000m, Duration = "90 phút", 
                },
                new Service
                {
                    Status = "Active", Name = "Keratin Treatment", Description = "Phục hồi tóc bằng liệu pháp keratin",
                    Price = 500_000m, Duration = "100 phút", 
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





        private async Task SeedAppointments()
        {
            var random = new Random();

            // Lấy dữ liệu từ các bảng liên quan
            var customers = await _context.Users.ToListAsync(); // Assuming 'Users' is for Customers
            var staffList = await _context.Staffs.ToListAsync();
            var services = await _context.Services.ToListAsync();
            var branches = await _context.Branchs.ToListAsync();

            var appointments = new List<Appointments>();

            // Tạo 100 - 200 cuộc hẹn ngẫu nhiên
            int appointmentCount = random.Next(100, 201);

            for (int i = 0; i < appointmentCount; i++)
            {
                var appointment = new Appointments
                {
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

            // Thêm vào cơ sở dữ liệu
            try
            {
                await _context.Appointments.AddRangeAsync(appointments);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }

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
                    Status = random.NextDouble() < 0.5 ? "Pending" : "Completed", // Random status
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                orders.Add(order);
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync(); // Save Orders to DB

            // Step 2: Seed OrderDetails with valid OrderId
            var orderDetails = new List<OrderDetail>();

            foreach (var order in orders)
            {
                for (int i = 0; i < random.Next(1, 5); i++) // Random number of order details (1 to 5)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId, // Use valid OrderId
                        ProductId = random.Next(1, 10), // Random ProductId; adjust as needed
                        ServiceId = random.Next(1, 10), // Random ServiceId; adjust as needed
                        Quantity = random.Next(1, 3), // Random quantity between 1 and 2
                        Price = random.Next(10, 100), // Random price between 10 and 100
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    orderDetails.Add(orderDetail);
                }
            }

            await _context.OrderDetails.AddRangeAsync(orderDetails);
            await _context.SaveChangesAsync(); // Save OrderDetails to DB
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