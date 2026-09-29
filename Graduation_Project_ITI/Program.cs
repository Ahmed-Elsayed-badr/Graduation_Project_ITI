using BLL.Chat;
using BLL.Configuration;
using BLL.Services;
using BLL.Services.Interfaces;
using Graduation_Project_ITI.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC =====
builder.Services.AddControllersWithViews(options =>
{
    // Non-nullable reference types (e.g. Category.Comment) are not automatically [Required].
    // Required fields are declared explicitly with [Required] instead.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// ===== Database =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// ===== Repositories & Services =====
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ===== Groq Chatbot =====
var groqOptions = builder.Configuration.GetSection("Groq").Get<GroqOptions>() ?? new GroqOptions();
builder.Services.AddSingleton(groqOptions);

builder.Services.AddHttpClient<GroqClient>(client =>
{
    client.BaseAddress = new Uri(groqOptions.BaseUrl);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", groqOptions.ApiKey);
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddScoped<ChatService>();

var app = builder.Build();

// ===== Development only: apply migrations + insert demo data (runs once, only if there are no products) =====
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        DevDataSeeder.Seed(db);
    }
    catch (Exception ex)
    {
        // The app still starts so the problem (usually the SQL Server connection string) is visible in the log.
        logger.LogError(ex, "Database migration / seeding failed. Check ConnectionStrings:DefaultConnection in appsettings.json.");
    }
}

// ===== HTTP request pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
