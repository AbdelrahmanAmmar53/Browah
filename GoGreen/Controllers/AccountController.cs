using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThanyaProject.Services;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtService _jwtService;
        private readonly IImageService _imageService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IImageRepository _imageRepository;

        public AccountController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            JwtService jwtService,
            IWebHostEnvironment env,
            ApplicationDbContext context,
            IUserService userService,
            IEmailService emailService,
            IImageService imageService,
            IImageRepository imageRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _env = env;
            _context = context;
            _userService = userService;
            _emailService = emailService;
            _imageService = imageService;
            _imageRepository = imageRepository;
        }

        private async Task<Berwah.Models.Models.Image?> SaveImageToCloudinaryAsync(IFormFile? file, int userId)
        {
            if (file == null || file.Length == 0) return null;

            var uploadResult = await _imageService.UploadImageAsync(file);

            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                return null;

            return new Berwah.Models.Models.Image
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId,
                UserId = userId
            };
        }

        #region Registration Step 1 - Send OTP

        [HttpPost("Register/Customer")]
        public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegister model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { status = "error", message = "كلمة المرور وتأكيد كلمة المرور غير متطابقين" });

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { status = "error", message = "البريد الإلكتروني مستخدم بالفعل" });

            string key = $"Register:{model.Email.Trim().ToLower()}";
            var otpCode = _userService.GenerateAndSaveOtp(key, model, UserType.User.ToString());

            string htmlMessage = $@"
            <div style='direction: rtl; font-family: Arial, sans-serif;'>
                <h2>رمز تأكيد إنشاء الحساب</h2>
                <p>رمز التحقق الخاص بك هو: <b style='font-size: 20px; color: #6c5ce7;'>{otpCode}</b></p>
                <p>هذا الرمز صالِح لمدة 5 دقائق فقط.</p>
            </div>";

            await _emailService.SendEmailAsync(model.Email, "رمز تأكيد الحساب - برواح", htmlMessage);

            return Ok(new { status = "pending_verification", message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني", email = model.Email });
        }

        [HttpPost("Register/Merchant")]
        public async Task<IActionResult> RegisterMerchant([FromForm] MerchantRegister model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { status = "error", message = "كلمة المرور وتأكيد كلمة المرور غير متطابقين" });

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { status = "error", message = "البريد الإلكتروني مستخدم بالفعل" });

            string key = $"Register:{model.Email.Trim().ToLower()}";
            var otpCode = _userService.GenerateAndSaveOtp(key, model, UserType.Merchant.ToString());

            string htmlMessage = $@"
            <div style='direction: rtl; font-family: Arial, sans-serif;'>
                <h2>رمز تأكيد إنشاء حساب تاجر</h2>
                <p>رمز التحقق الخاص بك هو: <b style='font-size: 20px; color: #6c5ce7;'>{otpCode}</b></p>
                <p>هذا الرمز صالِح لمدة 5 دقائق فقط.</p>
            </div>";

            await _emailService.SendEmailAsync(model.Email, "رمز تأكيد الحساب - برواح", htmlMessage);

            return Ok(new { status = "pending_verification", message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني", email = model.Email });
        }

        [HttpPost("Register/Delivery")]
        public async Task<IActionResult> RegisterDelivery([FromForm] DeliveryRegister model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { status = "error", message = "كلمة المرور وتأكيد كلمة المرور غير متطابقين" });

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { status = "error", message = "البريد الإلكتروني مستخدم بالفعل" });

            string key = $"Register:{model.Email.Trim().ToLower()}";
            var otpCode = _userService.GenerateAndSaveOtp(key, model, UserType.Delivery.ToString());

            string htmlMessage = $@"
            <div style='direction: rtl; font-family: Arial, sans-serif;'>
                <h2>رمز تأكيد إنشاء حساب مندوب توصيل</h2>
                <p>رمز التحقق الخاص بك هو: <b style='font-size: 20px; color: #6c5ce7;'>{otpCode}</b></p>
                <p>هذا الرمز صالِح لمدة 5 دقائق فقط.</p>
            </div>";

            await _emailService.SendEmailAsync(model.Email, "رمز تأكيد الحساب - برواح", htmlMessage);

            return Ok(new { status = "pending_verification", message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني", email = model.Email });
        }

        #endregion

        #region Registration Step 2 - Verify OTP & Create Account

        [HttpPost("VerifyRegisterOtp")]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyOTP model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // تأكد من مطابقة نفس الـ Key المستخدم أثناء حفظ الـ OTP في الخطوة الأولى
            string key = $"Register:{model.TargetEmail.Trim().ToLower()}";

            var isValid = _userService.VerifyOtp(key, model.Otp, out var registerModel, out var userTypeStr);

            Console.WriteLine($"Incoming Email: {model.TargetEmail}");
            Console.WriteLine($"Incoming OTP: {model.Otp}");
            Console.WriteLine($"Is OTP Valid: {isValid}");

            if (!isValid || registerModel == null || string.IsNullOrEmpty(userTypeStr))
                return BadRequest(new { status = "error", message = "رمز التحقق غير صحيح أو انتهت صلاحيته" });

            User user;
            string userPassword;

            // 1. معالجة حساب Customer
            if (userTypeStr == UserType.User.ToString() && registerModel is CustomerRegister customerDto)
            {
                user = new User
                {
                    UserName = customerDto.Email.Trim(),
                    Email = customerDto.Email.Trim(),
                    FirstName = customerDto.FirstName,
                    SecondName = customerDto.SecondName,
                    ThirdName = customerDto.ThirdName,
                    LastName = customerDto.LastName,
                    PhoneNumber = customerDto.PhoneNumber,
                    Address = customerDto.Goverment,
                    UserType = UserType.User,
                    EmailConfirmed = true
                };
                userPassword = customerDto.Password;
            }
            // 2. معالجة حساب Merchant
            else if (userTypeStr == UserType.Merchant.ToString() && registerModel is MerchantRegister merchantDto)
            {
                user = new User
                {
                    UserName = merchantDto.Email.Trim(),
                    Email = merchantDto.Email.Trim(),
                    FirstName = merchantDto.FirstName,
                    SecondName = merchantDto.SecondName,
                    ThirdName = merchantDto.ThirdName,
                    LastName = merchantDto.LastName,
                    PhoneNumber = merchantDto.PhoneNumber,
                    Address = merchantDto.AddressDetails,
                    UserType = UserType.Merchant,
                    EmailConfirmed = true
                };
                userPassword = merchantDto.Password;
            }
            // 3. معالجة حساب Delivery
            else if (userTypeStr == UserType.Delivery.ToString() && registerModel is DeliveryRegister deliveryDto)
            {
                user = new User
                {
                    UserName = deliveryDto.Email.Trim(),
                    Email = deliveryDto.Email.Trim(),
                    FirstName = deliveryDto.FirstName,
                    SecondName = deliveryDto.SecondName,
                    ThirdName = deliveryDto.ThirdName,
                    LastName = deliveryDto.LastName,
                    PhoneNumber = deliveryDto.PhoneNumber,
                    Address = deliveryDto.GovermentOrArea,
                    UserType = UserType.Delivery,
                    EmailConfirmed = true
                };
                userPassword = deliveryDto.Password;
            }
            else
            {
                return BadRequest(new { status = "error", message = "نوع البيانات غير مطابق" });
            }

            // إنشاء الحساب لكل الأنواع في مكان واحد موحد
            var result = await _userManager.CreateAsync(user, userPassword);
            if (!result.Succeeded) return BadRequest(new { status = "error", errors = result.Errors });

            if (!await _roleManager.RoleExistsAsync(userTypeStr))
                await _roleManager.CreateAsync(new Role { Name = userTypeStr });

            await _userManager.AddToRoleAsync(user, userTypeStr);

            // رفع صور الـ Delivery فقط في حال كان الموديل الخاص به
            if (registerModel is DeliveryRegister delDto)
            {
                var imagesToUpload = new List<IFormFile?>
        {
            delDto.PersonalImage,
            delDto.NationalIdImage,
            delDto.DrivingLicenseImage,
            delDto.CriminalRecordImage
        };

                foreach (var file in imagesToUpload)
                {
                    if (file != null)
                    {
                        var imageEntity = await SaveImageToCloudinaryAsync(file, user.Id);
                        if (imageEntity != null)
                        {
                            await _imageRepository.AddAsync(imageEntity);
                        }
                    }
                }
            }

            var token = _jwtService.CreateToken(user, new[] { userTypeStr });

            return Ok(new
            {
                status = "success",
                token = token,
                data = new
                {
                    email = user.Email,
                    role = userTypeStr,
                    userId = user.Id
                }
            });
        }

        #endregion

        #region Login & Logout

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid Email or Password");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.Password,
                false);

            if (!result.Succeeded)
                return Unauthorized("Invalid Email or Password");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.CreateToken(user, roles);

            return Ok(new
            {
                status = "success",
                token = token,
                data = new
                {
                    user = new
                    {
                        id = user.Id,
                        name = $"{user.FirstName} {user.LastName}".Trim(),
                        email = user.Email,
                        role = roles.FirstOrDefault(),
                        last_login = DateTime.UtcNow
                    }
                }
            });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { status = "success", message = "Logout successful!" });
        }

        #endregion

        #region Forgot Password Workflow

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPas model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.TargetEmail)
                        ?? _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.TargetEmail);

            if (user == null) return NotFound("المستخدم غير موجود");

            string key = $"ResetPassword:{user.Email.Trim().ToLower()}";
            var otp = _userService.GenerateAndSaveOtp(key, null, null); 

            if (model.SendVie.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                string htmlMessage = $@"
            <div style='direction: rtl; font-family: Arial, sans-serif;'>
                <h2>رمز إعادة تعيين كلمة المرور</h2>
                <p>رمز التحقق الخاص بك هو: <b style='font-size: 20px; color: #6c5ce7;'>{otp}</b></p>
                <p>هذا الرمز صالِح لمدة 5 دقائق فقط.</p>
            </div>";

                await _emailService.SendEmailAsync(user.Email, "رمز إعادة تعيين كلمة المرور - برواح", htmlMessage);
            }

            return Ok(new { status = "success", message = "تم إرسال رمز التحقق بنجاح" });
        }

        [HttpPost("VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] VerifyOTP model)
        {
            string key = $"ResetPassword:{model.TargetEmail.Trim().ToLower()}";
            var isValid = _userService.VerifyOtp(key, model.Otp, out _, out _);

            if (!isValid)
                return BadRequest(new { status = "error", message = "رمز التحقق غير صحيح أو انتهت صلاحيته" });

            return Ok(new { status = "success", message = "تم التحقق بنجاح" });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPass model)
        {
            var user = await _userManager.FindByNameAsync(model.TargetEmail)
                        ?? _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.TargetEmail);

            if (user == null) return NotFound("المستخدم غير موجود");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { status = "success", message = "تم تغيير كلمة المرور بنجاح" });
        }

        #endregion
    }
}