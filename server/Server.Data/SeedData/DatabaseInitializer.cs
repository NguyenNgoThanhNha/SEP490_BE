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

                // Staff Role
                if (!_context.StaffRole.Any())
                {
                    await SeedStaffRole();
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

                if (!_context.SkinCareRoutineStep.Any())
                {
                    await SeedSkincareRoutineSteps();
                }

                if (!_context.Shifts.Any())
                {
                    await SeedShifts();
                }

                if (!_context.WorkSchedule.Any())
                {
                    await SeedWorkSchedules();
                }

                if (!_context.Staff_ServiceCategory.Any())
                {
                    await SeedStaffServiceCategory();
                }

                if (!_context.SkinConcern.Any())
                {
                    await SeedConcerns();
                }

                if (!_context.SkincareRoutineConcern.Any())
                {
                    await SeedSkincareRoutineConcerns();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Seed các user role
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

        // Seed các staff role
        private async Task SeedStaffRole()
        {
            var cashierRole = new StaffRole
            {
                StaffRoleName = "Cashier",
                Description = "Chịu trách nhiệm xử lý thanh toán và hóa đơn"
            };

            var specialistRole = new StaffRole
            {
                StaffRoleName = "Specialist",
                Description = "Cung cấp dịch vụ chuyên biệt cho khách hàng"
            };

            var defaultStaffRole = new StaffRole
            {
                StaffRoleName = "DefaultStaff",
                Description = "Nhân viên mặc định, có thể hỗ trợ các nhiệm vụ chung"
            };

            List<StaffRole> staffRoles = new()
            {
                cashierRole,
                specialistRole,
                defaultStaffRole
            };

            var existingRoles = await _context.StaffRole
                .Where(r => staffRoles.Select(sr => sr.StaffRoleName).Contains(r.StaffRoleName))
                .Select(r => r.StaffRoleName)
                .ToListAsync();

            var newRoles = staffRoles.Where(sr => !existingRoles.Contains(sr.StaffRoleName)).ToList();

            if (newRoles.Any()) // Chỉ thêm nếu có role mới
            {
                await _context.StaffRole.AddRangeAsync(newRoles);
                await _context.SaveChangesAsync();
            }
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
            for (int i = 1; i <= 55; i++)
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

        private async Task SeedShifts()
        {
            var shifts = new List<Shifts>
            {
                new Shifts
                {
                    ShiftName = "Ca sáng", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(13, 0, 0)
                },
                new Shifts
                {
                    ShiftName = "Ca chiều", StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(17, 0, 0)
                },
                new Shifts
                {
                    ShiftName = "Ca tối", StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(21, 0, 0)
                }
            };

            if (!_context.Shifts.Any()) // Kiểm tra nếu chưa có dữ liệu
            {
                await _context.Shifts.AddRangeAsync(shifts);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedWorkSchedules()
        {
            var staffs = await _context.Staffs.ToListAsync();
            var shifts = await _context.Shifts.ToListAsync();

            if (!staffs.Any() || !shifts.Any())
            {
                throw new Exception("Nhân viên hoặc Ca làm việc bị thiếu. Vui lòng thêm họ vào trước.");
            }

            var scheduleList = new List<WorkSchedule>();
            var random = new Random();

            var startDate = new DateTime(2025, 4, 2); // Bắt đầu từ tuần đầu tiên của tháng (Thứ 2)
            var endDate = new DateTime(2025, 4, 30); // Kết thúc vào thứ 7 của tuần cuối cùng

            foreach (var staff in staffs)
            {
                // Chọn một ca ngẫu nhiên cho nhân viên (cố định suốt tháng)
                var assignedShift = shifts[random.Next(shifts.Count)];

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    scheduleList.Add(new WorkSchedule
                    {
                        StaffId = staff.StaffId,
                        ShiftId = assignedShift.ShiftId,
                        DayOfWeek = (int)date.DayOfWeek, // Chuyển thành số (Monday = 1, ..., Saturday = 6)
                        WorkDate = date, // Ngày làm việc cụ thể
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                }
            }

            await _context.WorkSchedule.AddRangeAsync(scheduleList);
            await _context.SaveChangesAsync();
        }


        private async Task SeedCompany()
        {
            var company = new Company
            {
                Name = "ABC Corporation",
                Address = "123 Main St, HCM",
                Description = "Nhà cung cấp giải pháp công nghệ hàng đầu",
                PhoneNumber = "0123456789",
                Email = "contact@abccorp.com",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // Thêm công ty vào cơ sở dữ liệu
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
        }

        //Làm từ đây
        private async Task SeedCategories()
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Sữa rửa mặt",
                    Description = "Sữa rửa mặt, loại bỏ bụi bẩn và dầu thừa",
                    Status = "Active",
                    ImageUrl = "https://example.com/facial-treatment.jpg"
                },
                new Category
                {
                    Name = "Nước cân bằng da",
                    Description = "Cân bằng độ pH của da, giúp da mềm mại và sẵn sàng hấp thụ dưỡng chất",
                    Status = "Active",
                    ImageUrl = "https://example.com/anti-aging.jpg"
                },
                new Category
                {
                    Name = "Serum",
                    Description = "Tinh chất cô đặc giúp điều trị các vấn đề về da như mụn và vết thâm",
                    Status = "Active",
                    ImageUrl = "https://example.com/acne-treatment.jpg"
                },
                new Category
                {
                    Name = "Kem dưỡng ẩm",
                    Description = "Kem dưỡng ẩm giúp cung cấp độ ẩm cần thiết cho da",
                    Status = "Active",
                    ImageUrl = "https://example.com/whitening-therapy.jpg"
                },
                new Category
                {
                    Name = "Kem chống nắng",
                    Description = "Kem chống nắng bảo vệ da khỏi tia UV có hại",
                    Status = "Active",
                    ImageUrl = "https://example.com/skin-detox.jpg"
                },
                new Category
                {
                    Name = "Mặt nạ",
                    Description = "Mặt nạ chăm sóc da giúp cung cấp dưỡng chất sâu và độ ẩm",
                    Status = "Active",
                    ImageUrl = "https://example.com/moisturizing.jpg"
                },
                new Category
                {
                    Name = "Tẩy tế bào chết",
                    Description = "Tẩy tế bào chết, thông thoáng lỗ chân lông và cải thiện kết cấu da",

                    Status = "Active",
                    ImageUrl = "https://example.com/eye-treatment.jpg"
                },
                new Category
                {
                    Name = "Chăm sóc cơ thể",
                    Description = "Sản phẩm chăm sóc cơ thể cho làn da mịn màng và săn chắc",

                    Status = "Active",
                    ImageUrl = "https://example.com/lifting-firming.jpg"
                },
                new Category
                {
                    Name = "Dầu gội đầu",
                    Description = "Dầu gội giúp làm sạch tóc và da đầu",

                    Status = "Active",
                    ImageUrl = "https://example.com/body-massage.jpg"
                },
                new Category
                {
                    Name = "Dầu xả",
                    Description = "Dầu xả cho tóc mềm mại, mượt mà và chắc khỏe",
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
                    Name = "Chăm sóc da mặt",
                    Description = "Dịch vụ chăm sóc da mặt chuyên sâu, làm sạch và tái tạo da",
                    Status = "Active",
                    Thumbnail = "https://example.com/facial-treatment.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Liệu pháp massage",
                    Description = "Liệu pháp massage thư giãn, giảm căng thẳng và mệt mỏi",
                    Status = "Active",
                    Thumbnail = "https://example.com/massage-therapy.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Tẩy tế bào chết toàn thân",
                    Description = "Dịch vụ tẩy tế bào chết toàn thân giúp da sáng mịn",
                    Status = "Active",
                    Thumbnail = "https://example.com/body-scrub.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Chăm sóc tóc",
                    Description = "Dịch vụ chăm sóc tóc chuyên nghiệp, phục hồi tóc hư tổn và nuôi dưỡng tóc",
                    Status = "Active",
                    Thumbnail = "https://example.com/hair-treatment.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Chăm sóc móng tay & chân",
                    Description = "Dịch vụ làm móng chuyên nghiệp, giúp bạn có bộ móng đẹp và khỏe",
                    Status = "Active",
                    Thumbnail = "https://example.com/manicure-pedicure.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Tẩy lông & triệt lông",
                    Description = "Dịch vụ triệt lông chuyên nghiệp, an toàn và hiệu quả",
                    Status = "Active",
                    Thumbnail = "https://example.com/waxing-hair-removal.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Điều trị chống lão hóa",
                    Description = "Liệu trình chống lão hóa, giúp giảm nếp nhăn và trẻ hóa làn da",
                    Status = "Active",
                    Thumbnail = "https://example.com/anti-aging-treatment.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Liệu pháp tinh dầu",
                    Description = "Liệu pháp sử dụng tinh dầu thiên nhiên giúp thư giãn và cân bằng cơ thể",
                    Status = "Active",
                    Thumbnail = "https://example.com/aromatherapy.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
// Thêm 2 danh mục mới
                new ServiceCategory
                {
                    Name = "Tạo kiểu tóc",
                    Description = "Dịch vụ tạo kiểu tóc, giúp bạn có mái tóc hoàn hảo và thời trang",
                    Status = "Active",
                    Thumbnail = "https://example.com/hair-styling.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Trang trí móng nghệ thuật",
                    Description = "Dịch vụ trang trí móng nghệ thuật, tạo nên vẻ đẹp độc đáo cho bộ móng của bạn",
                    Status = "Active",
                    Thumbnail = "https://example.com/nail-art.jpg",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                }
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
                    ProductName = "Kem Dầu Tẩy Trang Remedy",
                    ProductDescription =
                        "Sữa rửa mặt dịu nhẹ với kết cấu chuyển đổi từ kem sang dầu. Công thức chứa dầu Marula, lý tưởng cho da nhạy cảm, dễ bị mẩn đỏ.",
                    Price = 1_050_000m,
                    Quantity = 100,
                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml"
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Essential Face Wash",
                    ProductDescription =
                        "A creamy cleanser that gently removes make-up while restoring a natural glow to the skin.",
                    Price = 990_000m, // Giá sản phẩm là 990.000 VND
                    Quantity = 100,
                    CategoryId = 1, // Cleanser
                    CompanyId = 1,
                    Brand = "xxx", // Comfortzone
                    Dimension = "150ml"
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Sữa Rửa Mặt Cơ Bản",
                    ProductDescription =
                        "Sữa rửa mặt dạng kem nhẹ nhàng làm sạch lớp trang điểm và giúp da sáng tự nhiên.",
                    Price = 990_000m,
                    Quantity = 100,
                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Gel Rửa Mặt Làm Sạch Sâu",
                    ProductDescription =
                        "Chứa 3% gluconolactone giúp làm sạch da sâu, tẩy tế bào chết và thông thoáng lỗ chân lông. Phù hợp cho da dầu, dễ bị mụn.",
                    Price = 1_120_000m,
                    Quantity = 100,
                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Sữa Rửa Mặt Làm Sạch Mụn",
                    ProductDescription =
                        "Sữa rửa mặt tạo bọt giúp làm sạch da và giảm dấu hiệu lão hóa. Chứa Acid Salicylic giúp loại bỏ tế bào chết và thông thoáng lỗ chân lông.",
                    Price = 1_400_000m,
                    Quantity = 100,
                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml"
                },

                new Product
                {
                    Status = "Active",
                    ProductName = "Dầu Tẩy Trang Prebiotic Micellar",
                    ProductDescription =
                        "Dầu tẩy trang giúp hòa tan lớp trang điểm, bụi bẩn và dầu thừa, mang lại làn da sạch khỏe mà không gây khô da.",
                    Price = 1_630_000m,
                    Quantity = 100,
                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml"
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Sữa Rửa Mặt Kombucha",
                    ProductDescription =
                        "Giúp làm sạch và cân bằng làn da nhờ công thức chứa kombucha, gừng, trà trắng và hoa nhài.",
                    Price = 1_170_000m,
                    Quantity = 100,
                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml"
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Sữa Rửa Mặt Tẩy Tế Bào Chết Monoi",
                    ProductDescription =
                        "Sữa rửa mặt kết hợp tẩy tế bào chết nhẹ nhàng, chứa dầu Monoi và thành phần tự nhiên giúp da sáng khỏe, mịn màng.",
                    Price = 1_200_000m,
                    Quantity = 100,
                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Rửa Mặt Ngừa Mụn",
                    ProductDescription =
                        "Sữa rửa mặt tạo bọt giúp ngăn ngừa mụn, làm sạch da nhờ Acid Salicylic và chiết xuất thảo dược tự nhiên.",
                    Price = 1_250_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Rửa Mặt Cỏ Chanh",
                    ProductDescription =
                        "Chứa dầu ô liu, hạt lanh và cỏ chanh giúp làm sạch da nhẹ nhàng, đồng thời làm dịu và dưỡng ẩm.",
                    Price = 1_450_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Rửa Mặt Tẩy Tế Bào Chết Than Hoạt Tính",
                    ProductDescription =
                        "Được pha chế với than hoạt tính, đá quý malachite và matcha xanh, sữa rửa mặt thanh lọc mạnh mẽ này chuyển từ dạng gel sang dạng bọt tẩy tế bào chết, giúp loại bỏ tạp chất và mang lại làn da cân bằng.",
                    Price = 1_490_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Rửa Mặt Beplain Mung Bean pH-Balanced",
                    ProductDescription =
                        "Sữa rửa mặt dịu nhẹ hàng ngày với 33% chiết xuất đậu xanh giúp làm sạch tạp chất mà vẫn giữ ẩm và mang lại cảm giác thoải mái cho da.",
                    Price = 355_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Rửa Mặt ISNTREE Yam Root Vegan Milk",
                    ProductDescription =
                        "Sản phẩm chăm sóc da dịu nhẹ và nuôi dưỡng, phù hợp với mọi loại da. Chứa chiết xuất từ rễ khoai mỡ Andong giúp loại bỏ tạp chất, bụi bẩn mà vẫn duy trì độ ẩm cần thiết. Công thức giàu axit amin giúp làm dịu da và tạo hàng rào bảo vệ, giữ lại độ ẩm quan trọng.",
                    Price = 450_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "220ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Gel Rửa Mặt Trị Mụn Normaderm",
                    ProductDescription =
                        "Không giống như hầu hết các loại sữa rửa mặt trị mụn khác làm mất đi độ ẩm tự nhiên của da, gel rửa mặt của Vichy không chứa dầu và xà phòng, nhẹ nhàng và không gây kích ứng. Công thức đặc biệt chứa các thành phần hoạt tính giúp giảm mụn, kiểm soát dầu nhờn và củng cố hàng rào bảo vệ da.",
                    Price = 720_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Gel Rửa Mặt Tươi Mát Purete Thermale",
                    ProductDescription =
                        "Sữa rửa mặt tạo bọt dày đặc giúp làm sạch hiệu quả bụi bẩn, lớp trang điểm và ô nhiễm khỏi da, đồng thời chống lại tác hại của nước cứng. Mang lại làn da mềm mại, tươi mới mà không gây cảm giác khô căng.",
                    Price = 600_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem Rửa Mặt Tạo Bọt Purete Thermale",
                    ProductDescription =
                        "Sữa rửa mặt dạng kem tạo bọt giúp làm sạch hiệu quả tạp chất, lớp trang điểm và ô nhiễm trên da mà không gây khô hay kích ứng. Đồng thời, sản phẩm còn giúp giảm tác động có hại của nước cứng lên da, mang lại cảm giác mềm mịn và tươi mới mà không bị căng da.",
                    Price = 650_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "125ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Rửa Mặt Tạo Bọt",
                    ProductDescription =
                        "Công thức kem sang trọng giúp làm sạch và nhẹ nhàng tẩy tế bào chết, đồng thời cấp ẩm sâu với sự kết hợp của các axit amin. Nhờ sự kết hợp mạnh mẽ giữa axit tranexamic và niacinamide, sản phẩm giúp làm sáng da, mang lại làn da rạng rỡ và đều màu hơn.",
                    Price = 1_170_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "120ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Rửa Mặt Tẩy Tế Bào Chết",
                    ProductDescription =
                        "Chứa các peptide giúp tăng cường collagen và giảm nếp nhăn, sữa rửa mặt này mang lại cảm giác tươi mới tức thì với lớp bọt mịn giàu dưỡng chất. Thành phần hạt jojoba thân thiện với môi trường giúp làm sạch mà không làm mất đi độ ẩm tự nhiên của da.",
                    Price = 1_250_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Gel Rửa Mặt Làm Sạch",
                    ProductDescription =
                        "Sản phẩm giúp loại bỏ lớp trang điểm, bụi bẩn và dầu thừa một cách hiệu quả nhờ sự kết hợp nhẹ nhàng của các peptide, chất chống oxy hóa và thành phần làm dịu da. Mang lại cảm giác sạch thoáng và tươi mát.",
                    Price = 1_250_000m,
                    Quantity = 100,

                    CategoryId = 1,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Xịt Khoáng Măng Cụt",
                    ProductDescription =
                        "Hòa mình vào thiên nhiên với từng tia xịt khoáng tươi mát. Sự kết hợp giữa măng cụt giàu chất chống oxy hóa, ribose tăng cường năng lượng và cỏ ba lá đỏ giúp se khít lỗ chân lông và làm mới làn da.",
                    Price = 1_120_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "125ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Tinh Chất Dứa",
                    ProductDescription =
                        "Làm mới làn da với nước hoa hồng chiết xuất từ dứa! Công thức chứa PHA, bromelain và axit tranexamic giúp loại bỏ da xỉn màu mà không gây kích ứng. Sản phẩm giúp tẩy tế bào chết nhẹ nhàng, làm sáng và cấp ẩm cho da, phù hợp cho mọi loại da và có thể sử dụng hàng ngày.",
                    Price = 1_400_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "120ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Táo Gai",
                    ProductDescription =
                        "Dành cho da mất nước, dễ kích ứng và nhạy cảm, nước hoa hồng từ táo gai giúp da trông tươi mới hơn. Thành phần từ hoa cúc, kinh giới và dầu khuynh diệp giúp bảo vệ và làm dịu da, trong khi chiết xuất cà rốt giàu vitamin giúp phục hồi và nuôi dưỡng làn da.",
                    Price = 1_200_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Tinh Chất Chanh",
                    ProductDescription =
                        "Dành cho mọi loại da, đặc biệt là da dầu, nước hoa hồng này giàu vitamin C giúp cân bằng và làm sáng da. Chiết xuất chanh tươi cùng công thức Biocomplex2™ độc quyền mang đến làn da tươi mới và căng tràn sức sống.",
                    Price = 990_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "120ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Hoa Cúc Làm Dịu Da",
                    ProductDescription =
                        "Nước hoa hồng làm dịu da với các loại thảo mộc giúp khôi phục độ cân bằng. Hoa cúc, rễ cây comfrey, cam thảo và lô hội giúp làm dịu và cấp ẩm, trong khi natri bicarbonate giúp trung hòa và bảo vệ da.",
                    Price = 990_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "120ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Đặc Trị Mụn",
                    ProductDescription =
                        "Nước hoa hồng dưỡng ẩm nhẹ giúp làm dịu và tươi mát làn da. Công thức chứa arnica, bạc hà và hoa oải hương giúp làm dịu kích ứng, đồng thời cung cấp độ ẩm cần thiết. Hoàn hảo để cấp nước sau khi rửa mặt hoặc bất cứ khi nào da cần làm mới.",
                    Price = 1_510_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Xịt Khoáng Chống Oxy Hóa",
                    ProductDescription =
                        "Nước cân bằng chống oxy hóa giúp làm săn chắc và cấp ẩm. Công thức dạng xịt tiện lợi bổ sung hàng rào bảo vệ da bằng cách tạo lớp màng chống oxy hóa, giúp chống lại tác hại của các gốc tự do và ngăn ngừa dấu hiệu lão hóa do các sản phẩm cuối của glycation (AGEs) gây ra. Chiết xuất đậu Hà Lan giúp làm săn chắc da, trong khi chiết xuất hoa hồng và đinh hương mang lại cảm giác dễ chịu và tươi mát, lý tưởng để sử dụng sau khi rửa mặt hoặc suốt cả ngày.",
                    Price = 1_640_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Xịt Dưỡng Ẩm UltraCalming",
                    ProductDescription =
                        "Xịt dưỡng ẩm nhẹ dịu giúp làm dịu da và giảm đỏ. Sử dụng sau khi rửa mặt để khóa ẩm và chuẩn bị da cho các bước dưỡng với dòng sản phẩm UltraCalming. Công thức nhẹ hấp thụ nhanh, hỗ trợ hàng rào bảo vệ da, giúp giảm kích ứng trong tương lai. Phức hợp UltraCalming độc quyền chứa chiết xuất yến mạch và thực vật giúp làm dịu và củng cố da, trong khi lô hội giúp cân bằng độ ẩm tự nhiên.",
                    Price = 1_510_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "177ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Xịt Dưỡng Ẩm Hyaluronic Ceramide",
                    ProductDescription =
                        "Xịt dưỡng ẩm giúp bão hòa làn da với độ ẩm và khóa chặt nước để da trở nên đàn hồi. Công thức giàu dưỡng chất chứa bốn loại Axit Hyaluronic giúp da giữ nước lâu dài. Ceramide kết hợp cùng Axit Hyaluronic giúp làm mịn các nếp nhăn và tăng cường hàng rào bảo vệ da. Nước hoa hồng giàu polyphenol và flavonoid chống oxy hóa giúp phục hồi và làm mới làn da.",
                    Price = 1_800_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Làm Dịu Da",
                    ProductDescription =
                        "Xịt khoáng hoa hồng giúp củng cố và làm dịu da, đặc biệt phù hợp với làn da nhạy cảm và dễ bị đỏ.",
                    Price = 990_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Cân Bằng Da",
                    ProductDescription =
                        "Nước hoa hồng không cồn giúp cân bằng độ ẩm và hỗ trợ tái tạo mô, lý tưởng để hoàn thiện bước làm sạch da.",
                    Price = 836_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Làm Sạch Sâu",
                    ProductDescription =
                        "Nước hoa hồng giúp làm mịn da với 3% gluconolactone, có tác dụng tẩy tế bào chết nhẹ nhàng.",
                    Price = 990_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Tái Tạo Da",
                    ProductDescription =
                        "Lấy cảm hứng từ ánh sáng phản chiếu trên đại dương, nước hoa hồng tái tạo da này được thiết kế để mang lại độ rạng rỡ và sáng mịn tối đa.",
                    Price = 1_320_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng Cân Bằng pH Acwell Licorice",
                    ProductDescription =
                        "Nước hoa hồng Acwell Licorice có độ pH 5.5 giúp cân bằng da hiệu quả. Chứa chiết xuất mẫu đơn và hàm lượng cao nước cam thảo giúp làm sáng da tự nhiên. Chiết xuất trà xanh giúp làm dịu và giảm thâm nám, bao gồm cả sẹo mụn và đốm nâu. Sau khi sử dụng, da cảm thấy sạch và mềm mại, không khô căng. Giúp các sản phẩm dưỡng da thẩm thấu tốt hơn.",
                    Price = 460_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Hoa Hồng COSRX AHA/BHA",
                    ProductDescription =
                        "Công thức chứa AHA + BHA + chiết xuất thực vật giúp cải thiện kết cấu da, tăng độ đàn hồi và kiểm soát lỗ chân lông. Loại bỏ tạp chất, tẩy tế bào chết và dưỡng ẩm chỉ trong một bước.",
                    Price = 355_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Cân Bằng Nhân Sâm Sulwhasoo",
                    ProductDescription =
                        "Nước cân bằng cao cấp giúp cải thiện nếp nhăn, tăng độ đàn hồi và cấp ẩm sâu, mang lại làn da trẻ trung.",
                    Price = 3_450_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Toner Dưỡng Da Trước Điều Trị",
                    ProductDescription =
                        "Chứa chất chống oxy hóa mạnh và peptide giúp thư giãn nếp nhăn và hỗ trợ sản xuất collagen, toner điều trị chống lão hóa này cân bằng độ ẩm cho da, đồng thời làm sáng và cải thiện kết cấu, tông da.",
                    Price = 1_120_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Nước Cân Bằng Hydraflora",
                    ProductDescription =
                        "Tinh chất này được pha chế với phức hợp pre- và probiotic giúp cân bằng và củng cố hệ vi sinh trên bề mặt da. Hỗn hợp chiết xuất thực vật giàu chất chống oxy hóa giúp làm sáng và bảo vệ da khỏi tác hại của các gốc tự do. Nước dừa và cây thùa xanh giúp thu nhỏ lỗ chân lông và mang lại làn da mịn màng.",
                    Price = 1_750_000m,
                    Quantity = 100,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "120ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Miếng Lót Toner Làm Sạch",
                    ProductDescription =
                        "Chứa các thành phần tẩy tế bào chết, chiết xuất kháng khuẩn và peptide tái tạo mạnh mẽ, miếng lót toner thấm sẵn giúp làm sạch da khỏi mụn, làm sáng da, làm mịn kết cấu da không đều và giảm viêm.",
                    Price = 1_250_000m,
                    Quantity = 60,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60 miếng",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Toner Vichy Aqualia Thermal Dưỡng Ẩm Tươi Mát",
                    ProductDescription =
                        "Toner 200ml dành cho da hỗn hợp và da dầu giúp loại bỏ bụi bẩn như nam châm, nuôi dưỡng da mềm mịn và săn chắc, đồng thời cân bằng độ pH để kem dưỡng dễ dàng thẩm thấu hơn.",
                    Price = 690_000m,
                    Quantity = 200,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Toner Vichy Normaderm Se Khít Lỗ Chân Lông",
                    ProductDescription =
                        "Nước cân bằng giúp làm sạch lỗ chân lông, giảm kích thước lỗ chân lông và kiểm soát dầu cho da dầu, dễ bị mụn.",
                    Price = 780_000m,
                    Quantity = 200,

                    CategoryId = 2,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Chăm Sóc Da Chuyên Sâu Sublime Skin",
                    ProductDescription =
                        "Tinh chất phục hồi đa chức năng giúp làm mịn, săn chắc và bảo vệ da, làm đầy nếp nhăn và mang lại đường nét khuôn mặt thon gọn hơn.",
                    Price = 3_520_000m,
                    Quantity = 30,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tinh Chất Dưỡng Ẩm & Làm Sáng Hydramemory",
                    ProductDescription =
                        "Tinh chất dưỡng ẩm chuyên sâu với phức hợp làm sáng (Niacinamide và NAG) và Polyglutamic Acid (PGA) giúp da khỏe mạnh hơn, mềm mại và mịn màng hơn.",
                    Price = 1_350_000m,
                    Quantity = 7,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "7 x 2ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tinh Chất Nâng Cơ & Căng Da Sublime Skin",
                    ProductDescription =
                        "Liệu pháp thẩm mỹ chuyên sâu với peptide chống nhăn, yếu tố tăng trưởng biểu bì (EGFs) và axit hyaluronic phân tử lớn giúp nâng cơ, giảm nếp nhăn. Da được cấp ẩm tức thì và trở nên mịn màng hơn.",
                    Price = 1_890_000m,
                    Quantity = 8,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "8 x 2ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Vitamin C Biolumin-C",
                    ProductDescription =
                        "Tinh chất hiệu suất cao giúp tăng cường hệ thống phòng vệ tự nhiên của da để làm sáng, săn chắc và giảm đáng kể nếp nhăn. Công nghệ sinh học tiên tiến kết hợp với phức hợp Vitamin C siêu ổn định giúp tối ưu hóa khả năng hấp thụ Vitamin C, chống lại tác hại oxy hóa và các dấu hiệu lão hóa da.",
                    Price = 3_320_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Giảm Mụn & Lão Hóa Age Bright",
                    ProductDescription =
                        "Tinh chất hoạt động kép giúp giảm và ngăn ngừa mụn trong khi giảm các dấu hiệu lão hóa da. Axit Salicylic giúp làm sạch mụn, loại bỏ tế bào chết, đẩy nhanh quá trình tái tạo da. Phức hợp AGE Bright™ hỗ trợ hệ vi sinh da để mang lại làn da sáng khỏe hơn.",
                    Price = 2_620_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Làm Mờ Đốm Nâu Powerbright",
                    ProductDescription =
                        "Bắt đầu làm mờ sự xuất hiện của các đốm nâu trong vài ngày: serum tiên tiến giúp giảm nhanh sự không đồng đều về sắc tố và tiếp tục cải thiện tông màu da theo thời gian.",
                    Price = 3_810_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Tinh Chất Làm Dịu Da UltraCalming",
                    ProductDescription =
                        "Giải pháp cho làn da nhạy cảm. Serum siêu cô đặc này giúp làm dịu, phục hồi và bảo vệ làn da nhạy cảm. Công thức UltraCalming™ độc quyền chứa yến mạch và chiết xuất thực vật giúp giảm kích ứng, kết hợp với peptide và dầu hoa anh thảo, hạt hướng dương, bơ giúp bảo vệ da khỏi các tác động bên ngoài.",
                    Price = 2_310_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "40ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Cấp Ẩm Hyaluronic Circular",
                    ProductDescription =
                        "Kích hoạt chu trình cấp ẩm cho da: serum dưỡng ẩm lâu dài ngay lập tức cung cấp độ ẩm cho da, bổ sung từ bên trong và giúp ngăn chặn sự mất nước trong tương lai. Công thức chứa Hyaluronic Acid cải tiến giúp thẩm thấu sâu, mang lại làn da căng mọng, rạng rỡ hơn theo thời gian. Ma trận dưỡng ẩm chứa chiết xuất tảo biển mang lại hiệu quả cấp ẩm nhanh chóng và bền vững.",
                    Price = 2_230_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Dưỡng Ẩm Dâu & Đại Hoàng",
                    ProductDescription =
                        "Khám phá làn da rạng rỡ, trẻ trung với serum chứa dâu tây, đại hoàng và phức hợp Hyaluronic thực vật độc đáo. Kết hợp với rau má giúp cấp ẩm sâu, mang lại làn da mềm mịn và săn chắc hơn. Phù hợp với mọi loại da, đặc biệt là da khô hoặc mất nước.",
                    Price = 1_500_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum C+E Chống Oxy Hóa Citrus & Kale",
                    ProductDescription =
                        "Serum nhẹ dịu dành cho mọi loại da. Vitamin C mạnh mẽ được ổn định bằng axit ferulic từ thực vật, cung cấp chất chống oxy hóa giúp làm sáng da, cải thiện nếp nhăn và giảm tác động của các gốc tự do.",
                    Price = 2_990_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Peptide Chiết Xuất Hoa Biển",
                    ProductDescription =
                        "Serum dạng gel dễ hấp thụ, cung cấp peptide thực vật cô đặc và chiết xuất thực vật giúp giảm sự xuất hiện của nếp nhăn, mang lại làn da săn chắc và căng mịn. Công thức Smart Collagen+ giúp trẻ hóa làn da, trong khi chiết xuất tảo biển tăng độ đàn hồi và giữ ẩm lâu dài.",
                    Price = 2_990_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Trị Mụn Vỏ Liễu",
                    ProductDescription =
                        "Hỗ trợ làm dịu kích ứng và giảm các vấn đề về da với serum cô đặc chứa chiết xuất vỏ liễu và tinh dầu tràm trà.",
                    Price = 1_680_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Phục Hồi Hoa Thanh Cúc",
                    ProductDescription =
                        "Phục hồi và thanh lọc da với sức mạnh của hoa thanh cúc, cúc la mã và dâm bụt. Serum này giúp cải thiện độ đàn hồi của da, mang lại hiệu ứng trẻ hóa và săn chắc.",
                    Price = 1_480_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "15ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Căng Bóng Power",
                    ProductDescription =
                        "Liệu pháp đa nhiệm mạnh mẽ chứa phức hợp peptide hiệu suất cao và chất chống oxy hóa kích thích collagen, giúp cải thiện rõ rệt nếp nhăn và mang lại làn da săn chắc, trẻ trung hơn.",
                    Price = 3_910_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Làm Sáng Da Firma-Bright",
                    ProductDescription =
                        "Chứa hàm lượng cao vitamin C ổn định, chất chống oxy hóa và peptide làm sáng da, chỉ vài giọt mỗi ngày giúp da trở nên săn chắc, căng bóng và rạng rỡ hơn.",
                    Price = 3_380_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Serum Hydrostem",
                    ProductDescription =
                        "Liệu pháp chống lão hóa mạnh mẽ này kết hợp các peptide trẻ hóa với chiết xuất thực vật giúp bảo vệ da khỏi tác hại của tia UV và ô nhiễm. Tế bào gốc thực vật giàu chất chống oxy hóa nuôi dưỡng tế bào da, tăng cường độ rạng rỡ và săn chắc, mang lại làn da sáng khỏe hơn.",
                    Price = 4_270_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Minéral 89 Booster",
                    ProductDescription =
                        "Công thức đột phá với 89% nước khoáng núi lửa Vichy giàu 15 khoáng chất thiết yếu, kết hợp với axit hyaluronic tinh khiết giúp củng cố hàng rào độ ẩm, bảo vệ da trước các tác nhân gây hại. Da được cấp ẩm, săn chắc và căng mọng, mang lại vẻ rạng rỡ khỏe mạnh mỗi ngày.",
                    Price = 1_170_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Minéral 89 Probiotic Fractions",
                    ProductDescription =
                        "Giải pháp phục hồi da chịu căng thẳng từ tác nhân bên trong và bên ngoài như ô nhiễm, căng thẳng tâm lý và biến đổi khí hậu. Công thức chứa các phân đoạn lợi khuẩn được nuôi dưỡng trong nước khoáng núi lửa Vichy kết hợp với Niacinamide (Vitamin B3) giúp tái tạo và củng cố làn da, giảm dấu hiệu căng thẳng rõ rệt.",
                    Price = 1_300_000m,
                    Quantity = 1,

                    CategoryId = 3,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem dưỡng phục hồi Barrier Builder",
                    ProductDescription =
                        "Kem dưỡng chuyên sâu giúp làm dịu và tái tạo làn da bị kích ứng. Công thức giàu dưỡng chất thẩm thấu sâu vào biểu bì, cung cấp sự kết hợp mạnh mẽ của các thành phần đã được kiểm nghiệm giúp phục hồi, cấp ẩm và củng cố hàng rào bảo vệ da.",
                    Price = 1_480_000m,
                    Quantity = 1,

                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem dưỡng Power Luxe",
                    ProductDescription =
                        "Bước cuối cùng trong chu trình dưỡng da ban đêm, khóa ẩm và hỗ trợ tái tạo tế bào trong lúc ngủ. Power Luxe hoạt động theo nhịp sinh học của da, giúp cân bằng và phục hồi hàng rào lipid tự nhiên, ngăn ngừa khô da và nhạy cảm. Công thức chứa 4 loại axit hyaluronic, Bakuchiol (thay thế tự nhiên cho retinol), chiết xuất tảo đỏ và peptide hỗ trợ tăng cường độ săn chắc và đàn hồi của da.",
                    Price = 3_960_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem dưỡng ẩm không dầu AquaBoost",
                    ProductDescription =
                        "Công thức chứa 6 loại peptide với các tác dụng như giảm mụn, làm sáng, chống lão hóa, giảm đỏ và bảo vệ da. Chiết xuất hoa loa kèn linh thiêng giúp duy trì làn da trẻ trung, giảm kích ứng và tăng cường bảo vệ da khỏi tác động môi trường.",
                    Price = 1_750_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem dưỡng Face Lift",
                    ProductDescription =
                        "Face Lift sử dụng phức hợp lipid L22 giúp phục hồi chức năng bảo vệ da như khi còn trẻ. Công thức cung cấp độ ẩm hoàn hảo, đồng thời chứa chất chống oxy hóa và peptide giúp bảo vệ da khỏi các gốc tự do gây hại, duy trì vẻ ngoài săn chắc và tươi trẻ.",
                    Price = 2_110_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Dạng Lỏng Sublime Skin",
                    ProductDescription =
                        "Kem dưỡng dạng lỏng Comfort Zone giúp săn chắc và làm đều màu da. Được chiết xuất từ thực vật chống lão hóa Achillea Millefolilum và Axit Hyaluronic, sản phẩm giúp làm căng, mịn và sáng da. Sử dụng kết hợp với Tinh Chất Sublime Skin Intensive. Chứa 99% thành phần có nguồn gốc tự nhiên, không chứa silicone, dẫn xuất động vật, dầu khoáng, chất tạo màu nhân tạo, ethoxylates (PEG) và acrylates. Không gây bít tắc lỗ chân lông.",
                    Price = 3_530_000m, // Giá sản phẩm là 3.530.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    CategoryId = 4, // Kem dưỡng ẩm
                    CompanyId = 1,
                    Brand = "xxx", // Comfortzone
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem Dưỡng Chất Dinh Dưỡng Sacred Nature",
                    ProductDescription =
                        "Kem dưỡng giàu thành phần tự nhiên và hữu cơ, mang lại hiệu quả tức thì và lâu dài, giúp da trông trẻ trung hơn. Công thức chứa phức hợp chiết xuất khoa học Sacred Nature™, bao gồm Myrtus, Cây Cơm Cháy và Lựu, giúp cải thiện đáng kể sức khỏe tổng thể của làn da. Chứa thành phần hương liệu tự nhiên.",
                    Price = 1_780_000m, // Giá sản phẩm là 1.780.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 60ml
                    CategoryId = 4, // Kem dưỡng ẩm
                    CompanyId = 1,
                    Brand = "xxx", // Comfortzone
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Active Pureness",
                    ProductDescription =
                        "Sự kết hợp độc đáo giữa Vitamin C và bột kiềm dầu giúp kem dưỡng này trở thành lựa chọn lý tưởng cho da dầu và dễ bị mụn. Sản phẩm cũng có thể được sử dụng như kem lót trang điểm.",
                    Price = 1_400_000m, // Giá sản phẩm là 1.400.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 30ml
                    CategoryId = 4, // Kem dưỡng ẩm
                    CompanyId = 1,
                    Brand = "xxx", // Comfortzone
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Remedy",
                    ProductDescription =
                        "Kem dưỡng nhẹ, mịn giúp củng cố hàng rào bảo vệ da, mang lại cảm giác thoải mái và bảo vệ. Prebiotics từ đường tự nhiên giúp duy trì sự cân bằng hệ vi sinh tự nhiên của da. Phù hợp cho làn da nhạy cảm, dễ kích ứng và dễ bị đỏ.",
                    Price = 1_960_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Ẩm Dâu Tây & Đại Hoàng",
                    ProductDescription =
                        "Với kết cấu gel-cream thuần chay nhẹ nhàng, kem dưỡng giúp phục hồi vẻ tươi sáng của làn da xỉn màu. Kết hợp phức hợp Hyaluronic Acid thực vật với panthenol, dâu tây và đại hoàng giúp khóa ẩm và mang lại làn da rạng rỡ. Phù hợp với mọi loại da.",
                    Price = 1_730_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "35ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Bakuchiol + Niacinamide",
                    ProductDescription =
                        "Kem dưỡng dạng gel-cream giúp phục hồi độ ẩm tự nhiên cho da, kết hợp độc đáo giữa Bakuchiol – một sự thay thế cho retinol – và Niacinamide. Công thức giúp làm mịn nếp nhăn, săn chắc da, thu nhỏ lỗ chân lông và cải thiện kết cấu da mà không gây kích ứng.",
                    Price = 2_000_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Kiểm Soát Dầu Mụn",
                    ProductDescription =
                        "Kem dưỡng siêu nhẹ giúp giảm bóng dầu, mang lại làn da mềm mại với lớp kết thúc lì. Chiết xuất sen giúp kiểm soát dầu nhờn, trong khi Salicylic Acid giải phóng theo thời gian giúp giảm mụn. Kẽm hyaluronate và arbutin hỗ trợ làm dịu kích ứng và mờ sẹo.",
                    Price = 2_000_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "35ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Phục Hồi Da Echinacea",
                    ProductDescription =
                        "Chiết xuất cúc dại Echinacea, ngải cứu và dầu hoa anh thảo giúp sửa chữa các dấu hiệu lão hóa mà không gây cảm giác nhờn rít. Kem dưỡng dạng lỏng này rất lý tưởng cho làn da mất nước hoặc bị kích ứng.",
                    Price = 1_990_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Ban Đêm PowerBright",
                    ProductDescription =
                        "Kem dưỡng đêm giúp phục hồi độ ẩm, làm sáng và giảm đốm nâu khi bạn ngủ. Công thức chứa Niacinamide, Hexylresorcinol và Vitamin C giúp làm mờ vết thâm. Enzyme bí ngô giúp làm mịn kết cấu da. Dầu hạt việt quất và mâm xôi giàu chất chống oxy hóa bảo vệ da khỏi tác động của ô nhiễm môi trường.",
                    Price = 2_260_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Ẩm Làm Dịu Da",
                    ProductDescription =
                        "Tạm biệt làn da khô thiếu nước với kem dưỡng nhẹ này! Công thức mỏng nhẹ giúp làm dịu da bị kích ứng và dưỡng ẩm cho các vùng da khô. Ngoài ra, còn giúp giảm tình trạng khô da do một số phương pháp điều trị mụn.",
                    Price = 635_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Ẩm Sâu",
                    ProductDescription =
                        "Kem dưỡng thế hệ mới với công nghệ Active HydraMesh Technology™ cung cấp độ ẩm liên tục trong 48 giờ và bảo vệ da khỏi tác động môi trường. Hyaluronic Acid kết hợp với chiết xuất cẩm quỳ, dưa chuột và arnica giúp da duy trì độ ẩm lâu dài. Ngoài ra, còn chứa chiết xuất nho, Vitamin C và E giúp bảo vệ da.",
                    Price = 1_930_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "100ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Phục Hồi Bảo Vệ Da",
                    ProductDescription =
                        "Kem dưỡng mịn giúp củng cố hàng rào bảo vệ da bị tổn thương. Công thức không chứa nước, giúp bảo vệ da khỏi các tác nhân gây căng thẳng. Phức hợp UltraCalming độc quyền chứa yến mạch và chiết xuất thực vật giúp giảm kích ứng, khó chịu, nóng rát. Dầu hoa anh thảo, dầu hạt lưu ly cùng Vitamin C và E giúp phục hồi làn da.",
                    Price = 1_430_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Ẩm Vichy Minéral 89 72H Không Mùi",
                    ProductDescription =
                        "Kem dưỡng ẩm Vichy Minéral 89 72H là một loại kem nhẹ, cung cấp độ ẩm kéo dài cho da. Công thức chứa Hyaluronic Acid, Nước khoáng núi lửa Vichy giàu khoáng chất, Vitamin B3 & E, và Squalane giúp củng cố hàng rào độ ẩm của da, bảo vệ khỏi tác nhân gây hại từ môi trường. Phù hợp với mọi loại da, đặc biệt là da khô và nhạy cảm. Thẩm thấu nhanh, mang lại làn da mềm mại, căng mướt và đủ ẩm.",
                    Price = 550_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Đêm Vichy Liftactiv B3 Giúp Làm Đều Màu Da Với Retinol Nguyên Chất",
                    ProductDescription =
                        "Kem dưỡng đêm Liftactiv B3 giúp cải thiện tình trạng tăng sắc tố và màu da không đồng đều. Kết hợp Niacinamide (Vitamin B3) và Retinol, sản phẩm hỗ trợ tái tạo tế bào da, giảm đốm nâu và cải thiện kết cấu da. Phù hợp với mọi loại da, kể cả da nhạy cảm, mang lại làn da rạng rỡ và đều màu hơn.",
                    Price = 1_670_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Ẩm Belif The True Cream Aqua Bomb",
                    ProductDescription =
                        "Kem dưỡng dạng gel nhẹ giúp cấp ẩm tức thì và mang lại cảm giác mát lạnh trên da. Công thức chứa hỗn hợp thảo mộc, bao gồm Lady's Mantle, giàu chất chống oxy hóa. Phù hợp với mọi loại da, đặc biệt là da dầu và da hỗn hợp. Thẩm thấu nhanh, mang lại làn da tươi mát, ẩm mượt mà không gây nhờn rít.",
                    Price = 970_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Oat So Simple Water Cream",
                    ProductDescription =
                        "Kem dưỡng ẩm nhẹ như nước mang lại cảm giác tươi mát trên da. Công thức đơn giản với chưa đến 10 thành phần, bao gồm chiết xuất yến mạch giúp làm dịu da. Dưỡng ẩm thiết yếu, giúp da cảm thấy thoải mái và cân bằng độ ẩm.",
                    Price = 700_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "80ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Dưỡng Làm Dịu Da AESTURA A-CICA 365",
                    ProductDescription =
                        "Kem dưỡng ẩm giúp làm dịu da nhạy cảm và kích ứng. Công thức chứa chiết xuất rau má giúp làm dịu và phục hồi da. Cung cấp độ ẩm lâu dài, giảm đỏ và viêm da. Phù hợp cho da khô, nhạy cảm hoặc dễ bị mụn.",
                    Price = 790_000m,
                    Quantity = 1,
                    CategoryId = 4,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Chống Nắng Sun Soul Face SPF30",
                    ProductDescription =
                        "Kem chống nắng Sun Soul Face SPF30 bảo vệ da khỏi tia UV có hại. Cung cấp khả năng chống nắng phổ rộng UVA/UVB, giúp ngăn ngừa cháy nắng, lão hóa sớm và ung thư da. Công thức nhẹ dịu, thích hợp sử dụng hằng ngày cho mọi loại da, kể cả da nhạy cảm. Thẩm thấu nhanh, không gây nhờn rít, có thể sử dụng làm lớp lót trang điểm.",
                    Price = 880_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Chống Nắng Skin Regimen Urban Shield SPF30",
                    ProductDescription =
                        "Kem chống nắng Skin Regimen Urban Shield SPF30 của Comfort Zone là sản phẩm nhẹ, không nhờn rít, giúp bảo vệ da khỏi tác hại của ô nhiễm đô thị và bức xạ UV.",
                    Price = 1_670_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "40ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Chống Nắng PoreScreen SPF40",
                    ProductDescription =
                        "Kem chống nắng PoreScreen SPF40 giúp giảm sự xuất hiện của lỗ chân lông và bảo vệ da khỏi tia UVA + UVB. Hiệu ứng như kem lót với lớp nền mỏng nhẹ, mang lại lớp phủ trong suốt tự nhiên.",
                    Price = 1_390_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Chống Nắng Invisible Physical Defense SPF30",
                    ProductDescription =
                        "Nói lời tạm biệt với lớp kem chống nắng dày, trắng bệt! Công thức chống nắng vật lý này cung cấp khả năng bảo vệ chống lại ánh sáng xanh và giúp làm dịu tác động của các tác nhân gây hại từ môi trường. Phức hợp nấm hoạt tính giúp làm dịu da, giảm mẩn đỏ và khô da do tia UV. Chiết xuất trà xanh chống oxy hóa giúp bảo vệ da khỏi gốc tự do. Phù hợp với mọi loại da, kể cả da nhạy cảm.",
                    Price = 1_245_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Chống Nắng Protection 50 Sport SPF50",
                    ProductDescription =
                        "Kem chống nắng bảo vệ da khỏi tác hại kéo dài của tia UV và ô nhiễm môi trường. Công nghệ vi hạt Oleosome giúp tăng hiệu quả chống nắng và duy trì độ ẩm cho da. Công thức nhẹ, không gây nhờn rít, giúp trung hòa tác hại và giữ ẩm cho da mà không tạo cảm giác nặng mặt.",
                    Price = 1_000_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Chống Nắng Kiềm Dầu SPF30",
                    ProductDescription =
                        "Kem chống nắng phổ rộng giúp ngăn ngừa bóng dầu và lão hóa da trên làn da dầu, dễ bị mụn. Công thức nhẹ, siêu mỏng chứa hỗn hợp tiên tiến gồm Kẽm Gluconate, Caffeine, Niacinamide, Biotin và Chiết xuất Men. Các chất hấp thụ dầu giúp duy trì lớp nền lì suốt cả ngày, ngăn ngừa bóng dầu mà không để lại cặn phấn.",
                    Price = 2_030_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Tinh Chất Chống Nắng Bảo Vệ Da",
                    ProductDescription =
                        "Giúp bảo vệ da khỏi tác động khắc nghiệt của ánh nắng mặt trời đồng thời làm mịn các nếp nhăn. Với cảm giác dưỡng ẩm, nuôi dưỡng và lớp hoàn thiện căng bóng, sản phẩm này giúp da trông trẻ trung đồng thời cung cấp khả năng bảo vệ SPF 30 phổ rộng cho da thường đến da khô.",
                    Price = 1_600_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Dưỡng Ẩm Chống Nắng Hàng Ngày SPF 40",
                    ProductDescription =
                        "Kem dưỡng ẩm nhẹ hàng ngày với chiết xuất hạt ca cao, vỏ quýt satsuma và SPF 40 hoàn toàn từ khoáng chất giúp cải thiện làn da tiếp xúc với ánh sáng xanh và ô nhiễm. Phù hợp với mọi loại da.",
                    Price = 1_930_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem Chống Nắng Khoáng Chất Cho Thể Thao SPF 30",
                    ProductDescription =
                        "Dễ dàng thoa lên mặt và cơ thể, kem chống nắng khoáng chất SPF 30 này không nhờn rít và có khả năng chống nước lên đến 40 phút. Hiệu quả cao cho các hoạt động ngoài trời như bơi lội và thể thao cường độ cao, bảo vệ làn da khỏi tác động của tia UV.",
                    Price = 1_480_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "147ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Chống Nắng Nâng Tông SPF 50+",
                    ProductDescription =
                        "Mang lại lớp nền tự nhiên với kem chống nắng SPF 50+ có màu nhẹ. Công thức hoàn toàn từ khoáng chất, không gây bít tắc lỗ chân lông và giàu chất chống oxy hóa giúp dưỡng ẩm, bảo vệ da khỏi tia UV và ánh sáng xanh.",
                    Price = 1_750_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Serum Chống Nắng Khoáng Chất SPF 30",
                    ProductDescription =
                        "Serum chống nắng này kết hợp bảo vệ da với lợi ích chăm sóc da. Peptide CellRenew-16 được cấp bằng sáng chế giúp dưỡng ẩm và ngăn chặn gốc tự do gây lão hóa. Mang lại làn da được bảo vệ và căng bóng mà không bị nhờn rít.",
                    Price = 1_730_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "40ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Chống Nắng Không Màu SPF 50",
                    ProductDescription =
                        "Kem chống nắng khoáng chất SPF 50 với công thức mỏng nhẹ, kết hợp giữa chất chống oxy hóa thực vật, độ ẩm nhẹ nhàng và lớp nền mờ trong suốt giúp bảo vệ da khỏi tổn thương từ ánh nắng mặt trời và các gốc tự do.",
                    Price = 1_350_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Chống Nắng Nâng Tông Tự Điều Chỉnh SPF 30",
                    ProductDescription =
                        "Chống nắng có màu tự điều chỉnh, phù hợp với hầu hết các tông da, mang lại vẻ ngoài khỏe khoắn đồng thời bảo vệ da khỏi tia UVA, UVB và hồng ngoại. Axit Hyaluronic giúp giữ ẩm, trong khi các chiết xuất giàu chất chống oxy hóa bảo vệ da khỏi gốc tự do.",
                    Price = 1_400_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Vichy Capital Soleil UV Age Daily SPF50 PA++++",
                    ProductDescription =
                        "Kem chống nắng bảo vệ da khỏi tia UV có hại và ngăn ngừa lão hóa sớm. Công thức bảo vệ phổ rộng giúp ngăn ngừa cháy nắng, lão hóa sớm và ung thư da.",
                    Price = 570_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "40ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Chống Nắng Dạng Lỏng SPF 50",
                    ProductDescription =
                        "Kem chống nắng dưỡng da chống lão hóa hằng ngày với SPF 50 phổ rộng trong công thức siêu nhẹ, không chứa oxybenzone, chống nước lên đến 80 phút và dễ dàng thẩm thấu vào da.",
                    Price = 880_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },

                new Product
                {
                    Status = "Active",
                    ProductName = "Kem chống nắng Neogen Day-Light Protection Airy",
                    ProductDescription =
                        "Neogen Day-Light Protection Airy là kem chống nắng nhẹ, không nhờn, giúp bảo vệ da toàn diện trước tia UVA và UVB. Được điều chế với các thành phần dịu nhẹ, sản phẩm phù hợp với da nhạy cảm. Kem thẩm thấu nhanh, để lại lớp nền mịn lì và có thể sử dụng như lớp lót trang điểm. Ngoài ra, sản phẩm còn chứa các thành phần dưỡng da như chất chống oxy hóa và dưỡng ẩm, giúp bảo vệ và nuôi dưỡng làn da.",
                    Price = 815_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem chống nắng dưỡng ẩm Round Lab Birch Juice",
                    ProductDescription =
                        "Round Lab Birch Juice Moisturizing Sunscreen là kem chống nắng dưỡng ẩm, bảo vệ da toàn diện trước tia UVA và UVB. Công thức chứa nước cây bạch dương giúp cấp ẩm hiệu quả, phù hợp với mọi loại da, đặc biệt là da khô và nhạy cảm. Sản phẩm thẩm thấu nhanh, để lại lớp nền căng bóng tự nhiên và có thể dùng làm lớp lót trang điểm.",
                    Price = 360_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem chống nắng Beet The Sun SPF 40 PA+++",
                    ProductDescription =
                        "Beet The Sun là kem chống nắng hóa học nhẹ, giúp bảo vệ da trước ánh nắng mặt trời và các tác nhân môi trường. Thành phần chứa chiết xuất củ dền – một chất chống oxy hóa mạnh mẽ giúp bảo vệ da khỏi tác động oxy hóa. Kết cấu kem mềm mượt, không gây vệt trắng, phù hợp với mọi tông da.",
                    Price = 500_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem chống nắng khoáng Klairs All-day Airy SPF50+ PA++++",
                    ProductDescription =
                        "Klairs All-day Airy là kem chống nắng khoáng dịu nhẹ, phù hợp với da nhạy cảm. Công thức chứa thành phần khoáng chất giúp kem thẩm thấu nhanh, để lại lớp nền mịn lì mà không gây bít tắc lỗ chân lông. Sản phẩm cũng giúp hạn chế tình trạng vệt trắng, mang lại lớp nền tự nhiên.",
                    Price = 480_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Kem chống nắng Goongbe Waterful Sun Lotion Mild SPF50+ PA++++",
                    ProductDescription =
                        "Goongbe Waterful Sun Lotion Mild là kem chống nắng dịu nhẹ, gốc nước, phù hợp với da nhạy cảm, kể cả trẻ em. Sản phẩm bảo vệ da trước tia UVA và UVB, giúp ngăn ngừa cháy nắng và lão hóa sớm.",
                    Price = 450_000m,
                    Quantity = 1,
                    CategoryId = 5,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "80ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ thảo dược Eight Greens Phyto – Hot",
                    ProductDescription =
                        "Mặt nạ Eight Greens Phyto – Hot chứa các thành phần thực vật giàu phytoestrogen và chất chống oxy hóa. Sản phẩm giúp cung cấp độ ẩm, tăng độ đàn hồi da, giảm dấu hiệu lão hóa, điều tiết dầu thừa và hỗ trợ giảm mụn, mang lại làn da tươi trẻ rạng rỡ.",
                    Price = 1_580_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ Kombucha Microbiome Leave-On",
                    ProductDescription =
                        "Mặt nạ dưỡng ẩm chuyên sâu giúp làm dịu da khô xỉn màu. Công thức giàu chiết xuất gừng, pre, pro và postbiotics giúp phục hồi làn da, củng cố hàng rào độ ẩm. Phù hợp với mọi loại da, giúp da căng bóng, khỏe mạnh.",
                    Price = 2_140_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ Vitamin C+E Citrus & Kale Potent",
                    ProductDescription =
                        "Mặt nạ dạng kem-gel chứa Vitamin C+E giúp phục hồi làn da xỉn màu. Công thức từ cam quýt, rau lá xanh và dầu bơ giúp chống lại tác nhân oxy hóa, giảm thiểu nếp nhăn và tổn thương do môi trường.",
                    Price = 1_780_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ dưỡng ẩm Stone Crop",
                    ProductDescription =
                        "Mặt nạ Stone Crop giúp cấp ẩm và phục hồi da, mang lại làn da sáng khỏe. Chiết xuất từ cây Stone Crop được sử dụng trong y học cổ truyền giúp chữa lành nhiều vấn đề da liễu, mang lại hiệu quả dưỡng da vượt trội.",
                    Price = 1_400_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ làm dịu Calm Skin Arnica",
                    ProductDescription =
                        "Mặt nạ Calm Skin Arnica giúp làm dịu da nhạy cảm với chiết xuất từ cây kim sa, cúc calendula và lá thường xuân. Sản phẩm giúp giảm tình trạng kích ứng, thanh lọc da và hỗ trợ làm dịu viêm da.",
                    Price = 1_650_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Phục Hồi Đa Vitamin",
                    ProductDescription =
                        "Mặt nạ siêu bổ sung giúp cứu làn da căng thẳng, lão hóa. Sử dụng sau bước làm sạch để cải thiện làn da xỉn màu, khô ráp, mất nước, bị tổn thương do ánh nắng và dấu hiệu lão hóa. Vitamin A, C và E đậm đặc cùng Axit Linoleic giúp phục hồi làn da hư tổn, cải thiện hàng rào bảo vệ da. Vitamin C và E giúp chống oxy hóa, bảo vệ da khỏi các gốc tự do. Chiết xuất Tảo biển giúp dưỡng ẩm, làm mềm da, trong khi Pro-Vitamin B5 nuôi dưỡng làn da tổn thương. Các chiết xuất thảo mộc từ Cam thảo, Comfrey và Ngưu bàng giúp làm dịu và tăng sức đề kháng cho da.",
                    Price = 2_050_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "75ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Kiểm Soát Dầu",
                    ProductDescription =
                        "Mặt nạ đất sét làm dịu giúp làm sạch mụn và giảm dấu hiệu lão hóa sớm. Đất sét hấp thụ dầu giúp giải độc da, trong khi Axit Salicylic làm sạch tắc nghẽn lỗ chân lông. Chiết xuất từ Yến mạch và Bisabolol giúp làm dịu làn da bị kích ứng do mụn. Dầu cây Rum giúp dưỡng ẩm, giảm nếp nhăn do mất nước. Công thức chứa Cam thảo và Niacinamide cũng giúp làm đều màu da.",
                    Price = 1_700_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "75ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Dưỡng Ẩm Tan Chảy",
                    ProductDescription =
                        "Mặt nạ siêu dưỡng ẩm chuyển đổi từ dạng sáp sang dầu để phục hồi làn da khô. Phức hợp MeltingPoint kích hoạt bởi nhiệt độ da giúp thẩm thấu sâu, nuôi dưỡng và cấp ẩm. Tảo vi sinh giúp bảo vệ da khỏi tác hại của ô nhiễm. Công thức giàu vitamin, mềm mịn giúp cung cấp độ ẩm lâu dài, mang lại làn da căng mịn. Axit Linoleic giúp nuôi dưỡng da, trong khi Vitamin E bảo vệ khỏi các gốc tự do, giúp da trông khỏe mạnh hơn.",
                    Price = 2_340_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "75ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Kỳ Diệu",
                    ProductDescription =
                        "Mặt nạ đa chức năng sử dụng đất sét làm sạch sâu, giảm thiểu lỗ chân lông, đồng thời chứa peptide giúp cải thiện nếp nhăn và công thức nâng cơ tức thì.",
                    Price = 1_200_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Ngủ Khóa Ẩm",
                    ProductDescription =
                        "Mặt nạ ngủ giúp làm mịn và hoàn thiện làn da ngay cả khi bạn ngủ. Peptide hoàng gia giúp thúc đẩy quá trình tái tạo tế bào, cung cấp dưỡng chất giúp da rạng rỡ hơn.",
                    Price = 2_230_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "75ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Collagen PolyPeptide",
                    ProductDescription =
                        "Mặt nạ Hydrogel chứa peptide hỗ trợ collagen và dưỡng chất cấp ẩm giúp giảm nếp nhăn, làm sáng vùng da bị lão hóa.",
                    Price = 1_320_000m,
                    Quantity = 2,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "2 miếng",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Cân Bằng Da",
                    ProductDescription =
                        "Mặt nạ chống lão hóa độc đáo giúp bảo vệ làn da, giữ ẩm, làm dịu và duy trì vẻ ngoài trẻ trung.",
                    Price = 1_200_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Phục Hồi Trẻ Hóa",
                    ProductDescription =
                        "Mặt nạ giải độc giúp làm dịu làn da nhạy cảm hoặc kích ứng với đất sét làm mát và peptide giúp phục hồi.",
                    Price = 1_200_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Dưỡng Ẩm Đêm Aqualia Thermal",
                    ProductDescription =
                        "Kem dưỡng và mặt nạ ngủ cấp ẩm sâu với Hyaluronic Acid và nước khoáng núi lửa Vichy giúp khóa ẩm, cải thiện làn da khô và xỉn màu qua đêm. Mang lại làn da mềm mại, căng mướt vào buổi sáng.",
                    Price = 1_320_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt Nạ Khoáng Dưỡng Ẩm",
                    ProductDescription =
                        "Mặt nạ dưỡng ẩm đầu tiên của Vichy với 10% nước khoáng núi lửa và Vitamin B3 giúp cấp ẩm tức thì, làm dịu làn da khô và khó chịu.",
                    Price = 970_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ đất sét làm sạch lỗ chân lông",
                    ProductDescription =
                        "Mặt nạ đất sét tốt nhất của chúng tôi kết hợp hai loại đất sét trắng siêu mịn với 15 khoáng chất từ nước núi lửa Vichy, giúp loại bỏ bã nhờn dư thừa và tạp chất, làm sạch lỗ chân lông và mang lại làn da mềm mại hơn.",
                    Price = 970_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ Sulwhasoo Activating",
                    ProductDescription =
                        "Mặt nạ giấy thấm tinh chất từ First Care Activating Serum VI, giúp cung cấp độ ẩm chuyên sâu, cải thiện độ đàn hồi da và mang lại làn da khỏe mạnh, rạng rỡ.",
                    Price = 1_520_000m,
                    Quantity = 5,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "25ml x 5 miếng",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ ngủ dưỡng ẩm từ gạo COSRX Ultimate Nourishing",
                    ProductDescription =
                        "Mặt nạ ngủ dịu nhẹ, giàu độ ẩm với chiết xuất từ gạo giúp nuôi dưỡng và làm sáng da. Cải thiện kết cấu da, giảm xỉn màu và cung cấp độ ẩm sâu. Công thức nhẹ, không gây nhờn rít, phù hợp cho mọi loại da, đặc biệt là da khô và nhạy cảm.",
                    Price = 690_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ giấy Klairs Rich Moist Soothing Tencel",
                    ProductDescription =
                        "Ceramide trên bề mặt da giúp ngăn chặn mất nước, duy trì độ ẩm lý tưởng cho da, giúp da mềm mại và khỏe mạnh. Ceramide cũng giúp ngăn ngừa quá trình lão hóa da, giữ cho làn da luôn tươi trẻ.",
                    Price = 65_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "25ml",
                },
                new Product
                {
                    Status = "Active",
                    ProductName = "Mặt nạ làm dịu Klairs Midnight Blue",
                    ProductDescription =
                        "Mặt nạ chăm sóc lỗ chân lông với bột than tre giúp làm sạch sâu, ngăn ngừa sự hình thành mụn đầu đen. Thành phần vàng Erirythtol trong sản phẩm giúp nhanh chóng làm dịu nhiệt độ da.",
                    Price = 65_000m,
                    Quantity = 1,
                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "25ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Mặt nạ Klairs Freshly Juiced Vitamin E",
                    ProductDescription =
                        "Sự kết hợp giữa Vitamin E và Niacinamide mang lại hiệu quả chống oxy hóa, làm sáng da, cải thiện nếp nhăn và ngăn ngừa các dấu hiệu lão hóa.",
                    Price = 545_000m,
                    Quantity = 1,

                    CategoryId = 6,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "90ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Mặt nạ tẩy tế bào chết Sacred Nature",
                    ProductDescription =
                        "Mặt nạ tẩy tế bào chết hữu cơ chứa 9% gluconolactone giúp tẩy tế bào chết nhẹ nhàng và làm sáng da ngay lập tức. Chứa chất chống oxy hóa từ Garden of Science™ giúp tăng cường sức đề kháng và sức sống cho làn da. Phù hợp với làn da xỉn màu và có tạp chất.",
                    Price = 1_740_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "110ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy tế bào chết Essential Scrub",
                    ProductDescription =
                        "Tẩy tế bào chết dạng hạt giúp loại bỏ tạp chất, thu nhỏ lỗ chân lông, làm sáng da và mang lại làn da mềm mịn. Phù hợp cho mọi loại da, đặc biệt là da dầu và dễ bị mụn.",
                    Price = 1_295_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dung dịch tẩy tế bào chết Liquid Resurfacing",
                    ProductDescription =
                        "Dung dịch tẩy tế bào chết dạng nhẹ chứa 2% salicylic acid kết hợp với chất chống oxy hóa mạnh mẽ và công nghệ CellRenew-16 độc quyền, giúp cải thiện tông màu và kết cấu da mà không làm mất đi độ ẩm tự nhiên của da.",
                    Price = 1_245_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "120ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy tế bào chết 5X Power Peel",
                    ProductDescription =
                        "Miếng tẩy tế bào chết nhẹ nhàng nhưng hiệu quả, giúp loại bỏ lớp da xỉn màu, cải thiện nếp nhăn và kích thích sản sinh collagen. Không gây đỏ hay kích ứng da, an toàn để sử dụng hàng ngày mà không cần thời gian nghỉ dưỡng.",
                    Price = 1_850_000m,
                    Quantity = 30,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "25ml x 30 miếng",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bột Tẩy Tế Bào Chết Daily Milkfoliant",
                    ProductDescription =
                        "Bột tẩy tế bào chết dạng sữa thuần chay với chiết xuất Yến Mạch và Dừa kích hoạt khi tiếp xúc với nước, giải phóng các chiết xuất thực vật giúp loại bỏ tế bào chết. Công thức nhẹ nhàng này kết hợp cùng chiết xuất Nho giàu Alpha Hydroxy Acid (AHA) và Beta Hydroxy Acid (BHA), cùng Sữa Dừa để mang lại làn da mịn màng, mềm mại. Chiết xuất Đu Đủ chứa enzym Papain giúp hỗ trợ quá trình tái tạo da.",
                    Price = 1_670_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "74ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dung Dịch Tẩy Da Chết Liquid Peelfoliant",
                    ProductDescription =
                        "Dung dịch tẩy da chết chuyên nghiệp với hỗn hợp 30% axit và enzym (Glycolic, Lactic, Salicylic, Phytic, Tranexamic, Gluconolactone và enzym Lựu lên men) hoạt động trên nhiều tầng da, giúp loại bỏ tế bào chết, làm sạch lỗ chân lông và mang lại làn da mịn màng, rạng rỡ. Hỗn hợp giàu lipid với chiết xuất Nam Việt Quất giúp dưỡng ẩm lâu dài.",
                    Price = 1_650_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bột Tẩy Tế Bào Chết Daily Superfoliant",
                    ProductDescription =
                        "Sản phẩm tái tạo da mạnh mẽ giúp mang lại làn da mịn màng và chống lại các tác nhân môi trường gây lão hóa sớm. Công thức bột tiên tiến kích hoạt khi tiếp xúc với nước, giải phóng các enzym mạnh mẽ, AHA giúp làm mịn da và công nghệ chống ô nhiễm. Than hoạt tính Binchotan giúp thanh lọc da bằng cách hấp thụ độc tố từ sâu trong lỗ chân lông, trong khi Niacinamide, Tảo Đỏ và Chiết xuất Quả Tara giúp bảo vệ da trước tác hại của ô nhiễm.",
                    Price = 1_670_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy Tế Bào Chết Nhiệt Đa Vitamin",
                    ProductDescription =
                        "Tẩy tế bào chết dạng nhiệt giúp truyền các thành phần chống lão hóa vào da. Công thức kết hợp giữa tẩy tế bào chết vật lý và hóa học để cải thiện kết cấu da và giúp vitamin thẩm thấu sâu hơn. Các hạt siêu mịn giúp loại bỏ tế bào chết, mang lại làn da tươi mới ngay lập tức. Công nghệ nhiệt kích hoạt khi tiếp xúc với nước giúp Salicylic Acid và Retinol thấm sâu, trong khi Chiết xuất Xương Rồng giúp đẩy nhanh quá trình tái tạo da. Trà Trắng ngăn chặn sự hình thành MMPs, còn Cam Thảo, Vitamin C và E giúp làm sáng da, bảo vệ khỏi gốc tự do và tăng độ săn chắc.",
                    Price = 1_670_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bột Tẩy Tế Bào Chết Daily Microfoliant",
                    ProductDescription =
                        "Mang lại làn da sáng mịn mỗi ngày với bột tẩy tế bào chết huyền thoại này. Bột gạo kích hoạt khi tiếp xúc với nước, giải phóng Papain, Salicylic Acid và Enzym Gạo giúp làm sạch da tối ưu. Hợp chất làm sáng da từ Axit Phytic trong Cám Gạo, Trà Trắng và Cam Thảo giúp cân bằng tông màu da, trong khi hỗn hợp keo Yến Mạch và Allantoin giúp làm dịu da. Nhẹ nhàng để sử dụng hàng ngày.",
                    Price = 1_670_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bột Tẩy Tế Bào Chết Dâu Tây Rhubarb",
                    ProductDescription =
                        "Làm sáng và mịn da với bột tẩy tế bào chết Dâu Tây Rhubarb từng đạt giải thưởng. Chứa axit lactic, hỗn hợp bột mịn và hợp chất Hyaluronic Acid thực vật, sản phẩm giúp loại bỏ tạp chất, dầu thừa và mang lại làn da sáng khỏe. Phù hợp với mọi loại da.",
                    Price = 1_350_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "120ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Mặt Nạ Nghệ Tẩy Da Chết",
                    ProductDescription =
                        "Chứa nghệ, đá citrine và zeolite, sản phẩm dạng bột vàng rực này đánh thức làn da của bạn. Khi thêm nước, hỗn hợp chuyển thành dạng mousse nhẹ nhàng, mang lại cảm giác ấm nóng khi tẩy tế bào chết và giúp làn da trở nên mịn màng, rạng rỡ.",
                    Price = 2_490_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "60ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy Da Chết Rễ Cam Thảo Làm Sáng Da",
                    ProductDescription =
                        "Giải pháp tẩy tế bào chết chuyên sâu với bông thấm giúp làm đều màu da. Axit lactic và mandelic nhẹ nhàng loại bỏ tế bào chết, trong khi chiết xuất Cam Thảo và Hydroquinone tự nhiên từ Khoai Tây Châu Phi giúp làm sáng vùng da sạm màu và sắc tố không đều.",
                    Price = 2_260_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy Da Chết Hoa Cúc Làm Dịu Da",
                    ProductDescription =
                        "Giải pháp tẩy tế bào chết nhẹ nhàng với bông thấm dành cho làn da nhạy cảm. Axit lactic và mandelic giúp tái tạo da mà không gây kích ứng. Chiết xuất Hoa Cúc làm dịu, Cúc Vạn Thọ dưỡng ẩm, và Hoa Arnica giúp giảm viêm. Mang lại làn da khỏe mạnh, cân bằng và rạng rỡ hơn.",
                    Price = 2_260_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "50ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Mặt Nạ Tẩy Tế Bào Chết Hạt Củ Cải",
                    ProductDescription =
                        "Mặt nạ tẩy tế bào chết dịu nhẹ này giúp ngăn ngừa mụn với chiết xuất thảo mộc giải độc. Cây tầm ma, yến mạch nguyên cám và vỏ cây liễu kích thích tái tạo da, làm mịn các nếp nhăn.",
                    Price = 1_670_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "30ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bông Tẩy Tế Bào Chết Anua Heartleaf 77%",
                    ProductDescription =
                        "Bông tẩy tế bào chết Anua Heartleaf 77 giúp cân bằng và cải thiện làn da. Mỗi miếng bông chứa serum giàu chiết xuất thực vật như Ulmus Davidiana, Pueraria Lobata và lá thông Pinus Palustris, giúp kiểm soát dầu và thu nhỏ lỗ chân lông. Thành phần chính, chiết xuất diếp cá (77%), cung cấp độ ẩm và làm dịu kích ứng, phù hợp cho cả da nhạy cảm. Gluconolactone, một dạng PHA thân thiện với da, nhẹ nhàng loại bỏ tế bào chết, giúp da mịn màng và sáng hơn.",
                    Price = 660_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "180ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bông Dưỡng Da No.5 Vitamin-Niacinamide",
                    ProductDescription =
                        "Chứa niacinamide và dẫn xuất vitamin C giúp giảm thâm nám, làm đều màu da và cải thiện kết cấu da. Các miếng bông được ngâm trong dung dịch tẩy tế bào chết nhẹ nhàng, giúp loại bỏ da chết, mang lại làn da sáng mịn hơn.",
                    Price = 400_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "180ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bông Dưỡng Ẩm Cica Balanceful",
                    ProductDescription =
                        "Miếng bông chứa thành phần thuần chay với 5% tinh dầu từ jojoba, olive, macadamia và baobab, cung cấp độ ẩm vượt trội giúp da mềm mịn. Kết hợp với phức hợp Centella 5D và 5 dẫn xuất từ rau má, sản phẩm giúp làm dịu da, giảm đỏ và viêm mụn hiệu quả.",
                    Price = 506_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "180ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bông Làm Sạch Lỗ Chân Lông Chiết Xuất Lá Thông",
                    ProductDescription =
                        "Chứa chiết xuất lá thông với hàm lượng 30.000ppm giúp giảm nhờn, loại bỏ dầu thừa sâu trong lỗ chân lông, mang lại làn da sạch và mịn màng.",
                    Price = 590_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "145ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dung Dịch Tẩy Da Chết Krave Kale-lalu-yAHA",
                    ProductDescription =
                        "Dung dịch tẩy tế bào chết dịu nhẹ giúp làm mịn bề mặt da và mờ vết thâm, mang lại làn da khỏe mạnh, rạng rỡ hơn. Chứa 5.25% Glycolic Acid kích thích tái tạo da tự nhiên mà không làm tổn thương hàng rào bảo vệ da.",
                    Price = 640_000m,
                    Quantity = 1,

                    CategoryId = 7,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Dưỡng Thể Rắn Yuzu",
                    ProductDescription =
                        "Dầu dưỡng thể dạng rắn giúp phục hồi làn da khô xỉn màu với chiết xuất yuzu và camu camu giàu vitamin, mang lại làn da sáng mịn từ mọi góc độ. PHA và tinh dầu nhiệt đới giúp cung cấp độ ẩm vượt trội cho mọi loại da.",
                    Price = 1_430_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Dưỡng Thể Măng Cụt",
                    ProductDescription =
                        "Sữa dưỡng thể mang lại độ ẩm tuyệt vời cho cơ thể với hương thơm dễ chịu từ măng cụt. Công thức chứa Lactic Acid Complex gồm axit lactic, ribose và chiết xuất hoa cỏ ba lá đỏ, giúp làm sáng và tái tạo làn da.",
                    Price = 1_145_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy Tế Bào Chết Đường Dừa",
                    ProductDescription =
                        "Loại bỏ tế bào chết và dưỡng ẩm toàn thân với hạt đường mía nguyên chất, một nguồn Alpha Hydroxy Acids tự nhiên giúp tẩy da chết hiệu quả, kết hợp với dầu dừa nguyên chất để dưỡng ẩm.",
                    Price = 1_120_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem Dưỡng Thể Săn Chắc Da Chiết Xuất Lô Hội",
                    ProductDescription =
                        "Kem dưỡng thể giàu thành phần tự nhiên giúp làm mịn và săn chắc làn da. Có thể sử dụng như kem dưỡng toàn thân hoặc tập trung vào các vùng cần làm săn chắc. Chiết xuất cà phê và tảo vi sinh giúp giảm sự xuất hiện của cellulite, trong khi dầu jojoba, lô hội và bơ hạt mỡ giúp dưỡng ẩm và phục hồi da khô.",
                    Price = 1_730_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "140ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Dưỡng Thể Stone Crop",
                    ProductDescription =
                        "Làm dịu và làm mềm da khô với dầu dưỡng thể nhẹ, hấp thụ nhanh, để lại lớp hoàn thiện mịn màng. Sự kết hợp giữa stone crop và arnica tạo nên dầu có mùi hương nhẹ nhàng, lý tưởng cho massage, chăm sóc tay chân hoặc dưỡng ẩm hàng ngày.",
                    Price = 940_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "240ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Bơ Dưỡng Thể Sacred Nature",
                    ProductDescription =
                        "Thành phần hữu cơ Eco-Cert mang lại những dưỡng chất tự nhiên hiệu quả nhất giúp da mềm mại và mịn màng. Công thức chứa Scientific Garden Extract™, một hỗn hợp gồm cây sim, cây cơm cháy và lựu. Chứa hương liệu từ thiên nhiên.",
                    Price = 1_340_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "220ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Dưỡng Tranquillity",
                    ProductDescription =
                        "Dầu tắm và dưỡng thể chứa dầu amaranth giàu dưỡng chất cùng hỗn hợp tinh dầu độc quyền giúp mang lại cảm giác thư thái tức thì, giảm căng thẳng và lo âu. Khi tiếp xúc với nước, dầu chuyển thành dạng sữa mịn. Khi thoa trực tiếp lên da, dầu giúp nuôi dưỡng, mang lại làn da mềm mại mà không để lại cảm giác nhờn dính.",
                    Price = 2_590_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy Tế Bào Chết Body Strategist",
                    ProductDescription =
                        "Tác động tẩy tế bào chết kép (cơ học và hóa học) giúp làn da trông mịn màng, mềm mại và sáng hơn ngay lập tức. Phù hợp với mọi loại da, đặc biệt là da khô và sần sùi. Lý tưởng để sử dụng hàng tuần quanh năm.",
                    Price = 1_500_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Dưỡng Thể Body Strategist",
                    ProductDescription =
                        "Sự kết hợp giữa các loại dầu thiên nhiên quý giá giúp giảm khô da, rạn da và mất độ đàn hồi. Cải thiện kết cấu và tông da hiệu quả.",
                    Price = 1_680_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem Săn Chắc Body Strategist",
                    ProductDescription =
                        "Kem dưỡng thể Body Strategist nhẹ nhàng và dưỡng ẩm, mang lại cảm giác săn chắc tức thì cho làn da nhờ sự kết hợp giữa khoa học và thiên nhiên, với thành phần chiết xuất từ Acmella Oleracea giúp giảm co cơ. Phù hợp với mọi loại da bị mất độ đàn hồi và săn chắc.",
                    Price = 1_680_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem Dưỡng Ẩm Toàn Thân",
                    ProductDescription =
                        "Dưỡng ẩm, làm mềm và săn chắc da với hỗn hợp tinh dầu từ khắp nơi trên thế giới. Dầu cam thơm và trà xanh Trung Quốc giúp làm dịu và mềm da. Oải hương Pháp và hoắc hương Indonesia làm dịu cảm giác căng thẳng, trong khi axit lactic tự nhiên và chiết xuất từ mía đường cùng táo giúp da mịn màng và giảm khô ráp.",
                    Price = 1_260_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "295ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Dưỡng Phục Hồi Da",
                    ProductDescription =
                        "Giàu chất chống oxy hóa, hỗn hợp dầu dưỡng này giúp phục hồi, làm dịu và cấp ẩm cho da. Dầu hạt mận Pháp, dầu bơ và dầu hạt hướng dương giàu axit béo omega và vitamin E giúp bảo vệ hàng rào lipid của da. Chiết xuất nhân sâm đỏ lên men giúp làm dịu và nuôi dưỡng, mang lại làn da sáng khỏe.",
                    Price = 1_960_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "125ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Sữa Tắm Dưỡng Ẩm",
                    ProductDescription =
                        "Làm sạch, nuôi dưỡng và tiếp thêm sinh lực cho làn da với sữa tắm giàu dưỡng chất này. Lấy cảm hứng từ các loại tinh dầu được sử dụng trong phòng tắm hơi Thổ Nhĩ Kỳ, công thức chứa dầu hương thảo Pháp, khuynh diệp Trung Quốc, tràm trà tươi và chanh giúp làm sạch da và đánh thức giác quan.",
                    Price = 1_260_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "295ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Dưỡng Thể Rạng Rỡ",
                    ProductDescription =
                        "Hỗn hợp xa hoa từ các loại dầu thực vật tinh khiết và squalane giúp làm dịu và cấp ẩm sâu cho da. Vitamin C và E có tác dụng bảo vệ, làm sáng và làm mềm da, trong khi các hạt nhũ mịn giúp da sáng rạng rỡ. Chiết xuất từ các loại thảo mộc và hoa như hương thảo, hoa sứ, đàn hương và hoa nhài mang lại hương thơm quyến rũ.",
                    Price = 1_980_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "100ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem Dưỡng Ẩm Căng Mịn Cơ Thể",
                    ProductDescription =
                        "Loại kem dưỡng thể giàu dưỡng chất này sẽ khiến bạn tự tin khoe làn da. Với công thức giúp làm săn chắc và dưỡng ẩm tuyệt vời, sản phẩm còn hỗ trợ giảm sự xuất hiện của vết rạn, sẹo và vùng da không đều màu.",
                    Price = 2_510_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Kem Dưỡng Ceramide Ato Illiyoon",
                    ProductDescription =
                        "Kem dưỡng ẩm từ thương hiệu ILLIYOON Hàn Quốc, chứa các thành phần dưỡng ẩm kết hợp với dưỡng chất tự nhiên, nuôi dưỡng làn da nhẹ nhàng, lành tính, phù hợp với mọi lứa tuổi và mọi loại da.",
                    Price = 295_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Tẩy Tế Bào Chết & Sữa Tắm Dear Doer",
                    ProductDescription =
                        "Sữa tắm và tẩy tế bào chết 2 trong 1 với thành phần từ thực vật, tạo bọt dày giúp làm sạch toàn thân. Chiết xuất từ đá núi lửa Perlite mịn và muối Andes thô giúp loại bỏ tế bào chết, mang lại hương thơm thư giãn.",
                    Price = 450_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "300ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Lotion Dưỡng Ẩm Aestura Atobarrier 365",
                    ProductDescription =
                        "Dưỡng ẩm, làm sáng và tăng cường sức đề kháng cho da, giúp da khỏe mạnh, ngăn ngừa nứt nẻ và khô ráp.",
                    Price = 600_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Xịt Dưỡng Cân Bằng pH Dr. Orga",
                    ProductDescription =
                        "Chiết xuất rau diếp cá giàu chất chống oxy hóa giúp làm dịu và giảm viêm, đồng thời ngăn ngừa mất nước cho da. Sản phẩm còn chứa AHA, BHA và PHA giúp tẩy tế bào chết nhẹ nhàng và hỗ trợ điều trị mụn cơ thể.",
                    Price = 590_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "150ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Dưỡng Ẩm Cơ Thể Derma B",
                    ProductDescription =
                        "Dầu dưỡng thể nhẹ, lâu trôi, chứa dầu hạnh nhân, argan và dầu hoa trà giúp dưỡng ẩm và làm săn chắc da. Công thức dịu nhẹ phù hợp với mọi loại da, kể cả da nhạy cảm. Hương đào tươi mát mang đến trải nghiệm thư giãn.",
                    Price = 430_000m,
                    Quantity = 1,

                    CategoryId = 8,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "200ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Davines Volu",
                    ProductDescription =
                        "Dành cho tóc mỏng và xẹp, giúp tăng độ bồng bềnh tự nhiên. Công thức đặc biệt tạo bọt siêu mịn giúp làm sạch nhẹ nhàng bụi bẩn hàng ngày, giúp tóc nhẹ và mềm mượt hơn. Hương hoa nhẹ nhàng đặc trưng của dòng Volu.",
                    Price = 915_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "1000ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Davines Calming",
                    ProductDescription =
                        "Phù hợp cho da đầu nhạy cảm hoặc dễ bị kích ứng, mang lại cảm giác làm sạch nhẹ nhàng, dễ chịu. Chiết xuất việt quất giàu vitamin C, B và chất chống oxy hóa giúp bảo vệ và nuôi dưỡng tóc và da đầu, đồng thời có tác dụng kháng viêm hiệu quả.",
                    Price = 985_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "1000ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Davines Dede",
                    ProductDescription =
                        "Phù hợp với mọi loại da đầu, giúp loại bỏ bụi bẩn hàng ngày, mang lại cảm giác tóc nhẹ nhàng, mềm mượt. Công thức tạo bọt siêu mịn giúp thư giãn và làm mới da đầu. Hương chanh tươi mát.",
                    Price = 915_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "1000ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Davines Melu",
                    ProductDescription =
                        "Đặc biệt phù hợp với tóc dài hoặc tóc hư tổn, giúp phục hồi và ngăn ngừa chẻ ngọn hiệu quả. Dạng kem mềm mịn giúp làm sạch nhẹ nhàng. Hương thơm gỗ và hoa đặc trưng của dòng Melu.",
                    Price = 1_055_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "1000ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội phục hồi Bain Décalcifiant Réparateur",
                    ProductDescription =
                        "Nước tốt cho bạn, nhưng không hẳn tốt cho tóc. Canxi trong nước tắm có thể gây hư tổn kéo dài, khiến tóc cứng, xỉn màu và dễ gãy. Hệ thống mạnh mẽ này giúp loại bỏ sự tích tụ canxi gây tổn thương và phục hồi đến 99% độ chắc khỏe ban đầu của tóc. Nó sửa chữa hư tổn kéo dài, giúp tóc mềm mại, bóng hơn 73% và mượt hơn gấp 2 lần. Tóc chắc khỏe hơn 93%.",
                    Price = 1_780_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "500ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội tăng cường độ dày Bain Densité",
                    ProductDescription =
                        "Làm sạch bụi bẩn tích tụ trên tóc. Tăng mật độ tóc. Tăng cường sức mạnh cho tóc. Tạo độ bóng. Giúp tóc dày dặn hơn đến tận ngọn. Tăng độ đàn hồi từ gốc tóc.",
                    Price = 1_780_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "500ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội dưỡng ẩm và tăng cường Bain Hydra-Fortifiant",
                    ProductDescription =
                        "Không chứa silicone. Nhẹ nhàng loại bỏ bã nhờn và bụi bẩn trên da đầu và tóc. Kiểm soát dầu hiệu quả. Ngăn ngừa rụng tóc do gãy rụng khi chải. Giảm gãy rụng đến 97%.",
                    Price = 1_780_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "500ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội phục hồi toàn diện L'Oreal Paris Elseve Total Repair 5",
                    ProductDescription =
                        "Total Repair 5 là giải pháp chăm sóc giúp giảm 5 dấu hiệu hư tổn của tóc. Với thành phần Ceramide mới tương tự như keratin tự nhiên trong sợi tóc, sản phẩm nhanh chóng thẩm thấu vào các vùng tóc yếu và hư tổn, giúp tóc chắc khỏe, mượt mà, bóng sáng và giảm chẻ ngọn.",
                    Price = 200_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "650ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội dưỡng sâu L'Oreal Professional Hair Spa",
                    ProductDescription =
                        "Chiết xuất từ tinh dầu tràm trà giúp làm sạch gàu, ngăn mùi hôi và mồ hôi trên da đầu. Kết hợp với massage giúp thư giãn da đầu, mang lại mái tóc và da đầu khỏe mạnh, sảng khoái.",
                    Price = 319_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "600ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội chống rụng tóc L'Oreal Paris Elseve Fall Resist 3X",
                    ProductDescription =
                        "Cung cấp dưỡng chất cần thiết cho tóc, ngăn ngừa rụng tóc, mang lại mái tóc chắc khỏe và đẹp hơn. Công thức chống rụng tóc với 3 tác động: nuôi dưỡng từ chân tóc, tái tạo cấu trúc tóc và giúp tóc chắc khỏe hơn.",
                    Price = 200_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "650ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội phục hồi liên kết Nº.4 Bond Maintenance",
                    ProductDescription =
                        "Dầu gội cô đặc cao giúp tăng cường độ chắc khỏe và độ bóng cho tóc dễ hư tổn trong khi làm sạch nhẹ nhàng. Bọt kem dày giúp nuôi dưỡng và phục hồi, mang lại mái tóc mềm mại và dễ kiểm soát hơn.",
                    Price = 760_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội phục hồi và tái tạo Gold Lust",
                    ProductDescription =
                        "Đánh thức mái tóc của bạn trở về thời kỳ óng ả, khỏe mạnh nhất. Sữa gội này kết hợp dầu dưỡng cổ truyền như cây bách và argan với phức hợp phục hồi sinh học tiên tiến để cân bằng da đầu và tăng cường độ chắc khỏe từ bên trong từng sợi tóc.",
                    Price = 1_350_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội dưỡng ẩm và làm sáng Supershine Hydrating",
                    ProductDescription =
                        "Tận hưởng sự sang trọng. Dầu gội này cung cấp độ ẩm dồi dào và làm mượt tóc, biến mái tóc xỉn màu trở nên bóng mượt. Nhẹ nhàng làm sạch mà không làm mất đi dầu tự nhiên của tóc, tăng cường độ bóng, giữ độ ẩm mà không làm nặng tóc, làm mềm và mịn tóc để tăng độ sáng và vẻ đẹp tự nhiên.",
                    Price = 1_245_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "250ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu gội không chứa sulfate Acidic Bonding Concentrate",
                    ProductDescription =
                        "Dầu gội không chứa sulfate Acidic Bonding Concentrate là công thức đậm đặc nhất của Redken, giúp phục hồi sức mạnh cho tất cả các loại tóc hư tổn.",
                    Price = 840_000m,
                    Quantity = 1,

                    CategoryId = 9,
                    CompanyId = 1,
                    Brand = "xxx",
                    Dimension = "300ml",
                },

                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Redken All Soft",
                    ProductDescription =
                        "Dầu gội dành cho tóc khô với công thức dưỡng ẩm của Redken. Nhẹ nhàng làm sạch, đồng thời bổ sung độ ẩm, sự mềm mại và vẻ ngoài khỏe mạnh cho tóc. Giúp tóc trở nên mềm mượt, dễ chải và bóng sáng hơn.",
                    Price = 770_000m, // Giá sản phẩm là 770.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1,
                    Brand = "xxx", // Redken
                    Dimension = "300ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Tăng Cường Izumi Tonic",
                    ProductDescription =
                        "Dầu gội tăng cường giúp làm sạch nhẹ nhàng, phục hồi và làm dày tóc yếu. Được bổ sung nước gạo để tăng cường sức khỏe và phục hồi tóc, dầu gội Izumi Tonic giúp tóc chắc khỏe hơn gấp 30 lần* và giảm chẻ ngọn đến 91%*. Công thức nhẹ nhàng giúp làm sạch sâu mà không làm mất đi độ ẩm tự nhiên của tóc..",
                    Price = 1_250_000m, // Giá sản phẩm là 1.250.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1,
                    Brand = "xxx", // Shu uemura (CompanyId mặc định là 5)
                    Dimension = "300ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Phục Hồi Tối Ưu",
                    ProductDescription =
                        "Dầu gội này giúp phục hồi và tái tạo tóc hư tổn nặng do tẩy, nhuộm hoặc xử lý hóa chất thường xuyên. Cung cấp dưỡng chất chuyên sâu mà không làm nặng tóc. Làm sạch nhẹ nhàng, loại bỏ bụi bẩn, tạp chất và sản phẩm dư thừa, đồng thời tăng cường sức mạnh từ gốc đến ngọn, giúp giảm gãy rụng và duy trì sức khỏe tóc. An toàn cho tóc nhuộm, giúp bảo vệ màu sắc mà không ảnh hưởng đến sức khỏe tóc.",
                    Price = 1_340_000m, // Giá sản phẩm là 1.340.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 300ml
                    // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1,
                    Brand = "xxx", // Shu uemura (CompanyId mặc định là 1)
                    Dimension = "300ml",
                    // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Fusion",
                    ProductDescription =
                        "Công nghệ Metal Purifier trong dầu gội giúp loại bỏ tạp chất kim loại nhờ cơ chế chống oxy hóa. Thành phần lipid siêu nhỏ giúp dưỡng ẩm tức thì và phục hồi tóc chuyên sâu.",
                    Price = 965_000m, // Giá sản phẩm là 965.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1,
                    Brand = "xxx", // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Gội Ultimate Repair",
                    ProductDescription =
                        "Dầu gội dạng kem với công thức tạo bọt nhẹ nhàng giúp làm sạch tóc hiệu quả, tạo bọt sang trọng. Được phát triển với công nghệ Metal Purifier, dầu gội này giúp thải độc và phục hồi tóc hư tổn do nhiệt và tẩy nhuộm, mang lại mái tóc khỏe mạnh, bóng mượt và mềm mại.",
                    Price = 660_000m, // Giá sản phẩm là 660.000 VND
                    Quantity = 1, // Sản phẩm có dung tích 250ml
                    // Không có chiết khấu
                    CategoryId = 9, // Shampoo
                    CompanyId = 1,
                    Brand = "xxx", // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Davines Dede",
                    ProductDescription =
                        "Sản phẩm có khả năng nuôi dưỡng tóc, ngăn ngừa rối và giữ cho mái tóc mềm mượt. Ngoài ra, khi sử dụng, bạn sẽ cảm nhận được tóc trở nên mềm mại, nhẹ nhàng và bồng bềnh hơn.",
                    Price = 1_318_000m, // Giá sản phẩm là 1.318.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Davines Love Smoothing",
                    ProductDescription =
                        "Dầu xả Davines Love Smoothing được thiết kế đặc biệt dành cho tóc khô, xù và tóc xoăn, giúp mái tóc trở nên mượt mà hơn. Ngoài ra, sản phẩm còn giúp cân bằng độ ẩm cho tóc, làm tóc nhẹ nhàng, mềm mượt và bồng bềnh hơn.",
                    Price = 1_550_000m, // Giá sản phẩm là 1.550.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Davines Melu",
                    ProductDescription =
                        "Sản phẩm có khả năng ngăn ngừa chẻ ngọn rất hiệu quả, đồng thời giúp tóc trở nên nhẹ nhàng và mềm mại hơn. Sản phẩm còn nuôi dưỡng nhẹ nhàng từ bên trong sợi tóc, làm tóc dày hơn và khỏe mạnh hơn. Dầu xả này có mùi hương hoa cỏ và gỗ nhẹ nhàng, mang lại cảm giác thư giãn.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Davines Momo",
                    ProductDescription =
                        "Dầu xả nhẹ nhàng này cung cấp dưỡng chất và độ ẩm cho tóc, ngăn ngừa rối tóc và mang lại sự mềm mại, mượt mà. Bọt xà phòng dày đặc và hương hoa dễ chịu của sản phẩm giúp nâng cao trải nghiệm chăm sóc tóc của bạn.",
                    Price = 1_318_000m, // Giá sản phẩm là 1.318.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Davines (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Fondant Renforçateur",
                    ProductDescription =
                        "Không chứa silicone, không chứa sulfate. Dầu xả có tác dụng ngăn ngừa gãy tóc và củng cố sợi tóc. Công thức nhẹ nhàng, cung cấp độ bền và sự mềm mại ngay lập tức, đồng thời cung cấp độ ẩm và độ bóng cho tóc. Sản phẩm giúp dễ dàng gỡ rối và tạo độ phồng cho tóc. Giảm 97% gãy rụng tóc do chải tóc.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Fondant Densité",
                    ProductDescription =
                        "Cải thiện kết cấu tóc, giúp tóc trở nên dày dặn hơn. Dễ dàng gỡ rối và làm tóc mềm mượt, dễ chải.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Fondant Fluidealiste",
                    ProductDescription =
                        "Dầu xả sang trọng này là giải pháp hoàn hảo cho tóc khô và tóc xù. Công thức nhẹ nhàng thẩm thấu sâu vào sợi tóc, cung cấp độ ẩm mạnh mẽ và làm mượt lớp biểu bì tóc. Tạm biệt tóc xù và chào đón mái tóc mềm mượt, dễ chải.",
                    Price = 1_220_000m, // Giá sản phẩm là 1.220.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Ciment Anti-Usure",
                    ProductDescription =
                        "Hồi sinh mái tóc của bạn với liệu trình phục hồi này. Dầu xả giúp sửa chữa tóc hư tổn, mang lại độ bóng rực rỡ và tăng cường sức mạnh cho sợi tóc để ngăn ngừa gãy rụng. Trải nghiệm mái tóc mềm mại, khỏe mạnh và đầy sức sống.",
                    Price = 1_110_000m, // Giá sản phẩm là 1.110.000 VND
                    Quantity = 1, // Dung tích là 200ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Kerastase (CompanyId mặc định là 1)
                    Dimension = "200ml",
                    // Tổng dung tích là 200ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Redken All Soft",
                    ProductDescription =
                        "Dầu xả cung cấp dưỡng ẩm sâu và phục hồi cho tóc khô, dễ gãy, mang lại mái tóc mềm mượt, suôn sẻ và bóng khỏe. Sản phẩm giúp nuôi dưỡng và làm mềm tóc, cải thiện độ bóng và kết cấu tóc, đồng thời tăng cường độ ẩm cho tóc từ gốc đến ngọn.",
                    Price = 840_000m, // Giá sản phẩm là 840.000 VND
                    Quantity = 1, // Dung tích là 300ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Redken (CompanyId mặc định là 1)
                    Dimension = "300ml",
                    // Tổng dung tích là 300ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Redken Frizz Dismiss",
                    ProductDescription = "Làm mượt tóc xù và tóc bay, mang lại mái tóc mềm mại và dễ tạo kiểu.",
                    Price = 770_000m, // Giá sản phẩm là 770.000 VND
                    Quantity = 1, // Dung tích là 300ml
                    CategoryId = 10,
                    CompanyId = 1,
                    Brand = "xxx", // Redken (CompanyId mặc định là 1)
                    Dimension = "300ml",
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả L'Oréal Paris Elvive Total Repair 5",
                    ProductDescription =
                        "Dầu xả này được thiết kế để củng cố và phục hồi tóc hư tổn. Với công thức chứa sự kết hợp của ceramide, sản phẩm giúp khắc phục 5 dấu hiệu hư tổn: chẻ ngọn, yếu, xơ rối, xỉn màu và mất ẩm, mang lại mái tóc mượt mà và được tái tạo.",
                    Price = 145_000m, // Giá sản phẩm là 145.000 VND
                    Quantity = 1, // Dung tích là 355ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // L'Oreal (CompanyId mặc định là 1)
                    Dimension = "355ml",
                    // Tổng dung tích là 355ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả L'Oréal Paris EverPure Moisture",
                    ProductDescription =
                        "Dầu xả không chứa sulfat này là lựa chọn hoàn hảo cho tóc nhuộm. Sản phẩm cung cấp độ ẩm sâu và dưỡng chất, làm tăng sự tươi sáng của màu tóc đồng thời giữ tóc mềm mại và dễ tạo kiểu. Được chiết xuất từ cây hương thảo, dầu xả giúp duy trì độ bóng và độ ẩm cho tóc mà không làm nặng tóc.",
                    Price = 170_000m, // Giá sản phẩm là 170.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // L'Oreal (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả L'Oréal Paris EverCurl Hydracharge",
                    ProductDescription =
                        "Dầu xả này được đặc chế dành riêng cho tóc xoăn, giúp cung cấp độ ẩm và định hình các lọn tóc xoăn trong khi giảm tình trạng tóc xù. Công thức nhẹ nhàng làm tăng độ bouncy và mềm mại cho tóc, mang lại độ ẩm lâu dài và kết cấu mềm mượt mà không chứa sulfat hay muối gây hại.",
                    Price = 195_000m, // Giá sản phẩm là 195.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // L'Oreal (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Wella Invigo Nutri-Enrich",
                    ProductDescription =
                        "Dầu xả Wella Invigo Nutri-Enrich được thiết kế dành cho tóc khô và hư tổn, giúp nuôi dưỡng và cung cấp độ ẩm cho tóc. Chứa chiết xuất quả goji và vitamin E, sản phẩm giúp cải thiện độ đàn hồi và khả năng kiểm soát tóc, đồng thời mang lại vẻ ngoài mềm mại và khỏe mạnh cho mái tóc.",
                    Price = 460_000m, // Giá sản phẩm là 460.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Redken Elements Renewing",
                    ProductDescription =
                        "Dầu xả không chứa silicone này giúp tái tạo và nuôi dưỡng mọi loại tóc. Được chiết xuất từ các thành phần tự nhiên, sản phẩm cung cấp độ ẩm sâu và giúp củng cố tóc, mang lại mái tóc mềm mại và khỏe mạnh mà không làm tóc nặng.",
                    Price = 1_180_000m, // Giá sản phẩm là 1.180.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Moisture Velvet Nourishing",
                    ProductDescription =
                        "Dầu xả cao cấp này cung cấp dưỡng chất sâu cho tóc khô và thô, mang lại độ ẩm thiết yếu và cải thiện độ mềm mại cho tóc. Công thức chứa hoa mẫu đơn Nhật Bản giúp gỡ rối tóc, đồng thời để lại mái tóc mềm mượt, khỏe mạnh và bóng bẩy.",
                    Price = 1_200_000m, // Giá sản phẩm là 1.200.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Shu uemura (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Ultimate Remedy",
                    ProductDescription =
                        "Dầu xả Ultimate Remedy được thiết kế đặc biệt dành cho tóc hư tổn nặng, cung cấp khả năng phục hồi và dưỡng ẩm sâu. Enriched với các thành phần tự nhiên, sản phẩm giúp phục hồi sức mạnh và độ đàn hồi cho tóc, mang lại mái tóc khỏe mạnh, tươi mới và dễ dàng chải chuốt.",
                    Price = 1_320_000m, // Giá sản phẩm là 1.320.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Shu uemura (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả No. 5 Bond Maintenance",
                    ProductDescription =
                        "Dầu xả No. 5 Bond Maintenance được công thức đặc biệt để dưỡng ẩm và phục hồi tóc hư tổn trong khi giảm thiểu sự gãy rụng. Sản phẩm giúp duy trì sức khỏe của tóc sau các liệu pháp hóa học, mang lại mái tóc mềm mại, bóng khỏe và dễ dàng chải chuốt.",
                    Price = 675_000m, // Giá sản phẩm là 675.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Opalex (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu xả Gold Lust Repair & Restore",
                    ProductDescription =
                        "Dầu xả Gold Lust Repair & Restore cao cấp giúp tái tạo và phục hồi tóc hư tổn, đồng thời làm tăng độ bóng và mềm mại cho tóc. Chứa sự kết hợp của các loại dầu và chiết xuất có khả năng phục hồi, sản phẩm giúp củng cố tóc và ngăn ngừa hư tổn trong tương lai, phù hợp cho mọi loại tóc.",
                    Price = 1_490_000m, // Giá sản phẩm là 1.490.000 VND
                    Quantity = 1, // Dung tích là 250ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Oribe (CompanyId mặc định là 1)
                    Dimension = "250ml",
                    // Tổng dung tích là 250ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Mặt nạ dưỡng ẩm Signature",
                    ProductDescription =
                        "Mặt nạ dưỡng ẩm Signature cung cấp độ ẩm sâu và dưỡng chất cho tóc khô và dễ gãy. Với công thức giàu các thành phần tự nhiên, sản phẩm giúp cải thiện độ đàn hồi và phục hồi cân bằng độ ẩm tự nhiên của tóc, mang lại mái tóc mềm mại, dễ chải và khỏe mạnh.",
                    Price = 1_640_000m, // Giá sản phẩm là 1.640.000 VND
                    Quantity = 1, // Dung tích là 160ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Oribe (CompanyId mặc định là 1)
                    Dimension = "160ml",
                    // Tổng dung tích là 160ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Xả Phục Hồi Tối Ưu",
                    ProductDescription =
                        "Được thiết kế dành cho tóc hư tổn nghiêm trọng, dầu xả này mang đến khả năng phục hồi và cấp ẩm chuyên sâu. Giàu dưỡng chất từ thiên nhiên, sản phẩm giúp tăng cường sức mạnh và độ đàn hồi cho tóc, mang lại vẻ ngoài tràn đầy sức sống và dễ dàng tạo kiểu.",
                    Price = 1_320_000m, // Giá sản phẩm là 1.320.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Shu Uemura (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
                new Product
                {
                    Status = "Active",

                    ProductName = "Dầu Xả Invigo Nutri-Enrich",
                    ProductDescription =
                        "Được thiết kế dành cho tóc khô hoặc căng thẳng, dầu xả này giúp nuôi dưỡng và bổ sung độ ẩm. Chứa chiết xuất goji berry và vitamin E, giúp cải thiện độ đàn hồi và dễ chải tóc, mang lại vẻ ngoài mềm mại và khỏe mạnh.",
                    Price = 460_000m, // Giá sản phẩm là 460.000 VND
                    Quantity = 1, // Dung tích là 1000ml
                    // Không có chiết khấu
                    CategoryId = 10, // Conditioner
                    CompanyId = 1,
                    Brand = "xxx", // Wella Professionals (CompanyId mặc định là 1)
                    Dimension = "1000ml",
                    // Tổng dung tích là 1000ml
                },
            };

            // Thêm sản phẩm vào cơ sở dữ liệu
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
        }

        private async Task SeedServices()
        {
            // Lấy danh sách các ServiceCategory từ cơ sở dữ liệu
            var serviceCategories = await _context.ServiceCategory.ToListAsync();

            // Kiểm tra nếu có đủ 10 ServiceCategory
            if (serviceCategories.Count < 10)
            {
                throw new InvalidOperationException("Cần có ít nhất 10 ServiceCategory trong cơ sở dữ liệu.");
            }

            var services = new List<Service>
            {
                // Dịch vụ chăm sóc da mặt (10 dịch vụ cũ)
                new Service
                {
                    Status = "Active",
                    Name = "Chăm sóc da mặt đặc trưng",
                    Duration = "60",
                    Price = 600_000m,
                    ServiceCategoryId =
                        serviceCategories[0].ServiceCategoryId, // Chia dịch vụ vào ServiceCategory đầu tiên
                    Description = "Liệu pháp chăm sóc da mặt phù hợp với nhu cầu của làn da bạn.",
                    Steps =
                        "1. Tư vấn về da.\n2. Làm sạch.\n3. Tẩy tế bào chết.\n4. Đắp mặt nạ và dưỡng ẩm.\n5. Massage mặt thư giãn."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Chống lão hóa da mặt",
                    Duration = "90",
                    Price = 800_000m,
                    ServiceCategoryId = serviceCategories[1].ServiceCategoryId, // ServiceCategory thứ 2
                    Description = "Giảm nếp nhăn và phục hồi vẻ tươi trẻ rạng rỡ.",
                    Steps =
                        "1. Làm sạch.\n2. Thoa huyết thanh.\n3. Massage nâng cơ.\n4. Mặt nạ chống lão hóa.\n5. Bảo vệ SPF."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Dưỡng Ẩm",
                    Duration = "60",
                    Price = 500_000m,
                    ServiceCategoryId = serviceCategories[0].ServiceCategoryId, // Chia cho ServiceCategory thứ nhất
                    Description = "Cung cấp độ ẩm sâu và làm căng mọng làn da.",
                    Steps = "1. Làm sạch da.\n2. Thoa serum dưỡng ẩm.\n3. Đắp mặt nạ cấp ẩm.\n4. Thoa kem dưỡng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Làm Sáng Da",
                    Duration = "75",
                    Price = 650_000m,
                    ServiceCategoryId = serviceCategories[1].ServiceCategoryId, // ServiceCategory thứ 2
                    Description = "Giúp làm sáng làn da xỉn màu và không đều màu.",
                    Steps =
                        "1. Làm sạch da.\n2. Tẩy tế bào chết.\n3. Thoa serum vitamin C.\n4. Đắp mặt nạ làm sáng da.\n5. Thoa kem chống nắng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Trị Mụn",
                    Duration = "90",
                    Price = 700_000m,
                    ServiceCategoryId = serviceCategories[2].ServiceCategoryId, // ServiceCategory thứ 3
                    Description = "Giúp giảm mụn và ngăn ngừa tình trạng nổi mụn.",
                    Steps =
                        "1. Phân tích da.\n2. Làm sạch sâu.\n3. Điều trị điểm mụn.\n4. Đắp mặt nạ làm dịu da.\n5. Thoa kem chống nắng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Làm Dịu Da",
                    Duration = "60",
                    Price = 550_000m,
                    ServiceCategoryId = serviceCategories[3].ServiceCategoryId, // ServiceCategory thứ 4
                    Description = "Giúp làm dịu và giảm kích ứng cho làn da nhạy cảm.",
                    Steps =
                        "1. Làm sạch nhẹ nhàng.\n2. Đắp mặt nạ chống viêm.\n3. Massage thư giãn.\n4. Thoa kem dưỡng và chống nắng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Trà Xanh",
                    Duration = "60",
                    Price = 500_000m,
                    ServiceCategoryId = serviceCategories[2].ServiceCategoryId, // ServiceCategory thứ 3
                    Description = "Liệu pháp giàu chất chống oxy hóa giúp làn da tươi trẻ.",
                    Steps =
                        "1. Làm sạch da.\n2. Thoa serum chiết xuất trà xanh.\n3. Đắp mặt nạ chống oxy hóa.\n4. Thoa kem dưỡng và chống nắng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Tăng Cường Collagen",
                    Duration = "75",
                    Price = 700_000m,
                    ServiceCategoryId = serviceCategories[3].ServiceCategoryId, // ServiceCategory thứ 4
                    Description = "Thúc đẩy sản xuất collagen giúp da săn chắc hơn.",
                    Steps =
                        "1. Làm sạch da.\n2. Thoa serum collagen.\n3. Massage mặt.\n4. Đắp mặt nạ chứa collagen.\n5. Thoa kem chống nắng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Thải Độc Da",
                    Duration = "60",
                    Price = 600_000m,
                    ServiceCategoryId = serviceCategories[4].ServiceCategoryId, // ServiceCategory thứ 5
                    Description = "Thanh lọc và thải độc tố cho làn da.",
                    Steps =
                        "1. Làm sạch da.\n2. Đắp mặt nạ thải độc.\n3. Thoa serum dưỡng da.\n4. Thoa kem dưỡng và chống nắng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Dưỡng Ẩm Qua Đêm",
                    Duration = "90",
                    Price = 750_000m,
                    ServiceCategoryId = serviceCategories[4].ServiceCategoryId, // ServiceCategory thứ 5
                    Description = "Cung cấp độ ẩm chuyên sâu giúp da rạng rỡ vào sáng hôm sau.",
                    Steps =
                        "1. Làm sạch da.\n2. Thoa serum dưỡng ẩm qua đêm.\n3. Massage thư giãn.\n4. Đắp mặt nạ dưỡng ẩm ban đêm."
                },


                // Các dịch vụ mới
                new Service
                {
                    Status = "Active",
                    Name = "Massage Thụy Điển",
                    Duration = "60",
                    Price = 600_000m,
                    ServiceCategoryId = serviceCategories[5].ServiceCategoryId, // ServiceCategory thứ 6
                    Description = "Liệu pháp massage cổ điển giúp thư giãn và giảm căng thẳng.",
                    Steps = "1. Tư vấn.\n2. Massage Thụy Điển nhẹ nhàng.\n3. Sử dụng tinh dầu thơm để thư giãn."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Tẩy Tế Bào Chết Toàn Thân",
                    Duration = "75",
                    Price = 650_000m,
                    ServiceCategoryId = serviceCategories[6].ServiceCategoryId, // ServiceCategory thứ 7
                    Description = "Loại bỏ tế bào chết và tái tạo làn da.",
                    Steps =
                        "1. Thoa kem tẩy tế bào chết toàn thân.\n2. Tẩy da nhẹ nhàng.\n3. Rửa sạch và thoa kem dưỡng ẩm."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Quấn Dưỡng Ẩm",
                    Duration = "60",
                    Price = 600_000m,
                    ServiceCategoryId = serviceCategories[5].ServiceCategoryId, // ServiceCategory thứ 6
                    Description = "Dưỡng ẩm sâu dành cho làn da khô.",
                    Steps = "1. Tẩy tế bào chết toàn thân.\n2. Thoa lớp quấn dưỡng ẩm.\n3. Rửa sạch và thoa kem dưỡng."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Massage Liệu Pháp Hương Thơm",
                    Duration = "90",
                    Price = 750_000m,
                    ServiceCategoryId = serviceCategories[6].ServiceCategoryId, // ServiceCategory thứ 7
                    Description = "Massage kết hợp tinh dầu giúp thư giãn cơ thể.",
                    Steps =
                        "1. Tư vấn.\n2. Massage hương thơm tập trung vào vùng căng thẳng.\n3. Thoa tinh dầu thư giãn."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Massage Chân",
                    Duration = "45",
                    Price = 400_000m,
                    ServiceCategoryId = serviceCategories[7].ServiceCategoryId, // ServiceCategory thứ 8
                    Description = "Giúp thư giãn và làm dịu đôi chân mệt mỏi.",
                    Steps = "1. Ngâm chân nước ấm.\n2. Massage chân thư giãn.\n3. Thoa kem dưỡng ẩm."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Massage Vùng Bụng",
                    Duration = "30",
                    Price = 500_000m,
                    ServiceCategoryId = serviceCategories[8].ServiceCategoryId, // ServiceCategory thứ 9
                    Description = "Giúp giảm khó chịu ở vùng bụng và cải thiện tiêu hóa.",
                    Steps =
                        "1. Massage vùng bụng nhẹ nhàng.\n2. Sử dụng tinh dầu thư giãn.\n3. Tư vấn chăm sóc cá nhân."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Liệu Pháp Thải Độc Cơ Thể",
                    Duration = "90",
                    Price = 800_000m,
                    ServiceCategoryId = serviceCategories[7].ServiceCategoryId, // ServiceCategory thứ 8
                    Description = "Thải độc và thanh lọc cơ thể.",
                    Steps =
                        "1. Tẩy tế bào chết toàn thân.\n2. Đắp mặt nạ thải độc.\n3. Massage thư giãn.\n4. Thoa kem dưỡng ẩm."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Tắm Bùn",
                    Duration = "75",
                    Price = 700_000m,
                    ServiceCategoryId = serviceCategories[8].ServiceCategoryId, // ServiceCategory thứ 9
                    Description = "Liệu pháp bùn toàn thân giúp thải độc tố.",
                    Steps = "1. Thoa lớp bùn ấm lên cơ thể.\n2. Nghỉ ngơi thư giãn.\n3. Rửa sạch và dưỡng ẩm."
                },
                new Service
                {
                    Status = "Active",
                    Name = "Tẩy Bóng Da",
                    Duration = "60",
                    Price = 600_000m,
                    ServiceCategoryId = serviceCategories[9].ServiceCategoryId, // ServiceCategory thứ 10
                    Description = "Nhẹ nhàng loại bỏ tế bào chết và làm mịn da.",
                    Steps = "1. Thoa kem tẩy bóng da.\n2. Tẩy da chết và làm sạch.\n3. Dưỡng ẩm bằng kem dưỡng da."
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
                    CompanyId = 1, // ID của công ty đã seed trước đó
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
                "Khuyến Mãi Ngày Lễ",
                "Giảm Giá Black Friday",
                "Khuyến Mãi Năm Mới",
                "Ưu Đãi Đặc Biệt Mùa Hè",
                "Giảm Giá Chớp Nhoáng",
                "Giảm Giá Xả Kho",
                "Mua Nhiều Tiết Kiệm Hơn"
            };

            var promotionDescriptions = new[]
            {
                "Nhận ưu đãi tuyệt vời trong mùa lễ này!",
                "Đợt giảm giá lớn nhất trong năm vào Black Friday!",
                "Chào đón năm mới với những ưu đãi độc quyền!",
                "Giải nhiệt mùa hè với những khuyến mãi nóng bỏng!",
                "Ưu đãi có thời hạn – đừng bỏ lỡ!",
                "Giảm giá xả kho cho các mặt hàng được chọn!",
                "Mua càng nhiều, tiết kiệm càng lớn với ưu đãi đặc biệt!"
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
                foreach (var product in products)
                {
                    var branchProduct = new Branch_Product
                    {
                        BranchId = branch.BranchId,
                        ProductId = product.ProductId,
                        Status = ObjectStatus.Active.ToString(),
                        StockQuantity = random.Next(5, 51), // Số lượng tồn kho từ 5 đến 50
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    branchProducts.Add(branchProduct);
                }
            }

            await _context.Branch_Products.AddRangeAsync(branchProducts);
            await _context.SaveChangesAsync();
        }


        private async Task SeedBranchServices()
        {
            var branches = await _context.Branchs.ToListAsync();
            var services = await _context.Services.ToListAsync();

            var branchServices = new List<Branch_Service>();

            foreach (var branch in branches)
            {
                foreach (var service in services)
                {
                    var branchService = new Branch_Service
                    {
                        BranchId = branch.BranchId,
                        ServiceId = service.ServiceId,
                        Status = ObjectStatus.Active.ToString(),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    branchServices.Add(branchService);
                }
            }

            await _context.Branch_Services.AddRangeAsync(branchServices);
            await _context.SaveChangesAsync();
        }


        private async Task SeedBranchPromotions()
        {
            var branches = await _context.Branchs.ToListAsync();
            var promotions = await _context.Promotions.ToListAsync();

            var branchPromotions = new List<Branch_Promotion>();

            foreach (var branch in branches)
            {
                foreach (var promotion in promotions)
                {
                    var branchPromotion = new Branch_Promotion
                    {
                        BranchId = branch.BranchId,
                        PromotionId = promotion.PromotionId,
                        Status = "Active", // Bạn có thể thay đổi thành "Pending" hoặc ngẫu nhiên nếu muốn
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    branchPromotions.Add(branchPromotion);
                }
            }

            await _context.Branch_Promotions.AddRangeAsync(branchPromotions);
            await _context.SaveChangesAsync();
        }


        private async Task SeedStaff()
        {
            var staffUsers = await _context.Users.Where(u => u.RoleID == 4).ToListAsync();

            if (staffUsers.Count < 55)
            {
                throw new Exception("Không đủ người dùng nhân viên trong cơ sở dữ liệu. Vui lòng thêm người dùng.");
            }

            var branches = await _context.Branchs.ToListAsync();
            if (branches.Count < 5)
            {
                throw new Exception("Not enough branches found. Please seed at least 5 branches.");
            }

            var defaultRole = await _context.StaffRole.FirstOrDefaultAsync(r => r.StaffRoleName == "DefaultStaff");
            var cashierRole = await _context.StaffRole.FirstOrDefaultAsync(r => r.StaffRoleName == "Cashier");
            var specialistRole = await _context.StaffRole.FirstOrDefaultAsync(r => r.StaffRoleName == "Specialist");

            if (defaultRole == null || cashierRole == null || specialistRole == null)
            {
                throw new Exception("Không tìm thấy vai trò nhân viên. Vui lòng kiểm tra lại dữ liệu.");
            }

            var staffList = new List<Staff>();
            int userIndex = 0;

            foreach (var branch in branches)
            {
                // 1. Nhân viên đầu tiên là "DefaultStaff"
                staffList.Add(new Staff
                {
                    UserId = staffUsers[userIndex++].UserId,
                    BranchId = branch.BranchId,
                    RoleId = defaultRole.StaffRoleId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });

                // 2. Nhân viên thứ hai là "Cashier"
                staffList.Add(new Staff
                {
                    UserId = staffUsers[userIndex++].UserId,
                    BranchId = branch.BranchId,
                    RoleId = cashierRole.StaffRoleId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });

                // 3. 9 nhân viên còn lại là "Specialist"
                for (int i = 0; i < 9; i++)
                {
                    staffList.Add(new Staff
                    {
                        UserId = staffUsers[userIndex++].UserId,
                        BranchId = branch.BranchId,
                        RoleId = specialistRole.StaffRoleId,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    });
                }
            }

            await _context.Staffs.AddRangeAsync(staffList);
            await _context.SaveChangesAsync();
        }


        private async Task SeedStaffServiceCategory()
        {
            var staffs = await _context.Staffs.ToListAsync();
            var serviceCategories = await _context.ServiceCategory.ToListAsync();

            if (!staffs.Any() || serviceCategories.Count < 5)
            {
                throw new Exception("Không đủ nhân viên hoặc danh mục dịch vụ để gán.");
            }

            var staffCategories = new List<Staff_ServiceCategory>();
            var random = new Random();

            foreach (var staff in staffs)
            {
                var selectedCategories = serviceCategories.OrderBy(x => random.Next()).Take(5).ToList();

                foreach (var category in selectedCategories)
                {
                    staffCategories.Add(new Staff_ServiceCategory
                    {
                        StaffId = staff.StaffId,
                        ServiceCategoryId = category.ServiceCategoryId,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    });
                }
            }

            await _context.Staff_ServiceCategory.AddRangeAsync(staffCategories);
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
                ("Kem Dầu Tẩy Trang Remedy", new List<string>
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
                ("Sữa Rửa Mặt Cơ Bản", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/Active-Pureness-Gel_San-pham-1091x1200.png",
                    "https://icgroup.dk/resources/product/119/43/activec-pureness---gel.png?width=800&height=600",
                    "https://www.michaelahann.at/wp-content/uploads/2022/08/Active-Pureness-Gel.jpg"
                }),

                // Clearing Skin Wash
                ("Gel Rửa Mặt Làm Sạch Sâu", new List<string>
                {
                    "https://www.dermalogica.com/cdn/shop/files/clearing-skin-wash_8.4oz_front.jpg?v=1710455212&width=1946",
                    "https://vn-test-11.slatic.net/p/1c096bc4b5b03330c62465154db802d2.jpg",
                    "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/25/images/dermalogica-clearing-skin-wash-250ml-2.jpg"
                }),

                // Oil To Foam Cleanser
                ("Sữa Rửa Mặt Làm Sạch Mụn", new List<string>
                {
                    "https://sieuthilamdep.com/images/detailed/20/sua-rua-mat-tay-trang-2-trong-1-dermalogica-oil-to-foam-total-cleanser.jpg",
                    "https://www.thedermacompany.co.uk/wp-content/uploads/2023/06/Dermalogica-Oil-To-Foam-Cleanser-Lifestyle.jpg",
                    "https://skinmart.com.au/cdn/shop/products/DermalogicaOilToFoamCleanser_2_2560x.png?v=1680137764"
                }),

                // Micellar Prebiotic PreCleanse
                ("Dầu Tẩy Trang Prebiotic Micellar", new List<string>
                {
                    "https://dermalogica.com.vn/cdn/shop/products/dermalogica-vietnam-cleansers-150ml-coming-soon-micellar-precleanse-s-a-t-y-trang-ch-a-prebiotic-danh-cho-m-i-lo-i-da-31640176623821.png?v=1717214352&width=1946",
                    "https://edbeauty.vn/wp-content/uploads/2024/08/image_2024_08_02T10_27_06_857Z.png",
                    "https://cdn1.parfuemerie-becker.de/media/cache/article/variation/109000/035020189_95635_1709027057.png"
                }),

                // Kmobucha Microbiome Foaming Cleaser
                ("Sữa Rửa Mặt Kombucha", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-kombucha-microbiome-foaming-cleanser-pdp.jpg?itok=XFjrWeJc",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAs0nEmyqyQbx0hl4vz1MUoLloMBlBJTO4QQ&s",
                    "https://emstore.com/cdn/shop/products/cleanser-2_75951af1-f4c2-4d54-80dd-9936b699a5c8.png?v=1683718720"
                }),

                // Monoi Age Corrective Exfoliating Cleanser
                ("Sữa Rửa Mặt Tẩy Tế Bào Chết Monoi", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-monoi-age-corrective-exfoliating-cleanser-pdp-compressed.jpg?itok=pRBEP6CO",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT7vbzeB98PqQkVa4vQKztlvJZTh1-H0DWPF62hO-Uo3_PGI5iz4E1jFv_p2VAD2LrgA-c&usqp=CAU",
                    "https://www.facethefuture.co.uk/cdn/shop/files/eminence-organics-monoi-age-corrective-exfoliating-cleanser-swatch-compressed.jpg?v=1695310185&width=600"
                }),

                // Acne Advanced Cleansing Foam
                ("Sữa Rửa Mặt Ngừa Mụn", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-acne-advanced-cleansing-foam-v2-400pix-compressor.jpg?itok=7eKH7RYv",
                    "https://wildflowerbeautystudio.ca/cdn/shop/products/acnecleansingfoam_300x300.jpg?v=1613861748",
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-slide/eminence-organics-acne-advanced-cleansing-foam-swatch-400x400.jpg?itok=_451iEc7"
                }),

                // Lemon Grass Cleanser
                ("Sữa Rửa Mặt Cỏ Chanh", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQg8RxjJM_IxA0iIIBfpofAqmoH7QLa7zIy2Q&s",
                    "https://images.squarespace-cdn.com/content/v1/5ea87ac4bf0b761180ffcfae/1626276341235-D5E9PCNK5OSRIP2RALJN/LemonGrassCleanser.jpg?format=1000w",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTcyaLelaqtVcZiyeWTM4QOoJVXWQveYuorDw&s"
                }),

                // Charcoal Exfoliating Gel Cleanser
                ("Sữa Rửa Mặt Tẩy Tế Bào Chết Than Hoạt Tính", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQcNxhG4XNakMZOCjxT2ulB1i2cnWkTVq1qxw&s",
                    "https://naturalbeautygroup.com/cdn/shop/files/Eminence-Organics-Charcoal-Exfoliating-Gel-Cleanser-Lifestyle.jpg?v=1711740188&width=1080",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRbNf8GcQLHfBysKN1piBJLl5kX-ovUWTfWhw&s"
                }),
                // Beplain Mung Bean pH-Balanced Cleansing Foam
                ("Sữa Rửa Mặt Beplain Mung Bean pH-Balanced", new List<string>
                {
                    "https://product.hstatic.net/200000773671/product/907d907dd544731a2a55_97fc43aed51040a58f76b2512b39f457_master.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT1ZAGpmlp5k_1VB64FLzA6HfAkRMDHXybYEw&s",
                    "https://product.hstatic.net/200000773671/product/ece36b53526af434ad7b_7e25a0c0d2f641e9a0fcfe8371587fca_master.jpg"
                }),
                // ISNTREE Yam Root Vegan Milk Cleanser
                ("Sữa Rửa Mặt ISNTREE Yam Root Vegan Milk", new List<string>
                {
                    "https://www.kanvasbeauty.com.au/cdn/shop/files/7_5a516b9f-1e89-47af-a0f3-c9e419cbee24_1200x.jpg?v=1711882846",
                    "https://www.skincupid.co.uk/cdn/shop/files/ISNTREEYamRootVeganMilkCleanser_220ml_5.png?v=1728904708&width=800",
                    "https://koreanskincare.nl/cdn/shop/files/452359592_470675092412851_8771590449487260742_n.jpg?v=1721894307"
                }),

                // Normaderm Anti-Acne Purifying Gel Cleanser
                ("Gel Rửa Mặt Trị Mụn Normaderm", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAztQlHO_gcl1kdZRAKV5QSYVFcAVKnen6yg&s",
                    "https://sohaticare.com/cdn/shop/files/3337875663076_3_82da998a-a47b-4461-a110-036d702f4886_4000x@3x.progressive.jpg?v=1706877499",
                    "https://bng.com.pk/cdn/shop/files/e3906fe2-cb9a-47d8-8a03-73dc6d7f25ce_0df3ce49-2f5b-4d45-b6f7-7db99714a57e_2400x.jpg?v=1720805633"
                }),

                // Purete Thermale Fresh Cleansing Gel
                ("Gel Rửa Mặt Tươi Mát Purete Thermale", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTBGIU-XNebd-dJK6_u0DMg5l1MKiD1l4zCJg&s",
                    "https://www.binsina.ae/media/catalog/product/8/1/81414_2.jpg?optimize=medium&bg-color=255,255,255&fit=bounds&height=600&width=600&canvas=600:600",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSw-hWi1mf7kCZzWrthwsn-3Opba1kYnJcQmA&s"
                }),

                // Purete Thermale Cleansing Foaming Cream
                ("Kem Rửa Mặt Tạo Bọt Purete Thermale", new List<string>
                {
                    "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/other-products/purete-thermale---hydrating-and-cleansing-foaming-cream/hydrating-cleansing-foaming-cream-pack2.jpeg?rev=8a1f1df622f24bfe963a86befb0031b4&sc_lang=vi-vn&cx=0.47&cy=0.43&cw=525&ch=596&hash=D4ABC102BDEC8E803268FC703A80B685",
                    "https://images-na.ssl-images-amazon.com/images/I/81fpwzdjEgL.jpg",
                    "https://m.media-amazon.com/images/I/71e3vIZEiBL._AC_UF1000,1000_QL80_.jpg"
                }),

                // Foaming Cream Cleanser
                ("Sữa Rửa Mặt Tạo Bọt", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/021924_FoamingCleanser_Carousel-Hero_1024x1024.jpg?v=1709334047",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRhMF2XnUc1FGnqMiZFBkLTUJJLw8mCOGt05A&s",
                    "https://templeskincare.com.au/wp-content/uploads/2024/10/20231026_027-Edit_5000px-Edit-CreamCleanser-scaled.jpg"
                }),

                // Exfoliating Cleanser
                ("Sữa Rửa Mặt Tẩy Tế Bào Chết", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/010924_Retail_ExfoliatingCleanser_PDP_1024x1024.jpg?v=1713463787",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRn6R3GviQQlSKoSH8nmK0q8PPuwSwUefS3Sg&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTNWmj3G1El9VvSp-muGnch-XOIXykwLaZa4w&s"
                }),

                // Cleansing Gel Face Wash
                ("Gel Rửa Mặt Làm Sạch", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQkaX2LqQ8a7zqysEzX98U1KhUdN6kjc6i22Q&s",
                    "https://images.squarespace-cdn.com/content/v1/5badc17c797f743dc830bb95/1720164751711-2KJJ7A6MK6WH95C7JGG9/HydroPeptide+Cleansing+Gel+Perth.png?format=1000w",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ9BYzA_QpTeT5wLVuqIQEQ0x6fMYRUEEdbzTqiVagtp3fB2bZ44hGnXnVWD7bfsUBXpGE&usqp=CAU"
                }),

                // Mangosteen Revitalizing Mist
                ("Xịt Khoáng Măng Cụt", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRhqpsm4vwoEDeWoXmYThy0w7GBLITExzxFkg&s",
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-slide/eminence-organics-mangosteen-revitalizing_mist_swatch-400x400px-compressed.png?itok=9ia6NZD4",
                    "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Mangosteen-Revitalizing-Mist-Lifestyle.jpg?v=1711740433&width=1080"
                }),

                // Pineapple Refining Tonique
                ("Nước Hoa Hồng Tinh Chất Dứa", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSipnwI-Quyzdj8X2z555QL6Nv8dXGypcpFUw&s",
                    "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Pineapple-Refining-Tonique-lifestyle.jpg?v=1711743675&width=1080",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRXvTAya614P0QjS3xR5k9bA9QyUs8DLnn72Q&s"
                }),

                // Hawthorn Tonique
                ("Nước Hoa Hồng Táo Gai", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQWgXY3ygvCD1JS1ZJM_UQB0DlglHmITznI7A&s",
                    "https://images.squarespace-cdn.com/content/v1/5ea87ac4bf0b761180ffcfae/1626276012659-MUTJNN1LVDNA49X09L55/Hawthorn+Tonique.jpg?format=1000w",
                }),

                // Lime Refresh Tonique
                ("Nước Hoa Hồng Tinh Chất Chanh", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-lime-refresh-tonique-400x400px.png?itok=75tgznFH",
                    "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Lime-Refresh-Tonique-Lifestyle.png?v=1711744338&width=1080",
                    "https://eminenceorganics.com/sites/default/files/article-image/eminence-organics-tonique.jpg"
                }),

                //Soothing Chamomile Tonique
                ("Nước Hoa Hồng Hoa Cúc Làm Dịu Da", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR0OK4AjASttojjEQbjJa9vTR7cy-Vf1UcAag&s",
                    "https://blog.skin-beauty.com/wp-content/uploads/2020/08/soothing-chamomile-tonique__64126.1586549554.1280.1280.jpg",
                }),

                // Multi-Acne Toner 
                ("Nước Hoa Hồng Đặc Trị Mụn", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/2-3.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQOyk2bFufKdc__jGiQJnFZxau1_kg7OmQUpg&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQK3uQghHwcl19vgzc-ZLG9e4hDV_aFbE4nwwwES3x6LrdmIl2zlBrhTBgg5lGrH6ST_5U&usqp=CAU"
                }),

                // Antioxidant Hydramist
                ("Xịt Khoáng Chống Oxy Hóa", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/2-117.jpg",
                    "https://www.dermalogica.co.uk/cdn/shop/products/Antioxidant-Hydramist-pdp-2.jpg?v=1721383738&width=1946",
                    "https://bellelab.co/wp-content/uploads/2019/12/Dermalogica_Antioxidant_Hydramist_150ml_3.jpg"
                }),

                // UltraCalming Mist
                ("Xịt Dưỡng Ẩm UltraCalming", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2020/05/2.jpg",
                    "https://myvienhana.vn/wp-content/uploads/2022/03/xit-khoang-dermalogica-Ultracalming-Mist.jpg",
                    "https://116805005.cdn6.editmysite.com/uploads/1/1/6/8/116805005/s595587978454154049_p56_i2_w1080.jpeg"
                }),

                // Hyaluronic Ceramide Mist
                ("Xịt Dưỡng Ẩm Hyaluronic Ceramide", new List<string>
                {
                    "https://www.dermalogica.com/cdn/shop/files/hyaluronic-ceramide-mist_front.jpg?v=1698103421&width=1946",
                    "https://sieuthilamdep.com/images/detailed/19/xit-khoang-cap-am-va-lam-diu-da-dermalogica-hyaluronic-ceramide-mist-2.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQV0eQsaz5GoUd5mI9LsEXP5mb0IEO7zlvz3A&s"
                }),

                // Remedy Toner 
                ("Nước Hoa Hồng Làm Dịu Da", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR5fMbd6K1rQjSbxMXbFo36ZZHjthpqr4lpDQ&s",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/Remedy-1-02-1091x1200.png",
                    "https://vn-test-11.slatic.net/p/5fe09d9ffdf46df663e3dfcd64fcf4c5.jpg"
                }),

                // Essential Toner
                ("Nước Hoa Hồng Cân Bằng Da", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/Essential-Toner_San-pham-1091x1200.png",
                    "https://hadibeauty.com/wp-content/uploads/2023/03/334204861_942692140091925_7678139925329751463_n.webp",
                }),

                // Active Pureness Toner
                ("Nước Hoa Hồng Làm Sạch Sâu", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/Active-Pureness-Toner_San-pham-1091x1200.png",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/cdd49c856665c35cd1ca025ee14c26d5429d3d2c_2000x-1091x1200.jpg",
                }),

                // Revitalizing Tonic
                ("Nước Hoa Hồng Tái Tạo Da", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTkXA3c63LygDEehGwwAJm2Y_SxgWwEt1vz8w&s",
                    "https://comfortzone.com.vn/wp-content/uploads/2023/01/8004608516330_4-1200x1200.jpg",
                    "https://cf.shopee.vn/file/vn-11134201-23030-p5amrfvba8nv3a"
                }),
                // Acwell Licorice pH Balancing Cleansing Toner
                ("Nước Hoa Hồng Cân Bằng pH Acwell Licorice", new List<string>
                {
                    "https://sokoglam.com/cdn/shop/files/SokoGlamPDP_Acwell_Revamped_Licorice_pH_Balancing_Cleansing_Toner-4_860x.png?v=1729736947",
                    "https://www.mikaela-beauty.com/cdn/shop/files/AX6H2549w_1200x1200.jpg?v=1720903305",
                }),

                // COSRX AHA/BHA Clarifying Treatment Toner
                ("Nước Hoa Hồng COSRX AHA/BHA", new List<string>
                {
                    "https://product.hstatic.net/1000006063/product/cosrx_ahabha_clarifying_treatment_toner_150ml_625a49f8074c41c59c9d185e582f0580_1024x1024.jpg",
                    "https://assets.aemi.vn/webp/CRX_TNR_150ml_001_img2.webp",
                }),
                // Sulwhasoo Concentrated Ginseng Renewing Water
                ("Nước Cân Bằng Nhân Sâm Sulwhasoo", new List<string>
                {
                    "https://cdn.shopify.com/s/files/1/0667/9416/0378/files/concentrated_ginseng_rejuvenating_water_kv_pc_vn_240819.jpg?v=1724121219",
                    "https://th.sulwhasoo.com/cdn/shop/files/TN-CGR-Water-1.jpg?v=1734325148",
                }),
                // Pre-Treatment Toner
                ("Toner Dưỡng Da Trước Điều Trị", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/011024_Pre-TreatmentToner_PDP_1024x1024.jpg?v=1711563597",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT0ITo2GuTRZWHrbdTatJFRzQcwsN5jDNfHOw&s",
                    "https://134449990.cdn6.editmysite.com/uploads/1/3/4/4/134449990/s148490656276618525_p144_i1_w550.jpeg"
                }),
                // Hydraflora
                ("Nước Cân Bằng Hydraflora", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_HydraFlora_PDP_eea6942c-bdcc-47a2-adf3-c2632906ffc3_grande.jpg?v=1730921087",
                    "https://hydropeptide.com/cdn/shop/files/020724_Swatch_Hydraflora_PDP_0f209cc8-b2dd-42ca-bd9d-06a83eb0d376_1024x1024.jpg?v=1730921090",
                }),
                // Clarifying Toner Pads
                ("Miếng Lót Toner Làm Sạch", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_ClarifyingToner_PDP_grande.jpg?v=1715974145",
                    "https://metafields-manager-by-hulkapps.s3-accelerate.amazonaws.com/uploads/hydropeptide-canada.myshopify.com/1718388492-022624_ClarifyingToner_BENEFITS.jpg",
                }),
                // Toner Vichy Aqualia Thermal Hydrating Refreshing Water
                ("Toner Vichy Aqualia Thermal Dưỡng Ẩm Tươi Mát", new List<string>
                {
                    "https://trungsoncare.com/images/detailed/10/1_n7ix-l0.png",
                    "https://ordinaryvietnam.net/wp-content/uploads/2022/03/Nuoc-hoa-hong-Vichy-Aqualia-Thermal-Hydrating-Refreshing-Water-Ordinary-Viet-Nam-3-600x600.jpg",
                }),
                // Toner Vichy Normaderm acne-prone skin purifying pore-tightening lotion
                ("Toner Vichy Normaderm Se Khít Lỗ Chân Lông",
                    new List<string>
                    {
                        "https://storage.beautyfulls.com/uploads-1/thanhhuong/2022/vichy/toner/vichy-normaderm-purifying-pore-tightening/nuoc-hoa-hong-vichy.jpg",
                        "https://escentual.com/cdn/shop/files/vichy_normaderm_purifying_pore-tightening_toning_lotion_200ml_2.png?v=1729191893",
                    }),
                // Sublime Skin Intensive Serum
                ("Serum Chăm Sóc Da Chuyên Sâu Sublime Skin", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2023/05/sublime-skin-07-1-1091x1200.png",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5pQrlNdExrdzyuV56Zv4vX2yGLGl1VR6juQ&s",
                }),
                // Hydramemory Hydra & Glow Ampoules
                ("Tinh Chất Dưỡng Ẩm & Làm Sáng Hydramemory", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2023/04/8004608510871%E2%80%8B_2-1091x1200.jpg",
                    "https://vonpreen.com/wp-content/uploads/2023/02/duong-da-Comfort-Zone-Hyramemory-Hydra-Glow-Ampoule-7-ong-x-2ml.jpg",
                }),
                // Subline Skin Lift & Firm Ampoule
                ("Tinh Chất Nâng Cơ & Căng Da Sublime Skin", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/ampollesublime_2000x-1091x1200.jpg",
                    "https://livelovespa.com/cdn/shop/products/Untitleddesign_33_626c334a-5cfe-4a98-9fd6-f1c4faf46476.png?v=1650075373&width=2048",
                }),
                // Biolumin-C Serum
                ("Serum Vitamin C Biolumin-C", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2020/05/2-1.jpg",
                    "https://stralabeauty.com/wp-content/uploads/2022/05/111341-dermalogica-biolumin-c-serum-open.jpg",
                }),
                // Age Bright Clearing Serum
                ("Serum Giảm Mụn & Lão Hóa Age Bright", new List<string>
                {
                    "https://www.facethefuture.co.uk/cdn/shop/files/111342-lifestyle-1_1750x1750_7f55ce82-2824-4c25-aa9b-e14e9f84f1d2.jpg?v=1695286223&width=600",
                    "https://dermalogica.com.vn/cdn/shop/products/facial-oils-and-serums-facial-oils-and-serums-30ml-age-bright-clearing-serum-30198061039821.png?v=1718765966&width=1445",
                    "https://veevee.store/wp-content/uploads/2023/10/dermalogica-age-bright-clearing-serum-2.webp"
                }),
                // Powerbright Dark Spot Serum
                ("Serum Làm Mờ Đốm Nâu Powerbright", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/dermalogica-vietnam-powerbright-dark-spot-serum-29677731414221_707x707.jpg",
                    "https://edbeauty.vn/wp-content/uploads/2023/08/Tinh-chat-duong-sang-da-Dermalogica-Powerbright-Dark-Spot-Serum-2.jpg",
                    "https://dermalogica-vietnam.com/wp-content/uploads/2019/05/power-serum-2.jpg"
                }),
                // UltraCalming Serum Concentrate
                ("Serum Tinh Chất Làm Dịu Da UltraCalming", new List<string>
                {
                    "https://cdn.dangcapphaidep.vn/wp-content/uploads/2018/06/Dermalogica-Ultracalming%E2%84%A2-Serum-Concentrate-1.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSk6cqAfkxjQz5Mbmi6kkvBSj-ktU8a-Ufmng&s",
                }),
                // Circular Hydration Serum With Hyaluronic Acid
                ("Serum Cấp Ẩm Hyaluronic Circular", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2024/03/Huyet-Thanh-Cap-Am-Chuyen-Sau-Circular-Hydration-Serum-30ml.jpg",
                    "https://www.depmoingay.net.vn/wp-content/uploads/2023/08/Tinh-chat-cap-am-chuyen-sau-Dermalogica-Circular-Hydration-Serum.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQNmRYaTc9DUWC6Db28yQs8xl-cPbyLqsHfUQ&s"
                }),
                // Strawberry Rhubarb Hyaluronic Serum
                ("Serum Dưỡng Ẩm Dâu & Đại Hoàng", new List<string>
                {
                    "https://beautyritual.ca/cdn/shop/products/eminence-organics-strawberry-rhubarb-hyaluronic-serum-swatch_6209ac7f-cf39-4122-b2a3-98a2d6c150ae.jpg?v=1722965085&width=480",
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-strawberry-rhubarb-hyaluronic-serum.jpg?itok=MnXS_0td",
                }),
                // Citrus & Kale Potent C+E Serum
                ("Serum C+E Chống Oxy Hóa Citrus & Kale", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-citrus-kale-potent-ce-serum-400x400px_0.png?itok=2SnWNB_z",
                    "https://store-cdn-media.dermpro.com/catalog/product/cache/10f519365b01716ddb90abc57de5a837/e/m/eminence_citrus_kale_potent_c_e_serum_2.jpg",
                    "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Citrus-Kale-Potent-C-E-Serum-Lifestyle.png?v=1711744368&width=1080"
                }),
                // Marine Flower Peptide Serum
                ("Phục hồi da Marine Flower Peptide Serum", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQeNfYFOyzx2eItOl_4C-VnSTtOqdEH8_Bp9w&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSYaWbbx2Sz426MAirXQgrJ9YqBH75dQxB2gw&s",
                    "https://images.squarespace-cdn.com/content/v1/5b3ca5a2b98a782f09c815d6/1669510664302-2M38DWMSGWL2H9N0GR6I/Fall-2022-Social-Media-Package-2-1080x1080.jpg"
                }),
                // Clear Skin Willow Bark Booster-Serum
                ("Phục hồi da Clear Skin Willow Bark Booster-Serum", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQN_Wauo_Rlilox29NSTEBgTFF9vxbY62PLZg&s",
                    "https://the-afternoon-prod.s3.ap-southeast-1.amazonaws.com/product/02644f3a-9594-42b3-b857-6a1753db3a84/block/k-t-602-copy1-min.jpg",
                }),
                // Cornflower Recovery Serum
                ("Phục hồi da Cornflower Recovery Serum", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-cornflower-recovery-serum-sq.jpg?itok=nIX5IZVH",
                    "https://www.dermstore.com/images?url=https://static.thcdn.com/productimg/original/11857248-1344866735395349.jpg&format=webp&auto=avif&width=1200&height=1200&fit=cover",
                }),
                // Power Serum
                ("Ngăn lão hóa Power Serum", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQV7CdjpeMqLmybea_wuyU1YkJaN9H56D83bg&s",
                    "https://images.squarespace-cdn.com/content/v1/5badc17c797f743dc830bb95/1721113519111-GS0XBO7FD4FEU9TPD5QZ/HydroPeptide+Power+Serum+.png?format=1000w",
                    "https://img-cdn.heureka.group/v1/975eff79-baec-4106-87d5-641a98f2bc61.jpg?width=350&height=350"
                }),
                // Firma-Bright
                ("Sáng da Firma-Bright", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR6XcpSDG4i6iLlhr1d8tah8jg9YvwaeHAJqg&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTxCwXA3JgUVHri_LMGzNtf2Xrkig-zmb6AvA&s",
                    "https://metafields-manager-by-hulkapps.s3-accelerate.amazonaws.com/uploads/hydropeptide-canada.myshopify.com/1726623141-082224_New_Retail_Packaging_Firma-Bright_BENEFITS.jpg"
                }),
                // Hydrostem Serum
                ("Dưỡng ẩm Hydrostem Serum", new List<string>
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
                ("Cấp ẩm Minéral 89 Probiotic Fractions", new List<string>
                {
                    "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/skincare/m89/mineral89-probiotic-fractions-pack3.jpg?rev=8745dcf5b4fb41dbbf921950bebe6ae3&cx=0.53&cy=0.55&cw=525&ch=596&hash=3CB4E2F0FB27C99F34CEF2B4B60F94C5",
                    "https://product.hstatic.net/1000006063/product/2_9c06e1144bc44c31976a4726501e6936_1024x1024.jpg",
                    "https://product.hstatic.net/200000617989/product/tinh_chat_vichy_mineral_89_probiotic_fractions___2__fdb740bf564c4b6eb8ce809b420b1d4c.jpg"
                }),
                // Barrier Builder
                ("Bảo vệ da Barrier Builder", new List<string>
                {
                    "https://hydropeptide.co.uk/cdn/shop/files/Barrier-Builder.jpg?v=1725351085",
                    "https://cdn.shopify.com/s/files/1/0345/0444/1995/files/how-to.jpg?v=1725358910",
                }),
                // Power Luxe
                ("Dưỡng ẩm Power Luxe", new List<string>
                {
                    "https://skinbeautifulrx.com/cdn/shop/products/PowerLuxe_1200x.jpg?v=1629145994",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSc1zwGe1F7e0YfxPgDTe8hUHB8978RAybE_Q&s",
                    "https://abeautybar.com.ua/content/images/35/1080x1071l80mc0/gidropitatelnyy-infuzionnyy-krem-hydropeptide-power-luxe4784-36045340977099.jpeg"
                }),
                // AquaBoost Oil Free Face Moisturizer
                ("Kem dưỡng ẩm không chứa dầu AquaBoost", new List<string>
                {
                    "https://cdn.cosmostore.org/cache/front/shop/products/511/1558828/650x650.jpg",
                    "https://images.squarespace-cdn.com/content/v1/64930aaaf0c0fc7ee1e1ffeb/1713368640714-4KTAPH5GBPSDEBBFLW8P/Hydropeptide+Aquaboost+example.png?format=1500w",
                }),
                // Face Lift Moisturizer
                ("Sáng và nâng da Face Lift Moisturizer", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/010924_New_Retail_FaceLift_PDP_grande.jpg?v=1711563449",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR999M9m99s-z5kSEiW-HqQchDa2YY783A04w&s",
                }),
                // Sublime Skin Fluid Cream
                ("Dưỡng ẩm Sublime Skin Fluid Cream", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2023/03/12201_1_SUBLIME_SKIN_Fluid_Cream_60ml_Comfort_Zone_2000x-1091x1200.webp",
                    "https://comfortzone.com.vn/wp-content/uploads/2023/03/sublime-skin-texture-fluid-cream.png",
                }),
                // Sacred Nature Nutrient Cream
                ("Dưỡng ẩm Sacred Nature Nutrient Cream", new List<string>
                {
                    "https://world.comfortzoneskin.com/cdn/shop/files/o3rmembdmxcpkfgj5rau_1600x.jpg?v=1718130042",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/6939f8f00a8a7ed77a04d50b9549311e267a48fe_2000x-1091x1200.jpg",
                }),
                // Active Pureness Fluid
                ("Dưỡng ẩm Active Pureness Fluid", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2023/11/San-pham-134-1091x1200.jpg",
                    "https://www.organicpavilion.com/cdn/shop/products/PF8aa29d2d702ea16e288b6fbaad4bf3bd52092193_2000x_6c1ce0b0-1fa2-4ae4-98bc-fc537be81713_large.png?v=1677234394",
                }),
                // Remedy Cream 
                ("Dưỡng ẩm Remedy Cream", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/Remedy-1-03-1091x1200.png",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/c9d0b89d8a9af00e2eb8909d8c768e15158b6d7e_2000x-1091x1200.jpg",
                }),
                // Strawberry Rhubarb Hyaluronic Hydrator
                ("Dưỡng ẩm Strawberry Rhubarb Hyaluronic Hydrator", new List<string>
                {
                    "https://www.everyoneblooms.com/wp-content/uploads/2024/07/Eminence-Organics-Strawberry-Rhubarb-Hyaluronic-Hydrator-SQ.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTdwtvJgu9013N_fBXR36JJrEmupTNpKqo17Q&s",
                }),
                // Bakuchiol + Niacinamide Moisturizer
                ("Dưỡng ẩm Bakuchiol + Niacinamide Moisturizer", new List<string>
                {
                    "https://pbs.twimg.com/media/GEFcv4YbQAAP1OE.jpg:large",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ2AJMC8ZAAzSqA09cJezP4jN79hpxNvKS56A&s",
                    "https://www.newbeauty.com/wp-content/uploads/2023/10/2023-Fall-Bakuchiol-And-Niacinamide-Collection-Lifestyle-High-38-1-scaled.jpg"
                }),
                // Acne Advanced Clarifying Hydrator
                ("Dưỡng ẩm Acne Advanced Clarifying Hydrator", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-acne-advanced-clarifying-hydrator-v2-400pix-compressor.jpg?itok=_ELq7sKM",
                    "https://buynaturalskincare.com/cdn/shop/products/Eminence-Organics-Acne-Advanced-Clarifying-Hydrator-Swatch.jpg?v=1665160459&width=1080",
                }),
                // Echinacea Recovery Cream
                ("Kem phục hồi Echinacea Recovery Cream", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-echinacea-recovery-cream-400x400px.png?itok=LNRyYSeA",
                    "https://cdn.cosmostore.org/cache/front/shop/products/127/302585/650x650.jpg",
                }),
                // PowerBright Overnight Cream
                ("Dưỡng da PowerBright Overnight Cream", new List<string>
                {
                    "https://edbeauty.vn/wp-content/uploads/2023/08/Kem-duong-Powerbright-Overnight-Cream-1.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR7Ul9pN39KXGUnc4ho3jvwm2hrKpmBZdKy-g&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS98UOJj0PdVmux2QYM31SSmllUTZkidUc9lA&s"
                }),
                // Skin Soothing Hydrating Lotion
                ("Dưỡng ẩm Skin Soothing Hydrating Lotion", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT1JQViTcVpNPKqP6Wn2e99hYZrczacbtnGtA&s",
                    "https://absoluteskin.com.au/cdn/shop/products/dermalogica-moisturisers-dermalogica-clear-start-skin-soothing-hydrating-lotion-59ml-28824281022558_300x.jpg?v=1711588930",
                    "https://www.dermalogica.ca/cdn/shop/products/3soothinglotionhands-min_2048x2048_9f7cd589-f542-4efb-9228-6d0fb802c954.webp?v=1660681593&width=1946"
                }),
                // Skin Smoothing Cream
                ("Kem làm mịn Skin Smoothing Cream", new List<string>
                {
                    "https://hoaanhdao.vn/media/sanpham/1530205200/KEM_D%C6%AF%E1%BB%A0NG_%E1%BA%A8M_L%C3%80M_M%E1%BB%8AN_DA_DERMALOGICA_SKIN_SMOOTHING_CREAM.jpg",
                    "https://www.facethefuture.co.uk/cdn/shop/files/skin-smoothing-cream-pdp-3.jpg?v=1692712240&width=600",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRj4i7SJV0CSo8uab92n_4olFmOhtRlXD9DJg&s"
                }),
                // Barrier Repair
                ("Kem dưỡng ẩm Barrier Repair", new List<string>
                {
                    "https://dangcapphaidep.vn/image-data/780-780/cdn/2021/11/Dermalogica-Barrier-Repair-1.jpg",
                    "https://veevee.store/wp-content/uploads/2023/10/dermalogica-barrier-repair-2.webp",
                }),
                // Minéral 89 72H Moisture Boosting Fragrance Free Cream 
                ("Kem dưỡng ẩm Minéral 89 72H Moisture Boosting Fragrance Free Cream", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTeDcDHtRmZHif3n-fLkzxuYnQIGuuaVoDVFg&s",
                    "https://media.superdrug.com//medias/sys_master/prd-images/h77/h30/10011961950238/prd-back-826896_600x600/prd-back-826896-600x600.jpg",
                }),
                // Liftactiv B3 Tone Correcting Night Cream With Pure Retinol
                ("Kem dưỡng đêm Liftactiv B3 Tone Correcting Night Cream With Pure Retinol", new List<string>
                {
                    "https://www.vichy.ca/dw/image/v2/AATL_PRD/on/demandware.static/-/Sites-vichy-master-catalog/default/dwe39c018d/product/2024/3337875873086/3337875873086.mainA-EN.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQHvHgPHulcbHZbcvkjI6m1cmQsZFNyiX9Aaw&s",
                }),
                // Belif The True Cream Aqua Bomb
                ("Kem cấp ẩm Belif The True Cream Aqua Bomb", new List<string>
                {
                    "https://image.hsv-tech.io/1987x0/tfs/common/d221fe39-305d-492d-b089-44607a9285fc.webp",
                    "https://image.hsv-tech.io/1920x0/bbx/common/b6851887-fbe5-46a8-b189-e9be2b2d5556.webp",
                }),
                // Oat So Simple Water Cream
                ("Kem dưỡng ẩm Oat So Simple Water Cream", new List<string>
                {
                    "https://kravebeauty.com/cdn/shop/files/9.24_OSS_PDP3.png?v=1727744093&width=1200",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRw5DZUUIS5TX9dU3_h8FD70ChLCeiuZsuTIiniSZZ9udxobZfPTA7A46-KPczF9YiCGr8&usqp=CAU",
                }),
                // AESTURA A-CICA 365 Calming Cream
                ("Kem dưỡng ẩm AESTURA A-CICA 365 Calming Cream", new List<string>
                {
                    "https://down-vn.img.susercontent.com/file/sg-11134207-7rbka-ln1cvnbe1d8532",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ5-Iis6x4JWFAUiCEywTriRNgD9JBhu0P_9g&s",
                }),
                // Sun Soul Face Cream SPF30
                ("Kem chống nắng Sun Soul Face Cream SPF30", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2023/03/8004608515975_1-1091x1200.jpg",
                    "https://shop.beautymanufactur.de/wp-content/uploads/2024/05/com-12163-comfort-zone-sun-soul-face-cream-spf30-texture-900x900.jpg.webp",
                }),
                // Skin Regimen Urban Shield SPF30
                ("Kem chống nắng Skin Regimen Urban Shield SPF30", new List<string>
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
                ("Kem chống nắng Invisible Physical Defense SPF30", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2020/05/7-3-590x600.jpg",
                    "https://m.media-amazon.com/images/I/51HC+QLIrVL._AC_UF350,350_QL80_.jpg",
                }),
                // Protection 50 Sport SPF50
                ("Kem chống nắng Protection 50 Sport SPF50", new List<string>
                {
                    "https://edbeauty.vn/wp-content/uploads/2019/12/kem-chong-nang-Dermalogica-protection-sport-spf-50-156ml.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR9OSPWVkYd5yhmaP6lPYcO1RPlqwK5FYaU-Q&s",
                }),
                // Oil Free Matte SPF30
                ("Kem chống nắng không chứa dầu Oil Free Matte SPF30", new List<string>
                {
                    "https://www.dermalogica.ie/cdn/shop/files/oil-free-matte-pdp_abddc3c6-01ee-4a08-9a22-3d93d66cc79e.jpg?v=1687444268",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQa4gMLYU2BQkxgljz_dQteQ7buLxR-1Q39yQ&s",
                }),
                // Radiant Protection SPF Fluid
                ("Kem chống nắng dạng lỏng SPF", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQSIeOviukKjzkEkUyMUzgkB5Up-8o7XucjLw&s",
                    "https://naturalbeautygroup.com/cdn/shop/files/eminence-organics-Radiant-Protection-SPF-Fluid-Swatch.png?v=1708611969&width=1080",
                }),
                // Lilikoi Daily Defense Moisturizer SPF 40
                ("Kem dưỡng ẩm Lilikoi Daily Defense SPF 40", new List<string>
                {
                    "https://cdn.cosmostore.org/cache/front/shop/products/486/1464606/350x350.jpg",
                    "https://images.squarespace-cdn.com/content/v1/63d45207da7f2a5e82696fe2/1674859081420-WZPSXPO81QEKEFHHKB4Y/eminence-organics-lilikoi-moisturizer-spf40-swatch-circle-400x400.jpg?format=1000w",
                }),
                // Lilikoi Mineral Defense Sport Sunscreen SPF 30
                ("Kem chống nắng thể thao Lilikoi Mineral Defense SPF 30", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-llilikoi-mineral-defense-sport-sunscreen-spf-30-400x400.jpg?itok=0IXRhb-K",
                    "https://images.squarespace-cdn.com/content/v1/5d75abb04593a56ccb9161cb/1572814643401-Q0O24YSFHA4JBI09B5T5/eminence-organics-stone-crop-revitalizing-body-scrub-swatch-toscana.png?format=1000w",
                }),
                // Daily Defense Tinted SPF
                ("Kem chống nắng Daily Defense Tinted SPF", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-daily-defense-tinted-spf-pdp.jpg?itok=DphokG8j",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS8Nl7TW6hlAMdQ9UBiRaTrH0dHNp476Ye3wQ&s",
                    "https://shop.vivadayspa.com/cdn/shop/files/EminenceOrganicDailyDefenseTintedSPF50_3_1800x1800.png?v=1719944530"
                }),
                // Solar Dew Sheer Mineral Melt SPF 30
                ("Kem chống nắng Solar Dew Sheer Mineral Melt SPF 30", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQOz514kGYGfzEyqZ4ItSevNqGTw4KzgJgkrQ&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAV2C0GAlWYxHpgCIWrRSvXA6JFAP0hbeHaA&s",
                    "https://www.purplebeautysupplies.com/cdn/shop/files/hydropeptide-solar-dew-spf-30-mineral-serum-135-oz-40-ml-478701.jpg?v=1715239028&width=1445"
                }),
                // Solar Defense Non-Tinted Sunscreen
                ("Kem chống nắng không màu Solar Defense", new List<string>
                {
                    "https://bizweb.dktcdn.net/thumb/1024x1024/100/318/244/products/untitled-bd9e4f55-c942-4778-8da9-70005304d193.jpg?v=1607671087160",
                    "https://metafields-manager-by-hulkapps.s3-accelerate.amazonaws.com/uploads/hydropeptide-canada.myshopify.com/1726621522-022624_SolarDefenseNonTinted_BENEFITS.jpg",
                }),
                // Solar Defense Tinted SPF 30
                ("Kem chống nắng có màu SPF 30", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_SolarDefenseTinted_PDP_1024x1024.jpg?v=1724437418",
                    "https://hydropeptide.co.uk/cdn/shop/products/HP--PDP-CaroselImages-Solar-Defense-Tinted-video.jpg?v=1662163522",
                }),
                // Vichy Capital Soleil UV Age Daily SPF50 PA++++ 
                ("Kem chống nắng Vichy Capital Soleil UV Age Daily SPF50 PA++++", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSspHWslG4DZpyDyDQNx2owKFNPb1_v0zeDFA&s",
                    "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/suncare/capital-soleil---uv-age/capital-soleil-uv-age-spf50-pack4.jpg?rev=02d0faa4daf344b7ae245ea718addbaa&cx=0.47&cy=0.53&cw=525&ch=596&hash=CFA022DF922328E3F8ABA2264ED86940",
                }),
                // Capital Soleil Ultra Light Face Sunscreen SPF 50
                ("Kem chống nắng Capital Soleil Ultra Light Face SPF 50", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS9WRnfVxm2mABipRz9LHO1eBdH1oDlU6wC7A&s",
                    "https://exclusivebeautyclub.com/cdn/shop/products/vichy-capital-soleil-ultra-light-sunscreen-spf-50-vichy-shop-at-exclusive-beauty-club-515732.jpg?v=1698096439",
                }),
                // Neogen Day-Light Protection Airy Sunscreen
                ("Kem chống nắng Neogen Day-Light Protection Airy", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTk5ssRRdBHGrHUnF87gc2M30_2EJcxarie1g&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTsD2FE2MUfeFeZlFXuvJgidm1iU8WgkkVU7Q&s",
                }),
                // Round Lab Birch Juice Moisturizing Sunscreen
                ("Kem chống nắng dưỡng ẩm Round Lab Birch Juice", new List<string>
                {
                    "https://product.hstatic.net/200000150709/product/1_10a2d9a626da4c03ac81c93497bae020.png",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSiew88Sg_ffo50u6QcX4kuVHoG_fK9u3qtkA&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTKdt5IbBt21U-Ppg4c-ic4PawxrqvJUgAgtg&s"
                }),
                // Beet The Sun SPF 40 PA+++
                ("Kem chống nắng Beet The Sun SPF 40 PA+++", new List<string>
                {
                    "https://www.dodoskin.com/cdn/shop/files/61J0umsAoGL_080fdcb3-37f3-4066-a1b9-8769b6fc1376_2048x2048.jpg?v=1707875322",
                    "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lpp4hgn0mhxw87",
                    "https://kravebeauty.com/cdn/shop/files/9.24_BTS_PDP3.png?v=1729803947&width=1200"
                }),
                // Klairs All-day Airy Mineral Sunscreen SPF50+ PA++++
                ("Kem chống nắng khoáng chất Klairs All-day Airy SPF50+ PA++++", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRxNHcQ2vQJhLia5CAIKeC0YLPPKi_KtyfcIg&s",
                    "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lz7ct301ck9t85",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSfMgba99CrRc61gzwD8wPnD1H7X-QlaB_tFw&s"
                }),
                // Goongbe Waterful Sun Lotion Mild SPF50+ PA++++
                ("Kem chống nắng dịu nhẹ Goongbe Waterful Sun Lotion SPF50+ PA++++", new List<string>
                {
                    "https://gomimall.vn/cdn/shop/files/6_55cd82b9-b3ba-438d-a9e3-d105ff8d9166.png?v=1727068956",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT_VhLOsDvcPtXOWARG0zxac-oDZExusVtWnw&s",
                    "https://www.ballagrio.com/cdn/shop/files/3_f0fb1c1d-ee7a-4762-a261-79a4e841791b_2048x.jpg?v=1734515800"
                }),
                // Eight Greens Phyto Masque – Hot
                ("Mặt nạ Eight Greens Phyto Masque – Hot", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-eight-greens-phyto-masque-hot-pdp.jpg?itok=-pxuW5H5",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTQtndM3VeufQiiNA59YlFA--hDSXxxB9qMow&s",
                    "https://organicskincare.com/wp-content/uploads/2015/08/Eminence-Eight-Greens-Phyto-Masque-360x360.jpg"
                }),
                // Kombucha Microbiome Leave-On Masque
                ("Mặt nạ Kombucha Microbiome Leave-On", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-kombucha-mircobiome-leave-on-masque-pdp.jpg?itok=m8OxvyO3",
                    "https://beautyritual.ca/cdn/shop/products/eminence-organics-kombucha-microbiome-leave-on-masque-swatch-pdp.jpg?v=1722521123&width=480",
                    "https://buynaturalskincare.com/cdn/shop/files/Eminence-Organics-Kombucha-Microbiome-Leave-On-Masque-lifestyle.jpg?v=1717443755&width=1080"
                }),
                // Citrus & Kale Potent C+E Masque
                ("Mặt nạ Citrus & Kale Potent C+E", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/content/blog/product-picks/eminence-organics-citrus-kale-potent-ce-masque.png",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRpPxaCBWNH4MBUIBigH9RkoB_9ExSPGYKnzw&s",
                    "https://i0.wp.com/jessicasapothecary.com/wp-content/uploads/2020/04/vitaminc_masque.jpg?resize=840%2C1050&ssl=1"
                }),
                // Stone Crop Masque
                ("Mặt nạ Stone Crop", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSZoviYz-lQL1doxfPgl9PLZEtC_e6oKEhwnA&s",
                    "https://www.shophalosaltskinspa.com/cdn/shop/products/CAEB78BD-1992-4EEB-B4C3-C4CF73CFE794.jpg?v=1662576743&width=1445",
                    "https://oresta.ca/cdn/shop/products/eminence-organics-eminence-stone-crop-masque-803682.jpg?v=1706991974&width=2000"
                }),
                // Calm Skin Arnica Masque
                ("Mặt nạ Calm Skin Arnica", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTJwWf92eq1uUCwgZTan7qEZlq-TgQfTXFOZA&s",
                    "https://vanislebeautyco.com/cdn/shop/products/image_61ac7e19-35ab-4d54-979d-5ce1d8e44d56.jpg?v=1672155911&width=720",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTImXEm-NSJyUUmbxpk5EcSniUif5Q09WVDFw&s"
                }),
                // Multivitamin Power Recovery Masque
                ("Mặt nạ Multivitamin Power Recovery Masque", new List<string>
                {
                    "https://sieuthilamdep.com/images/detailed/15/mat-na-vitamin-chong-lao-hoa-dermalogica-multivitamin-power-recovery-masque.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRn48kip8eDY41wOLGcEU7hx4NvHyqrpzwPpA&s",
                    "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lowl5l8fp4631a"
                }),
                // Sebum Clearing Masque
                ("Mặt nạ đất sét Sebum Clearing Masque", new List<string>
                {
                    "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/30/images/dermalogica-sebum-clearing-masque-75ml%281%29.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQI_EgDwBMh0LKVO-pyPoV9JSyBhIAbVJLDSg&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRwazAQaO2dRExfe9aDgIYK2dlsywUuOlLByA&s"
                }),
                // Melting Moisture Masque
                ("Mặt nạ cấp ẩm Melting Moisture Masque", new List<string>
                {
                    "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/30/images/dermalogica-melting-moisture-masque.jpg",
                    "https://edbeauty.vn/wp-content/uploads/2023/08/Mat-na-cap-am-Dercalogica-Melting-Moisture-Masque-1.jpg",
                    "https://www.depmoingay.net.vn/wp-content/uploads/2023/08/Mat-na-cap-am-chuyen-sau-Dercalogica-Melting-Moisture-Masque-1.jpg"
                }),
                // Miracle Mask
                ("Mặt nạ dưỡng da Miracle Mask", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_MiracleMask_PDP_1024x1024.jpg?v=1715974173",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTw_gxtCWIU4_0n5EORj1Kl9XmFLdUwXkOVeg&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSwt8mTBHwwlIOOzAMW9avZFEcDHdAb96msRw&s"
                }),
                // Hydro-Lock Sleep Mask
                ("Mặt nạ ngủ Hydro-Lock", new List<string>
                {
                    "https://hydropeptide.com.es/image/cache/catalog/Products/hydro-lock-sleep-mask/012224_New_Retail_Packaging_Hydro-LockSleepMask_PDP_950x_2x.progressive-1000x1000.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTOC_f_73gSfPV_aCpydJNdeAV0aJ18mz-VVg&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQRVLHWy0Ee-uIrQCXBvxrGXgF4b7X69GKfOg&s"
                }),
                // PolyPeptide Collagel+ Mask 
                ("Mặt nạ PolyPeptide Collagel Mask", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/011124_PolypeptideCollagel_Face_PDP_1024x1024.jpg?v=1705108115",
                    "https://m.media-amazon.com/images/I/61kfwzxp9sL._AC_UF1000,1000_QL80_.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRpqGAgS_WZV8gv_OroncyYwbcWdujtINeEHg&s"
                }),
                // Balancing Mask
                ("Mặt nạ giảm mụn Balancing Mask", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQfYIUlZvjKdCtWBk5N0Hjzr5rWg0f7c8NBdw&s",
                    "https://hydropeptide.asia/wp-content/uploads/2021/02/HP-PDP-BalancingMask-Hero-2.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTCNUS7D4oiAqWaQbCt8I4Ao1amYgwd9ojTGQ&s"
                }),
                // Rejuvenating Mask
                ("Mặt nạ Rejuvenating Mask", new List<string>
                {
                    "https://cdn.shopify.com/s/files/1/0345/0444/1995/files/PDP-RejuvenatingMask-Results.jpg?v=1661906989",
                    "https://hydropeptide.co.uk/cdn/shop/products/rejuvenating-mask_full-size.jpg?v=1662076519",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS5xMKeR40mJv14WObkfKV-EpCMvfT5jlPDlA&s"
                }),
                // Aqualia Thermal Night Spa
                ("Mặt nạ ngủ dưỡng ẩm Aqualia Thermal Night Spa", new List<string>
                {
                    "https://product.hstatic.net/200000124701/product/00014081_vichy_mat_na_khoang_75ml_m9104500_6680_5c9e_large_88ade14c39_01d00c3a004a464b8b0e09f6bbc6ffb1_master.jpg",
                    "https://www.vichy.com.vn/-/media/project/loreal/brand-sites/vchy/apac/vn-vichy/products/product-packshots---1/aqualia/vichy_aqualia_thermal_creme_nuit.png?rev=296ada59cb5e41109fb3b0d565c68596&cx=0.51&cy=0.54&cw=525&ch=596&hash=2D0C1F95950702CE1CC90F8691F193F5",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQK37Y_CeTdJPn4VyPC0fCvjfmImZwSQrOhrQdTNwu4hPPB7NwrdedizcNK0FDZEplJQTg&usqp=CAU"
                }),
                // Quenching Mineral Mask
                ("Mặt nạ khoáng Quenching Mineral Mask", new List<string>
                {
                    "https://product.hstatic.net/200000124701/product/00014081_vichy_mat_na_khoang_75ml_m9104500_6680_5c9e_large_88ade14c39_01d00c3a004a464b8b0e09f6bbc6ffb1_master.jpg",
                    "https://cf.shopee.vn/file/1674db822b2bc08d254590acabf64547",
                }),
                // Pore Purifying Clay Mask
                ("Mặt nạ đất sét Pore Purifying Clay Mask", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTmtvhbhIddJEIZPbAz6q8EFueP1vtX4HhqQw&s",
                    "https://down-vn.img.susercontent.com/file/244ad88d90d841ab5af1681edc2258a3",
                }),
                // Sulwhasoo Activating Mask
                ("Mặt nạ chăm sóc và kích hoạt da Sulwhasoo Activating Mask", new List<string>
                {
                    "https://kallos.co/cdn/shop/files/12_33f1be98-3f05-421b-98cb-67be17d1e90b.jpg?v=1693728111&width=900",
                    "https://assets.aemi.vn/images/2024/5/1715574861200-0",
                    "https://product.hstatic.net/200000714339/product/z5219389220375_0b7217f34a936c85ae10f994e6bdda40_2c716e8ce65146fc938d37df2bbd805d.jpg"
                }),
                // COSRX Ultimate Nourishing Rice Spa Overnight Mask
                ("Mặt nạ ngủ dưỡng da gạo COSRX Ultimate Nourishing Rice Spa", new List<string>
                {
                    "https://image.hsv-tech.io/1920x0/bbx/products/0e8fbb74-e136-4ea3-9ef0-76f6d44bca3b.webp",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT8ESM2p1P_SgJXVC6VPdOIambFBoukzgAGdw&s",
                }),
                // Klairs Rich Moist Soothing Tencel Sheet Mask
                ("Mặt nạ giấy Tencel làm dịu da Rich Moist Soothing của Klairs", new List<string>
                {
                    "https://product.hstatic.net/1000006063/product/klairs_rich_moist_soothing_tencel_sheet_mask_94678ee3e2134354b5a039b569cff87e_1024x1024.jpg",
                    "https://product.hstatic.net/200000551679/product/dear__klairs_mat_na_giay_rich_moist___2__db38203e18c541a19031c3212db864d4_1024x1024.jpg",
                    "https://product.hstatic.net/200000714339/product/klairs-rich-moist-soothing-sheet_bee5a326d74642bab0e5862e663e09b8_1024x1024.jpg"
                }),
                // Klairs Midnight Blue Calming Sheet Mask
                ("Mặt nạ làm dịu da Blue Calming Sheet Mask", new List<string>
                {
                    "https://image.hsv-tech.io/1987x0/bbx/common/38079d73-de48-4721-88af-c0e72b28a471.webp",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS4SsU7xocAIl8x0h4bj_lrLO0felkJMgASmw&s",
                }),
                // Dear, Klairs Freshly Juiced Vitamin E Mask
                ("Mặt nạ ngủ dưỡng da Klairs", new List<string>
                {
                    "https://www.guardian.com.vn/media/catalog/product/cache/30b2b44eba57cd45fd3ef9287600968e/3/0/3020993_iifvm7d8r9crinym.jpg",
                    "https://image.hsv-tech.io/1920x0/bbx/common/12aecda8-804a-47d0-9205-b881a6ce3174.webp",
                }),
                // Sacred Nature Exfoliant Mask
                ("Mặt nạ tẩy tế bào chết Sacred Nature", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/0d080a4d83fbeff70dd59e4585d73e1d76f7b92d_2000x-1091x1200.jpg",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/e8a1b4fb805fbdc0b8d1f659b595477ce4d7ff79_2000x.jpg",
                    "https://eideal.com/cdn/shop/files/Exfoliant-mask-Texture-1.jpg?v=1703580157"
                }),
                // Essential Scrub
                ("Kem tẩy tế bào chết Essential Scrub", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2023/11/San-pham-112.jpg",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/bf8201b7fbba797ceca178721986f5de589c9eed_2000x-698x768.jpg",
                }),
                // Liquid Resurfacing Solution
                ("Tẩy tế bào chết Liquid Resurfacing Solution", new List<string>
                {
                    "https://hydropeptide.com/cdn/shop/files/012224_New_Retail_Packaging_LiquidResufacingSolution_PDP_1024x1024.jpg?v=1718661002",
                    "https://hydropeptide.ca/cdn/shop/files/022024_LiquidResurfacingSolution_PDP.jpg?v=1712791423",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSuv41x-1M5vuU3jX5Ggnd6NfFwrKNUe_D56Q&s"
                }),
                // 5X Power Peel Face Exfoliator
                ("Tẩy tế bào chết cho mặt 5X Power Peel", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS6Xgx3XAAFH8I5qf0piOxYUBFpx_wro35G6Q&s",
                    "https://hydropeptide.ca/cdn/shop/files/012224_Old_Retail_Packaging_5xPowerPeel_Sachets_PDP.jpg?v=1712790410",
                }),
                // Daily Milkfoliant
                ("Tẩy tế bào chết Daily Milkfoliant", new List<string>
                {
                    "https://edbeauty.vn/wp-content/uploads/2023/08/Tay-te-bao-chet-Dermalogica-Daily-Milkfoliant-2.jpg",
                    "https://veevee.store/wp-content/uploads/2023/10/dermalogica-daily-milkfoliant-exfoliator-2.webp",
                }),
                // Liquid Peelfoliant
                ("Peel tái tạo da Liquid Peelfoliant", new List<string>
                {
                    "https://healthygoods.com.vn/resource/images/2023/12/dermalogica-liquid-peelfoliant-59ml1.jpg",
                    "https://www.spacenk.com/on/demandware.static/-/Library-Sites-spacenk-global/default/dwb77b7327/dermalogica-liquid-peel-review-space-nk.jpg",
                }),
                // Daily Superfoliant
                ("Bột tẩy da chết than hoạt tính Daily Superfoliant", new List<string>
                {
                    "https://dermalogica-vietnam.com/wp-content/uploads/2024/03/T%E1%BA%A9y-T%E1%BA%BF-B%C3%A0o-Ch%E1%BA%BFt-Than-Ho%E1%BA%A1t-T%C3%ADnh-Daily-Superfoliant.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWwLsIyzmrB8hOus3oKNIoufCNavvosd0Zog&s",
                }),
                // Multivitamin Thermafoliant
                ("Kem tẩy tế bào chết Multivitamin Thermafoliant", new List<string>
                {
                    "https://dangcapphaidep.vn/image-data/780-780/upload/2023/08/30/images/dermalogica-multivitamin-thermafoliant-1.jpg",
                    "https://veevee.store/wp-content/uploads/2023/10/dermalogica-multivitamin-thermafoliant-1.webp",
                }),
                // Daily Microfoliant
                ("Bột tẩy da chết Dermalogica Daily Microfoliant", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS6Xgx3XAAFH8I5qf0piOxYUBFpx_wro35G6Q&s",
                    "https://thesecretdayspa.co.uk/cdn/shop/products/US-PDP-How-To-Product-In-Hand---Daily-Microfoliant_1800x1800.webp?v=1678979333",
                }),
                // Strawberry Rhubarb Dermafoliant
                ("Dâu tây đại hoàng Dermafoliant", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-strawberry-rhubarb-dermafoliant.jpg?itok=lP2gq52k",
                    "https://anjouspa.com/wp-content/uploads/2023/01/anjou-spa-fresh-fruit-facial-27.jpg",
                }),
                // Turmeric Energizing Treatment
                ("Liệu pháp tăng cường năng lượng bằng nghệ", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTbvQKm1rsUl0wMFrKASBA5iBt0VX69eQIyjw&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTb2Zl2efrdTibFj1FlzpWE9n4qQaT7a-fe4w&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1TWpyNYBuXgZHao3xTL4Y4KWq-VYK_yFLNw&s"
                }),
                // Bright Skin Licorice Root Exfoliating Peel
                ("Lột da tẩy tế bào chết Bright Skin Licorice Root", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTL9Oq6DD-KmiZGSPvBt6viuJI3wf059HorRQ&s",
                    "https://emstore.com/cdn/shop/files/licorice-root-exfoliating-peel-pads.jpg?v=1695202885",
                }),
                // Calm Skin Chamomile Exfoliating Peel
                ("Tẩy tế bào chết hoa cúc Calm Skin", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRFf6rWs4EsBBhwJBa458IvTlMEnUf1ZZqiXQ&s",
                    "https://static.thcdn.com/productimg/960/960/11370437-1344871983881681.jpg",
                }),
                // Radish Seed Refining Peel
                ("Peel da vỏ hạt củ cải", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-radish-seed-refining-peel-400x400px.png?itok=G7HyJYY5",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS7PLc_tx0I_lQDDPwITKuaqEAVUN7u7IsuMw&s",
                }),
                // Anua Heartleaf 77% Clear Pad
                ("Tấm lót trong suốt Anua Heartleaf 77%", new List<string>
                {
                    "https://bizweb.dktcdn.net/100/525/087/products/56aa5b2e-5258-4122-beee-d53128f44c3c.jpg?v=1733139782937",
                    "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lrqzijemkds443",
                    "https://cdn.shopify.com/s/files/1/0560/7328/9826/files/IMG-4057.jpg?v=1714980820"
                }),
                // No.5 Vitamin-Niacinamide Concentrated Pad
                ("Miếng bông cô đặc Vitamin-Niacinamide số 5", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTkRt0Da2RE6ez4IpyF-9eVbo1rgj_6wA-4LQ&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQqjR310AXBDPsifkWK0rsXBxhDnTBDCW50Yg&s",
                    "https://www.mikaela-beauty.com/cdn/shop/files/AX6H2399w_1200x1200.jpg?v=1711843023"
                }),
                // Balanceful Cica Toner Pad
                ("Miếng lót cân bằng Cica", new List<string>
                {
                    "https://product.hstatic.net/1000006063/product/bt_91f25bc0d4854817a2c94eff8a459e9e_1024x1024.jpg",
                    "https://product.hstatic.net/1000328823/product/_20__19__9352572e4c5245a4a9756cb089a6d047_master.png",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTxFuy4bq9_k4ZsMOIcLNhKFheC0oJnhQ7gJQ&s"
                }),
                // Pine Needle Pore Pad Clear Touch
                ("Miếng bông tẩy da chết, làm sạch lỗ chân lông Pine Needle Pore Pad Clear Touch", new List<string>
                {
                    "https://down-vn.img.susercontent.com/file/7876b204a9fcf65d0930190e274bb254",
                    "https://cdn.zochil.shop/bbafea58-b901-4841-9319-281f9b7c4be3_t1500.jpg",
                }),
                // Krave Kale-lalu-yAHA
                ("Tẩy da chết Krave Kale-lalu-yAHA", new List<string>
                {
                    "https://kravebeauty.com/cdn/shop/files/9.24_KLY_PDP1.png?v=1727740531",
                    "https://i.ebayimg.com/images/g/M18AAOSw9BpkHShE/s-l400.jpg",
                    "https://lilabeauty.com.au/cdn/shop/products/buy-krave-beauty-kale-lalu-yaha-200ml-at-lila-beauty-korean-and-japanese-beauty-skin-care-255400.jpg?v=1648860498"
                }),
                // Yuzu Solid Body Oil
                ("Dầu dưỡng thể Yuzu Solid Body", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTa6cZWEjkws8RPCsTXRPcfNM7GlmkJ86fuXg&s",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT091jFcMQELHhS2zqu8kDBc39zoRi4Sck4LQ&s",
                }),
                // Mangosteen Body Lotion
                ("Sữa dưỡng thể Mangosteen", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/eminence-organics-mangosteen-body-lotion-400x400px-compressed.png?itok=Ofu_3Y-c",
                    "https://cdn11.bigcommerce.com/s-wcc90u14r8/images/stencil/1280x1280/products/9804/24894/skin-beauty-eminence-mangosteen-body-lotion-8.4__31975.1729024904.jpg?c=2",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSMES-hL9lyHa3KeoivJfChbejtJvNOfXyflQ&s"
                }),
                // Coconut Sugar Scrub
                ("Tẩy tế bào chết bằng đường dừa", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTFrsNXy29hCQjfyrcStKLnNID2zv_9QuKvvQ&s",
                    "https://i5.walmartimages.com/seo/Eminence-Stone-Crop-Revitalizing-Body-Scrub-8-4-oz_f7168ae2-fd69-45f7-9c04-443ddcc24516.961abd73cdb34020becd439ede8416a6.jpeg",
                }),
                // Stone Crop Contouring Body Cream
                ("Kem Dưỡng Thể Stone Crop Contouring", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/stone-crop-body-contouring-cream-400x400.png?itok=gTrvgdf1",
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-slide/eminence-organics-stone-crop-body-contouring-cream-swatch-400x400.jpg?itok=CW8Z9DlV",
                }),
                // Stone Crop Body Oil
                ("Dầu dưỡng thể Stone Crop", new List<string>
                {
                    "https://eminenceorganics.com/sites/default/files/styles/product_medium/public/product-image/stone-crop-body-oil-400x400.png?itok=kCbSF1gV",
                    "https://i5.walmartimages.com/asr/3c096190-35c1-443b-876c-65b80976c79b.d68538517adb4174192b340277828d4e.jpeg?odnHeight=768&odnWidth=768&odnBg=FFFFFF",
                }),
                // Sacred Nature Body Butter
                ("Bơ dưỡng thể Sacred Nature", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/9720a1e9b759cb104a72600261dc4320a1e31fe4_2000x.jpg",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/181a900228143e96fd981d63502195294ef76dfe_2000x-1091x1200.jpg",
                    "https://vonpreen.com/wp-content/uploads/2023/03/Comfort-Zone-Sacred-Nature-Body-Butter.jpg"
                }),
                // Tranquillity Oil
                ("Tinh dầu Tranquility", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSz0kljZlGJ8sUGfVH9Q5GXUrVZd51g4A83yQ&s",
                    "https://down-vn.img.susercontent.com/file/sg-11134201-22110-4gj57hclpnjv4b",
                }),
                // Body Strategist Peel Scrub
                ("Tẩy tế bào chết Body Strategist", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/4a34a3dd50e62994703db0d5db6c98c3ac321337_2000x.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRNq-zuTktcugPue1xZh1lJ_BagEEnHf-x0mQ&s",
                    "https://i.makeup.ae/600x600/qgmotujz0cvp.jpg"
                }),
                // Body Strategist Oil
                ("Dầu Body Strategist", new List<string>
                {
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/61200985bf4310406e0a68b32f26e54ffad19bf5_2000x.jpg",
                    "https://oadep.com/wp-content/uploads/2023/03/duong-the-Comfort-Zone-Body-Strategist-Oi-100ml-chinh-hang.jpg",
                    "https://comfortzone.com.vn/wp-content/uploads/2022/10/c0c3698e5819ca297e7b8be9f087c36a4a31cef4_2000x-1091x1200.jpg"
                }),
                // Body Strategist Contour Cream
                ("Kem dưỡng thể Body Strategist Contour Cream", new List<string>
                {
                    "https://world.comfortzoneskin.com/cdn/shop/files/myqk9504x4uzbbh0gz8u_5000x.jpg?v=1718128996",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRQ1zUHTv9Xso7yRlufR12dR9ahzcB1Po3M0LBxOdrtbIOrBmPDpaq-lVlUdo-Dkey4qsQ&usqp=CAU",
                }),
                // Body Hydrating Cream
                ("Kem dưỡng ẩm toàn thân", new List<string>
                {
                    "https://dangcapphaidep.vn/image-data/780-780/cdn/2018/07/Dermalogica-Body-Hydrating-Cream-1.jpg",
                    "https://dermalogica-vietnam.com/wp-content/uploads/2021/06/2.1.jpg",
                    "https://bluemercury.com/cdn/shop/files/global_images-666151111103-2.jpg?v=1725057930&width=1500"
                }),
                // Phyto Replenish Body Oil
                ("Dầu dưỡng thể Phyto Replenish", new List<string>
                {
                    "https://dr-skincare.com/wp-content/uploads/2024/03/64.png",
                    "https://product.hstatic.net/1000160964/product/phyto-replenish-body-oil-with-ingredients-400_2x_2b58ad161eca4e67851e178c2b38bfd4_master.jpg",
                    "https://dermalogica-vietnam.com/wp-content/uploads/2020/06/8.8.jpg"
                }),
                // Conditioning Body Wash
                ("Sữa tắm dưỡng ẩm", new List<string>
                {
                    "https://hoaoaihuong.vn/upload/data/images/product/1587433351_ConditioningBodyWashwithRosemaryIllustration_1_700x800.jpg",
                    "https://www.depmoingay.net.vn/wp-content/uploads/2023/08/Sua-tam-min-da-Dermalogica-Conditioning-Hand-Body-Wash-1.jpg",
                    "https://www.dermalogica.ca/cdn/shop/products/conditioning-body-wash_84-01c_590x617_bf506ab2-c69d-47c3-8b74-6594dedaea1b.jpg?v=1600289356&width=1445"
                }),
                // Nourishing Glow Body Oil
                ("Dầu dưỡng thể Nourishing Glow", new List<string>
                {
                    "https://hydropeptide.co.uk/cdn/shop/products/nourishing-glow_full-size.jpg?v=1660861721",
                    "https://cdn.shopify.com/s/files/1/0345/0444/1995/files/PDP-NourishingGlow-HowToUse.jpg?v=1660794950",
                    "https://i.makeup.ae/m/mu/muboadhvbzcg.jpg"
                }),
                // Firming Body Moisturizer
                ("Kem dưỡng ẩm săn chắc cơ thể", new List<string>
                {
                    "https://hydropeptide.co.uk/cdn/shop/products/firming-moisturizer_full-size.jpg?v=1681516762",
                    "https://www.fruitionskintherapy.ca/wp-content/uploads/hydropeptide-body-firming-body-moisturizer-2.jpg",
                }),
                // Illiyoon Ceramide Ato Concentrate Cream
                ("Kem dưỡng ẩm Illiyoon Ceramide Ato Concentrate", new List<string>
                {
                    "https://seoulplus.com/wp-content/uploads/2023/07/Illiyoon-Ceramide-Ato-Concentrate-Cream-1.jpg",
                    "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-llzx6x3exbdrb8",
                }),
                // Dear Doer The Hidden Body Scrub & Wash
                ("Dear Doer Tẩy tế bào chết và rửa mặt ẩn", new List<string>
                {
                    "https://m.media-amazon.com/images/I/71f+T8Po-bL.jpg",
                    "https://m.media-amazon.com/images/I/71qOFL2EeUL._AC_UF350,350_QL80_.jpg",
                    "https://www.ballagrio.com/cdn/shop/files/2_ff23dccd-b3b9-471c-b9af-0e44d9101f2a_2048x.jpg?v=1689571364"
                }),
                // Aestura Atobarrier 365 Ceramide Lotion
                ("Sữa dưỡng ẩm Aestura Atobarrier 365 Ceramide", new List<string>
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
                ("Dầu dưỡng ẩm hàng ngày Derma B", new List<string>
                {
                    "https://sokoglam.com/cdn/shop/files/Soko-Glam-PDP-Derma-B-Daily-Moisture-Body-Oil-02_860x.png?v=1687301614",
                    "https://down-vn.img.susercontent.com/file/8b3b623e8eeeddd133584f44dd7a77e1",
                    "https://koreamarket.ru/wa-data/public/shop/products/04/90/19004/images/10900/10900.970.jpg"
                }),
                // Davines Energizing Shampoo
                ("Dầu gội Davines Energizing", new List<string>
                {
                    "https://davinesvietnam.com/wp-content/uploads/2019/07/dau-goi-Davines-Energizing-Shampoo-1000ml-1.jpg",
                    "https://www.planetbeauty.com/cdn/shop/files/Energize_shp_bk_x2000.jpg?v=1684540681",
                }),
                // Davines Volu Shampoo
                ("Dầu gội Davines Volu", new List<string>
                {
                    "https://vn.davines.com/cdn/shop/files/rtqshfyvzhwofcdaxnm9_2000x_bf029392-1ae3-48bf-99f6-dab3cb178723.jpg?v=1721123829",
                    "https://i.ebayimg.com/images/g/1gwAAOSwfi5kQG9-/s-l1200.jpg",
                }),
                // Davines Calming Shampoo
                ("Dầu gội làm dịu Davines", new List<string>
                {
                    "https://vn.davines.com/cdn/shop/products/71262_NATURALTECH_CALMING_Calming_Shampoo_250ml_Davines_2000x.jpg?v=1721118741",
                    "https://bizweb.dktcdn.net/100/141/195/products/1363f1c28cd32f8d76c2.jpg?v=1718777142610",
                }),
                // Davines Dede Shampoo
                ("Dầu gội Davines Dede", new List<string>
                {
                    "https://vn.davines.com/cdn/shop/products/75019_ESSENTIAL_HAIRCARE_DEDE_Shampoo_250ml_Davines_2000x.jpg?v=1721121499",
                    "https://www.planetbeauty.com/cdn/shop/products/Davines_Dede_Shampoo_back_x2000.jpg?v=1683765042",
                }),
                // Davines Melu Shampoo
                ("Dầu gội Davines Melu", new List<string>
                {
                    "https://vn.davines.com/cdn/shop/products/75097_ESSENTIAL_HAIRCARE_MELU_Shampoo_250ml_Davines_600x.jpg?v=1721121211",
                    "https://down-vn.img.susercontent.com/file/sg-11134201-7rd4y-lxae2wn24xn0cb",
                }),
                // Bain Décalcifiant Réparateur Repairing Shampoo
                ("Dầu gội phục hồi Bain Décalcifiant Réparateur", new List<string>
                {
                    "https://www.lmching.com/cdn/shop/files/ProductSize_1_2_-2024-08-09T092055.601_800x.jpg?v=1723170064",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRImTYO-AKUfNQ9nQUAVCCmGJqOD7DmC3NTuw&s",
                }),
                // Bain Densité Shampoo
                ("Dầu gội Bain Densité", new List<string>
                {
                    "https://www.lmching.com/cdn/shop/files/ProductSize_1_2_-2024-08-09T092055.601_800x.jpg?v=1723170064",
                    "https://beautytribe.com/cdn/shop/files/kerastase-densifique-bain-densite-250ml-105115-467836.jpg?v=1732792066&width=1500",
                }),
                // Bain Hydra-Fortifiant Shampoo
                ("Dầu gội Bain Hydra-Fortifiant", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWOrdBrhw9Spr0jyNZMHmGhMMRTWI5QkzBJw&s",
                    "https://supernovasalon.com/cdn/shop/products/bain-hydra-fortifiant-lifestyle_900x.webp?v=1647475834",
                    "https://shopurbannirvana.com/cdn/shop/files/GENESIS-BAIN-HYDRA-500ML-2.jpg?v=1730219325&width=1445"
                }),
                // L'Oreal Paris Elseve Total Repair 5 Repairing
                ("Dầu gội phục hồi tóc hư tổn L'Oreal Paris Elseve Total Repair 5", new List<string>
                {
                    "https://product.hstatic.net/1000006063/product/l_oreal_elseve_total_repair_5_shampoo_650ml_cf374f7742d44e639abc23d1b63e3fcb_1024x1024.jpg",
                    "https://image.hsv-tech.io/1987x0/bbx/l_oreal_paris_elseve_total_repair_5_shampoo_330ml_d9cfd29c63b142db8bbf4201153ca0b7.png",
                }),
                // L'Oreal Professional Hair Spa Deep Nourishing
                ("Dầu gội dưỡng sâu L'Oreal Professional Hair Spa", new List<string>
                {
                    "https://media.hcdn.vn/catalog/product/g/o/google-shopping-dau-goi-l-oreal-professionnel-cap-am-cho-toc-kho-600ml_img_680x680_d30c8d_fit_center.jpg",
                    "https://down-vn.img.susercontent.com/file/sg-11134201-7rd75-lty2ehqkzei8ac",
                }),
                // L'Oreal Paris Elseve Fall Resist 3X Anti-Hairfall
                ("Dầu gội chống rụng tóc L'Oreal Paris Elseve Fall Resist 3X", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSNOiI0F3AwhpbFi1s-H6iAqE0Hy1y6Dv-L-w&s",
                    "https://m.media-amazon.com/images/I/612rxvwYs1L._AC_UF1000,1000_QL80_.jpg",
                }),
                // Nº.4 Bond Maintenance Shampoo
                ("Dầu gội bảo dưỡng liên kết số 4", new List<string>
                {
                    "https://image.hsv-tech.io/1987x0/bbx/common/3bb29575-9f8e-48eb-a709-f019f95cc65f.webp",
                    "https://image.hsv-tech.io/1987x0/bbx/common/1d335186-bd89-4e62-ab87-72d9bd03557a.webp",
                    "https://product.hstatic.net/1000172157/product/olaplex_cap_no4_5_250ml_04_b501890da8f440d6a4cf66d2069d1524_large.jpg"
                }),
                // Gold Lust Repair & Restore Shampoo
                ("Dầu gội phục hồi và phục hồi Gold Lust", new List<string>
                {
                    "https://www.sephora.com/productimages/sku/s2438166-main-zoom.jpg?imwidth=315",
                    "https://www.oribe.com/cdn/shop/files/1200Wx1200H-400172_1b27cf32-3e50-43b4-954b-1790ae536f2e.jpg?v=1698874898",
                    "https://images-na.ssl-images-amazon.com/images/I/71kp9RhJD3L.jpg"
                }),
                // Supershine Hydrating Shampoo
                ("Dầu gội dưỡng ẩm Supershine", new List<string>
                {
                    "https://www.oribe.com/cdn/shop/files/402453-0_b7d201cd-c3f9-4b48-84c5-f9e3b03a522a.jpg?v=1725462201",
                    "https://images.finncdn.no/dynamic/1280w/2024/12/vertical-0/13/8/384/925/188_a9edb849-aba8-4933-acad-827f251c0c4e.jpg",
                }),
                // Acidic Bonding Concentrate sulfate-free Shampoo
                ("Dầu gội không chứa sulfate có tính axit liên kết cô đặc", new List<string>
                {
                    "https://images-na.ssl-images-amazon.com/images/I/61T+3kBuHoL.jpg",
                    "https://images-na.ssl-images-amazon.com/images/I/71J1-1eoRbL.jpg",
                }),
                // Redken All Soft Shampoo
                ("Dầu gội Redken All Soft", new List<string>
                {
                    "https://images-na.ssl-images-amazon.com/images/I/71xQ59SWkML.jpg",
                }),
                // Izumi Tonic Strengthening Shampoo
                ("Dầu gội tăng cường Izumi Tonic", new List<string>
                {
                    "https://static.thcdn.com/productimg/original/14204021-6465063343389571.jpg",
                    "https://static.thcdn.com/productimg/original/14204021-9055063343445593.jpg",
                }),
                // Ultimate Reset Extreme Repair Shampoo
                ("Dầu gội phục hồi Ultimate Reset Extreme Repair", new List<string>
                {
                    "https://cdn.haarshop.ch/catalog/product/thumbnail/51ac8a89d88d4eed1e1f5a7566ce210ab624b84b71e44e4fa3b44063/image/0/570x570/111/99/7/0/70941222e739145ec75a7a705ff8f17b6f55fc15_3474636610181_bi_shu_uemura_ultimate_reset_shampoo.jpg",
                    "https://m.media-amazon.com/images/I/81j7Ha+ZN+L._AC_UF1000,1000_QL80_.jpg",
                }),
                // Fusion Shampoo
                ("Dầu gội Fusion", new List<string>
                {
                    "https://www.wella.com/professional/m/Supernova/Fusion/PDP/Fusion-shampoo-supernova-slider-packshot-1_d.jpg",
                    "https://www.wella.com/professional/m/Supernova/Fusion/PDP/Fusion-shampoo-supernova-slider-packshot-6_d.jpg",
                }),
                // Ultimate Repair Shampoo
                ("Dầu gội phục hồi tối ưu", new List<string>
                {
                    "https://www.wella.com/professional/m/care/Fire/Product_Packshots/Ultimate-Repair-Shampoo-slider-packshot-v2_d.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTXUItcEx4vQtgGnn4SBGez8xi8-68r49hSaQ&s",
                    "https://cdn.awsli.com.br/613/613406/produto/2389103400b84107f38.jpg"
                }),
                // Davines Dede Conditioner 
                ("Dầu xả Davines Dede", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWFr_SKchvHJVFDWloohAIqYFse_GceySgQw&s",
                    "https://vn.davines.com/cdn/shop/products/PROSIZEWEBSITE-11_2084x.jpg?v=1721121409",
                }),
                // Davines Love Smoothing Conditioner
                ("Dầu xả Davines Love Smoothing", new List<string>
                {
                    "https://davinesvietnam.com/wp-content/uploads/2019/07/dau-xa-Davines-Love-Smoothing-Conditioner-250ml-chinh-hang-gia-re.jpg",
                    "https://lizi.vn/wp-content/uploads/2020/02/dau-goi-davines-love-smoothing-3.jpeg",
                }),
                // Davines Melu Conditioner
                ("Dầu xả Davines Melu", new List<string>
                {
                    "https://vn.davines.com/cdn/shop/products/75608_ESSENTIAL_HAIRCARE_MELU_Conditioner_250ml_Davines_2000x_4a9aa7af-bc07-4713-bd1f-4b8c9c5b4d53_2000x.jpg?v=1721121159",
                    "https://aslaboratory.com.ua/image/cache/catalog/import/001818-crop-600x750-product_thumb.jpg",
                }),
                // Davines Momo Conditioner 
                ("Dầu xả Davines Momo", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSlTOtpGa0eZH_hOfmHfed3ivvAl-PaNgDPpg&s",
                    "https://conceptcshop.com/cdn/shop/files/davines-revitalisant-hydratant-momo-1000-ml-130430.jpg?v=1728407522&width=2048",
                }),
                // Fondant Renforçateur Conditioner
                ("Dầu xả Fondant Renforçateur", new List<string>
                {
                    "https://supernovasalon.com/cdn/shop/products/fondant-renforcateur-lifestyle_800x.webp?v=1647476539",
                    "https://millies.ie/cdn/shop/files/KerastaseGenesisFondantRenforcateur_3.jpg?v=1705681358&width=3024",
                }),
                // Fondant Densité Conditioner
                ("Dầu xả Fondant Densité", new List<string>
                {
                    "https://emmakcreativestyling.ie/wp-content/uploads/2023/04/Kerastase-Densifique-Fondant-Densite_Gallery-Image-2.webp",
                    "https://img-cdn.heureka.group/v1/6cdbcbe4-8b14-535b-ad4c-34848f5dc065.jpg",
                }),
                // Fondant Fluidealiste Conditioner
                ("Dầu xả Fondant Fluidealiste", new List<string>
                {
                    "https://cdn11.bigcommerce.com/s-f7ta3/images/stencil/1280x1280/products/3856/8706/kerastase-discipline-fondant-fluidealiste-conditioner-200ml__51586.1657021232.jpg?c=2",
                    "https://static.thcdn.com/productimg/960/960/10951828-6334927764118014.jpg",
                }),
                // Ciment Anti-Usure Conditioner
                ("Dầu xả chống rụng tóc Ciment", new List<string>
                {
                    "https://i0.wp.com/salonvenere.com/wp-content/uploads/2023/01/3474636397884.Main_.jpg?fit=930%2C930&ssl=1",
                    "https://www.kerastase.co.uk/dw/image/v2/AAQP_PRD/on/demandware.static/-/Sites-ker-master-catalog/en_GB/dwda75a874/product/resistance/3474636397884.pt01.jpg?sw=340&sh=340&sm=cut&sfrm=jpg&q=80",
                }),
                // Redken All Soft Conditioner
                ("Dầu xả Redken All Soft", new List<string>
                {
                    "https://www.redken.com/dw/image/v2/AAFM_PRD/on/demandware.static/-/Sites-ppd-redken-master-catalog/default/dwf2db2623/images/pdp/all-soft-conditioner/redken-all-soft-conditioner-for-dry-hair.png",
                    "https://images-na.ssl-images-amazon.com/images/I/617nmDgkTfL.jpg",
                }),
                // Redken Frizz Dismiss Conditioner
                ("Dầu xả Redken Frizz Dismiss", new List<string>
                {
                    "https://www.ubuy.vn/productimg/?image=aHR0cHM6Ly9tLm1lZGlhLWFtYXpvbi5jb20vaW1hZ2VzL0kvNjFSSTQ4VTMtV0wuX1NMMTUwMF8uanBn.jpg",
                    "https://m.media-amazon.com/images/I/61ERLLB3uSL.jpg",
                    "https://www.ozhairandbeauty.com/_next/image?url=https%3A%2F%2Fcdn.shopify.com%2Fs%2Ffiles%2F1%2F1588%2F9573%2Ffiles%2FRedken-Frizz-Dismiss-Sodium-Chloride-Free-Conditioner_7.jpg%3Fv%3D1714700684&w=3840&q=75"
                }),
                // L'Oréal Paris Elvive Total Repair 5 Conditioner
                ("Dầu xả L'Oréal Paris Elvive Total Repair 5", new List<string>
                {
                    "https://i5.walmartimages.com/seo/L-Oreal-Paris-Elvive-Total-Repair-5-Repairing-Conditioner-For-Damaged-Hair-With-Protein-And-Ceramide-Strong-Silky-Shiny-Healthy-Renewed-28-Fl-Oz_cec04c0d-34eb-4fa9-9de8-3001f4a60d25.a403fb04c77a2dfe68657dcebd78d065.jpeg",
                    "https://images-na.ssl-images-amazon.com/images/I/81VWk+QHgyL.jpg",
                }),
                // L'Oréal Paris EverPure Moisture Conditioner
                ("Dầu xả dưỡng ẩm L'Oréal Paris EverPure", new List<string>
                {
                    "https://m.media-amazon.com/images/I/61wefZxO9gL.jpg",
                }),
                // L'Oréal Paris EverCurl Hydracharge Conditioner
                ("Dầu xả dưỡng ẩm L'Oréal Paris EverCurl", new List<string>
                {
                    "https://m.media-amazon.com/images/I/71I7nEWa6DL.jpg",
                }),
                // Invigo Nutri-Enrich Conditioner
                ("Dầu xả Invigo Nutri-Enrich", new List<string>
                {
                    "https://mcgrocer.com/cdn/shop/files/aeb74ae52abdfdf7746a2a4a84d407ae.jpg?v=1710962092",
                    "https://www.capitalhairandbeauty.ie/Images/Product/Default/xlarge/867618.jpg",
                    "https://www.lookfantastic.com/images?url=https://static.thcdn.com/productimg/original/11711572-1815158483432062.jpg&format=webp&auto=avif&width=1200&height=1200&fit=cover"
                }),
                // Elements Renewing Conditioner
                ("Dầu xả phục hồi tóc Elements Renewing Conditioner", new List<string>
                {
                    "https://www.modernhairbeauty.com/wp-content/uploads/2021/08/Elements-renewing-conditioner.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTnbpIdPoJxjZzQ0dJZc1DVy5NRqkHgT7iWDw&s",
                }),
                // Moisture Velvet Nourishing Conditioner
                ("Dầu xả dưỡng ẩm Moisture Velvet", new List<string>
                {
                    "https://www.everythinghairandbeauty.com.au/assets/full/SHUUEMURA-3474630146693.jpg?20220420183416",
                }),
                // Ultimate Remedy Conditioner
                ("Dầu xả Ultimate Remedy", new List<string>
                {
                    "https://www.shuuemuraartofhair-usa.com/dw/image/v2/AANG_PRD/on/demandware.static/-/Sites-shu-master-catalog/default/dw328561bc/2019/Full/Ultimate-Reset/shu-uemura-ultimate-reset-conditioner.jpg?sw=270&sfrm=png&q=70",
                    "https://images.squarespace-cdn.com/content/v1/5b688fd0f407b48b1e37b441/1610912816432-JL6OFF0M1CVE9Z7RWEI9/ultimate_remedy_travel_size_shampoo.jpg?format=1000w",
                }),
                // No. 5 Bond Maintenance Conditioner
                ("Dầu xả bảo dưỡng Bond số 5", new List<string>
                {
                    "https://arterashop.com/wp-content/uploads/2021/01/OLAP5-600x600.jpg",
                    "https://image.hsv-tech.io/1987x0/bbx/common/5e14cb4a-d1d0-449b-b27b-467e92b456ce.webp",
                }),
                // Gold Lust Repair & Restore Conditioner
                ("Dầu xả phục hồi và phục hồi Gold Lust", new List<string>
                {
                    "https://cdn.vuahanghieu.com/unsafe/0x900/left/top/smart/filters:quality(90)/https://admin.vuahanghieu.com/upload/product/2024/03/dau-xa-oribe-gold-lust-repair-restore-conditioner-200ml-66077ad485b29-30032024093708.jpg",
                    "https://bizweb.dktcdn.net/100/445/245/products/3-1719469113736.png?v=1720865387960",
                    "https://www.oribe.com/cdn/shop/products/1200Wx1200H-400103-4_8b6f47b7-d8fe-4613-a565-91248acdafee.jpg?v=1704092392&width=3840"
                }),
                // Signature Moisture Masque
                ("Mặt nạ dưỡng ẩm Signature", new List<string>
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
                ("Chăm sóc da mặt đặc trưng", new List<string>
                {
                    "https://i.pinimg.com/736x/27/81/a7/2781a79854d1a98a16fde4f099bcc8a5.jpg",
                    "https://i.pinimg.com/736x/2c/87/b8/2c87b81f2775b10b59767a43fe28e20f.jpg",
                    "https://i.pinimg.com/736x/5f/fd/bb/5ffdbbdd837d0e1b597bf7b55608ecf9.jpg",
                    "https://i.pinimg.com/736x/7f/86/b9/7f86b9ae476a90f6e13706c96db78e9d.jpg",
                    "https://i.pinimg.com/736x/4c/d2/e4/4cd2e442ed27ad93414300ad5ce65bf9.jpg"
                }),
                // Anti-Aging Facial
                ("Chống lão hóa da mặt", new List<string>
                {
                    "https://i.pinimg.com/736x/42/82/8d/42828de778bdc6d184fde7ef8de141d7.jpg",
                    "https://i.pinimg.com/736x/33/f5/2b/33f52b7c23657d2ac0cd8d9aef3826c7.jpg",
                    "https://i.pinimg.com/736x/c5/25/bc/c525bc3ce54da328c0d5a06226603ac9.jpg",
                    "https://i.pinimg.com/736x/5d/b2/30/5db2300075fe7591f1b7ddc140fb36c0.jpg",
                    "https://i.pinimg.com/736x/28/20/90/28209092643b28bd55e12004a5158fb9.jpg"
                }),
                // Hydrating Facial
                ("Liệu Pháp Dưỡng Ẩm", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/52/e2/c8/52e2c875539100b6fd36dfcb81cb334b.jpg",
                    "https://i.pinimg.com/736x/5e/87/da/5e87daadb4e52e5d9d048bf52b943988.jpg",
                    "https://i.pinimg.com/736x/64/bf/86/64bf86354531fd40bbad0fe5979bbe64.jpg",
                }),
                // Brightening Facial
                ("Liệu Pháp Làm Sáng Da", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/ad/bd/b1/adbdb1e65aac64f3f44cbf7b7a548b30.jpg",
                    "https://i.pinimg.com/736x/66/51/44/665144d405e77c90658a184cc69f1728.jpg",
                    "https://i.pinimg.com/736x/c3/b2/c7/c3b2c74b916a8ca4ad5a111e46d62fe1.jpg",
                    "https://i.pinimg.com/736x/95/24/b1/9524b1129e39e0c4e8336ee825469156.jpg"
                }),
                // Acne Treatment Facial
                ("Liệu Pháp Trị Mụn", new List<string>
                {
                    "https://i.pinimg.com/736x/fc/c4/6b/fcc46b80915cd02cde8c8f7d8975c01b.jpg",
                    "https://i.pinimg.com/736x/c6/14/6d/c6146dba83a0356ede4f775a46b64a34.jpg",
                    "https://i.pinimg.com/736x/33/38/e3/3338e3a12f420ba6b75ef9097acc8329.jpg",
                    "https://i.pinimg.com/736x/dc/92/9f/dc929f3ff80c4f48be5fb81809426022.jpg",
                    "https://i.pinimg.com/736x/95/24/b1/9524b1129e39e0c4e8336ee825469156.jpg"
                }),
                // Soothing Facial
                ("Liệu Pháp Làm Dịu Da", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/50/72/37/5072376f6e17d3219b0404e93a8cd89b.jpg",
                    "https://i.pinimg.com/736x/d8/62/97/d862974838bb5477234ddfe9130df3cb.jpg",
                    "https://i.pinimg.com/736x/90/e8/8b/90e88bf68152266da89017591b6cbed6.jpg",
                }),
                // Green Tea Facial
                ("Liệu Pháp Trà Xanh", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/7a/b9/40/7ab940119bbfd56905f68154084c73f6.jpg",
                    "https://i.pinimg.com/736x/35/78/64/357864ef765a43a798742ed02ebf3766.jpg",
                    "https://i.pinimg.com/736x/4f/94/81/4f9481757db772237b2f348d3f24d47a.jpg",
                }),
                // Collagen Boost Facial
                ("Liệu Pháp Tăng Cường Collagen", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/ac/ad/30/acad30ac766eb1e68002d8e45d60d7d3.jpg",
                    "https://i.pinimg.com/736x/63/b3/01/63b30170bfe60842ac58502f52b7f250.jpg",
                    "https://i.pinimg.com/736x/5b/77/92/5b7792126b2a90abe9b4bbf1eb0ef76b.jpg",
                    "https://i.pinimg.com/736x/3f/a4/30/3fa430040c0aa63d7682ef4457b8251d.jpg"
                }),
                // Detox Facial
                ("Liệu Pháp Thải Độc Da", new List<string>
                {
                    "https://i.pinimg.com/736x/2f/d5/55/2fd555a265af712266ff825946d84c36.jpg",
                    "https://i.pinimg.com/736x/26/7c/46/267c46cac2f68d850351bb84f1a05aac.jpg",
                    "https://i.pinimg.com/736x/66/51/44/665144d405e77c90658a184cc69f1728.jpg",
                    "https://i.pinimg.com/736x/4f/94/81/4f9481757db772237b2f348d3f24d47a.jpg",
                    "https://i.pinimg.com/736x/8e/83/a6/8e83a64aac39e18f75acaf0efe8f0df9.jpg"
                }),
                // Overnight Hydration Facial
                ("Liệu Pháp Dưỡng Ẩm Qua Đêm", new List<string>
                {
                    "https://i.pinimg.com/736x/58/b1/bf/58b1bf4c47a37a4f21b99a7a0bc35a8e.jpg",
                    "https://i.pinimg.com/736x/a1/6a/4b/a16a4b0cb8492406f5712927de9e2c05.jpg",
                    "https://i.pinimg.com/736x/bb/4f/14/bb4f14b2a54b5df40aaf29964618faf0.jpg",
                    "https://i.pinimg.com/736x/4a/a3/dc/4aa3dc07ca1938573d318717c437e0a6.jpg",
                }),
                // Swedish Massage
                ("Massage Thụy Điển", new List<string>
                {
                    "https://i.pinimg.com/736x/a4/e5/c6/a4e5c65d43fc9a6aa54dd3160b9e839d.jpg",
                    "https://www.health.com/thmb/K_Vtfnh3Yu-Ceya3aETxfH72k9Q=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/GettyImages-1175433234-034014dc5b9c45edaeaf04c7b80ceafc.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRR32fYGDt-A-868QxQbHt0O3YK7ZP6Och_7Q&s",
                }),
                // Full Body Scrub
                ("Tẩy tế bào chết toàn thân", new List<string>
                {
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcShtNFeG36aaJZsLyW4Jbq2mnJQq1EUVsb4CTkCP36InrBM9-LyRiyd_YZeutVVcuF1ub0&usqp=CAU",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ6DKTthrU1lo3VIdPKVfIu0IjVY7EfpO7KocRY6xFhpMG4dWfhcGSREJ-TMDZ9mog2O-o&usqp=CAU",
                    "https://i.pinimg.com/736x/aa/fc/b5/aafcb5c41918eac7a035334e26becc05.jpg",
                }),
                // Moisturizing Body Wrap
                ("Liệu Pháp Quấn Dưỡng Ẩm", new List<string>
                {
                    "https://i.pinimg.com/736x/d9/7a/41/d97a41c66c2b8c27413e5b8386ee6734.jpg",
                    "https://i.pinimg.com/736x/bd/78/87/bd7887c01d97a59665f04f62616f78da.jpg",
                }),
                // Aromatherapy Massage
                ("Massage Liệu Pháp Hương Thơm", new List<string>
                {
                    "https://i.pinimg.com/736x/c6/c8/c6/c6c8c65052fb8f59d3c7b495f47aace6.jpg",
                    "https://i0.wp.com/www.absolute-aromas.com/wp-content/uploads/2022/11/aromatherapy_massage_techniques.jpg?fit=1280%2C848&ssl=1",
                    "https://i.pinimg.com/736x/25/fc/d0/25fcd0f1f682b5ae41eafcc0d1397b5c.jpg",
                    "https://i.pinimg.com/736x/04/09/be/0409be31db953306c4ba2021a8a10b84.jpg",
                }),
                // Foot Massage
                ("Massage chân", new List<string>
                {
                    "https://i.pinimg.com/736x/b3/ee/14/b3ee141b5983c783a88d7160a30e734a.jpg",
                    "https://i.pinimg.com/736x/ad/55/f8/ad55f85f17ae9b07cd6c94bd67e28754.jpg",
                    "https://i.pinimg.com/736x/55/49/69/5549692df73157f1bf2c49a8fb6daf52.jpg",
                    "https://i.pinimg.com/736x/26/d2/b9/26d2b991409c50d6baf530cbc7397b77.jpg",
                    "https://i.pinimg.com/736x/37/e9/64/37e964a2263041df8131c138f9219d79.jpg"
                }),
                // Abdominal Massage
                ("Massage Vùng Bụng", new List<string>
                {
                    "https://images.squarespace-cdn.com/content/v1/5f2864b6ee63644ee0b157d3/1716227683804-A6SSUR28MIBPHBSSXODP/stomach+massage+for+constipation.jpg",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQFO_Je7Wo2NTv7rKUoQNB2LCWShvPvie0FSVRE6zzkQjk92ika0RruE9bJa5YZAXa74pk&usqp=CAU",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRdxvYhv5difFnJ1XoXVyNoIdxHn08KX5ZEs5Zm_oXmJkmLiZM8mQFXKP6kH1YbHICY5JE&usqp=CAU",
                    "https://i.pinimg.com/736x/4e/f2/9b/4ef29b5b0d121283f8b07cd16ba8f562.jpg",
                }),
                // Detox Body Treatment
                ("Liệu Pháp Thải Độc Cơ Thể", new List<string>
                {
                    "https://i.pinimg.com/736x/c2/e0/42/c2e0422404b022ae69d9ba98cd659748.jpg",
                    "https://i.pinimg.com/736x/62/78/07/627807715ea10ff33f2b633e86fec023.jpg",
                    "https://i.pinimg.com/736x/fa/60/1b/fa601bea62f56aeae561dbf884d4e184.jpg",
                    "https://i.pinimg.com/736x/a4/16/c7/a416c7d0d39a76dec17f7fed548e65a7.jpg",
                }),
                // Mud Bath
                ("Tắm bùn", new List<string>
                {
                    "https://i.pinimg.com/736x/a0/1c/41/a01c4134bff32537d93b0db8cfe4ba52.jpg",
                    "https://i.pinimg.com/736x/e5/94/0d/e5940d85b2ab0a370a05b2e15f6689e3.jpg",
                    "https://i.pinimg.com/736x/b1/c0/6f/b1c06f280faaac6beb3f3ea3cdb60063.jpg",
                    "https://i.pinimg.com/736x/35/8b/6a/358b6a8da772fb59bea486501845148c.jpg",
                }),
                // Body Polish
                ("Tẩy Bóng Da", new List<string>
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
                new Blog
                {
                    Title = "Làm thế nào để thư giãn tại Spa",
                    Content = "Tìm hiểu những cách tốt nhất để tận hưởng một ngày thư giãn tại spa.", AuthorId = 1,
                    Status = "Accept", Note = "Blog phổ biến"
                },
                new Blog
                {
                    Title = "Lợi ích của việc chăm sóc da mặt",
                    Content = "Khám phá những lợi ích của việc chăm sóc da mặt.", AuthorId = 2, Status = "Published",
                    Note = "Thông tin"
                },
                new Blog
                {
                    Title = "5 phương pháp trị liệu spa hàng đầu giúp giảm căng thẳng",
                    Content = "Hướng dẫn các phương pháp trị liệu spa hiệu quả nhất giúp giảm căng thẳng.",
                    AuthorId = 3, Status = "Draft", Note = "Cần xem xét lại"
                },
                new Blog
                {
                    Title = "Mẹo chăm sóc da cho người mới bắt đầu",
                    Content = "Mẹo chăm sóc da đơn giản và hiệu quả cho người mới bắt đầu.", AuthorId = 4,
                    Status = "Accept", Note = "Tuyệt vời cho người mới bắt đầu"
                },
                new Blog
                {
                    Title = "Nghệ thuật trị liệu bằng hương thơm",
                    Content = "Khám phá những lợi ích và kỹ thuật của liệu pháp hương thơm.", AuthorId = 5,
                    Status = "Pending", Note = "Đang chờ phê duyệt"
                },
                new Blog
                {
                    Title = "Làm thế nào để chọn gói spa phù hợp",
                    Content = "Mẹo lựa chọn gói spa phù hợp nhất với nhu cầu của bạn.", AuthorId = 1, Status = "Accept",
                    Note = "Khách hàng ưa thích"
                },
                new Blog
                {
                    Title = "Tầm quan trọng của việc tự chăm sóc",
                    Content = "Tại sao việc tự chăm sóc lại cần thiết cho sức khỏe tinh thần và thể chất.",
                    AuthorId = 2, Status = "Accept", Note = "Động lực"
                },
                new Blog
                {
                    Title = "Khám phá liệu pháp đá nóng", Content = "Mọi thứ bạn cần biết về liệu pháp đá nóng.",
                    AuthorId = 3, Status = "Rejected", Note = "Cần thêm nội dung"
                },
                new Blog
                {
                    Title = "Mẹo duy trì làn da khỏe mạnh",
                    Content = "Thực hành tốt nhất để giữ cho làn da khỏe mạnh và rạng rỡ.", AuthorId = 4,
                    Status = "Accept", Note = "Đã được nghiên cứu kỹ lưỡng"
                },
                new Blog
                {
                    Title = "Khoa học đằng sau liệu pháp spa",
                    Content = "Hiểu được lợi ích của liệu pháp spa đối với cơ thể và tâm trí của bạn.", AuthorId = 5,
                    Status = "Pending", Note = "Thông tin chi tiết"
                }
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
                    Status = random.NextDouble() < 0.5
                        ? "Active"
                        : "Inactive", // Random status (50% chance for active or inactive)
                    Description = "Phiếu giảm giá cho nhiều sản phẩm khác nhau", // Example description
                    DiscountAmount = random.Next(5, 50) * 1000, // Random discount between 5% and 50%
                    MinOrderAmount = random.Next(5, 500),
                    ValidFrom = DateTime.Now.AddDays(-random.Next(30,
                        60)), // Random start date (between 30 and 60 days ago)
                    ValidTo = DateTime.Now.AddDays(random.Next(30,
                        60)), // Random expiration date (30 to 60 days from now)
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
                    OrderCode = random.Next(100000, 999999), // Random order code
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
                        var branchId = random.Next(1, 5); // Random BranchId (assuming you have 4 branches)
                        var randomProduct = products[random.Next(products.Count)];
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            Quantity = quantity, // Random số lượng từ 1 đến 2
                            UnitPrice = unitPrice, // Random giá từ 10 đến 100
                            SubTotal = quantity * unitPrice,
                            ProductId = randomProduct.ProductId,
                            BranchId = branchId,
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
                            StaffId = staffList[random.Next(staffList.Count)].StaffId, // Random StaffId
                            ServiceId = services[random.Next(services.Count)].ServiceId, // Random ServiceId
                            BranchId = branches[random.Next(branches.Count)].BranchId, // Random BranchId

                            // Random thời gian hẹn trong khoảng 30 ngày tới
                            AppointmentsTime = DateTime.Now.AddDays(random.Next(1, 31)).AddHours(random.Next(8, 18)),

                            Status = random.Next(2) == 0 ? "Pending" : "Completed", // Ngẫu nhiên trạng thái
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
            var random = new Random();
            var skincareRoutines = new List<SkincareRoutine>
            {
                new SkincareRoutine
                {
                    Name = "Da dầu",
                    Description = "Chăm sóc da dầu và giảm bã nhờn dư thừa.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Da dầu",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Da khô",
                    Description = "Dưỡng ẩm và nuôi dưỡng làn da khô.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Da khô",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Da trung tính",
                    Description = "Duy trì sự cân bằng trung tính cho da.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Da trung tính",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Da hỗn hợp",
                    Description = "Chăm sóc cho cả vùng da khô và da dầu.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Da hỗn hợp",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Mụn đầu đen",
                    Description = "Giúp làm sạch và ngăn ngừa mụn đầu đen.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Mụn đầu đen",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Mụn trứng cá",
                    Description = "Liệu trình chăm sóc da giảm mụn trứng cá.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Mụn trứng cá",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Quầng thâm mắt",
                    Description = "Giúp giảm quầng thâm dưới mắt.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Quầng thâm mắt",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Mụn có nhân đóng",
                    Description = "Liệu trình kiểm soát mụn có nhân đóng.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Mụn có nhân đóng",
                    TotalPrice = random.Next(500000, 5000000)
                },
                new SkincareRoutine
                {
                    Name = "Nếp nhăn Glabella",
                    Description = "Giúp giảm nếp nhăn vùng giữa hai lông mày.",
                    TotalSteps = 4,
                    TargetSkinTypes = "Nếp nhăn Glabella",
                    TotalPrice = random.Next(500000, 5000000)
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
                        "Da dầu" => new[] { 3, 5, 31, 44, 45, 46 },
                        "Da khô" => new[] { 1, 6, 13, 24, 28, 39, 42, 48 },
                        "Da trung tính" => new[] { 2, 7, 20, 26, 30, 33, 44, 49, 50 },
                        "Da hỗn hợp" => new[] { 10, 11, 15, 23, 32, 34, 43, 45, 48 },
                        "Mụn đầu đen" => new[] { 3, 9, 11, 25, 34, 38, 45, 46 },
                        "Mụn trứng cá" => new[] { 6, 9, 14, 19, 31, 33, 40, 44, 45 },
                        "Quầng thâm mắt" => new[] { 1, 7, 24, 26, 28, 43, 44, 50 },
                        "Mụn có nhân đóng" => new[] { 5, 10, 11, 23, 31, 34, 45, 46 },
                        "Nếp nhăn Glabella" => new[] { 2, 6, 8, 28, 32, 41, 42, 48 },
                        _ => Array.Empty<int>()
                    };

                    var routineServices = routine.Name switch
                    {
                        "Da dầu" => new[] { 4, 9 },
                        "Da khô" => new[] { 3, 6, 10 },
                        "Da trung tính" => new[] { 1, 5, 8 },
                        "Da hỗn hợp" => new[] { 2, 7 },
                        "Mụn đầu đen" => new[] { 4, 5, 9 },
                        "Mụn trứng cá" => new[] { 5, 6, 9 },
                        "Quầng thâm mắt" => new[] { 3, 4, 8 },
                        "Mụn có nhân đóng" => new[] { 4, 7 },
                        "Nếp nhăn glabella" => new[] { 2, 3, 8 },
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


        public async Task SeedSkincareRoutineSteps()
        {
            var skincareRoutines = _context.SkincareRoutines.ToList();
            var productRoutineSteps = new List<ProductRoutineStep>();
            var serviceRoutineSteps = new List<ServiceRoutineStep>();
            var random = new Random();

            // Danh sách tên các bước phổ biến
            var defaultSteps = new List<string> { "Rửa mặt", "Tẩy tế bào chết", "Dưỡng ẩm", "Bôi serum", "Chống nắng" };

            foreach (var routine in skincareRoutines)
            {
                var totalSteps = routine.TotalSteps ?? 1;

                // Lấy sản phẩm & dịch vụ liên quan đến routine
                var routineProducts = _context.ProductRoutines
                    .Where(p => p.RoutineId == routine.SkincareRoutineId)
                    .Select(p => p.ProductId)
                    .ToList();

                var routineServices = _context.ServiceRoutine
                    .Where(s => s.RoutineId == routine.SkincareRoutineId)
                    .Select(s => s.ServiceId)
                    .ToList();

                for (int i = 0; i < totalSteps; i++)
                {
                    var stepName = i < defaultSteps.Count ? defaultSteps[i] : $"Bước {i + 1}";

                    var stepEntity = new SkinCareRoutineStep
                    {
                        SkincareRoutineId = routine.SkincareRoutineId,
                        Name = stepName,
                        Step = i + 1,
                        IntervalBeforeNextStep = i < totalSteps - 1 ? 2 : null
                    };

                    var existingStep = _context.SkinCareRoutineStep
                        .FirstOrDefault(s =>
                            s.SkincareRoutineId == stepEntity.SkincareRoutineId && s.Step == stepEntity.Step);

                    if (existingStep == null)
                    {
                        _context.SkinCareRoutineStep.Add(stepEntity);
                        await _context.SaveChangesAsync(); // Lưu ngay để lấy `StepId`
                    }
                    else
                    {
                        stepEntity = existingStep;
                    }

                    // Lựa chọn ngẫu nhiên sản phẩm
                    var productTakeCount = Math.Max(1, routineProducts.Count / Math.Max(1, totalSteps));
                    var selectedProducts = routineProducts
                        .OrderBy(_ => random.Next())
                        .Take(productTakeCount)
                        .ToList();

                    foreach (var productId in selectedProducts)
                    {
                        if (!_context.ProductRoutineSteps.Any(s =>
                                s.StepId == stepEntity.SkinCareRoutineStepId && s.ProductId == productId))
                        {
                            productRoutineSteps.Add(new ProductRoutineStep
                            {
                                StepId = stepEntity.SkinCareRoutineStepId,
                                ProductId = productId,
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now
                            });
                        }
                    }

                    // Lựa chọn ngẫu nhiên dịch vụ
                    var serviceTakeCount = Math.Max(1, routineServices.Count / Math.Max(1, totalSteps));
                    var selectedServices = routineServices
                        .OrderBy(_ => random.Next())
                        .Take(serviceTakeCount)
                        .ToList();

                    foreach (var serviceId in selectedServices)
                    {
                        if (!_context.ServiceRoutineSteps.Any(s =>
                                s.StepId == stepEntity.SkinCareRoutineStepId && s.ServiceId == serviceId))
                        {
                            serviceRoutineSteps.Add(new ServiceRoutineStep
                            {
                                StepId = stepEntity.SkinCareRoutineStepId,
                                ServiceId = serviceId,
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now
                            });
                        }
                    }
                }
            }

            if (productRoutineSteps.Any())
                _context.ProductRoutineSteps.AddRange(productRoutineSteps);

            if (serviceRoutineSteps.Any())
                _context.ServiceRoutineSteps.AddRange(serviceRoutineSteps);

            await _context.SaveChangesAsync();
        }


        private async Task SeedConcerns()
        {
            var concerns = new List<SkinConcern>
            {
                new SkinConcern { Code = "skin_type_0", Name = "Da dầu" },
                new SkinConcern { Code = "skin_type_1", Name = "Da khô" },
                new SkinConcern { Code = "skin_type_2", Name = "Da trung tính" },
                new SkinConcern { Code = "skin_type_3", Name = "Da hỗn hợp" },
                new SkinConcern { Code = "blackhead", Name = "Mụn đầu đen" },
                new SkinConcern { Code = "acne", Name = "Mụn trứng cá" },
                new SkinConcern { Code = "dark_circle", Name = "Quầng thâm mắt" },
                new SkinConcern { Code = "closed_comedones", Name = "Mụn có nhân đóng" },
                new SkinConcern { Code = "glabella_wrinkle", Name = "Nếp nhăn giữa hai chân mày" },

                // Concerns mới dựa theo dữ liệu JSON
                new SkinConcern { Code = "eye_pouch", Name = "Bọng mắt" },
                new SkinConcern { Code = "crows_feet", Name = "Nếp nhăn đuôi mắt" },
                new SkinConcern { Code = "eye_finelines", Name = "Vết nhăn quanh mắt" },
                new SkinConcern { Code = "forehead_wrinkle", Name = "Nếp nhăn trán" },
                new SkinConcern { Code = "nasolabial_fold", Name = "Nếp nhăn rãnh cười" },
                new SkinConcern { Code = "skin_spot", Name = "Đốm sắc tố" },
                new SkinConcern { Code = "mole", Name = "Nốt ruồi" },
                new SkinConcern { Code = "enlarged_pores", Name = "Lỗ chân lông to" }
            };

            // Kiểm tra nếu chưa có concern nào mới thêm vào
            if (!_context.SkinConcern.Any())
            {
                await _context.SkinConcern.AddRangeAsync(concerns);
                await _context.SaveChangesAsync();
            }
        }


        public async Task SeedSkincareRoutineConcerns()
        {
            var routineConcernMapping = new Dictionary<string, string>
            {
                { "Da dầu", "skin_type_0" },
                { "Da khô", "skin_type_1" },
                { "Da trung tính", "skin_type_2" },
                { "Da hỗn hợp", "skin_type_3" },
                { "Mụn đầu đen", "blackhead" },
                { "Mụn trứng cá", "acne" },
                { "Quầng thâm mắt", "dark_circle" },
                { "Mụn có nhân đóng", "closed_comedones" },
                { "Nếp nhăn Glabella", "glabella_wrinkle" }
            };

            foreach (var pair in routineConcernMapping)
            {
                var routine = await _context.SkincareRoutines.FirstOrDefaultAsync(r => r.Name == pair.Key);
                var concern = await _context.SkinConcern.FirstOrDefaultAsync(c => c.Code == pair.Value);

                if (routine != null && concern != null)
                {
                    // Kiểm tra nếu chưa tồn tại liên kết thì mới thêm
                    var exists = await _context.SkincareRoutineConcern
                        .AnyAsync(rc =>
                            rc.SkincareRoutineId == routine.SkincareRoutineId &&
                            rc.SkinConcernId == concern.SkinConcernId);

                    if (!exists)
                    {
                        _context.SkincareRoutineConcern.Add(new SkincareRoutineConcern
                        {
                            SkincareRoutineId = routine.SkincareRoutineId,
                            SkinConcernId = concern.SkinConcernId
                        });
                    }
                }
            }

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