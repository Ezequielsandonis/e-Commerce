using System.ComponentModel.DataAnnotations;

namespace eCommerce.Models
{
    public class Categoria
    {
        //inicializar modelo con una lista de productos
        public Categoria()
        {
            Productos = new List<Producto>();
        }

        [Key]
        public int CategoriaId { get; set; }

        [Required (ErrorMessage ="El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = null!; // no permite valores nulos

        [Required(ErrorMessage = "La descripcion es requerido")]
        [StringLength(1000)]
        public string Descripcion {  get; set; } = null!;

        // referencias
        public ICollection<Producto> Productos { get; set; } = null!;



    }
}
