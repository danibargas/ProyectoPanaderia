namespace ProyectoPanaderia.Models.ViewModels
{
    public class ProductoViewModel
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public double Precio { get; set; }
        public int Cantidad { get; set; }
        public int idtipo { get; set; }
        public string Alergenos { get; set; }
    }
}
