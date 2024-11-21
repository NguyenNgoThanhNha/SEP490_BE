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
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<BlogRating> BlogRatings { get; set; }
        public DbSet<Branch_Service> Branch_Services { get; set; }
        public DbSet<Branch_Product> Branch_Products { get; set; }
        public DbSet<Branch_Promotion> Branch_Promotions { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        
      
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
        }
    }
}
