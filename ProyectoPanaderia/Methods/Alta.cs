using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProyectoPanaderia.Datos;
using ProyectoPanaderia.Models.Entidades;
using ProyectoPanaderia.Models.ViewModels;

namespace ProyectoPanaderia.Methods
{
    public class Alta
    {
        private readonly Conexion _con;

        public Alta(Conexion con)
        {
            _con = con;
        }
        
    }
}
