using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class MonitoreoWebView : Window
    {
        public MonitoreoWebView()
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
                // Cargar roles para identificar Admin y Organizador por IdRol
                var roles = (await ConexionDB.Client.From<Rol>().Get()).Models;
                var rolesPrivilegiados = roles
                    .Where(r => string.Equals(r.Nombre, "Admin", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(r.Nombre, "Organizador", StringComparison.OrdinalIgnoreCase))
                    .Select(r => r.IdRol)
                    .ToHashSet();

                // Usuarios web (clientes): los que no tienen rol Admin/Organizador
                var usuarios = (await ConexionDB.Client.From<Usuario>().Get()).Models;
                var usuariosWeb = usuarios
                    .Where(u => !rolesPrivilegiados.Contains(u.IdRol))
                    .OrderByDescending(u => u.IdUsuario)
                    .ToList();

                txtWebUsuarios.Text = usuariosWeb.Count.ToString();
                dgWebUsuarios.ItemsSource = usuariosWeb
                    .Select(u => new
                    {
                        Nombre = u.NombreUsuario,
                        Correo = u.EmailUsuario,
                        Estado = u.EstadoUsuario,
                    })
                    .ToList();

                // Eventos publicos
                var eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;
                var eventosPublicos = eventos.Where(ev => ev.EsPublico).ToList();
                txtWebEventos.Text = eventosPublicos.Count.ToString();

                // Ordenes recientes
                var ordenes = (await ConexionDB.Client.From<Orden>().Get()).Models
                    .OrderByDescending(o => o.FechaOrden)
                    .Take(50)
                    .ToList();

                txtWebOrdenes.Text = ordenes.Count.ToString();
                dgWebOrdenes.ItemsSource = ordenes
                    .Select(o => new
                    {
                        Fecha = o.FechaOrden.ToString("dd/MM/yyyy HH:mm"),
                        Comprador = string.IsNullOrEmpty(o.CompradorNombre) ? "-" : o.CompradorNombre,
                        Total = $"Bs {o.DescuentoOrden:N0}",
                        Estado = o.EstadoOrden,
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos web: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
