using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Part2_CMCS.Data;
using Part2_CMCS.Hubs;

var builder = WebApplication.CreateBuilder(args);

// -------------------
// SERVICES
// -------------------
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Cookie Authentication properly
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";          // Redirects here if not logged in
        options.LogoutPath = "/Account/Logout";        // Handles logout
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect for unauthorized role access
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true; // extend session if user stays active
    });

// Role-based authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LecturerOnly", policy => policy.RequireRole("Lecturer"));
    options.AddPolicy("PCOnly", policy => policy.RequireRole("PC"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
});

// SignalR for real-time communication (already good)
builder.Services.AddSignalR();

var app = builder.Build();

// -------------------
// DATABASE SEED (test users)
// -------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Users.Add(new Part2_CMCS.Models.User
        {
            Username = "lecturer1",
            Password = "password", // prototype only
            Role = "Lecturer",
            FullName = "Test Lecturer"
        });
        db.Users.Add(new Part2_CMCS.Models.User
        {
            Username = "pc1",
            Password = "password",
            Role = "PC",
            FullName = "Test PC"
        });
        db.SaveChanges();
    }
}

// -------------------
// MIDDLEWARE ORDER (VERY IMPORTANT)
// -------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication first, then Authorization
app.UseAuthentication();
app.UseAuthorization();

// -------------------
// ROUTES
// -------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<StatusHub>("/statusHub");

app.Run();
