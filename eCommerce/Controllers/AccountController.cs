using eCommerce.Data;
using eCommerce.Models;
using eCommerce.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Security.Claims;


namespace eCommerce.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(AppDbContext context) : base(context)
        {

        }

        //Registrarse - get

        //acceso publico
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        //registro-post
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult>Register(Usuario usuario)
        {
            //validar
            try
            {
                if (usuario !=null)
                {
                    //verificar si el nombre de usuario ya esta en uso / comparar datos de la bd con los que recibo del modelo
                    if (await _context.Usuarios.AnyAsync(u=>u.NombreUsuario == usuario.NombreUsuario))
                    {
                        //mostrar mensaje de error
                        ModelState.AddModelError(nameof(usuario.NombreUsuario),"El nombre de usuario ya esta en uso");
                        return View(usuario);
                    }

                    //generar un token con su fecha de expiracion de 10 minutos
                    usuario.Token = Guid.NewGuid().ToString();
                    
                    usuario.FechaExpiracion = DateTime.UtcNow.AddMinutes(10);

                    //asignar el rol de usuario de cliente automaticamente
                    var clienteRol = await _context.Roles.FirstOrDefaultAsync(r=>r.Nombre=="Cliente");

                    if (clienteRol != null)
                    {
                        usuario.RolId = clienteRol.RolId;
                    }

                    //agregar nueva direccion al usuario registrado

                    usuario.Direcciones = new List<Direccion>
                    {
                        new Direccion
                        {
                          Address = usuario.Direccion,
                          Ciudad = usuario.Ciudad,
                          Provincia = usuario.Provincia,
                          CodigoPostal = usuario.CodigoPostal,

                        }
                    };

                     //insertar y guardar
                    _context.Usuarios.Add(usuario);
                   await _context.SaveChangesAsync();


                    // Enviar correo con el token de activación
                    Email email = new();
                    if (!string.IsNullOrEmpty(usuario.Correo))
                    {
                        email.Enviar(usuario.Correo, usuario.Token);
                    }

                  
                    //redirigir al usuario
                    return RedirectToAction("Token");
                }
                //si hyaa un error recargar la vista
                return View(usuario);
            }
            catch (DbException dbException)
            {
                //error dedicado a nuevos registros
                return HandleDbError(dbException);
            }
        }


      
        // Método para activar la cuenta
        public async Task<IActionResult> Token()
        {
            // Obtener el token de la consulta
            string token = Request.Query["valor"].ToString().Trim('\'', '"');
            if (!string.IsNullOrEmpty(token)) // Validar que el token no sea nulo o vacío
            {
                try
                {
                    // Buscar el usuario por el token
                    var usuario = await _context.Usuarios
                        .Where(u => u.Token == token)
                        .FirstOrDefaultAsync();

                    if (usuario == null) // Si no se encuentra el usuario con ese token
                    {
                        ViewData["mensaje"] = "Enlace de activación inválido o usuario no encontrado.";
                        return View(); 
                    }

                    // Verificar si el token ha expirado
                    if (usuario.FechaExpiracion < DateTime.UtcNow)
                    {
                        await ActualizarToken(usuario.Correo);
                        ViewData["mensaje"] = "Enlace de activación expirado, revise su bandeja de entrada.";
                      
                        return View(); 
                    }

                    // Verificar si el usuario ya ha activado su cuenta previamente
                    if (usuario.Estado == true)
                    {
                        ViewData["mensaje"] = "La cuenta ya ha sido activada anteriormente.";
                        return View();
                    }

                    // Activar la cuenta y eliminar el token
                    usuario.Estado = true;
                    usuario.Token = null; // Eliminar el token después de la activación
                    usuario.FechaExpiracion = null; // eliminar la fecha  de expiracion
                    await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos

                    // Autenticar al usuario y establecer la cookie de autenticación
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.Name, usuario.NombreUsuario)); // Nombre de usuario
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Cliente")); // Rol del usuario

                    // Iniciar sesión
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    // Redirigir a la página principal después de la activación y autenticación exitosa
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception e)
                {
                    // Manejar posibles errores y retornar una vista de error si ocurre alguna excepción
                    return HandleError(e);
                }
            }
            else
            {
                // Si el token es nulo o no se envió en la consulta
                ViewData["mensaje"] = "Verifique su correo para activar su cuenta.";
                return View(); // Retornar a la vista con mensaje de error
            }
        }


        //LOGIN/Get
        [AllowAnonymous]
        public IActionResult Login()
        {
          

            return View();
        }

        //LOGIN - POST
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                //logica para autenticar al usuario

                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == username && u.Contrasenia == password);
                //validar
                if (user != null)
                {


                    // Verificar el estado de la cuenta y la fecha de expiración
                    DateTime fechaExpiracion = DateTime.UtcNow;

                    if (!user.Estado && user.FechaExpiracion > fechaExpiracion)
                    {
                        // Si el estado es false y la fecha de expiración es válida
                        if (!string.IsNullOrEmpty(user.Correo))
                        {
                           await ActualizarToken(user.Correo); // Reenviar el correo de activación
                        }
                        ViewBag.Error = "Su cuenta no fue activada, se reenvió un correo a su cuenta, verifíquela.";
                        return View();
                       
                    }
                    else if (!user.Estado)
                    {
                        // Si el estado es false pero la fecha de expiración es inválida
                        ViewBag.Error = "Su cuenta no fue activada, verifique su bandeja de entrada.";
                       
                    } else
                    {
                        //si todo esta bien iniciar sesión:

                        //crear identidad para el usaurio autenticado 

                        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                        //establecerle el  valor de nombre de usuario
                        identity.AddClaim(new Claim(ClaimTypes.Name, username));
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()));

                        //guardar en una variable el rol del usuario autenticado
                        var rol = await _context.Roles.FirstOrDefaultAsync(r => r.RolId == user.RolId);
                        if (rol != null)
                        {
                            //asignarle el rol 
                            identity.AddClaim(new Claim(ClaimTypes.Role, rol.Nombre));
                        }

                        //Iniciar sesión al usuario
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                        //redirigir al usuario dependiendo de su rol
                        if (rol != null)
                        {
                            if (rol.Nombre == "Administrador" || rol.Nombre == "Staff")
                            {
                                return RedirectToAction("Index", "Dashboard");
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }

                    }
                  
                }

                //si existe un error
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View();


            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        //METODO LOGOUT
        public async Task<IActionResult> Logout()
        {
            //cerrar la sesión y eliminar la cookie de registro
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        //metodo para errores de acceso denegado
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }



        private async Task ActualizarToken(string correo)
        {
            // Buscar al usuario por correo
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

            // Validar si se encontró el usuario
            if (usuario != null)
            {
                // Calcular la nueva fecha de expiración (10 minutos a partir de ahora)
                usuario.FechaExpiracion = DateTime.UtcNow.AddMinutes(10);

                // Generar un nuevo token
                usuario.Token = Guid.NewGuid().ToString();

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                // Enviar el correo de activación
                Email email = new();
                email.Enviar(correo, usuario.Token);
            }
        }


    }
}
