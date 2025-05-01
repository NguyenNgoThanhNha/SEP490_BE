using Microsoft.EntityFrameworkCore;
using Server.Business.Commons.Response;
using Server.Data.UnitOfWorks;

namespace Server.Business.Services;

public class NotificationServices
{
    private readonly UnitOfWorks _unitOfWorks;

    public NotificationServices(UnitOfWorks unitOfWorks)
    {
        _unitOfWorks = unitOfWorks;
    }
    
    public async Task<GetAllNotificationsByUserIdResponse> GetAllNotificationsByUserIdAsync(int userId, int pageIndex, int pageSize)
    {
        var query = _unitOfWorks.NotificationRepository
            .FindByCondition(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new GetAllNotificationsByUserIdResponse
        {
            Message = "Lấy danh sách thông báo thành công",
            Data = items,
            Pagination = new Pagination
            {
                page = pageIndex,
                totalPage = totalPages,
                totalCount = totalCount
            }
        };
    }
    
    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await _unitOfWorks.NotificationRepository
            .FindByCondition(n => n.NotificationId == notificationId)
            .FirstOrDefaultAsync();

        if (notification == null) return false;

        notification.isRead = true;
        notification.UpdatedDate = DateTime.Now;

        _unitOfWorks.NotificationRepository.Update(notification);
        await _unitOfWorks.SaveChangesAsync();

        return true;
    }


}