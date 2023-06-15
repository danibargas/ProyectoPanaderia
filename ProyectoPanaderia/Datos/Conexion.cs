using Microsoft.EntityFrameworkCore;
using ProyectoPanaderia.Models.Entidades;

namespace ProyectoPanaderia.Datos
{
    public class Conexion : DbContext
    {
        public Conexion(DbContextOptions<Conexion> options) : base(options){ }

        public DbSet<Productos> Productos { get; set; }

        public DbSet<Alergenos> Alergenos { get; set;}

        public DbSet<AlergenosPorProducto> AlergenosPorProductos { get; set; }

        public DbSet<Pedido> Pedido { get; set; }

        public DbSet<ProductosPorTipo> ProductosPorTipos { get;set; }

        public DbSet<Socio> Socio { get; set; }

        public DbSet<Tipos> Tipos { get; set; }

        public DbSet<Usuarios> Usuarios { get; set; }

        public DbSet<DetallePedido> DetallePedido { get; set; }

        public DbSet<Grupos> Grupos { get; set; }

    }
}
