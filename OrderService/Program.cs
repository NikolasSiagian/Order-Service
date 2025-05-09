using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderService.Context;
using OrderService.Data;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
var userManagementConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var OrderServiceConnection = builder.Configuration.GetConnectionString("OrderServiceContext")
    ?? throw new InvalidOperationException("Connection string 'OrderServiceContext' not found.");

// **Tambahkan DbContext untuk ASP.NET Identity (UserManagement)**
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(userManagementConnection));

// **Tambahkan DbContext untuk data akademik (OrderService)**
builder.Services.AddDbContext<OrderServiceContext>(options =>
    options.UseSqlServer(OrderServiceConnection));

// **Tambahkan Identity menggunakan ApplicationDbContext**
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();


app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();


app.Run();
