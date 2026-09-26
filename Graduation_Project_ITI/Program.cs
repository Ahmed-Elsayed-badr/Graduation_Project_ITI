using Microsoft.EntityFrameworkCore;
using System;
using Graduation_Project_ITI;
using BLL.Chat;
using System.Net.Http.Headers;
using BLL.Configuration;
using Graduation_Project_ITI.Data;



var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection")
//    ));


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
builder.Services.AddDbContext<AppDbContext>();

var app = builder.Build();
// ===== بيانات تجريبية (في وضع التطوير فقط) =====
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DevDataSeeder.Seed(db);
}

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
