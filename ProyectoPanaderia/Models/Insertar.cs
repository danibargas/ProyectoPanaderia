using Microsoft.AspNetCore.Authentication;
using ProyectoPanaderia.Datos;
using ProyectoPanaderia.Models.Entidades;
using System.Security.Claims;

namespace ProyectoPanaderia.Models
{
	

	public class Insertar
	{
		private readonly Conexion _con;

		public Insertar(Conexion con)
		{
			_con = con;
		}

		//public string Registro(string nombre, string apellidos, DateTime fechnac, string email, string nick, string cont, string cont2)
		//{
		//		//Comprueba existencia

		//		var nickan = from s in _con.Socio where s.NickName == nick select s;
		//		var primero = _con.Socio.Count();
		//		if (nickan.Any() || nick.Contains("Admin"))
		//		{
		//			return "Ya existente";
		//		}
		//		else if (primero == 0)
		//		{
		//			Socio socio = new Socio()
		//			{
		//				Nombre = nombre,
		//				Apellidos = apellidos,
		//				Email = email,
		//				FechaNacimiento = fechnac,
		//				NickName = nick,
		//				Contraseña = cont,
		//				IdSocio = 1
		//			};
		//			Usuarios usuario = new Usuarios()
		//			{
		//				IdUsuario = 1,
		//				IdSocio = 1,
		//				IdGrupo = 3
		//			};
		//			_con.Socio.Add(socio);
		//			_con.Usuarios.Add(usuario);
		//			_con.SaveChanges();

		//			return "OK";
  //          }
		//		else if (cont != cont2)
		//		{
		//			return "Las contraseñas no coinciden";
		//		}
		//		else
		//		{
		//			var ultimo = _con.Socio.Select(s => s.IdSocio).Max();
		//			ultimo++;
		//			Socio socio = new Socio()
		//			{
		//				Nombre = nombre,
		//				Apellidos = apellidos,
		//				Email = email,
		//				FechaNacimiento = fechnac,
		//				NickName = nick,
		//				Contraseña = cont,
		//				IdSocio = ultimo
		//			};

		//			var ultimo2 = _con.Usuarios.Select(u=> u.IdUsuario).Max();
		//			ultimo2++;
		//			Usuarios usuario = new Usuarios()
		//			{
		//				IdUsuario = ultimo2,
		//				IdSocio = ultimo,
		//				IdGrupo = 3
		//			};
		//			_con.Socio.Add(socio);
		//			_con.Usuarios.Add(usuario);
		//			_con.SaveChanges();

		//			return "OK";
		//		}
			
		//}

    }
}
