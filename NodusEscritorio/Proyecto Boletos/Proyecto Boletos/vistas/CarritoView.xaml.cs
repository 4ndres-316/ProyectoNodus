using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    /// <summary>
    /// Lógica de interacción para CarritoView.xaml
    /// </summary>
    public partial class CarritoView : UserControl
    {
        private int _idUsuario;
        private List<MetodoPago> _metodosPago;
        private string _vistaActual = "boletos";

        public CarritoView(int idUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
        }

        public CarritoView()
            : this(0) { }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarMetodosPago();
            await CargarEventos();
        }

        private async Task CargarMetodosPago()
        {
            try
            {
                _metodosPago = (await ConexionDB.Client.From<MetodoPago>().Get()).Models;
                if (_metodosPago == null || _metodosPago.Count == 0)
                    _metodosPago = (await ConexionDB.Client.From<MetodoPago>().Get()).Models;
            }
            catch { }
        }

        // ─── BOTONES ─────────────────────────────────────────────────────────

        private async void btnBoletos_Click(object sender, RoutedEventArgs e)
        {
            _vistaActual = "boletos";
            await CargarEventos();
        }

        private async void btnReservas_Click(object sender, RoutedEventArgs e)
        {
            _vistaActual = "reservas";
            await CargarReservas();
        }

        private async void btnEventos_Click(object sender, RoutedEventArgs e)
        {
            _vistaActual = "eventos";
            await CargarEventos();
        }

        // ─── CARGA DE DATOS ──────────────────────────────────────────────────

        private async Task CargarEventos()
        {
            try
            {
                var eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;
                var recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
                var fechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;

                var pendientes = eventos
                    .Where(ev => (ev.EstadoEvento ?? "").ToLower() == "en espera")
                    .Select(ev =>
                    {
                        var recinto = recintos.Find(r => r.IdRecinto == ev.IdRecinto);
                        var fecha = fechas.Find(f => f.Id == ev.IdFechaEvento);
                        return new EventoCarritoVista
                        {
                            IdEvento = ev.IdEvento,
                            NombreEvento = ev.NombreEvento,
                            NombreRecinto = recinto?.NombreRecinto ?? ev.IdRecinto.ToString(),
                            FechaInicio = fecha?.FechaInicio.ToString("dd/MM/yyyy") ?? "",
                            NombreReservante = ev.NombreReservante,
                            Estado = ev.EstadoEvento,
                        };
                    })
                    .ToList();

                ConfigurarColumnas(false);
                dgVentas.ItemsSource = pendientes;
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

        private async Task CargarReservas()
        {
            try
            {
                var reservas = (await ConexionDB.Client.From<Reserva>().Get()).Models;
                var recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
                var fechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;

                var pendientes = reservas
                    .Where(r =>
                        (r.EstadoReserva ?? "").ToLower() == "en espera"
                        || (r.EstadoReserva ?? "").ToLower() == "activa"
                    )
                    .Select(r =>
                    {
                        var recinto = recintos.Find(rc => rc.IdRecinto == r.IdRecinto);
                        var fecha = fechas.Find(f => f.Id == r.FechaReserva);
                        return new ReservaCarritoVista
                        {
                            IdReserva = r.IdReserva,
                            NombreReservante = r.NombreReservante,
                            NombreRecinto = recinto?.NombreRecinto ?? r.IdRecinto.ToString(),
                            FechaInicio = fecha?.FechaInicio.ToString("dd/MM/yyyy") ?? "",
                            Estado = r.EstadoReserva,
                        };
                    })
                    .ToList();

                ConfigurarColumnas(true);
                dgVentas.ItemsSource = pendientes;
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
        }

        // ─── COLUMNAS DINÁMICAS ──────────────────────────────────────────────

        private void ConfigurarColumnas(bool esReserva)
        {
            dgVentas.AutoGenerateColumns = false;
            dgVentas.Columns.Clear();

            if (esReserva)
            {
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "ID",
                        Binding = new System.Windows.Data.Binding("IdReserva"),
                        Width = 50,
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Reservante",
                        Binding = new System.Windows.Data.Binding("NombreReservante"),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Recinto",
                        Binding = new System.Windows.Data.Binding("NombreRecinto"),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Fecha",
                        Binding = new System.Windows.Data.Binding("FechaInicio"),
                        Width = 100,
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Estado",
                        Binding = new System.Windows.Data.Binding("Estado"),
                        Width = 100,
                    }
                );
            }
            else
            {
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "ID",
                        Binding = new System.Windows.Data.Binding("IdEvento"),
                        Width = 50,
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Evento",
                        Binding = new System.Windows.Data.Binding("NombreEvento"),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Recinto",
                        Binding = new System.Windows.Data.Binding("NombreRecinto"),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Fecha",
                        Binding = new System.Windows.Data.Binding("FechaInicio"),
                        Width = 100,
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Reservante",
                        Binding = new System.Windows.Data.Binding("NombreReservante"),
                        Width = 120,
                    }
                );
                dgVentas.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = "Estado",
                        Binding = new System.Windows.Data.Binding("Estado"),
                        Width = 100,
                    }
                );
            }

            // Columna Pagar
            var colPagar = new DataGridTemplateColumn { Header = "Pagar", Width = 80 };
            var templatePagar = new DataTemplate();
            var btnPagarFactory = new FrameworkElementFactory(typeof(Button));
            btnPagarFactory.SetValue(Button.ContentProperty, "Pagar");
            btnPagarFactory.SetValue(Button.HeightProperty, 26.0);
            btnPagarFactory.SetValue(Button.PaddingProperty, new Thickness(8, 0, 8, 0));
            btnPagarFactory.SetValue(
                Button.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(46, 125, 90))
            );
            btnPagarFactory.SetValue(Button.ForegroundProperty, Brushes.White);
            btnPagarFactory.SetValue(
                Button.BorderBrushProperty,
                new SolidColorBrush(Color.FromRgb(46, 125, 90))
            );
            btnPagarFactory.SetValue(Button.CursorProperty, Cursors.Hand);
            btnPagarFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(btnPagar_Click));
            templatePagar.VisualTree = btnPagarFactory;
            colPagar.CellTemplate = templatePagar;
            dgVentas.Columns.Add(colPagar);

            // Columna Eliminar
            var colEliminar = new DataGridTemplateColumn { Header = "Eliminar", Width = 80 };
            var templateEliminar = new DataTemplate();
            var btnEliminarFactory = new FrameworkElementFactory(typeof(Button));
            btnEliminarFactory.SetValue(Button.ContentProperty, "Eliminar");
            btnEliminarFactory.SetValue(Button.HeightProperty, 26.0);
            btnEliminarFactory.SetValue(Button.PaddingProperty, new Thickness(8, 0, 8, 0));
            btnEliminarFactory.SetValue(
                Button.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(244, 67, 54))
            );
            btnEliminarFactory.SetValue(Button.ForegroundProperty, Brushes.White);
            btnEliminarFactory.SetValue(
                Button.BorderBrushProperty,
                new SolidColorBrush(Color.FromRgb(244, 67, 54))
            );
            btnEliminarFactory.SetValue(Button.CursorProperty, Cursors.Hand);
            btnEliminarFactory.AddHandler(
                Button.ClickEvent,
                new RoutedEventHandler(btnEliminar_Click)
            );
            templateEliminar.VisualTree = btnEliminarFactory;
            colEliminar.CellTemplate = templateEliminar;
            dgVentas.Columns.Add(colEliminar);
        }

        // ─── ACCIONES ────────────────────────────────────────────────────────

        private void btnPagar_Click(object sender, RoutedEventArgs e)
        {
            var ventanaPago = new PagoEventoDialog(_metodosPago);
            ventanaPago.Owner = Window.GetWindow(this);
            ventanaPago.ShowDialog();

            if (ventanaPago.Resultado == ResultadoPago.Cancelado)
                return;

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var btn = sender as Button;
                    var row = btn?.DataContext;

                    if (_vistaActual == "reservas")
                    {
                        var item = row as ReservaCarritoVista;
                        if (item == null)
                            return;

                        // Actualizar estado reserva
                        var reserva = (
                            await ConexionDB
                                .Client.From<Reserva>()
                                .Where(r => r.IdReserva == item.IdReserva)
                                .Get()
                        ).Models.FirstOrDefault();
                        if (reserva != null)
                        {
                            reserva.EstadoReserva = "Pagado";
                            await ConexionDB.Client.From<Reserva>().Update(reserva);
                        }
                    }
                    else
                    {
                        var item = row as EventoCarritoVista;
                        if (item == null)
                            return;

                        // Actualizar estado evento
                        var evento = (
                            await ConexionDB
                                .Client.From<Evento>()
                                .Where(ev => ev.IdEvento == item.IdEvento)
                                .Get()
                        ).Models.FirstOrDefault();
                        if (evento != null)
                        {
                            evento.EstadoEvento = "Pagado";
                            await ConexionDB.Client.From<Evento>().Update(evento);
                        }
                    }

                    // Registrar pago
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

                    MessageBox.Show(
                        "Pago registrado exitosamente.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Recargar
                    if (_vistaActual == "reservas")
                        await CargarReservas();
                    else
                        await CargarEventos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al pagar: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            });
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var row = btn?.DataContext;

            if (_vistaActual == "reservas")
            {
                var item = row as ReservaCarritoVista;
                if (item == null)
                    return;

                if (
                    MessageBox.Show(
                        $"¿Eliminar la reserva de '{item.NombreReservante}'?",
                        "Confirmar",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    ) != MessageBoxResult.Yes
                )
                    return;

                try
                {
                    await ConexionDB
                        .Client.From<Reserva>()
                        .Where(r => r.IdReserva == item.IdReserva)
                        .Delete();
                    await CargarReservas();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al eliminar: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            else
            {
                var item = row as EventoCarritoVista;
                if (item == null)
                    return;

                if (
                    MessageBox.Show(
                        $"¿Eliminar el evento '{item.NombreEvento}'?",
                        "Confirmar",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    ) != MessageBoxResult.Yes
                )
                    return;

                try
                {
                    await ConexionDB
                        .Client.From<Evento>()
                        .Where(ev => ev.IdEvento == item.IdEvento)
                        .Delete();
                    await CargarEventos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al eliminar: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }
    }

    // ─── CLASES VISTA ────────────────────────────────────────────────────────────

    public class EventoCarritoVista
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public string NombreRecinto { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public string NombreReservante { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class ReservaCarritoVista
    {
        public int IdReserva { get; set; }
        public string NombreReservante { get; set; } = string.Empty;
        public string NombreRecinto { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
