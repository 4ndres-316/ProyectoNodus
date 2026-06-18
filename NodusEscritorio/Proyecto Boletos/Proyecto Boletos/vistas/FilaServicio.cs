using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public class FilaServicio
    {
        public Border Panel { get; }
        public Action OnEliminar { get; set; }
        public int? IdServicioSeleccionado { get; private set; }
        public int Cantidad { get; private set; } = 1;
        public int PrecioAcordado { get; private set; }

        private List<Servicio> _servicios;
        private List<Proveedor> _proveedores;
        private List<ProveedorServicio> _proveedorServicios;
        private List<DetalleProveedorServicio> _detalles;

        private ComboBox _cmbCategoria;
        private ComboBox _cmbServicio;
        private ComboBox _cmbProveedor;
        private ComboBox _cmbItem;
        private TextBox _txtCantidad;
        private TextBox _txtPrecio;
        private TextBlock _txtInfoProveedor;

        public FilaServicio(List<Servicio> servicios, List<Proveedor> proveedores,
                            List<ProveedorServicio> proveedorServicios,
                            List<DetalleProveedorServicio> detalles = null)
        {
            _servicios = servicios;
            _proveedores = proveedores;
            _proveedorServicios = proveedorServicios;
            _detalles = detalles ?? new List<DetalleProveedorServicio>();

            // Agrupar categorías por NombreCategoria (ya cruzado en CargarDatos)
            var categorias = servicios
                .Select(s => s.NombreCategoria ?? "")
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct().OrderBy(c => c).ToList();

            _cmbCategoria = new ComboBox
            {
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(5),
                MinWidth = 140
            };
            _cmbCategoria.ItemsSource = categorias;
            _cmbCategoria.SelectionChanged += CmbCategoria_Changed;

            _cmbServicio = new ComboBox
            {
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(5),
                MinWidth = 150,
                DisplayMemberPath = "NombreServicio",
                SelectedValuePath = "IdServicio",
                IsEnabled = false
            };
            _cmbServicio.SelectionChanged += CmbServicio_Changed;

            _cmbProveedor = new ComboBox
            {
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(5),
                MinWidth = 150,
                DisplayMemberPath = "NombreComercial",
                SelectedValuePath = "NitProveedor",
                IsEnabled = false
            };
            _cmbProveedor.SelectionChanged += CmbProveedor_Changed;

            _cmbItem = new ComboBox
            {
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(5),
                MinWidth = 150,
                DisplayMemberPath = "NombreItem",
                IsEnabled = false
            };
            _cmbItem.SelectionChanged += CmbItem_Changed;

            _txtInfoProveedor = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
                MinWidth = 70
            };

            _txtCantidad = new TextBox
            {
                Text = "1",
                Width = 50,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 8, 0)
            };
            _txtCantidad.TextChanged += (s, e) =>
            {
                if (int.TryParse(_txtCantidad.Text, out int c)) Cantidad = c;
            };

            _txtPrecio = new TextBox
            {
                Width = 80,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 8, 0)
            };
            _txtPrecio.TextChanged += (s, e) =>
            {
                if (int.TryParse(_txtPrecio.Text, out int p)) PrecioAcordado = p;
            };

            var btnEliminar = new Button
            {
                Content = "✕",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Cursor = Cursors.Hand
            };
            btnEliminar.Click += (s, e) => OnEliminar?.Invoke();

            var fila = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 4)
            };

            void Lbl(string t) => fila.Children.Add(new TextBlock
            {
                Text = t,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
                FontSize = 12
            });

            Lbl("Categoría:"); fila.Children.Add(_cmbCategoria);
            Lbl("Servicio:");   fila.Children.Add(_cmbServicio);
            Lbl("Proveedor:");  fila.Children.Add(_cmbProveedor);
            Lbl("Ítem:");       fila.Children.Add(_cmbItem);
            fila.Children.Add(_txtInfoProveedor);
            Lbl("Cant:");       fila.Children.Add(_txtCantidad);
            Lbl("Precio Bs:");  fila.Children.Add(_txtPrecio);
            fila.Children.Add(btnEliminar);

            Panel = new Border
            {
                Child = fila,
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 4, 0, 4)
            };
        }

        private void CmbCategoria_Changed(object sender, SelectionChangedEventArgs e)
        {
            var categoria = _cmbCategoria.SelectedItem as string;
            if (categoria == null) return;

            _cmbServicio.ItemsSource = _servicios
                .Where(s => s.NombreCategoria == categoria).ToList();
            _cmbServicio.IsEnabled = true;
            _cmbServicio.SelectedIndex = -1;
            _cmbProveedor.ItemsSource = null;
            _cmbProveedor.IsEnabled = false;
            _cmbItem.ItemsSource = null;
            _cmbItem.IsEnabled = false;
            _txtInfoProveedor.Text = "";
            IdServicioSeleccionado = null;
        }

        private void CmbServicio_Changed(object sender, SelectionChangedEventArgs e)
        {
            var servicio = _cmbServicio.SelectedItem as Servicio;
            if (servicio == null) return;

            IdServicioSeleccionado = servicio.IdServicio;

            // Los NIT de proveedor son long — comparar con IdProveedor (long)
            var nitsProveedores = _proveedorServicios
                .Where(ps => ps.IdServicio == servicio.IdServicio)
                .Select(ps => ps.IdProveedor)   // long
                .ToList();

            var proveedoresFiltrados = _proveedores
                .Where(p => nitsProveedores.Contains(p.NitProveedor))  // long == long
                .ToList();

            _cmbProveedor.ItemsSource = proveedoresFiltrados;
            _cmbProveedor.IsEnabled = proveedoresFiltrados.Count > 0;
            _cmbProveedor.SelectedIndex = -1;
            _cmbItem.ItemsSource = null;
            _cmbItem.IsEnabled = false;
            _txtInfoProveedor.Text = proveedoresFiltrados.Count == 0 ? "Sin proveedor" : "";
        }

        private void CmbProveedor_Changed(object sender, SelectionChangedEventArgs e)
        {
            var proveedor = _cmbProveedor.SelectedItem as Proveedor;
            if (proveedor == null) return;

            var items = _detalles
                .Where(d => d.IdProveedor == proveedor.NitProveedor &&
                            d.IdServicio == IdServicioSeleccionado)
                .ToList();

            _cmbItem.ItemsSource = items;
            _cmbItem.IsEnabled = items.Count > 0;
            _cmbItem.SelectedIndex = -1;

            _txtPrecio.Text = "";
            PrecioAcordado = 0;
            _txtInfoProveedor.Text = items.Count == 0 ? "Sin ítems registrados" : "";
        }

        private void CmbItem_Changed(object sender, SelectionChangedEventArgs e)
        {
            var item = _cmbItem.SelectedItem as DetalleProveedorServicio;
            if (item == null) return;

            PrecioAcordado = (int)item.PrecioItem;
            _txtPrecio.Text = item.PrecioItem.ToString();
            _txtInfoProveedor.Text = $"Bs {item.PrecioItem}";
        }

        public bool EstaCompleta()
        {
            return IdServicioSeleccionado != null && Cantidad > 0;
        }
    }
}
