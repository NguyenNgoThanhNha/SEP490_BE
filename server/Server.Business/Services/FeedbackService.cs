using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Request;
using Server.Business.Commons.Response;
using Server.Data;
using Server.Data.Entities;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class FeedbackService
{
    private readonly UnitOfWorks _unitOfWorks;

    public FeedbackService(UnitOfWorks unitOfWorks)
    {
        _unitOfWorks = unitOfWorks;
    }
    
    public async Task<FeebackResponse> CreateFeedbackService(ServiceFeedbackRequest feedback)
    {
        var service = await _unitOfWorks.ServiceRepository.GetByIdAsync(feedback.ServiceId);
        if (service == null)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Service not found"
            };
        }
        
        var customer = await _unitOfWorks.UserRepository.GetByIdAsync(feedback.CustomerId ?? 0);
        if (customer == null)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Customer feedback not found"
            };
        }

        var customerService = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.ServiceId == feedback.ServiceId && x.CustomerId == feedback.CustomerId && x.Status == OrderStatusEnum.Completed.ToString())
            .FirstOrDefaultAsync();
        if (customerService == null)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Customer not use this service"
            };
        }
        
        var countServiceFeedback = await _unitOfWorks.FeedbackServiceRepository
            .FindByCondition(x => x.CustomerId == feedback.CustomerId 
                                  && x.ServiceId == feedback.ServiceId  
                                  && x.Status == FeedbackStatus.Feedbacked.ToString())
            .ToListAsync();

        if (countServiceFeedback.Count >= 3)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Customer feedback only 3 times"
            };
        }
        
        var serviceFeedback = new ServiceFeedback()
        {
            ServiceId = feedback.ServiceId,
            CustomerId = feedback.CustomerId,
            Comment = feedback.Comment,
            Rating = feedback.Rating,
            Status = FeedbackStatus.Feedbacked.ToString(),
            CreatedBy = customer.UserName,
            UpdatedBy = customer.UserName,
            ImageBefore = feedback.ImageBefore ?? "",
            ImageAfter = feedback.ImageAfter ?? ""
        };
        
        await _unitOfWorks.FeedbackServiceRepository.AddAsync(serviceFeedback);
        var result = await _unitOfWorks.FeedbackServiceRepository.Commit();
        return result > 0 ? new FeebackResponse()
        {
            Success = true,
            Message = "Create feedback success"
        } : new FeebackResponse()
        {
            Success = false,
            Message = "Create feedback failed"
        };
    }

    public async Task<FeebackResponse> CreateFeedbackAppointment(AppointmentFeedbackRequest request)
    {
        var appointment = await _unitOfWorks.AppointmentsRepository.GetByIdAsync(request.AppointmentId);
        if (appointment == null)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Appointment not found"
            };
        }
        
        var customer = await _unitOfWorks.UserRepository.GetByIdAsync(request.CustomerId ?? 0);
        if (customer == null)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Customer feedback not found"
            };
        }
        
        var customerAppointment = await _unitOfWorks.AppointmentsRepository
            .FindByCondition(x =>
                x.AppointmentId == request.AppointmentId && x.CustomerId == request.CustomerId && x.Status == OrderStatusEnum.Completed.ToString())
            .FirstOrDefaultAsync();
        
        if (customerAppointment == null)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Customer not use this service"
            };
        }
        
        var countFeedback = await _unitOfWorks.FeedbackAppointmentRepository
            .FindByCondition(x => x.CustomerId == request.CustomerId 
                                  && x.AppointmentId == request.AppointmentId  
                                  && x.Status == FeedbackStatus.Feedbacked.ToString())
            .ToListAsync();
        
        if (countFeedback.Count >= 3)
        {
            return new FeebackResponse()
            {
                Success = false,
                Message = "Customer feedback only 3 times"
            };
        }
        
        var appointmentFeedback = new AppointmentFeedback()
        {
            AppointmentId = request.AppointmentId,
            CustomerId = request.CustomerId,
            Comment = request.Comment,
            Rating = request.Rating,
            Status = FeedbackStatus.Feedbacked.ToString(),
            CreatedBy = customer.UserName,
            UpdatedBy = customer.UserName,
            ImageBefore = request.ImageBefore ?? "",
            ImageAfter = request.ImageAfter ?? ""
        };
        
        await _unitOfWorks.FeedbackAppointmentRepository.AddAsync(appointmentFeedback);
        var result = await _unitOfWorks.FeedbackAppointmentRepository.Commit();
        return result > 0 ? new FeebackResponse()
        {
            Success = true,
            Message = "Create feedback success"
        } : new FeebackResponse()
        {
            Success = false,
            Message = "Create feedback failed"
        };
    }
}