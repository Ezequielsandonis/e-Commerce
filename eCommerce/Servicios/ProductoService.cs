using eCommerce.Data;
using eCommerce.Models;
using eCommerce.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Servicios
{
    public class ProductoService : IProductoService
    {
        //instancia
        private readonly AppDbContext _context;

        public ProductoService(AppDbContext context)
        {
            _context = context;
        }


        //Traer producto por id
        public  Producto GetProducto(int id)
        {
            var producto = _context.Productos
                //incluir categoria
                .Include(p=>p.Categoria)
                .FirstOrDefault(p=>p.ProductoId == id);

            //validar
            if (producto != null)
             
            return producto;
            
            //si no retorna producto vacio
            return  new Producto();
              
        }


        //traer lista de productos destacados
        public async Task<List<Producto>> GetProductosDestacados()
        {
            //los primeros 9
            IQueryable<Producto> productosQuery = _context.Productos;
            //filtrar los productos activos / disponibles
            productosQuery = productosQuery.Where(p => p.Activo);

            //lista de los primeros 9
            List<Producto> productosDestacados = await productosQuery.Take(9).ToListAsync();

            //retorna los productos destacados
            return productosDestacados;
        }

        //p paginados
        public async Task<ProductosPaginadosViewModel> GetProductosPaginados(int? categoriaId, string? busqueda, int pagina, int productosPorPagina)
        {
            IQueryable<Producto> query = _context.Productos;
            //filtrar por activo
            query = query.Where(p => p.Activo);
            //filtrar por categoria
            if (categoriaId.HasValue)
            
                query = query.Where(p => p.CategoriaId == categoriaId);
            
            //filtrar  por busqyeda (Nombre o Descripcion)
            if (!string.IsNullOrEmpty(busqueda))
            
                query = query.Where(p => p.Nombre.Contains(busqueda) || p.Descripcion.Contains(busqueda));
            
            //filtrar por pagina

            //¿Cuantos productos hay en la consulta?
            int totalProductos = await query.CountAsync();

            //total de pagina
            //conversion implicita/parse
            //total de pagimnas = TotalProductos dividido en productos por pagina 
            int totalPaginas = (int)Math.Ceiling((double)totalProductos / productosPorPagina);  

            //numero de pagina proporcionado
            //si la pagina es menor a 1
            if(pagina<1)
                //mi numero de pagina es 1
                pagina = 1;
           //si la pagina es mayor al total de pagina calculado
            else if (pagina>totalPaginas)
                //mi pagina es la ultima
                pagina = totalPaginas;

            //lista de productos solo si hay resultados
            //lista vacia
            List<Producto> productos = new();

            //validar
            if (totalProductos>0)
            {
                //actualizar la lista de productos
                productos = await query
                    //recordfar explicar
                    .OrderBy(p => p.Nombre)
                    .Skip((pagina-1)*productosPorPagina)
                    .Take(productosPorPagina)
                    .ToListAsync();
            }

            //mensaje sin resultado
            bool mostrarMensajesSinResultado = totalProductos == 0;

            //crear modelo para la vista
            var model = new ProductosPaginadosViewModel
            {
                Productos = productos,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                CategoriaIdSeleccionada = categoriaId,
                Busqueda = busqueda,
                MostrarMensajeSinResultado = mostrarMensajesSinResultado
            };

            return model;
            
        }
    }
}
