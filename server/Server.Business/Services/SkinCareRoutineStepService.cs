using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Response;
using Server.Business.Dtos;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Business.Exceptions;
using Server.Business.Models;
using Server.Data;

namespace Server.Business.Services
{
    public class SkinCareRoutineStepService
    {
        private readonly UnitOfWorks _unitOfWorks;
        private readonly IMapper _mapper;

        public SkinCareRoutineStepService(UnitOfWorks unitOfWorks, IMapper mapper)
        {
            _unitOfWorks = unitOfWorks;
            _mapper = mapper;
        }

        public async Task<GetAllSkinCareRoutineStepPaginationResponse> GetAllPaginationAsync(int page, int pageSize)
        {
            var query = _unitOfWorks.SkinCareRoutineStepRepository
                .GetAll()
                .OrderByDescending(x => x.SkincareRoutineId)
                .ThenBy(x => x.Step);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = _mapper.Map<List<SkinCareRoutineStepDto>>(items);

            return new GetAllSkinCareRoutineStepPaginationResponse
            {
                message = "Lấy danh sách bước skincare thành công",
                data = result,
                pagination = new Pagination
                {
                    page = page,
                    totalPage = totalPages,
                    totalCount = totalCount
                }
            };
        }


        public async Task<SkinCareRoutineStepDetailDto> GetDetailAsync(int stepId)
        {
            var step = await _unitOfWorks.SkinCareRoutineStepRepository.GetAll()
                .Include(x => x.ProductRoutineSteps)
                .Include(x => x.ServiceRoutineSteps)
                .FirstOrDefaultAsync(x => x.SkinCareRoutineStepId == stepId);

            if (step == null)
                throw new NotFoundException("Bước skincare không tồn tại.");

            // Lấy danh sách sản phẩm
            var productIds = step.ProductRoutineSteps.Select(x => x.ProductId).ToList();
            var products = await _unitOfWorks.ProductRepository
                .GetAll()
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            // Lấy danh sách dịch vụ
            var serviceIds = step.ServiceRoutineSteps.Select(x => x.ServiceId).ToList();
            var services = await _unitOfWorks.ServiceRepository
                .GetAll()
                .Where(s => serviceIds.Contains(s.ServiceId))
                .ToListAsync();

            var dto = new SkinCareRoutineStepDetailDto
            {
                SkinCareRoutineStepId = step.SkinCareRoutineStepId,
                Name = step.Name,
                Step = step.Step,
                IntervalBeforeNextStep = step.IntervalBeforeNextStep,
                Products = _mapper.Map<List<ProductModel>>(products),
                Services = _mapper.Map<List<ServiceModel>>(services)
            };

            return dto;
        }


        public async Task<SkinCareRoutineStepDto> CreateAsync(CreateSkinCareRoutineStepDto dto)
        {
            var skincareRoutine = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(dto.SkincareRoutineId);
            if (skincareRoutine == null)
                throw new BadRequestException("Gói liệu trình không tồn tại.");

            if (dto.Step > skincareRoutine.TotalSteps)
            {
                throw new BadRequestException("Số thứ tự bước skincare không được lớn hơn tổng số bước của gói liệu trình.");
            }
            
            var existingStep = await _unitOfWorks.SkinCareRoutineStepRepository
                .GetAll()
                .FirstOrDefaultAsync(x => x.SkincareRoutineId == dto.SkincareRoutineId && x.Step == dto.Step);
            if (existingStep != null)
                throw new BadRequestException("Bước skincare với số thứ tự này đã tồn tại trong gói liệu trình.");

            var entity = new SkinCareRoutineStep()
            {
                Name = dto.Name,
                SkincareRoutineId = skincareRoutine.SkincareRoutineId,
                Step = dto.Step,
                Description = dto.Description,
                IntervalBeforeNextStep = dto.IntervalBeforeNextStep,
                CreatedDate = DateTime.Now,
            };
            var createdObject = await _unitOfWorks.SkinCareRoutineStepRepository.AddAsync(entity);
            var result = await _unitOfWorks.SkincareRoutineRepository.Commit();
            if (result > 0)
            {
                // Add product to routine
                var listProductSteps = new List<ProductRoutineStep>();
                if (dto.ProductIds != null && dto.ProductIds.Count > 0)
                {
                    foreach (var productId in dto.ProductIds)
                    {
                        // check product in routine
                        var productRoutine = await _unitOfWorks.ProductRoutineRepository
                            .FirstOrDefaultAsync(x => x.ProductId == productId && x.RoutineId == skincareRoutine.SkincareRoutineId);
                        if (productRoutine != null)
                        {
                            continue;
                        }
                        else
                        {
                            var productRoutineEntity = new ProductRoutine()
                            {
                                ProductId = productId,
                                RoutineId = skincareRoutine.SkincareRoutineId,
                                Status = ObjectStatus.Active.ToString(),
                                CreatedDate = DateTime.Now,
                            };
                            await _unitOfWorks.ProductRoutineRepository.AddAsync(productRoutineEntity);   
                            await _unitOfWorks.ProductRoutineRepository.Commit();
                        }
                        
                        // check product in step
                        var productStep = await _unitOfWorks.ProductRoutineStepRepository
                            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StepId == createdObject.SkinCareRoutineStepId);
                        if (productStep != null)
                        {
                            continue;
                        }
                        else
                        {
                            var productStepEntity = new ProductRoutineStep()
                            {
                                ProductId = productId,
                                StepId = createdObject.SkinCareRoutineStepId,
                                CreatedDate = DateTime.Now,
                            };
                            listProductSteps.Add(productStepEntity);
                        }
                    }
                }
                await _unitOfWorks.ProductRoutineStepRepository.AddRangeAsync(listProductSteps);
                
                // Add service to routine
                var listServiceSteps = new List<ServiceRoutineStep>();
                if (dto.ServiceIds != null && dto.ServiceIds.Count > 0)
                {
                    foreach (var serviceId in dto.ServiceIds)
                    {
                        // check service in routine
                        var serviceRoutine = await _unitOfWorks.ServiceRoutineRepository
                            .FirstOrDefaultAsync(x => x.ServiceId == serviceId && x.RoutineId == skincareRoutine.SkincareRoutineId);
                        if (serviceRoutine != null)
                        {
                            continue;
                        }
                        else
                        {
                            var serviceRoutineEntity = new ServiceRoutine()
                            {
                                ServiceId = serviceId,
                                RoutineId = skincareRoutine.SkincareRoutineId,
                                Status = ObjectStatus.Active.ToString(),
                                CreatedDate = DateTime.Now,
                            };
                            await _unitOfWorks.ServiceRoutineRepository.AddAsync(serviceRoutineEntity);
                            await _unitOfWorks.ServiceRoutineRepository.Commit();
                        }

                        // check service in step
                        var serviceStep = await _unitOfWorks.ServiceRoutineStepRepository
                            .FirstOrDefaultAsync(x => x.ServiceId == serviceId && x.StepId == createdObject.SkinCareRoutineStepId);
                        if (serviceStep != null)
                        {
                            continue;
                        }
                        else
                        {
                            var serviceStepEntity = new ServiceRoutineStep()
                            {
                                ServiceId = serviceId,
                                StepId = createdObject.SkinCareRoutineStepId,
                                CreatedDate = DateTime.Now,
                            };
                            listServiceSteps.Add(serviceStepEntity);
                        }
                    }
                }
                await _unitOfWorks.ServiceRoutineStepRepository.AddRangeAsync(listServiceSteps);
            }
            else
            {
                throw new BadRequestException("Có lỗi xảy ra khi tạo bước skincare.");
            }
            return _mapper.Map<SkinCareRoutineStepDto>(createdObject);
        }

        
        public async Task<SkinCareRoutineStepDto> UpdateAsync(int stepId, UpdateSkinCareRoutineStepDto dto)
        {
            var step = await _unitOfWorks.SkinCareRoutineStepRepository.GetByIdAsync(stepId);
            if (step == null)
                throw new NotFoundException("Bước skincare không tồn tại.");

            var skincareRoutine = await _unitOfWorks.SkincareRoutineRepository.GetByIdAsync(step.SkincareRoutineId);
            if (skincareRoutine == null)
                throw new NotFoundException("Gói liệu trình không tồn tại.");

            var duplicateStep = await _unitOfWorks.SkinCareRoutineStepRepository
                .GetAll()
                .FirstOrDefaultAsync(x => x.SkincareRoutineId == skincareRoutine.SkincareRoutineId && x.Step == dto.Step && x.SkinCareRoutineStepId != stepId);
            if (duplicateStep != null)
                throw new BadRequestException("Bước skincare với số thứ tự này đã tồn tại trong gói liệu trình.");

            // Cập nhật thông tin cơ bản
            step.Name = dto.Name;
            step.Step = dto.Step;
            step.IntervalBeforeNextStep = dto.IntervalBeforeNextStep;
            step.UpdatedDate = DateTime.Now;

            _unitOfWorks.SkinCareRoutineStepRepository.Update(step);
            await _unitOfWorks.SkincareRoutineRepository.Commit();

            // Cập nhật Products
            var existingProductSteps = await _unitOfWorks.ProductRoutineStepRepository
                .GetAll()
                .Where(x => x.StepId == stepId)
                .ToListAsync();
            var newProductIds = dto.ProductIds ?? new List<int>();

            var toRemoveProducts = existingProductSteps.Where(x => !newProductIds.Contains(x.ProductId)).ToList();
            var toAddProductIds = newProductIds.Where(id => !existingProductSteps.Any(x => x.ProductId == id)).ToList();

            await _unitOfWorks.ProductRoutineStepRepository.RemoveRangeAsync(toRemoveProducts);

            var newProductSteps = new List<ProductRoutineStep>();
            foreach (var productId in toAddProductIds)
            {
                // Đảm bảo có trong routine
                var productRoutine = await _unitOfWorks.ProductRoutineRepository
                    .FirstOrDefaultAsync(x => x.ProductId == productId && x.RoutineId == skincareRoutine.SkincareRoutineId);
                if (productRoutine == null)
                {
                    var productRoutineEntity = new ProductRoutine()
                    {
                        ProductId = productId,
                        RoutineId = skincareRoutine.SkincareRoutineId,
                        Status = ObjectStatus.Active.ToString(),
                        CreatedDate = DateTime.Now,
                    };
                    await _unitOfWorks.ProductRoutineRepository.AddAsync(productRoutineEntity);
                    await _unitOfWorks.ProductRoutineRepository.Commit();
                }

                newProductSteps.Add(new ProductRoutineStep
                {
                    ProductId = productId,
                    StepId = stepId,
                    CreatedDate = DateTime.Now,
                });
            }
            await _unitOfWorks.ProductRoutineStepRepository.AddRangeAsync(newProductSteps);

            // Cập nhật Services
            var existingServiceSteps = await _unitOfWorks.ServiceRoutineStepRepository
                .GetAll()
                .Where(x => x.StepId == stepId)
                .ToListAsync();
            var newServiceIds = dto.ServiceIds ?? new List<int>();

            var toRemoveServices = existingServiceSteps.Where(x => !newServiceIds.Contains(x.ServiceId)).ToList();
            var toAddServiceIds = newServiceIds.Where(id => !existingServiceSteps.Any(x => x.ServiceId == id)).ToList();

            await _unitOfWorks.ServiceRoutineStepRepository.RemoveRangeAsync(toRemoveServices);

            var newServiceSteps = new List<ServiceRoutineStep>();
            foreach (var serviceId in toAddServiceIds)
            {
                var serviceRoutine = await _unitOfWorks.ServiceRoutineRepository
                    .FirstOrDefaultAsync(x => x.ServiceId == serviceId && x.RoutineId == skincareRoutine.SkincareRoutineId);
                if (serviceRoutine == null)
                {
                    var serviceRoutineEntity = new ServiceRoutine()
                    {
                        ServiceId = serviceId,
                        RoutineId = skincareRoutine.SkincareRoutineId,
                        Status = ObjectStatus.Active.ToString(),
                        CreatedDate = DateTime.Now,
                    };
                    await _unitOfWorks.ServiceRoutineRepository.AddAsync(serviceRoutineEntity);
                    await _unitOfWorks.ServiceRoutineRepository.Commit();
                }

                newServiceSteps.Add(new ServiceRoutineStep
                {
                    ServiceId = serviceId,
                    StepId = stepId,
                    CreatedDate = DateTime.Now,
                });
            }
            await _unitOfWorks.ServiceRoutineStepRepository.AddRangeAsync(newServiceSteps);

            await _unitOfWorks.SkincareRoutineRepository.Commit();

            return _mapper.Map<SkinCareRoutineStepDto>(step);
        }



        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWorks.SkinCareRoutineStepRepository.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWorks.SkinCareRoutineStepRepository.Remove(id);
            await _unitOfWorks.SaveChangesAsync();
            return true;
        }
    }
}
