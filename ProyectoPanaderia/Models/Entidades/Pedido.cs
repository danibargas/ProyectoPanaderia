using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(IdPedido))]
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdSocio { get; set; }
        public DateTime FechaRealizacion { get; set; }
        public string Estado { get; set; }
        public string Cobrado { get; set; }
        public double Total { get; set; }

        [ForeignKey("IdSocio")]
        public Socio Socio { get; set; }

    }
}
