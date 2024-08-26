using System.ComponentModel.DataAnnotations;

namespace eCommerce.Models
{
    public class Rol
    {
        [Key]
        public int RolId { get; set; }

        [Required(ErrorMessage ="El nombre del rol es requerido")]
        [StringLength(50)]
        public string ? Nombre { get; set; }
    }
}
