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

        public Window1(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            Timer();
            ConfigurarInterfaz();
            MainContent.Content = new vistas.DashboardView();
        }

        private void ConfigurarInterfaz()
        {
            tbUsuario.Text = _usuarioActual.NombreUsuario;
            tbRol.Text = _usuarioActual.IdRol == 1 ? "Administrador" : "Organizador";

            if (_usuarioActual.IdRol != 1)
                btnUsuarios.Visibility = Visibility.Collapsed;
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
            MainContent.Content = new vistas.EventosView(_usuarioActual.IdUsuario);
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
            MainContent.Content = new vistas.CarritoView();
        }
    }
}
