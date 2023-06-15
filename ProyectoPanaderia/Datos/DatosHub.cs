using Microsoft.AspNetCore.SignalR;
using ProyectoPanaderia.Datos;
using ProyectoPanaderia.Models.ViewModels;
using System.Drawing;
using System.Threading.Tasks;

public class DatosHub : Hub
{
    private readonly Conexion _con;

    public DatosHub(Conexion con)
    {
        _con = con;
    }

    public async Task ActualizarGrupoSocio(int socioId, int nuevoGrupoId)
    {
        try
        {
            var socio = _con.Usuarios.FirstOrDefault(sg => sg.IdSocio == socioId);
            if (socio != null)
            {
                socio.IdGrupo = nuevoGrupoId;
                _con.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("No se encontró el socio");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al actualizar el grupo del socio", ex);
        }
    }
    public async Task ModificarSocio(int id, string nombre, string apellidos, string nick, string email, string cont, DateTime fecha)
    {
        try
        {
            var socio = _con.Socio.FirstOrDefault(sg => sg.IdSocio == id);
            if (socio != null)
            {
                socio.Nombre = nombre;
                socio.Apellidos = apellidos;
                socio.NickName = nick;
                socio.Email = email;
                socio.Contraseña = cont;
                socio.FechaNacimiento = fecha;
                _con.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("No se encontró el socio");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al actualizar el grupo del socio", ex);
        }
    }
    public List<SociosViewModel> ObtenerListadoSocios()
    {
        var socios = from s in _con.Socio
                     join sg in _con.Usuarios on s.IdSocio equals sg.IdSocio
                     join g in _con.Grupos on sg.IdGrupo equals g.IdGrupo
                     select new SociosViewModel
                     {
                         IdSocio = s.IdSocio,
                         Nombre = s.Nombre,
                         Apellidos = s.Apellidos,
                         Grupo = g.Nombre
                     };

        return socios.ToList();
    }
}

