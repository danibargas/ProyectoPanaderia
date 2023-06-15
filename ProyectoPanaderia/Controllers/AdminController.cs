using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using ProyectoPanaderia.Datos;
using ProyectoPanaderia.Methods;
using ProyectoPanaderia.Models.Entidades;
using ProyectoPanaderia.Models.ViewModels;
using System.Linq;
using System.Security.Policy;

namespace ProyectoPanaderia.Controllers
{
    

    public class AdminController : Controller
    {
        private readonly Conexion _con;
        private readonly DatosHub _hubContext;
        private readonly Restricciones _res;

        public AdminController(Conexion con, DatosHub hubContext, Restricciones res)
        {
            _con = con;
            _hubContext = hubContext;
            _res=res;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Route("/admin/listadousuarios/{nick}")]
        public IActionResult ListadoUsuarios(string nick)
        {
            try
            {
                var socios = from s in _con.Socio
                             join u in _con.Usuarios on s.IdSocio equals u.IdSocio
                             join g in _con.Grupos on u.IdGrupo equals g.IdGrupo
                             where !s.NickName.Contains(nick)
                             select new SociosViewModel
                             {
                                 IdSocio = s.IdSocio,
                                 Nombre = s.Nombre,
                                 Apellidos = s.Apellidos,
                                 NickName = s.NickName,
                                 Email = s.Email,
                                 FechaNac = s.FechaNacimiento.Date,
                                 Grupo = g.Nombre
                             };

                var grupos = from g in _con.Grupos
                             where !g.Nombre.Contains("Admin")
                             select new GruposViewModel
                             {
                                 IdGrupo = g.IdGrupo,
                                 Nombre = g.Nombre
                             };

                var model = new TablaUsuarios
                {
                    Socios = socios.ToList(),
                    Grupos = grupos.ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return Content(ex.Source + "---" + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarSocio(int socioId)
        {
            try
            {
                var socio = _con.Socio.FirstOrDefault(e => e.IdSocio == socioId);
                _con.Socio.Remove(socio);
                _con.SaveChanges();

                return Json(new { success = true, message = "Grupo actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el grupo del socio", error = ex.Message});
            }
        }


        [HttpPost]
        public IActionResult ActualizarGrupoSocio(int socioId, int nuevoGrupoId)
        {
            try
            {
                var socio = _con.Usuarios.FirstOrDefault(sg => sg.IdSocio == socioId);
                if (socio != null)
                {
                    socio.IdGrupo = nuevoGrupoId;
                    _con.SaveChanges();

                    // Llamar al método ActualizarGrupoSocio del Hub para notificar a los clientes
                    _hubContext.ActualizarGrupoSocio(socioId, nuevoGrupoId);

                    return Json(new { success = true, message = "Grupo actualizado correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se encontró el socio" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el grupo del socio", error = ex.Message });
            }
        }

        public async Task<IActionResult> ListaTipos()
        {
            var tipos = _con.Tipos.ToList();
            return View(tipos);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarTipo(int idtipo)
        {
            try
            {
                var tipo = _con.Tipos.FirstOrDefault(e => e.IdTipo == idtipo);
                _con.Tipos.Remove(tipo);
                _con.SaveChanges();

                return Json(new { success = true, message = "Tipo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el tipo", error = ex.Message });
            }
        }

        public async Task<IActionResult> AltaTipo()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AltaTipo(string nombre)
        {
            try
            {
                var cont = _con.Tipos.Count();
                var tipos = from t in _con.Tipos where t.Denominacion.Contains(nombre) select t;

                if(_res.ValidarCampo(nombre, "nombre")=="error"){
                    TempData["error"] = "Campo no valido";
                    return View();
                }
                else if (tipos != null && tipos.Any())
                {
                    TempData["error"] = "Ya existe este tipo";
                    return View();
                }
                else if (cont == 0)
                {
                    Tipos t = new Tipos
                    {
                        IdTipo = 1,
                        Denominacion = nombre,
                    };
                    _con.Tipos.Add(t);
                    _con.SaveChanges();

                    TempData["ok"] = "Tipo " + nombre + " añadido correctamente";
                    return View();
                }
                else
                {
                    var ultimo = _con.Tipos.Select(s => s.IdTipo).Max();

                    Tipos tipo = new Tipos
                    {
                        IdTipo = ultimo + 1,
                        Denominacion = nombre,
                    };
                    _con.Tipos.Add(tipo);
                    _con.SaveChanges();

                    TempData["ok"] = "Tipo " + nombre + " añadido correctamente";
                    return View();
                }
            }catch (Exception ex)
            {
                return Content(ex.ToString());
            }
            
        }
        //------------------------------------------------------------------------------------------
        public async Task<IActionResult> ListadoAlergenos()
        {
            var tipos = _con.Alergenos.ToList();
            return View(tipos);
        }
        //----------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> EliminarAlergeno(int idalergeno)
        {
            try
            {
                var alergeno = _con.Alergenos.FirstOrDefault(e => e.IdAlergeno == idalergeno);
                _con.Alergenos.Remove(alergeno);
                _con.SaveChanges();

                return Json(new { success = true, message = "Tipo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el tipo", error = ex.Message });
            }
        }
        //_--------------------------------------------------------------------------------------------------
        public async Task<IActionResult> AltaAlergeno()
        {
            return View();
        }
        //----------------------------------------------------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> AltaAlergeno(string nombre)
        {
            try
            {
                var cont = _con.Alergenos.Count();
                var alergenos = from a in _con.Alergenos where a.Descripcion.Contains(nombre) select a;
                var respo = _res.ValidarCampo(nombre, "nombre");

                if( respo != "OK"){
                    TempData["error"] = "Campo no valido";
                    return View();
                }
                else if (alergenos != null && alergenos.Any())
                {
                    TempData["error"] = "Ya existe este alergeno";
                    return View();
                }
                else if (cont == 0)
                {
                    Alergenos a = new Alergenos
                    {
                        IdAlergeno = 1,
                        Descripcion = nombre,
                    };
                    _con.Alergenos.Add(a);
                    _con.SaveChanges();

                    TempData["ok"] = "Alergeno " + nombre + " añadido correctamente";
                    return View();
                }
                else
                {
                    var ultimo = _con.Alergenos.Select(a => a.IdAlergeno).Max();

                    Alergenos alergeno = new Alergenos
                    {
                        IdAlergeno = ultimo + 1,
                        Descripcion = nombre,
                    };
                    _con.Alergenos.Add(alergeno);
                    _con.SaveChanges();

                    TempData["ok"] = "Alergeno " + nombre + " añadido correctamente";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }

        }
        //----------------------------------------------------------
        public async Task<IActionResult> ListadoPedidos()
        {
            var pedidos = from p in _con.Pedido
                          join s in _con.Socio on p.IdSocio equals s.IdSocio orderby p.Estado descending, p.Cobrado ascending
                          select new PedidoViewModel
                          {
                              IdPedido = p.IdPedido,
                              NickName = s.NickName,
                              Fecha = DateOnly.FromDateTime(p.FechaRealizacion.Date),
                              Estado = p.Estado,
                              Cobrado = p.Cobrado,
                              Precio = p.Total
                          };
            return View(pedidos.ToList());
        }
        //----------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CambiarCobrado(int idpedido, string cobrado)
        {
            try
            {
                var pedido = _con.Pedido.FirstOrDefault(e => e.IdPedido == idpedido);
                pedido.Cobrado = cobrado;
                _con.SaveChanges();

                return Json(new { success = true, message = "Tipo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el tipo", error = ex.Message });
            }
        }
        //-------------------------------------------------------------
        public async Task<IActionResult> Detalles(int idpedido)
        {
            var detalles = (from p in _con.Productos
                            join dp in _con.DetallePedido on p.IdProducto equals dp.IdProducto
                            join pd in _con.Pedido on dp.IdPedido equals pd.IdPedido
                            where pd.IdPedido == idpedido
                            select new DetallePedidos
                            {
                                cantidad = dp.Cantidad,
                                Producto = p.Name,
                                Total = dp.Total,
                                TotalProducto = pd.Total
                            }).ToList();

            return View(detalles);
        }
        //-------------------------------------------------------------
        public async Task<IActionResult> EliminarPedido(int idpedido)
        {
            try
            {
                var pedido = _con.Pedido.FirstOrDefault(e => e.IdPedido == idpedido);
                _con.Pedido.Remove(pedido);
                _con.SaveChanges();

                return Json(new { success = true, message = "Pedido eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el pedido", error = ex.Message });
            }
        }
        //--------------------------------------------------------------
        public async Task<IActionResult> ListadoProductos()
        {
            var productos = _con.Productos.ToList();
            return View(productos);
        }
        //----------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> EliminarProducto(int idproducto)
        {
            try
            {
                var producto = _con.Productos.FirstOrDefault(e => e.IdProducto == idproducto);
                _con.Productos.Remove(producto);
                _con.SaveChanges();

                return Json(new { success = true, message = "Tipo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el tipo", error = ex.Message });
            }
        }
        //---------------------------------------------------------------
        public async Task<IActionResult> AltaProducto()
        {
            var alergenos = _con.Alergenos.ToList();
            var tipos = _con.Tipos.ToList();

            var model = new AltaProductoViewModel
            {
                Alergenos = alergenos,
                Tipos = tipos
            };

            return View(model);
        }
        //----------------------------------------------------------------
        [HttpGet]
        [Route("/admin/editproducto/{idproducto}")]
        public async Task<IActionResult> EditProducto(int idproducto)
        {
            var resultados = _con.Productos
                                .Join(_con.ProductosPorTipos, p => p.IdProducto, pt => pt.IdProducto, (p, pt) => new { p, pt })
                                .Join(_con.Tipos, temp => temp.pt.IdTipo, t => t.IdTipo, (temp, t) => new { temp.p, temp.pt, t })
                                .Join(_con.AlergenosPorProductos, temp2 => temp2.p.IdProducto, ap => ap.IdProducto, (temp2, ap) => new { temp2.p, temp2.pt, temp2.t, ap })
                                .Join(_con.Alergenos, temp3 => temp3.ap.IdAlergeno, a => a.IdAlergeno, (temp3, a) => new { temp3.p, temp3.pt, temp3.t, temp3.ap, a })
                                .Where(temp4 => temp4.p.IdProducto == idproducto)
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
            var alergenos = _con.Alergenos.ToList();
            var tipos = _con.Tipos.ToList();

            var model = new ProductoEdit
            {
                Alergenos = alergenos,
                Tipos = tipos,
                Productos = resultados
            };

            return View(model);

        }
        //----------------------------------------------------------------
        [HttpPost]
        [Route("/admin/edit/{idproducto}")]
        public async Task<IActionResult> Edit(int idproducto, string nombre, string des, int cantidad, string precio, int tipo, List<int> alerg)
        {
            var resultados = (from p in _con.Productos
                              join pt in _con.ProductosPorTipos on p.IdProducto equals pt.IdProducto
                              join t in _con.Tipos on pt.IdTipo equals t.IdTipo
                              join ap in _con.AlergenosPorProductos on p.IdProducto equals ap.IdProducto
                              join a in _con.Alergenos on ap.IdAlergeno equals a.IdAlergeno
                              where p.IdProducto == idproducto
                              group a by new { p.IdProducto, p.Name, p.Descripcion, t.IdTipo, p.PrecioUnitario, p.Cantidad } into g
                              select new ProductoViewModel
                              {
                                  IdProducto = g.Key.IdProducto,
                                  Nombre = g.Key.Name,
                                  Descripcion = g.Key.Descripcion,
                                  Precio = g.Key.PrecioUnitario,
                                  Cantidad = g.Key.Cantidad,
                                  Alergenos = string.Join(", ", g.Select(a => a.Descripcion)),
                                  idtipo = g.Key.IdTipo
                              }).ToList();

            var alergenos = _con.Alergenos.ToList();
            var tipos = _con.Tipos.ToList();

            var model = new ProductoEdit
            {
                Alergenos = alergenos,
                Tipos = tipos,
                Productos = resultados
            };

            var res = _con.Productos.ToList();

            var producto = _con.Productos.FirstOrDefault(p => p.IdProducto == idproducto);
            producto.Name = nombre;
            producto.Descripcion = des;
            producto.Cantidad = cantidad;
            producto.PrecioUnitario = double.Parse(precio);

            var productoTipo = _con.ProductosPorTipos.FirstOrDefault(pt => pt.IdProducto == idproducto);
            productoTipo.IdTipo = tipo;

                

            if (!alerg.Any() && alerg == null)
            {
                TempData["error"] = "Selecciona algun alergeno";
                return Json(alerg);

            }
            else 
            {

                // ...

                var adas = _con.AlergenosPorProductos.Where(w => w.IdProducto == idproducto);
                _con.AlergenosPorProductos.RemoveRange(adas);
                _con.SaveChanges();

                var ultimo = _con.AlergenosPorProductos.Select(a => a.Id).DefaultIfEmpty(0).Max();
                foreach (var a in alerg)
                {
                    ultimo++;

                 
                        AlergenosPorProducto ap = new AlergenosPorProducto
                        {
                            Id = ultimo,
                            IdProducto = idproducto,
                            IdAlergeno = a
                        };
                        _con.AlergenosPorProductos.Add(ap);
                }
                _con.SaveChanges();
                return View("ListadoProductos", res);

            }
        }
        //----------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> AltaProducto(string nombre, string des, int tipo, int cantidad, string precio, List<int> aler)
        {
            try
            {
                var alergenos = _con.Alergenos.ToList();
                var tipos = _con.Tipos.ToList();

                var model = new AltaProductoViewModel
                {
                    Alergenos = alergenos,
                    Tipos = tipos
                };

                var res1 = _res.ValidarCampo(nombre, "apellidos");
                var res2 = _res.ValidarCampo(des, "apellidos");
                var res3 = _res.ValidarCampo(precio, "precio");

                var existente = from p in _con.Productos where p.Name.Contains(nombre) select p;
                var primero = _con.Productos.Count();

                if (res1 != "OK")
                {
                    TempData["error"] = "Campo nombre no valido";
                    return View(model);
                }
                else if (res2 != "OK")
                {
                    TempData["error"] = "Campo descripcion no valido";
                    return View(model);
                }
                else if (res3 != "OK")
                {
                    TempData["error"] = "Campo precio no valido";
                    return View(model);
                }
                else if (cantidad<=0)
                {
                    TempData["error"] = "Introduzca una cantidad valida";
                    return View(model);
                }
                else if (existente != null && existente.Any())
                {
                    TempData["error"] = "Este producto ya existe";
                    return View(model);
                }
                else if(!aler.Any()){
                    TempData["error"] = "Seleccione un alérgeno";
                    return View(model);
                }
                else if (primero == 0)
                {
                    Productos p = new Productos
                    {
                        IdProducto = 1,
                        Cantidad = cantidad,
                        Descripcion = des,
                        Name = nombre,
                        PrecioUnitario = double.Parse(precio)
                    };
                    _con.Productos.Add(p);
                    var contTipo = _con.ProductosPorTipos.Count();
                    if (contTipo == 0)
                    {
                        ProductosPorTipo productosPorTipo = new ProductosPorTipo
                        {
                            Id = 1,
                            IdProducto = 1,
                            IdTipo = tipo
                        };
                        _con.ProductosPorTipos.Add(productosPorTipo);

                        var contAler = _con.AlergenosPorProductos.Count();
                        if (contAler == 0)
                        {
                            contAler += 1;
                            foreach (var a in aler)
                            {

                                AlergenosPorProducto al = new AlergenosPorProducto
                                {
                                    Id = contAler,
                                    IdAlergeno = a,
                                    IdProducto = 1
                                };
                                _con.AlergenosPorProductos.Add(al);
                                contAler++;
                            }
                            _con.SaveChanges();
                            TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                            return View(model);
                        }
                        else
                        {
                            var ultimoAl = _con.AlergenosPorProductos.Select(a => a.Id).Max();
                            ultimoAl++;
                            foreach (var a in aler)
                            {

                                AlergenosPorProducto al = new AlergenosPorProducto
                                {
                                    Id = ultimoAl,
                                    IdAlergeno = a,
                                    IdProducto = 1
                                };
                                _con.AlergenosPorProductos.Add(al);
                                ultimoAl++;
                            }
                            _con.SaveChanges();
                            TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                            return View(model);

                        }
                    }
                    else
                    {
                        var ultimoTipo = _con.ProductosPorTipos.Select(a => a.Id).Max();
                        ultimoTipo += 1;

                        ProductosPorTipo productosPorTipo = new ProductosPorTipo
                        {
                            Id = ultimoTipo,
                            IdProducto = 1,
                            IdTipo = tipo
                        };
                        _con.ProductosPorTipos.Add(productosPorTipo);


                        var contAler = _con.AlergenosPorProductos.Count();
                        if (contAler == 0)
                        {
                            contAler += 1;
                            foreach (var a in aler)
                            {

                                AlergenosPorProducto al = new AlergenosPorProducto
                                {
                                    Id = contAler,
                                    IdAlergeno = a,
                                    IdProducto = 1
                                };
                                _con.AlergenosPorProductos.Add(al);
                                contAler++;
                            }
                            _con.SaveChanges();
                            TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                            return View(model);
                        }
                        else
                        {
                            var ultimoAl = _con.AlergenosPorProductos.Select(a => a.Id).Max();
                            ultimoAl++;
                            foreach (var a in aler)
                            {

                                AlergenosPorProducto al = new AlergenosPorProducto
                                {
                                    Id = ultimoAl,
                                    IdAlergeno = a,
                                    IdProducto = 1
                                };
                                _con.AlergenosPorProductos.Add(al);
                                ultimoAl++;
                            }
                            _con.SaveChanges();
                            TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                            return View(model);

                        }
                    }
                }
                else
                {
                    var ultimo = _con.Productos.Select(p => p.IdProducto).Max();
                    ultimo++;

                    Productos pro = new Productos
                    {
                        IdProducto = ultimo,
                        Name = nombre,
                        Cantidad = cantidad,
                        Descripcion = des,
                        PrecioUnitario = double.Parse(precio),
                    };
                    _con.Productos.Add(pro);


                    var contTipos = _con.ProductosPorTipos.Count();
                    if (contTipos == 0)
                    {
                        ProductosPorTipo productosPorTipo = new ProductosPorTipo
                        {
                            Id = 1,
                            IdProducto = ultimo,
                            IdTipo = tipo
                        };
                        _con.ProductosPorTipos.Add(productosPorTipo);

                        var contAler = _con.AlergenosPorProductos.Count();
                        if (contAler == 0)
                        {
                            contAler += 1;
                            foreach (var a in aler)
                            {

                                AlergenosPorProducto al = new AlergenosPorProducto
                                {
                                    Id = contAler,
                                    IdAlergeno = a,
                                    IdProducto = ultimo
                                };
                                _con.AlergenosPorProductos.Add(al);
                                contAler++;
                            }
                            _con.SaveChanges();
                            TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                            return View(model);
                        }
                        else
                        {
                            var ultimoAl = _con.AlergenosPorProductos.Select(a => a.Id).Max();
                            ultimoAl++;
                            foreach (var a in aler)
                            {

                                AlergenosPorProducto al = new AlergenosPorProducto
                                {
                                    Id = ultimoAl,
                                    IdAlergeno = a,
                                    IdProducto = ultimo
                                };
                                _con.AlergenosPorProductos.Add(al);
                                ultimoAl++;
                            }
                            _con.SaveChanges();
                            TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                            return View(model);

                        }
                    }
                    else
                    {
                        var ultimoTipo=_con.ProductosPorTipos.Select(a => a.Id).Max();
                            ultimoTipo++;
                            ProductosPorTipo productosPorTipo = new ProductosPorTipo
                            {
                                Id = ultimoTipo,
                                IdProducto = ultimo,
                                IdTipo = tipo
                            };
                            _con.ProductosPorTipos.Add(productosPorTipo);

                            var contAler = _con.AlergenosPorProductos.Count();
                            if (contAler == 0)
                            {
                                contAler += 1;
                                foreach (var a in aler)
                                {

                                    AlergenosPorProducto al = new AlergenosPorProducto
                                    {
                                        Id = contAler,
                                        IdAlergeno = a,
                                        IdProducto = ultimo
                                    };
                                    _con.AlergenosPorProductos.Add(al);
                                    contAler++;
                                }
                                _con.SaveChanges();
                                TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                                return View(model);
                        }
                            else
                            {
                                var ultimoAl = _con.AlergenosPorProductos.Select(a => a.Id).Max();
                                ultimoAl++;
                                foreach (var a in aler)
                                {

                                    AlergenosPorProducto al = new AlergenosPorProducto
                                    {
                                        Id = ultimoAl,
                                        IdAlergeno = a,
                                        IdProducto = ultimo
                                    };
                                    _con.AlergenosPorProductos.Add(al);
                                    ultimoAl++;
                                }
                                _con.SaveChanges();
                                TempData["ok"] = "Producto " + nombre + " añadido correctamente";
                                return View(model);
                            }
                        }
                    }
            }

            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }


    }
}


