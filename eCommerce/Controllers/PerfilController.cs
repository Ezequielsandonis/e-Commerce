using eCommerce.Data;
using eCommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Controllers
{
    public class PerfilController : BaseController
    {

        public PerfilController(AppDbContext context) : base(context)
        {

        }

        //obtener informacion del usuario
        public async Task<IActionResult> Details(int? id)
        {
            //si el id es nulo
           if (id == null)
           {
                //retornar no encontrado
                return NotFound();
           }

            var usuario = await _context.Usuarios
                 //incluir direcciones del usuario para permitirle manejarlas
                 .Include(u => u.Direcciones)
                 .FirstOrDefaultAsync(u => u.UsuarioId == id);

            //validar
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);

        }

        //agregar direccion -GET
        public IActionResult AgregarDireccion( int id)
        {
            //enviar id del usuario en un viewbag
            ViewBag.Id = id;
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarDireccion( Direccion direccion, int id)
        {
            //control de errores
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == id);
                if (usuario != null)
                {
                    //asignar valor a la propiedad direcciona
                    direccion.Usuario = usuario;
                }
                //agregar y guardar
                _context.Add(direccion);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id });
            }
            catch (Exception e)
            {

               return View(direccion);
            }

            //buscar el usaurio
         
        }

    }
}
