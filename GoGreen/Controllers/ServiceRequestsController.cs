using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Berwah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berwah.Controllers
{
    /// <summary>The customer's side of service requests: see the offers made on a request and pick one provider.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly WalletService _wallet;

        public ServiceRequestsController(ApplicationDbContext context, INotificationService notificationService, WalletService wallet)
        {
            _context = context;
            _notificationService = notificationService;
            _wallet = wallet;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Offers made on one of my requests (newest first).</summary>
        [HttpGet("{requestId}/offers")]
        public async Task<IActionResult> GetOffers(int requestId)
        {
            try
            {
                var request = await _context.ServiceRequests.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == CurrentUserId);
                if (request == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });

                var offers = await _context.ServiceOffers.AsNoTracking()
                    .Where(f => f.ServiceRequestId == requestId && !f.Withdrawn)
                    .OrderBy(f => f.Price)
                    .Select(f => new
                    {
                        id = f.Id,
                        providerId = f.ProviderId,
                        providerName = (f.Provider.FirstName + " " + f.Provider.LastName).Trim(),
                        providerImage = f.Provider.ProfileImageUrl,
                        providerRating = f.Provider.Rating,
                        attachmentUrl = f.AttachmentUrl,
                        // The phone is only shared once I picked this provider.
                        providerPhone = request.AcceptedOfferId == f.Id && !f.Provider.HidePhoneNumber ? f.Provider.PhoneNumber : null,
                        price = f.Price,
                        description = f.Description,
                        executionPeriod = f.ExecutionPeriod,
                        createdAt = f.CreatedAt,
                        isAccepted = request.AcceptedOfferId == f.Id
                    })
                    .ToListAsync();

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        requestId,
                        requestStatus = request.Status.ToString(),
                        acceptedOfferId = request.AcceptedOfferId,
                        providerDone = request.ProviderDoneAt != null,
                        providerDoneAt = request.ProviderDoneAt,
                        myRating = request.CustomerRating,
                        offers
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>A provider's public profile, for a customer who received an offer from them (or the provider's own).</summary>
        [HttpGet("providers/{providerId}")]
        public async Task<IActionResult> GetProvider(int providerId)
        {
            try
            {
                var me = CurrentUserId;
                var allowed = me == providerId || await _context.ServiceOffers.AnyAsync(f =>
                    f.ProviderId == providerId && f.ServiceRequest.UserId == me);
                if (!allowed)
                    return NotFound(new { status = "error", message = "مقدم الخدمة غير موجود" });

                var p = await _context.Users.AsNoTracking().Where(u => u.Id == providerId)
                    .Select(u => new
                    {
                        id = u.Id,
                        name = (u.FirstName + " " + u.LastName).Trim(),
                        image = u.ProfileImageUrl,
                        rating = u.Rating,
                        isVerified = u.IsVerified,
                        serviceName = u.ServiceName,
                        experience = u.PreviousExperience,
                        address = u.Address,
                        joinedAt = u.CreatedAt,
                        hidePhone = u.HidePhoneNumber,
                        phone = u.PhoneNumber
                    }).FirstOrDefaultAsync();
                if (p == null)
                    return NotFound(new { status = "error", message = "مقدم الخدمة غير موجود" });

                var offersCount = await _context.ServiceOffers.CountAsync(f => f.ProviderId == providerId && !f.Withdrawn);
                var completed = await _context.ServiceOffers.CountAsync(f =>
                    f.ProviderId == providerId && f.ServiceRequest.AcceptedOfferId == f.Id &&
                    f.ServiceRequest.Status == ServiceRequestStatus.Completed);
                var chosen = await _context.ServiceOffers.CountAsync(f =>
                    f.ProviderId == providerId && f.ServiceRequest.AcceptedOfferId == f.Id);

                // The phone only after this customer picked the provider.
                var picked = await _context.ServiceOffers.AnyAsync(f =>
                    f.ProviderId == providerId && f.ServiceRequest.UserId == me && f.ServiceRequest.AcceptedOfferId == f.Id);

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        p.id, p.name, p.image, p.rating, p.isVerified, p.serviceName, p.experience, p.address, p.joinedAt,
                        offersCount,
                        acceptedJobs = chosen,
                        completedJobs = completed,
                        phone = picked && !p.hidePhone ? p.phone : null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>The jobs I was chosen for (I am the provider): to do, in progress and done.</summary>
        [HttpGet("my-jobs")]
        public async Task<IActionResult> MyJobs()
        {
            try
            {
                var me = CurrentUserId;
                var jobs = await _context.ServiceRequests.AsNoTracking()
                    .Where(r => r.AcceptedOfferId != null &&
                                _context.ServiceOffers.Any(o => o.Id == r.AcceptedOfferId && o.ProviderId == me))
                    .OrderByDescending(r => r.Id)
                    .Select(r => new
                    {
                        id = r.Id,
                        status = r.Status.ToString(),
                        title = r.SubCategory.Name,
                        description = r.Description,
                        price = _context.ServiceOffers.Where(o => o.Id == r.AcceptedOfferId).Select(o => o.Price).FirstOrDefault(),
                        customerId = r.UserId,
                        customerName = (r.User.FirstName + " " + r.User.LastName).Trim(),
                        customerPhone = r.User.HidePhoneNumber ? null : r.User.PhoneNumber,
                        address = r.DeliveryAddress ?? r.PickupAddress,
                        preferredDate = r.PreferredDate,
                        preferredTime = r.PreferredTimeSlot,
                        providerDone = r.ProviderDoneAt != null,
                        rating = r.CustomerRating,
                        review = r.CustomerReview,
                        createdAt = r.CreatedAt
                    })
                    .ToListAsync();
                return Ok(new { status = "success", data = jobs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private async Task<(ServiceRequest? request, int? providerId)> LoadJobAsync(int requestId)
        {
            var request = await _context.ServiceRequests.FirstOrDefaultAsync(r => r.Id == requestId);
            if (request?.AcceptedOfferId == null) return (request, null);
            var providerId = await _context.ServiceOffers.Where(o => o.Id == request.AcceptedOfferId)
                .Select(o => (int?)o.ProviderId).FirstOrDefaultAsync();
            return (request, providerId);
        }

        /// <summary>The provider starts working on a job I gave them.</summary>
        [HttpPost("{requestId}/start")]
        public async Task<IActionResult> Start(int requestId)
        {
            try
            {
                var (request, providerId) = await LoadJobAsync(requestId);
                if (request == null || providerId != CurrentUserId)
                    return NotFound(new { status = "error", message = "المهمة غير موجودة" });
                if (request.Status != ServiceRequestStatus.Approved)
                    return BadRequest(new { status = "error", message = "لا يمكن بدء هذه المهمة الآن" });

                request.Status = ServiceRequestStatus.InProgress;
                await _context.SaveChangesAsync();
                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = request.UserId,
                    Title = "بدأ تنفيذ طلبك 🛠️",
                    Message = $"بدأ مقدم الخدمة العمل على الطلب #{request.Id}."
                });
                return Ok(new { status = "success", message = "تم بدء المهمة" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>The provider says the job is finished; the customer is asked to confirm it.</summary>
        [HttpPost("{requestId}/provider-done")]
        public async Task<IActionResult> ProviderDone(int requestId)
        {
            try
            {
                var (request, providerId) = await LoadJobAsync(requestId);
                if (request == null || providerId != CurrentUserId)
                    return NotFound(new { status = "error", message = "المهمة غير موجودة" });
                if (request.Status != ServiceRequestStatus.Approved && request.Status != ServiceRequestStatus.InProgress)
                    return BadRequest(new { status = "error", message = "لا يمكن إنهاء هذه المهمة الآن" });

                request.Status = ServiceRequestStatus.InProgress;
                request.ProviderDoneAt ??= DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = request.UserId,
                    Title = "تم تنفيذ طلبك ✅",
                    Message = $"أنهى مقدم الخدمة الطلب #{request.Id}. أكد الاستلام وقيّمه."
                });
                return Ok(new { status = "success", message = "تم إبلاغ العميل بإنهاء المهمة" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        public class ConfirmJobDto
        {
            public int? Rating { get; set; }
            public string? Review { get; set; }
        }

        /// <summary>I confirm the provider finished the job, and may rate them.</summary>
        [HttpPost("{requestId}/confirm")]
        public async Task<IActionResult> Confirm(int requestId, [FromBody] ConfirmJobDto? dto)
        {
            try
            {
                var (request, providerId) = await LoadJobAsync(requestId);
                if (request == null || request.UserId != CurrentUserId || providerId == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });
                if (request.Status == ServiceRequestStatus.Completed)
                    return BadRequest(new { status = "error", message = "تم تأكيد هذا الطلب من قبل" });
                if (request.ProviderDoneAt == null)
                    return BadRequest(new { status = "error", message = "لم ينه مقدم الخدمة المهمة بعد" });

                int? rating = dto?.Rating;
                if (rating != null && (rating < 1 || rating > 5))
                    return BadRequest(new { status = "error", message = "التقييم يجب أن يكون من 1 إلى 5" });

                request.Status = ServiceRequestStatus.Completed;
                request.CompletedAt = DateTime.UtcNow;
                request.CustomerRating = rating;
                // The price held for the job goes to the provider's wallet.
                await _wallet.PayProviderAsync(request.Id, request.UserId, providerId.Value);
                var review = dto?.Review?.Trim();
                request.CustomerReview = string.IsNullOrEmpty(review) ? null : (review.Length > 500 ? review[..500] : review);
                await _context.SaveChangesAsync();

                if (rating != null)
                {
                    // The provider's rating is the average of what customers gave for finished jobs.
                    var avg = await _context.ServiceRequests
                        .Where(r => r.CustomerRating != null && r.AcceptedOfferId != null &&
                                    _context.ServiceOffers.Any(o => o.Id == r.AcceptedOfferId && o.ProviderId == providerId.Value))
                        .AverageAsync(r => (double)r.CustomerRating!);
                    var provider = await _context.Users.FirstOrDefaultAsync(u => u.Id == providerId.Value);
                    if (provider != null)
                    {
                        provider.Rating = Math.Round(avg, 1);
                        await _context.SaveChangesAsync();
                    }
                }

                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = providerId.Value,
                    Title = "أكد العميل استلام الخدمة 🎉",
                    Message = rating == null
                        ? $"أكد العميل إتمام الطلب #{request.Id}."
                        : $"أكد العميل إتمام الطلب #{request.Id} وقيّمك {rating} من 5."
                });
                return Ok(new { status = "success", message = "تم تأكيد إتمام الخدمة" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>Puts a chosen job back on the table: the request is open again and the other offers are still there.</summary>
        private async Task ReopenAsync(ServiceRequest request, int providerId, bool byCustomer)
        {
            // The price held when the offer was accepted goes back to the customer's wallet.
            await _wallet.ReleaseServiceHoldAsync(request.Id, request.UserId);
            var offer = await _context.ServiceOffers.FirstOrDefaultAsync(o => o.Id == request.AcceptedOfferId);
            if (offer != null) offer.Withdrawn = true;
            request.AcceptedOfferId = null;
            request.ProviderDoneAt = null;
            request.Status = ServiceRequestStatus.Pending;
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(new SendNotification
            {
                UserId = byCustomer ? providerId : request.UserId,
                Title = byCustomer ? "تم تغيير مقدم الخدمة" : "اعتذر مقدم الخدمة عن طلبك",
                Message = byCustomer
                    ? $"اختار العميل مقدم خدمة آخر للطلب #{request.Id}."
                    : $"اعتذر مقدم الخدمة عن الطلب #{request.Id}. اختر عرضًا آخر من العروض المتاحة."
            });

            // The others who offered are told the request is open again.
            var others = await _context.ServiceOffers
                .Where(f => f.ServiceRequestId == request.Id && !f.Withdrawn)
                .Select(f => f.ProviderId).Distinct().ToListAsync();
            foreach (var other in others)
            {
                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = other,
                    Title = "الطلب متاح من جديد",
                    Message = $"عاد الطلب #{request.Id} متاحًا للاختيار، وعرضك لا يزال قائمًا."
                });
            }
        }

        /// <summary>
        /// The customer cancels a request nobody has offered on: it is deleted for good (the request,
        /// its chat and any withdrawn offers). Once someone has offered, or a provider was chosen, it can't be.
        /// </summary>
        [HttpDelete("{requestId}")]
        public async Task<IActionResult> CancelRequest(int requestId)
        {
            try
            {
                var request = await _context.ServiceRequests
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == CurrentUserId);
                if (request == null)
                    return NotFound(new { status = "error", message = "الخدمة غير موجودة" });
                // Cancelling works in every state (offers made, provider chosen, job running); only a
                // job the customer already confirmed as done is kept.
                if (request.Status == ServiceRequestStatus.Completed)
                    return BadRequest(new { status = "error", message = "الخدمة اتنفذت وتم تأكيدها، لا يمكن إلغاؤها" });

                var offers = await _context.ServiceOffers.Where(o => o.ServiceRequestId == requestId).ToListAsync();
                var providersToTell = offers.Where(o => !o.Withdrawn).Select(o => o.ProviderId).Distinct().ToList();
                if (request.AcceptedOfferId != null)
                {
                    request.AcceptedOfferId = null;
                    await _context.SaveChangesAsync();
                }

                // Chats about this request (Kind 1) go with it.
                var chats = await _context.Conversations.Where(c => c.Kind == 1 && c.RefId == requestId).ToListAsync();
                if (chats.Count > 0)
                {
                    var chatIds = chats.Select(c => c.Id).ToList();
                    _context.ChatMessages.RemoveRange(_context.ChatMessages.Where(m => chatIds.Contains(m.ConversationId)));
                    _context.Conversations.RemoveRange(chats);
                }
                _context.ServiceOffers.RemoveRange(offers);
                _context.ServiceRequests.Remove(request);
                await _context.SaveChangesAsync();

                foreach (var providerId in providersToTell)
                {
                    try
                    {
                        await _notificationService.SendNotificationAsync(new SendNotification
                        {
                            UserId = providerId,
                            Title = "تم إلغاء الطلب",
                            Message = $"ألغى العميل طلب الخدمة #{requestId}."
                        });
                    }
                    catch { /* best effort */ }
                }

                return Ok(new { status = "success", message = "تم إلغاء الخدمة وحذفها" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        public class TripLocationDto
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        /// <summary>The chosen provider's app reports where he is while the job runs.</summary>
        [HttpPost("{requestId}/location")]
        public async Task<IActionResult> UpdateProviderLocation(int requestId, [FromBody] TripLocationDto dto)
        {
            try
            {
                var (request, providerId) = await LoadJobAsync(requestId);
                if (request == null || providerId != CurrentUserId)
                    return NotFound(new { status = "error", message = "المهمة غير موجودة" });
                if (request.Status != ServiceRequestStatus.Approved && request.Status != ServiceRequestStatus.InProgress)
                    return BadRequest(new { status = "error", message = "المهمة غير نشطة" });
                if (dto.Latitude < -90 || dto.Latitude > 90 || dto.Longitude < -180 || dto.Longitude > 180)
                    return BadRequest(new { status = "error", message = "إحداثيات غير صحيحة" });

                request.ProviderLatitude = dto.Latitude;
                request.ProviderLongitude = dto.Longitude;
                request.ProviderLocationAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { status = "success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>
        /// The trip as the customer (or the provider) follows it: the two ends, the stage, and where the
        /// provider is. The app asks again every few seconds.
        /// </summary>
        [HttpGet("{requestId}/track")]
        public async Task<IActionResult> Track(int requestId)
        {
            try
            {
                var me = CurrentUserId;
                var r = await _context.ServiceRequests.AsNoTracking()
                    .Include(x => x.SubCategory)
                    .FirstOrDefaultAsync(x => x.Id == requestId);
                if (r == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });

                var offer = r.AcceptedOfferId == null
                    ? null
                    : await _context.ServiceOffers.AsNoTracking().Include(o => o.Provider)
                        .FirstOrDefaultAsync(o => o.Id == r.AcceptedOfferId);
                if (r.UserId != me && offer?.ProviderId != me)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });

                // waiting -> toPickup (provider chosen, on his way) -> onTrip (started) -> arrived (provider says done) -> completed.
                string stage = r.Status switch
                {
                    ServiceRequestStatus.Completed => "completed",
                    ServiceRequestStatus.Cancelled => "cancelled",
                    ServiceRequestStatus.InProgress => r.ProviderDoneAt != null ? "arrived" : "onTrip",
                    ServiceRequestStatus.Approved => "toPickup",
                    _ => "waiting"
                };
                bool live = stage == "toPickup" || stage == "onTrip" || stage == "arrived";
                var p = offer?.Provider;

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        id = r.Id,
                        stage,
                        title = r.SubCategory.Name,
                        isProvider = offer?.ProviderId == me,
                        price = offer?.Price,
                        pickup = new { address = r.PickupAddress, lat = r.PickupLatitude, lng = r.PickupLongitude },
                        destination = new { address = r.DeliveryAddress, lat = r.DeliveryLatitude, lng = r.DeliveryLongitude },
                        provider = p == null ? null : new
                        {
                            id = p.Id,
                            name = (p.FirstName + " " + p.LastName).Trim(),
                            image = p.ProfileImageUrl,
                            rating = p.Rating,
                            phone = p.HidePhoneNumber ? null : p.PhoneNumber,
                            lat = live ? r.ProviderLatitude : null,
                            lng = live ? r.ProviderLongitude : null,
                            ageSeconds = live && r.ProviderLocationAt != null
                                ? (double?)(DateTime.UtcNow - r.ProviderLocationAt.Value).TotalSeconds
                                : null
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>The chosen provider backs out of a job that is not finished yet.</summary>
        [HttpPost("{requestId}/withdraw")]
        public async Task<IActionResult> Withdraw(int requestId)
        {
            try
            {
                var (request, providerId) = await LoadJobAsync(requestId);
                if (request == null || providerId != CurrentUserId)
                    return NotFound(new { status = "error", message = "المهمة غير موجودة" });
                if ((request.Status != ServiceRequestStatus.Approved && request.Status != ServiceRequestStatus.InProgress)
                    || request.ProviderDoneAt != null)
                    return BadRequest(new { status = "error", message = "لا يمكن الاعتذار عن هذه المهمة الآن" });

                await ReopenAsync(request, providerId.Value, byCustomer: false);
                return Ok(new { status = "success", message = "تم الاعتذار عن المهمة" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>The customer picks someone else, as long as the chosen provider has not started.</summary>
        [HttpPost("{requestId}/change-provider")]
        public async Task<IActionResult> ChangeProvider(int requestId)
        {
            try
            {
                var (request, providerId) = await LoadJobAsync(requestId);
                if (request == null || request.UserId != CurrentUserId || providerId == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });
                if (request.Status != ServiceRequestStatus.Approved)
                    return BadRequest(new { status = "error", message = "لا يمكن تغيير مقدم الخدمة بعد بدء التنفيذ" });

                await ReopenAsync(request, providerId.Value, byCustomer: true);
                return Ok(new { status = "success", message = "تم إلغاء اختيار مقدم الخدمة، اختر عرضًا آخر" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>Choose one offer: its provider gets the job, the others are told it went elsewhere.</summary>
        [HttpPost("offers/{offerId}/accept")]
        public async Task<IActionResult> AcceptOffer(int offerId)
        {
            try
            {
                var offer = await _context.ServiceOffers
                    .Include(f => f.ServiceRequest)
                    .FirstOrDefaultAsync(f => f.Id == offerId);
                if (offer == null || offer.Withdrawn || offer.ServiceRequest.UserId != CurrentUserId)
                    return NotFound(new { status = "error", message = "العرض غير موجود" });

                var request = offer.ServiceRequest;
                if (request.Status != ServiceRequestStatus.Pending)
                    return BadRequest(new { status = "error", message = "تم اختيار منفذ لهذا الطلب بالفعل أو أنه لم يعد متاحًا" });

                // The offer's price is held from the customer's wallet until the job is confirmed.
                if (!await _wallet.TryChargeAsync(request.UserId, offer.Price, $"حجز مبلغ الخدمة #{request.Id}", serviceRequestId: request.Id))
                {
                    var balance = await _wallet.BalanceAsync(request.UserId);
                    return BadRequest(new
                    {
                        status = "error",
                        code = "insufficient_wallet",
                        balance,
                        required = offer.Price,
                        message = $"رصيد محفظتك ({balance:0.##} ج.م) لا يكفي لقبول هذا العرض ({offer.Price:0.##} ج.م)، اشحن محفظتك أولاً"
                    });
                }

                request.AcceptedOfferId = offer.Id;
                request.Status = ServiceRequestStatus.Approved;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    await _wallet.ReleaseServiceHoldAsync(request.Id, request.UserId);
                    throw;
                }

                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = offer.ProviderId,
                    Title = "تم قبول عرضك ✅",
                    Message = $"اختار العميل عرضك على الطلب #{request.Id}. تواصل معه لتنفيذ المهمة."
                });

                var others = await _context.ServiceOffers
                    .Where(f => f.ServiceRequestId == request.Id && f.Id != offer.Id)
                    .Select(f => f.ProviderId).Distinct().ToListAsync();
                foreach (var providerId in others)
                {
                    await _notificationService.SendNotificationAsync(new SendNotification
                    {
                        UserId = providerId,
                        Title = "تم اختيار مقدم آخر",
                        Message = $"اختار العميل عرضًا آخر على الطلب #{request.Id}."
                    });
                }

                return Ok(new { status = "success", message = "تم اختيار مقدم الخدمة" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
