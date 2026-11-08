using Berwah.Models.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.App
{
    public class DbSeed
    {
        public static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new Role { Name = "Admin" });
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new Role { Name = "User" });
            if (!await roleManager.RoleExistsAsync("Merchant"))
                await roleManager.CreateAsync(new Role { Name = "Merchant" });
            if (!await roleManager.RoleExistsAsync("Accountant"))
                await roleManager.CreateAsync(new Role { Name = "Accountant" });
            if (!await roleManager.RoleExistsAsync("Supporter"))
                await roleManager.CreateAsync(new Role { Name = "Supporter" });
            if (!await roleManager.RoleExistsAsync("SupervisorForMerchant"))
                await roleManager.CreateAsync(new Role { Name = "SupervisorForMerchant" });
            if (!await roleManager.RoleExistsAsync("SupervisorUsed"))
                await roleManager.CreateAsync(new Role { Name = "SupervisorUsed"});
            if (!await roleManager.RoleExistsAsync("SupervisorForAd"))
                await roleManager.CreateAsync(new Role { Name = "SupervisorForAd" });
            if (!await roleManager.RoleExistsAsync("SupervisorForServices"))
                await roleManager.CreateAsync(new Role { Name = "SupervisorForServices" });
            if (!await roleManager.RoleExistsAsync("BossOfSupervisor"))
                await roleManager.CreateAsync(new Role { Name = "BossOfSupervisor" });
            if (!await roleManager.RoleExistsAsync("Helper"))
                await roleManager.CreateAsync(new Role { Name = "Helper" });
            if (!await roleManager.RoleExistsAsync("Delivery"))
                await roleManager.CreateAsync(new Role { Name = "Delivery" });
        }
        public static async Task SeedAdminAsync(UserManager<User> userManager)
        {
            var adminEmail = "admin@berwah.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                var newAdmin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,

                    FirstName = "Admin",
                    SecondName = "Admin",
                    ThirdName = "Ahmed",
                    LastName = "Browah",

                    PhoneNumber = "01116363262",
                    Address = "Alex",
                    UserType = Models.Enum.UserType.Admin
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@1234");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        public static async Task SeedSupervisorsAsync(UserManager<User> userManager)
        {
            // 1. مشرف الإعلانات
            var adSupervisorEmail = "ad@berwah.com";
            if (await userManager.FindByEmailAsync(adSupervisorEmail) == null)
            {
                var adSupervisor = new User
                {
                    UserName = adSupervisorEmail,
                    Email = adSupervisorEmail,
                    EmailConfirmed = true,
                    FirstName = "مشرف",
                    SecondName = "الإعلانات",
                    ThirdName = "الجديده",
                    LastName = "برواح",
                    PhoneNumber = "01000000001",
                    Address = "الرياض",
                    UserType = Models.Enum.UserType.Staff // أو حسب تصميمك
                };

                var result = await userManager.CreateAsync(adSupervisor, "Supervisor@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adSupervisor, "SupervisorForAd");
                }
            }

            // 2. مشرف الخدمات
            var serviceSupervisorEmail = "service@berwah.com";
            if (await userManager.FindByEmailAsync(serviceSupervisorEmail) == null)
            {
                var serviceSupervisor = new User
                {
                    UserName = serviceSupervisorEmail,
                    Email = serviceSupervisorEmail,
                    EmailConfirmed = true,
                    FirstName = "مشرف",
                    SecondName = "طلبات",
                    ThirdName = "الخدمات",
                    LastName = "برواح",
                    PhoneNumber = "01000000002",
                    Address = "الرياض",
                    UserType = Models.Enum.UserType.Staff
                };

                var result = await userManager.CreateAsync(serviceSupervisor, "Supervisor@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(serviceSupervisor, "SupervisorForServices"); 
                }
            }

            // 3. مشرف المستعمل
            var usedSupervisorEmail = "useds@berwah.com";
            if (await userManager.FindByEmailAsync(usedSupervisorEmail) == null)
            {
                var usedSupervisor = new User
                {
                    UserName = usedSupervisorEmail,
                    Email = usedSupervisorEmail,
                    EmailConfirmed = true,
                    FirstName = "مشرف",
                    SecondName = "المنتاجات",
                    ThirdName = "المستعملة",
                    LastName = "برواح",
                    PhoneNumber = "01111111111",
                    Address = "الرياض",
                    UserType = Models.Enum.UserType.Staff
                };

                var result = await userManager.CreateAsync(usedSupervisor, "Supervisor@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(usedSupervisor, "SupervisorUsed");
                }
            }
        }
    }
}
