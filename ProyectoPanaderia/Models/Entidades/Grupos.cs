using Microsoft.EntityFrameworkCore;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(IdGrupo))]
    public class Grupos
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; }
    }
}
