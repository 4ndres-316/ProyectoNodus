using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class NuevoClienteDialog : Window
    {
        public Usuario ClienteCreado { get; private set; }

        public NuevoClienteDialog()
        {
            InitializeComponent();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = txtNombre.Text.Trim();
            var username = txtUsername.Text.Trim();
            var contrasena = pbPassword.Password;

            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingresa el nombre completo del cliente.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Ingresa un username para el cliente.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(contrasena))
            {
                MessageBox.Show("Ingresa una contrasena para el cliente.", "Validacion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                pbPassword.Focus();
                return;
            }

            try
            {
                // Verificar que el username no exista ya
                var existente = await ConexionDB.Client.From<Usuario>()
                    .Where(u => u.Username == username)
                    .Get();

                if (existente.Models.Count > 0)
                {
                    MessageBox.Show($"El username '{username}' ya esta en uso. Elige otro.", "Username duplicado",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUsername.Focus();
                    return;
                }

                // Hash SHA-256 de "username:password" igual que la app movil
                string hashPassword;
                using (var sha = SHA256.Create())
                {
                    var input = $"{username}:{contrasena}";
                    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                    hashPassword = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }

                // Buscar IdRol de cliente (el que no es Admin ni Organizador)
                var roles = (await ConexionDB.Client.From<Rol>().Get()).Models;

                // Buscar explícitamente el rol "Cliente"
                var rolCliente = roles.Find(r =>
                    string.Equals(r.Nombre, "Cliente", StringComparison.OrdinalIgnoreCase));

                int idRolCliente = rolCliente?.IdRol ?? 3;

                var nuevoUsuario = new Usuario
                {
                    IdRol = idRolCliente,
                    NombreUsuario = nombre,
                    Username = username,
                    EmailUsuario = txtCorreo.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Password = hashPassword,
                    EstadoUsuario = "Activo",
                    FotoRostro = "",
                };

                var resp = await ConexionDB.Client.From<Usuario>().Insert(nuevoUsuario);
                ClienteCreado = resp.Models[0];

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar cliente: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
