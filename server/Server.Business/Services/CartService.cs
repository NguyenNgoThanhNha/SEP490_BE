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
                return ApiResult<ApiResponse>.Error(new ApiResponse { message = "Vui lòng đăng nhập vào hệ thống" });

            string cacheKey = GetCartKey(userId);
            string cachedCart = await _redisDb.StringGetAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedCart))
            {
                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                    { data = JsonConvert.DeserializeObject<List<CartDTO>>(cachedCart) });
            }

            var cart = await GetCartFromDatabase(userId);
            if (cart.Any())
            {
                await _redisDb.StringSetAsync(cacheKey, JsonConvert.SerializeObject(cart), TimeSpan.FromMinutes(60));
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse { data = cart });
        }

        public async Task<ApiResult<ApiResponse>> AddToCart(AddToCartRequest request)
        {
            await EnsureConnected();
            if (request.UserId <= 0)
                return ApiResult<ApiResponse>.Error(new ApiResponse { message = "Vui lòng đăng nhập vào hệ thống" });

            var productBranch = await _unitOfWorks.Brand_ProductRepository
                .FirstOrDefaultAsync(x => x.Id == request.ProductBranchId);
            
            if (await _unitOfWorks.ProductRepository.GetByIdAsync(productBranch.ProductId) == null)
                return ApiResult<ApiResponse>.Error(new ApiResponse
                    { message = "Sản phẩm không tồn tại trong hệ thống." });

            var existingCart = await _unitOfWorks.CartRepository.FindByCondition(c => c.CustomerId == request.UserId)
                .FirstOrDefaultAsync();
            if (existingCart == null)
            {
                existingCart = new Cart
                    { CustomerId = request.UserId, CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now };
                await _unitOfWorks.CartRepository.AddAsync(existingCart);
                await _unitOfWorks.CartRepository.Commit();
            }

            var detail = await _unitOfWorks.ProductCartRepository
                .FindByCondition(c => c.CartId == existingCart.CartId && c.ProductId == productBranch.ProductId)
                .FirstOrDefaultAsync();
            if (detail == null)
            {
                if (request.Operation != Data.OperationTypeEnum.Add)
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                        { message = "Sản phẩm chưa tồn tại trong giỏ hàng!" });

                await _unitOfWorks.ProductCartRepository.AddAsync(new ProductCart
                    { CartId = existingCart.CartId, ProductId = productBranch.ProductId, Quantity = 1 });
            }
            else
            {
                detail.Quantity = request.Operation switch
                {
                    Data.OperationTypeEnum.Add => detail.Quantity + request.Quantity,
                    Data.OperationTypeEnum.Subtract => Math.Max(1, detail.Quantity - request.Quantity),
                    Data.OperationTypeEnum.Replace => Math.Max(1, request.Quantity),
                    _ => detail.Quantity
                };
                _unitOfWorks.ProductCartRepository.Update(detail);
            }

            if (await _unitOfWorks.ProductCartRepository.Commit() > 0)
            {
                var cart = await GetCartFromDatabase(request.UserId);
                await UpdateCartCache(request.UserId, cart);
                return ApiResult<ApiResponse>.Succeed(new ApiResponse { data = cart });
            }

            return ApiResult<ApiResponse>.Error(new ApiResponse { message = "Lỗi cập nhật vào hệ thống" });
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
        
        public async Task<ApiResult<ApiResponse>> DeleteProductFromCart(int productId, int userId)
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

            var detail = await _unitOfWorks.ProductCartRepository
                .FindByCondition(c => c.CartId == existingCart.CartId && c.ProductId == productId)
                .FirstOrDefaultAsync();
            if (detail == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Sản phẩm không tồn tại trong giỏ hàng"
                });
            }

            _unitOfWorks.ProductCartRepository.Remove(detail.ProductCartId);
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
                data = true
            });
        }

        private async Task<List<CartDTO>> GetCartFromDatabase(int userId)
        {
            var cartItems = await _context.ProductCart
                .Include(c => c.Cart)
                .Include(c => c.Product)
                .ThenInclude(p => p.Branch_Products)
                .Include(c => c.Product)
                .ThenInclude(p => p.Category)
                .Include(c => c.Product)
                .Where(c => c.Cart.CustomerId == userId)
                .Select(c => new CartDTO
                {
                    CartId = c.CartId,
                    ProductCartId = c.ProductCartId,
                    ProductId = c.ProductId,
                    ProductName = c.Product.ProductName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    StockQuantity = c.Product.Branch_Products.Sum(bp => bp.StockQuantity),
                    Product = _mapper.Map<ProductDetailDto>(c.Product)
                })
                .ToListAsync();

            // Lấy danh sách productId từ giỏ hàng
            var productIds = cartItems.Select(x => x.ProductId).ToList();

            // Gọi service để lấy danh sách hình ảnh sản phẩm
            var listProductModel = await _productService.GetListImagesOfProductIds(productIds);

            // Gán hình ảnh vào sản phẩm trong giỏ hàng
            foreach (var cartItem in cartItems)
            {
                var productWithImages = listProductModel.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                if (productWithImages != null)
                {
                    cartItem.Product.images = productWithImages.images;
                }
            }

            return cartItems;
        }

    }
}