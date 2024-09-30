using eCommerce.Data;
using eCommerce.Models;
using eCommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using System;
using System.Globalization;
using System.Security.Claims;

namespace eCommerce.Controllers
{
    public class CarritoController : BaseController
    {
        public CarritoController(AppDbContext context) : base(context)
        {

        }


       [AllowAnonymous]
       public async Task<IActionResult> Index()
        {
            //obtener los detalles del carrito
            var carritoViewModel = await GetCarritoViewModelAsync();

            var itemsEliminar = new List<CarritoItemViewModel>();

            foreach (var item in carritoViewModel.Items)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                //calcular el subtotal
                if (producto != null)
                {
                    //obtener lso datos de la bd
                    item.Producto = producto;
                    if (!producto.Activo)
                    
                    //Agregar productos a eliminar a la variable itemseliminar
                    itemsEliminar.Add(item);
                    else
                    //calcular si la cantidad es mayor al stock disponible
                    item.Cantidad = Math.Min(item.Cantidad, producto.Stock);

                                   
                }
                else
                {
                    //Agregar productos a eliminar a la variable itemseliminar
                    itemsEliminar.Add(item);
                }

            }
            //ciclo para eliminar los productos no validos
            foreach (var item in itemsEliminar)
            {
                carritoViewModel.Items.Remove(item);
                
            }
            //Actualizar la cookie con los items correctos
            await UpdateCarritoViewModelAsync(carritoViewModel);
            //recalcular
            carritoViewModel.Total = carritoViewModel.Items.Sum(item => item.Subtotal);

            //verificar si el usaurio esta autenticado y guardar su Id
            var usuarioId = User.Identity?.IsAuthenticated == true ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : 0;

            //validar que la direccion de envio sea valida , caso contrario inicializar nueva lista vacia
            var direcciones = User.Identity?.IsAuthenticated == true ? _context.Direcciones.Where(d => d.UsuarioId == usuarioId).ToList()
                : new List<Direccion>();

            //crear Viewmodel
            var procederConCompraViewModel = new ProcederConCompraViewModel
            {
                Carrito = carritoViewModel,
                Direcciones = direcciones,
            };

            return View(procederConCompraViewModel);
       }


        //Actualizar cantidad del carrito
        public async Task<IActionResult> ActualizarCantidad(int id, int cantidad)
        {
            //oibtener el Viewmodel
            var carritoViewModel = await GetCarritoViewModelAsync();
            // asignar los items del carrito
            var carritoItem = carritoViewModel.Items.FirstOrDefault(i=>i.ProductoId == id);

            if (carritoItem != null)
            {
                //asignar cantidad
                carritoItem.Cantidad = cantidad;
                 
                //encontrar producto
                var producto = await _context.Productos.FindAsync(id);

                //asegurar que  el producto esta activo y el stock es mayor a 0
                if (producto != null && producto.Activo && producto.Stock>0)
                {
                    //actualizar la cantidad del item del carrito con su stock
                    carritoItem.Cantidad = Math.Min(cantidad, producto.Stock);
                }

                //actualizar el carrito 
                await UpdateCarritoViewModelAsync(carritoViewModel);
            }

            return RedirectToAction("Index","Carrito");
        }

        //eliminar un producto del carrito
        [HttpPost]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            //oibtener el Viewmodel
            var carritoViewModel = await GetCarritoViewModelAsync();
            // asignar los items del carrito
            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == id);

            if (carritoItem != null)
            {
                //eliminar item
                carritoViewModel.Items.Remove(carritoItem);

                //actualizar el carrito 
                await UpdateCarritoViewModelAsync(carritoViewModel);
            }

            return RedirectToAction("Index");
        }



        //Vaciar el carrito por completo(eliminar la cookie)
        [HttpPost]
        public async Task<IActionResult> VaciarCarrito()
        {
            await RemoveCarritoViewModelAsync();
            return RedirectToAction("Index");
        }

        private async Task RemoveCarritoViewModelAsync()
        {

            //eliminar la cookie
            await Task.Run(() =>
            {
                Response.Cookies.Delete("carrito");
            });
        }

        //proceder con compra
        //variables que contienen la informacion de la cuenta de paypal sandbox
        private readonly string clientId = "AVrGbeI68PsCdCwZGxLcj3ksDLcKUJMT3Eyh7gMOJH56tD8LIQOBGGd3j1EzX_24_oOCbmwY4bAStUeT";
        private readonly string clientSecret = "EAs_9W0iybbEXyVCw9-b9jFL6O_EcN1usrTwxX_pQhYz3yuZ4BVZJ736q5ltK1Obx5V1mw3iaB4Wuz2I";
        public IActionResult ProcederConCompra(string montoTotalString, int direccionIdSeleccionada )
        {
            if (direccionIdSeleccionada > 0)
            {
                //crear una cookie con un tiempo de expiracion de un dia
                Response.Cookies.Append(
                    "direccionIdSeleccionada",direccionIdSeleccionada.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(1) }
                    );


            }
            else
            {
                return View("Index");
            }

            // Intentar convertir el string a decimal
            if (!decimal.TryParse(montoTotalString, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal montoTotal) || montoTotal <= 0)
            {
                ModelState.AddModelError("", "El monto total debe ser mayor que 0.");
                return View("Index");
            }

            //crear nueva orden en paypal usando el paquete de paypal
            var request = new OrdersCreateRequest();
            //preferencia para que retorne la representacin del objeto
            request.Prefer("return=representation");
            request.RequestBody(BuilderRequestBody(montoTotal));

            var environment = new SandboxEnvironment(clientId, clientSecret);
            var client = new PayPalHttpClient(environment);

            //hacer transaccion
            try
            {
                //almacenar datos utiles
                var response = client.Execute(request).Result;
                var statusCode = response.StatusCode;
                var responseBody = response.Result<Order>();
                //redirigir al usuario
                var approveLink = responseBody.Links.FirstOrDefault(x => x.Rel == "approve");
                //validar la respuesta
                if (approveLink != null)
                {
                    return Redirect(approveLink.Href);
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            catch (HttpException ex)
            {

                return (IActionResult)ex;
            }
        }




        private OrderRequest BuilderRequestBody(decimal montoTotal)
        {
            //obtener Url base de la app para retornar a mi sitio
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            //CUERPO DE SOLICITUD DE LA ORDEN
            var request = new OrderRequest()
            {
                // llenr valores
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>()
                {
                    new PurchaseUnitRequest()
                    {
                        AmountWithBreakdown = new AmountWithBreakdown()
                        {
                            CurrencyCode = "USD",
                             Value = montoTotal.ToString("F2",CultureInfo.InvariantCulture)
                        }
                    }

                },
              
                ApplicationContext = new ApplicationContext()
                {
                    //url de retorno en caso de completar el pago
                    ReturnUrl = $"{baseUrl}/Carrito/PagoCompletado",
                    //en caso de canmcelar el pago
                    CancelUrl=$"{baseUrl}/Carrito/Index"
                }
            };

            return request;
     
        }

        public IActionResult PagoCompletado()
        {
            try
            {
                //almacenar la cookie del carrito
                var carritoJson = Request.Cookies["Carrito"];
                //
                int direccionId = 0;

                //intentar obtener la direccion de la cookie y retornar el valor de la cookie
                if (Request.Cookies.TryGetValue("direccionseleccionada", out string? cookieValue)&&
                    //luego parsearla a int 
                    int.TryParse(cookieValue, out int parseValue))
                {
                    direccionId = parseValue;
                }

                //desearlizar el objeto lista 
                List<ProductoIdAndCantidad>? productoIdAndCantidads = !string.IsNullOrEmpty(carritoJson)
                    ? JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson) : null;
                //inicializar un carritoViewmodel
                CarritoViewModel carritoViewModel = new();
                //validar
                if (productoIdAndCantidads!=null)
                {
                    //recorrer la vista
                    foreach (var item in productoIdAndCantidads)
                    {
                        var producto = _context.Productos.Find(item.ProductoId);
                        if (producto != null)
                        {
                            //inicializar
                            carritoViewModel.Items.Add(new CarritoItemViewModel
                            {
                                ProductoId = producto.ProductoId,
                                Nombre = producto.Nombre,
                                Precio = producto.Precio,
                                Cantidad = item.Cantidad,
                            });
                                                                               
                        }

                       
                    }

                }

                //almacer el usaurio id
                var usuarioId = User.Identity?.IsAuthenticated == true ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
                    : 0;
                //sacar el total del carrito
                carritoViewModel.Total = carritoViewModel.Items.Sum(i => i.Subtotal);
                //crear el registro del pedido en la bd
                var pedido = new Pedido
                {
                    //asignarle sus campos
                    UsuarioId = usuarioId,
                    Fecha = DateTime.UtcNow,
                    Estado = "Vendido",
                    DireccionIdSeleccionada = direccionId,
                    Total = carritoViewModel.Total,
                };

                //agregar y guardar
                _context.Pedidos.Add(pedido);
                _context.SaveChanges();

                //crear el registro del pedidoDetalles en la bd
                foreach (var item in carritoViewModel.Items)
                {
                    var pedidoDetalle = new Detalle_Pedido
                    {
                        PedidoId = pedido.PedidoId,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        Precio = item.Precio,
                    };

                    _context.DetallePedidos.Add(pedidoDetalle);
                    //reducir el stock con los productos ya comprados
                    var producto = _context.Productos.FirstOrDefault(p => p.ProductoId == item.ProductoId);

                    if (producto != null)
                    {
                        producto.Stock -= item.Cantidad;
                    }
                }

                _context.SaveChanges();

                //eliminar cookies quen ya no se necesitan
                Response.Cookies.Delete("carrito");
                Response.Cookies.Delete("direccionseleccionada");
                //retornar la informacion en un ViewBag

                ViewBag.DetallePedidos = _context.DetallePedidos
                    .Where(dp=>dp.PedidoId == pedido.PedidoId)
                    //Incluir el producto (ssus detalles)
                    .Include(dp=>dp.Producto)
                    //convertir a una lista
                    .ToList();

                return View("PagoCompletado",pedido);
            }
            catch (Exception e)
            {
                return HandleError(e);
               
            }
        }
    }
}
