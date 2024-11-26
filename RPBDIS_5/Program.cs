using Microsoft.EntityFrameworkCore;
using RPBDIS_5.Data;
using RPBDIS_5.Middlewares;
using Microsoft.AspNetCore.Identity;
using RPBDIS_5.Areas.Identity.Data;
using RPBDIS_5.Areas.Identity.Models;

var builder = WebApplication.CreateBuilder(args);

// ���������� �������� ��� ASP.NET Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection"))); // ��� SQLite

// ��������� ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
// ���������� DbContext � �������������� ������ ����������� �� ������������ (appsettings.json)
builder.Services.AddDbContext<MonitoringContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ��������� ��������� ������������ � �������������
builder.Services.AddControllersWithViews();

// ��������� ��������� ������ (���� ���������)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // ����� ����� ������
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Middleware ��� ��������� ������
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ��������� ������
app.UseSession();

// ��������� �������������
app.UseRouting();

builder.Services.AddHttpContextAccessor();
app.UseAuthentication();
app.UseAuthorization();

// Вызов инициализации пользователей и ролей
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Вызов метода инициализации
    await DbUserInitializer.Initialize(services);
}

// ����������� ������������ �������� ��� MVC (�� ���������)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ��������� Razor Pages (��������, ��� ����� � �����������)
app.MapRazorPages();

app.Run();
