using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public class EventoHistorialVista
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public string NombreRecinto { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public partial class HistorialClienteDialog : Window
    {
        private readonly Usuario _cliente;
        private List<EventoHistorialVista> _eventosVista = new List<EventoHistorialVista>();
        private List<Evento> _eventosReales = new List<Evento>();

        public HistorialClienteDialog(Usuario cliente)
        {
            InitializeComponent();
            _cliente = cliente;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtNombreCliente.Text = _cliente.NombreUsuario;
            txtInfoCliente.Text = $"@{_cliente.Username}  |  {_cliente.EmailUsuario}  |  {_cliente.Telefono}";
            await CargarHistorial();
        }

        private async Task CargarHistorial()
        {
            try
            {
                var eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;
                var recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
                var fechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;
                var ordenes = (await ConexionDB.Client.From<Orden>().Get()).Models;
                var pagos = (await ConexionDB.Client.From<Pago>().Get()).Models;

                var nombreCliente = _cliente.NombreUsuario.ToLower();
                _eventosReales = eventos
                    .Where(ev => ev.IdOrganizador == _cliente.IdUsuario ||
                                 (ev.NombreReservante ?? "").ToLower().Contains(nombreCliente))
                    .ToList();

                _eventosVista = _eventosReales
                    .Select(ev =>
                    {
                        var r = recintos.Find(rc => rc.IdRecinto == ev.IdRecinto);
                        var f = fechas.Find(fe => fe.Id == ev.IdFechaEvento);
                        return new EventoHistorialVista
                        {
                            IdEvento = ev.IdEvento,
                            NombreEvento = ev.NombreEvento,
                            NombreRecinto = r?.NombreRecinto ?? "-",
                            Fecha = f?.FechaInicio.ToString("dd/MM/yyyy") ?? "-",
                            Estado = ev.EstadoEvento,
                        };
                    })
                    .OrderByDescending(x => x.Fecha)
                    .ToList();

                txtTotalEventos.Text = _eventosVista.Count.ToString();
                dgEventos.ItemsSource = _eventosVista;

                var ordenesCliente = ordenes
                    .Where(o => o.IdUsuario == _cliente.IdUsuario ||
                                (o.CompradorNombre ?? "").ToLower().Contains(nombreCliente))
                    .ToList();

                decimal totalGastado = 0;
                var filaOrdenes = ordenesCliente
                    .Select(o =>
                    {
                        var pago = pagos.Find(p => p.IdOrden == o.IdOrden);
                        totalGastado += pago?.MontoPago ?? 0;
                        return new
                        {
                            Fecha = o.FechaOrden.ToString("dd/MM/yyyy HH:mm"),
                            Comprador = o.CompradorNombre,
                            Nit = o.CompradorNit,
                            Total = $"Bs {(pago?.MontoPago ?? 0):N2}",
                            Estado = o.EstadoOrden,
                        };
                    })
                    .OrderByDescending(x => x.Fecha)
                    .ToList();

                txtTotalOrdenes.Text = ordenesCliente.Count.ToString();
                txtTotalGastado.Text = $"Bs {totalGastado:N2}";
                dgOrdenes.ItemsSource = filaOrdenes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnEditarEvento_Click(object sender, RoutedEventArgs e)
        {
            var vista = (sender as Button)?.Tag as EventoHistorialVista;
            if (vista == null) return;

            var eventoReal = _eventosReales.Find(ev => ev.IdEvento == vista.IdEvento);
            if (eventoReal == null) return;

            var dialog = new EditarEventoDialog(eventoReal);
            dialog.Owner = this;
            var result = dialog.ShowDialog();

            if (result == true)
                await CargarHistorial();
        }
    }
}
