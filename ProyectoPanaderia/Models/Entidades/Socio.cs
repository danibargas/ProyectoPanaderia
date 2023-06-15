using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(IdSocio))]
    public class Socio
    {
        public int IdSocio { get; set; }
        public string NickName { get; set; }

        public string Contraseña { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public DateTime FechaNacimiento { get; set; }

    }
}
