using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using NurseCourse.Models;
using NurseCourse.Services;
using NurseCourse.Support;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
builder.Services.AddDbContext<ModularCourseDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("connection")
    )
);

builder.Services.AddControllersWithViews();
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
})
.WithAccessToken(options =>
{
    options.Audience = builder.Configuration["Auth0:Audience"];
    options.Scope = "openid profile email"; 
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Custom";
    options.DefaultChallengeScheme = "Custom";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = $"https://{Configuration["Auth0:Domain"]}";
    options.Audience = Configuration["Auth0:Audience"];

    // Mapeo del claim personalizado de roles al claim estándar de roles de ASP.NET
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
})
.AddPolicyScheme("Custom", "Custom", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        string authorization = context.Request.Headers[HeaderNames.Authorization];
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            return JwtBearerDefaults.AuthenticationScheme;
        return "Auth0";
    };
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; 
});
builder.Services.ConfigureSameSiteNoneCookies();
builder.Services.AddHttpClient();
builder.Services.AddScoped<Auth0Service>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});
builder.Services.AddDbContext<ModularCourseDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSession();

app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();