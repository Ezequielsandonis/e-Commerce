using eCommerce.Data;
using eCommerce.Models;
using eCommerce.Models.ViewModels;
using eCommerce.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics;

namespace eCommerce.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IProductoService productoService , ICategoriaService categoriaService) : base(context)
        {
            _logger = logger;
            _productoService = productoService;
            _categoriaService = categoriaService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categoria = await _categoriaService.GetCategoria();
            try
            {
                //listar los productos destcados 
                List<Producto> productosDestacados = await _productoService.GetProductosDestacados();
                return View(productosDestacados);
            }
            catch (Exception e)
            {

               return HandleError(e);
            }
           
        }

        //detalles de producto seleccionado
        public  IActionResult DetalleProducto(int id)
        {
           var producto =  _productoService.GetProducto(id);
            //validar
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        //get productos + filtrado
        public async Task<IActionResult> Productos(int? categoriaId , string? busqueda, int pagina = 1)
        {
            try
            {
                int productosPorPagina = 9;
                var model = await _productoService.GetProductosPaginados(categoriaId,busqueda,pagina,productosPorPagina);

                ViewBag.Categoria = await _categoriaService.GetCategoria();

                //validar solicitud ajax
                if (Request.Headers["X-Requested-With"]=="XMLHttpRequest")
                {
                    //si es ajax devolver vist parcial con el modelo
                    return PartialView("_ProductosPartial",model);
                }

                //si no es, devolver vista de prductos
                return View(model);
            }
            catch (Exception e)
            {

                return HandleError(e);
            }
        }

        //agregar productos sin necesidad de redireccion
        public async Task<IActionResult> AgregarProducto(int id , int cantidad, int? categoriaId, string? busqueda, int pagina = 1 )
        {
            //llamar aal metodo del controlador base 
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel != null)
            {
                //recargar vista
                return RedirectToAction("Productos", new {id, categoriaId, busqueda, pagina });
            }
            else
            {
                return NotFound();
            }
        }

        //agregar productos desde diferentes vistas
        public async Task<IActionResult> AgregarProductoIndex(int id, int cantidad)
        {
            //llamar aal metodo del controlador base 
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel != null)
            {
                //recargar vista
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> AgregarProductoDetalle(int id, int cantidad)
        {
            //llamar aal metodo del controlador base 
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel != null)
            {
                //recargar vista con su identificador
                return RedirectToAction("DetalleProducto", new {id});
            }
            else
            {
                return NotFound();
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

    
    }
}
