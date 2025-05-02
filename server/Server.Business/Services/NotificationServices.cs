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
    
    public async Task<GetAllNotificationsByUserIdResponse> GetAllNotificationsByUserIdAsync(int userId, bool? isRead, int pageIndex, int pageSize)
    {
        var query = _unitOfWorks.NotificationRepository
            .FindByCondition(n => n.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.isRead == isRead.Value);
        }

        query = query.OrderByDescending(n => n.CreatedDate);
        
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

    public async Task<bool> MarkAllAsReadByUserIdAsync(int userId)
    {
        var notifications = await _unitOfWorks.NotificationRepository
            .FindByCondition(n => n.UserId == userId && !n.isRead.HasValue)
            .ToListAsync();

        if (notifications == null || !notifications.Any()) return false;

        foreach (var notification in notifications)
        {
            notification.isRead = true;
            notification.UpdatedDate = DateTime.Now;
        }

        await _unitOfWorks.NotificationRepository.UpdateRangeAsync(notifications);
        await _unitOfWorks.SaveChangesAsync();

        return true;
    }

}