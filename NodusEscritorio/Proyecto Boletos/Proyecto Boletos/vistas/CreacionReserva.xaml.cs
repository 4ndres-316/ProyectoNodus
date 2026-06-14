using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class CreacionReserva : Window
    {
        private int _idUsuario;
        private List<Departamento> _departamentos;
        private List<Ciudad> _ciudades;
        private List<Recinto> _recintos;
        private List<MetodoPago> _metodosPago;

        public CreacionReserva(int idUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                _departamentos = (await ConexionDB.Client.From<Departamento>().Get()).Models;
                cmbDepartamento.ItemsSource = _departamentos;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar departamentos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            try
            {
                _ciudades = (await ConexionDB.Client.From<Ciudad>().Get()).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar ciudades: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

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

        private void cmbDepartamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDepartamento.SelectedValue == null || _ciudades == null)
                return;

            int idDepartamento = (int)cmbDepartamento.SelectedValue;
            cmbCiudad.ItemsSource = _ciudades
                .Where(c => c.IdDepartamento == idDepartamento)
                .ToList();
            cmbCiudad.SelectedIndex = -1;
            cmbRecinto.ItemsSource = null;
            cmbRecinto.SelectedIndex = -1;
            ResetDisponibilidad();
        }

        private void cmbCiudad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCiudad.SelectedValue == null || _recintos == null)
                return;

            int idCiudad = (int)cmbCiudad.SelectedValue;
            cmbRecinto.ItemsSource = _recintos.Where(r => r.IdCiudad == idCiudad).ToList();
            cmbRecinto.SelectedIndex = -1;
            ResetDisponibilidad();
        }

        private async void cmbRecinto_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e
        ) => await VerificarDisponibilidad();

        private async void Fecha_Changed(object sender, SelectionChangedEventArgs e) =>
            await VerificarDisponibilidad();

        private async void Fecha_TextChanged(object sender, TextChangedEventArgs e) =>
            await VerificarDisponibilidad();

        private void ResetDisponibilidad()
        {
            txtDisponibilidad.Text = "Selecciona recinto y fechas";
            bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(236, 236, 236));
            txtDisponibilidad.Foreground = Brushes.Gray;
        }

        private async Task VerificarDisponibilidad()
        {
            if (
                cmbRecinto.SelectedValue == null
                || dpFechaInicio.SelectedDate == null
                || dpFechaFin.SelectedDate == null
                || !TimeSpan.TryParse(txtHoraInicio.Text, out TimeSpan horaInicio)
                || !TimeSpan.TryParse(txtHoraFin.Text, out TimeSpan horaFin)
            )
            {
                ResetDisponibilidad();
                return;
            }

            long idRecinto = (long)cmbRecinto.SelectedValue;
            var inicioSolicitado = dpFechaInicio.SelectedDate.Value.Date + horaInicio;
            var finSolicitado = dpFechaFin.SelectedDate.Value.Date + horaFin;

            if (finSolicitado <= inicioSolicitado)
            {
                txtDisponibilidad.Text = "La fecha fin debe ser posterior al inicio";
                bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));
                txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                return;
            }

            try
            {
                var todasLasFechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;

                // Fechas ocupadas por eventos
                var eventosRecinto = (
                    await ConexionDB
                        .Client.From<Evento>()
                        .Filter(
                            "id_recinto",
                            Supabase.Postgrest.Constants.Operator.Equals,
                            idRecinto.ToString()
                        )
                        .Get()
                ).Models;

                var fechasEventos = todasLasFechas
                    .Where(f => eventosRecinto.Select(ev => ev.IdFechaEvento).Contains(f.Id))
                    .ToList();

                // Fechas ocupadas por reservas
                var reservasRecinto = (
                    await ConexionDB
                        .Client.From<Reserva>()
                        .Filter(
                            "id_recinto",
                            Supabase.Postgrest.Constants.Operator.Equals,
                            idRecinto.ToString()
                        )
                        .Get()
                ).Models;

                var fechasReservas = todasLasFechas
                    .Where(f => reservasRecinto.Select(r => r.FechaReserva).Contains(f.Id))
                    .ToList();

                var todasFechasOcupadas = fechasEventos.Concat(fechasReservas).ToList();

                bool ocupado = todasFechasOcupadas.Any(f =>
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

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRecinto.SelectedValue == null)
            {
                MessageBox.Show(
                    "Selecciona un recinto.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show(
                    "Las fechas son obligatorias.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (
                !TimeSpan.TryParse(txtHoraInicio.Text, out TimeSpan horaInicio)
                || !TimeSpan.TryParse(txtHoraFin.Text, out TimeSpan horaFin)
            )
            {
                MessageBox.Show(
                    "Escribe horas válidas (Ej: 18:30).",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombreReservante.Text))
            {
                MessageBox.Show(
                    "El nombre reservante es obligatorio.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (txtDisponibilidad.Text != "✔ Recinto disponible")
            {
                MessageBox.Show(
                    "El recinto no está disponible en ese horario.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Abrir ventana de pago
            var ventanaPago = new PagoEventoDialog(_metodosPago);
            ventanaPago.Owner = this;
            ventanaPago.ShowDialog();

            if (ventanaPago.Resultado == ResultadoPago.Cancelado)
                return;

            btnGuardar.IsEnabled = false;

            // Determinar estado según resultado de pago
            string estadoReserva = ventanaPago.Resultado == ResultadoPago.Pagar
                ? "Pagado"
                : "En espera";

            FechaEvento fechaInserta = null;

            try
            {
                // 1. FechaEvento
                var fechaEvento = new FechaEvento
                {
                    FechaInicio = dpFechaInicio.SelectedDate.Value,
                    FechaFin = dpFechaFin.SelectedDate.Value,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                };
                var fechaResp = await ConexionDB.Client.From<FechaEvento>().Insert(fechaEvento);
                fechaInserta = fechaResp.Models.First();

                // 2. Reserva
                var reserva = new Reserva
                {
                    IdRecinto = (int)(long)cmbRecinto.SelectedValue,
                    FechaReserva = fechaInserta.Id,
                    NombreReservante = txtNombreReservante.Text.Trim(),
                    EstadoReserva = estadoReserva,
                };
                await ConexionDB.Client.From<Reserva>().Insert(reserva);

                // 3. Orden y Pago si eligió pagar
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

                string mensaje = ventanaPago.Resultado == ResultadoPago.Pagar
                    ? "Reserva creada exitosamente con estado: Pagado."
                    : "Reserva creada exitosamente con estado: En espera.";

                MessageBox.Show(
                    mensaje,
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                if (fechaInserta != null)
                {
                    try
                    {
                        await ConexionDB
                            .Client.From<FechaEvento>()
                            .Where(f => f.Id == fechaInserta.Id)
                            .Delete();
                    }
                    catch { }
                }

                MessageBox.Show(
                    $"Error al guardar: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                btnGuardar.IsEnabled = true;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show(
                    "¿Seguro que deseas cancelar?",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes
            )
                this.Close();
        }
    }
}
