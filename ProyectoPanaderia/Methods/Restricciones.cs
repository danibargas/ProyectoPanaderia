using System.Text.RegularExpressions;

namespace ProyectoPanaderia.Methods
{
    public class Restricciones
    {
        public string ValidarCampo(string valor, string tipo)
        {
            Regex rex_nom = new Regex("^[a-zA-Z]+$");
            Regex rex_cont = new Regex("^.{8,}$");
            Regex rex_precio = new Regex(@"^\d+(,\d+)*$");
            Regex rex_apellidos = new Regex("^[a-zA-Z\\s]+$");

            switch (tipo)
            {
                case "nombre":
                    if (!rex_nom.IsMatch(valor))
                    {
                        return "error";
                    }
                    else
                    {
                        return "OK";
                    }
                case "apellidos":
                    if (!rex_apellidos.IsMatch(valor))
                    {
                        return "error";
                    }
                    else
                    {
                        return "OK";
                    }
                case "precio":
                    if (!rex_precio.IsMatch(valor))
                    {
                        return "error";
                    }
                    else
                    {
                        return "OK";
                    }
                case "contraseña":
                    if (!rex_cont.IsMatch(valor))
                    {
                        return "error";
                    }
                    else
                    {
                        return "OK";
                    }
                default: return "OK";
            }
        }

        public string ValidarFecha(DateTime fecha)
        {
            DateTime hoy = DateTime.Now;
            int dif = hoy.Year - fecha.Year;

            if (dif < 18)
            {
                return "error";
            }
            else
            {
                return "OK";
            }
        }
    }
}
