using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoPanaderia.Datos;
using ProyectoPanaderia.Models;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using ProyectoPanaderia.Models.Entidades;
using ProyectoPanaderia.Methods;

namespace ProyectoPanaderia.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthenticationService _auth;
        private readonly ILogger<HomeController> _logger;
        private readonly Conexion _con;
        private readonly Insertar _ins;
        private readonly Restricciones _res;

		public HomeController(ILogger<HomeController> logger, Conexion con, Insertar ins, IAuthenticationService auth, Restricciones res)
        {
            _auth = auth;
            _logger = logger;
            _con = con;
            _ins = ins;
            _res = res;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Tiendas()
        {
            return View();
        }
        public IActionResult Contacto()
        {
            return View();
        }
        public IActionResult Conocenos()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string nick, string contraseña)
        {
            var res = from s in _con.Socio where s.NickName == nick select s.Contraseña;
            if (res.Any() && res.Contains(contraseña)){
                var resultado = await SignIn(nick);
                if (resultado != "OK")
                {
                    return Content(resultado);
                }
                return RedirectToAction("Index");
            }
            else
            {
                if (res.Any()){
                    return Json(res);
                }
                TempData["error"] = "Usuario o contraseña incorrectos";
                return View();
            }
        }
        public IActionResult Registro()
        {
            return View();
        }

        public string MayorEdad(DateTime fecha) 
        {
			DateTime hoy = DateTime.Today;
            int res = hoy.Year - fecha.Year;
            return res.ToString();
        }
        [HttpPost]
        public async Task<IActionResult> Registro(string nombre, string apellidos, DateTime fechnac, string email, string nick, string cont, string cont2)
        {
            var nickan = from s in _con.Socio where s.NickName.Contains(nick) select s;
            var primero = _con.Socio.Count();

            var res1 = _res.ValidarCampo(nombre, "nombre");
            var res2 = _res.ValidarCampo(apellidos, "apellidos");
            var res3 = _res.ValidarCampo(cont, "contraseña");
            var res4 = _res.ValidarFecha(fechnac);

            if (res4!="OK")
            {
                TempData["error"] = "Tienes que ser mayor de edad";
                return View();
            }
            else if (res1 != "OK")
            {
                TempData["error"] = "Campo nombre no valido";
                return View();
            }
            else if (res2 != "OK")
            {
                TempData["error"] = "Campo apellidos no valido";
                return View();
            }
            else if (res3 != "OK")
            {
                TempData["error"] = "La contraseña tiene que tener mas de 8 caracteres";
                return View();
            }
            else if (nickan.Any() || nick.Contains("Admin"))
            {
                TempData["error"] = "No puede ser ese nickname";
                return View();
            }
            else if (cont != cont2)
            {
                TempData["error"] = "Las contraseñas no coinciden";
                return View();
            }
            else if (primero == 0)
            {
                Socio socio = new Socio()
                {
                    Nombre = nombre,
                    Apellidos = apellidos,
                    Email = email,
                    FechaNacimiento = fechnac,
                    NickName = nick,
                    Contraseña = cont,
                    IdSocio = 1
                };
                Usuarios usuario = new Usuarios()
                {
                    IdUsuario = 1,
                    IdSocio = 1,
                    IdGrupo = 3
                };
                _con.Socio.Add(socio);
                _con.Usuarios.Add(usuario);
                _con.SaveChanges();


                var res = await SignIn(nick);
                if (res.ToString() == "OK")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "No se  pudo iniciar sesion";
                    return View();
                }
            }
            else
            {
                var ultimo = _con.Socio.Select(s => s.IdSocio).Max();
                ultimo++;
                Socio socio = new Socio()
                {
                    Nombre = nombre,
                    Apellidos = apellidos,
                    Email = email,
                    FechaNacimiento = fechnac,
                    NickName = nick,
                    Contraseña = cont,
                    IdSocio = ultimo
                };

                var ultimo2 = _con.Usuarios.Select(u => u.IdUsuario).Max();
                ultimo2++;
                Usuarios usuario = new Usuarios()
                {
                    IdUsuario = ultimo2,
                    IdSocio = ultimo,
                    IdGrupo = 3
                };
                _con.Socio.Add(socio);
                _con.Usuarios.Add(usuario);
                _con.SaveChanges();

                var res =await SignIn(nick);
                if(res.ToString()=="OK")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "No se  pudo iniciar sesion";
                    return View();
                }
                
            }
        }


            public async Task<string> SignIn(string nickname)
            {
            try
            {
                var grupo = from s in _con.Socio join u in _con.Usuarios on s.IdSocio equals u.IdSocio 
                            join g in _con.Grupos on u.IdGrupo equals g.IdGrupo where s.NickName == nickname select g.Nombre;

                if (grupo.Contains("Admin"))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, nickname),
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    await _auth.SignInAsync(HttpContext, "Cookies", principal, new AuthenticationProperties
                    {
                        IsPersistent = false,
                    });

                    return "OK";

                }
                else if(grupo.Contains("Moderador"))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, nickname),
                        new Claim(ClaimTypes.Role, "Moderador")
                    };
                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    await _auth.SignInAsync(HttpContext, "Cookies", principal, new AuthenticationProperties
                    {
                        IsPersistent = false,
                    });
                    return "OK";
                }
                else 
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, nickname),
                        new Claim(ClaimTypes.Role, "Usuario")
                    };
                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    await _auth.SignInAsync(HttpContext, "Cookies", principal, new AuthenticationProperties
                    {
                        IsPersistent = false,
                    });
                    return "OK";
                }
            }
            catch(Exception ex)
            {
                return ex.Source+"+++"+ex.Message;
            }

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<JsonResult> Taba()
        {
            var resultados = from p in _con.Productos
                             join pt in _con.ProductosPorTipos on p.IdProducto equals pt.IdProducto
                             join t in _con.Tipos on pt.IdTipo equals t.IdTipo
                             join ap in _con.AlergenosPorProductos on p.IdProducto equals ap.IdProducto
                             join a in _con.Alergenos on ap.IdAlergeno equals a.IdAlergeno
                             group a by new { p.IdProducto, p.Name, p.Descripcion, t.IdTipo } into g
                             select new
                             {
                                 g.Key.IdProducto,
                                 g.Key.Name,
                                 g.Key.Descripcion,
                                 g.Key.IdTipo,
                                 Alergenos = string.Join(", ", g.Select(a => a.Descripcion))
                             };

            return Json(resultados);
        }

        public async Task<JsonResult> CargarTipos()
        {
            var result = from T in _con.Tipos select new { T.IdTipo, T.Denominacion };

            return Json(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}