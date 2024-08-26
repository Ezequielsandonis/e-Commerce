using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Models
{
    public class Usuario
    {
        //inicializar la clase con la propiedad pedidos como una lista vacia para que en los usuarios ya exista una lista de pedidos vacia
        public Usuario()
        {
            Pedidos = new List<Pedido>();
        }

        [Key]
       public int UsuarioId {  get; set; }

        [Required(ErrorMessage="El nombre es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;
        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(15)]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerido")]
        [StringLength(200)]
        public string Contrasenia { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(50)]
        public string Correo { get; set; } = null!;
        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(100)]
        public string Direccion { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(30)]
        public string Provincia { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(100)]
        public string Ciudad { get; set; } = null!;

        [Required(ErrorMessage = "El Correo es requerido")]
        [StringLength(10)]
        public string CodigoPostal { get; set; } = null!;

        [Required]
        public int RolId { get; set; }

        //referencias fk
        [ForeignKey("RolId")]
        public Rol Rol { get; set; } = null!;
        //coleccion de pedidos
        public ICollection<Pedido> Pedidos { get; set; }


        //asociar direcciones a usuario
        [InverseProperty("Usuario")]
        public ICollection<Direccion> Direcciones { get; set; } = null!;
        
    }
}
