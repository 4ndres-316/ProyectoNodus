using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public class ProveedorItem
    {
        public long NitProveedor { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class ItemOferta
    {
        public string Clave { get; set; } = string.Empty;
        public long NitProveedor { get; set; }
        public int IdServicio { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string NombreServicio { get; set; } = string.Empty;
        public string NombreItem { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string PrecioFormateado => $"Bs {Precio:F2}";
    }

    public class ItemRecinto
    {
        public long IdRecinto { get; set; }
        public string NombreRecinto { get; set; } = string.Empty;
        public string TipoRecinto { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public long PrecioHoraRaw { get; set; }
        public string PrecioFormateado => $"Bs {PrecioHoraRaw}/hr";
    }

    public class ItemCarrito
    {
        public string Clave { get; set; } = string.Empty;
        public bool EsRecinto { get; set; }
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
        public string SubtotalFormateado => $"Bs {Subtotal:F2}";
    }

    public class InvitadoTemporal
    {
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }

    public partial class NuevoPedidoDialog : Window
    {
        private int _idUsuario;
        private List<MetodoPago> _metodosPago;
        private List<ItemOferta> _ofertasCompletas = new List<ItemOferta>();
        private List<ItemCarrito> _carrito = new List<ItemCarrito>();
        private List<InvitadoTemporal> _invitados = new List<InvitadoTemporal>();
        private long _recintoSeleccionadoId = 0;
        private Usuario _clientePreseleccionado;

        public NuevoPedidoDialog(int idUsuario, List<MetodoPago> metodosPago, Usuario clientePreseleccionado = null)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
            _metodosPago = metodosPago ?? new List<MetodoPago>();
            _clientePreseleccionado = clientePreseleccionado;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFechaInicio.SelectedDate = DateTime.Today;
            dpFechaFin.SelectedDate = DateTime.Today;

            if (_clientePreseleccionado != null)
            {
                Title = $"Crear Evento — {_clientePreseleccionado.NombreUsuario}";
            }

            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                // Recintos disponibles
                var recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models
                    .Where(r => !string.Equals(r.EstadoRecinto, "Clausurado", StringComparison.OrdinalIgnoreCase))
                    .Select(r => new ItemRecinto
                    {
                        IdRecinto = r.IdRecinto,
                        NombreRecinto = r.NombreRecinto,
                        TipoRecinto = r.TipoRecinto,
                        Capacidad = r.Capacidad,
                        PrecioHoraRaw = r.PrecioHora,
                    })
                    .ToList();

                lvRecintos.ItemsSource = recintos;

                // Catalogo de servicios por proveedor
                var servicios = (await ConexionDB.Client.From<Servicio>().Get()).Models
                    .Where(s => !string.Equals(s.Estado, "Inactivo", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var categorias = (await ConexionDB.Client.From<Categoria>().Get()).Models;
                var proveedores = (await ConexionDB.Client.From<Proveedor>().Get()).Models;
                var detalles = (await ConexionDB.Client.From<DetalleProveedorServicio>().Get()).Models;
                var proveedorServicios = (await ConexionDB.Client.From<ProveedorServicio>().Get()).Models;

                _ofertasCompletas = new List<ItemOferta>();

                foreach (var s in servicios)
                {
                    var cat = categorias.Find(c => c.IdCategoria == s.IdCategoria);
                    var nombreCat = cat?.NombreCategoria ?? "";
                    var itemsDetalle = detalles.Where(d => d.IdServicio == s.IdServicio).ToList();

                    if (itemsDetalle.Count > 0)
                    {
                        foreach (var d in itemsDetalle)
                        {
                            _ofertasCompletas.Add(new ItemOferta
                            {
                                Clave = $"det_{d.Id}",
                                NitProveedor = d.IdProveedor,
                                IdServicio = s.IdServicio,
                                NombreCategoria = nombreCat,
                                NombreServicio = s.NombreServicio,
                                NombreItem = d.NombreItem,
                                Precio = (decimal)d.PrecioItem,
                            });
                        }
                    }
                    else
                    {
                        foreach (var ps in proveedorServicios.Where(ps => ps.IdServicio == s.IdServicio))
                        {
                            _ofertasCompletas.Add(new ItemOferta
                            {
                                Clave = $"ps_{ps.IdProveedorServicio}",
                                NitProveedor = ps.IdProveedor,
                                IdServicio = s.IdServicio,
                                NombreCategoria = nombreCat,
                                NombreServicio = s.NombreServicio,
                                NombreItem = s.NombreServicio,
                                Precio = 0m,
                            });
                        }
                    }
                }

                var nitsConOfertas = _ofertasCompletas.Select(o => o.NitProveedor).Distinct().ToHashSet();
                var provItems = new List<ProveedorItem>
                {
                    new ProveedorItem { NitProveedor = 0, Nombre = "Todos los proveedores" }
                };
                provItems.AddRange(
                    proveedores
                        .Where(p => nitsConOfertas.Contains(p.NitProveedor))
                        .Select(p => new ProveedorItem { NitProveedor = p.NitProveedor, Nombre = p.NombreComercial })
                );

                lbProveedores.ItemsSource = provItems;
                lbProveedores.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // RECINTO

        private void btnSeleccionarRecinto_Click(object sender, RoutedEventArgs e)
        {
            var recinto = (sender as Button)?.Tag as ItemRecinto;
            if (recinto == null) return;

            if (!int.TryParse(txtCantidad.Text, out int horas) || horas <= 0)
            {
                MessageBox.Show("Ingresa un numero de horas valido en el campo inferior.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _recintoSeleccionadoId = recinto.IdRecinto;

            _carrito.RemoveAll(c => c.EsRecinto);
            _carrito.Insert(0, new ItemCarrito
            {
                Clave = $"recinto_{recinto.IdRecinto}",
                EsRecinto = true,
                NombreServicio = $"Recinto: {recinto.NombreRecinto}",
                Cantidad = horas,
                PrecioUnitario = (decimal)recinto.PrecioHoraRaw,
            });

            ActualizarCarrito();
            txtCantidad.Text = "1";
        }

        // SERVICIOS

        private void lbProveedores_SelectionChanged(object sender, SelectionChangedEventArgs e) => FiltrarCatalogo();

        private void txtBuscarServicio_TextChanged(object sender, TextChangedEventArgs e) => FiltrarCatalogo();

        private void FiltrarCatalogo()
        {
            var prov = lbProveedores.SelectedItem as ProveedorItem;
            var texto = txtBuscarServicio?.Text.Trim().ToLower() ?? "";

            var filtrado = _ofertasCompletas.AsEnumerable();

            if (prov != null && prov.NitProveedor != 0)
                filtrado = filtrado.Where(o => o.NitProveedor == prov.NitProveedor);

            if (!string.IsNullOrEmpty(texto))
                filtrado = filtrado.Where(o =>
                    o.NombreCategoria.ToLower().Contains(texto) ||
                    o.NombreServicio.ToLower().Contains(texto) ||
                    o.NombreItem.ToLower().Contains(texto));

            lvCatalogo.ItemsSource = filtrado.ToList();
        }

        private void btnAgregarItem_Click(object sender, RoutedEventArgs e)
        {
            var oferta = (sender as Button)?.Tag as ItemOferta;
            if (oferta == null) return;

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                cantidad = 1;

            var existente = _carrito.Find(c => c.Clave == oferta.Clave);
            if (existente != null)
            {
                existente.Cantidad += cantidad;
            }
            else
            {
                _carrito.Add(new ItemCarrito
                {
                    Clave = oferta.Clave,
                    EsRecinto = false,
                    IdServicio = oferta.IdServicio,
                    NombreServicio = $"{oferta.NombreItem} ({oferta.NombreServicio})",
                    Cantidad = cantidad,
                    PrecioUnitario = oferta.Precio,
                });
            }

            ActualizarCarrito();
            txtCantidad.Text = "1";
        }

        // CARRITO

        private void btnQuitarItem_Click(object sender, RoutedEventArgs e)
        {
            var sel = lvCarrito.SelectedItem as ItemCarrito;
            if (sel == null) return;
            if (sel.EsRecinto) _recintoSeleccionadoId = 0;
            _carrito.Remove(sel);
            ActualizarCarrito();
        }

        private void ActualizarCarrito()
        {
            lvCarrito.ItemsSource = null;
            lvCarrito.ItemsSource = _carrito;
            txtSubtotalCarrito.Text = $"Bs {_carrito.Sum(c => c.Subtotal):F2}";
            txtCantItems.Text = $"{_carrito.Count} items";
        }

        // INVITADOS

        private void btnAgregarInvitado_Click(object sender, RoutedEventArgs e)
        {
            var nombre = txtNombreInvitado.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Escribe el nombre del invitado.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int maxInv = 0;
            int.TryParse(txtMaxInvitados.Text, out maxInv);
            if (maxInv > 0 && _invitados.Count >= maxInv)
            {
                MessageBox.Show($"Ya alcanzaste el maximo de {maxInv} invitados.", "Limite alcanzado",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tipo = (cmbTipoInvitado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "General";
            _invitados.Add(new InvitadoTemporal { Nombre = nombre, Tipo = tipo });
            txtNombreInvitado.Text = string.Empty;
            ActualizarListaInvitados();
        }

        private void btnQuitarInvitado_Click(object sender, RoutedEventArgs e)
        {
            var inv = (sender as Button)?.Tag as InvitadoTemporal;
            if (inv == null) return;
            _invitados.Remove(inv);
            ActualizarListaInvitados();
        }

        private void ActualizarListaInvitados()
        {
            lvInvitados.ItemsSource = null;
            lvInvitados.ItemsSource = _invitados;

            int max = 0;
            int.TryParse(txtMaxInvitados.Text, out max);
            var maxTexto = max > 0 ? $" / {max} maximo" : "";
            txtContadorInvitados.Text = $"{_invitados.Count} invitados agregados{maxTexto}.";
        }

        // PAGO Y CREACION DE EVENTO

        private async void btnProcederPago_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreEvento.Text))
            {
                MessageBox.Show("Ingresa el nombre del evento.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Selecciona la fecha de inicio y fin del evento.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(txtHoraInicio.Text.Trim(), out TimeSpan horaInicio))
                horaInicio = new TimeSpan(8, 0, 0);

            if (!TimeSpan.TryParse(txtHoraFin.Text.Trim(), out TimeSpan horaFin))
                horaFin = new TimeSpan(18, 0, 0);

            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito esta vacio. Agrega al menos un recinto o servicio.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que la fecha de inicio no sea en el pasado
            var inicioEvento = dpFechaInicio.SelectedDate.Value.Date + horaInicio;
            if (inicioEvento <= DateTime.Now)
            {
                MessageBox.Show(
                    "La fecha y hora de inicio no puede ser en el pasado.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Validar que el recinto no esté ocupado en ese horario
            if (_recintoSeleccionadoId > 0)
            {
                try
                {
                    var fechaInicioBuscar = dpFechaInicio.SelectedDate.Value;
                    var fechaFinBuscar = dpFechaFin.SelectedDate.Value;
                    var inicioSolicitado = fechaInicioBuscar.Date + horaInicio;
                    var finSolicitado = fechaFinBuscar.Date + horaFin;

                    var todasFechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;

                    var eventosRecinto = (await ConexionDB.Client.From<Evento>()
                        .Filter("id_recinto", Supabase.Postgrest.Constants.Operator.Equals, _recintoSeleccionadoId.ToString())
                        .Get()).Models;

                    var reservasRecinto = (await ConexionDB.Client.From<Reserva>()
                        .Filter("id_recinto", Supabase.Postgrest.Constants.Operator.Equals, _recintoSeleccionadoId.ToString())
                        .Get()).Models;

                    var idsFechasOcupadas = new HashSet<int>(
                        eventosRecinto.Select(ev => ev.IdFechaEvento)
                        .Concat(reservasRecinto.Select(r => r.FechaReserva))
                    );

                    bool ocupado = todasFechas
                        .Where(f => idsFechasOcupadas.Contains(f.Id))
                        .Any(f =>
                        {
                            var ini = f.FechaInicio.Date + f.HoraInicio;
                            var fin = f.FechaFin.Date + f.HoraFin;
                            return ini < finSolicitado && fin > inicioSolicitado;
                        });

                    if (ocupado)
                    {
                        MessageBox.Show(
                            "El recinto ya está ocupado en ese horario. Elige otro horario o recinto.",
                            "Recinto Ocupado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }
                }
                catch { }
            }

            var desglose = _carrito.Select(c => new LineaDesglose
            {
                Concepto = $"{c.NombreServicio} x{c.Cantidad}",
                Monto = c.Subtotal,
            }).ToList();

            var ventanaPago = new PagoEventoDialog(_metodosPago, desglose, _clientePreseleccionado);
            ventanaPago.Owner = this;
            ventanaPago.ShowDialog();

            if (ventanaPago.Resultado != ResultadoPago.Pagar)
                return;

            try
            {
                var fechaInicio = dpFechaInicio.SelectedDate.Value;
                var fechaFin = dpFechaFin.SelectedDate.Value;
                var nombreEvento = txtNombreEvento.Text.Trim();
                var esPublico = chkEsPublico.IsChecked == true;
                var nombreCliente = $"{ventanaPago.ClienteNombre} {ventanaPago.ClienteApellido}".Trim();

                int topeReserva = 0;
                int.TryParse(txtMaxInvitados.Text, out topeReserva);

                // 1. Crear FechaEvento
                var fechaResp = await ConexionDB.Client.From<FechaEvento>().Insert(new FechaEvento
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                });
                var fechaEvento = fechaResp.Models.First();

                // 2. Crear Evento
                var eventoResp = await ConexionDB.Client.From<Evento>().Insert(new Evento
                {
                    IdOrganizador = _idUsuario,
                    IdRecinto = (int)_recintoSeleccionadoId,
                    IdFechaEvento = fechaEvento.Id,
                    NombreEvento = nombreEvento,
                    Categoria = "Personal",
                    EstadoEvento = "Confirmado",
                    EsPublico = esPublico,
                    NombreReservante = nombreCliente,
                    Descuento = 0,
                    TopeReserva = topeReserva,
                });
                var evento = eventoResp.Models.First();

                // 3. Crear EventoServicio por cada servicio en el carrito
                foreach (var item in _carrito.Where(c => !c.EsRecinto && c.IdServicio > 0))
                {
                    await ConexionDB.Client.From<EventoServicio>().Insert(new EventoServicio
                    {
                        IdEvento = evento.IdEvento,
                        IdServicio = item.IdServicio,
                        Cantidad = (long)item.Cantidad,
                        EstadoEventoServicio = "Activo",
                    });
                }

                // 4. Crear Orden
                var ordenResp = await ConexionDB.Client.From<Orden>().Insert(new Orden
                {
                    IdUsuario = _idUsuario,
                    FechaOrden = DateTime.Now,
                    EstadoOrden = "pagado",
                    DescuentoOrden = ventanaPago.Descuento,
                    CompradorNombre = nombreCliente,
                    CompradorNit = ventanaPago.ClienteNit,
                    CompradorCorreo = ventanaPago.ClienteCorreo,
                });
                var orden = ordenResp.Models.First();

                // 5. Crear Pago
                await ConexionDB.Client.From<Pago>().Insert(new Pago
                {
                    IdOrden = orden.IdOrden,
                    IdMetodoPago = ventanaPago.IdMetodoPagoSeleccionado,
                    MontoPago = ventanaPago.TotalFinal,
                    Moneda = "BOB",
                    FechaPago = DateTime.Now,
                    EstadoPago = "pagado",
                    ReferenciaPago = ventanaPago.Nota,
                });

                // 6. Crear Invitados
                foreach (var inv in _invitados)
                {
                    await ConexionDB.Client.From<Invitado>().Insert(new Invitado
                    {
                        IdEvento = evento.IdEvento,
                        NombreInvitado = inv.Nombre,
                        TipoInvitado = inv.Tipo,
                        EstadoInvitado = "Pendiente",
                    });
                }

                var invMsg = _invitados.Count > 0
                    ? $"\n{_invitados.Count} invitados registrados."
                    : "";

                MessageBox.Show(
                    $"Evento '{nombreEvento}' creado exitosamente.\n" +
                    $"Factura emitida a: {nombreCliente}\n" +
                    $"Fecha: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}" +
                    invMsg,
                    "Evento Registrado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar el evento: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void chkEsPublico_Checked(object sender, RoutedEventArgs e)
        {
            tabBoletos.Visibility = Visibility.Visible;
        }
    }
}
