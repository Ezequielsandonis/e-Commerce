using eCommerce.Data;
using eCommerce.Models;
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

                    //recordar programar envio de correo...

                    //aurenticar usuario y establecer la cookie de atutenticacion
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    //establecerle el  valor de nombre de usuario
                    identity.AddClaim(new Claim(ClaimTypes.Name, usuario.NombreUsuario));
                    //asignar rol predeterminado al usuario
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Cliente"));

                    //Iniciar sesión al usuario
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    //redirigir al usuario
                    return RedirectToAction("Index","Home");
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

        //LOGIN/Get
        [AllowAnonymous]
        public IActionResult Login()
        {
            //verificar si el usuario ya esta logueado
            //se usa la identity que esta almacenada en una cookie
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
            
            }

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
                        new Claim(ClaimTypes.Role, rol.Nombre);
                    }

                    //Iniciar sesión al usuario
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    //redirigir al usuario dependiendo de su rol
                    if (rol !=null)
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

    }
}
