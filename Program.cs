using Microsoft.EntityFrameworkCore;
using RailGo.Data;
using RailGo.Services;

var builder = WebApplication.CreateBuilder(args);

// Conectarea aplicației la baza de date SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adăugăm MVC în aplicație
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
// Introducem date demo în baza de date
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DataSeeder.Seed(context);
}

// Configurare aplicație
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Ruta principală a aplicației
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Trips}/{action=Search}/{id?}");

app.Run();