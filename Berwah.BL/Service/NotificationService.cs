using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Berwah.BL.Service
{
    public class NotificationService: INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task<IEnumerable<NotificationHistory>> GetNotificationHistoryAsync(int userId)
        {
            var notifications = await _notificationRepository.GetNotificationHistoryAsync(userId);

            return notifications.Select(n => new NotificationHistory
            {
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt
            });
        }
        public async Task SendNotificationAsync(SendNotification sendNotification)
        {
            var notification = new Models.Models.Notification
            {
                UserId = sendNotification.UserId,
                Title = sendNotification.Title,
                Message = sendNotification.Message,
                CreatedAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateNotificationAsync(notification);
        }
    }
}
