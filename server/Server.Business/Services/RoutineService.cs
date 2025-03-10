using AutoMapper;
using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
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
}