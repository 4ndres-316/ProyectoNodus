using System;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Proyecto_Boletos.vistas
{
    public class FacturaDialog : Window
    {
        private FacturaData _data;
        private Border _pnlFactura;

        public FacturaDialog(FacturaData data)
        {
            _data = data;
            Title = $"Factura #{data.NumeroFactura}";
            Width = 560;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SetResourceReference(Window.BackgroundProperty, "Color3");

            var rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Content = rootGrid;

            // ── Área scrolleable de la factura ──
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20),
            };
            Grid.SetRow(scroll, 0);
            rootGrid.Children.Add(scroll);

            _pnlFactura = ConstruirFactura();
            scroll.Content = _pnlFactura;

            // ── Footer con botones ──
            var footer = new Border
            {
                BorderThickness = new Thickness(0, 1, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                Padding = new Thickness(20, 12, 20, 12),
            };
            Grid.SetRow(footer, 1);
            rootGrid.Children.Add(footer);

            var rowBotones = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            footer.Child = rowBotones;

            var btnCerrar = new Button
            {
                Content = "Cerrar",
                Width = 90,
                Height = 34,
                Margin = new Thickness(0, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
            };
            btnCerrar.SetResourceReference(Button.BackgroundProperty, "Color2");
            btnCerrar.Foreground = Brushes.White;
            btnCerrar.SetResourceReference(Button.BorderBrushProperty, "Color2");
            btnCerrar.Click += (s, e) => Close();
            rowBotones.Children.Add(btnCerrar);

            var btnExportar = new Button
            {
                Content = "🖨 Exportar / Imprimir",
                Height = 34,
                Padding = new Thickness(16, 0, 16, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
            };
            btnExportar.SetResourceReference(Button.BackgroundProperty, "Color1");
            btnExportar.Foreground = Brushes.White;
            btnExportar.SetResourceReference(Button.BorderBrushProperty, "Color1");
            btnExportar.Click += BtnExportar_Click;
            rowBotones.Children.Add(btnExportar);
        }

        private Border ConstruirFactura()
        {
            var root = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(28),
            };

            var stack = new StackPanel();
            root.Child = stack;

            // ── Encabezado ──
            var header = new Grid { Margin = new Thickness(0, 0, 0, 20) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var colIzq = new StackPanel();
            Grid.SetColumn(colIzq, 0);

            var lblEmpresa = new TextBlock
            {
                Text = "NODUS",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
            };
            lblEmpresa.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
            colIzq.Children.Add(lblEmpresa);

            colIzq.Children.Add(new TextBlock
            {
                Text = "Sistema de Boletos",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
            });
            header.Children.Add(colIzq);

            var colDer = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(colDer, 1);

            var lblFacturaTitulo = new TextBlock
            {
                Text = "FACTURA",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            lblFacturaTitulo.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
            colDer.Children.Add(lblFacturaTitulo);

            colDer.Children.Add(new TextBlock
            {
                Text = $"N° {_data.NumeroFactura:D6}",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
            });
            colDer.Children.Add(new TextBlock
            {
                Text = $"Fecha: {_data.FechaCompra}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                HorizontalAlignment = HorizontalAlignment.Right,
            });
            header.Children.Add(colDer);
            stack.Children.Add(header);

            stack.Children.Add(Separador());

            // ── Datos del comprador ──
            stack.Children.Add(SeccionTitulo("DATOS DEL COMPRADOR"));

            var gridComprador = new Grid { Margin = new Thickness(0, 8, 0, 0) };
            gridComprador.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            gridComprador.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridComprador.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridComprador.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridComprador.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AgregarFilaComprador(gridComprador, 0, "Nombre:", string.IsNullOrWhiteSpace(_data.CompradorNombre) ? "—" : _data.CompradorNombre);
            AgregarFilaComprador(gridComprador, 1, "NIT/CI:", string.IsNullOrWhiteSpace(_data.CompradorNit) ? "—" : _data.CompradorNit);
            AgregarFilaComprador(gridComprador, 2, "Correo:", string.IsNullOrWhiteSpace(_data.CompradorCorreo) ? "—" : _data.CompradorCorreo);
            stack.Children.Add(gridComprador);

            stack.Children.Add(Separador());

            // ── Detalle de compra ──
            stack.Children.Add(SeccionTitulo("DETALLE DE COMPRA"));
            stack.Children.Add(TablaDetalle());

            stack.Children.Add(Separador());

            // ── Totales ──
            stack.Children.Add(SeccionTotales());

            stack.Children.Add(Separador());

            // ── Pago ──
            stack.Children.Add(SeccionTitulo("INFORMACIÓN DE PAGO"));
            stack.Children.Add(FilaResumen("Método de pago:", _data.MetodoPago));
            stack.Children.Add(FilaResumen("Estado:", _data.EstadoPago.ToUpper()));

            stack.Children.Add(Separador());

            // ── Footer ──
            stack.Children.Add(new TextBlock
            {
                Text = "Gracias por tu compra — Nodus Sistema de Boletos",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0),
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"© {DateTime.Now.Year} Nodus — Todos los derechos reservados",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0),
            });

            return root;
        }

        private Border TablaDetalle()
        {
            var tabla = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 8, 0, 0),
                ClipToBounds = true,
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Pixel) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            tabla.Child = grid;

            // Encabezado tabla
            var headerBg = new Border
            {
                CornerRadius = new CornerRadius(3, 3, 0, 0),
            };
            headerBg.SetResourceReference(Border.BackgroundProperty, "Color1");
            Grid.SetRow(headerBg, 0);
            Grid.SetColumnSpan(headerBg, 4);
            grid.Children.Add(headerBg);

            AgregarCeldaTabla(grid, "Concepto", 0, 0, true);
            AgregarCeldaTabla(grid, "Cant.", 0, 1, true, HorizontalAlignment.Center);
            AgregarCeldaTabla(grid, "P. Unit.", 0, 2, true, HorizontalAlignment.Right);
            AgregarCeldaTabla(grid, "Subtotal", 0, 3, true, HorizontalAlignment.Right);

            // Fila de datos
            string concepto = $"{_data.NombreEvento}  –  {_data.NombreTipoBoleto}";
            AgregarCeldaTabla(grid, concepto, 1, 0, false);
            AgregarCeldaTabla(grid, _data.Cantidad.ToString(), 1, 1, false, HorizontalAlignment.Center);
            AgregarCeldaTabla(grid, $"Bs {_data.PrecioUnitario:F2}", 1, 2, false, HorizontalAlignment.Right);
            AgregarCeldaTabla(grid, $"Bs {_data.Subtotal:F2}", 1, 3, false, HorizontalAlignment.Right);

            return tabla;
        }

        private StackPanel SeccionTotales()
        {
            var sp = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };

            sp.Children.Add(FilaResumen("Subtotal:", $"Bs {_data.Subtotal:F2}"));
            sp.Children.Add(FilaResumen("Honorario (10%):", $"Bs {_data.Honorario:F2}"));
            sp.Children.Add(FilaResumen("Descuento:", $"- Bs {_data.Descuento:F2}"));

            var separador = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Margin = new Thickness(0, 6, 0, 6),
            };
            sp.Children.Add(separador);

            var rowTotal = new Grid();
            rowTotal.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowTotal.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var lblTotal = new TextBlock
            {
                Text = "TOTAL PAGADO:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
            };
            lblTotal.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
            Grid.SetColumn(lblTotal, 0);
            rowTotal.Children.Add(lblTotal);

            var lblMonto = new TextBlock
            {
                Text = $"Bs {_data.TotalPagado:F2}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
            };
            lblMonto.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
            Grid.SetColumn(lblMonto, 1);
            rowTotal.Children.Add(lblMonto);

            sp.Children.Add(rowTotal);
            return sp;
        }

        // ── Helpers ──

        private Border Separador() => new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
            Margin = new Thickness(0, 14, 0, 14),
        };

        private TextBlock SeccionTitulo(string texto) => new TextBlock
        {
            Text = texto,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130)),
            Margin = new Thickness(0, 0, 0, 4),
        };

        private Grid FilaResumen(string label, string valor)
        {
            var g = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            g.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
            });
            var valTb = new TextBlock
            {
                Text = valor,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
            };
            Grid.SetColumn(valTb, 1);
            g.Children.Add(valTb);
            return g;
        }

        private void AgregarFilaComprador(Grid grid, int row, string label, string valor)
        {
            var lbl = new TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                Margin = new Thickness(0, 3, 14, 3),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(lbl, row);
            Grid.SetColumn(lbl, 0);
            grid.Children.Add(lbl);

            var val = new TextBlock
            {
                Text = valor,
                FontSize = 12,
                Margin = new Thickness(0, 3, 0, 3),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(val, row);
            Grid.SetColumn(val, 1);
            grid.Children.Add(val);
        }

        private void AgregarCeldaTabla(
            Grid grid,
            string texto,
            int row,
            int col,
            bool esHeader,
            HorizontalAlignment align = HorizontalAlignment.Left
        )
        {
            var tb = new TextBlock
            {
                Text = texto,
                Padding = new Thickness(10, 7, 10, 7),
                FontSize = 12,
                FontWeight = esHeader ? FontWeights.Bold : FontWeights.Normal,
                Foreground = esHeader ? Brushes.White : new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                HorizontalAlignment = align,
                TextWrapping = TextWrapping.Wrap,
            };
            Grid.SetRow(tb, row);
            Grid.SetColumn(tb, col);
            grid.Children.Add(tb);
        }

        // ── Exportar / Imprimir ──

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() != true)
                return;

            // Asegurar que el layout esté medido antes de imprimir
            _pnlFactura.Measure(new Size(dlg.PrintableAreaWidth, double.PositiveInfinity));
            _pnlFactura.Arrange(new Rect(new Size(dlg.PrintableAreaWidth, _pnlFactura.DesiredSize.Height)));

            var capabilities = dlg.PrintQueue.GetPrintCapabilities(dlg.PrintTicket);
            double areaW = capabilities.PageImageableArea?.ExtentWidth ?? dlg.PrintableAreaWidth;
            double areaH = capabilities.PageImageableArea?.ExtentHeight ?? dlg.PrintableAreaHeight;

            double scaleX = areaW / _pnlFactura.ActualWidth;
            double scaleY = areaH / _pnlFactura.ActualHeight;
            double scale = Math.Min(scaleX, scaleY);
            if (scale > 1) scale = 1;

            var visual = new DrawingVisual();
            using (var ctx = visual.RenderOpen())
            {
                var brush = new VisualBrush(_pnlFactura);
                ctx.DrawRectangle(
                    brush,
                    null,
                    new Rect(
                        new Point(0, 0),
                        new Size(
                            _pnlFactura.ActualWidth * scale,
                            _pnlFactura.ActualHeight * scale
                        )
                    )
                );
            }

            dlg.PrintVisual(visual, $"Factura #{_data.NumeroFactura} — Nodus");

            MessageBox.Show(
                "Documento enviado a la impresora.\n\nPara guardar como PDF, selecciona 'Microsoft Print to PDF' en el diálogo.",
                "Impresión",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
