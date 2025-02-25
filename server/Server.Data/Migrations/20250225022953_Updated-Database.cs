using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BedType",
                columns: table => new
                {
                    BedTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BedType", x => x.BedTypeId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.CompanyId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Logger",
                columns: table => new
                {
                    LoggerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(type: "longtext", nullable: false),
                    Exception = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logger", x => x.LoggerId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Promotion",
                columns: table => new
                {
                    PromotionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PromotionName = table.Column<string>(type: "longtext", nullable: false),
                    PromotionDescription = table.Column<string>(type: "longtext", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    Image = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotion", x => x.PromotionId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceCategory",
                columns: table => new
                {
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategory", x => x.ServiceCategoryId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    ShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ShiftName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.ShiftId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SkincareRoutine",
                columns: table => new
                {
                    SkincareRoutineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Steps = table.Column<string>(type: "longtext", nullable: true),
                    Frequency = table.Column<string>(type: "longtext", nullable: true),
                    TargetSkinTypes = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkincareRoutine", x => x.SkincareRoutineId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StaffRole",
                columns: table => new
                {
                    StaffRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    StaffRoleName = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffRole", x => x.StaffRoleId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoleName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.RoleId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Voucher",
                columns: table => new
                {
                    VoucherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "longtext", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RemainQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voucher", x => x.VoucherId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductName = table.Column<string>(type: "longtext", nullable: false),
                    ProductDescription = table.Column<string>(type: "longtext", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Dimension = table.Column<string>(type: "longtext", nullable: true),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    SkinTypeSuitable = table.Column<string>(type: "longtext", nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Product_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Duration = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    Steps = table.Column<string>(type: "longtext", nullable: true),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.ServiceId);
                    table.ForeignKey(
                        name: "FK_Service_ServiceCategory_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategory",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Avatar = table.Column<string>(type: "longtext", nullable: true),
                    Gender = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OTPCode = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    STK = table.Column<string>(type: "longtext", nullable: true),
                    Bank = table.Column<string>(type: "longtext", nullable: true),
                    QRCode = table.Column<string>(type: "longtext", nullable: true),
                    AccountBalance = table.Column<string>(type: "longtext", nullable: true),
                    CreateBy = table.Column<string>(type: "longtext", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    ModifyBy = table.Column<string>(type: "longtext", nullable: true),
                    ModifyDate = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    BonusPoint = table.Column<int>(type: "int", nullable: false),
                    TypeLogin = table.Column<string>(type: "longtext", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_UserRole_RoleID",
                        column: x => x.RoleID,
                        principalTable: "UserRole",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    ProductImagesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    image = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.ProductImagesId);
                    table.ForeignKey(
                        name: "FK_ProductImages_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductRoutine",
                columns: table => new
                {
                    ProductRoutineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RoutineId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRoutine", x => x.ProductRoutineId);
                    table.ForeignKey(
                        name: "FK_Product_Routine_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_Routine_Routine",
                        column: x => x.RoutineId,
                        principalTable: "SkincareRoutine",
                        principalColumn: "SkincareRoutineId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceImages",
                columns: table => new
                {
                    ServiceImagesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    image = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceImages", x => x.ServiceImagesId);
                    table.ForeignKey(
                        name: "FK_ServiceImages_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceRoutine",
                columns: table => new
                {
                    ServiceRoutineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    RoutineId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRoutine", x => x.ServiceRoutineId);
                    table.ForeignKey(
                        name: "FK_Service_Routine_Routine",
                        column: x => x.RoutineId,
                        principalTable: "SkincareRoutine",
                        principalColumn: "SkincareRoutineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Service_Routine_Service",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Blog",
                columns: table => new
                {
                    BlogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog", x => x.BlogId);
                    table.ForeignKey(
                        name: "FK_Blog_User_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BranchName = table.Column<string>(type: "longtext", nullable: false),
                    BranchAddress = table.Column<string>(type: "longtext", nullable: false),
                    BranchPhone = table.Column<string>(type: "longtext", nullable: false),
                    LongAddress = table.Column<string>(type: "longtext", nullable: true),
                    LatAddress = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branch_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_User_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    CartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Cart_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    ChatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_Chat_User_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_User_SenderId",
                        column: x => x.SenderId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false),
                    isRead = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderCode = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VoucherId = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderType = table.Column<string>(type: "longtext", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Order_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Voucher_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Voucher",
                        principalColumn: "VoucherId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductFeedback",
                columns: table => new
                {
                    ProductFeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "longtext", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    ImageBefore = table.Column<string>(type: "longtext", nullable: true),
                    ImageAfter = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFeedback", x => x.ProductFeedbackId);
                    table.ForeignKey(
                        name: "FK_ProductFeedback_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductFeedback_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServiceFeedback",
                columns: table => new
                {
                    ServiceFeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "longtext", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    ImageBefore = table.Column<string>(type: "longtext", nullable: true),
                    ImageAfter = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceFeedback", x => x.ServiceFeedbackId);
                    table.ForeignKey(
                        name: "FK_ServiceFeedback_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceFeedback_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SkinHealth",
                columns: table => new
                {
                    SkinHealthId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SkinColor = table.Column<string>(type: "longtext", nullable: true),
                    SkinToneIta = table.Column<string>(type: "longtext", nullable: true),
                    SkinTone = table.Column<string>(type: "longtext", nullable: true),
                    SkinHueHa = table.Column<string>(type: "longtext", nullable: true),
                    SkinAge = table.Column<string>(type: "longtext", nullable: true),
                    SkinType = table.Column<string>(type: "longtext", nullable: true),
                    LeftEyelids = table.Column<string>(type: "longtext", nullable: true),
                    RightEyelids = table.Column<string>(type: "longtext", nullable: true),
                    EyePouch = table.Column<string>(type: "longtext", nullable: true),
                    EyePouchSeverity = table.Column<string>(type: "longtext", nullable: true),
                    DarkCircle = table.Column<string>(type: "longtext", nullable: true),
                    ForeheadWrinkle = table.Column<string>(type: "longtext", nullable: true),
                    CrowsFeet = table.Column<string>(type: "longtext", nullable: true),
                    EyeFineLines = table.Column<string>(type: "longtext", nullable: true),
                    GlabellaWrinkle = table.Column<string>(type: "longtext", nullable: true),
                    NasolabialFold = table.Column<string>(type: "longtext", nullable: true),
                    NasolabialFoldSeverity = table.Column<string>(type: "longtext", nullable: true),
                    PoresForehead = table.Column<string>(type: "longtext", nullable: true),
                    PoresLeftCheek = table.Column<string>(type: "longtext", nullable: true),
                    PoresRightCheek = table.Column<string>(type: "longtext", nullable: true),
                    PoresJaw = table.Column<string>(type: "longtext", nullable: true),
                    BlackHead = table.Column<string>(type: "longtext", nullable: true),
                    Rectangle = table.Column<string>(type: "longtext", nullable: true),
                    Mole = table.Column<string>(type: "longtext", nullable: true),
                    ClosedComedones = table.Column<string>(type: "longtext", nullable: true),
                    SkinSpot = table.Column<string>(type: "longtext", nullable: true),
                    FaceMaps = table.Column<string>(type: "longtext", nullable: true),
                    Sensitivity = table.Column<string>(type: "longtext", nullable: true),
                    SensitivityArea = table.Column<string>(type: "longtext", nullable: true),
                    SensitivityIntensity = table.Column<string>(type: "longtext", nullable: true),
                    Ance = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinHealth", x => x.SkinHealthId);
                    table.ForeignKey(
                        name: "FK_SkinHealth_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoutine",
                columns: table => new
                {
                    UserRoutineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoutineId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    ProgressNotes = table.Column<string>(type: "longtext", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoutine", x => x.UserRoutineId);
                    table.ForeignKey(
                        name: "FK_User_Routine_Routine",
                        column: x => x.RoutineId,
                        principalTable: "SkincareRoutine",
                        principalColumn: "SkincareRoutineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Routine_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BlogComment",
                columns: table => new
                {
                    BlogCommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogComment", x => x.BlogCommentId);
                    table.ForeignKey(
                        name: "FK_BlogComment_Blog_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blog",
                        principalColumn: "BlogId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogComment_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BlogRating",
                columns: table => new
                {
                    BlogRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogRating", x => x.BlogRatingId);
                    table.ForeignKey(
                        name: "FK_BlogRating_Blog_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blog",
                        principalColumn: "BlogId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogRating_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Branch_Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PromotionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branch_Product_Branch",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_Product_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_Product_Promotion_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotion",
                        principalColumn: "PromotionId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Branch_Promotion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch_Promotion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branch_Promotion_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_Promotion_Promotion_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotion",
                        principalColumn: "PromotionId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Branch_Service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch_Service", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branch_Service_Branch",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Branch_Service_Service",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Room_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Room_ServiceCategory_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategory",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.StaffId);
                    table.ForeignKey(
                        name: "FK_Staff_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Staff_StaffRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "StaffRole",
                        principalColumn: "StaffRoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Staff_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductCart",
                columns: table => new
                {
                    ProductCartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCart", x => x.ProductCartId);
                    table.ForeignKey(
                        name: "FK_Product_Cart_Cart",
                        column: x => x.CartId,
                        principalTable: "Cart",
                        principalColumn: "CartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_Cart_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    OrderDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_OrderDetail_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shipping",
                columns: table => new
                {
                    ShippingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ShippingAddress = table.Column<string>(type: "longtext", nullable: false),
                    ShippingMethod = table.Column<string>(type: "longtext", nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    TrackingNumber = table.Column<string>(type: "longtext", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipping", x => x.ShippingId);
                    table.ForeignKey(
                        name: "FK_Shipping_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TransactionCode = table.Column<int>(type: "int", nullable: true),
                    Reference = table.Column<string>(type: "longtext", nullable: true),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: true),
                    TransactionDateTime = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaction_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoutineStep",
                columns: table => new
                {
                    UserRoutineStepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserRoutineId = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    Step = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoutineStep", x => x.UserRoutineStepId);
                    table.ForeignKey(
                        name: "FK_UserRoutineStep_UserRoutine_UserRoutineId",
                        column: x => x.UserRoutineId,
                        principalTable: "UserRoutine",
                        principalColumn: "UserRoutineId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bed",
                columns: table => new
                {
                    BedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Thumbnail = table.Column<string>(type: "longtext", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    BedTypeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bed", x => x.BedId);
                    table.ForeignKey(
                        name: "FK_Bed_BedType_BedTypeId",
                        column: x => x.BedTypeId,
                        principalTable: "BedType",
                        principalColumn: "BedTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bed_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ShiftName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedule_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedule_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Work_Schedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Work_Schedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Work_Schedule_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "ShiftId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Work_Schedule_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoutineLogger",
                columns: table => new
                {
                    UserRoutineLoggerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Step_Logger = table.Column<string>(type: "longtext", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoutineLogger", x => x.UserRoutineLoggerId);
                    table.ForeignKey(
                        name: "FK_UserRoutineLogger_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_UserRoutineLogger_UserRoutineStep_StepId",
                        column: x => x.StepId,
                        principalTable: "UserRoutineStep",
                        principalColumn: "UserRoutineStepId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoutineLogger_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: true),
                    BedId = table.Column<int>(type: "int", nullable: true),
                    AppointmentsTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: false),
                    Feedback = table.Column<string>(type: "longtext", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointments_Bed_BedId",
                        column: x => x.BedId,
                        principalTable: "Bed",
                        principalColumn: "BedId");
                    table.ForeignKey(
                        name: "FK_Appointments_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_Appointments_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId");
                    table.ForeignKey(
                        name: "FK_Appointments_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BedAvailability",
                columns: table => new
                {
                    BedAvailabilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BedId = table.Column<int>(type: "int", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BedAvailability", x => x.BedAvailabilityId);
                    table.ForeignKey(
                        name: "FK_BedAvailability_Bed_BedId",
                        column: x => x.BedId,
                        principalTable: "Bed",
                        principalColumn: "BedId");
                    table.ForeignKey(
                        name: "FK_BedAvailability_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AppointmentFeedback",
                columns: table => new
                {
                    AppointmentFeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "longtext", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    ImageBefore = table.Column<string>(type: "longtext", nullable: true),
                    ImageAfter = table.Column<string>(type: "longtext", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentFeedback", x => x.AppointmentFeedbackId);
                    table.ForeignKey(
                        name: "FK_AppointmentFeedback_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentFeedback_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentFeedback_AppointmentId",
                table: "AppointmentFeedback",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentFeedback_UserId",
                table: "AppointmentFeedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BedId",
                table: "Appointments",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BranchId",
                table: "Appointments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CustomerId",
                table: "Appointments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_OrderId",
                table: "Appointments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_RoomId",
                table: "Appointments",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffId",
                table: "Appointments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Bed_BedTypeId",
                table: "Bed",
                column: "BedTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bed_RoomId",
                table: "Bed",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_BedAvailability_BedId",
                table: "BedAvailability",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_BedAvailability_RoomId",
                table: "BedAvailability",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_AuthorId",
                table: "Blog",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComment_BlogId",
                table: "BlogComment",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogComment_CustomerId",
                table: "BlogComment",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogRating_BlogId",
                table: "BlogRating",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogRating_CustomerId",
                table: "BlogRating",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_CompanyId",
                table: "Branch",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_ManagerId",
                table: "Branch",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Product_BranchId",
                table: "Branch_Product",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Product_ProductId_BranchId",
                table: "Branch_Product",
                columns: new[] { "ProductId", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Product_PromotionId",
                table: "Branch_Product",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Promotion_BranchId",
                table: "Branch_Promotion",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Promotion_PromotionId",
                table: "Branch_Promotion",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Service_BranchId_ServiceId",
                table: "Branch_Service",
                columns: new[] { "BranchId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branch_Service_ServiceId",
                table: "Branch_Service",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_CustomerId",
                table: "Cart",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_ReceiverId",
                table: "Chat",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_SenderId",
                table: "Chat",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerId",
                table: "Order",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_VoucherId",
                table: "Order",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ProductId",
                table: "OrderDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryId",
                table: "Product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CompanyId",
                table: "Product",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCart_CartId",
                table: "ProductCart",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCart_ProductId_CartId",
                table: "ProductCart",
                columns: new[] { "ProductId", "CartId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductFeedback_ProductId",
                table: "ProductFeedback",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFeedback_UserId",
                table: "ProductFeedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRoutine_ProductId_RoutineId",
                table: "ProductRoutine",
                columns: new[] { "ProductId", "RoutineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRoutine_RoutineId",
                table: "ProductRoutine",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_Room_BranchId",
                table: "Room",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Room_ServiceCategoryId",
                table: "Room",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_BranchId",
                table: "Schedule",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_StaffId",
                table: "Schedule",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Service_ServiceCategoryId",
                table: "Service",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFeedback_ServiceId",
                table: "ServiceFeedback",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFeedback_UserId",
                table: "ServiceFeedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceImages_ServiceId",
                table: "ServiceImages",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRoutine_RoutineId",
                table: "ServiceRoutine",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRoutine_ServiceId_RoutineId",
                table: "ServiceRoutine",
                columns: new[] { "ServiceId", "RoutineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipping_OrderId",
                table: "Shipping",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SkinHealth_UserId",
                table: "SkinHealth",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_BranchId",
                table: "Staff",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_RoleId",
                table: "Staff",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_UserId",
                table: "Staff",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CustomerId",
                table: "Transaction",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_OrderId",
                table: "Transaction",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleID",
                table: "User",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutine_RoutineId",
                table: "UserRoutine",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutine_UserId_RoutineId",
                table: "UserRoutine",
                columns: new[] { "UserId", "RoutineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutineLogger_StaffId",
                table: "UserRoutineLogger",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutineLogger_StepId",
                table: "UserRoutineLogger",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutineLogger_UserId",
                table: "UserRoutineLogger",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoutineStep_UserRoutineId",
                table: "UserRoutineStep",
                column: "UserRoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_Work_Schedule_ShiftId",
                table: "Work_Schedule",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Work_Schedule_StaffId",
                table: "Work_Schedule",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentFeedback");

            migrationBuilder.DropTable(
                name: "BedAvailability");

            migrationBuilder.DropTable(
                name: "BlogComment");

            migrationBuilder.DropTable(
                name: "BlogRating");

            migrationBuilder.DropTable(
                name: "Branch_Product");

            migrationBuilder.DropTable(
                name: "Branch_Promotion");

            migrationBuilder.DropTable(
                name: "Branch_Service");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "Logger");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "ProductCart");

            migrationBuilder.DropTable(
                name: "ProductFeedback");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductRoutine");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "ServiceFeedback");

            migrationBuilder.DropTable(
                name: "ServiceImages");

            migrationBuilder.DropTable(
                name: "ServiceRoutine");

            migrationBuilder.DropTable(
                name: "Shipping");

            migrationBuilder.DropTable(
                name: "SkinHealth");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "UserRoutineLogger");

            migrationBuilder.DropTable(
                name: "Work_Schedule");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Blog");

            migrationBuilder.DropTable(
                name: "Promotion");

            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "UserRoutineStep");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "Bed");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "UserRoutine");

            migrationBuilder.DropTable(
                name: "BedType");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Voucher");

            migrationBuilder.DropTable(
                name: "StaffRole");

            migrationBuilder.DropTable(
                name: "SkincareRoutine");

            migrationBuilder.DropTable(
                name: "Branch");

            migrationBuilder.DropTable(
                name: "ServiceCategory");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserRole");
        }
    }
}
