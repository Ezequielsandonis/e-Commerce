using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Models
{
    public class Direccion
    {
        [Key]
        public int DireccionId { get; set; }

        [Required(ErrorMessage = "La direccion es requerida")]
        [StringLength(30)]
        public string Address { get; set; } = null!;

   
        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(30)]
        public string Provincia { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(100)]
        public string Ciudad { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(10)]
        public string CodigoPostal { get; set; } = null!;

        //relacion con usuario
        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set;} = null!;
    }
}
