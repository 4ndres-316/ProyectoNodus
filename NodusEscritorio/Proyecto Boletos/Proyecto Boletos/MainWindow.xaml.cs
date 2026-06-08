using Proyecto_Boletos.Db;
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
using static System.Net.WebRequestMethods;

namespace Proyecto_Boletos
{
    public partial class MainWindow : Window
    {
        private bool mostrando = false;

        public MainWindow()
        {
            InitializeComponent();
            _ = InicializarConexion();
        }

        private async Task InicializarConexion()
        {
            try
            {
                await ConexionDB.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al conectar:\n{ex.Message}",
                    "Error de Conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var usuario = tbUser.Text.Trim();
            var contrasena = pbPassword.Password;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show(
                    "Ingrese su usuario.",
                    "Campo Requerido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                tbUser.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                MessageBox.Show(
                    "Ingrese su contraseña.",
                    "Campo Requerido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                pbPassword.Focus();
                return;
            }

            btnLogin.IsEnabled = false;

            try
            {
                var resultado = await ConexionDB
                    .Client.From<Usuario>()
                    .Where(u => u.Username == usuario)
                    .Single();

                if (resultado == null)
                {
                    MessageBox.Show(
                        $"Usuario '{usuario}' no encontrado.",
                        "Usuario no encontrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    tbUser.Focus();
                    return;
                }

                if (resultado.Password != contrasena)
                {
                    MessageBox.Show(
                        "Contraseña incorrecta.",
                        "Error de Autenticación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    pbPassword.Clear();
                    pbPassword.Focus();
                    return;
                }

                if (resultado.IdRol == 3)
                {
                    MessageBox.Show(
                        $"Usuario: {usuario}\nNo tiene acceso al sistema.",
                        "Acceso Denegado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation
                    );
                    return;
                }

                if (resultado.EstadoUsuario != "Activo")
                {
                    var resultado_dialogo = MessageBox.Show(
                        $"Usuario: {usuario}\nSu cuenta ha sido deshabilitada.\nPonganse en contacto con nosotros o entre a la web para solucionar el problema.\nTeléfono: 68630004",
                        "Acceso Denegado",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation
                    );

                    if (resultado_dialogo == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://www.youtube.com/",
                            UseShellExecute = true
                        });
                    }
                    return;
                }

                new Window1(resultado).Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al verificar credenciales:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                btnLogin.IsEnabled = true;
            }
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCambio_Click(object sender, RoutedEventArgs e)
        {
            Tema.CambiarTema();
        }

        private void btnMostrar_Click(object sender, RoutedEventArgs e)
        {
            if (!mostrando)
            {
                txtPassword.Text = pbPassword.Password;

                txtPassword.Visibility = Visibility.Visible;
                pbPassword.Visibility = Visibility.Collapsed;

                iconoOjo.Text = "\uF460";

                mostrando = true;
            }
            else
            {
                pbPassword.Password = txtPassword.Text;

                pbPassword.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;

                iconoOjo.Text = "\uE890";

                mostrando = false;
            }
        }
    }
}
