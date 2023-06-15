using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNetCore.Mvc;
using ProyectoPanaderia.Datos;
using ProyectoPanaderia.Methods;
using ProyectoPanaderia.Models.Entidades;
using ProyectoPanaderia.Models.ViewModels;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Intrinsics.Arm;

namespace ProyectoPanaderia.Controllers
{
    public class SocioController : Controller
    {
        private readonly Conexion _con;
        private readonly DatosHub _datosHub;
        private readonly Restricciones _res;

        public SocioController(Conexion con, DatosHub datosHub, Restricciones res)
        {
            _datosHub = datosHub;
            _con = con;
            _res = res;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Route("/socio/ProductosPorTipo/{tipo}")]
        public async Task<IActionResult> Productos(int tipo)
        {
            var resultados = _con.Productos
                                .Join(_con.ProductosPorTipos, p => p.IdProducto, pt => pt.IdProducto, (p, pt) => new { p, pt })
                                .Join(_con.Tipos, temp => temp.pt.IdTipo, t => t.IdTipo, (temp, t) => new { temp.p, temp.pt, t })
                                .Join(_con.AlergenosPorProductos, temp2 => temp2.p.IdProducto, ap => ap.IdProducto, (temp2, ap) => new { temp2.p, temp2.pt, temp2.t, ap })
                                .Join(_con.Alergenos, temp3 => temp3.ap.IdAlergeno, a => a.IdAlergeno, (temp3, a) => new { temp3.p, temp3.pt, temp3.t, temp3.ap, a })
                                .Where(temp4 => temp4.t.IdTipo == tipo)
                                .GroupBy(temp5 => new { temp5.p.IdProducto, temp5.p.Name, temp5.p.Descripcion, temp5.t.IdTipo, temp5.p.PrecioUnitario, temp5.p.Cantidad })
                                .Select(g => new ProductoViewModel
                                {
                                    IdProducto = g.Key.IdProducto,
                                    Nombre = g.Key.Name,
                                    Descripcion = g.Key.Descripcion,
                                    Precio = g.Key.PrecioUnitario,
                                    Cantidad = g.Key.Cantidad,
                                    Alergenos = string.Join(", ", g.Select(a => a.a.Descripcion)),
                                    idtipo = g.Key.IdTipo
                                })
                                .ToList();
            List<ProductoViewModel> list = resultados;
            return View(list);
        }


        [Route("/socio/modificarse/{nick}")]
        public async Task<IActionResult> Modificarse(string nick)
        {
            var socio = _con.Socio.FirstOrDefault(e => e.NickName == nick);

            if (socio != null)
            {
                Socio resultado = socio;
                return View(resultado);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Route("/socio/modificarse/{id}")]
        public async Task<IActionResult> Modificarse(int id, string nombre, string apellidos, string nick, string email, string cont, string cont2, DateTime fecha)
        {
            DateTime hoy = DateTime.Now;
            int años=hoy.Year-fecha.Year;
            using (var tansaction = _con.Database.BeginTransaction())
            {
                try
                {
                    var email2=from s in _con.Socio where s.Email.Contains(email) select s;
                    var socio = _con.Socio.FirstOrDefault(sg => sg.IdSocio == id);

                    var res1 = _res.ValidarCampo(nombre, "nombre");
                    var res2 = _res.ValidarCampo(apellidos, "apellidos");
                    var res3 = _res.ValidarCampo(cont, "contraseña");
                    if (socio != null)
                    {
                        if (res1 != "OK")
                        {
                            TempData["error"] = "Campo nombre no valido";
                            return View(socio);
                        }
                        else if (res2 != "OK")
                        {
                            TempData["error"] = "Campo apellidos no valido";
                            return View(socio);
                        }
                        if (res3 != "OK")
                        {
                            TempData["error"] = "La contraseña debe tener un minimo de 8 caracteres";
                            return View(socio);
                        }
                        else if (cont != cont2)
                        {
                            TempData["error"] = "Las contraseñas no coinciden";
                            return View(socio);
                        }
                        else if (socio.Email != email && email2.Any())
                        {
                            TempData["error"] = "Este email ya esta en uso";
                            return View(socio);
                        }
                        else if (años < 18)
                        {
                            TempData["error"] = "Tienes que ser mayor de edad";
                            return View(socio);
                        }
                        else
                        {
                            socio.Nombre = nombre;
                            socio.Apellidos = apellidos;
                            socio.Email = email;
                            socio.Contraseña = cont;
                            socio.FechaNacimiento = fecha;
                            _con.SaveChanges();

                            tansaction.Commit();
                            await _datosHub.ModificarSocio(id, nombre, apellidos, nick, email, cont, fecha);

                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "No se encontró el socio" });
                    }
                }

                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error al actualizar el grupo del socio", error = fecha });
                }
            }
        }
        public async Task<IActionResult> AnadirProducto(int idproducto, string socio, string precio)
        {

            try
            {
                var id = (from s in _con.Socio where s.NickName.Contains(socio) select s.IdSocio).Single();
                var pedido = (from p in _con.Pedido
                              join s in _con.Socio on p.IdSocio equals s.IdSocio
                              where s.IdSocio == id && p.Estado.Contains("PENDIENTE")
                              select p).SingleOrDefault();


                if (pedido == null )
                {
                    var cont = _con.Pedido.Count();
                    if (cont == 0)
                    {
                        DateTime hoy = DateTime.Now;
                        Pedido ped = new Pedido
                        {
                            IdPedido = 1,
                            IdSocio = id,
                            Estado = "Pendiente",
                            Cobrado = "No",
                            FechaRealizacion = hoy,
                            Total = double.Parse(precio)
                        };

                        DetallePedido dp = new DetallePedido
                        {
                            id = 1,
                            IdPedido = 1,
                            IdProducto = idproducto,
                            Cantidad = 1,
                            PrecioUnitario = double.Parse(precio),
                            Total = double.Parse(precio)
                        };

                        _con.Pedido.Add(ped);
                        _con.DetallePedido.Add(dp);
                        _con.SaveChanges();
                        return Json(new { success = true, message = socio + "--" + id });
                    }
                    else
                    {
                        var ultimo = _con.Pedido.Select(x => x.IdPedido).Max();
                        ultimo++;
                        DateTime hoy = DateTime.Now;
                        Pedido ped = new Pedido
                        {
                            IdPedido = ultimo,
                            IdSocio = id,
                            Estado = "Pendiente",
                            Cobrado = "No",
                            FechaRealizacion = hoy,
                            Total = double.Parse(precio)
                        };
                        var ultimo2 = _con.DetallePedido.Select(g => g.id).Max();
                        ultimo2++;
                        DetallePedido dp = new DetallePedido
                        {
                            id = ultimo2,
                            IdPedido = ultimo,
                            IdProducto = idproducto,
                            Cantidad = 1,
                            PrecioUnitario = double.Parse(precio),
                            Total = double.Parse(precio)
                        };

                        _con.Pedido.Add(ped);
                        _con.DetallePedido.Add(dp);
                        _con.SaveChanges();
                        return Json(new { success = true, message = socio + "--" + id });
                    }
                }
                else
                {
                    var pañadido = _con.DetallePedido.Where(p => p.IdProducto == idproducto).Where(pd => pd.IdPedido == pedido.IdPedido).SingleOrDefault();
                    if (pañadido != null)
                    {
                        pañadido.Cantidad = pañadido.Cantidad + 1;
                        pañadido.Total = pañadido.Total + double.Parse(precio);
                        pedido.Total = pedido.Total + double.Parse(precio);
                        _con.SaveChanges();
                        return Json(new { success = true, message = socio + "--" + id });
                    }
                    else 
                    {

                        var ultimo = _con.DetallePedido.Select(p => p.id).Max();
                        ultimo++;
                        DetallePedido dp = new DetallePedido
                        {
                            id = ultimo,
                            IdPedido = pedido.IdPedido,
                            IdProducto = idproducto,
                            Cantidad = 1,
                            PrecioUnitario = double.Parse(precio),
                            Total = double.Parse(precio)
                        };

                        _con.DetallePedido.Add(dp);

                        pedido.Total = pedido.Total + dp.Total;

                        _con.SaveChanges();

                        return Json(new { success = true, message = socio + "--" + id });
                    }

                }
            }catch(Exception ex)
            {
                var id = (from s in _con.Socio where s.NickName.Contains(socio) select s.IdSocio).Single();
                var pedido = (from p in _con.Pedido
                              join s in _con.Socio on p.IdSocio equals s.IdSocio
                              where s.IdSocio == id && p.Estado.Contains("PENDIENTE")
                              select p).SingleOrDefault();
               
                if (pedido == null)
                {
                    return Json(new { success = true, message = socio + "--" + id + "---null  "+ex.Message+ "   "+ex.InnerException }); ;
                }
                else
                {
                    return Json(new { success = true, message = socio + "--" + id + "---" + pedido.IdSocio }); ;
                }
                
            }
        }
        [Route("/socio/carrito/{nick}")]
        public async Task<IActionResult> Carrito(string nick)
        {
            var idpedido =( from s in _con.Socio join p in _con.Pedido on s.IdSocio equals p.IdSocio where s.NickName.Contains(nick) where p.Estado.Contains("Pendiente") select p.IdPedido).FirstOrDefault();
            var carrito = _con.DetallePedido.Where(x => x.IdPedido == idpedido);
            var productos = _con.Productos;
            Detalles comb = new Detalles
            {
                Detalle = carrito.ToList(),
                Productos = productos.ToList(),
            };

            return View(comb);
        }

        public async Task<IActionResult> CambiarCantidad(int id, int idpedido, int cantidad)
        {
            var prod = _con.DetallePedido.Where(x => x.id == id).Where(x => x.IdPedido == idpedido).Single();
            var pedido = _con.Pedido.Where(s => s.IdPedido == idpedido).Single();
            pedido.Total-=prod.Total;
            prod.Cantidad = cantidad;
            prod.Total= cantidad*prod.PrecioUnitario;
            pedido.Total+= prod.Total;

            _con.SaveChanges();
            return Json(new { success = true, message = "Cambiado correctamente" }); ;
        }

        public async Task<IActionResult> DetallesPedido(int idpedido)
        {
            var detalles = (from p in _con.Productos
                           join dp in _con.DetallePedido on p.IdProducto equals dp.IdProducto
                           join pd in _con.Pedido on dp.IdPedido equals pd.IdPedido
                           where pd.IdPedido==idpedido
                           select new DetallePedidos
                           {
                               cantidad = dp.Cantidad,
                               Producto = p.Name,
                               Total = dp.Total,
                               TotalProducto = pd.Total
                           }).ToList();

            return View(detalles);
        }

        public async Task<IActionResult> EliminarProducto(int id, int cantidad, int idpedido)
        {
            var detalle = _con.DetallePedido.Where(x => x.id == id).Single();
            var pedido = _con.Pedido.Where(w => w.IdPedido == idpedido).Single();
            pedido.Total -= cantidad;
            _con.DetallePedido.Remove(detalle);
            _con.SaveChanges();

            return Json(new { success = true, message = "Eliminado correctamente" });
        }

        public async Task<IActionResult> ConfirmarPedido(List<int> idproducto, List<int> cantidad, int idpedido, string nick)
        {
            try
            {
                var idp = (from s in _con.Socio join p in _con.Pedido on s.IdSocio equals p.IdSocio 
                           where s.NickName.Contains(nick) where p.Estado.Contains("Pendiente") select p.IdPedido).FirstOrDefault();
                var carrito = _con.DetallePedido.Where(e => e.IdPedido == idpedido).ToList();
                var idsocio = _con.Socio.Where(w => w.NickName.Contains(nick)).Select(x => x.IdSocio).SingleOrDefault();
                for (int i = 0; i < idproducto.Count; i++)
                {
                    int id = idproducto[i];
                    int c = cantidad[i];
                    var producto = _con.Productos.Where(p => p.IdProducto == id).Single();
                    producto.Cantidad -= c;
                    if (producto.Cantidad < 0)
                    {
                        TempData["error"] = "Debes seleccionar menos unidades";
                        return View("Carrito", carrito);
                    }
                    else
                    {
                        var pedido = _con.Pedido.Where(p => p.IdPedido == idpedido).Single();
                        pedido.Estado = "Finalizado";

                    }
                }
                _con.SaveChanges();

                return Json(new { success = true, message = "Todo ok" });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            
            //
            //return Json(new { success = true, message = "Eliminado correctamente" });
        }

        [Route("/socio/pedidossocio/{nick}")]
        public async Task<IActionResult> PedidosSocio(string nick)
        {
            var id = _con.Socio.Where(x => x.NickName.Contains(nick)).Select(x => x.IdSocio).Single();
            var pedidos=_con.Pedido.Where(y=>y.IdSocio==id).ToList();

            return View(pedidos); 
        }
        
        public async Task MandarEmail(int idpedido, int idsocio)
        {
            var socio = _con.Socio.Where(i => i.IdSocio == idsocio).Select(i => i.Email).Single();
            var detalles = _con.DetallePedido.Where(p => p.IdPedido == idpedido);
            var productos = _con.Productos;
            var pedido = _con.Pedido.Where(d => d.IdPedido == idpedido).Select(d => d.Total).Single();

            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("dani.bargas2001@gmail.com", "loschispos");

            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Host = "smtp.gmail.com";
            client.Timeout = 20000;

            MailMessage message = new MailMessage();
            message.From = new MailAddress("dani.bargas2001@gmail.com");
            message.To.Add(socio);
            message.Subject = "Confirmacion de su pedido";
            message.Body="<h3>Gracias por confiar en nosotros, los detalles de su pedido son:</h3><hr/>";

            foreach(var p in detalles)
            {
                foreach(var s in productos)
                {
                    if(p.IdProducto==s.IdProducto)
                    {
                        message.Body += $"<p>x{p.Cantidad}  {s.Name}  =  {p.Total}</p>";
                    }
                    
                }
                
            }
            message.Body += $"<h5>Total: {pedido}</h5>";
            message.IsBodyHtml = true;
            client.Send(message);

        }
    }
}