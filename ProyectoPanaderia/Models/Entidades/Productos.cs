using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(IdProducto))]
    public class Productos
    {
        public int IdProducto { get; set; }
        public string Name { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }

    }
}
