using AutoMapper;
using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Business.Models;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class RoutineService
{
    private readonly UnitOfWorks _unitOfWorks;
    private readonly IMapper _mapper;
    private readonly ProductService _productService;
    private readonly ServiceService _serviceService;

    public RoutineService(UnitOfWorks unitOfWorks, IMapper mapper, ProductService productService, ServiceService serviceService)
    {
        _unitOfWorks = unitOfWorks;
        _mapper = mapper;
        _productService = productService;
        _serviceService = serviceService;
    }
    
    public async Task<SkincareRoutineModel> GetSkincareRoutineDetails(int id)
    {
        var routine =  await _unitOfWorks.SkincareRoutineRepository.FindByCondition(x => x.SkincareRoutineId == id)
            .Include(x => x.UserRoutines)
            .Include(x => x.ProductRoutines)
            .ThenInclude(x => x.Products)
            .ThenInclude(x => x.Category)
            .Include(x => x.ServiceRoutines)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .FirstOrDefaultAsync();
        if (routine == null) return null;

        var routineModel = _mapper.Map<SkincareRoutineModel>(routine);

        // get images of product
        var productRoutines = routine.ProductRoutines;
        var listProduct = new List<Product>();
        foreach (var productRoutine in productRoutines)
        {
            listProduct.Add(productRoutine.Products);
        }

        var listProductModel = await _productService.GetListImagesOfProduct(listProduct);
        
        // get image of service
        var serviceRoutines = routine.ServiceRoutines;
        var listService = new List<Data.Entities.Service>();
        foreach (var serviceRoutine in serviceRoutines)
        {
            listService.Add(serviceRoutine.Service);
        }

        var listServiceModel = await _serviceService.GetListImagesOfServices(listService);
        
        
        // map images
        foreach (var serviceRoutine in routineModel.ServiceRoutines)
        {
            foreach (var service in listServiceModel)
            {
                if (serviceRoutine.Service.ServiceId == service.ServiceId)
                {
                    serviceRoutine.Service.images = service.images;
                }
            }   
        }

        foreach (var productRoutine in routineModel.ProductRoutines)
        {
            foreach (var product in listProductModel)
            {
                if (productRoutine.Products.ProductId == product.ProductId)
                {
                    productRoutine.Products.images = product.images;
                }
            }
        }
        
        return routineModel;
    }
    
    public async Task<List<SkincareRoutineModel>> GetListSkincareRoutine()
    {
        var routines = _unitOfWorks.SkincareRoutineRepository.GetAll();
        var routineModels = _mapper.Map<List<SkincareRoutineModel>>(routines);
        return routineModels;
    }
    
    public async Task<List<SkinCareRoutineStepModel>> GetListSkincareRoutineStepByRoutineId(int routineId)
    {
        var steps = await _unitOfWorks.SkinCareRoutineStepRepository
            .FindByCondition(x => x.SkincareRoutineId == routineId)
            .Include(x => x.ServiceRoutineSteps)
            .ThenInclude(x => x.Service)
            .ThenInclude(x => x.ServiceCategory)
            .Include(x => x.ProductRoutineSteps)
            .ThenInclude(x => x.Product)
            .ThenInclude(x => x.Category)
            .ToListAsync();

        var stepModels = _mapper.Map<List<SkinCareRoutineStepModel>>(steps);

        // Lấy danh sách sản phẩm từ stepModels
        var productRoutineSteps = stepModels.SelectMany(x => x.ProductRoutineSteps).ToList();
        var listProduct = productRoutineSteps.Select(pr => pr.Product).ToList();
        var listProductModel = await _productService.GetListImagesOfProduct(_mapper.Map<List<Product>>(listProduct));

        // Lấy danh sách dịch vụ từ stepModels
        var serviceRoutineSteps = stepModels.SelectMany(x => x.ServiceRoutineSteps).ToList();
        var listService = serviceRoutineSteps.Select(sr => sr.Service).ToList();
        var listServiceModel = await _serviceService.GetListImagesOfServices(_mapper.Map<List<Data.Entities.Service>>(listService));

        // Map images của dịch vụ
        foreach (var serviceRoutine in serviceRoutineSteps)
        {
            var serviceImage = listServiceModel.FirstOrDefault(s => s.ServiceId == serviceRoutine.Service.ServiceId);
            if (serviceImage != null)
            {
                serviceRoutine.Service.images = serviceImage.images;
            }
        }

        // Map images của sản phẩm
        foreach (var productRoutineStep in productRoutineSteps)
        {
            var productImage = listProductModel.FirstOrDefault(p => p.ProductId == productRoutineStep.Product.ProductId);
            if (productImage != null)
            {
                productRoutineStep.Product.images = productImage.images;
            }
        }

        return stepModels;
    }

}