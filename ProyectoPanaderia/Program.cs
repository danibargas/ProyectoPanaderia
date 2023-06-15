using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoPanaderia.Datos;
using ProyectoPanaderia.Methods;
using ProyectoPanaderia.Models;
var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<Conexion>(options=>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DatosHub>();
builder.Services.AddScoped<Alta>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(opt =>
{
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});
builder.Services.AddSignalR();
builder.Services.AddScoped<Insertar>();
builder.Services.AddScoped<Restricciones>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<DatosHub>("/datosHub");

    app.UseEndpoints(endpoints =>
    {
        // Otras rutas

        
    });

    // Otros endpoints...
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
