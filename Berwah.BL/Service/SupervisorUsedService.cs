using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Delivery;
using Berwah.Models.DTO.SupervisorUsed;
using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class SupervisorUsedService : ISupervisorUsedService
    {
        private readonly ISupervisorUsedRepository _repo;
        private readonly INotificationService _notifications; 

        public SupervisorUsedService(ISupervisorUsedRepository repo, INotificationService notifications)
        {
            _repo = repo;
            _notifications = notifications;
        }

        public async Task<UsedAdStatusDto> GetUsedAdsStatsAsync()
        {
            return await _repo.GetStatsAsync();
        }

        public async Task<IEnumerable<object>> GetUsedAdsByStatusAsync(AdStatus? status, int page, int pageSize)
        {
            var ads = await _repo.GetUsedAdsByStatusAsync(status, page, pageSize);

            return ads.Select(a => new
            {
                a.Id,
                Title = a.Title ?? "بدون عنوان",
                Price = a.Price,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
                PublisherName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "مستخدم مجهول",
                ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url ?? ""
            });
        }

        public async Task<object?> GetUsedAdDetailsAsync(int id)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return null;

            return new
            {
                ad.Id,
                ad.Title,
                ad.Description,
                ad.Price,
                Status = ad.Status.ToString(),
                Publisher = ad.User != null ? new
                {
                    ad.User.Id,
                    FullName = $"{ad.User.FirstName} {ad.User.LastName}".Trim(),
                    ad.User.PhoneNumber,
                    ad.User.IsVerified
                } : null,
                Images = ad.AdvertismentImages?.Select(ai => new
                {
                    ai.Image?.Url,
                    IsMain = ai.IsMain
                })
            };
        }

        public async Task<bool> ApproveAndPublishAdAsync(int id, UsedAdActionDto dto)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Approved;
            if (dto.ApprovedPrice.HasValue) ad.Price = dto.ApprovedPrice.Value;
            if (!string.IsNullOrEmpty(dto.ApprovedDescription)) ad.Description = dto.ApprovedDescription;

            var updated = await _repo.UpdateAdAsync(ad);

            if (updated && dto.SendNotification && ad.UserId>0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "تم قبول إعلانك المستعمل 🎉",
                        Message = $"تم اعتماد ونشر إعلانك #{ad.Id} بنجاح."
                    });
                }
                catch { }
            }

            return updated;
        }

        public async Task<bool> RequestEditFromPublisherAsync(int id, string? reason)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Pending; 
            var updated = await _repo.UpdateAdAsync(ad);

            if (updated && ad.UserId>0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "طلب تعديل على إعلانك 📝",
                        Message = $"يرجى تعديل الإعلان #{ad.Id}. السبب: {reason ?? "راجع التفاصيل"}"
                    });
                }
                catch { }
            }

            return updated;
        }

        public async Task<bool> SuspendAdAsync(int id, string? reason)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Suspended;
            return await _repo.UpdateAdAsync(ad);
        }

        public async Task<bool> RejectAdWithPenaltyAsync(int id, UsedAdActionDto dto)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Rejected;
            var updated = await _repo.UpdateAdAsync(ad);

            if (updated && dto.SendNotification && ad.UserId>0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "تم رفض الإعلان ❌",
                        Message = $"عذراً، تم رفض إعلانك المستعمل #{ad.Id}. السبب: {dto.Reason ?? "مخالفة الشروط"}"
                    });
                }
                catch { }
            }

            return updated;
        }

        public async Task<IEnumerable<object>> GetWosallyOrdersAsync(string? status, int page, int pageSize)
        {
            var orders = await _repo.GetWosallyOrdersAsync(status, page, pageSize);
            return orders.Select(o => new
            {
                o.Id,
                Status = o.Status.ToString(),
                o.CreatedAt,
                BuyerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : "مشتري مجهول"
            });
        }

        public async Task<object?> GetWosallyOrderDetailsAsync(int id)
        {
            var order = await _repo.GetWosallyOrderByIdAsync(id);
            if (order == null) return null;

            return new
            {
                order.Id,
                Status = order.Status.ToString(),
                EscrowStatus = "محتجز لدى وصلني", // ضمان أمين مبلغ آمن
                Buyer = order.User != null ? new { order.User.Id, FullName = $"{order.User.FirstName} {order.User.LastName}", order.User.PhoneNumber } : null
            };
        }

        public async Task<bool> ApproveAndRevealBuyerDataAsync(int id, WosallyActionDto dto)
        {
            var order = await _repo.GetWosallyOrderByIdAsync(id);
            if (order == null) return false;

            return true;
        }

        public async Task<bool> SendJointAlertAsync(int id)
        {
            var order = await _repo.GetWosallyOrderByIdAsync(id);
            if (order == null) return false;

            return true;
        }

        public async Task<IEnumerable<object>> GetComplaintsAsync(string? status, int page, int pageSize)
        {
            var tickets = await _repo.GetComplaintsAsync(status, page, pageSize);
            return tickets.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.CreatedAt,
                Status = t.Status.ToString()
            });
        }

        public async Task<bool> FollowUpComplaintAsync(int id, ComplaintActionForUsedDto dto)
        {
            var ticket = await _repo.GetComplaintByIdAsync(id);
            if (ticket == null) return false;

            // تحديث حالة الشكوى لمتابعة ميدانية
            return await _repo.UpdateTicketAsync(ticket);
        }

      

        public async Task<ComplaintDetailsResponseDto?> GetDetailedComplaintAsync(int id)
        {
            var ticket = await _repo.GetDetailedComplaintAsync(id);
            if (ticket == null) return null;

            return new ComplaintDetailsResponseDto
            {
                Id = ticket.Id,
                TicketNumber = $"CMP-{ticket.Id}",
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status.ToString(),
                CreatedAt = ticket.CreatedAt,
                Item = ticket.Advertisement != null ? new ItemDetailsDto
                {
                    Id = ticket.Advertisement.Id,
                    Title = ticket.Advertisement.Title,
                    Price = ticket.Advertisement.Price,
                    ImageUrl = ticket.Advertisement.AdvertismentImages?.FirstOrDefault()?.Image?.Url ?? "",
                    Condition = ticket.Advertisement.Condition.ToString()
                } : null,
                Buyer = ticket.User != null ? new PartyDto
                {
                    Id = ticket.User.Id,
                    FullName = $"{ticket.User.FirstName} {ticket.User.LastName}".Trim(),
                    PhoneNumber = ticket.User.PhoneNumber ?? "",
                    IsVerified = ticket.User.IsVerified
                } : null,
                Seller = ticket.Advertisement?.User != null ? new PartyDto
                {
                    Id = ticket.Advertisement.User.Id,
                    FullName = $"{ticket.Advertisement.User.FirstName} {ticket.Advertisement.User.LastName}".Trim(),
                    PhoneNumber = ticket.Advertisement.User.PhoneNumber ?? "",
                    IsVerified = ticket.Advertisement.User.IsVerified
                } : null,
                InspectionChecklist = new List<string>
        {
            "فحص صور السلعة المعتمَدة قبل الشحن",
            "الاستماع لإعادة المشتري والتدقيق",
            "مراجعة بوليصة الاستلام ومحضر التسليم"
        }
            };
        }

        public async Task<bool> SendComplaintToSellerAsync(int id, ComplaintActionForUsedDto dto)
        {
            var ticket = await _repo.GetDetailedComplaintAsync(id);
            if (ticket == null) return false;

            ticket.Status = TicketStatus.InProgress;
            var updated = await _repo.UpdateTicketAsync(ticket);

            if (updated && ticket.Advertisement?.UserId > 0)
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = ticket.Advertisement.UserId,
                    Title = "شكوى جديدة بخصوص إعلانك ⚠️",
                    Message = $"يرجى الرد على الشكوى #{ticket.Id}. السبب: {dto.Reason ?? "مراجعة النزاع"}"
                });
            }
            return updated;
        }

        public async Task<bool> EscalateToBossAsync(int id, ComplaintActionForUsedDto dto)
        {
            var ticket = await _repo.GetDetailedComplaintAsync(id);
            if (ticket == null) return false;

            ticket.Status = TicketStatus.Closed; // أو تحويله لحالة مصعّدة
            return await _repo.UpdateTicketAsync(ticket);
        }

        public async Task<SupervisorWalletDto> GetWalletDataAsync(int supervisorId)
        {
            var txs = await _repo.GetWalletTransactionsAsync(supervisorId);
            return new SupervisorWalletDto
            {
                CurrentBalance = 8450.0m,
                PendingAmount = 4050.0m,
                TotalEarnings = 14850.0m,
                RecentTransactions = txs.Select(t => new TransactionSupervisorDto
                {
                    Id = t.Id,
                    Title = t.Description ?? "معاملة مالية",
                    Amount = t.Amount,
                    Date = t.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    Type = t.Amount > 0 ? "In" : "Out"
                }).ToList()
            };
        }
    }
}
    
