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
    /// Lógica de interacción para DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatosReales();
        }

        private async Task CargarDatosReales()
        {
            try
            {
                // 1. USUARIOS ACTIVOS
                var responseUsuarios = await ConexionDB.Client.From<Usuario>()
                    .Filter("estado_usuario", Supabase.Postgrest.Constants.Operator.ILike, "activo")
                    .Get();
                txtUsuariosActivos.Text = responseUsuarios.Models.Count.ToString();

                // 2. EVENTOS + FECHAS + RECINTOS
                var eventos  = (await ConexionDB.Client.From<Evento>().Get()).Models;
                var fechas   = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;
                var recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;

                foreach (var ev in eventos)
                {
                    ev.EstadoEvento = ev.EstadoEvento
                        .Replace("'", "")
                        .Replace("::character varying", "")
                        .Trim();
                }

                var estadosActivos = new[] { "Programado", "Reprogramado" };

                // Solo eventos con estado activo cuya fecha de inicio aún no ha llegado
                var eventosActivos = eventos
                    .Select(ev => new
                    {
                        Evento      = ev,
                        FechaEvento = fechas.Find(f => f.Id == ev.IdFechaEvento),
                    })
                    .Where(x => x.FechaEvento != null
                             && estadosActivos.Contains(x.Evento.EstadoEvento)
                             && x.FechaEvento.FechaInicio.Date > DateTime.Today)
                    .OrderBy(x => x.FechaEvento.FechaInicio)
                    .ToList();

                // 3. CONTADOR — solo eventos aún no iniciados
                txtTotalEventos.Text = eventosActivos.Count.ToString();

                // 4. GRID DE EVENTOS ACTIVOS
                dgProximosEventos.ItemsSource = eventosActivos
                    .Select(x =>
                    {
                        var recinto = recintos.Find(r => r.IdRecinto == x.Evento.IdRecinto);
                        return new
                        {
                            Fecha   = x.FechaEvento.FechaInicio.ToString("dd/MM/yyyy"),
                            Hora    = x.FechaEvento.HoraInicio.ToString(@"hh\:mm"),
                            Nombre  = x.Evento.NombreEvento,
                            Recinto = recinto?.NombreRecinto ?? "-",
                            Estado  = x.Evento.EstadoEvento,
                        };
                    })
                    .ToList();

                // 5. VENTAS — detalle_orden + orden + boleto
                try
                {
                    var detalles = (await ConexionDB.Client.From<DetalleOrden>().Get()).Models;
                    var ordenes  = (await ConexionDB.Client.From<Orden>().Get()).Models;
                    var boletos  = (await ConexionDB.Client.From<Boleto>().Get()).Models;

                    bool EsPagado(string estado) =>
                        string.Equals(estado?.Trim(), "pagado", StringComparison.OrdinalIgnoreCase);

                    var detallesPagados = detalles
                        .Where(d => EsPagado(ordenes.Find(o => o.IdOrden == d.IdOrden)?.EstadoOrden))
                        .ToList();

                    txtBoletosVendidos.Text = detallesPagados.Count.ToString();
                    txtIngresos.Text = $"BS {detallesPagados.Sum(d => d.PrecioUnitario * d.Cantidad):N0}";

                    var ventas = detalles
                        .Select(d =>
                        {
                            var orden  = ordenes.Find(o => o.IdOrden == d.IdOrden);
                            var boleto = boletos.Find(b => b.IdBoleto == d.IdBoleto);
                            var evNombre = boleto != null
                                ? (eventos.Find(ev => ev.IdEvento == (int)(boleto.IdEvento ?? 0))?.NombreEvento ?? "-")
                                : "-";
                            bool pagado = EsPagado(orden?.EstadoOrden);
                            return new VentaDashboard
                            {
                                _fechaOrden = orden?.FechaOrden ?? DateTime.MinValue,
                                Fecha      = orden?.FechaOrden.ToString("dd/MM/yyyy HH:mm") ?? "-",
                                Evento     = evNombre,
                                TipoBoleto = boleto?.TipoBoleto ?? "-",
                                Precio     = $"BS {d.PrecioUnitario:N0}",
                                Comprador  = string.IsNullOrEmpty(orden?.CompradorNombre) ? "-" : orden.CompradorNombre,
                                Estado     = pagado ? "Pagado" : "Pendiente",
                            };
                        })
                        .OrderByDescending(v => v._fechaOrden)
                        .ToList();

                    dgVentas.ItemsSource = ventas;
                }
                catch
                {
                    txtBoletosVendidos.Text = "0";
                    txtIngresos.Text = "BS 0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar el dashboard: {ex.Message}",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

    }

    public class VentaDashboard
    {
        public DateTime _fechaOrden { get; set; }
        public string Fecha      { get; set; } = string.Empty;
        public string Evento     { get; set; } = string.Empty;
        public string TipoBoleto { get; set; } = string.Empty;
        public string Precio     { get; set; } = string.Empty;
        public string Comprador  { get; set; } = string.Empty;
        public string Estado     { get; set; } = string.Empty;
    }
}
