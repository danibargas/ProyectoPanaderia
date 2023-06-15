using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(Id))]
    public class ProductosPorTipo
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public int IdTipo { get; set; }

        [ForeignKey("IdProducto")]
        public Productos Productos { get; set; }
        [ForeignKey("IdTipo")]
        public Tipos Tipos { get; set; }
    }
}
