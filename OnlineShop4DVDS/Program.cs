using Authorize;
using DataAccess.Data;
using DataAccess.DbInitializer;
using DataAccess.Repository.IRepository;
using DataAccess.Repository;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ultility;
using Stripe;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectDB"));
});

//Stripe service injection
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// DI for Identity and connect it with DBContext
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configure password validation
    options.Password.RequireDigit = true; // Change to false to not require a digit
    options.Password.RequiredLength = 6; // Change minimum length to 6
    options.Password.RequireNonAlphanumeric = false; // Change to false to not require non-alphanumeric characters
    options.Password.RequireUppercase = false; // Change to false to not require uppercase letters
    options.Password.RequireLowercase = true; // Require at least one lowercase letter
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //Time before unlock account when reaching the max fail attemps
    options.Lockout.MaxFailedAccessAttempts = 3; //Lock account when reaching the max fail attemps

}).AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
	options.ValidationInterval = TimeSpan.Zero; // enables immediate logout, after updating the user's stat
});

// Path for Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Customer/Home/Index";
    options.AccessDeniedPath = "/Admin/User/AccessDenied";
});

// Add HttpContextAccessor service
builder.Services.AddHttpContextAccessor();

// Add Authorization Handler
builder.Services.AddSingleton<IAuthorizationHandler, AdminOrSelfHandler>();

// Authorization Configuration
builder.Services.AddAuthorization(options =>
{
    // Custom create policy
    // Custom edit policy
    options.AddPolicy("AdminOrCreatePermission", policy => policy.RequireAssertion(context => context.User.IsInRole(StaticDetail.Role_Admin) || context.User.HasClaim(c => c.Type == "Create" && c.Value == "Create")));

    // Custom edit policy
    options.AddPolicy("AdminOrEditPermission", policy => policy.RequireAssertion(context => context.User.IsInRole(StaticDetail.Role_Admin) || context.User.HasClaim(c => c.Type == "Edit" && c.Value == "Edit")));

    // Custom delete policy
    options.AddPolicy("AdminOrDeletePermission", policy => policy.RequireAssertion(context => context.User.IsInRole(StaticDetail.Role_Admin) || context.User.HasClaim(c => c.Type == "Delete" && c.Value == "Delete")));

    // Custom not customer policy
    options.AddPolicy("NotCustomerPermission", policy => policy.RequireAssertion(context => context.User.Identity.IsAuthenticated && !context.User.IsInRole(StaticDetail.Role_Customer)));

    // Customer policy with class file
    options.AddPolicy("AdminOrOnlySelfPermission", policy => policy.Requirements.Add(new AdminOrSelfRequirement()));
});

//IEmailSender Configuration
builder.Services.AddTransient<IEmailSender, MailJetEmailSender>();

//Google Login Configuration
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientID").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
    options.Events = new OAuthEvents
    {
        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/Customer/Account/ExternalLoginCallBack?remoteError=" + context.Failure.Message);
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

//Add DbInitializer
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDistributedMemoryCache();// Để Lưu lại cache

builder.Services.AddSession(options => //Set time cho session
{
    options.IdleTimeout = TimeSpan.FromHours(240);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
});
//===================================================== cấu hình dung lượng
// Configure the limit for multipart/form-data requests
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 22147483648; // 2 GB
});

// Configure Kestrel server options
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 22147483648; // 2 GB
});

//=====================================================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:Secretkey").Get<string>();

app.UseSession();

// Chạy hình ảnh tĩnh
app.UseStaticFiles();

app.UseRouting();

// Add Authentication
app.UseAuthentication();

app.UseAuthorization();


//Add IDbInitializer
SeedDatabase();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");


app.Run();

// Call IDbInitializer
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}
