using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPanaderia.Models.Entidades
{
	[PrimaryKey(nameof(id))]
	public class DetallePedido
	{
		public int id { get; set; }
		public int IdPedido { get; set; }
		public int IdProducto { get; set; }
		public int Cantidad { get; set; }
		public double PrecioUnitario { get; set; }
		public double Total { get; set; }

		[ForeignKey(nameof(IdProducto))]
		public Productos Productos { get; set; }

		[ForeignKey(nameof(IdPedido))]
		public Pedido Pedido { get; set; }
	}
}
