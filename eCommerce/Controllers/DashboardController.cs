using eCommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Controllers
{
    //heredar del controlador base
    public class DashboardController : BaseController
    {
        
        public DashboardController(AppDbContext context):base(context) 
        {
            
        }

        //Vista para mostrar los enlaces a otros cruds

        public IActionResult Index()
        {
            return View();
        }
    }
}
