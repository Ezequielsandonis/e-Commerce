using eCommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Data
{
    public class AppDbContext : DbContext
    {
        //recibe la cadena de conexion
        public AppDbContext(DbContextOptions<AppDbContext>options): base(options)
        {
                
        }

        //--Dbset de cada modelo para interactuar con las tablas de la db

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<Direccion> Direcciones { get; set; } = null!;
        public DbSet<Detalle_Pedido> DetallePedidos { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;


        //--configurar relaciones y resstricciones


        //metodo protegido de sobreescritura
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //relacion 1 a muchos entre Usuario-Pedido
           modelBuilder.Entity<Usuario>()
                //Un usuario tiene muchos pedidos
                .HasMany(u=>u.Pedidos)
                .WithOne(p=>p.Usuario)
                //En la tabla pedido está la llave foranea de la relacion
                .HasForeignKey(p=>p.UsuarioId)
                // Si se elimina un usuario se eliminan todos sus pedidos
                .OnDelete(DeleteBehavior.Cascade);



            //relacion 1 a muchos entre producto y Detalle pedidos
            modelBuilder.Entity<Producto>()
              //Un producto tiene muchos detalles pedido
               .HasMany(p => p.DetallesPedido)
               .WithOne(dp => dp.Producto)
                //En la tabla DetallePedido está la llave foranea de la relacion
               .HasForeignKey(dp => dp.ProductoId)
                // Si se elimina un producto se eliminan todos sus detalles pedido asociados
               .OnDelete(DeleteBehavior.Cascade);



            //relacion 1 a muchos entre pedido y detalle pedido
            modelBuilder.Entity<Pedido>()
                //Un pedido tiene muchos detalle Pedidos
              .HasMany(p => p.DetallesPedido)
              .WithOne(dp => dp.Pedido)

              .HasForeignKey(dp => dp.PedidoId)       
              .OnDelete(DeleteBehavior.Cascade);

            //Eliminar la relacion entre pedido y direccion
            modelBuilder.Entity<Pedido>()
              .Ignore(p => p.Direccion);


            //relacion 1 a muchos entre Categoria y productos
            modelBuilder.Entity<Categoria>()
               //Una categoria tiene muchos productos
              .HasMany(c => c.Productos)
              .WithOne(p => p.Categoria)

              .HasForeignKey( p=> p.CategoriaId)
              //si se borra una cateigia el campo categoria Id de los productos asociados se establece en null
              //no se eliminan los productos asociados
              .OnDelete(DeleteBehavior.Restrict);


        }

    }
}
