using Berwah.Models.DTO;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationHistory>> GetNotificationHistoryAsync(int userId);
        Task SendNotificationAsync(SendNotification sendNotification);
    }
}
