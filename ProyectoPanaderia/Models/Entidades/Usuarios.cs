using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(IdUsuario))]
    public class Usuarios
    {
        public int IdUsuario { get; set; }

        public int IdSocio { get; set; }
        public int IdGrupo { get; set; }

        [ForeignKey("IdSocio")]
        public Socio Socio { get; set;}

        [ForeignKey(nameof(IdGrupo))]
        public Grupos Grupos { get; set; }
    }
}
