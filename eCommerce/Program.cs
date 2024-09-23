using eCommerce.Data;
using eCommerce.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

//Cadena de conexión
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

//servicio de autorizacion para administradores
builder.Services.AddAuthorization(options =>
{
    //politica para autorizar usuarios administradores o staff
    options.AddPolicy("RequireAdminOrStaff", policy => policy.RequireRole("Administrador","Staff"));
});

//autentificacion basadas en un esquema de  cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    //tiempo de expiración de la cookie
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    //rutas de inicio de sesion y acceso denegado
    options.LoginPath= "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

//otros servicios
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();



var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
  
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//ANTES DE AUTORIZAR SE AUTENTIFICA
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
