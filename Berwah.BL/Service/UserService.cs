using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class UserService : IUserService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UserService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        private readonly ConcurrentDictionary<string, (string Code, DateTime Expiry, object? RegisterModel, string? UserType)> _otpStore
            = new ConcurrentDictionary<string, (string, DateTime, object?, string?)>();

        public string GenerateAndSaveOtp(string key, object? registerModel, string? userType)
        {
            var code = new Random().Next(1000, 9999).ToString();
            _otpStore[key] = (code, DateTime.UtcNow.AddMinutes(5), registerModel, userType);
            return code;
        }

        public bool VerifyOtp(string key, string code, out object? registerModel, out string? userType)
        {
            registerModel = null;
            userType = null;

            if (_otpStore.TryGetValue(key, out var data))
            {
                if (data.Expiry >= DateTime.UtcNow && data.Code == code)
                {
                    registerModel = data.RegisterModel;
                    userType = data.UserType;
                    _otpStore.TryRemove(key, out _);
                    return true;
                }
            }
            return false;
        }

        public async Task<byte[]> ExportUserDataAsExcelAsync(int userId)
        {
      
            using var scope = _scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<ISupporterRepository>();

            var user = await userRepository.GetUserWithDetailsForExportAsync(userId);

            if (user == null)
                throw new Exception("المستخدم غير موجود");

            using (var workbook = new XLWorkbook())
            {
   
                var profileSheet = workbook.Worksheets.Add("البيانات الشخصية");
                profileSheet.Cell(1, 1).Value = "المعرف (ID)";
                profileSheet.Cell(1, 2).Value = user.Id;
                profileSheet.Cell(2, 1).Value = "الاسم";
                profileSheet.Cell(2, 2).Value = $"{user.FirstName} {user.LastName}";
                profileSheet.Cell(3, 1).Value = "البريد الإلكتروني";
                profileSheet.Cell(3, 2).Value = user.Email;
                profileSheet.Cell(4, 1).Value = "رقم الهاتف";
                profileSheet.Cell(4, 2).Value = user.PhoneNumber;
                profileSheet.Cell(5, 1).Value = "تاريخ الإنضمام";
                profileSheet.Cell(5, 2).Value = user.CreatedAt.ToString("yyyy-MM-dd");

                // 2. شيت الإعلانات
                var adsSheet = workbook.Worksheets.Add("الإعلانات");
                adsSheet.Cell(1, 1).Value = "ID";
                adsSheet.Cell(1, 2).Value = "العنوان";
                adsSheet.Cell(1, 3).Value = "السعر";
                adsSheet.Cell(1, 4).Value = "التاريخ";

                int row = 2;
                if (user.Advertisements != null)
                {
                    foreach (var ad in user.Advertisements)
                    {
                        adsSheet.Cell(row, 1).Value = ad.Id;
                        adsSheet.Cell(row, 2).Value = ad.Title;
                        adsSheet.Cell(row, 3).Value = ad.Price;
                        adsSheet.Cell(row, 4).Value = ad.CreatedAt.ToString("yyyy-MM-dd");
                        row++;
                    }
                }

                // 3. شيت طلبات الخدمات
                var requestsSheet = workbook.Worksheets.Add("طلبات الخدمات");
                requestsSheet.Cell(1, 1).Value = "ID";
                requestsSheet.Cell(1, 2).Value = "تفاصيل الطلب";
                requestsSheet.Cell(1, 3).Value = "التاريخ";

                row = 2;
                if (user.ServiceRequests != null)
                {
                    foreach (var req in user.ServiceRequests)
                    {
                        requestsSheet.Cell(row, 1).Value = req.Id;
                        requestsSheet.Cell(row, 2).Value = req.Description;
                        requestsSheet.Cell(row, 3).Value = req.CreatedAt.ToString("yyyy-MM-dd");
                        row++;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}