using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class ClientesView : UserControl
    {
        private int _idUsuarioAdmin;
        private List<MetodoPago> _metodosPago = new List<MetodoPago>();
        private List<Usuario> _todosClientes = new List<Usuario>();

        public ClientesView(int idUsuarioAdmin)
        {
            InitializeComponent();
            _idUsuarioAdmin = idUsuarioAdmin;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                var roles = (await ConexionDB.Client.From<Rol>().Get()).Models;
                var idRolCliente = roles
                    .Where(r =>
                        string.Equals(r.Nombre, "Cliente", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(r.Nombre, "Client", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(r.Nombre, "Usuario", StringComparison.OrdinalIgnoreCase)
                    )
                    .Select(r => r.IdRol)
                    .ToHashSet();

                // Si no hay rol "Cliente" definido, tomamos todos los que no son Admin ni Organizador
                var rolesPrivilegiados = roles
                    .Where(r =>
                        string.Equals(r.Nombre, "Admin", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(
                            r.Nombre,
                            "Organizador",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .Select(r => r.IdRol)
                    .ToHashSet();

                var usuarios = (await ConexionDB.Client.From<Usuario>().Get()).Models;
                _todosClientes = usuarios
                    .Where(u => !rolesPrivilegiados.Contains(u.IdRol))
                    .OrderBy(u => u.NombreUsuario)
                    .ToList();

                _metodosPago = (await ConexionDB.Client.From<MetodoPago>().Get()).Models;

                MostrarClientes(_todosClientes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar clientes: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void MostrarClientes(List<Usuario> lista)
        {
            lvClientes.ItemsSource = null;
            lvClientes.ItemsSource = lista;
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtBuscar.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(texto))
            {
                MostrarClientes(_todosClientes);
                return;
            }

            var filtrado = _todosClientes
                .Where(u =>
                    (u.NombreUsuario ?? "").ToLower().Contains(texto)
                    || (u.Username ?? "").ToLower().Contains(texto)
                    || (u.EmailUsuario ?? "").ToLower().Contains(texto)
                    || (u.Telefono ?? "").ToLower().Contains(texto)
                )
                .ToList();

            MostrarClientes(filtrado);
        }

        private void btnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NuevoClienteDialog();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            if (dialog.ClienteCreado != null)
            {
                // Recargar y opcionalmente abrir evento para el nuevo cliente
                _ = CargarDatos();

                var resultado = MessageBox.Show(
                    $"Cliente '{dialog.ClienteCreado.NombreUsuario}' registrado.\n¿Crear un evento para este cliente ahora?",
                    "Cliente Registrado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (resultado == MessageBoxResult.Yes)
                    AbrirCrearEvento(dialog.ClienteCreado);
            }
        }

        private void btnCrearEvento_Click(object sender, RoutedEventArgs e)
        {
            var cliente = (sender as Button)?.Tag as Usuario;
            if (cliente == null)
                return;
            AbrirCrearEvento(cliente);
        }

        private void AbrirCrearEvento(Usuario cliente)
        {
            var ventana = new NuevoPedidoDialog(_idUsuarioAdmin, _metodosPago, cliente);
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }

        private async void btnHistorial_Click(object sender, RoutedEventArgs e)
        {
            var cliente = (sender as Button)?.Tag as Usuario;
            if (cliente == null)
                return;

            try
            {
                var ventana = new HistorialClienteDialog(cliente);
                ventana.Owner = Window.GetWindow(this);
                ventana.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
