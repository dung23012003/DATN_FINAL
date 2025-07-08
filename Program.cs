using Microsoft.EntityFrameworkCore;
using ShopDongY.Data;
using ShopDongY.Models;

var builder = WebApplication.CreateBuilder(args);

// C?u hình DbContext
builder.Services.AddDbContext<ShopDongYContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});

// Thêm Session service
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();


// =========================
// ?? B??C 2: Kh?i t?o d? li?u Role m?c ??nh (Admin, Customer)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ShopDongYContext>();
    SeedRoles(context); // G?i hàm Seed
}
// =========================


// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Luôn ??t tr??c Authorization
app.UseAuthorization();

// Route cho khu v?c (Area)
app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Route m?c ??nh
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


// ? Hàm kh?i t?o d? li?u Role
static void SeedRoles(ShopDongYContext context)
{
    if (!context.Roles.Any())
    {
        var roles = new List<RoleModel>
        {
            new RoleModel { RoleName = "Admin" },
            new RoleModel { RoleName = "Customer" }
        };

        context.Roles.AddRange(roles);
        context.SaveChanges();
    }
}
