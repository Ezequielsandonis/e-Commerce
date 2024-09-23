using eCommerce.Models;

namespace eCommerce.Servicios
{
    public interface ICategoriaService
    {
        // Metodo de tipo lista para Obtener las categorias

        Task<List<Categoria>> GetCategoria();
    }
}
