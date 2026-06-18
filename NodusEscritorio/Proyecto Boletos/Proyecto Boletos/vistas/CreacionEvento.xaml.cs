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
        private List<Departamento> _departamentos;
        private List<Ciudad> _ciudades;
        private List<Recinto> _recintos;
        private List<Servicio> _servicios;
        private List<Categoria> _categorias;
        private List<Proveedor> _proveedores;
        private List<ProveedorServicio> _proveedorServicios;
        private List<DetalleProveedorServicio> _detalles;
        private List<MetodoPago> _metodosPago;

        private List<FilaServicio> _filasServicio = new List<FilaServicio>();
        private List<FilaBoleto> _filasBoleto = new List<FilaBoleto>();

        private Evento _eventoAReprogramar;
        private FechaEvento _fechaAReprogramar;
        private bool _modoReprogramar => _eventoAReprogramar != null;

        public CreacionEvento(int idUsuario, string nombreUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
            _nombreUsuario = nombreUsuario;
        }

        public CreacionEvento(Evento evento, FechaEvento fecha)
        {
            InitializeComponent();
            _eventoAReprogramar = evento;
            _fechaAReprogramar = fecha;
            _idUsuario = evento.IdOrganizador;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
            if (_modoReprogramar)
                CargarDatosReprogramar();
        }

        private void CargarDatosReprogramar()
        {
            txtTituloHeader.Text = "Reprogramar Evento";
            Title = "Reprogramar Evento";

            txtNombreEvento.Text = _eventoAReprogramar.NombreEvento;

            foreach (ComboBoxItem item in cmbCategoria.Items)
            {
                if (item.Content?.ToString() == _eventoAReprogramar.Categoria)
                {
                    item.IsSelected = true;
                    break;
                }
            }

            txtImagenUrl.Text = _eventoAReprogramar.ImagenUrl ?? string.Empty;
            chkEsPublico.IsChecked = _eventoAReprogramar.EsPublico;
            txtNombreReservante.Text = _eventoAReprogramar.NombreReservante ?? string.Empty;
            txtNombreReservante.IsEnabled = false;

            // Pre-seleccionar el cascade de ubicación
            var recinto = _recintos?.Find(r => r.IdRecinto == (long)_eventoAReprogramar.IdRecinto);
            if (recinto != null && recinto.IdCiudad.HasValue)
            {
                var ciudad = _ciudades?.Find(c => c.IdCiudad == recinto.IdCiudad.Value);
                if (ciudad != null)
                {
                    cmbDepartamento.SelectedItem = _departamentos?.Find(d => (long)d.IdDepartamento == ciudad.IdDepartamento);
                    cmbCiudad.SelectedItem = ciudad;
                    cmbRecinto.SelectedItem = recinto;
                }
            }

            // No se puede cambiar el recinto al reprogramar
            cmbDepartamento.IsEnabled = false;
            cmbCiudad.IsEnabled = false;
            cmbRecinto.IsEnabled = false;

            dpFechaInicio.SelectedDate = _fechaAReprogramar.FechaInicio;
            dpFechaFin.SelectedDate = _fechaAReprogramar.FechaFin;
            txtHoraInicio.Text = _fechaAReprogramar.HoraInicio.ToString(@"hh\:mm");
            txtHoraFin.Text = _fechaAReprogramar.HoraFin.ToString(@"hh\:mm");
        }

        private async Task CargarDatos()
        {
            try
            {
                _departamentos = (await ConexionDB.Client.From<Departamento>().Get()).Models;
                cmbDepartamento.DisplayMemberPath = "NombreDepartamento";
                cmbDepartamento.ItemsSource = _departamentos;

                _ciudades = (await ConexionDB.Client.From<Ciudad>().Get()).Models;
                cmbCiudad.DisplayMemberPath = "NombreCiudad";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar ubicaciones: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                _recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
                cmbRecinto.DisplayMemberPath = "NombreRecinto";
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
                _proveedorServicios = (await ConexionDB.Client.From<ProveedorServicio>().Get()).Models;
                _detalles = (await ConexionDB.Client.From<DetalleProveedorServicio>().Get()).Models;
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

        // ─── UBICACIÓN CASCADE ───────────────────────────────────────────────

        private void cmbDepartamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ciudades == null) return;
            var dept = cmbDepartamento.SelectedItem as Departamento;
            if (dept == null) return;

            cmbCiudad.ItemsSource = _ciudades.Where(c => c.IdDepartamento == (long)dept.IdDepartamento).ToList();
            cmbCiudad.SelectedIndex = -1;
            cmbRecinto.ItemsSource = null;
            cmbRecinto.SelectedIndex = -1;
            ResetDisponibilidad();
        }

        private void cmbCiudad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_recintos == null) return;
            var ciudad = cmbCiudad.SelectedItem as Ciudad;
            if (ciudad == null) return;

            cmbRecinto.ItemsSource = _recintos.Where(r => r.IdCiudad == ciudad.IdCiudad).ToList();
            cmbRecinto.SelectedIndex = -1;
            ResetDisponibilidad();
        }

        private async void cmbRecinto_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => await VerificarDisponibilidad();

        private async void Fecha_Changed(object sender, SelectionChangedEventArgs e)
            => await VerificarDisponibilidad();

        private async void Fecha_TextChanged(object sender, TextChangedEventArgs e)
            => await VerificarDisponibilidad();

        private void ResetDisponibilidad()
        {
            txtDisponibilidad.Text = "Selecciona recinto y fechas";
            bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(236, 236, 236));
            txtDisponibilidad.Foreground = Brushes.Gray;
        }

        private async Task VerificarDisponibilidad()
        {
            var recintoSel = cmbRecinto.SelectedItem as Recinto;
            if (recintoSel == null
                || dpFechaInicio.SelectedDate == null
                || dpFechaFin.SelectedDate == null
                || !TimeSpan.TryParse(txtHoraInicio.Text, out TimeSpan horaInicio)
                || !TimeSpan.TryParse(txtHoraFin.Text, out TimeSpan horaFin))
            {
                ResetDisponibilidad();
                return;
            }

            long idRecinto = recintoSel.IdRecinto;
            var inicioSolicitado = dpFechaInicio.SelectedDate.Value.Date + horaInicio;
            var finSolicitado = dpFechaFin.SelectedDate.Value.Date + horaFin;

            if (finSolicitado <= inicioSolicitado)
            {
                txtDisponibilidad.Text = "La fecha fin debe ser posterior al inicio";
                bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));
                txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                return;
            }

            if (!_modoReprogramar && inicioSolicitado <= DateTime.Now)
            {
                txtDisponibilidad.Text = "⚠ La fecha y hora de inicio ya pasó";
                bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));
                txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                return;
            }

            try
            {
                var todasLasFechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;

                var eventosRecinto = (
                    await ConexionDB
                        .Client.From<Evento>()
                        .Filter("id_recinto", Supabase.Postgrest.Constants.Operator.Equals, idRecinto.ToString())
                        .Get()
                ).Models;

                // En modo reprogramar excluimos la fecha actual del evento
                var idsExcluir = new HashSet<int>();
                if (_modoReprogramar)
                    idsExcluir.Add(_fechaAReprogramar.Id);

                var fechasEventos = todasLasFechas
                    .Where(f => eventosRecinto.Select(ev => ev.IdFechaEvento).Contains(f.Id)
                                && !idsExcluir.Contains(f.Id))
                    .ToList();

                var reservasRecinto = (
                    await ConexionDB
                        .Client.From<Reserva>()
                        .Filter("id_recinto", Supabase.Postgrest.Constants.Operator.Equals, idRecinto.ToString())
                        .Get()
                ).Models;

                var fechasReservas = todasLasFechas
                    .Where(f => reservasRecinto.Select(r => r.FechaReserva).Contains(f.Id)
                                && !idsExcluir.Contains(f.Id))
                    .ToList();

                bool ocupado = fechasEventos.Concat(fechasReservas).Any(f =>
                {
                    var ini = f.FechaInicio.Date + f.HoraInicio;
                    var fin = f.FechaFin.Date + f.HoraFin;
                    return ini < finSolicitado && fin > inicioSolicitado;
                });

                if (ocupado)
                {
                    txtDisponibilidad.Text = "⚠ Recinto ocupado en ese horario";
                    bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));
                    txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                }
                else
                {
                    txtDisponibilidad.Text = "✔ Recinto disponible";
                    bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(232, 245, 238));
                    txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(30, 130, 80));
                }
            }
            catch { }
        }

        // ─── ES PÚBLICO ──────────────────────────────────────────────────────

        private void chkEsPublico_Changed(object sender, RoutedEventArgs e)
        {
            if (seccionBoletos == null)
                return;

            bool esPublico = chkEsPublico.IsChecked == true;
            seccionBoletos.Visibility = esPublico ? Visibility.Visible : Visibility.Collapsed;

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

            var fila = new FilaServicio(_servicios, _proveedores, _proveedorServicios, _detalles);
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

            if (_modoReprogramar)
            {
                await GuardarReprogramar();
                return;
            }

            var recintoSel = cmbRecinto.SelectedItem as Recinto;
            if (recintoSel == null)
            {
                MessageBox.Show("Selecciona un recinto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Las fechas son obligatorias.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParseExact(txtHoraInicio.Text.Trim(), @"hh\:mm", null, out TimeSpan horaInicio))
            {
                MessageBox.Show("Formato de Hora Inicio inválido. Usa HH:mm (ej. 09:30).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParseExact(txtHoraFin.Text.Trim(), @"hh\:mm", null, out TimeSpan horaFin))
            {
                MessageBox.Show("Formato de Hora Fin inválido. Usa HH:mm (ej. 22:00).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombreReservante.Text))
            {
                MessageBox.Show("El nombre reservante es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var inicioEvento = dpFechaInicio.SelectedDate.Value.Date + horaInicio;
            if (inicioEvento <= DateTime.Now)
            {
                MessageBox.Show(
                    "La fecha y hora de inicio del evento no puede ser en el pasado.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!txtDisponibilidad.Text.Contains("disponible"))
            {
                MessageBox.Show("El recinto no está disponible en ese horario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidarFilasServicio()) return;
            if (!ValidarFilasBoleto()) return;

            var ventanaPago = new PagoEventoDialog(_metodosPago);
            ventanaPago.Owner = this;
            ventanaPago.ShowDialog();

            if (ventanaPago.Resultado == ResultadoPago.Cancelado) return;

            btnGuardar.IsEnabled = false;
            txtEstadoGuardado.Text = "Guardando...";

            string estadoEvento = ventanaPago.Resultado == ResultadoPago.Pagar ? "Programado" : "En espera";
            int idRecinto = (int)recintoSel.IdRecinto;

            FechaEvento fechaInserta = null;
            Reserva reservaInserta = null;

            try
            {
                // 1. FechaEvento
                var fechaResp = await ConexionDB.Client.From<FechaEvento>().Insert(new FechaEvento
                {
                    FechaInicio = dpFechaInicio.SelectedDate.Value,
                    FechaFin = dpFechaFin.SelectedDate.Value,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                });
                fechaInserta = fechaResp.Models.First();

                // 2. Reserva
                var reservaResp = await ConexionDB.Client.From<Reserva>().Insert(new Reserva
                {
                    IdRecinto = idRecinto,
                    FechaReserva = fechaInserta.Id,
                    NombreReservante = txtNombreReservante.Text.Trim(),
                    EstadoReserva = estadoEvento,
                });
                reservaInserta = reservaResp.Models.First();

                // 3. Evento
                var eventoResp = await ConexionDB.Client.From<Evento>().Insert(new Evento
                {
                    IdOrganizador = _idUsuario,
                    IdRecinto = idRecinto,
                    IdFechaEvento = fechaInserta.Id,
                    NombreEvento = txtNombreEvento.Text.Trim(),
                    Categoria = ((ComboBoxItem)cmbCategoria.SelectedItem).Content.ToString(),
                    EstadoEvento = estadoEvento,
                    NombreReservante = txtNombreReservante.Text.Trim(),
                    ImagenUrl = txtImagenUrl.Text.Trim(),
                    EsPublico = chkEsPublico.IsChecked == true,
                    IdReserva = reservaInserta.IdReserva,
                });
                var eventoInserto = eventoResp.Models.First();
                int idEvento = eventoInserto.IdEvento;

                // 4. EventoServicio
                foreach (var fila in _filasServicio)
                {
                    if (fila.IdServicioSeleccionado == null) continue;
                    await ConexionDB.Client.From<EventoServicio>().Insert(new EventoServicio
                    {
                        IdEvento = idEvento,
                        IdServicio = fila.IdServicioSeleccionado.Value,
                        Cantidad = fila.Cantidad,
                        EstadoEventoServicio = "pendiente",
                    });
                }

                // 5. TipoBoleto
                if (chkEsPublico.IsChecked == true)
                {
                    foreach (var fila in _filasBoleto)
                    {
                        if (string.IsNullOrWhiteSpace(fila.NombreTipo)) continue;
                        await ConexionDB.Client.From<TipoBoleto>().Insert(new TipoBoleto
                        {
                            IdEvento = idEvento,
                            NombreTipoBoleto = fila.NombreTipo,
                            Precio = fila.Precio,
                            CantidadTotal = fila.CantidadTotal,
                            CantidadDisponible = fila.CantidadTotal,
                            Descripcion = fila.Descripcion,
                            UrlImagen = fila.ImagenUrl,
                        });
                    }
                }

                // 6. Orden + Pago si eligió pagar ahora
                if (ventanaPago.Resultado == ResultadoPago.Pagar)
                {
                    var ordenResp = await ConexionDB.Client.From<Orden>().Insert(new Orden
                    {
                        IdUsuario = _idUsuario,
                        FechaOrden = DateTime.Now,
                        EstadoOrden = "pagado",
                        DescuentoOrden = 0,
                    });
                    var ordenInserta = ordenResp.Models.First();

                    await ConexionDB.Client.From<Pago>().Insert(new Pago
                    {
                        IdOrden = ordenInserta.IdOrden,
                        IdMetodoPago = ventanaPago.IdMetodoPagoSeleccionado,
                        MontoPago = ventanaPago.Monto,
                        Moneda = "BOB",
                        FechaPago = DateTime.Now,
                        EstadoPago = "pagado",
                        ReferenciaPago = ventanaPago.Nota,
                    });
                }

                string msg = ventanaPago.Resultado == ResultadoPago.Pagar
                    ? "Evento creado con estado: Programado."
                    : "Evento creado con estado: En espera.";

                MessageBox.Show(msg, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                // Rollback de registros parcialmente insertados
                if (reservaInserta != null)
                {
                    try { await ConexionDB.Client.From<Reserva>().Where(r => r.IdReserva == reservaInserta.IdReserva).Delete(); } catch { }
                }
                if (fechaInserta != null)
                {
                    try { await ConexionDB.Client.From<FechaEvento>().Where(f => f.Id == fechaInserta.Id).Delete(); } catch { }
                }

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

        private async Task GuardarReprogramar()
        {
            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show(
                    "Las fechas de inicio y fin son obligatorias.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (dpFechaFin.SelectedDate < dpFechaInicio.SelectedDate)
            {
                MessageBox.Show(
                    "La fecha fin no puede ser anterior a la fecha inicio.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            TimeSpan horaInicio, horaFin;
            if (!TimeSpan.TryParseExact(txtHoraInicio.Text.Trim(), @"hh\:mm", null, out horaInicio))
            {
                MessageBox.Show(
                    "Formato de Hora Inicio inválido. Usa HH:mm (ej. 09:30).",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!TimeSpan.TryParseExact(txtHoraFin.Text.Trim(), @"hh\:mm", null, out horaFin))
            {
                MessageBox.Show(
                    "Formato de Hora Fin inválido. Usa HH:mm (ej. 22:00).",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            btnGuardar.IsEnabled = false;
            txtEstadoGuardado.Text = "Guardando...";

            try
            {
                _fechaAReprogramar.FechaInicio = dpFechaInicio.SelectedDate.Value;
                _fechaAReprogramar.FechaFin = dpFechaFin.SelectedDate.Value;
                _fechaAReprogramar.HoraInicio = horaInicio;
                _fechaAReprogramar.HoraFin = horaFin;
                await ConexionDB.Client.From<FechaEvento>().Update(_fechaAReprogramar);

                _eventoAReprogramar.NombreEvento = txtNombreEvento.Text.Trim();
                _eventoAReprogramar.Categoria = (
                    (ComboBoxItem)cmbCategoria.SelectedItem
                ).Content.ToString();
                _eventoAReprogramar.ImagenUrl = txtImagenUrl.Text.Trim();
                _eventoAReprogramar.EsPublico = chkEsPublico.IsChecked == true;
                _eventoAReprogramar.EstadoEvento = "Reprogramado";
                await ConexionDB.Client.From<Evento>().Update(_eventoAReprogramar);

                MessageBox.Show(
                    "Evento reprogramado correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al reprogramar: {ex.Message}",
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
    }
}
