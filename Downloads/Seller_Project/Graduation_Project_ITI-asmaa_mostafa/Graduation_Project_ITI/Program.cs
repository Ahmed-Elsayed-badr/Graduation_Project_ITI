using BLL.Chat;
using BLL.Configuration;
using BLL.ReviewImplementation;
using BLL.ReviewInter;
using BLL.SellerOrderImplementation;
using BLL.Services;
using BLL.Services.Interfaces;
using DAL;
using Graduation_Project_ITI.Data;
using Graduation_Project_ITI.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews(options =>
{
    // Non-nullable reference types (e.g. Category.Comment)
    // are not automatically [Required].
    // Required fields are declared explicitly with [Required].
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});


// ============================================================
// Database
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found in appsettings.json."
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
);


// ============================================================
// Seller Orders & Reviews
// ============================================================

builder.Services.AddScoped<ISellerOrderInterface, SellerOrderImplement>();

builder.Services.AddScoped<IReviewInterface, ReviewImplement>();


// ============================================================
// Repositories & Services
// ============================================================

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();


// ============================================================
// Groq Chatbot
// ============================================================

var groqOptions =
    builder.Configuration
        .GetSection("Groq")
        .Get<GroqOptions>()
    ?? new GroqOptions();

builder.Services.AddSingleton(groqOptions);


builder.Services.AddHttpClient<GroqClient>(client =>
{
    client.BaseAddress = new Uri(groqOptions.BaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            groqOptions.ApiKey
        );

    client.Timeout = TimeSpan.FromSeconds(60);
});


builder.Services.AddScoped<ChatService>();


// ============================================================
// Password Hasher
// ============================================================

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


// ============================================================
// Authentication
// ============================================================

builder.Services
    .AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        options.SlidingExpiration = true;
    });


// ============================================================
// Authorization
// ============================================================

builder.Services.AddAuthorization();


var app = builder.Build();


// ============================================================
// Development
// Apply migrations + insert demo data
// ============================================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var logger =
        scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();

    try
    {
        var db =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        db.Database.Migrate();

        DevDataSeeder.Seed(db);
    }
    catch (Exception ex)
    {
        // The app still starts so the problem
        // is visible in the log.
        logger.LogError(
            ex,
            "Database migration / seeding failed. " +
            "Check ConnectionStrings:DefaultConnection in appsettings.json."
        );
    }
}


// ============================================================
// HTTP Request Pipeline
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // The default HSTS value is 30 days.
    app.UseHsts();
}


// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();


// ============================================================
// Static Files
// ============================================================

app.UseStaticFiles();


// ============================================================
// Routing
// ============================================================

app.UseRouting();


// ============================================================
// Authentication
// IMPORTANT: Must come before UseAuthorization()
// ============================================================

app.UseAuthentication();


// ============================================================
// Authorization
// ============================================================

app.UseAuthorization();


// ============================================================
// MVC Default Route
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Register}/{id?}"
);


// ============================================================
// Run Application
// ============================================================

app.Run();