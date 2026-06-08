using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class Configuracion : UserControl
    {
        private List<Usuario> _usuarios;
        private List<Rol> _roles;
        private Usuario _usuarioSeleccionado;
        private bool _modoEdicion = false;

        public Configuracion()
        {
            InitializeComponent();
            Loaded += Configuracion_Loaded;
        }

        private async void Configuracion_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarUsuarios();
        }

        private async Task CargarUsuarios()
        {
            try
            {
                var responseRoles = await ConexionDB.Client.From<Rol>().Get();
                _roles = responseRoles.Models;

                // Cargar roles en el ComboBox
                cmbRol.ItemsSource = _roles;
                cmbRol.DisplayMemberPath = "Nombre";  // Muestra el nombre
                cmbRol.SelectedValuePath = "IdRol";   // Valor interno es el ID

                var responseUsuarios = await ConexionDB.Client.From<Usuario>().Get();
                _usuarios = responseUsuarios.Models;

                foreach (var usuario in _usuarios)
                {
                    usuario.Rol = _roles.FirstOrDefault(r => r.IdRol == usuario.IdRol);
                }

                dgUsuarios.ItemsSource = _usuarios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem != null)
            {
                _usuarioSeleccionado = dgUsuarios.SelectedItem as Usuario;
            }
            else
            {
                _usuarioSeleccionado = null;
            }
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            HabilitarFormulario(true);
            _modoEdicion = false;

            // Deshabilitar
            btnNuevo.IsEnabled = false;

            // Habilitar
            btnGuardar.IsEnabled = true;
            btnCancelar.IsEnabled = true;
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                if (_modoEdicion)
                {
                    await ActualizarUsuario();
                }
                else
                {
                    await CrearUsuario();
                }

                await CargarUsuarios();
                LimpiarFormulario();
                HabilitarFormulario(false);
                btnGuardar.IsEnabled = false;
                btnCancelar.IsEnabled = false;
                btnNuevo.IsEnabled = true;
                dgUsuarios.IsEnabled = true;

                // Ocultar controles de estado después de guardar
                cmbEstado.Visibility = Visibility.Collapsed;
                lblEstado.Visibility = Visibility.Collapsed;

                MessageBox.Show("Usuario guardado exitosamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar usuario: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CrearUsuario()
        {
            var nuevoUsuario = new Usuario
            {
                NombreUsuario = txtNombreUsuario.Text.Trim(),
                EmailUsuario = txtEmail.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                IdRol = (int)cmbRol.SelectedValue,
                EstadoUsuario = "Activo"
            };

            await ConexionDB.Client.From<Usuario>().Insert(nuevoUsuario);
        }

        private async Task ActualizarUsuario()
        {
            _usuarioSeleccionado.NombreUsuario = txtNombreUsuario.Text.Trim();
            _usuarioSeleccionado.EmailUsuario = txtEmail.Text.Trim(); 
            _usuarioSeleccionado.Telefono = txtTelefono.Text.Trim();
            _usuarioSeleccionado.Username = txtUsername.Text.Trim();

            if (!string.IsNullOrEmpty(txtPassword.Text.Trim()))
            {
                _usuarioSeleccionado.Password = txtPassword.Text.Trim();
            }

            _usuarioSeleccionado.IdRol = (int)cmbRol.SelectedValue;

            // Guardar el estado si el usuario era inactivo
            if (_usuarioSeleccionado.EstadoUsuario == "Inactivo" && cmbEstado.Visibility == Visibility.Visible)
            {
                _usuarioSeleccionado.EstadoUsuario = ((ComboBoxItem)cmbEstado.SelectedItem).Content.ToString();
            }

            await ConexionDB.Client.From<Usuario>().Update(_usuarioSeleccionado);
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;

            CargarDatosFormulario(_usuarioSeleccionado);
            HabilitarFormulario(true);
            _modoEdicion = true;

            // Mostrar u ocultar cmbEstado y lblEstado según el estado del usuario
            if (_usuarioSeleccionado.EstadoUsuario == "Inactivo")
            {
                cmbEstado.Visibility = Visibility.Visible;
                lblEstado.Visibility = Visibility.Visible;
            }
            else
            {
                cmbEstado.Visibility = Visibility.Collapsed;
                lblEstado.Visibility = Visibility.Collapsed;
            }

            // Deshabilitar
            btnNuevo.IsEnabled = false;

            // Habilitar
            btnGuardar.IsEnabled = true;
            btnCancelar.IsEnabled = true;
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioSeleccionado == null) return;

            var resultado = MessageBox.Show(
                $"¿Estás seguro de deshabilitar al usuario '{_usuarioSeleccionado.NombreUsuario}'?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _usuarioSeleccionado.EstadoUsuario = "Inactivo";
                    await ConexionDB.Client
                        .From<Usuario>()
                        .Update(_usuarioSeleccionado);

                    await CargarUsuarios();
                    LimpiarFormulario();
                    MessageBox.Show("Usuario deshabilitado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            HabilitarFormulario(false);
            btnGuardar.IsEnabled = false;
            btnCancelar.IsEnabled = false;
            btnNuevo.IsEnabled = true;
            dgUsuarios.SelectedItem = null;
            _modoEdicion = false;
            
            // Ocultar controles de estado
            cmbEstado.Visibility = Visibility.Collapsed;
            lblEstado.Visibility = Visibility.Collapsed;
        }

        private void LimpiarFormulario()
        {
            txtNombreUsuario.Clear();
            txtEmail.Clear();
            txtTelefono.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            cmbRol.SelectedIndex = 2;
        }

        private void HabilitarFormulario(bool habilitar)
        {
            campos.IsEnabled = habilitar;
            dgUsuarios.IsEnabled = !habilitar;
        }

        private void CargarDatosFormulario(Usuario usuario)
        {
            txtNombreUsuario.Text = usuario.NombreUsuario;
            txtEmail.Text = usuario.EmailUsuario;
            txtTelefono.Text = usuario.Telefono;
            txtUsername.Text = usuario.Username;
            txtPassword.Text = usuario.Password;

            // Selecciona el rol correspondiente
            cmbRol.SelectedValue = usuario.IdRol;
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("El usuario es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!_modoEdicion && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("La contraseña es obligatoria", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = txtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(texto))
            {
                dgUsuarios.ItemsSource = _usuarios;
                return;
            }

            var filtrado = _usuarios.Where(u =>
                u.NombreUsuario.ToLower().Contains(texto) ||
                u.EmailUsuario.ToLower().Contains(texto) ||
                u.Username.ToLower().Contains(texto) ||
                (u.Rol?.Nombre.ToLower().Contains(texto) ?? false)
            ).ToList();

            dgUsuarios.ItemsSource = filtrado;
        }
    }
}