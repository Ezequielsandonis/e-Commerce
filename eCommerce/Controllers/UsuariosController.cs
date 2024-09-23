using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eCommerce.Data;
using eCommerce.Models;

namespace eCommerce.Controllers
{
    public class UsuariosController : BaseController
    {
      
        public UsuariosController(AppDbContext context):base(context)
        {
         
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Usuarios.Include(u => u.Rol);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["RolId"] = new SelectList(_context.Roles, "RolId", "Nombre");
            return View();
        }

        
 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsuarioId,Nombre,Telefono,NombreUsuario,Contrasenia,Correo,Direccion,Provincia,Ciudad,CodigoPostal,RolId")] Usuario usuario)
        {
            // asignar a una variable el valor del rol que se encuentre en base al RolId del usuario para que el modelo sea válido
            var rol = await _context.Roles
                .Where(d=>d.RolId == usuario.RolId)
                .FirstOrDefaultAsync();

            if (rol != null)
            {

                //asignar rol y direcciones al usuario

                usuario.Rol = rol;

                //con los datos del usuario  crear una nueva lista de direcciones
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

                //No se le asigna pedidos ya que pedidos puede ser null y se inicializa en el modelo como una lista vacia

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RolId"] = new SelectList(_context.Roles, "RolId", "Nombre", usuario.RolId);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["RolId"] = new SelectList(_context.Roles, "RolId", "Nombre", usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UsuarioId,Nombre,Telefono,NombreUsuario,Contrasenia,Correo,Direccion,Provincia,Ciudad,CodigoPostal,RolId")] Usuario usuario)
        {
            if (id != usuario.UsuarioId)
            {
                return NotFound();
            }

            // asignar a una variable el valor del rol que se encuentre en base al RolId del usuario para que el modelo sea válido
            var rol = await _context.Roles
                .Where(d => d.RolId == usuario.RolId)
                .FirstOrDefaultAsync();

            if (rol != null)
            {
           

               //obtener usuario existente y guardarlo en una variable
               var existingUser = await _context.Usuarios
                    //incluir direcciones
                    .Include(u => u.Direcciones)
                    .FirstOrDefaultAsync(u => u.UsuarioId == id);

                //validar 
                if (existingUser != null)
                {

                    //si las direcciones del usuario existente es mayor a 0
                    if (existingUser.Direcciones.Count > 0)
                    {
                        //actualizar valores de la direccion existente creando una nueva direccion
                        var direccion = existingUser.Direcciones.First();

                        direccion.Address = usuario.Direccion;
                        direccion.Ciudad = usuario.Ciudad;
                        direccion.Provincia = usuario.Provincia;
                        direccion.CodigoPostal = usuario.CodigoPostal;


                    }
                    //si no existen direcciones crear una nueva y asignarla al usuario
                    else
                    {
                        //crear una nueva lista de Direccion
                        existingUser.Direcciones = new List<Direccion> {


                            new Direccion
                            {
                               Address = usuario.Direccion,
                               Ciudad = usuario.Ciudad,
                               Provincia = usuario.Provincia,
                               CodigoPostal = usuario.CodigoPostal,
                            }

                        };

                    }

                    //asignar los nuevos parametros  al usuario
                    existingUser.Rol = rol;
                    existingUser.RolId = usuario.RolId;
                    existingUser.Nombre = usuario.Nombre;   
                    existingUser.Telefono = usuario.Telefono;
                    existingUser.NombreUsuario = usuario.NombreUsuario;
                    existingUser.Contrasenia = usuario.Contrasenia;
                    existingUser.Correo = usuario.Correo;

                    try
                    {
                        //actualizar usuario
                        _context.Update(existingUser);
                        //guardar cambios
                        await _context.SaveChangesAsync();
                    }

                    catch (DbUpdateConcurrencyException)
                    {
                        //existe un usuario?
                       if (!UsuarioExists(usuario.UsuarioId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            //si no lanzar exepcion
                            throw;
                        }
                    }

                    return RedirectToAction(nameof(Index));

                }
            }
            ViewData["RolId"] = new SelectList(_context.Roles, "RolId", "Nombre", usuario.RolId);
            return View(usuario);
        }



        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }
    }
}
