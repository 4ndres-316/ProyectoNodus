using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<Servicio> _servicios;
        private List<ProveedorServicio> _proveedorServicios;
        private Proveedor _proveedorSeleccionado;
        private bool _modoEdicion = false;
        private int _idRol;

        public ProveedoresView(int idRol)
        {
            InitializeComponent();
            _idRol = idRol;

            Loaded += async (s, e) =>
            {
                await CargarCiudades();
                await CargarServicios();
                await CargarProveedores();
            };

            txtBuscar.TextChanged += txtBuscar_TextChanged;
        }

        public ProveedoresView()
            : this(0) { }

        // ─── CARGA ───────────────────────────────────────────────────────────

        private async Task CargarCiudades()
        {
            try
            {
                _ciudades = (await ConexionDB.Client.From<Ciudad>().Get()).Models;
                cmbCiudad.ItemsSource = _ciudades;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar ciudades: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task CargarServicios()
        {
            try
            {
                _servicios = (await ConexionDB.Client.From<Servicio>().Get()).Models;

                cmbServicio.ItemsSource = _servicios;
                cmbServicio.DisplayMemberPath = "NombreServicio";
                cmbServicio.SelectedValuePath = "IdServicio";

                _proveedorServicios = (
                    await ConexionDB.Client.From<ProveedorServicio>().Get()
                ).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar servicios: {ex.Message}");
            }
        }

        private async Task CargarProveedores()
        {
            try
            {
                _proveedores = (await ConexionDB.Client.From<Proveedor>().Get()).Models;

                foreach (var p in _proveedores)
                {
                    // Nombre ciudad
                    var ciudad = _ciudades?.Find(c => c.IdCiudad == p.IdCiudad);
                    p.NombreCiudad = ciudad?.NombreCiudad ?? string.Empty;

                    // Servicios con precio desde proveedor_servicio
                    if (_proveedorServicios != null && _servicios != null)
                    {
                        var psDelProveedor = _proveedorServicios
                            .Where(ps => ps.IdProveedor == p.NitProveedor)
                            .ToList();

                        var partes = psDelProveedor
                            .Select(ps =>
                            {
                                var servicio = _servicios.Find(s => s.IdServicio == ps.IdServicio);
                                return $"{servicio?.NombreServicio ?? ps.IdServicio.ToString()} — Bs {ps.Precio}";
                            })
                            .ToList();

                        p.ServiciosConPrecio =
                            partes.Count > 0 ? string.Join(" | ", partes) : "Sin servicios";
                    }
                }

                dgProveedores.ItemsSource = _proveedores;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar proveedores: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ─── BUSCADOR ────────────────────────────────────────────────────────

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_proveedores == null)
                return;

            var texto = (txtBuscar.Text ?? "").Trim().ToLower();

            if (string.IsNullOrEmpty(texto))
            {
                dgProveedores.ItemsSource = _proveedores;
                return;
            }

            dgProveedores.ItemsSource = _proveedores
                .Where(p =>
                    (p.NombreComercial ?? "").ToLower().Contains(texto)
                    || (p.TelefonoComercial ?? "").ToLower().Contains(texto)
                    || (p.NombreCiudad ?? "").ToLower().Contains(texto)
                    || (p.EstadoProveedor ?? "").ToLower().Contains(texto)
                    || (p.ServiciosConPrecio ?? "").ToLower().Contains(texto)
                    || p.NitProveedor.ToString().Contains(texto)
                )
                .ToList();
        }

        // ─── SELECCIÓN ───────────────────────────────────────────────────────

        private void dgProveedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _proveedorSeleccionado = dgProveedores.SelectedItem as Proveedor;
        }

        // ─── CRUD ─────────────────────────────────────────────────────────────

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
            if (string.IsNullOrWhiteSpace(txtNombreComercial.Text))
            {
                MessageBox.Show(
                    "El nombre comercial es obligatorio.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTelefonoComercial.Text))
            {
                MessageBox.Show(
                    "El teléfono comercial es obligatorio.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (cmbCiudad.SelectedValue == null)
            {
                MessageBox.Show(
                    "Debe seleccionar una ciudad.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (cmbEstadoProveedor.SelectedItem == null)
            {
                MessageBox.Show(
                    "Debe seleccionar un estado.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            long nit = 0;
            if (!_modoEdicion)
            {
                if (string.IsNullOrWhiteSpace(txtNitProveedor.Text))
                {
                    MessageBox.Show(
                        "El NIT es obligatorio.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (!long.TryParse(txtNitProveedor.Text.Trim(), out nit))
                {
                    MessageBox.Show(
                        "El NIT debe ser un número válido.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
            }

            int? idCiudad =
                cmbCiudad.SelectedValue != null
                    ? (int?)Convert.ToInt32(cmbCiudad.SelectedValue)
                    : null;
            try
            {
                if (_modoEdicion)
                {
                    _proveedorSeleccionado.NombreComercial = txtNombreComercial.Text.Trim();
                    _proveedorSeleccionado.TelefonoComercial = txtTelefonoComercial.Text.Trim();
                    _proveedorSeleccionado.IdCiudad = idCiudad;
                    _proveedorSeleccionado.EstadoProveedor = (
                        (ComboBoxItem)cmbEstadoProveedor.SelectedItem
                    ).Content.ToString();

                    await ConexionDB.Client.From<Proveedor>().Update(_proveedorSeleccionado);

                    var relacion = _proveedorServicios.FirstOrDefault(x =>
                        x.IdProveedor == _proveedorSeleccionado.NitProveedor
                    );

                    if (relacion != null)
                    {
                        relacion.IdServicio = (int)(long)cmbServicio.SelectedValue;

                        long precio = 0;

                        long.TryParse(txtPrecioProveedor.Text, out precio);

                        relacion.Precio = precio;

                        await ConexionDB.Client.From<ProveedorServicio>().Update(relacion);
                    }
                }
                else
                {
                    var nuevo = new Proveedor
                    {
                        NitProveedor = nit,
                        NombreComercial = txtNombreComercial.Text.Trim(),
                        TelefonoComercial = txtTelefonoComercial.Text.Trim(),
                        IdCiudad = idCiudad,
                        EstadoProveedor = (
                            (ComboBoxItem)cmbEstadoProveedor.SelectedItem
                        ).Content.ToString(),
                    };

                    await ConexionDB.Client.From<Proveedor>().Insert(nuevo);

                    if (cmbServicio.SelectedValue != null)
                    {
                        long precio = 0;

                        long.TryParse(txtPrecioProveedor.Text, out precio);

                        var proveedorServicio = new ProveedorServicio
                        {
                            IdProveedor = nit,
                            IdServicio = (int)(long)cmbServicio.SelectedValue,
                            Precio = precio,
                        };

                        await ConexionDB.Client.From<ProveedorServicio>().Insert(proveedorServicio);
                    }
                }

                await CargarProveedores();
                LimpiarFormulario();
                HabilitarFormulario(false);
                btnGuardar.IsEnabled = false;
                btnCancelar.IsEnabled = false;
                btnNuevo.IsEnabled = true;

                MessageBox.Show(
                    "Proveedor guardado exitosamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar proveedor: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Proveedor p)
                _proveedorSeleccionado = p;

            if (_proveedorSeleccionado == null)
                return;

            txtNitProveedor.Text = _proveedorSeleccionado.NitProveedor.ToString();
            txtNombreComercial.Text = _proveedorSeleccionado.NombreComercial;
            txtTelefonoComercial.Text = _proveedorSeleccionado.TelefonoComercial;
            cmbCiudad.SelectedValue = _proveedorSeleccionado.IdCiudad;

            var relacion = _proveedorServicios?.FirstOrDefault(x =>
                x.IdProveedor == _proveedorSeleccionado.NitProveedor
            );

            if (relacion != null)
            {
                cmbServicio.SelectedValue = relacion.IdServicio;

                txtPrecioProveedor.Text = relacion.Precio.ToString();
            }

            foreach (ComboBoxItem item in cmbEstadoProveedor.Items)
                if (item.Content.ToString() == _proveedorSeleccionado.EstadoProveedor)
                {
                    cmbEstadoProveedor.SelectedItem = item;
                    break;
                }

            HabilitarFormulario(true);
            _modoEdicion = true;
            btnGuardar.IsEnabled = true;
            btnCancelar.IsEnabled = true;
            btnNuevo.IsEnabled = false;
        }

        private async void btnDeshabilitar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Proveedor p)
                _proveedorSeleccionado = p;

            if (_proveedorSeleccionado == null)
                return;

            if (_proveedorSeleccionado.EstadoProveedor == "Inactivo")
            {
                MessageBox.Show(
                    "El proveedor ya está inactivo.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Deshabilitar a '{_proveedorSeleccionado.NombreComercial}'? Su estado cambiará a Inactivo.",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                _proveedorSeleccionado.EstadoProveedor = "Inactivo";
                await ConexionDB.Client.From<Proveedor>().Update(_proveedorSeleccionado);
                await CargarProveedores();

                MessageBox.Show(
                    "Proveedor deshabilitado.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
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

        // ─── HELPERS ─────────────────────────────────────────────────────────

        private void LimpiarFormulario()
        {
            txtNitProveedor.Clear();
            txtNombreComercial.Clear();
            txtTelefonoComercial.Clear();
            txtPrecioProveedor.Clear();

            cmbCiudad.SelectedIndex = -1;
            cmbServicio.SelectedIndex = -1;
            cmbEstadoProveedor.SelectedIndex = 0;
        }

        private void HabilitarFormulario(bool habilitar)
        {
            campos.IsEnabled = habilitar;
            dgProveedores.IsEnabled = !habilitar;
        }
    }
}
