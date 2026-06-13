using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class RecintosView : UserControl
    {
        private List<Recinto> _recintos;
        private List<Ciudad> _ciudades;
        private List<FechaEvento> _fechasEventos;
        private List<Evento> _eventos;
        private Recinto _recintoSeleccionado;
        private bool _modoEdicion = false;

        public RecintosView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarCiudades();
            await CargarRecintos();
            await CargarEventosYFechas();
        }

        private async Task CargarCiudades()
        {
            try
            {
                var response = await ConexionDB.Client.From<Ciudad>().Get();
                _ciudades = response.Models;
                cmbIdCiudad.ItemsSource = _ciudades;
                cmbIdCiudad.DisplayMemberPath = "NombreCiudad";
                cmbIdCiudad.SelectedValuePath = "IdCiudad";
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

        private async Task CargarRecintos()
        {
            try
            {
                var response = await ConexionDB.Client.From<Recinto>().Get();
                _recintos = response.Models;

                // Cruzar nombre de ciudad
                foreach (var r in _recintos)
                {
                    var ciudad = _ciudades?.Find(c => c.IdCiudad == r.IdCiudad);
                    r.NombreCiudad = ciudad?.NombreCiudad ?? string.Empty;
                }

                dgRecintos.ItemsSource = _recintos;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar recintos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task CargarEventosYFechas()
        {
            try
            {
                _eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;
                _fechasEventos = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar eventos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ─── BUSCADOR SIMPLE ──────────────────────────────────────────────────

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_recintos == null)
                return;

            var texto = (txtBuscar.Text ?? "").Trim().ToLower();

            if (string.IsNullOrEmpty(texto))
            {
                dgRecintos.ItemsSource = _recintos;
                return;
            }

            dgRecintos.ItemsSource = _recintos
                .Where(r =>
                    (r.NombreRecinto ?? "").ToLower().Contains(texto)
                    || (r.DireccionRecinto ?? "").ToLower().Contains(texto)
                    || (r.TipoRecinto ?? "").ToLower().Contains(texto)
                    || (r.EstadoRecinto ?? "").ToLower().Contains(texto)
                    || (r.NombreCiudad ?? "").ToLower().Contains(texto)
                    || r.Capacidad.ToString().Contains(texto)
                )
                .ToList();
        }

        // ─── DATAGRID ─────────────────────────────────────────────────────────

        private void dgRecintos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _recintoSeleccionado = dgRecintos.SelectedItem as Recinto;
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
            if (
                string.IsNullOrWhiteSpace(txtNombreRecinto.Text)
                || string.IsNullOrWhiteSpace(txtDireccion.Text)
            )
            {
                MessageBox.Show(
                    "El nombre y la dirección son obligatorios.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!int.TryParse(txtCapacidad.Text, out int capacidadValida))
            {
                MessageBox.Show(
                    "La capacidad debe ser un número entero válido.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            int? idCiudad =
                cmbIdCiudad.SelectedValue != null ? (int?)cmbIdCiudad.SelectedValue : null;

            try
            {
                if (_modoEdicion)
                {
                    _recintoSeleccionado.NombreRecinto = txtNombreRecinto.Text.Trim();
                    _recintoSeleccionado.DireccionRecinto = txtDireccion.Text.Trim();
                    _recintoSeleccionado.IdCiudad = idCiudad;
                    _recintoSeleccionado.Capacidad = capacidadValida;
                    _recintoSeleccionado.DescripcionRecinto = txtDescripcion.Text.Trim();
                    _recintoSeleccionado.TipoRecinto = (
                        (ComboBoxItem)cmbTipoRecinto.SelectedItem
                    ).Content.ToString();
                    _recintoSeleccionado.EstadoRecinto = (
                        (ComboBoxItem)cmbEstadoRecinto.SelectedItem
                    ).Content.ToString();

                    await ConexionDB.Client.From<Recinto>().Update(_recintoSeleccionado);
                }
                else
                {
                    var nuevoRecinto = new Recinto
                    {
                        NombreRecinto = txtNombreRecinto.Text.Trim(),
                        DireccionRecinto = txtDireccion.Text.Trim(),
                        IdCiudad = idCiudad,
                        Capacidad = capacidadValida,
                        DescripcionRecinto = txtDescripcion.Text.Trim(),
                        TipoRecinto = (
                            (ComboBoxItem)cmbTipoRecinto.SelectedItem
                        ).Content.ToString(),
                        EstadoRecinto = (
                            (ComboBoxItem)cmbEstadoRecinto.SelectedItem
                        ).Content.ToString(),
                    };

                    await ConexionDB.Client.From<Recinto>().Insert(nuevoRecinto);
                }

                await CargarRecintos();
                LimpiarFormulario();
                HabilitarFormulario(false);
                btnGuardar.IsEnabled = false;
                btnCancelar.IsEnabled = false;
                btnNuevo.IsEnabled = true;

                MessageBox.Show(
                    "Recinto guardado exitosamente",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar recinto: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Recinto recinto)
                _recintoSeleccionado = recinto;

            if (_recintoSeleccionado == null)
                return;

            txtNombreRecinto.Text = _recintoSeleccionado.NombreRecinto;
            txtDireccion.Text = _recintoSeleccionado.DireccionRecinto;
            txtCapacidad.Text = _recintoSeleccionado.Capacidad.ToString();
            txtDescripcion.Text = _recintoSeleccionado.DescripcionRecinto;
            cmbIdCiudad.SelectedValue = _recintoSeleccionado.IdCiudad;

            foreach (ComboBoxItem item in cmbTipoRecinto.Items)
                if (item.Content.ToString() == _recintoSeleccionado.TipoRecinto)
                {
                    cmbTipoRecinto.SelectedItem = item;
                    break;
                }

            foreach (ComboBoxItem item in cmbEstadoRecinto.Items)
                if (item.Content.ToString() == _recintoSeleccionado.EstadoRecinto)
                {
                    cmbEstadoRecinto.SelectedItem = item;
                    break;
                }

            HabilitarFormulario(true);
            _modoEdicion = true;
            btnGuardar.IsEnabled = true;
            btnCancelar.IsEnabled = true;
            btnNuevo.IsEnabled = false;
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Recinto recinto)
                _recintoSeleccionado = recinto;

            if (_recintoSeleccionado == null)
                return;

            var resultado = MessageBox.Show(
                $"¿Estás seguro de eliminar '{_recintoSeleccionado.NombreRecinto}'?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    await ConexionDB
                        .Client.From<Recinto>()
                        .Where(x => x.IdRecinto == _recintoSeleccionado.IdRecinto)
                        .Delete();

                    await CargarRecintos();
                    LimpiarFormulario();

                    MessageBox.Show(
                        "Recinto eliminado",
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
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            HabilitarFormulario(false);
            btnGuardar.IsEnabled = false;
            btnCancelar.IsEnabled = false;
            btnNuevo.IsEnabled = true;
            dgRecintos.SelectedItem = null;
            _modoEdicion = false;
        }

        private void LimpiarFormulario()
        {
            txtNombreRecinto.Clear();
            txtDireccion.Clear();
            txtCapacidad.Clear();
            txtDescripcion.Clear();
            cmbIdCiudad.SelectedIndex = -1;
            cmbTipoRecinto.SelectedIndex = 0;
            cmbEstadoRecinto.SelectedIndex = 0;
        }

        private void HabilitarFormulario(bool habilitar)
        {
            campos.IsEnabled = habilitar;
            dgRecintos.IsEnabled = !habilitar;
        }
    }
}
