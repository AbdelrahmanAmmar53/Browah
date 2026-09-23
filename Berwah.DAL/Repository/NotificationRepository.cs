using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ApplicationDbContext _context;
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public Task<IEnumerable<NotificationHistory>> GetNotificationHistoryAsync(int userId)
        {
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationHistory
                {
                    Title = n.Title,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt
                })
                .ToList();
            return Task.FromResult<IEnumerable<NotificationHistory>>(notifications);
        }
        public async Task CreateNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
    }
}
