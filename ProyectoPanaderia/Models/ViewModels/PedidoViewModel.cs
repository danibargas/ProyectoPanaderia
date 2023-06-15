namespace ProyectoPanaderia.Models.ViewModels
{
    public class PedidoViewModel
    {
        public int IdPedido { get; set; }
        public string NickName { get; set; }
        public DateOnly Fecha { get; set; }
        public string Estado { get; set; }
        public string Cobrado { get; set; }
        public double Precio { get; set; }
    }
}
