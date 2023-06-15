using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProyectoPanaderia.Models.Entidades
{
    [PrimaryKey(nameof(IdTipo))]
    public class Tipos
    {
        public int IdTipo { get; set; }
        public string Denominacion { get; set; }
    
    }
}
