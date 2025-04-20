using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services
{
    public class OrderDetailService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;
        private readonly ProductService _productService;

        public OrderDetailService(UnitOfWorks unitOfWorks, IMapper mapper, ProductService productService)
        {
            this._unitOfWorks = unitOfWorks;
            _mapper = mapper;
            _productService = productService;
        }

        public async Task<ApiResult<OrderDetail>> CreateOrderDetailAsync(CUOrderDetailDto model)
        {
            // Kiểm tra sự tồn tại của Order
            var orderExists = await _unitOfWorks.OrderRepository
      .FindByCondition(x => x.OrderId == model.OrderId)
      .AnyAsync();

            if (!orderExists)
            {
                return ApiResult<OrderDetail>.Error(null, "Order not found");
            }

            // Kiểm tra sự tồn tại của Product với trạng thái Active
            var productExists = await _unitOfWorks.ProductRepository
     .FindByCondition(x => x.ProductId == model.ProductId && x.Status == "Active")
     .AnyAsync();
            if (!productExists)
            {
                return ApiResult<OrderDetail>.Error(null, "Product not found");
            }

            // Kiểm tra sự tồn tại của Service với trạng thái Active
            var serviceExists = await _unitOfWorks.ServiceRepository
    .FindByCondition(x => x.ServiceId == model.ServiceId && x.Status == "Active")
    .AnyAsync();

            if (!serviceExists)
            {
                return ApiResult<OrderDetail>.Error(null, "Service not found");
            }

            // Tạo OrderDetail từ DTO
            var orderDetail = _mapper.Map<OrderDetail>(model);

            try
            {
                // Thêm vào cơ sở dữ liệu qua UnitOfWork
                await _unitOfWorks.OrderDetailRepository.AddAsync(orderDetail);
                await _unitOfWorks.OrderDetailRepository.Commit();

                // Lấy lại thông tin với các bảng liên kết
                var orderDetailWithIncludes = await _unitOfWorks.OrderDetailRepository
    .FindByCondition(od => od.OrderDetailId == orderDetail.OrderDetailId)
    .Include(od => od.Order)
    .FirstOrDefaultAsync();


                if (orderDetailWithIncludes == null)
                {
                    return ApiResult<OrderDetail>.Error(null, "Failed to retrieve the created order detail.");
                }

                // Map sang DTO để trả về thông tin
                var orderDetailDto = _mapper.Map<OrderDetail>(orderDetailWithIncludes);
                return ApiResult<OrderDetail>.Succeed(orderDetailDto);
            }
            catch (Exception ex)
            {
                return ApiResult<OrderDetail>.Error(null, ex.Message);
            }
        }

        public async Task<List<OrderDetailModels>> GetOrderDetailByBranchId(int branchId)
        {
            var orderDetails = await _unitOfWorks.OrderDetailRepository
                .FindByCondition(x => x.BranchId == branchId)
                .Include(x => x.Product)
                .ThenInclude(x => x.ProductImages)
                .Include(x => x.Product)
                .ThenInclude(x => x.Category)
                .Include(x => x.Promotion)
                .Include(x => x.Order)
                .ToListAsync();
            
            var listProduct = new List<Product>();
            var listProductModels = new List<ProductModel>();
            listProduct = orderDetails.Select(od => od.Product).ToList();
            listProductModels = await _productService.GetListImagesOfProduct(listProduct);
            var orderDetailModels = _mapper.Map<List<OrderDetailModels>>(orderDetails);
            
            foreach (var orderDetail in orderDetailModels)
            {
                var matchedProduct = listProductModels.FirstOrDefault(p => p.ProductId == orderDetail.Product.ProductId);
                if (matchedProduct != null)
                {
                    orderDetail.Product.images = matchedProduct.images;
                    orderDetail.Product.Branch = matchedProduct.Branch;
                }
            }
            return _mapper.Map<List<OrderDetailModels>>(orderDetails);
        }

        public async Task<GetAllOrderDetailPaginationResponse> GetOrderDetailsByBranchIdAsync(int branchId, int page = 1, int pageSize = 5)
        {
            var query = _unitOfWorks.OrderDetailRepository
                .FindByCondition(od => od.BranchId == branchId)
                .Include(od => od.Product)
                .Include(od => od.Branch)
                .Include(od => od.Order)
                .Include(od => od.Promotion); 

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedOrderDetails = await query
                .OrderByDescending(od => od.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<OrderDetailModels>>(pagedOrderDetails);

            return new GetAllOrderDetailPaginationResponse
            {
                data = mapped,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }


    }
}
