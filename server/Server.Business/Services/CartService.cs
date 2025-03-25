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
        private readonly IConnectionMultiplexer _redis;

        public CartService(UnitOfWorks unitOfWorks,
            IMapper mapper,
            IHttpContextAccessor httpContext,
            IConnectionMultiplexer redis,
            AppDbContext context)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _httpContextAccessor = httpContext;
            _redis = redis;
            _redisDb = redis.GetDatabase();
            _context = context;
        }

        private string GetCartKey(string userId) => $"{_cartPrefix}{userId}";

        private bool IsConnected()
        {
            return _redis.IsConnected;
        }

        public async Task<ApiResult<ApiResponse>> GetCart(int userId)
        {
            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Vui lòng đăng nhập vào hệ thống"
                });
            }

            string cacheKey = GetCartKey(userId.ToString());

            if (IsConnected())
            {
                string cachedCart = await _redisDb.StringGetAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedCart))
                {
                    return ApiResult<ApiResponse>.Succeed(new ApiResponse
                    {
                        data = JsonConvert.DeserializeObject<List<CartDTO>>(cachedCart)
                    });
                }
            }

            // Lấy danh sách sản phẩm trong giỏ hàng
            var cart = await GetCartFromDatabase(userId);

            if (cart.Any())
            {
                // Lấy danh sách ProductId từ giỏ hàng
                var productIds = cart.Select(c => c.ProductId).ToList();

                // Lấy hình ảnh của các sản phẩm trong giỏ hàng
                var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
                    .Where(si => productIds.Contains(si.ProductId))
                    .GroupBy(si => si.ProductId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(si => si.image).ToArray());

                // Gán danh sách hình ảnh vào sản phẩm trong giỏ hàng
                foreach (var item in cart)
                {
                    item.Product.images = productImages.ContainsKey(item.ProductId)
                        ? productImages[item.ProductId]
                        : Array.Empty<string>(); // Nếu không có hình ảnh, gán mảng rỗng
                }

                if (IsConnected())
                {
                    await _redisDb.StringSetAsync(cacheKey, JsonConvert.SerializeObject(cart),
                        TimeSpan.FromMinutes(60));
                }
            }

            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                data = cart
            });
        }


        public async Task<ApiResult<ApiResponse>> AddToCart(CartRequest request)
        {
            var customerId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Vui lòng đăng nhập vào hệ thống"
                });
            }

            int customerIdInt = int.Parse(customerId);

            if (await _unitOfWorks.ProductRepository.GetByIdAsync(request.ProductId) == null)
            {
                return ApiResult<ApiResponse>.Error(new ApiResponse
                {
                    message = "Sản phẩm không tồn tại trong hệ thống."
                });
            }

            var existingCart = await _unitOfWorks.CartRepository
                .FindByCondition(c => c.CustomerId == customerIdInt)
                .FirstOrDefaultAsync();

            if (existingCart == null)
            {
                existingCart = new Cart
                {
                    CustomerId = customerIdInt,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWorks.CartRepository.AddAsync(existingCart);
                await _unitOfWorks.CartRepository.Commit();
            }

            var detail = await _unitOfWorks.ProductCartRepository
                .FindByCondition(c => c.CartId == existingCart.CartId && c.ProductId == request.ProductId)
                .FirstOrDefaultAsync();

            if (detail == null)
            {
                if (request.Operation != Data.OperationTypeEnum.Add)
                {
                    return ApiResult<ApiResponse>.Error(new ApiResponse
                    {
                        message = "Sản phẩm chưa tồn tại trong giỏ hàng!"
                    });
                }

                var productCart = new ProductCart
                {
                    CartId = existingCart.CartId,
                    ProductId = request.ProductId,
                    Quantity = 1
                };

                await _unitOfWorks.ProductCartRepository.AddAsync(productCart);
            }
            else
            {
                if (request.Operation == Data.OperationTypeEnum.Add)
                {
                    detail.Quantity += request.Quantity;
                }
                else if (request.Operation == Data.OperationTypeEnum.Subtract)
                {
                    detail.Quantity = Math.Max(1, detail.Quantity - request.Quantity);
                }
                else if (request.Operation == Data.OperationTypeEnum.Replace)
                {
                    detail.Quantity = Math.Max(1, request.Quantity);
                }

                _unitOfWorks.ProductCartRepository.Update(detail);
            }

            var result = await _unitOfWorks.ProductCartRepository.Commit();
            if (result > 0)
            {
                var cart = await GetCartFromDatabase(customerIdInt); // Lấy dữ liệu giỏ hàng từ DB
                await UpdateCartCache(customerIdInt, cart); // Cập nhật Redis với đúng format

                return ApiResult<ApiResponse>.Succeed(new ApiResponse
                {
                    data = cart
                });
            }

            return ApiResult<ApiResponse>.Error(new ApiResponse
            {
                message = "Lỗi cập nhật vào hệ thống"
            });
        }


        public async Task<ApiResult<ApiResponse>> DeleteProductFromCart(int productId)
        {
            var customerId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

            await CacheCart(int.Parse(customerId));
            return ApiResult<ApiResponse>.Succeed(new ApiResponse
            {
                data = true
            });
        }

        private async Task CacheCart(int userId)
        {
            if (!IsConnected())
            {
                return;
            }

            await _redisDb.KeyDeleteAsync(GetCartKey(userId.ToString()));

            var cart = await _context.ProductCart
                .Include(c => c.Cart)
                .Include(c => c.Product)
                .ThenInclude(x => x.Category)
                .Where(c => c.Cart.CustomerId == userId)
                .Select(c => new CartDTO
                {
                    CartId = c.CartId,
                    ProductCartId = c.ProductCartId,
                    ProductId = c.ProductId,
                    ProductName = c.Product.ProductName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    Product = _mapper.Map<ProductDetailDto>(c.Product),
                })
                .ToListAsync();

            if (cart != null && cart.Count > 0)
            {
                await _redisDb.StringSetAsync(GetCartKey(userId.ToString()), JsonConvert.SerializeObject(cart),
                    TimeSpan.FromMinutes(60));
            }
        }

        private async Task<List<CartDTO>> GetCartFromDatabase(int customerId)
        {
            var cart = await _context.ProductCart
                .Include(c => c.Cart)
                .Include(c => c.Product)
                .ThenInclude(x => x.Category)
                .Where(c => c.Cart.CustomerId == customerId)
                .Select(c => new CartDTO
                {
                    CartId = c.CartId,
                    ProductCartId = c.ProductCartId,
                    ProductId = c.ProductId,
                    ProductName = c.Product.ProductName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    Product = _mapper.Map<ProductDetailDto>(c.Product),
                })
                .ToListAsync();

            if (cart.Any())
            {
                var productIds = cart.Select(c => c.ProductId).ToList();

                var productImages = await _unitOfWorks.ProductImageRepository.GetAll()
                    .Where(si => productIds.Contains(si.ProductId))
                    .GroupBy(si => si.ProductId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(si => si.image).ToArray());

                foreach (var item in cart)
                {
                    item.Product.images = productImages.ContainsKey(item.ProductId)
                        ? productImages[item.ProductId]
                        : Array.Empty<string>();
                }
            }

            return cart;
        }


        private async Task UpdateCartCache(int customerId, List<CartDTO> cart)
        {
            string cacheKey = GetCartKey(customerId.ToString());

            if (IsConnected() && cart.Any())
            {
                await _redisDb.StringSetAsync(cacheKey, JsonConvert.SerializeObject(cart), TimeSpan.FromMinutes(60));
            }
        }
    }
}