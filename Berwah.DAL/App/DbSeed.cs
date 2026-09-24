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
            if (!await roleManager.RoleExistsAsync("SupervisorUsed"))
                await roleManager.CreateAsync(new Role { Name = "SupervisorUsed" });
            if (!await roleManager.RoleExistsAsync("SupervisorUsers"))
                await roleManager.CreateAsync(new Role { Name = "SupervisorUsers" });
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
    }
}
