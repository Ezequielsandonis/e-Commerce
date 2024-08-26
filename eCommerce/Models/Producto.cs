using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Models
{
    public class Producto
    {
        [Key]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El codigo es requerido")]
        [StringLength(500)]
        public string Codigo { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El modelo es requerido")]
        [StringLength(50)]
        public string Modelo { get; set; } = null!;

        [Required(ErrorMessage = "La descripcion es requerido")]
        [StringLength(1000)]
        public string Descripcion { get; set; } = null!;

        [Required]

        public decimal Precio { get; set; }

        [Required(ErrorMessage = "La imagen es requerido")]
        [StringLength(500)]
        public string Imagen { get; set; } = null!;

       
        public int CategoriaId { get; set; }

        //fk categoria
        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; } = null!;

        [Required]

        public int Stock {  get; set; }

        [Required]
        [StringLength(100)]
        public string Marca { get; set; } = null!;

        [Required]
        public bool Activo { get; set; }

        //Ref a Detalle Pedido
        public ICollection<Detalle_Pedido> DetallesPedido { get; set; } = null!;

    }
}
