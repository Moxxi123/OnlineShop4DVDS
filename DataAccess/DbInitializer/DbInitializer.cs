using DataAccess.Data;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ultility;

namespace DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DatabaseContext _dbContext;

        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, DatabaseContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public void Initialize()
        {
            // Auto apply pending migrations
            try
            {
                if (_dbContext.Database.GetPendingMigrations().Any())
                {
                    _dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Handle exception appropriately (log it, etc.)
            }

            // Create roles and claims if they don't exist
            if (!_roleManager.RoleExistsAsync(StaticDetail.Role_Admin).GetAwaiter().GetResult())
            {
                CreateRoleWithClaims(StaticDetail.Role_Admin, new[] { "Create", "Edit", "Delete" }).GetAwaiter().GetResult();
                CreateRoleWithClaims(StaticDetail.Role_Employee, new[] { "Create", "Edit" }).GetAwaiter().GetResult();
                CreateRoleWithClaims(StaticDetail.Role_Customer, Array.Empty<string>()).GetAwaiter().GetResult();

                // Create admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Name = "admin"
                };
                _userManager.CreateAsync(adminUser, "Admin123").GetAwaiter().GetResult();

                // Mail confirm for admin
                var tokenConfirm = _userManager.GenerateEmailConfirmationTokenAsync(adminUser).GetAwaiter().GetResult();
                _userManager.ConfirmEmailAsync(adminUser, tokenConfirm).GetAwaiter().GetResult();

                // Assign admin role to admin user
                adminUser = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@admin.com");
                if (adminUser != null)
                {
                    _userManager.AddToRoleAsync(adminUser, StaticDetail.Role_Admin).GetAwaiter().GetResult();
                }
            }

            // Ensure the admin user exists and is assigned the admin role
            if (_userManager.FindByEmailAsync("admin@admin.com").GetAwaiter().GetResult() == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Name = "admin"
                };
                _userManager.CreateAsync(adminUser, "Admin123").GetAwaiter().GetResult();

                // Mail confirm for admin
                var tokenConfirm = _userManager.GenerateEmailConfirmationTokenAsync(adminUser).GetAwaiter().GetResult();
                _userManager.ConfirmEmailAsync(adminUser, tokenConfirm).GetAwaiter().GetResult();

                adminUser = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@admin.com");
                if (adminUser != null)
                {
                    _userManager.AddToRoleAsync(adminUser, StaticDetail.Role_Admin).GetAwaiter().GetResult();
                }
            }

            // Add default content type
            if (!_dbContext.ContentTypes.Any(c => c.Type.ToLower() == StaticDetail.ContentType_Free.ToLower()))
            {
                _dbContext.ContentTypes.AddAsync(new ContentType { Type = StaticDetail.ContentType_Free, Status = false }).GetAwaiter().GetResult();
                _dbContext.SaveChangesAsync().GetAwaiter().GetResult();
            }

            if (!_dbContext.ContentTypes.Any(c => c.Type.ToLower() == StaticDetail.ContentType_Paid.ToLower()))
            {
                _dbContext.ContentTypes.AddAsync(new ContentType { Type = StaticDetail.ContentType_Paid, Status = false }).GetAwaiter().GetResult();
                _dbContext.SaveChangesAsync().GetAwaiter().GetResult();
            }

            // Add default promotion
            if (!_dbContext.Promotions.Any(p => p.Description == "Default"))
            {
                _dbContext.Promotions.AddAsync(new Promotion
                {
                    Description = "Default",
                    DiscountPercent = 0,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.MaxValue,
                    Status = StaticDetail.PromotionStatus_Active
                }).GetAwaiter().GetResult();
                _dbContext.SaveChangesAsync().GetAwaiter().GetResult();
            }
        }

        private async Task CreateRoleWithClaims(string roleName, string[] claims)
        {
            var role = new ApplicationRole { Name = roleName, Status = true };
            await _roleManager.CreateAsync(role);

            foreach (var claim in claims)
            {
                await _roleManager.AddClaimAsync(role, new Claim(claim, claim));
            }
        }
    }
}
