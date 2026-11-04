using Berwah.BL.Service;
using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IImageRepository _imageRepository;

        public AccountController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            JwtService jwtService,
            ApplicationDbContext context,
            IUserService userService,
            IEmailService emailService,
            IImageService imageService,
            IImageRepository imageRepository,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
            _userService = userService;
            _emailService = emailService;
            _imageService = imageService;
            _imageRepository = imageRepository;
            _notificationService = notificationService;
        }

        #region Helpers

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string?> CheckEmailAndPhoneExistenceAsync(string email, string phoneNumber)
        {
            var emailNormalized = email.Trim().ToLower();
            var phoneNormalized = phoneNumber.Trim();

            var existingEmail = await _userManager.FindByNameAsync(emailNormalized);
            if (existingEmail != null)
                return "البريد الإلكتروني مستخدم بالفعل";

            var existingPhone = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNormalized);
            if (existingPhone != null)
                return "رقم الهاتف مستخدم بالفعل";

            return null;
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 9)
                return false;

            return password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        private async Task<IActionResult> ProcessRegistrationAsync<T>(T model, string email, string phone, string password, string confirmPassword, UserType userType, string title)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { status = "error", message = "البيانات المدخلة غير صحيحة", errors = ModelState.Values.SelectMany(v => v.Errors) });

            if (!IsValidEmail(email))
                return BadRequest(new { status = "error", message = "صيغة البريد الإلكتروني غير صحيحة" });

            var normalizedEmail = email.Trim().ToLower();
            var normalizedPhone = phone.Trim();

            if (password != confirmPassword)
                return BadRequest(new { status = "error", message = "كلمة المرور وتأكيد كلمة المرور غير متطابقين" });

            var existenceError = await CheckEmailAndPhoneExistenceAsync(normalizedEmail, normalizedPhone);
            if (existenceError != null)
                return BadRequest(new { status = "error", message = existenceError });

            if (!IsStrongPassword(password))
                return BadRequest(new { status = "error", message = "كلمة المرور يجب أن تحتوي على أحرف كبيرة وصغيرة وأرقام ورموز، وطولها 9 أحرف على الأقل" });

            string key = $"Register:{normalizedEmail}";
            var otpCode = _userService.GenerateAndSaveOtp(key, model, userType.ToString());

            string htmlMessage = $@"
            <div style='direction: rtl; font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px; border-radius: 10px;'>
                <h2 style='color: #333;'>{title}</h2>
                <p>مرحباً بك في برواح!</p>
                <p>رمز التحقق الخاص بك هو:</p>
                <div style='background: white; padding: 15px; border-radius: 5px; text-align: center;'>
                    <b style='font-size: 24px; color: #6c5ce7; letter-spacing: 5px;'>{otpCode}</b>
                </div>
                <p style='color: #666;'>هذا الرمز صالح لمدة <b>5 دقائق</b> فقط.</p>
                <p style='color: #999; font-size: 12px;'>إذا لم تطلب هذا الرمز، يرجى تجاهل هذه الرسالة.</p>
            </div>";

            await _emailService.SendEmailAsync(normalizedEmail, "رمز تأكيد الحساب - برواح", htmlMessage);

            return Ok(new
            {
                status = "pending_verification",
                message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني",
                email = normalizedEmail,
                expiresIn = 300
            });
        }

        //private async Task<Berwah.Models.Models.Image?> SaveImageToCloudinaryAsync(FileDataDto? fileDto, int userId)
        //{
        //    if (fileDto == null || fileDto.Content == null || fileDto.Content.Length == 0)
        //        return null;

        //    // Call the new byte-array Overload
        //    var uploadResult = await _imageService.UploadImageAsync(fileDto.Content, fileDto.FileName, fileDto.ContentType);

        //    if (uploadResult == null || string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
        //        return null;

        //    return new Berwah.Models.Models.Image
        //    {
        //        Url = uploadResult.SecureUrl.ToString(),
        //        PublicId = uploadResult.PublicId,
        //        UserId = userId
        //    };
        //}

        private async Task<Berwah.Models.Models.Image?> SaveImageToCloudinaryAsync(IFormFile? file, int userId)
        {
            if (file == null || file.Length == 0)
                return null;

            // استدعاء الميثود الأصلية بالـ ImageService التي تأخذ IFormFile
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

        #endregion

        #region Registration Step 1

        [HttpPost("Register/Customer")]
        public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegister model)
        {
            try
            {
                return await ProcessRegistrationAsync(model, model.Email, model.PhoneNumber, model.Password, model.ConfirmPassword, UserType.User, "رمز تأكيد إنشاء الحساب");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء معالجة الطلب", details = ex.Message });
            }
        }

        [HttpPost("Register/Merchant")]
        public async Task<IActionResult> RegisterMerchant([FromForm] MerchantRegister model)
        {
            try
            {
                // The store picture is uploaded now; only its URL travels with the pending sign-up.
                model.ProfileImageUrl = null;
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    var res = await _imageService.UploadImageAsync(model.ProfileImage);
                    if (res != null && !string.IsNullOrEmpty(res.SecureUrl?.ToString()))
                        model.ProfileImageUrl = res.SecureUrl.ToString();
                }
                model.ProfileImage = null;

                return await ProcessRegistrationAsync(model, model.Email, model.PhoneNumber, model.Password, model.ConfirmPassword, UserType.Merchant, "رمز تأكيد إنشاء حساب تاجر");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء معالجة الطلب", details = ex.Message });
            }
        }

        [HttpPost("Register/Delivery")]
        public async Task<IActionResult> RegisterDelivery([FromForm] DeliveryRegister model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "البيانات المدخلة غير صحيحة", errors = ModelState.Values.SelectMany(v => v.Errors) });

                if (!IsValidEmail(model.Email))
                    return BadRequest(new { status = "error", message = "صيغة البريد الإلكتروني غير صحيحة" });

                var normalizedEmail = model.Email.Trim().ToLower();
                var normalizedPhone = model.PhoneNumber.Trim();

                if (model.Password != model.ConfirmPassword)
                    return BadRequest(new { status = "error", message = "كلمة المرور وتأكيد كلمة المرور غير متطابقين" });

                var existenceError = await CheckEmailAndPhoneExistenceAsync(normalizedEmail, normalizedPhone);
                if (existenceError != null)
                    return BadRequest(new { status = "error", message = existenceError });

                if (!IsStrongPassword(model.Password))
                    return BadRequest(new { status = "error", message = "كلمة المرور يجب أن تحتوي على أحرف كبيرة وصغيرة وأرقام ورموز، وطولها 9 أحرف على الأقل" });

                // رفع الصور فوراً والحصول على الروابط أثناء أن الـ Stream صالح ومفتوح
                if (model.PersonalImage != null && model.PersonalImage.Length > 0)
                {
                    var res = await _imageService.UploadImageAsync(model.PersonalImage);
                    if (res != null && !string.IsNullOrEmpty(res.SecureUrl?.ToString()))
                        model.PersonalImageUrl = res.SecureUrl.ToString();
                }

                if (model.NationalIdImage != null && model.NationalIdImage.Length > 0)
                {
                    var res = await _imageService.UploadImageAsync(model.NationalIdImage);
                    if (res != null && !string.IsNullOrEmpty(res.SecureUrl?.ToString()))
                        model.NationalIdImageUrl = res.SecureUrl.ToString();
                }

                if (model.DrivingLicenseImage != null && model.DrivingLicenseImage.Length > 0)
                {
                    var res = await _imageService.UploadImageAsync(model.DrivingLicenseImage);
                    if (res != null && !string.IsNullOrEmpty(res.SecureUrl?.ToString()))
                        model.DrivingLicenseImageUrl = res.SecureUrl.ToString();
                }

                if (model.CriminalRecordImage != null && model.CriminalRecordImage.Length > 0)
                {
                    var res = await _imageService.UploadImageAsync(model.CriminalRecordImage);
                    if (res != null && !string.IsNullOrEmpty(res.SecureUrl?.ToString()))
                        model.CriminalRecordImageUrl = res.SecureUrl.ToString();
                }

                if (model.VeicleImage != null && model.VeicleImage.Length > 0)
                {
                    var res = await _imageService.UploadImageAsync(model.VeicleImage);
                    if (res != null && !string.IsNullOrEmpty(res.SecureUrl?.ToString()))
                        model.VehicleImageUrl = res.SecureUrl.ToString();
                }

                model.PersonalImage = null;
                model.NationalIdImage = null;
                model.DrivingLicenseImage = null;
                model.CriminalRecordImage = null;
                model.VeicleImage = null;

                string key = $"Register:{normalizedEmail}";
                var otpCode = _userService.GenerateAndSaveOtp(key, model, UserType.Delivery.ToString());

                string htmlMessage = $@"
        <div style='direction: rtl; font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px; border-radius: 10px;'>
            <h2 style='color: #333;'>رمز تأكيد إنشاء حساب مندوب توصيل</h2>
            <p>مرحباً بك في برواح!</p>
            <p>رمز التحقق الخاص بك هو:</p>
            <div style='background: white; padding: 15px; border-radius: 5px; text-align: center;'>
                <b style='font-size: 24px; color: #6c5ce7; letter-spacing: 5px;'>{otpCode}</b>
            </div>
            <p style='color: #666;'>هذا الرمز صالح لمدة <b>5 دقائق</b> فقط.</p>
        </div>";

                await _emailService.SendEmailAsync(normalizedEmail, "رمز تأكيد الحساب - برواح", htmlMessage);

                return Ok(new
                {
                    status = "pending_verification",
                    message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني",
                    email = normalizedEmail,
                    expiresIn = 300
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء معالجة الطلب", details = ex.Message });
            }
        }

        #endregion

        #region Registration Step 2 - Verify OTP & Create Account

        [HttpPost("VerifyRegisterOtp")]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyOTP model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "البيانات المدخلة غير صحيحة" });

                var normalizedEmail = model.TargetEmail.Trim().ToLower();
                string key = $"Register:{normalizedEmail}";

                var isValid = _userService.VerifyOtp(key, model.Otp, out var registerModel, out var userTypeStr);

                if (!isValid || registerModel == null || string.IsNullOrEmpty(userTypeStr))
                    return BadRequest(new { status = "error", message = "رمز التحقق غير صحيح أو انتهت صلاحيته" });

                User user = new User
                {
                    UserName = normalizedEmail,
                    Email = normalizedEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                };
                string userPassword = string.Empty;

                //لليوزر
                if (userTypeStr == UserType.User.ToString() && registerModel is CustomerRegister customerDto)
                {
                    user.FirstName = customerDto.FirstName.Trim();
                    user.SecondName = customerDto.SecondName.Trim();
                    user.ThirdName = customerDto.ThirdName.Trim();
                    user.LastName = customerDto.LastName.Trim();
                    user.PhoneNumber = customerDto.PhoneNumber.Trim();
                    user.Address = customerDto.Goverment.Trim();
                    user.UserType = UserType.User;
                    userPassword = customerDto.Password;
                }
                // المتجر او البقاله
                else if (userTypeStr == UserType.Merchant.ToString() && registerModel is MerchantRegister merchantDto)
                {
                    user.FirstName = merchantDto.FirstName.Trim();
                    user.SecondName = merchantDto.SecondName.Trim();
                    user.ThirdName = merchantDto.ThirdName.Trim();
                    user.LastName = merchantDto.LastName.Trim();
                    user.PhoneNumber = merchantDto.PhoneNumber.Trim();
                    user.Address = merchantDto.Address.Trim();

                    user.BusinessName = merchantDto.BusinessName?.Trim();
                    user.BusinessType = merchantDto.BusinessType?.Trim();
                    user.TypeOfService = merchantDto.TypeOfService?.Trim();
                    user.BusinessDescription = merchantDto.BusinessDescription?.Trim();
                    user.DeliveryTimeSpan = merchantDto.DeliveryTimeSpan?.Trim();
                    user.MinimumOrder = merchantDto.MinimumOrder;
                    user.WorkHours = merchantDto.WorkHours?.Trim();
                    user.AcceptOrdersAutomatically = merchantDto.AcceptOrdersAutomatically;
                    user.BusinessSubCategoryIds = string.IsNullOrWhiteSpace(merchantDto.SubCategoryIds)
                        ? null
                        : string.Join(",", merchantDto.SubCategoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(x => int.TryParse(x, out var n) ? n : 0).Where(n => n > 0).Distinct());
                    if (!string.IsNullOrEmpty(merchantDto.ProfileImageUrl))
                        user.ProfileImageUrl = merchantDto.ProfileImageUrl;

                    if (merchantDto.Latitude.HasValue && merchantDto.Longitude.HasValue
                        && Berwah.BL.Service.MerchantService.IsValidLocation(merchantDto.Latitude.Value, merchantDto.Longitude.Value))
                    {
                        user.Latitude = merchantDto.Latitude.Value;
                        user.Longitude = merchantDto.Longitude.Value;
                    }

                    user.UserType = UserType.Merchant;
                    userPassword = merchantDto.Password;
                }
                //للدليفري
                else if (userTypeStr == UserType.Delivery.ToString() && registerModel is DeliveryRegister deliveryDto)
                {
                    user.FirstName = deliveryDto.FirstName.Trim();
                    user.SecondName = deliveryDto.SecondName.Trim();
                    user.ThirdName = deliveryDto.ThirdName.Trim();
                    user.LastName = deliveryDto.LastName.Trim();
                    user.PhoneNumber = deliveryDto.PhoneNumber.Trim();
                    user.Address = deliveryDto.Address.Trim();
                    user.VehicleType = deliveryDto.VehicleType.Trim();
                    user.PreviousExperience = deliveryDto.PreviousExperience?.Trim();
                    user.UserType = UserType.Delivery;
                    userPassword = deliveryDto.Password;

                    user.ProfileImageUrl = deliveryDto.PersonalImageUrl;
                    user.NationalIdImageUrl = deliveryDto.NationalIdImageUrl;
                    user.DrivingLicenseImageUrl = deliveryDto.DrivingLicenseImageUrl;
                    user.CriminalRecordImageUrl = deliveryDto.CriminalRecordImageUrl;
                    user.VehicleImageUrl = deliveryDto.VehicleImageUrl;
                }
                else
                {
                    return BadRequest(new { status = "error", message = "نوع البيانات غير مطابق" });
                }

                var result = await _userManager.CreateAsync(user, userPassword);
                if (!result.Succeeded)
                    return BadRequest(new { status = "error", message = "فشل إنشاء الحساب", errors = result.Errors.Select(e => e.Description) });

                if (!await _roleManager.RoleExistsAsync(userTypeStr))
                    await _roleManager.CreateAsync(new Role { Name = userTypeStr });

                await _userManager.AddToRoleAsync(user, userTypeStr);


                if (registerModel is DeliveryRegister delDto)
                {

                    if (delDto.PersonalImage != null)
                    {
                        var personalImgEntity = await SaveImageToCloudinaryAsync(delDto.PersonalImage, user.Id);
                        if (personalImgEntity != null)
                        {
                            user.ProfileImageUrl = personalImgEntity.Url;
                            await _imageRepository.AddAsync(personalImgEntity);
                        }
                    }


                    if (delDto.NationalIdImage != null)
                    {
                        var natImgEntity = await SaveImageToCloudinaryAsync(delDto.NationalIdImage, user.Id);
                        if (natImgEntity != null)
                        {
                            user.NationalIdImageUrl = natImgEntity.Url;
                            await _imageRepository.AddAsync(natImgEntity);
                        }
                    }


                    if (delDto.DrivingLicenseImage != null)
                    {
                        var licImgEntity = await SaveImageToCloudinaryAsync(delDto.DrivingLicenseImage, user.Id);
                        if (licImgEntity != null)
                        {
                            user.DrivingLicenseImageUrl = licImgEntity.Url;
                            await _imageRepository.AddAsync(licImgEntity);
                        }
                    }


                    if (delDto.CriminalRecordImage != null)
                    {
                        var crimImgEntity = await SaveImageToCloudinaryAsync(delDto.CriminalRecordImage, user.Id);
                        if (crimImgEntity != null)
                        {
                            user.CriminalRecordImageUrl = crimImgEntity.Url;
                            await _imageRepository.AddAsync(crimImgEntity);
                        }
                    }

                    if (delDto.VeicleImage != null)
                    {
                        var vehImgEntity = await SaveImageToCloudinaryAsync(delDto.VeicleImage, user.Id);
                        if (vehImgEntity != null)
                        {
                            user.VehicleImageUrl = vehImgEntity.Url;
                            await _imageRepository.AddAsync(vehImgEntity);
                        }
                    }


                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();
                }

                var token = _jwtService.CreateToken(user, new[] { userTypeStr });

                var refreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    Expires = DateTime.UtcNow.AddDays(7), 
                    UserId = user.Id
                };

                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message = "تم إنشاء الحساب بنجاح",
                    token = token,
                    refreshToken = refreshToken.Token, 
                    data = new
                    {
                        userId = user.Id,
                        email = user.Email,
                        name = $"{user.FirstName} {user.LastName}".Trim(),
                        role = userTypeStr
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء التحقق", details = ex.Message });
            }
        }
        
        #endregion

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.RefreshToken))
                    return BadRequest(new { status = "error", message = "البيانات المدخلة غير صحيحة" });

                var storedRefreshToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRole)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(rt => rt.Token == model.RefreshToken);

                if (storedRefreshToken == null || !storedRefreshToken.IsActive)
                    return Unauthorized(new { status = "error", message = "جلسة العمل منتهية، يرجى تسجيل الدخول من جديد" });

                var user = storedRefreshToken.User;

                var roles = await _userManager.GetRolesAsync(user);

                var newAccessToken = _jwtService.CreateToken(user, roles);

                storedRefreshToken.Revoked = DateTime.UtcNow;

                var newRefreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                };

                await _context.RefreshTokens.AddAsync(newRefreshToken);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message = "تم تجديد الـ Token بنجاح",
                    token = newAccessToken,
                    refreshToken = newRefreshToken.Token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء تجديد الـ Token", details = ex.Message });
            }
        }

        #region Login & Logout

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "البيانات المدخلة غير صحيحة", errors = ModelState.Values.SelectMany(v => v.Errors) });

                var input = model.Email.Trim();

                var user = IsValidEmail(input)
                    ? await _userManager.FindByNameAsync(input.ToLower())
                    : await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == input);

                if (user == null)
                    return Unauthorized(new { status = "error", message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (!result.Succeeded)
                    return Unauthorized(new { status = "error", message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });

                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.CreateToken(user, roles);

                var refreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    Expires = DateTime.UtcNow.AddDays(7), 
                    UserId = user.Id
                };

                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message = "تم تسجيل الدخول بنجاح",
                    token = token,
                    refreshToken = refreshToken.Token, 
                    data = new
                    {
                        userId = user.Id,
                        email = user.Email,
                        name = $"{user.FirstName} {user.LastName}".Trim(),
                        role = roles.FirstOrDefault(),
                        lastLogin = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء تسجيل الدخول", details = ex.Message });
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok(new { status = "success", message = "تم تسجيل الخروج بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء تسجيل الخروج", details = ex.Message });
            }
        }

        #endregion

        #region Forgot Password Workflow

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPas model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "البيانات المدخلة غير صحيحة" });

                var input = model.TargetEmail.Trim();

                var user = IsValidEmail(input)
                    ? await _userManager.FindByNameAsync(input.ToLower())
                    : await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == input);

                if (user == null)
                    return NotFound(new { status = "error", message = "المستخدم غير موجود" });

                string key = $"ResetPassword:{user.Email!.Trim().ToLower()}";
                var otp = _userService.GenerateAndSaveOtp(key, null, null);

                if (model.SendVie.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    string htmlMessage = $@"
                    <div style='direction: rtl; font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #333;'>رمز إعادة تعيين كلمة المرور</h2>
                        <p>رمز التحقق الخاص بك هو:</p>
                        <div style='background: white; padding: 15px; border-radius: 5px; text-align: center;'>
                            <b style='font-size: 24px; color: #6c5ce7; letter-spacing: 5px;'>{otp}</b>
                        </div>
                        <p style='color: #666;'>هذا الرمز صالح لمدة <b>5 دقائق</b> فقط.</p>
                        <p style='color: #999; font-size: 12px;'>إذا لم تطلب هذا الرمز، يرجى تجاهل هذه الرسالة.</p>
                    </div>";

                    await _emailService.SendEmailAsync(user.Email, "رمز إعادة تعيين كلمة المرور - برواح", htmlMessage);
                }

                return Ok(new { status = "success", message = "تم إرسال رمز التحقق بنجاح", email = user.Email });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء معالجة الطلب", details = ex.Message });
            }
        }

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOTP model)
        {
            try
            {
                var normalizedEmail = model.TargetEmail.Trim().ToLower();
                string key = $"ResetPassword:{normalizedEmail}";
                var isValid = _userService.VerifyOtp(key, model.Otp, out _, out _);

                if (!isValid)
                    return BadRequest(new { status = "error", message = "رمز التحقق غير صحيح أو انتهت صلاحيته" });

                var user = await _userManager.FindByNameAsync(normalizedEmail);
                if (user == null)
                    return NotFound(new { status = "error", message = "المستخدم غير موجود" });

                // إنشاء Reset Token مؤقت لإكماله في خطوة ResetPassword للأمان
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                return Ok(new { status = "success", message = "تم التحقق بنجاح", resetToken = resetToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء التحقق", details = ex.Message });
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPass model)
        {
            try
            {
                var normalizedEmail = model.TargetEmail.Trim().ToLower();

                var user = await _userManager.FindByNameAsync(normalizedEmail)
                           ?? await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.TargetEmail.Trim());

                if (user == null)
                    return NotFound(new { status = "error", message = "المستخدم غير موجود" });

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (!result.Succeeded)
                    return BadRequest(new { status = "error", message = "فشل تغيير كلمة المرور", errors = result.Errors.Select(e => e.Description) });

                return Ok(new { status = "success", message = "تم تغيير كلمة المرور بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء تغيير كلمة المرور", details = ex.Message });
            }
        }

        #endregion

        #region Profile & Dashboard Endpoints

        [Authorize]
        [HttpGet("UserProfile")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new { status = "error", message = "المستخدم غير موجود" });

                
                var activeAdsCount = await _context.Advertisements
                    .CountAsync(a => a.UserId == user.Id && a.IsActive);

                
                var latestAds = await _context.Advertisements
                    .Where(a => a.UserId == user.Id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .Select(a => new AdSummaryDto
                    {
                        AdId = a.Id,
                        Title = a.Title,
                        Price = a.Price,
                        ImageUrl = a.AdvertismentImages
                            .Select(img => img.Image.Url)
                            .FirstOrDefault(), 
                        PostedDate = a.CreatedAt
                    }).ToListAsync();

                var userImage = await _context.Images.FirstOrDefaultAsync(i => i.UserId == user.Id);

                
                var arabicCulture = new System.Globalization.CultureInfo("ar-EG");
                var joinedDateFormatted = user.CreatedAt.ToString("MMMM yyyy", arabicCulture);

                var dashboardDto = new UserDashboardDto
                {
                    UserId = user.Id.ToString(),
                    FullName = $"{user.FirstName} {user.SecondName} {user.ThirdName} {user.LastName}".Trim(),
                    ProfileImageUrl = userImage?.Url,
                    Address = user.Address,
                    JoinedDateText = $"عضو منذ {joinedDateFormatted}",
                    ActiveAdsCount = activeAdsCount,
                    ViewsToday = 0, 
                    Rating = user.Rating,
                    LatestAds = latestAds
                };

                return Ok(new { status = "success", data = dashboardDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);
                if (user == null) return NotFound();

                if (!string.IsNullOrEmpty(model.FirstName)) user.FirstName = model.FirstName.Trim();
                if (!string.IsNullOrEmpty(model.SecondName)) user.SecondName = model.SecondName.Trim();
                if (!string.IsNullOrEmpty(model.ThirdName)) user.ThirdName = model.ThirdName.Trim();
                if (!string.IsNullOrEmpty(model.LastName)) user.LastName = model.LastName.Trim();
                if (!string.IsNullOrEmpty(model.Address)) user.Address = model.Address.Trim();

                // رفع الصورة باستخدام الـ Overload الجديدة الخاصة بـ IFormFile
                if (model.ProfileImage != null)
                {
                    var imageEntity = await SaveImageToCloudinaryAsync(model.ProfileImage, user.Id);
                    if (imageEntity != null)
                    {
                        var oldImage = await _context.Images.FirstOrDefaultAsync(i => i.UserId == user.Id);
                        if (oldImage != null)
                            _context.Images.Remove(oldImage);

                        await _imageRepository.AddAsync(imageEntity);
                    }
                }

                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();

                return Ok(new { status = "success", message = "تم تحديث البيانات بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        #endregion

        #region Settings & Preferences Endpoints

        [Authorize]
        [HttpGet("Settings")]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);
                if (user == null) return NotFound();

                var userImage = await _context.Images.FirstOrDefaultAsync(i => i.UserId == user.Id);

                var settingsDto = new UserSettingDto
                {
                    FullName = $"{user.FirstName} {user.SecondName} {user.ThirdName} {user.LastName}".Trim(),
                    ProfileImageUrl = userImage?.Url,
                    Address = string.IsNullOrEmpty(user.Address) ? "لم تتم إضافة العنوان بعد" : user.Address,
                    Language = user.language ?? "العربية",
                    HidePhoneNumber = user.HidePhoneNumber,
                    EnableNotifications = user.PushNotificationEnable,
                    LastPasswordChangeText = user.LastPasswordChangeDate.HasValue
                        ? $"آخر تحديث {GetTimeAgo(user.LastPasswordChangeDate.Value)}"
                        : "لم يتم التغيير مؤخراً"
                };

                return Ok(new { status = "success", data = settingsDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest(new { status = "error", message = "كلمات المرور غير متطابقة" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { status = "error", message = "كلمة المرور الحالية غير صحيحة أو الجديدة لا تستوفي الشروط", errors = result.Errors.Select(e => e.Description) });

            user.LastPasswordChangeDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new { status = "success", message = "تم تغيير كلمة المرور بنجاح" });
        }

        [Authorize]
        [HttpPatch("UpdatePreferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            if (model.HidePhoneNumber.HasValue) user.HidePhoneNumber = model.HidePhoneNumber.Value;
            if (model.PushNotificationsEnabled.HasValue) user.PushNotificationEnable = model.PushNotificationsEnabled.Value;
            if (!string.IsNullOrEmpty(model.Language)) user.language = model.Language;

            await _userManager.UpdateAsync(user);

            return Ok(new { status = "success", message = "تم تحديث التفضيلات بنجاح" });
        }

        [Authorize]
        [HttpPost("FreezeAccount")]
        public async Task<IActionResult> FreezeAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            user.IsFrozen = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { status = "success", message = "تم تجميد الحساب بنجاح" });
        }

        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();


            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { status = "error", message = "فشل حذف الحساب" });

            return Ok(new { status = "success", message = "تم حذف الحساب نهائياً" });
        }

        [Authorize]
        [HttpGet("export-data")]
        public async Task<IActionResult> ExportUserData()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { status = "fail", message = "غير مصرح لك بالوصول" });
            }

            var fileBytes = await _userService.ExportUserDataAsExcelAsync(userId);

            var fileName = $"UserData_Archive_{userId}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        #endregion
        #region Transactions & Orders Endpoints
        [Authorize]
        [HttpGet("MyTransactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // 1. جلب بيانات الطلبات المحددة فقط وتفادي مشكلة الـ Nulls في الجدول
                var orders = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.UserId == userId)
                    .Select(o => new
                    {
                        o.Id,
                        o.ConditionStaus,
                        o.TotalPrice,
                        o.CreatedAt
                    })
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var orderRows = orders.Select(o => (
                    CreatedAt: o.CreatedAt,
                    Row: new UserTransactionDto
                    {
                        OrderId = o.Id,
                        Title = $"طلب منتجات أو خدمات #{o.Id}",
                        Status = string.IsNullOrEmpty(o.ConditionStaus) ? "قيد المعالجة" : o.ConditionStaus,
                        Type = "Order",
                        TotalPrice = (decimal)o.TotalPrice,
                        TransactionDate = o.CreatedAt.ToString("yyyy-MM-dd hh:mm tt")
                    }
                ));

                // 2. جلب الحركات المالية (Refunds)
                var transactions = await _context.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.Type == "Refund")
                    .Select(t => new
                    {
                        t.Id,
                        t.Description,
                        t.Amount,
                        t.CreatedAt
                    })
                    .ToListAsync();

                var refundRows = transactions.Select(t => (
                    CreatedAt: t.CreatedAt,
                    Row: new UserTransactionDto
                    {
                        OrderId = -t.Id,
                        Title = string.IsNullOrEmpty(t.Description) ? "استرداد أموال" : t.Description,
                        Status = "تم الاسترداد",
                        Type = "Refund",
                        TotalPrice = t.Amount,
                        TransactionDate = t.CreatedAt.ToString("yyyy-MM-dd hh:mm tt")
                    }
                ));

                // 3. الدمج والتنظيم النهائي
                var result = orderRows
                    .Concat(refundRows)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => x.Row)
                    .ToList();

                return Ok(new { status = "success", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("UpdateTransactionStatus/{orderId}")]
        public async Task<IActionResult> UpdateTransactionStatus(int orderId, [FromBody] UpdateTransactionStatusDto model)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { status = "error", message = "الطلب أو المعاملة غير موجودة" });

                order.ConditionStaus = model.NewStatus;
                await _context.SaveChangesAsync();

                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = model.TargetUserId,
                    Title = "تحديث معاملة 📦",
                    Message = $"تم تغيير حالة المعاملة الخاصة بك إلى: {model.NewStatus}"
                });

                return Ok(new { status = "success", message = "تم تحديث حالة المعاملة وإرسال الإشعار للطرف الآخر بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #endregion

        #region Payment Methods & Billing Endpoints
        [Authorize]
        [HttpGet("PaymentMethods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                // جلب معرف المستخدم الحالي من التوكن
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // جلب البطاقات الحقيقية الخاصة بالمستخدم من جدول VisaCard
                var cards = await _context.VisaCards
                    .Where(v => v.UserId == userId)
                    .Select(v => new PaymentMethodDto
                    {
                        Id = v.Id,
                        CardName = v.CardName,
                        CardNumber ="•••• " + v.CardNumber.Substring(v.CardNumber.Length - 4),
                        Expiration = v.Expiration,
                        IsDefault = v.IsDefault
                    })
                    .ToListAsync();

                return Ok(new { status = "success", data = cards });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("AddPaymentCard")]
        public async Task<IActionResult> AddPaymentCard([FromBody] AddVisaCardDto model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "البيانات المدخلة غير صحيحة" });


                if (model.IsDefault)
                {
                    var existingCards = await _context.VisaCards.Where(v => v.UserId == userId).ToListAsync();
                    foreach (var card in existingCards)
                    {
                        card.IsDefault = false;
                    }
                }
                var newVisa = new VisaCard
                {
                    CardName = model.CardName.Trim(),
                    CardNumber = model.CardNumber.Trim(),
                    Expiration = model.Expiration.Trim(),
                    IsDefault = model.IsDefault,
                    UserId = userId
                };

                _context.VisaCards.Add(newVisa);
                await _context.SaveChangesAsync();

                return Ok(new { status = "success", message = "تم حفظ وتأكيد وسيلة الدفع بنجاح", cardId = newVisa.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء حفظ البطاقة", details = ex.Message });
            }
        }
        #endregion
        #region Private Helper Methods
        private string GetTimeAgo(DateTime date)
        {
            var span = DateTime.UtcNow - date;
            if (span.Days > 30) return $"قبل {span.Days / 30} أشهر";
            if (span.Days > 0) return $"قبل {span.Days} أيام";
            return "اليوم";
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        #endregion
    }
}
   