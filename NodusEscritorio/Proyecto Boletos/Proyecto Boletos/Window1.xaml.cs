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
using System.Windows.Shapes;
using System.Windows.Threading;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos
{
    public partial class Window1 : Window
    {
        private Usuario _usuarioActual;
        private Rol _rolActual;

        public Window1(Usuario usuario, Rol rol)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            _rolActual = rol;
            Timer();
            ConfigurarInterfaz();
            MainContent.Content = new vistas.DashboardView();
        }

        private void ConfigurarInterfaz()
        {
            tbUsuario.Text = _usuarioActual.NombreUsuario;
            tbRol.Text = _rolActual?.Nombre ?? "Sin rol";

            // Partimos mostrando todos los botones del sidebar
            btnDashboard.Visibility = Visibility.Visible;
            btnClientes.Visibility = Visibility.Visible;
            btnUsuarios.Visibility = Visibility.Visible;
            btnEventos.Visibility = Visibility.Visible;
            btnRecintos.Visibility = Visibility.Visible;
            btnProveedores.Visibility = Visibility.Visible;
            btnReportes.Visibility = Visibility.Visible;
            btnCarrito.Visibility = Visibility.Visible;
            btnMonitoreoWeb.Visibility = Visibility.Visible;
            btnMonitoreoMovil.Visibility = Visibility.Visible;

            string nombreRol = _rolActual?.Nombre?.Trim() ?? "";

            if (string.Equals(nombreRol, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                // Organizador: solo Dashboard, Clientes, Eventos, Recintos, Proveedores, Carrito
                btnUsuarios.Visibility = Visibility.Collapsed;
                btnReportes.Visibility = Visibility.Collapsed;
                btnMonitoreoWeb.Visibility = Visibility.Collapsed;
                btnMonitoreoMovil.Visibility = Visibility.Collapsed;
            }
            // Administrador: todos los botones visibles (por defecto arriba)
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            MainWindow principal = new MainWindow();
            principal.Show();
            this.Close();
        }

        private void btnCambio_Click(object sender, RoutedEventArgs e)
        {
            Tema.CambiarTema();
        }

        private void Timer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                tbFechaHora.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            timer.Start();
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.DashboardView();
        }

        private void btnEventos_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.EventosView(_usuarioActual.IdUsuario, _usuarioActual.NombreUsuario);
        }

        private void btnRecintos_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.RecintosView();
        }

        private void btnProveedores_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.ProveedoresView(_usuarioActual.IdRol);
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.ReportesView();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.Configuracion();
        }

        private void btnCarrito_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.CarritoView(_usuarioActual.IdUsuario);
        }

        public void AbrirCarritoEnEspera()
        {
            MainContent.Content = new vistas.CarritoView(_usuarioActual.IdUsuario, "espera");
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new vistas.ClientesView(_usuarioActual.IdUsuario);
        }

        private void btnMonitoreoWeb_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new vistas.MonitoreoWebView();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void btnMonitoreoMovil_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new vistas.MonitoreoMovilView();
            ventana.Owner = this;
            ventana.ShowDialog();
        }
    }
}
