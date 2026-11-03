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
        private readonly INotificationPusher? _pusher;
        public NotificationService(INotificationRepository notificationRepository, INotificationPusher? pusher = null)
        {
            _notificationRepository = notificationRepository;
            _pusher = pusher;
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

            // Real time: the app that is open gets it now; the stored copy is for the list.
            if (_pusher != null)
            {
                await _pusher.PushAsync(notification.UserId, new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    createdAt = notification.CreatedAt
                });
            }
        }
    }
}
