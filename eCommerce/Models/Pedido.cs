using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }

        //ref a usuario
        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; } = null!;

        [Required]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El Estado es requerido")]
        public string Estado { get; set; } = null!;

        //ref a Direccion
        public int DireccionIdSeleccionada { get; set; } // UN usuario puede tener multiples direcciones
      //[ForeignKey("DireccionId")]
        public Direccion Direccion { get; set; } = null!;

        public decimal Total { get; set; }

        //Ref a Detalle Pedido
        public ICollection<Detalle_Pedido> DetallesPedido { get; set; } = null!;
    }
}
