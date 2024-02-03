using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using proiect_ASP_NET.Data;
using proiect_ASP_NET.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// In loc de ApplicationUser era IdentityUser, dar am modificat pentru ca am declarat o clasa noua care o mosteneste 
// pe cea din framework pentru a o modifica extern
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>();

var app = builder.Build();

// CreateScope ofera acces la instanta curenta a aplicatiei
// Variabila scope are un Service Provider folosit pentru a injecta dependentele (DB, cookie, sesiune, autentificare
using (var scope = app.Services.CreateScope())
{
	var service = scope.ServiceProvider;
	SeedData.Initialize(service);
}

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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
