using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(IdAlergeno))]
    public class Alergenos
    {
        public int IdAlergeno { get; set; }
        public string Descripcion { get; set; }

    }
}
