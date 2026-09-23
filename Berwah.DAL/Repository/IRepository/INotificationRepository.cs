using Berwah.Models.DTO;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface INotificationRepository
    {
        Task<IEnumerable<NotificationHistory>> GetNotificationHistoryAsync(int userId);
        Task CreateNotificationAsync(Notification notification);
    }
}
