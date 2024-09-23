using eCommerce.Data;
using eCommerce.Models;
using eCommerce.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Orders;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace eCommerce.Controllers
{
    //controlador base para metodos compartidos
    public class BaseController : Controller
    {
        //instancia de la db
        public readonly AppDbContext _context;
        public BaseController(AppDbContext context)
        {
            _context = context;
        }


        //NUMERO DE PRODUCTOS DEL CARRITO
        //retorna una vista de resultados
        public override ViewResult View(string? viewName, object? model)
        {
            //Llenar el viewbag con los productos
            ViewBag.NumeroProductos = GetCarritoCount();
            return base.View(viewName, model);
        }

        //numero de productos
        protected int GetCarritoCount()
        {
            int Count = 0;

            //usar una cookie para almacenar los productos agregados al carrito

            //cargar la cookie en la variable carritojson
            string? carritoJson = Request.Cookies["carrito"];

            //desearlizar
            if (!string.IsNullOrEmpty(carritoJson))
            {
                var carrito = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);
                if (carrito != null)
                {
                    Count = carrito.Count;
                }
            }


            return Count;
        }

        //agregar producto al carrito
        public async Task<CarritoViewModel> AgregarProductoAlCarrito(int productoId, int cantidad)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            //validar 
            if (producto != null)
            {
                var carritoViewModel = await GetCarritoViewModelAsync();

                //buscar algun producto que coincida con el producto Id que se recibe
                var carritoItem = carritoViewModel.Items.FirstOrDefault(
                    item => item.ProductoId == productoId
                    ); 

                if (carritoItem != null)
                
                    carritoItem.Cantidad += cantidad;
                else
                {
                    //inicializar el viewmodel
                    carritoViewModel.Items.Add
                        (
                         new CarritoItemViewModel
                         {
                             ProductoId = productoId,
                             Nombre = producto.Nombre,
                             Precio = producto.Precio,
                             Cantidad = cantidad
                         }
                      );
                }

                //calcular el total del carrito
                carritoViewModel.Total = carritoViewModel.Items.Sum(
                    Item=>Item.Cantidad * Item.Precio);

                //actualizar el carrito
                await UpdateCarritoViewModelAsync(carritoViewModel);
                return carritoViewModel;

            }

            return new CarritoViewModel();
           
        }

        //actualizar carrito
        public async Task UpdateCarritoViewModelAsync( CarritoViewModel carritoViewModel)
        {
            //almacenar en una variable lo que se recibe en el viewmodel
            var productoIds = carritoViewModel.Items.Select(
                item => new ProductoIdAndCantidad
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad
                }
              ).ToList();

            //convertir la variable en un objeto json
            var carritoJson = await Task.Run(() => JsonConvert.SerializeObject(productoIds));

            //crear y establecer fecha de expiracion de la cookie
            Response.Cookies.Append("carrito",carritoJson, new CookieOptions {Expires = DateTimeOffset.Now.AddDays(7) });

        }


        //get carrito / detalles
        public async Task<CarritoViewModel> GetCarritoViewModelAsync()
        {
            //request a la cookie
            var carritoJson = Request.Cookies["carrito"];
            //evaluar si la cookie tiene valores
            if ( string.IsNullOrEmpty(carritoJson))
            {
                //retornar el viewmodel vacio
                return new CarritoViewModel();
            }

            //desearlizar el objeto
            var productoIdsAndCantidad = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);

            //crear el carrito ViewModel
            var carritoViewModel = new CarritoViewModel();

            if (productoIdsAndCantidad != null)
            {
                //seguir deserealizando la cookie
                foreach (var item in productoIdsAndCantidad)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto != null)
                    {
                        carritoViewModel.Items.Add
                            //inicializaar un nuevo carritoItem
                            (
                            new CarritoItemViewModel
                            {
                                ProductoId = producto.ProductoId,
                                Nombre = producto.Nombre,
                                Precio = producto.Precio,
                                Cantidad = item.Cantidad

                            });
                    }
                }
            }

            //sumar el total del carrito y retornarlo
            carritoViewModel.Total = carritoViewModel.Items.Sum(item=>item.Subtotal);
            return carritoViewModel;

        }



        //metodos reutilzables para mostrarmensajes de error / registra errores

        //manejo adicionaal de execpciones
        protected IActionResult HandleError(Exception e)
        {
            return View(
                "Error", new ErrorViewModel {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier

                }
            );
        }

        //error de base de datos
        protected IActionResult HandleDbError(DbException dbException)
        {
            //crearViewModel para errores de db 
            var ViewModel = new DbErrorViewModel
            {
                ErrorMessage = "Error de base de datos",
                Details = dbException.Message
            };
            return View("DbError", ViewModel);
        }

        //error de base de datos actualizar
        protected IActionResult HandleDbUpdateError(DbUpdateException dbUpdateException)
        {
            //crearViewModel para errores de db 
            var ViewModel = new DbErrorViewModel
            {
                ErrorMessage = "Error al actualizar la base de datos",
                Details = dbUpdateException.Message
            };
            return View("DbError", ViewModel);
        }


    } 
}

