using Berwah.BL.Service;
using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository;
using Berwah.DAL.Repository.IRepository;
using Berwah.Hubs;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ThanyaProject.Services;


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ربط إعدادات الإيميل و Cloudinary
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSetting"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

#region Add DI
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<Berwah.Services.ServiceJobAutoConfirm>();
builder.Services.AddHostedService<Berwah.Services.CaptainLicenseReminder>();
builder.Services.AddHostedService<Berwah.Services.NotificationCleanup>();
builder.Services.AddSingleton<Berwah.Services.FcmSender>();
builder.Services.AddSingleton<INotificationPusher, Berwah.Services.SignalRNotificationPusher>();
builder.Services.AddScoped<ISupporterService, SupporterService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IAdvertismentService, AdvertisementService>();
builder.Services.AddScoped<IAdvertisementDetailService, AdvertisementDetailService>();
builder.Services.AddScoped<Berwah.Services.WalletService>();
builder.Services.AddScoped<Berwah.Services.AdPromotionService>();
builder.Services.AddSingleton<Berwah.Services.GeoZoneService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupervisorForAdService, SupervisorForAdService>();
builder.Services.AddScoped<ISupervisorForSRService, SupervisorForSRService>();
builder.Services.AddScoped<ISupervisorUsedService, SupervisorUsedService>();


builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<ISupporterRepository, SupporterRepository>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISupervisorForAdRepository, SupervisorForAdRepository>();
builder.Services.AddScoped<ISupervisorForSR, SupervisorForSRRepository>();
builder.Services.AddScoped<ISupervisorUsedRepository, SupervisorUsedRepository>();


builder.Services.AddSignalR();

builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 9;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
#endregion

var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("Key")!);

// 2. إعدادات Authentication الاعتمادية على Authorization Header
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
    // SignalR cannot set a header from the app's websocket: it sends the token in the query.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/notificationHub"))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200",
                "https://browah-elmazory.vercel.app/"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Berwah API", Version = "v2" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "ضع التوكن مباشرة هنا دون كتابة كلمة Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await DbSeed.SeedRolesAsync(roleManager);
    await DbSeed.SeedAdminAsync(userManager);
    await DbSeed.SeedSupervisorsAsync(userManager);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseMiddleware<Berwah.Localization.EnglishResponseMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/orderHub");
app.MapHub<NotificationHub>("/notificationHub").RequireAuthorization();
app.MapHub<Berwah.Hubs.ChatHub>("/chatHub");

app.Run();