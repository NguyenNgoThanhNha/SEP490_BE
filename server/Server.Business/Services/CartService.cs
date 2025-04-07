using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Business.Commons;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using StackExchange.Redis;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Server.Business.Ultils;

namespace Server.Business.Services
{
    public class CartService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDatabase _redisDb;
        private readonly string _cartPrefix = "cart";
        private readonly AppDbContext _context;
        private readonly ProductService _productService;
        private readonly RedisSetting _redisSetting;
        private readonly IConnectionMultiplexer _redis;

        public CartService(UnitOfWorks unitOfWorks,
            IMapper mapper,
            IHttpContextAccessor httpContext,
            IConnectionMultiplexer redis,
            AppDbContext context,
            IOptions<RedisSetting> redisSetting,
            ProductService productService)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _httpContextAccessor = httpContext;
            _redisSetting = redisSetting.Value;
            _redis = redis;
            _redisDb = redis.GetDatabase();
            _context = context;
            _productService = productService;
        }

        private string GetCartKey(int userId) => $"{_cartPrefix}{userId}";

        private async Task EnsureConnected()
        {
            if (!_redis.IsConnected)
            {
                try
                {
                    await _redis.GetDatabase().PingAsync();
                }
                catch
                {
                    throw new Exception("Cannot connect to Redis");
                }
            }
        }

        public async Task<ApiResult<ApiResponse>> GetCart(int userId)
        {
            await EnsureConnected();

            if (userId <= 0)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Vui lòng đăng nhập vào hệ thống"
                });
            }

            string cacheKey = GetCartKey(userId);
            string cachedCart = await _redisDb.StringGetAsync(cacheKey);

            List<CartDTO> cart;

            if (!string.IsNullOrEmpty(cachedCart))
            {
                cart = JsonConvert.DeserializeObject<List<CartDTO>>(cachedCart);
            }
            else
            {
                cart = await GetCartFromDatabase(userId);

                if (cart.Any())
                {
                    await _redisDb.StringSetAsync(
                        cacheKey,
                        JsonConvert.SerializeObject(cart),
                        TimeSpan.FromMinutes(60));
                }
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Lấy dữ liệu cart thành công!",
                data = cart
            });
        }

        private async Task UpdateCartCache(int userId, List<CartDTO> cart)
        {
            await _redisDb.KeyDeleteAsync(GetCartKey(userId));
            if (cart.Any())
            {
                await _redisDb.StringSetAsync(GetCartKey(userId), JsonConvert.SerializeObject(cart),
                    TimeSpan.FromMinutes(60));
            }
        }

        public async Task<ApiResult<ApiResponse>> AddToCart(AddToCartRequest request)
        {
            await EnsureConnected();

            if (request.UserId <= 0)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Vui lòng đăng nhập vào hệ thống"
                });
            }

            // 1. Kiểm tra productBranchId hợp lệ
            var productBranch = await _unitOfWorks.Brand_ProductRepository
                .FirstOrDefaultAsync(x => x.Id == request.ProductBranchId);

            if (productBranch == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Sản phẩm chi nhánh không tồn tại."
                });
            }

            // 2. Tìm Cart (nếu chưa có thì tạo mới)
            var existingCart = await _unitOfWorks.CartRepository
                .FindByCondition(c => c.CustomerId == request.UserId)
                .FirstOrDefaultAsync();

            if (existingCart == null)
            {
                existingCart = new Cart
                {
                    CustomerId = request.UserId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _unitOfWorks.CartRepository.AddAsync(existingCart);
                await _unitOfWorks.CartRepository.Commit();
            }

            // 3. Kiểm tra sản phẩm đã có trong giỏ chưa
            var cartItem = await _unitOfWorks.ProductCartRepository
                .FindByCondition(c => c.CartId == existingCart.CartId && c.ProductBranchId == productBranch.Id)
                .FirstOrDefaultAsync();

            if (cartItem == null)
            {
                if (request.Operation != Data.OperationTypeEnum.Add)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Sản phẩm chưa tồn tại trong giỏ hàng!"
                    });
                }

                // Thêm mới
                var newItem = new ProductCart
                {
                    CartId = existingCart.CartId,
                    ProductBranchId = productBranch.Id,
                    Quantity = Math.Max(1, request.Quantity),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _unitOfWorks.ProductCartRepository.AddAsync(newItem);
            }
            else
            {
                // Cập nhật số lượng theo Operation
                cartItem.Quantity = request.Operation switch
                {
                    Data.OperationTypeEnum.Add => cartItem.Quantity + request.Quantity,
                    Data.OperationTypeEnum.Subtract => Math.Max(1, cartItem.Quantity - request.Quantity),
                    Data.OperationTypeEnum.Replace => Math.Max(1, request.Quantity),
                    _ => cartItem.Quantity
                };

                cartItem.UpdatedDate = DateTime.Now;
                _unitOfWorks.ProductCartRepository.Update(cartItem);
            }

            // 4. Lưu thay đổi
            if (await _unitOfWorks.ProductCartRepository.Commit() > 0)
            {
                var cart = await GetCartFromDatabase(request.UserId);
                await UpdateCartCache(request.UserId, cart);

                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    message = "Cập nhật giỏ hàng thành công",
                    data = cart
                });
            }

            return ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = "Lỗi cập nhật giỏ hàng"
            });
        }


        //public async Task<ApiResult<ApiResponse>> DeleteProductFromCart(int productBranchId, int userId)
        //{
        //    await EnsureConnected();
        //    var customerId = userId.ToString();
        //    if (string.IsNullOrEmpty(customerId))
        //    {
        //        return ApiResult<ApiResponse>.Error(new ApiResponse
        //        {
        //            message = "Vui lòng đăng nhập vào hệ thống"
        //        });
        //    }

        //    var existingCart = await _unitOfWorks.CartRepository
        //        .FindByCondition(c => c.CustomerId == int.Parse(customerId))
        //        .FirstOrDefaultAsync();
        //    if (existingCart == null)
        //    {
        //        return ApiResult<ApiResponse>.Error(new ApiResponse
        //        {
        //            message = "Giỏ hàng không có sản phẩm"
        //        });
        //    }

        //    var detail = await _unitOfWorks.ProductCartRepository
        //        .FindByCondition(c => c.CartId == existingCart.CartId && c.ProductBranchId == productBranchId)
        //        .FirstOrDefaultAsync();
        //    if (detail == null)
        //    {
        //        return ApiResult<ApiResponse>.Error(new ApiResponse
        //        {
        //            message = "Sản phẩm không tồn tại trong giỏ hàng"
        //        });
        //    }

        //    _unitOfWorks.ProductCartRepository.Remove(detail.ProductCartId);
        //    int result = await _unitOfWorks.ProductCartRepository.Commit();
        //    if (result == 0)
        //    {
        //        return ApiResult<ApiResponse>.Error(new ApiResponse
        //        {
        //            message = "Lỗi xóa sản phẩm trong giỏ hàng"
        //        });
        //    }
        //    var cart = await GetCartFromDatabase(userId);
        //    await UpdateCartCache(int.Parse(customerId), cart);
        //    return ApiResult<ApiResponse>.Succeed(new ApiResponse
        //    {
        //        message = "Xóa sản phẩm khỏi giỏ hàng thành công",
        //        data = cart
        //    });
        //}

        public async Task<ApiResult<ApiResponse>> DeleteProductsFromCart(List<int> productBranchIds, int userId)
        {
            await EnsureConnected();
            var customerId = userId.ToString();
            if (string.IsNullOrEmpty(customerId))
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Vui lòng đăng nhập vào hệ thống"
                });
            }

            var existingCart = await _unitOfWorks.CartRepository
                .FindByCondition(c => c.CustomerId == int.Parse(customerId))
                .FirstOrDefaultAsync();
            if (existingCart == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Giỏ hàng không có sản phẩm"
                });
            }

            var detailsToRemove = await _unitOfWorks.ProductCartRepository
                .FindByCondition(c => c.CartId == existingCart.CartId && productBranchIds.Contains(c.ProductBranchId))
                .ToListAsync();

            if (detailsToRemove == null || !detailsToRemove.Any())
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Không tìm thấy sản phẩm cần xóa trong giỏ hàng"
                });
            }

            foreach (var item in detailsToRemove)
            {
                _unitOfWorks.ProductCartRepository.Remove(item.ProductCartId);
            }

            int result = await _unitOfWorks.ProductCartRepository.Commit();
            if (result == 0)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Lỗi xóa sản phẩm trong giỏ hàng"
                });
            }

            var cart = await GetCartFromDatabase(userId);
            await UpdateCartCache(int.Parse(customerId), cart);

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                message = "Xóa sản phẩm khỏi giỏ hàng thành công",
                data = cart
            });
        }




        private async Task<List<CartDTO>> GetCartFromDatabase(int userId)
        {
            var cartEntities = await _context.ProductCart
                .Include(c => c.Cart)
                .Include(c => c.ProductBranch)
                    .ThenInclude(pb => pb.Product)
                        .ThenInclude(p => p.Category)
                .Where(c => c.Cart.CustomerId == userId)
                .ToListAsync();

            var cartItems = cartEntities.Select(c => new CartDTO
            {
                ProductCartId = c.ProductCartId,
                CartId = c.CartId,
                Quantity = c.Quantity, // ✅ Số lượng user thêm vào cart

                Product = new ProductDetailInCartDto
                {
                    ProductId = c.ProductBranch.Product.ProductId,
                    ProductName = c.ProductBranch.Product.ProductName,
                    ProductDescription = c.ProductBranch.Product.ProductDescription,
                    Price = c.ProductBranch.Product.Price,
                    Quantity = c.ProductBranch.Product.Quantity,
                    Discount = c.ProductBranch.Product.Discount ?? 0,
                    CategoryId = c.ProductBranch.Product.CategoryId,
                    CompanyId = c.ProductBranch.Product.CompanyId,
                    Dimension = c.ProductBranch.Product.Dimension,
                    Volume = c.ProductBranch.Product.Volume,
                    Status = c.ProductBranch.Product.Status,
                    Brand = c.ProductBranch.Product.Brand,
                    SkinTypeSuitable = c.ProductBranch.Product.SkinTypeSuitable,
                    CreatedDate = c.ProductBranch.Product.CreatedDate,
                    UpdatedDate = c.ProductBranch.Product.UpdatedDate,
                    BrandId = c.ProductBranch.BranchId,
                    ProductBranchId = c.ProductBranch.Id,
                    StockQuantity = c.ProductBranch.StockQuantity, // ✅ lấy trực tiếp tồn kho từ branch_product
                    Category = c.ProductBranch.Product.Category == null ? null : new CategoryDetailInCartDto
                    {
                        CategoryId = c.ProductBranch.Product.Category.CategoryId,
                        Name = c.ProductBranch.Product.Category.Name,
                        Description = c.ProductBranch.Product.Category.Description,
                        Status = c.ProductBranch.Product.Category.Status,
                        ImageUrl = c.ProductBranch.Product.Category.ImageUrl,
                        CreatedDate = c.ProductBranch.Product.Category.CreatedDate,
                        UpdatedDate = c.ProductBranch.Product.Category.UpdatedDate
                    }
                }
            }).ToList();

            // Load hình ảnh sản phẩm
            var productIds = cartItems.Select(x => x.Product.ProductId).Distinct().ToList();
            var productImages = await _productService.GetListImagesOfProductIds(productIds);

            foreach (var item in cartItems)
            {
                var matched = productImages.FirstOrDefault(p => p.ProductId == item.Product.ProductId);
                if (matched != null)
                {
                    item.Product.Images = matched.images?.ToList() ?? new List<string>();
                }
            }

            return cartItems;
        }




    }
}