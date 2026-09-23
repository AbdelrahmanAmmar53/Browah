using Berwah.BL.Service.IService;
using System;
using System.Collections.Concurrent;

namespace Berwah.BL.Service
{
    public class UserService : IUserService
    {
        // نخزن كود الـ OTP، وقت الانتهاء، بيانات التسجيل (إن وجدت)، ونوع المستخدم
        private readonly ConcurrentDictionary<string, (string Code, DateTime Expiry, object? RegisterModel, string? UserType)> _otpStore
            = new ConcurrentDictionary<string, (string, DateTime, object?, string?)>();

        public string GenerateAndSaveOtp(string key, object? registerModel , string? userType)
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
    }
}