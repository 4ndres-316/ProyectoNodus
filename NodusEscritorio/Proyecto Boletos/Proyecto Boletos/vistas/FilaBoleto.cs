using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Proyecto_Boletos.vistas
{
    public class FilaBoleto
    {
        public Border Panel { get; }
        public Action OnEliminar { get; set; }
        public string NombreTipo { get; private set; } = string.Empty;
        public decimal Precio { get; private set; }
        public int CantidadTotal { get; private set; }
        public string Descripcion { get; private set; } = string.Empty;
        public string ImagenUrl { get; private set; } = string.Empty;

        public FilaBoleto()
        {
            var txtNombre   = new TextBox { Width = 130, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtPrecio   = new TextBox { Width = 80,  Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtCantidad = new TextBox { Width = 70,  Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtDesc     = new TextBox { Width = 150, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtImagen   = new TextBox { Width = 120, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };

            txtNombre.TextChanged   += (s, e) => NombreTipo = txtNombre.Text;
            txtPrecio.TextChanged   += (s, e) => { if (decimal.TryParse(txtPrecio.Text, out decimal p)) Precio = p; };
            txtCantidad.TextChanged += (s, e) => { if (int.TryParse(txtCantidad.Text, out int c)) CantidadTotal = c; };
            txtDesc.TextChanged     += (s, e) => Descripcion = txtDesc.Text;
            txtImagen.TextChanged   += (s, e) => ImagenUrl = txtImagen.Text;

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

            Lbl("Nombre:");       fila.Children.Add(txtNombre);
            Lbl("Precio Bs:");    fila.Children.Add(txtPrecio);
            Lbl("Cantidad:");     fila.Children.Add(txtCantidad);
            Lbl("Descripción:");  fila.Children.Add(txtDesc);
            Lbl("Imagen URL:");   fila.Children.Add(txtImagen);
            fila.Children.Add(btnEliminar);

            Panel = new Border
            {
                Child = fila,
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 4, 0, 4)
            };
        }

        public bool EstaCompleta()
        {
            return !string.IsNullOrWhiteSpace(NombreTipo) && Precio > 0 && CantidadTotal > 0;
        }
    }
}
