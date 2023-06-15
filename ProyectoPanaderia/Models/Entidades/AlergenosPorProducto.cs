using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(Id))]
    public class AlergenosPorProducto
    {
        public int Id { get; set; }

        [ForeignKey("IdProducto")]
        public Productos Productos { get; set; }
        public int IdProducto { get; set; }

        [ForeignKey("IdAlergeno")]
        public Alergenos Alergenos { get; set; }
        public int IdAlergeno { get; set; }
    }
}
