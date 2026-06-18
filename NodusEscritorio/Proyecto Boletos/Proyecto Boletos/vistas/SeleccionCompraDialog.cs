using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Proyecto_Boletos.vistas
{
    public class SeleccionCompraDialog : Window
    {
        public bool Confirmado { get; private set; }
        public TipoBoletoCardVista TipoSeleccionado { get; private set; }
        public int Cantidad { get; private set; }

        private ComboBox _cmbTipo;
        private TextBox _txtCantidad;
        private TextBlock _txtDisponibles;
        private List<TipoBoletoCardVista> _tipos;

        public SeleccionCompraDialog(EventoCardVista card)
        {
            _tipos = card.TiposBoleto;

            Title = $"Comprar — {card.NombreEvento}";
            Width = 400;
            Height = 280;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titulo = new TextBlock
            {
                Text = card.NombreEvento,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16),
            };
            titulo.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
            Grid.SetRow(titulo, 0);
            root.Children.Add(titulo);

            var lblTipo = new TextBlock
            {
                Text = "Tipo de boleto",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4),
            };
            Grid.SetRow(lblTipo, 1);
            root.Children.Add(lblTipo);

            _cmbTipo = new ComboBox
            {
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 12),
            };
            foreach (var t in _tipos)
                _cmbTipo.Items.Add($"{t.NombreTipo}  —  Bs {t.Precio:F2}  ({t.Disponibles} disp.)");
            _cmbTipo.SelectedIndex = 0;
            _cmbTipo.SelectionChanged += (s, e) => ActualizarDisponibles();
            Grid.SetRow(_cmbTipo, 2);
            root.Children.Add(_cmbTipo);

            var rowCant = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            rowCant.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowCant.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var lblCant = new TextBlock
            {
                Text = "Cantidad",
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(lblCant, 0);
            rowCant.Children.Add(lblCant);

            _txtDisponibles = new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 160, 100)),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(_txtDisponibles, 1);
            rowCant.Children.Add(_txtDisponibles);
            Grid.SetRow(rowCant, 3);
            root.Children.Add(rowCant);

            _txtCantidad = new TextBox
            {
                Text = "1",
                Padding = new Thickness(6, 4, 6, 4),
                BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                BorderThickness = new Thickness(1),
            };
            Grid.SetRow(_txtCantidad, 4);
            root.Children.Add(_txtCantidad);

            var rowBotones = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0),
            };
            Grid.SetRow(rowBotones, 5);

            var btnCancelar = new Button
            {
                Content = "Cancelar",
                Width = 90,
                Height = 32,
                Margin = new Thickness(0, 0, 8, 0),
            };
            btnCancelar.SetResourceReference(Button.BackgroundProperty, "Color2");
            btnCancelar.Foreground = Brushes.White;
            btnCancelar.SetResourceReference(Button.BorderBrushProperty, "Color2");
            btnCancelar.Click += (s, e) => { Confirmado = false; Close(); };
            rowBotones.Children.Add(btnCancelar);

            var btnOk = new Button
            {
                Content = "Continuar",
                Width = 90,
                Height = 32,
            };
            btnOk.SetResourceReference(Button.BackgroundProperty, "Color1");
            btnOk.Foreground = Brushes.White;
            btnOk.SetResourceReference(Button.BorderBrushProperty, "Color1");
            btnOk.Click += BtnOk_Click;
            rowBotones.Children.Add(btnOk);

            root.Children.Add(rowBotones);
            Content = root;

            ActualizarDisponibles();
        }

        private void ActualizarDisponibles()
        {
            int idx = _cmbTipo.SelectedIndex;
            if (idx >= 0 && idx < _tipos.Count)
                _txtDisponibles.Text = $"{_tipos[idx].Disponibles} disponibles";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            int idx = _cmbTipo.SelectedIndex;
            if (idx < 0)
            {
                MessageBox.Show("Selecciona un tipo de boleto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(_txtCantidad.Text.Trim(), out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tipo = _tipos[idx];
            if (cantidad > tipo.Disponibles)
            {
                MessageBox.Show($"Solo hay {tipo.Disponibles} boletos disponibles para este tipo.", "Sin disponibilidad", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TipoSeleccionado = tipo;
            Cantidad = cantidad;
            Confirmado = true;
            Close();
        }
    }
}
