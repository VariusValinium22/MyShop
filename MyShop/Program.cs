using Microsoft.EntityFrameworkCore;
using MyShop.MyHelpers;

var builder = WebApplication.CreateBuilder(args);

// Register DBContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
//    options.UseSqlServer(connectionString);
//});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(36000);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();
