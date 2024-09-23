using eCommerce.Data;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Controllers
{
    public class PerfilController : BaseController
    {

        public PerfilController(AppDbContext context) : base(context)
        {

        }
  
        public IActionResult Index()
        {
            return View();
        }
    }
}
