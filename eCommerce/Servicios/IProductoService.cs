using eCommerce.Data;
using eCommerce.Models;
using eCommerce.Models.ViewModels;

namespace eCommerce.Servicios
{
    public interface IProductoService
    {
        //Metodo para obtener un producto (por id)
        Producto GetProducto(int id);

        //lista de  productos destacados para mostrar en el home
        Task<List<Producto>> GetProductosDestacados();
        
        //productos paginados / recibe id de categoria , busqueda y pagina
        Task<ProductosPaginadosViewModel> GetProductosPaginados(int? categoriaId, string? busqueda, int pagina, int productosPorPagina);
    }
}
