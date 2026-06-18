using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class MonitoreoMovilView : Window
    {
        public MonitoreoMovilView()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async void btnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                var eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;

                // Boletos
                var boletos = (await ConexionDB.Client.From<Boleto>().Get()).Models
                    .OrderByDescending(b => b.IdBoleto)
                    .ToList();

                txtMovilBoletos.Text = boletos.Count.ToString();
                dgMovilBoletos.ItemsSource = boletos
                    .Take(60)
                    .Select(b =>
                    {
                        var evNombre = eventos.Find(ev => ev.IdEvento == (int)(b.IdEvento ?? 0))?.NombreEvento ?? "-";
                        return new
                        {
                            Codigo = b.Codigo,
                            Evento = evNombre,
                            Tipo = b.TipoBoleto,
                            Precio = $"Bs {b.PrecioBoleto:N0}",
                            Estado = b.EstadoValidacion,
                        };
                    })
                    .ToList();

                // Reservas activas (eventos confirmados / programados)
                var reservasActivas = eventos
                    .Where(ev => string.Equals(ev.EstadoEvento, "Confirmado", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(ev.EstadoEvento, "Programado", StringComparison.OrdinalIgnoreCase))
                    .Count();
                txtMovilReservas.Text = reservasActivas.ToString();

                // Invitados
                var invitados = (await ConexionDB.Client.From<Invitado>().Get()).Models
                    .OrderByDescending(i => i.IdInvitado)
                    .ToList();

                txtMovilInvitados.Text = invitados.Count.ToString();
                dgMovilInvitados.ItemsSource = invitados
                    .Take(60)
                    .Select(i => new
                    {
                        Nombre = i.NombreInvitado,
                        Tipo = i.TipoInvitado,
                        Estado = i.EstadoInvitado,
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos movil: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
