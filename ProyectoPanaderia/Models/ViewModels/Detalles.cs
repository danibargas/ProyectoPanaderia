using ProyectoPanaderia.Models.Entidades;

namespace ProyectoPanaderia.Models.ViewModels
{
    public class Detalles
    {
        public List<DetallePedido> Detalle { get; set; }
        public List<Productos> Productos { get; set; }
    }
}
