using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class CreacionEvento : Window
    {
        private int _idUsuario;
        private string _nombreUsuario;
        private List<Recinto> _recintos;
        private List<Servicio> _servicios;
        private List<Categoria> _categorias;
        private List<Proveedor> _proveedores;
        private List<ProveedorServicio> _proveedorServicios;
        private List<MetodoPago> _metodosPago;
        private List<Reserva> _reservas;

        private List<FilaServicio> _filasServicio = new List<FilaServicio>();
        private List<FilaBoleto> _filasBoleto = new List<FilaBoleto>();

        public CreacionEvento(int idUsuario, string nombreUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
            _nombreUsuario = nombreUsuario;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                _recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar recintos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                _reservas = (await ConexionDB.Client.From<Reserva>().Get()).Models;

                var reservasVista = _reservas
                    .Select(r => new ReservaVista
                    {
                        IdReserva = r.IdReserva,
                        IdRecinto = r.IdRecinto,
                        FechaReserva = r.FechaReserva,
                        NombreReservante = r.NombreReservante,
                        Display =
                            $"{r.NombreReservante} — {_recintos?.Find(rc => rc.IdRecinto == r.IdRecinto)?.NombreRecinto ?? r.IdRecinto.ToString()}",
                    })
                    .ToList();

                cmbReservas.ItemsSource = reservasVista;
                cmbReservas.DisplayMemberPath = "Display";
                cmbReservas.SelectedValuePath = "IdReserva";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar reservas: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                _categorias = (await ConexionDB.Client.From<Categoria>().Get()).Models;
                _servicios = (await ConexionDB.Client.From<Servicio>().Get()).Models;

                foreach (var s in _servicios)
                {
                    var cat = _categorias?.Find(c => c.IdCategoria == s.IdCategoria);
                    s.NombreCategoria = cat?.NombreCategoria ?? $"Categoría {s.IdCategoria}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar servicios: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                _proveedores = (await ConexionDB.Client.From<Proveedor>().Get()).Models;
                _proveedorServicios = (
                    await ConexionDB.Client.From<ProveedorServicio>().Get()
                ).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar proveedores: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                _metodosPago = (
                    await ConexionDB
                        .Client.From<MetodoPago>()
                        .Filter("estado", Supabase.Postgrest.Constants.Operator.Equals, "activo")
                        .Get()
                ).Models;

                if (_metodosPago == null || _metodosPago.Count == 0)
                    _metodosPago = (await ConexionDB.Client.From<MetodoPago>().Get()).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar métodos de pago: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ─── ES PÚBLICO ──────────────────────────────────────────────────────

        private void chkEsPublico_Changed(object sender, RoutedEventArgs e)
        {
            if (seccionBoletos == null)
                return;

            bool esPublico = chkEsPublico.IsChecked == true;
            seccionBoletos.Visibility = esPublico ? Visibility.Visible : Visibility.Collapsed;

            // Si se desmarca, limpiar boletos ya agregados
            if (!esPublico && _filasBoleto.Count > 0)
            {
                _filasBoleto.Clear();
                panelBoletos.Children.Clear();
            }
        }

        // ─── SERVICIOS DINÁMICOS ─────────────────────────────────────────────

        private void btnAgregarServicio_Click(object sender, RoutedEventArgs e)
        {
            if (_servicios == null || _proveedores == null || _proveedorServicios == null)
            {
                MessageBox.Show(
                    "Los datos aún están cargando, espera un momento.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var fila = new FilaServicio(_servicios, _proveedores, _proveedorServicios);
            fila.OnEliminar = () =>
            {
                _filasServicio.Remove(fila);
                panelServicios.Children.Remove(fila.Panel);
            };
            _filasServicio.Add(fila);
            panelServicios.Children.Add(fila.Panel);
        }

        // ─── BOLETOS DINÁMICOS ───────────────────────────────────────────────

        private void btnAgregarBoleto_Click(object sender, RoutedEventArgs e)
        {
            var fila = new FilaBoleto();
            fila.OnEliminar = () =>
            {
                _filasBoleto.Remove(fila);
                panelBoletos.Children.Remove(fila.Panel);
            };
            _filasBoleto.Add(fila);
            panelBoletos.Children.Add(fila.Panel);
        }

        // ─── VALIDACIONES ────────────────────────────────────────────────────

        private bool ValidarFilasServicio()
        {
            for (int i = 0; i < _filasServicio.Count; i++)
            {
                var fila = _filasServicio[i];
                if (fila.IdServicioSeleccionado == null)
                {
                    MessageBox.Show(
                        $"El servicio #{i + 1} no tiene servicio seleccionado. Complétalo o elimínalo.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return false;
                }
                if (fila.Cantidad <= 0)
                {
                    MessageBox.Show(
                        $"El servicio #{i + 1} debe tener una cantidad mayor a 0.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return false;
                }
            }
            return true;
        }

        private bool ValidarFilasBoleto()
        {
            if (chkEsPublico.IsChecked != true)
                return true;

            for (int i = 0; i < _filasBoleto.Count; i++)
            {
                var fila = _filasBoleto[i];
                if (string.IsNullOrWhiteSpace(fila.NombreTipo))
                {
                    MessageBox.Show(
                        $"El tipo de boleto #{i + 1} necesita un nombre. Complétalo o elimínalo.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return false;
                }
                if (fila.Precio <= 0)
                {
                    MessageBox.Show(
                        $"El tipo de boleto #{i + 1} debe tener un precio mayor a 0.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return false;
                }
                if (fila.CantidadTotal <= 0)
                {
                    MessageBox.Show(
                        $"El tipo de boleto #{i + 1} debe tener una cantidad mayor a 0.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return false;
                }
            }
            return true;
        }

        // ─── GUARDAR ─────────────────────────────────────────────────────────

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreEvento.Text))
            {
                MessageBox.Show(
                    "El nombre del evento es obligatorio.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (cmbReservas.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona una reserva.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!ValidarFilasServicio())
                return;
            if (!ValidarFilasBoleto())
                return;

            var ventanaPago = new PagoEventoDialog(_metodosPago);
            ventanaPago.Owner = this;
            ventanaPago.ShowDialog();

            if (ventanaPago.Resultado == ResultadoPago.Cancelado)
                return;

            btnGuardar.IsEnabled = false;
            txtEstadoGuardado.Text = "Guardando...";

            string estadoEvento =
                ventanaPago.Resultado == ResultadoPago.Pagar ? "Pagado" : "En espera";

            try
            {
                var itemReserva = cmbReservas.SelectedItem as ReservaVista;

                var evento = new Evento
                {
                    IdOrganizador = _idUsuario,
                    IdRecinto = itemReserva.IdRecinto,
                    IdFechaEvento = itemReserva.FechaReserva,
                    NombreEvento = txtNombreEvento.Text.Trim(),
                    Categoria = ((ComboBoxItem)cmbCategoria.SelectedItem).Content.ToString(),
                    EstadoEvento = estadoEvento,
                    NombreReservante = itemReserva.NombreReservante,
                    ImagenUrl = txtImagenUrl.Text.Trim(),
                    EsPublico = chkEsPublico.IsChecked == true,
                    IdReserva = itemReserva.IdReserva,
                };
                var eventoResp = await ConexionDB.Client.From<Evento>().Insert(evento);
                var eventoInserto = eventoResp.Models.First();
                int idEvento = eventoInserto.IdEvento;

                foreach (var fila in _filasServicio)
                {
                    if (fila.IdServicioSeleccionado == null)
                        continue;
                    await ConexionDB
                        .Client.From<EventoServicio>()
                        .Insert(
                            new EventoServicio
                            {
                                IdEvento = idEvento,
                                IdServicio = fila.IdServicioSeleccionado.Value,
                                Cantidad = fila.Cantidad,
                                PrecioAcordado = fila.PrecioAcordado,
                                EstadoEventoServicio = "pendiente",
                            }
                        );
                }

                if (chkEsPublico.IsChecked == true)
                {
                    foreach (var fila in _filasBoleto)
                    {
                        if (string.IsNullOrWhiteSpace(fila.NombreTipo))
                            continue;
                        await ConexionDB
                            .Client.From<TipoBoleto>()
                            .Insert(
                                new TipoBoleto
                                {
                                    IdEvento = idEvento,
                                    NombreTipoBoleto = fila.NombreTipo,
                                    Precio = fila.Precio,
                                    CantidadTotal = fila.CantidadTotal,
                                    CantidadDisponible = fila.CantidadTotal,
                                    Descripcion = fila.Descripcion,
                                    UrlImagen = fila.ImagenUrl,
                                }
                            );
                    }
                }

                if (ventanaPago.Resultado == ResultadoPago.Pagar)
                {
                    var ordenResp = await ConexionDB
                        .Client.From<Orden>()
                        .Insert(
                            new Orden
                            {
                                IdUsuario = _idUsuario,
                                FechaOrden = DateTime.Now,
                                EstadoOrden = "pagado",
                                DescuentoOrden = 0,
                            }
                        );
                    var ordenInserta = ordenResp.Models.First();

                    await ConexionDB
                        .Client.From<Pago>()
                        .Insert(
                            new Pago
                            {
                                IdOrden = ordenInserta.IdOrden,
                                IdMetodoPago = ventanaPago.IdMetodoPagoSeleccionado,
                                MontoPago = ventanaPago.Monto,
                                Moneda = "BOB",
                                FechaPago = DateTime.Now,
                                EstadoPago = "pagado",
                                ReferenciaPago = ventanaPago.Nota,
                            }
                        );
                }

                string msg =
                    ventanaPago.Resultado == ResultadoPago.Pagar
                        ? "Evento creado con estado: Pagado."
                        : "Evento creado con estado: En espera.";

                MessageBox.Show(msg, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                btnGuardar.IsEnabled = true;
                txtEstadoGuardado.Text = "";
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show(
                    "¿Seguro que deseas cancelar? Se perderán los datos.",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes
            )
                this.Close();
        }

        private void cmbReservas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cmbReservas.SelectedItem as ReservaVista;
            if (item == null)
                return;

            txtNombreReservante.Text = item.NombreReservante;

            var recinto = _recintos?.Find(r => r.IdRecinto == item.IdRecinto);
            txtRecintoInfo.Text = recinto?.NombreRecinto ?? item.IdRecinto.ToString();
        }
    }

    public class ReservaVista
    {
        public int IdReserva { get; set; }
        public int IdRecinto { get; set; }
        public int FechaReserva { get; set; }
        public string NombreReservante { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }
}
