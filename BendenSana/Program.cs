using BendenSana.Models.Repositories;
using BendenSana.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
// Add services to the container.
builder.Services.AddControllersWithViews();

// add repository dependencies
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Otomatik migrate (geliþtirmede çok iþe yarar)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ------------------------------------------------------------
// BAÞLANGIÇ VERÝLERÝ (SEED DATA)
// Veritabaný boþsa otomatik kategori ekler.
// ------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // DbContext'i çaðýrýyoruz (Namespace'i kontrol et: BendenSana.Identity veya Data olabilir)
        var context = services.GetRequiredService<AppDbContext>();

        // Eðer veritabanýnda hiç kategori yoksa...
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Elektronik", ImageUrl = "/images/cats/elektronik.jpg" },
                new Category { Name = "Moda & Giyim", ImageUrl = "/images/cats/moda.jpg" },
                new Category { Name = "Ev & Yaþam", ImageUrl = "/images/cats/ev.jpg" },
                new Category { Name = "Spor & Outdoor", ImageUrl = "/images/cats/spor.jpg" },
                new Category { Name = "Hobi & Oyun", ImageUrl = "/images/cats/oyun.jpg" }
            );

            // Renkler yoksa ekle (Ürün eklerken ColorId sorarsa diye)
            if (!context.Colors.Any())
            {
                context.Colors.AddRange(
                    new Color { Name = "Siyah", HexCode = "#000000" },
                    new Color { Name = "Beyaz", HexCode = "#FFFFFF" },
                    new Color { Name = "Kýrmýzý", HexCode = "#FF0000" },
                    new Color { Name = "Mavi", HexCode = "#0000FF" }
                );
            }

            context.SaveChanges(); // Veritabanýna iþle
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanýna baþlangýç verileri eklenirken bir hata oluþtu.");
    }

    // ... Kategori ekleme kodlarý bittiði yerin altý ...

    // KULLANICI SEED (Baþlangýç Kullanýcýsý Oluþturma)
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>(); // ApplicationUser veya IdentityUser

    // Eðer veritabanýnda hiç kullanýcý yoksa...
    if (!userManager.Users.Any())
    {
        var demoUser = new ApplicationUser
        {
            UserName = "demo@sakarya.edu.tr",
            Email = "demo@sakarya.edu.tr",
            EmailConfirmed = true,
            // EKLENEN KISIMLAR:
            FirstName = "Demo",
            LastName = "User"
            // Eðer LastName de zorunluysa (ki genelde öyledir) onu da ekledik.
            // Hata devam ederse ApplicationUser sýnýfýna bakýp baþka zorunlu alan var mý kontrol ederiz.
        };

        userManager.CreateAsync(demoUser, "Sau.1234").Wait();
    }
}
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Rolleri Tanýmla (Eðer yoksa oluþturur)
        string[] roleNames = { "Admin", "Seller", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // ==========================================
        // 2. ADMIN KULLANICISI OLUÞTUR
        // ==========================================
        var adminEmail = "admin@bendensana.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Sistem",
                LastName = "Yöneticisi"
            };
            var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // ==========================================
        // 3. SATICI (SELLER) KULLANICISI OLUÞTUR
        // ==========================================
        var sellerEmail = "seller@bendensana.com";
        var sellerUser = await userManager.FindByEmailAsync(sellerEmail);
        if (sellerUser == null)
        {
            sellerUser = new ApplicationUser
            {
                UserName = sellerEmail,
                Email = sellerEmail,
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Satýcý"
            };
            var createResult = await userManager.CreateAsync(sellerUser, "Seller123!"); // Þifreye dikkat
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(sellerUser, "Seller");
            }
        }

        // ==========================================
        // 4. NORMAL KULLANICI (USER) OLUÞTUR
        // ==========================================
        var userEmail = "user@bendensana.com";
        var normalUser = await userManager.FindByEmailAsync(userEmail);
        if (normalUser == null)
        {
            normalUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Kullanýcý"
            };
            var createResult = await userManager.CreateAsync(normalUser, "User123!"); // Þifreye dikkat
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Roller ve kullanýcýlar oluþturulurken hata meydana geldi.");
    }
}


app.Run(); 