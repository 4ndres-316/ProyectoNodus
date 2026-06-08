using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class EventosView : UserControl
    {
        private List<Evento> _eventos;
        private List<Recinto> _recintos;
        private List<FechaEvento> _fechasEventos;
        private Evento _eventoSeleccionado;
        private bool _modoEdicion = false;
        private int _idUsuario;
        private DateTime _mesActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        public EventosView(int idUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
        }

        public EventosView()
            : this(0) { }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarRecintos();
            await CargarFechasEventos();
            await CargarEventos();
        }

        private async Task CargarRecintos()
        {
            try
            {
                var response = await ConexionDB.Client.From<Recinto>().Get();
                _recintos = response.Models;
                cmbRecinto.ItemsSource = _recintos;
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
        }

        private async Task CargarFechasEventos()
        {
            try
            {
                var response = await ConexionDB.Client.From<FechaEvento>().Get();
                _fechasEventos = response.Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar fechas: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task CargarEventos()
        {
            try
            {
                var response = await ConexionDB.Client.From<Evento>().Get();
                _eventos = response.Models;
                RenderizarCalendario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar eventos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ─── CALENDARIO ───

        private void RenderizarCalendario()
        {
            ugCalendario.Children.Clear();

            txtMesAnio.Text = _mesActual
                .ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"))
                .ToUpper();

            int diasEnMes = DateTime.DaysInMonth(_mesActual.Year, _mesActual.Month);
            int diaSemanaInicio = (int)new DateTime(_mesActual.Year, _mesActual.Month, 1).DayOfWeek;

            for (int i = 0; i < diaSemanaInicio; i++)
                ugCalendario.Children.Add(CrearCeldaVacia());

            for (int dia = 1; dia <= diasEnMes; dia++)
            {
                var fecha = new DateTime(_mesActual.Year, _mesActual.Month, dia);
                var eventosDelDia = ObtenerEventosDeVista()
                    .Where(ev =>
                        fecha.Date >= ev.FechaEvento.Date && fecha.Date <= ev.FechaFin.Date
                    )
                    .ToList();

                ugCalendario.Children.Add(CrearCelda(fecha, eventosDelDia));
            }
        }

        private UIElement CrearCeldaVacia()
        {
            return new Border
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0.5),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                MinHeight = 70,
            };
        }

        private UIElement CrearCelda(DateTime fecha, List<EventoVista> eventos)
        {
            bool esHoy = fecha.Date == DateTime.Today;

            var panel = new StackPanel { Margin = new Thickness(4) };

            var lblDia = new TextBlock
            {
                Text = fecha.Day.ToString(),
                FontSize = 13,
                FontWeight = esHoy ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
            };

            if (esHoy)
            {
                var circulo = new Border
                {
                    Width = 26,
                    Height = 26,
                    CornerRadius = new CornerRadius(13),
                    HorizontalAlignment = HorizontalAlignment.Right,
                };
                circulo.SetResourceReference(Border.BackgroundProperty, "Color1");

                var lbl2 = new TextBlock
                {
                    Text = fecha.Day.ToString(),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                circulo.Child = lbl2;
                panel.Children.Add(circulo);
            }
            else
            {
                panel.Children.Add(lblDia);
            }

            foreach (var ev in eventos.Take(2))
            {
                var indicador = new Border
                {
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(4, 2, 4, 2),
                    Margin = new Thickness(0, 1, 0, 1),
                    Child = new TextBlock
                    {
                        Text = ev.NombreEvento,
                        FontSize = 10,
                        Foreground = Brushes.White,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                    },
                };
                indicador.SetResourceReference(Border.BackgroundProperty, "Color1");
                panel.Children.Add(indicador);
            }

            if (eventos.Count > 2)
            {
                var txtMas = new TextBlock
                {
                    Text = $"+{eventos.Count - 2} más",
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                txtMas.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
                panel.Children.Add(txtMas);
            }

            var celda = new Border
            {
                Child = panel,
                BorderThickness = new Thickness(0.5),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                MinHeight = 70,
                Background = Brushes.White,
                Cursor = Cursors.Hand,
            };

            if (esHoy)
                celda.SetResourceReference(Border.BackgroundProperty, "Color2");

            celda.MouseEnter += (s, e) =>
            {
                if (!esHoy)
                    celda.Background = new SolidColorBrush(Color.FromRgb(245, 250, 248));
            };

            celda.MouseLeave += (s, e) =>
            {
                if (!esHoy)
                    celda.Background = Brushes.White;
                else
                    celda.SetResourceReference(Border.BackgroundProperty, "Color2");
            };

            celda.MouseLeftButtonDown += (s, e) => MostrarPopupEventos(fecha, eventos, celda);

            return celda;
        }

        private void MostrarPopupEventos(
            DateTime fecha,
            List<EventoVista> eventos,
            UIElement elemento
        )
        {
            txtPopupFecha.Text = fecha.ToString(
                "dddd, dd 'de' MMMM 'de' yyyy",
                new System.Globalization.CultureInfo("es-ES")
            );

            lstPopupEventos.ItemsSource = eventos;
            txtSinEventos.Visibility =
                eventos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            lstPopupEventos.Visibility =
                eventos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            popupEventosDia.PlacementTarget = elemento;
            popupEventosDia.Placement = PlacementMode.Mouse;
            popupEventosDia.IsOpen = true;
        }

        private List<EventoVista> ObtenerEventosDeVista()
        {
            var vista = new List<EventoVista>();
            if (_eventos == null)
                return vista;

            foreach (var ev in _eventos)
            {
                var recinto = _recintos?.Find(r => r.IdRecinto == ev.IdRecinto);
                var fechaEvento = _fechasEventos?.Find(f => f.Id == ev.IdFechaEvento);

                if (fechaEvento == null)
                    continue;

                vista.Add(
                    new EventoVista
                    {
                        IdEvento = ev.IdEvento,
                        NombreEvento = ev.NombreEvento,
                        FechaEvento = fechaEvento.FechaInicio,
                        HoraEvento = fechaEvento.HoraInicio,
                        FechaFin = fechaEvento.FechaFin,
                        HoraFin = fechaEvento.HoraFin,
                        Categoria = ev.Categoria,
                        EstadoEvento = ev.EstadoEvento,
                        NombreRecinto = recinto?.NombreRecinto ?? ev.IdRecinto.ToString(),
                        CapacidadRecinto = recinto?.Capacidad ?? 0,
                    }
                );
            }
            return vista;
        }

        // ─── NAVEGACIÓN CALENDARIO ───

        private void btnMesAnterior_Click(object sender, RoutedEventArgs e)
        {
            _mesActual = _mesActual.AddMonths(-1);
            RenderizarCalendario();
        }

        private void btnMesSiguiente_Click(object sender, RoutedEventArgs e)
        {
            _mesActual = _mesActual.AddMonths(1);
            RenderizarCalendario();
        }

        // ─── BOTONES SUPERIORES ───

        private void btnIrReservas_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Navegar a Reservas",
                "Reservas",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void btnAgregarEvento_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            pnlFormulario.Visibility = Visibility.Visible;
            _modoEdicion = false;
            btnEditar.IsEnabled = false;
            btnEliminar.IsEnabled = false;
        }

        // ─── FORMULARIO ───

        private void cmbRecinto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRecinto.SelectedItem is Recinto recintoElegido)
                txtCapacidad.Text = recintoElegido.Capacidad.ToString();
            else
                txtCapacidad.Text = string.Empty;
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (
                string.IsNullOrWhiteSpace(txtNombreEvento.Text)
                || dpFechaEvento.SelectedDate == null
            )
            {
                MessageBox.Show(
                    "El nombre y la fecha son obligatorios.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!TimeSpan.TryParse(txtHoraEvento.Text, out TimeSpan horaValida))
            {
                MessageBox.Show(
                    "Escribe una hora válida (ejemplo: 18:30).",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (cmbRecinto.SelectedValue == null)
            {
                MessageBox.Show(
                    "Debe seleccionar un recinto.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            int idRecinto = (int)cmbRecinto.SelectedValue;

            try
            {
                if (_modoEdicion)
                {
                    var fechaEvento =
                        _fechasEventos?.Find(f => f.Id == _eventoSeleccionado.IdFechaEvento)
                        ?? new FechaEvento();

                    fechaEvento.FechaInicio = dpFechaEvento.SelectedDate.Value;
                    fechaEvento.FechaFin = dpFechaEvento.SelectedDate.Value;
                    fechaEvento.HoraInicio = horaValida;
                    fechaEvento.HoraFin = horaValida;

                    await ConexionDB.Client.From<FechaEvento>().Update(fechaEvento);

                    _eventoSeleccionado.NombreEvento = txtNombreEvento.Text.Trim();
                    _eventoSeleccionado.IdRecinto = idRecinto;
                    _eventoSeleccionado.Categoria = (
                        (ComboBoxItem)cmbTipoEvento.SelectedItem
                    ).Content.ToString();
                    _eventoSeleccionado.EstadoEvento = (
                        (ComboBoxItem)cmbEstadoEvento.SelectedItem
                    ).Content.ToString();

                    await ConexionDB.Client.From<Evento>().Update(_eventoSeleccionado);
                }
                else
                {
                    var fechaEvento = new FechaEvento
                    {
                        FechaInicio = dpFechaEvento.SelectedDate.Value,
                        FechaFin = dpFechaEvento.SelectedDate.Value,
                        HoraInicio = horaValida,
                        HoraFin = horaValida,
                    };

                    var fechaResponse = await ConexionDB
                        .Client.From<FechaEvento>()
                        .Insert(fechaEvento);
                    var fechaInsertada = fechaResponse.Models.First();

                    var nuevoEvento = new Evento
                    {
                        NombreEvento = txtNombreEvento.Text.Trim(),
                        IdRecinto = idRecinto,
                        IdOrganizador = _idUsuario,
                        Categoria = ((ComboBoxItem)cmbTipoEvento.SelectedItem).Content.ToString(),
                        EstadoEvento = (
                            (ComboBoxItem)cmbEstadoEvento.SelectedItem
                        ).Content.ToString(),
                        IdFechaEvento = fechaInsertada.Id,
                        EsPublico = "true",
                    };

                    await ConexionDB.Client.From<Evento>().Insert(nuevoEvento);
                }

                await CargarFechasEventos();
                await CargarEventos();
                LimpiarFormulario();
                pnlFormulario.Visibility = Visibility.Collapsed;

                MessageBox.Show(
                    "Evento guardado exitosamente",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar evento: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (_eventoSeleccionado == null)
                return;

            var fechaEvento = _fechasEventos?.Find(f => f.Id == _eventoSeleccionado.IdFechaEvento);

            txtNombreEvento.Text = _eventoSeleccionado.NombreEvento;
            dpFechaEvento.SelectedDate = fechaEvento?.FechaInicio ?? DateTime.Now;
            txtHoraEvento.Text =
                fechaEvento != null ? fechaEvento.HoraInicio.ToString(@"hh\:mm") : string.Empty;
            cmbRecinto.SelectedValue = _eventoSeleccionado.IdRecinto;

            foreach (ComboBoxItem item in cmbTipoEvento.Items)
                if (item.Content.ToString() == _eventoSeleccionado.Categoria)
                {
                    cmbTipoEvento.SelectedItem = item;
                    break;
                }

            foreach (ComboBoxItem item in cmbEstadoEvento.Items)
                if (item.Content.ToString() == _eventoSeleccionado.EstadoEvento)
                {
                    cmbEstadoEvento.SelectedItem = item;
                    break;
                }

            _modoEdicion = true;
            pnlFormulario.Visibility = Visibility.Visible;
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_eventoSeleccionado == null)
                return;

            var resultado = MessageBox.Show(
                $"¿Estás seguro de eliminar el evento '{_eventoSeleccionado.NombreEvento}'?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    int idFecha = _eventoSeleccionado.IdFechaEvento;

                    await ConexionDB
                        .Client.From<Evento>()
                        .Where(x => x.IdEvento == _eventoSeleccionado.IdEvento)
                        .Delete();

                    bool fechaUsadaOtraVez = _eventos.Any(ev =>
                        ev.IdEvento != _eventoSeleccionado.IdEvento && ev.IdFechaEvento == idFecha
                    );

                    if (!fechaUsadaOtraVez)
                    {
                        await ConexionDB
                            .Client.From<FechaEvento>()
                            .Where(f => f.Id == idFecha)
                            .Delete();
                    }

                    await CargarFechasEventos();
                    await CargarEventos();
                    LimpiarFormulario();
                    pnlFormulario.Visibility = Visibility.Collapsed;

                    MessageBox.Show(
                        "Evento eliminado",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            pnlFormulario.Visibility = Visibility.Collapsed;
            _eventoSeleccionado = null;
            _modoEdicion = false;
        }

        private void LimpiarFormulario()
        {
            txtNombreEvento.Clear();
            dpFechaEvento.SelectedDate = DateTime.Now;
            txtHoraEvento.Clear();
            txtCapacidad.Clear();
            cmbRecinto.SelectedIndex = -1;
            cmbTipoEvento.SelectedIndex = 0;
            cmbEstadoEvento.SelectedIndex = 0;
            cmbTipoAcceso.SelectedIndex = 0;
        }
    }

    public class EventoVista
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public DateTime FechaEvento { get; set; }
        public TimeSpan HoraEvento { get; set; }
        public DateTime FechaFin { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string NombreRecinto { get; set; } = string.Empty;
        public int CapacidadRecinto { get; set; }
        public string EstadoEvento { get; set; } = string.Empty;
    }
}
