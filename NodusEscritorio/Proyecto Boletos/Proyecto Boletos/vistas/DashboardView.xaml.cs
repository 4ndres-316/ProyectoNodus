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
                var responseUsuarios = await ConexionDB
                    .Client.From<Usuario>()
                    .Filter("estado_usuario", Supabase.Postgrest.Constants.Operator.Equals, "Activo")
                    .Get();
                txtUsuariosActivos.Text = responseUsuarios.Models.Count.ToString();

                // 2. EVENTOS + FECHAS + RECINTOS
                var responseEventos  = await ConexionDB.Client.From<Evento>().Get();
                var responseFechas   = await ConexionDB.Client.From<FechaEvento>().Get();
                var responseRecintos = await ConexionDB.Client.From<Recinto>().Get();

                var eventos  = responseEventos.Models;
                var fechas   = responseFechas.Models;
                var recintos = responseRecintos.Models;

                // Normalizar estado por si la BD trae valores con comillas/cast
                foreach (var ev in eventos)
                {
                    ev.EstadoEvento = ev.EstadoEvento
                        .Replace("'", "")
                        .Replace("::character varying", "")
                        .Trim();
                }

                var eventosConFecha = eventos
                    .Select(ev => new
                    {
                        Evento      = ev,
                        FechaEvento = fechas.Find(f => f.Id == ev.IdFechaEvento),
                    })
                    .Where(x => x.FechaEvento != null)
                    .ToList();

                // 3. TOTAL EVENTOS PROGRAMADOS
                txtTotalEventos.Text = eventosConFecha
                    .Count(x => x.Evento.EstadoEvento == "Programado"
                             || x.Evento.EstadoEvento == "En espera"
                             || x.Evento.EstadoEvento == "Pagado")
                    .ToString();

                // 4. PRÓXIMOS EVENTOS
                var proximosEventos = eventosConFecha
                    .Where(x => x.FechaEvento.FechaInicio.Date >= DateTime.Today)
                    .OrderBy(x => x.FechaEvento.FechaInicio)
                    .Select(x =>
                    {
                        var recinto = recintos.Find(r => r.IdRecinto == x.Evento.IdRecinto);
                        return new
                        {
                            Fecha   = x.FechaEvento.FechaInicio.ToString("dd/MM/yyyy"),
                            Hora    = x.FechaEvento.HoraInicio.ToString(@"hh\:mm"),
                            Nombre  = x.Evento.NombreEvento,
                            Recinto = recinto?.NombreRecinto ?? x.Evento.IdRecinto.ToString(),
                            Estado  = x.Evento.EstadoEvento,
                        };
                    })
                    .ToList();

                dgProximosEventos.ItemsSource = proximosEventos;

                // 5. BOLETOS VENDIDOS e INGRESOS
                try
                {
                    var responseBoletos = await ConexionDB.Client.From<Boleto>().Get();
                    var boletos = responseBoletos.Models;
                    var vendidos = boletos.Where(b =>
                        b.EstadoBoleto == "vendido" ||
                        b.EstadoBoleto == "usado" ||
                        b.EstadoBoleto == "pagado").ToList();

                    txtBoletosVendidos.Text = vendidos.Count.ToString();
                    txtIngresos.Text = $"BS {vendidos.Sum(b => b.PrecioBoleto):N0}";
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
}
