using ProyectoPanaderia.Models.Entidades;

namespace ProyectoPanaderia.Models.ViewModels
{
    public class ProductoEdit
    {
        public List<ProductoViewModel> Productos { get; set; }
        public List<Alergenos> Alergenos { get; set;}
        public List<Tipos> Tipos { get; set; }
    }
}
