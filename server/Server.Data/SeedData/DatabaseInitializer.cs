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

            var startDate = new DateTime(2025, 4, 1); // Bắt đầu từ tuần đầu tiên của tháng (Thứ 2)
            var endDate = new DateTime(2025, 5, 31); // Kết thúc vào thứ 7 của tuần cuối cùng

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
                    District = 3317,
                    WardCode = 55079,
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
                    District = 3317,
                    WardCode = 55079,
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
                    District = 3317,
                    WardCode = 55079,
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
                    District = 3317,
                    WardCode = 55079,
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
                    District = 3317,
                    WardCode = 55079,
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
                })
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