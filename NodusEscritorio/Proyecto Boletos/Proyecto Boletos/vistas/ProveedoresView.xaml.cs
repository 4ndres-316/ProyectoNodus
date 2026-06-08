using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class ProveedoresView : UserControl
    {
        private List<Proveedor> _proveedores;
        private List<Ciudad> _ciudades;
        private Proveedor _proveedorSeleccionado;
        private bool _modoEdicion = false;
        private int _idRol;

        public ProveedoresView(int idRol)
        {
            InitializeComponent();
            _idRol = idRol;

            // Solo el Administrador (rol 1) ve el botón Proveedores Web
            if (_idRol == 1)
                //btnVistaAdmin.Visibility = Visibility.Visible;

            btnNuevo.Click += btnNuevo_Click;
            btnGuardar.Click += btnGuardar_Click;
            /*btnEditar.Click += btnEditar_Click;
            btnEliminar.Click += btnEliminar_Click;*/
            btnCancelar.Click += btnCancelar_Click;
            dgProveedores.SelectionChanged += dgProveedores_SelectionChanged;

            Loaded += async (s, e) =>
            {
                await CargarCiudades();
                await CargarProveedores();
            };
        }

        // Solo para el diseñador de Visual Studio
        public ProveedoresView() : this(0) { }

        // ---------------------------------------------------------------
        // Carga de datos
        // ---------------------------------------------------------------
        private async Task CargarCiudades()
        {
            try
            {
                var response = await ConexionDB.Client.From<Ciudad>().Get();
                _ciudades = response.Models;
                cmbCiudad.ItemsSource = _ciudades;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ciudades: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarProveedores()
        {
            try
            {
                var response = await ConexionDB.Client.From<Proveedor>().Get();
                _proveedores = response.Models;
                dgProveedores.ItemsSource = _proveedores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------------------------------------------------------
        // Botón Vista General
        // ---------------------------------------------------------------
        private void btnVistaCompartida_Click(object sender, RoutedEventArgs e)
        {
            //panelGeneral.Visibility = Visibility.Visible;
        }

        // ---------------------------------------------------------------
        // Botón Proveedores Web — abre nueva ventana
        // ---------------------------------------------------------------
        private void btnVistaAdmin_Click(object sender, RoutedEventArgs e)
        {
            var ventanaWeb = new ProveedoresWebView();
            ventanaWeb.ShowDialog();
        }

        // ---------------------------------------------------------------
        // Selección en DataGrid
        // ---------------------------------------------------------------
        private void dgProveedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProveedores.SelectedItem != null)
            {
                _proveedorSeleccionado = dgProveedores.SelectedItem as Proveedor;
            }
            else
            {
                _proveedorSeleccionado = null;
            }
        }

        // ---------------------------------------------------------------
        // CRUD
        // ---------------------------------------------------------------
        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            HabilitarFormulario(true);
            _modoEdicion = false;

            btnGuardar.IsEnabled = true;
            btnCancelar.IsEnabled = true;
            btnNuevo.IsEnabled = false;
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNitProveedor.Text) ||
                string.IsNullOrWhiteSpace(txtNombreComercial.Text))
            {
                MessageBox.Show("El NIT y el nombre comercial son obligatorios.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCiudad.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una ciudad.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idCiudad = (int)cmbCiudad.SelectedValue;

            try
            {
                if (_modoEdicion)
                {
                    _proveedorSeleccionado.NombreComercial = txtNombreComercial.Text.Trim();
                    _proveedorSeleccionado.TelefonoComercial = txtTelefonoComercial.Text.Trim();
                    _proveedorSeleccionado.IdCiudad = idCiudad;
                    _proveedorSeleccionado.EstadoProveedor = ((ComboBoxItem)cmbEstadoProveedor.SelectedItem).Content.ToString();

                    await ConexionDB.Client.From<Proveedor>().Update(_proveedorSeleccionado);
                }
                else
                {
                    var nuevo = new Proveedor
                    {
                        NitProveedor = txtNitProveedor.Text.Trim(),
                        NombreComercial = txtNombreComercial.Text.Trim(),
                        TelefonoComercial = txtTelefonoComercial.Text.Trim(),
                        IdCiudad = idCiudad,
                        EstadoProveedor = ((ComboBoxItem)cmbEstadoProveedor.SelectedItem).Content.ToString()
                    };

                    await ConexionDB.Client.From<Proveedor>().Insert(nuevo);
                }

                await CargarProveedores();
                LimpiarFormulario();
                HabilitarFormulario(false);

                btnGuardar.IsEnabled = false;
                btnCancelar.IsEnabled = false;
                btnNuevo.IsEnabled = true;
                dgProveedores.IsEnabled = true;

                MessageBox.Show("Proveedor guardado exitosamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar proveedor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (_proveedorSeleccionado == null) return;

            txtNitProveedor.Text = _proveedorSeleccionado.NitProveedor;
            txtNombreComercial.Text = _proveedorSeleccionado.NombreComercial;
            txtTelefonoComercial.Text = _proveedorSeleccionado.TelefonoComercial;

            // Seleccionar la ciudad correcta en el ComboBox
            cmbCiudad.SelectedValue = _proveedorSeleccionado.IdCiudad;

            foreach (ComboBoxItem item in cmbEstadoProveedor.Items)
            {
                if (item.Content.ToString() == _proveedorSeleccionado.EstadoProveedor)
                {
                    cmbEstadoProveedor.SelectedItem = item;
                    break;
                }
            }

            HabilitarFormulario(true);
            _modoEdicion = true;

            btnGuardar.IsEnabled = true;
            btnCancelar.IsEnabled = true;
            btnNuevo.IsEnabled = false;
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_proveedorSeleccionado == null) return;

            var resultado = MessageBox.Show(
                $"¿Estás seguro de eliminar el proveedor '{_proveedorSeleccionado.NombreComercial}'?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    await ConexionDB.Client.From<Proveedor>()
                        .Where(x => x.NitProveedor == _proveedorSeleccionado.NitProveedor)
                        .Delete();

                    await CargarProveedores();
                    LimpiarFormulario();

                    MessageBox.Show("Proveedor eliminado.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
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
            dgProveedores.SelectedItem = null;
            _modoEdicion = false;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        private void LimpiarFormulario()
        {
            txtNitProveedor.Clear();
            txtNombreComercial.Clear();
            txtTelefonoComercial.Clear();
            cmbCiudad.SelectedIndex = -1;
            cmbEstadoProveedor.SelectedIndex = 0;
        }

        private void HabilitarFormulario(bool habilitar)
        {
            campos.IsEnabled = habilitar;
            dgProveedores.IsEnabled = !habilitar;
        }
    }
}