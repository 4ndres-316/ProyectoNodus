using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public class ServicioEventoVista
    {
        public int IdEventoServicio { get; set; }
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public long Cantidad { get; set; }
    }

    public partial class EditarEventoDialog : Window
    {
        private readonly Evento _evento;
        private FechaEvento _fechaEvento;
        private List<ItemOferta> _ofertasCompletas = new List<ItemOferta>();
        private List<ServicioEventoVista> _serviciosEvento = new List<ServicioEventoVista>();
        private int _nuevoIdRecinto = 0;

        public EditarEventoDialog(Evento evento)
        {
            InitializeComponent();
            _evento = evento;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtTituloEvento.Text = _evento.NombreEvento;
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                // Fecha del evento
                _fechaEvento = (await ConexionDB.Client.From<FechaEvento>()
                    .Where(f => f.Id == _evento.IdFechaEvento).Get()).Models.FirstOrDefault();

                // Pre-cargar campos del evento
                txtNombre.Text = _evento.NombreEvento;
                txtReservante.Text = _evento.NombreReservante;
                chkEsPublico.IsChecked = _evento.EsPublico;
                txtMaxInvitados.Text = _evento.TopeReserva.ToString();

                foreach (ComboBoxItem item in cmbEstado.Items)
                {
                    if (string.Equals(item.Content?.ToString(), _evento.EstadoEvento, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbEstado.SelectedItem = item;
                        break;
                    }
                }
                if (cmbEstado.SelectedItem == null) cmbEstado.SelectedIndex = 0;

                if (_fechaEvento != null)
                {
                    dpFechaInicio.SelectedDate = _fechaEvento.FechaInicio;
                    dpFechaFin.SelectedDate = _fechaEvento.FechaFin;
                    txtHoraInicio.Text = _fechaEvento.HoraInicio.ToString(@"hh\:mm");
                    txtHoraFin.Text = _fechaEvento.HoraFin.ToString(@"hh\:mm");
                }

                // Recinto actual
                var recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
                var recintoActual = recintos.Find(r => r.IdRecinto == _evento.IdRecinto);
                txtRecintoActual.Text = recintoActual?.NombreRecinto ?? "Sin recinto";
                _nuevoIdRecinto = _evento.IdRecinto ?? 0;

                lvRecintos.ItemsSource = recintos
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

                // Servicios actuales del evento
                var eventoServicios = (await ConexionDB.Client.From<EventoServicio>()
                    .Where(es => es.IdEvento == _evento.IdEvento).Get()).Models;
                var servicios = (await ConexionDB.Client.From<Servicio>().Get()).Models;

                _serviciosEvento = eventoServicios.Select(es => new ServicioEventoVista
                {
                    IdEventoServicio = es.IdEventoServicio,
                    IdServicio = es.IdServicio,
                    NombreServicio = servicios.Find(s => s.IdServicio == es.IdServicio)?.NombreServicio ?? $"Servicio {es.IdServicio}",
                    Cantidad = es.Cantidad,
                }).ToList();

                ActualizarListaServicios();

                // Catalogo completo para agregar
                var categorias = (await ConexionDB.Client.From<Categoria>().Get()).Models;
                var proveedores = (await ConexionDB.Client.From<Proveedor>().Get()).Models;
                var detalles = (await ConexionDB.Client.From<DetalleProveedorServicio>().Get()).Models;
                var proveedorServicios = (await ConexionDB.Client.From<ProveedorServicio>().Get()).Models;

                _ofertasCompletas = new List<ItemOferta>();
                foreach (var s in servicios.Where(s => !string.Equals(s.Estado, "Inactivo", StringComparison.OrdinalIgnoreCase)))
                {
                    var cat = categorias.Find(c => c.IdCategoria == s.IdCategoria);
                    var nombreCat = cat?.NombreCategoria ?? "";
                    var itemsDet = detalles.Where(d => d.IdServicio == s.IdServicio).ToList();

                    if (itemsDet.Count > 0)
                    {
                        foreach (var d in itemsDet)
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
                    else
                    {
                        foreach (var ps in proveedorServicios.Where(ps => ps.IdServicio == s.IdServicio))
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

                lvCatalogo.ItemsSource = _ofertasCompletas;
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
            _nuevoIdRecinto = (int)recinto.IdRecinto;
            txtRecintoActual.Text = recinto.NombreRecinto;
        }

        // SERVICIOS
        private void txtBuscarServicio_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtBuscarServicio.Text.Trim().ToLower();
            lvCatalogo.ItemsSource = string.IsNullOrEmpty(texto)
                ? _ofertasCompletas
                : _ofertasCompletas.Where(o =>
                    o.NombreCategoria.ToLower().Contains(texto) ||
                    o.NombreServicio.ToLower().Contains(texto) ||
                    o.NombreItem.ToLower().Contains(texto)).ToList();
        }

        private void btnAgregarServicio_Click(object sender, RoutedEventArgs e)
        {
            var oferta = (sender as Button)?.Tag as ItemOferta;
            if (oferta == null) return;

            if (_serviciosEvento.Any(s => s.IdServicio == oferta.IdServicio))
            {
                var existente = _serviciosEvento.First(s => s.IdServicio == oferta.IdServicio);
                existente.Cantidad++;
            }
            else
            {
                _serviciosEvento.Add(new ServicioEventoVista
                {
                    IdEventoServicio = 0,
                    IdServicio = oferta.IdServicio,
                    NombreServicio = $"{oferta.NombreItem} ({oferta.NombreServicio})",
                    Cantidad = 1,
                });
            }

            ActualizarListaServicios();
        }

        private void btnQuitarServicio_Click(object sender, RoutedEventArgs e)
        {
            var srv = (sender as Button)?.Tag as ServicioEventoVista;
            if (srv == null) return;
            _serviciosEvento.Remove(srv);
            ActualizarListaServicios();
        }

        private void ActualizarListaServicios()
        {
            lvServiciosEvento.ItemsSource = null;
            lvServiciosEvento.ItemsSource = _serviciosEvento;
        }

        // GUARDAR
        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del evento no puede estar vacio.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Actualizar FechaEvento
                if (_fechaEvento != null)
                {
                    if (dpFechaInicio.SelectedDate.HasValue)
                        _fechaEvento.FechaInicio = dpFechaInicio.SelectedDate.Value;
                    if (dpFechaFin.SelectedDate.HasValue)
                        _fechaEvento.FechaFin = dpFechaFin.SelectedDate.Value;
                    if (TimeSpan.TryParse(txtHoraInicio.Text, out var hi))
                        _fechaEvento.HoraInicio = hi;
                    if (TimeSpan.TryParse(txtHoraFin.Text, out var hf))
                        _fechaEvento.HoraFin = hf;

                    await ConexionDB.Client.From<FechaEvento>().Update(_fechaEvento);
                }

                // 2. Actualizar Evento
                _evento.NombreEvento = txtNombre.Text.Trim();
                _evento.NombreReservante = txtReservante.Text.Trim();
                _evento.EsPublico = chkEsPublico.IsChecked == true;
                _evento.IdRecinto = _nuevoIdRecinto;
                _evento.EstadoEvento = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? _evento.EstadoEvento;
                int.TryParse(txtMaxInvitados.Text, out int tope);
                _evento.TopeReserva = tope;

                await ConexionDB.Client.From<Evento>().Update(_evento);

                // 3. Sincronizar EventoServicio: eliminar todos y recrear
                await ConexionDB.Client.From<EventoServicio>()
                    .Where(es => es.IdEvento == _evento.IdEvento)
                    .Delete();

                foreach (var srv in _serviciosEvento)
                {
                    await ConexionDB.Client.From<EventoServicio>().Insert(new EventoServicio
                    {
                        IdEvento = _evento.IdEvento,
                        IdServicio = srv.IdServicio,
                        Cantidad = srv.Cantidad,
                        EstadoEventoServicio = "Activo",
                    });
                }

                MessageBox.Show("Evento actualizado correctamente.", "Guardado",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cambios: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
