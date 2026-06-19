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
        private int _idUsuario;
        private string _nombreUsuario;
        private List<MetodoPago> _metodosPago = new List<MetodoPago>();
        private DateTime _mesActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        public EventosView(int idUsuario, string nombreUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
            _nombreUsuario = nombreUsuario;
        }

        public EventosView(int idUsuario)
            : this(idUsuario, string.Empty) { }

        public EventosView()
            : this(0) { }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try { _metodosPago = (await ConexionDB.Client.From<MetodoPago>().Get()).Models; } catch { }
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

                // Normalizar estado_evento por si tiene comillas/cast sobrantes de la BD
                foreach (var ev in _eventos)
                {
                    ev.EstadoEvento = ev.EstadoEvento
                        .Replace("'", "")
                        .Replace("::character varying", "")
                        .Trim();
                }

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
                // No mostrar en el calendario eventos que aún no concretaron su venta/pago
                if (string.Equals(ev.EstadoEvento?.Trim(), "En reserva", StringComparison.OrdinalIgnoreCase)) continue; // ← NUEVO
                if (string.Equals(ev.EstadoEvento?.Trim(), "Cancelado", StringComparison.OrdinalIgnoreCase)) continue; // ← NUEVO

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

        private async void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is EventoVista evVista))
                return;

            var resultado = MessageBox.Show(
                $"¿Cancelar el evento '{evVista.NombreEvento}'?",
                "Confirmar cancelación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                var ev = _eventos?.Find(x => x.IdEvento == evVista.IdEvento);
                if (ev == null)
                    return;

                ev.EstadoEvento = "Cancelado";
                await ConexionDB.Client.From<Evento>().Update(ev);

                popupEventosDia.IsOpen = false;

                await CargarFechasEventos();
                await CargarEventos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cancelar: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void btnCrearEvento_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new NuevoPedidoDialog(_idUsuario, _metodosPago);
            ventana.Owner = Window.GetWindow(this);
            bool? result = ventana.ShowDialog();

            if (result != true)
                return;

            if (ventana.CreadoSinPago)
            {
                (Window.GetWindow(this) as Window1)?.AbrirCarritoEnEspera();
                return;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                await CargarFechasEventos();
                await CargarEventos();
            });
        }

        private void BtnReprogramar_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is EventoVista evVista))
                return;

            var ev = _eventos?.Find(x => x.IdEvento == evVista.IdEvento);
            var fechaEv = _fechasEventos?.Find(f => f.Id == ev?.IdFechaEvento);

            if (ev == null || fechaEv == null)
            {
                MessageBox.Show(
                    "No se encontraron los datos del evento.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            popupEventosDia.IsOpen = false;

            var ventana = new NuevoPedidoDialog(ev, fechaEv, _metodosPago);
            ventana.Owner = Window.GetWindow(this);
            bool? result = ventana.ShowDialog();

            if (result == true)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    await CargarFechasEventos();
                    await CargarEventos();
                });
            }
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
