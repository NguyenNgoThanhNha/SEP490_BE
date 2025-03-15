using Microsoft.EntityFrameworkCore;

namespace Server.Data.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {

        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<Category> Categorys { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Shipping> Shippings { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<BlogRating> BlogRatings { get; set; }
        public DbSet<Branch_Service> Branch_Services { get; set; }
        public DbSet<Branch_Product> Branch_Products { get; set; }
        public DbSet<Branch_Promotion> Branch_Promotions { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<ServiceImages> ServiceImages { get; set; }
        public DbSet<SkinHealth> SkinHealths { get; set; }
        public DbSet<SkincareRoutine> SkincareRoutines { get; set; }
        public DbSet<SkinCareRoutineStep> SkinCareRoutineStep { get; set; }
        public DbSet<UserRoutine> UserRoutines { get; set; }
        public DbSet<ProductRoutine> ProductRoutines { get; set; }
        public DbSet<ServiceRoutine> ServiceRoutine { get; set; }
        public DbSet<Logger> Logger { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<ProductCart> ProductCart { get; set; }
        public DbSet<ServiceCategory> ServiceCategory { get; set; }
        
        public DbSet<AppointmentFeedback> AppointmentFeedback { get; set; }
        public DbSet<ProductFeedback> ProductFeedback { get; set; }
        public DbSet<ServiceFeedback> ServiceFeedback { get; set; }
        public DbSet<UserRoutineStep> UserRoutineStep { get; set; }
        public DbSet<UserRoutineLogger> UserRoutineLogger { get; set; }

        public DbSet<Shifts> Shifts { get; set; }
        public DbSet<StaffRole> StaffRole { get; set; }
        public DbSet<StaffLeave> StaffLeave { get; set; }
        public DbSet<WorkSchedule> WorkSchedule { get; set; }
        public DbSet<SkinHealthImage> SkinHealthImage { get; set; }
        public DbSet<Staff_ServiceCategory> Staff_ServiceCategory { get; set; }
        
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Branch_Service
            modelBuilder.Entity<Branch_Service>(e =>
            {
                e.ToTable("Branch_Service");
                e.HasKey(e => e.Id);
                e.HasIndex(e => new { e.BranchId, e.ServiceId }).IsUnique();

                e.HasOne(e => e.Branch)
                    .WithMany(e => e.Branch_Services)
                    .HasForeignKey(e => e.BranchId)
                    .HasConstraintName("FK_Branch_Service_Branch");

                e.HasOne(e => e.Service)
                    .WithMany(e => e.Branch_Services)
                    .HasForeignKey(e => e.ServiceId)
                    .HasConstraintName("FK_Branch_Service_Service");

            });
            
            // Product_Service
            modelBuilder.Entity<Branch_Product>(e =>
            {
                e.ToTable("Branch_Product");
                e.HasKey(e => e.Id);
                e.HasIndex(e => new { e.ProductId, e.BranchId }).IsUnique();

                e.HasOne(e => e.Product)
                    .WithMany(e => e.Branch_Products)
                    .HasForeignKey(e => e.ProductId)
                    .HasConstraintName("FK_Branch_Product_Product");

                e.HasOne(e => e.Branch)
                    .WithMany(e => e.Branch_Products)
                    .HasForeignKey(e => e.BranchId)
                    .HasConstraintName("FK_Branch_Product_Branch");
            });
            
            // UserRoutines
            modelBuilder.Entity<UserRoutine>(e =>
            {
                e.ToTable("UserRoutine");
                e.HasKey(e => e.UserRoutineId);
                e.HasIndex(e => new { e.UserId, e.RoutineId }).IsUnique();

                e.HasOne(e => e.User)
                    .WithMany(e => e.UserRoutines)
                    .HasForeignKey(e => e.UserId)
                    .HasConstraintName("FK_User_Routine_User");

                e.HasOne(e => e.Routine)
                    .WithMany(e => e.UserRoutines)
                    .HasForeignKey(e => e.RoutineId)
                    .HasConstraintName("FK_User_Routine_Routine");
            });
            
            // UserRoutines
            modelBuilder.Entity<UserRoutine>(e =>
            {
                e.ToTable("UserRoutine");
                e.HasKey(e => e.UserRoutineId);
                e.HasIndex(e => new { e.UserId, e.RoutineId }).IsUnique();

                e.HasOne(e => e.User)
                    .WithMany(e => e.UserRoutines)
                    .HasForeignKey(e => e.UserId)
                    .HasConstraintName("FK_User_Routine_User");

                e.HasOne(e => e.Routine)
                    .WithMany(e => e.UserRoutines)
                    .HasForeignKey(e => e.RoutineId)
                    .HasConstraintName("FK_User_Routine_Routine");
            });
            
            // ProductRoutines
            modelBuilder.Entity<ProductRoutine>(e =>
            {
                e.ToTable("ProductRoutine");
                e.HasKey(e => e.ProductRoutineId);
                e.HasIndex(e => new { e.ProductId, e.RoutineId }).IsUnique();

                e.HasOne(e => e.Products)
                    .WithMany(e => e.ProductRoutines)
                    .HasForeignKey(e => e.ProductId)
                    .HasConstraintName("FK_Product_Routine_Product");

                e.HasOne(e => e.Routine)
                    .WithMany(e => e.ProductRoutines)
                    .HasForeignKey(e => e.RoutineId)
                    .HasConstraintName("FK_Product_Routine_Routine");
            });
            
            // ServiceRoutine
            modelBuilder.Entity<ServiceRoutine>(e =>
            {
                e.ToTable("ServiceRoutine");
                e.HasKey(e => e.ServiceRoutineId);
                e.HasIndex(e => new { e.ServiceId, e.RoutineId }).IsUnique();

                e.HasOne(e => e.Service)
                    .WithMany(e => e.ServiceRoutines)
                    .HasForeignKey(e => e.ServiceId)
                    .HasConstraintName("FK_Service_Routine_Service");

                e.HasOne(e => e.Routine)
                    .WithMany(e => e.ServiceRoutines)
                    .HasForeignKey(e => e.RoutineId)
                    .HasConstraintName("FK_Service_Routine_Routine");
            });
            
            // ProductCart
            modelBuilder.Entity<ProductCart>(e =>
            {
                e.ToTable("ProductCart");
                e.HasKey(e => e.ProductCartId);
                e.HasIndex(e => new { e.ProductId, e.CartId }).IsUnique();

                e.HasOne(e => e.Product)
                    .WithMany(e => e.ProductCarts)
                    .HasForeignKey(e => e.ProductId)
                    .HasConstraintName("FK_Product_Cart_Product");

                e.HasOne(e => e.Cart)
                    .WithMany(e => e.ProductCarts)
                    .HasForeignKey(e => e.CartId)
                    .HasConstraintName("FK_Product_Cart_Cart");
            });
        }
    }
}
