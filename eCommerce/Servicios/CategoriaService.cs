using eCommerce.Data;
using eCommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Servicios
{
    public class CategoriaService : ICategoriaService
    {
    
        //instancia del contexto
        private readonly AppDbContext _context;
        public CategoriaService(AppDbContext context)
        {
                _context = context;
        }


        //implementacion de la interfaz
        public async Task<List<Categoria>> GetCategoria()
        {
           // Retorna  una lista de  las categorías de la base de datos
           return await _context.Categorias.ToListAsync();
        }


    }
}
