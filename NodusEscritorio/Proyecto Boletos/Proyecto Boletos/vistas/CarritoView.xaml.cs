using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class CarritoView : UserControl
    {
        private int _idUsuario;
        private List<MetodoPago> _metodosPago;
        private string _vistaActual = "boletos";

        public CarritoView(int idUsuario, string vistaInicial = "boletos")
        {
            InitializeComponent();
            _idUsuario = idUsuario;
            _vistaActual = vistaInicial;
        }

        public CarritoView()
            : this(0) { }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarMetodosPago();

            if (_vistaActual == "espera")
            {
                MostrarPanelEspera();
                await CargarEspera();
            }
            else
            {
                await CargarBoletos();
            }
        }

        private async Task CargarMetodosPago()
        {
            try
            {
                _metodosPago = (await ConexionDB.Client.From<MetodoPago>().Get()).Models;
            }
            catch { }
        }

        // ─── NAVEGACIÓN ──────────────────────────────────────────────────────

        private async void btnBoletos_Click(object sender, RoutedEventArgs e)
        {
            _vistaActual = "boletos";
            MostrarPanelBoletos();
            await CargarBoletos();
        }

        private async void btnEspera_Click(object sender, RoutedEventArgs e)
        {
            _vistaActual = "espera";
            MostrarPanelEspera();
            await CargarEspera();
        }

        private async void btnHistorial_Click(object sender, RoutedEventArgs e)
        {
            _vistaActual = "historial";
            MostrarPanelHistorial();
            await CargarHistorial();
        }

        private void MostrarPanelBoletos()
        {
            panelBoletos.Visibility = Visibility.Visible;
            panelEspera.Visibility = Visibility.Collapsed;
            panelHistorial.Visibility = Visibility.Collapsed;
            ActivarTab(btnBoletos);
            DesactivarTab(btnEspera);
            DesactivarTab(btnHistorial);
        }

        private void MostrarPanelEspera()
        {
            panelEspera.Visibility = Visibility.Visible;
            panelBoletos.Visibility = Visibility.Collapsed;
            panelHistorial.Visibility = Visibility.Collapsed;
            ActivarTab(btnEspera);
            DesactivarTab(btnBoletos);
            DesactivarTab(btnHistorial);
        }

        private void MostrarPanelHistorial()
        {
            panelHistorial.Visibility = Visibility.Visible;
            panelBoletos.Visibility = Visibility.Collapsed;
            panelEspera.Visibility = Visibility.Collapsed;
            ActivarTab(btnHistorial);
            DesactivarTab(btnBoletos);
            DesactivarTab(btnEspera);
        }

        private void ActivarTab(Button btn)
        {
            btn.SetResourceReference(Button.BackgroundProperty, "Color1");
            btn.Foreground = Brushes.White;
            btn.SetResourceReference(Button.BorderBrushProperty, "Color1");
        }

        private void DesactivarTab(Button btn)
        {
            btn.SetResourceReference(Button.BackgroundProperty, "Color3");
            btn.SetResourceReference(Button.ForegroundProperty, "Color1");
            btn.SetResourceReference(Button.BorderBrushProperty, "Color1");
        }

        // ─── BOLETOS: CARGA DE EVENTOS PÚBLICOS ──────────────────────────────

        private async Task CargarBoletos()
        {
            txtCargandoBoletos.Visibility = Visibility.Visible;
            txtSinBoletos.Visibility = Visibility.Collapsed;
            wpBoletos.Children.Clear();

            try
            {
                var eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;
                var recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
                var fechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;
                var tiposBoleto = (await ConexionDB.Client.From<TipoBoleto>().Get()).Models;

                var cards = eventos
                    .Where(ev =>
                        ev.EsPublico
                        && !string.Equals(
                            ev.EstadoEvento?.Trim(),
                            "Cancelado",
                            StringComparison.OrdinalIgnoreCase
                        )
                        && !string.Equals(
                            ev.EstadoEvento?.Trim(),
                            "En reserva",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .Select(ev =>
                    {
                        var tipos = tiposBoleto
                            .Where(t => t.IdEvento == ev.IdEvento && t.CantidadDisponible > 0)
                            .ToList();
                        if (tipos.Count == 0)
                            return null;

                        var recinto = recintos.Find(r => r.IdRecinto == ev.IdRecinto);
                        var fecha = fechas.Find(f => f.Id == ev.IdFechaEvento);

                        return new EventoCardVista
                        {
                            IdEvento = ev.IdEvento,
                            NombreEvento = ev.NombreEvento,
                            ImagenUrl = ev.ImagenUrl ?? string.Empty,
                            FechaDisplay =
                                fecha != null
                                    ? fecha.FechaInicio.ToString("dd/MM/yyyy")
                                        + "  "
                                        + fecha.HoraInicio.ToString(@"hh\:mm")
                                    : "—",
                            NombreRecinto = recinto?.NombreRecinto ?? "—",
                            PrecioBase = tipos.Min(t => t.Precio),
                            BoletosDisponibles = tipos.Sum(t => t.CantidadDisponible),
                            TiposBoleto = tipos
                                .Select(t => new TipoBoletoCardVista
                                {
                                    IdTipoBoleto = t.IdTipoBoleto,
                                    NombreTipo = t.NombreTipoBoleto,
                                    Precio = t.Precio,
                                    Disponibles = t.CantidadDisponible,
                                })
                                .ToList(),
                        };
                    })
                    .Where(c => c != null)
                    .ToList();

                txtCargandoBoletos.Visibility = Visibility.Collapsed;

                if (cards.Count == 0)
                {
                    txtSinBoletos.Visibility = Visibility.Visible;
                    return;
                }

                foreach (var card in cards)
                    wpBoletos.Children.Add(CrearCard(card));
            }
            catch (Exception ex)
            {
                txtCargandoBoletos.Visibility = Visibility.Collapsed;
                MessageBox.Show(
                    $"Error al cargar eventos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private Border CrearCard(EventoCardVista card)
        {
            var root = new Border
            {
                Width = 265,
                Margin = new Thickness(8),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.15,
                    Color = Colors.Black,
                },
            };
            root.SetResourceReference(Border.BorderBrushProperty, "Color1");

            var stack = new StackPanel();
            root.Child = stack;

            // ── Imagen ──
            var imgBorder = new Border
            {
                Height = 150,
                CornerRadius = new CornerRadius(7, 7, 0, 0),
                ClipToBounds = true,
            };
            imgBorder.SetResourceReference(Border.BackgroundProperty, "Color2");

            if (!string.IsNullOrWhiteSpace(card.ImagenUrl))
            {
                try
                {
                    var img = new Image
                    {
                        Stretch = Stretch.UniformToFill,
                        Source = new BitmapImage(new Uri(card.ImagenUrl, UriKind.Absolute)),
                    };
                    imgBorder.Child = img;
                }
                catch
                {
                    imgBorder.Child = IconoEvento();
                }
            }
            else
            {
                imgBorder.Child = IconoEvento();
            }
            stack.Children.Add(imgBorder);

            // ── Cuerpo ──
            var body = new StackPanel { Margin = new Thickness(14, 10, 14, 14) };
            stack.Children.Add(body);

            var txtNombre = new TextBlock
            {
                Text = card.NombreEvento,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 6),
            };
            txtNombre.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
            body.Children.Add(txtNombre);

            body.Children.Add(FilaInfo("📅", card.FechaDisplay));
            body.Children.Add(FilaInfo("📍", card.NombreRecinto));

            var sepLine = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Margin = new Thickness(0, 8, 0, 8),
            };
            body.Children.Add(sepLine);

            var rowPrecio = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            rowPrecio.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );
            rowPrecio.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var lblPrecioTitulo = new TextBlock
            {
                Text = "Desde",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130)),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(lblPrecioTitulo, 0);
            rowPrecio.Children.Add(lblPrecioTitulo);

            var lblPrecio = new TextBlock
            {
                Text = $"Bs {card.PrecioBase:F2}",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
            };
            lblPrecio.SetResourceReference(TextBlock.ForegroundProperty, "Color1");
            Grid.SetColumn(lblPrecio, 1);
            rowPrecio.Children.Add(lblPrecio);
            body.Children.Add(rowPrecio);

            var lblDisp = new TextBlock
            {
                Text = $"🎟 {card.BoletosDisponibles} disponibles",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 160, 100)),
                Margin = new Thickness(0, 0, 0, 10),
            };
            body.Children.Add(lblDisp);

            var btnComprar = new Button
            {
                Content = "Comprar",
                Height = 34,
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = Brushes.White,
                Tag = card,
            };
            btnComprar.SetResourceReference(Button.BackgroundProperty, "Color1");
            btnComprar.SetResourceReference(Button.BorderBrushProperty, "Color1");
            btnComprar.Click += BtnComprar_Click;
            body.Children.Add(btnComprar);

            return root;
        }

        private TextBlock IconoEvento()
        {
            var tb = new TextBlock
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Text = "",
                FontSize = 40,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            return tb;
        }

        private StackPanel FilaInfo(string icono, string texto)
        {
            var sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 4),
            };
            sp.Children.Add(
                new TextBlock
                {
                    Text = icono,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                }
            );
            sp.Children.Add(
                new TextBlock
                {
                    Text = texto,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(90, 90, 90)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 200,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            );
            return sp;
        }

        // ─── COMPRAR ─────────────────────────────────────────────────────────

        private async void BtnComprar_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is EventoCardVista card))
                return;

            var dialogo = new SeleccionCompraDialog(card);
            dialogo.Owner = Window.GetWindow(this);
            dialogo.ShowDialog();

            if (!dialogo.Confirmado)
                return;

            var tipoSeleccionado = dialogo.TipoSeleccionado;
            int cantidad = dialogo.Cantidad;

            var desglose = new List<LineaDesglose>
            {
                new LineaDesglose
                {
                    Concepto = $"{tipoSeleccionado.NombreTipo} x{cantidad}",
                    Monto = tipoSeleccionado.Precio * cantidad,
                },
            };

            var ventanaPago = new PagoEventoDialog(_metodosPago, desglose);
            ventanaPago.Owner = Window.GetWindow(this);
            ventanaPago.ShowDialog();

            if (ventanaPago.Resultado == ResultadoPago.Cancelado)
                return;

            string estadoOrden =
                ventanaPago.Resultado == ResultadoPago.Pagar ? "Programado" : "En Espera";

            try
            {
                var tipoDb = (
                    await ConexionDB
                        .Client.From<TipoBoleto>()
                        .Where(t => t.IdTipoBoleto == tipoSeleccionado.IdTipoBoleto)
                        .Get()
                ).Models.FirstOrDefault();

                if (tipoDb == null || tipoDb.CantidadDisponible < cantidad)
                {
                    MessageBox.Show(
                        "No hay suficientes boletos disponibles.",
                        "Sin disponibilidad",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                var ordenResp = await ConexionDB
                    .Client.From<Orden>()
                    .Insert(
                        new Orden
                        {
                            IdUsuario = _idUsuario,
                            FechaOrden = DateTime.Now,
                            EstadoOrden = estadoOrden,
                            DescuentoOrden = ventanaPago.Descuento,
                            CompradorNombre =
                                $"{ventanaPago.ClienteNombre} {ventanaPago.ClienteApellido}".Trim(),
                            CompradorNit = ventanaPago.ClienteNit,
                            CompradorCorreo = ventanaPago.ClienteCorreo,
                        }
                    );
                var ordenInserta = ordenResp.Models.First();

                await ConexionDB
                    .Client.From<DetalleOrden>()
                    .Insert(
                        new DetalleOrden
                        {
                            IdOrden = ordenInserta.IdOrden,
                            IdTipoBoleto = tipoSeleccionado.IdTipoBoleto,
                            Cantidad = cantidad,
                            PrecioUnitario = tipoSeleccionado.Precio,
                            Descuento = 0,
                        }
                    );

                tipoDb.CantidadDisponible -= cantidad;
                await ConexionDB.Client.From<TipoBoleto>().Update(tipoDb);

                if (ventanaPago.Resultado == ResultadoPago.Pagar)
                {
                    await ConexionDB
                        .Client.From<Pago>()
                        .Insert(
                            new Pago
                            {
                                IdOrden = ordenInserta.IdOrden,
                                IdMetodoPago = ventanaPago.IdMetodoPagoSeleccionado,
                                MontoPago = ventanaPago.TotalFinal,
                                Moneda = "BOB",
                                FechaPago = DateTime.Now,
                                EstadoPago = "pagado",
                                ReferenciaPago = ventanaPago.Nota,
                            }
                        );

                    MessageBox.Show(
                        "¡Compra registrada exitosamente!",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Compra guardada en 'En Espera'. Completa el pago cuando desees.",
                        "En Espera",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }

                await CargarBoletos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al procesar la compra: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ─── EN ESPERA: CARGA ────────────────────────────────────────────────

        private async Task CargarEspera()
        {
            txtSinEspera.Visibility = Visibility.Collapsed;
            dgEspera.ItemsSource = null;
            ConfigurarColumnasEspera();

            try
            {
                var ordenes = (
                    await ConexionDB
                        .Client.From<Orden>()
                        .Where(o => o.IdUsuario == _idUsuario)
                        .Get()
                )
                    .Models.Where(o =>
                        string.Equals(
                            o.EstadoOrden?.Trim(),
                            "en espera",
                            StringComparison.OrdinalIgnoreCase
                        )
                        || string.Equals(
                            o.EstadoOrden?.Trim(),
                            "pendiente",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .ToList();

                var detalles = (await ConexionDB.Client.From<DetalleOrden>().Get()).Models;
                var tiposBoleto = (await ConexionDB.Client.From<TipoBoleto>().Get()).Models;
                var eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;

                var vista = new List<OrdenEsperaVista>();

                foreach (var orden in ordenes)
                {
                    var dets = detalles.Where(d => d.IdOrden == orden.IdOrden).ToList();
                    foreach (var det in dets)
                    {
                        var tipo = tiposBoleto.Find(t => t.IdTipoBoleto == det.IdTipoBoleto);
                        var evento =
                            tipo != null ? eventos.Find(ev => ev.IdEvento == tipo.IdEvento) : null;

                        vista.Add(
                            new OrdenEsperaVista
                            {
                                TipoFila = "orden",
                                IdOrden = orden.IdOrden,
                                IdDetalleOrden = det.IdDetalleOrden,
                                IdTipoBoleto = det.IdTipoBoleto,
                                NombreEvento = evento?.NombreEvento ?? "—",
                                NombreTipoBoleto = tipo?.NombreTipoBoleto ?? "—",
                                Cantidad = det.Cantidad,
                                PrecioUnitario = det.PrecioUnitario,
                                Total = det.PrecioUnitario * det.Cantidad,
                                FechaOrden = orden.FechaOrden.ToString("dd/MM/yyyy HH:mm"),
                                Estado = orden.EstadoOrden,
                            }
                        );
                    }
                }

                // Eventos creados con "Sin pago por ahora": también deben aparecer en espera
                var eventosEnEspera = eventos
                    .Where(ev =>
                        ev.IdOrganizador == _idUsuario
                        && (
                            string.Equals(
                                ev.EstadoEvento?.Trim(),
                                "En espera",
                                StringComparison.OrdinalIgnoreCase
                            )
                            || string.Equals(
                                ev.EstadoEvento?.Trim(),
                                "En reserva",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                    .ToList();

                if (eventosEnEspera.Count > 0)
                {
                    var fechasEventos = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;

                    foreach (var ev in eventosEnEspera)
                    {
                        var fechaEv = fechasEventos.Find(f => f.Id == ev.IdFechaEvento);
                        vista.Add(
                            new OrdenEsperaVista
                            {
                                TipoFila = "evento",
                                IdEvento = ev.IdEvento,
                                NombreEvento = ev.NombreEvento,
                                NombreTipoBoleto = "Evento (pago pendiente)",
                                FechaOrden =
                                    fechaEv != null
                                        ? fechaEv.FechaInicio.ToString("dd/MM/yyyy")
                                            + "  "
                                            + fechaEv.HoraInicio.ToString(@"hh\:mm")
                                        : "—",
                                Estado = ev.EstadoEvento,
                            }
                        );
                    }
                }

                if (vista.Count == 0)
                {
                    txtSinEspera.Visibility = Visibility.Visible;
                    return;
                }

                dgEspera.ItemsSource = vista;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar pendientes: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ConfigurarColumnasEspera()
        {
            dgEspera.Columns.Clear();
            var tc = (Style)Application.Current.FindResource("TextoCentrado");

            dgEspera.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Evento",
                    Binding = new Binding("NombreEvento"),
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                    ElementStyle = tc,
                }
            );
            dgEspera.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Tipo",
                    Binding = new Binding("NombreTipoBoleto"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    ElementStyle = tc,
                }
            );
            dgEspera.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Cant.",
                    Binding = new Binding("CantidadDisplay"),
                    Width = 100,
                    ElementStyle = tc,
                }
            );
            dgEspera.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Total",
                    Binding = new Binding("TotalFormateado"),
                    Width = 100,
                    ElementStyle = tc,
                }
            );
            dgEspera.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Fecha",
                    Binding = new Binding("FechaOrden"),
                    Width = 200,
                    ElementStyle = tc,
                }
            );

            dgEspera.Columns.Add(
                CrearColumnaBoton("Pagar", Color.FromRgb(46, 125, 90), BtnPagar_Click)
            );
            dgEspera.Columns.Add(
                CrearColumnaBoton("Descartar", Color.FromRgb(244, 67, 54), BtnDescartar_Click)
            );
        }

        private DataGridTemplateColumn CrearColumnaBoton(
            string label,
            Color color,
            RoutedEventHandler handler
        )
        {
            var col = new DataGridTemplateColumn { Header = label, Width = 85 };
            var tpl = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, label);
            factory.SetValue(Button.HeightProperty, 26.0);
            factory.SetValue(Button.PaddingProperty, new Thickness(8, 0, 8, 0));
            factory.SetValue(Button.BackgroundProperty, new SolidColorBrush(color));
            factory.SetValue(Button.ForegroundProperty, Brushes.White);
            factory.SetValue(Button.BorderBrushProperty, new SolidColorBrush(color));
            factory.SetValue(Button.CursorProperty, System.Windows.Input.Cursors.Hand);
            factory.AddHandler(Button.ClickEvent, handler);
            tpl.VisualTree = factory;
            col.CellTemplate = tpl;
            return col;
        }

        // ─── PAGAR PENDIENTE ─────────────────────────────────────────────────

        private void BtnPagar_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.DataContext is OrdenEsperaVista item))
                return;

            if (item.TipoFila == "evento")
            {
                PagarEventoEnEspera(item);
                return;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var desglose = new List<LineaDesglose>
                    {
                        new LineaDesglose
                        {
                            Concepto =
                                $"{item.NombreEvento} — {item.NombreTipoBoleto} x{item.Cantidad}",
                            Monto = item.Total,
                        },
                    };

                    var ventanaPago = new PagoEventoDialog(_metodosPago, desglose);
                    ventanaPago.Owner = Window.GetWindow(this);
                    ventanaPago.ShowDialog();

                    if (ventanaPago.Resultado != ResultadoPago.Pagar)
                        return;

                    var ordenDb = (
                        await ConexionDB
                            .Client.From<Orden>()
                            .Where(o => o.IdOrden == item.IdOrden)
                            .Get()
                    ).Models.FirstOrDefault();

                    if (ordenDb != null)
                    {
                        ordenDb.EstadoOrden = "pagado";
                        ordenDb.CompradorNombre =
                            $"{ventanaPago.ClienteNombre} {ventanaPago.ClienteApellido}".Trim();
                        ordenDb.CompradorNit = ventanaPago.ClienteNit;
                        ordenDb.CompradorCorreo = ventanaPago.ClienteCorreo;
                        ordenDb.DescuentoOrden = ventanaPago.Descuento;
                        await ConexionDB.Client.From<Orden>().Update(ordenDb);
                    }

                    await ConexionDB
                        .Client.From<Pago>()
                        .Insert(
                            new Pago
                            {
                                IdOrden = item.IdOrden,
                                IdMetodoPago = ventanaPago.IdMetodoPagoSeleccionado,
                                MontoPago = ventanaPago.TotalFinal,
                                Moneda = "BOB",
                                FechaPago = DateTime.Now,
                                EstadoPago = "pagado",
                                ReferenciaPago = ventanaPago.Nota,
                            }
                        );

                    MessageBox.Show(
                        "Pago registrado exitosamente.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    await CargarEspera();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al pagar: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            });
        }

        // ─── PAGAR EVENTO EN ESPERA (creado con "Sin pago por ahora") ────────

        private void PagarEventoEnEspera(OrdenEsperaVista item)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    // 1. Recuperar el evento
                    var eventoDb = (
                        await ConexionDB
                            .Client.From<Evento>()
                            .Where(ev => ev.IdEvento == item.IdEvento)
                            .Get()
                    ).Models.FirstOrDefault();

                    if (eventoDb == null)
                    {
                        MessageBox.Show("No se encontró el evento.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var desglose = new List<LineaDesglose>();

                    // 2. Precio del recinto
                    if (eventoDb.IdRecinto.HasValue)
                    {
                        var recintoDb = (
                            await ConexionDB
                                .Client.From<Recinto>()
                                .Where(r => r.IdRecinto == eventoDb.IdRecinto.Value)
                                .Get()
                        ).Models.FirstOrDefault();

                        if (recintoDb != null)
                        {
                            desglose.Add(new LineaDesglose
                            {
                                Concepto = $"Recinto: {recintoDb.NombreRecinto} (Bs/hora)",
                                Monto = (decimal)recintoDb.PrecioHora,
                            });
                        }
                    }

                    // 3. Servicios vinculados al evento → precio desde DetalleProveedorServicio
                    var eventosServicios = (
                        await ConexionDB
                            .Client.From<EventoServicio>()
                            .Where(es => es.IdEvento == eventoDb.IdEvento)
                            .Get()
                    ).Models;

                    if (eventosServicios.Count > 0)
                    {
                        var todosServicios = (await ConexionDB.Client.From<Servicio>().Get()).Models;
                        var todosItems = (await ConexionDB.Client.From<DetalleProveedorServicio>().Get()).Models;

                        foreach (var es in eventosServicios)
                        {
                            var servicio = todosServicios.Find(s => s.IdServicio == es.IdServicio);
                            // Tomar el primer item oferta que coincida con ese servicio
                            var itemOferta = todosItems.Find(i => i.IdServicio == es.IdServicio);

                            string nombreConcepto = servicio?.NombreServicio
                                ?? itemOferta?.NombreItem
                                ?? $"Servicio #{es.IdServicio}";

                            if (es.Cantidad > 1)
                                nombreConcepto += $" x{es.Cantidad}";

                            decimal precioUnitario = itemOferta != null ? (decimal)itemOferta.PrecioItem : 0m;
                            decimal montoTotal = precioUnitario * es.Cantidad;

                            desglose.Add(new LineaDesglose
                            {
                                Concepto = $"Servicio: {nombreConcepto}",
                                Monto = montoTotal,
                            });
                        }
                    }

                    if (desglose.Count == 0)
                    {
                        desglose.Add(new LineaDesglose
                        {
                            Concepto = $"Evento: {eventoDb.NombreEvento}",
                            Monto = 0m,
                        });
                    }

                    // 4. Abrir diálogo de pago con el desglose
                    var ventanaPago = new PagoEventoDialog(_metodosPago, desglose);
                    ventanaPago.Owner = Window.GetWindow(this);
                    ventanaPago.ShowDialog();

                    if (ventanaPago.Resultado != ResultadoPago.Pagar)
                        return;

                    // 5. Marcar evento y reserva como Programado
                    eventoDb.EstadoEvento = "Programado";
                    await ConexionDB.Client.From<Evento>().Update(eventoDb);

                    if (eventoDb.IdReserva.HasValue)
                    {
                        var reservaDb = (
                            await ConexionDB
                                .Client.From<Reserva>()
                                .Where(r => r.IdReserva == (int)eventoDb.IdReserva.Value)
                                .Get()
                        ).Models.FirstOrDefault();

                        if (reservaDb != null)
                        {
                            reservaDb.EstadoReserva = "Programado";
                            await ConexionDB.Client.From<Reserva>().Update(reservaDb);
                        }
                    }

                    // 6. Crear orden y pago
                    var ordenResp = await ConexionDB
                        .Client.From<Orden>()
                        .Insert(new Orden
                        {
                            IdUsuario = _idUsuario,
                            FechaOrden = DateTime.Now,
                            EstadoOrden = "pagado",
                            DescuentoOrden = ventanaPago.Descuento,
                            CompradorNombre = $"{ventanaPago.ClienteNombre} {ventanaPago.ClienteApellido}".Trim(),
                            CompradorNit = ventanaPago.ClienteNit,
                            CompradorCorreo = ventanaPago.ClienteCorreo,
                        });
                    var ordenInserta = ordenResp.Models.First();

                    await ConexionDB
                        .Client.From<Pago>()
                        .Insert(new Pago
                        {
                            IdOrden = ordenInserta.IdOrden,
                            IdMetodoPago = ventanaPago.IdMetodoPagoSeleccionado,
                            MontoPago = ventanaPago.TotalFinal,
                            Moneda = "BOB",
                            FechaPago = DateTime.Now,
                            EstadoPago = "pagado",
                            ReferenciaPago = ventanaPago.Nota,
                        });

                    MessageBox.Show(
                        "Evento marcado como pagado. Ya aparecerá en el calendario como Programado.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    await CargarEspera();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al pagar el evento: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            });
        }

        private void DescartarEventoEnEspera(OrdenEsperaVista item)
        {
            if (
                MessageBox.Show(
                    $"¿Cancelar el evento '{item.NombreEvento}'? Esta acción no se puede deshacer.",
                    "Confirmar cancelación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) != MessageBoxResult.Yes
            )
                return;

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var eventoDb = (
                        await ConexionDB
                            .Client.From<Evento>()
                            .Where(ev => ev.IdEvento == item.IdEvento)
                            .Get()
                    ).Models.FirstOrDefault();

                    if (eventoDb != null)
                    {
                        eventoDb.EstadoEvento = "Cancelado";
                        await ConexionDB.Client.From<Evento>().Update(eventoDb);

                        if (eventoDb.IdReserva.HasValue)
                        {
                            var reservaDb = (
                                await ConexionDB
                                    .Client.From<Reserva>()
                                    .Where(r => r.IdReserva == (int)eventoDb.IdReserva.Value)
                                    .Get()
                            ).Models.FirstOrDefault();

                            if (reservaDb != null)
                            {
                                reservaDb.EstadoReserva = "Cancelado";
                                await ConexionDB.Client.From<Reserva>().Update(reservaDb);
                            }
                        }
                    }

                    await CargarEspera();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al cancelar el evento: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            });
        }

        // ─── DESCARTAR PENDIENTE ─────────────────────────────────────────────

        private void BtnDescartar_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.DataContext is OrdenEsperaVista item))
                return;

            if (item.TipoFila == "evento")
            {
                DescartarEventoEnEspera(item);
                return;
            }

            if (
                MessageBox.Show(
                    $"¿Descartar la compra de '{item.NombreTipoBoleto}' del evento '{item.NombreEvento}'?\n"
                        + "Se liberarán los boletos reservados.",
                    "Confirmar descarte",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) != MessageBoxResult.Yes
            )
                return;

            Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var tipoDb = (
                        await ConexionDB
                            .Client.From<TipoBoleto>()
                            .Where(t => t.IdTipoBoleto == item.IdTipoBoleto)
                            .Get()
                    ).Models.FirstOrDefault();

                    if (tipoDb != null)
                    {
                        tipoDb.CantidadDisponible += item.Cantidad;
                        await ConexionDB.Client.From<TipoBoleto>().Update(tipoDb);
                    }

                    await ConexionDB
                        .Client.From<DetalleOrden>()
                        .Where(d => d.IdDetalleOrden == item.IdDetalleOrden)
                        .Delete();

                    var otrosDetalles = (
                        await ConexionDB
                            .Client.From<DetalleOrden>()
                            .Where(d => d.IdOrden == item.IdOrden)
                            .Get()
                    ).Models;

                    if (otrosDetalles.Count == 0)
                    {
                        await ConexionDB
                            .Client.From<Orden>()
                            .Where(o => o.IdOrden == item.IdOrden)
                            .Delete();
                    }

                    await CargarEspera();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al descartar: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            });
        }

        // ─── HISTORIAL: CARGA ──────────────────────────────────────────────────

        private async Task CargarHistorial()
        {
            txtCargandoHistorial.Visibility = Visibility.Visible;
            txtSinHistorial.Visibility = Visibility.Collapsed;
            dgHistorial.ItemsSource = null;
            ConfigurarColumnasHistorial();

            try
            {
                var ordenes = (
                    await ConexionDB
                        .Client.From<Orden>()
                        .Where(o => o.IdUsuario == _idUsuario)
                        .Get()
                ).Models.Where(o =>
                    string.Equals(o.EstadoOrden?.Trim(), "pagado", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(o.EstadoOrden?.Trim(), "Programado", StringComparison.OrdinalIgnoreCase) // ← NUEVO
                ).ToList();

                var detalles = (await ConexionDB.Client.From<DetalleOrden>().Get()).Models;
                var tiposBoleto = (await ConexionDB.Client.From<TipoBoleto>().Get()).Models;
                var eventos = (await ConexionDB.Client.From<Evento>().Get()).Models;
                var pagos = (await ConexionDB.Client.From<Pago>().Get()).Models;
                var metodosPago = _metodosPago ?? new List<MetodoPago>();

                var vista = new List<CompraHistorialVista>();

                foreach (var orden in ordenes)
                {
                    var dets = detalles.Where(d => d.IdOrden == orden.IdOrden).ToList();
                    var pagoOrden = pagos.Find(p => p.IdOrden == orden.IdOrden);
                    var metodoPago =
                        pagoOrden != null
                            ? metodosPago.Find(m => m.IdMetodoPago == pagoOrden.IdMetodoPago)
                            : null;

                    foreach (var det in dets)
                    {
                        var tipo = tiposBoleto.Find(t => t.IdTipoBoleto == det.IdTipoBoleto);
                        var evento =
                            tipo != null ? eventos.Find(ev => ev.IdEvento == tipo.IdEvento) : null;

                        decimal subtotal = det.PrecioUnitario * det.Cantidad;
                        decimal honorario = Math.Round(subtotal * 0.10m, 2);
                        decimal total = subtotal + honorario - orden.DescuentoOrden;
                        if (total < 0)
                            total = 0;

                        vista.Add(
                            new CompraHistorialVista
                            {
                                IdOrden = orden.IdOrden,
                                IdDetalleOrden = det.IdDetalleOrden,
                                NombreEvento = evento?.NombreEvento ?? "—",
                                NombreTipoBoleto = tipo?.NombreTipoBoleto ?? "—",
                                Cantidad = det.Cantidad,
                                PrecioUnitario = det.PrecioUnitario,
                                Subtotal = subtotal,
                                Honorario = honorario,
                                Descuento = orden.DescuentoOrden,
                                TotalPagado = pagoOrden?.MontoPago ?? total,
                                FechaCompra = orden.FechaOrden.ToString("dd/MM/yyyy HH:mm"),
                                EstadoPago = pagoOrden?.EstadoPago ?? orden.EstadoOrden,
                                CompradorNombre = orden.CompradorNombre,
                                CompradorNit = orden.CompradorNit,
                                CompradorCorreo = orden.CompradorCorreo,
                                MetodoPago = metodoPago?.Nombre ?? "—",
                            }
                        );
                    }
                }

                txtCargandoHistorial.Visibility = Visibility.Collapsed;

                if (vista.Count == 0)
                {
                    txtSinHistorial.Visibility = Visibility.Visible;
                    return;
                }

                dgHistorial.ItemsSource = vista;
            }
            catch (Exception ex)
            {
                txtCargandoHistorial.Visibility = Visibility.Collapsed;
                MessageBox.Show(
                    $"Error al cargar historial: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ConfigurarColumnasHistorial()
        {
            dgHistorial.Columns.Clear();
            var tc = (Style)Application.Current.FindResource("TextoCentrado");

            dgHistorial.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Evento",
                    Binding = new Binding("NombreEvento"),
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                    ElementStyle = tc,
                }
            );
            dgHistorial.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Tipo Boleto",
                    Binding = new Binding("NombreTipoBoleto"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    ElementStyle = tc,
                }
            );
            dgHistorial.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Cant.",
                    Binding = new Binding("Cantidad"),
                    Width = 70,
                    ElementStyle = tc,
                }
            );
            dgHistorial.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Total Pagado",
                    Binding = new Binding("TotalPagadoFormateado"),
                    Width = 130,
                    ElementStyle = tc,
                }
            );
            dgHistorial.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Fecha Compra",
                    Binding = new Binding("FechaCompra"),
                    Width = 160,
                    ElementStyle = tc,
                }
            );
            dgHistorial.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Estado",
                    Binding = new Binding("EstadoPago"),
                    Width = 90,
                    ElementStyle = tc,
                }
            );
            dgHistorial.Columns.Add(
                new DataGridTextColumn
                {
                    Header = "Método",
                    Binding = new Binding("MetodoPago"),
                    Width = 90,
                    ElementStyle = tc,
                }
            );

            dgHistorial.Columns.Add(
                CrearColumnaBoton("Ver Factura", Color.FromRgb(46, 125, 90), BtnVerFactura_Click)
            );
        }

        private void BtnVerFactura_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.DataContext is CompraHistorialVista item))
                return;

            var factura = new FacturaData
            {
                NumeroFactura = item.IdOrden,
                FechaCompra = item.FechaCompra,
                CompradorNombre = item.CompradorNombre,
                CompradorNit = item.CompradorNit,
                CompradorCorreo = item.CompradorCorreo,
                NombreEvento = item.NombreEvento,
                NombreTipoBoleto = item.NombreTipoBoleto,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                Subtotal = item.Subtotal,
                Honorario = item.Honorario,
                Descuento = item.Descuento,
                TotalPagado = item.TotalPagado,
                MetodoPago = item.MetodoPago,
                EstadoPago = item.EstadoPago,
            };

            var ventana = new FacturaDialog(factura);
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }
    } // end CarritoView

    // ─── MODELOS DE VISTA ────────────────────────────────────────────────────────

    public class EventoCardVista
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
        public string FechaDisplay { get; set; } = string.Empty;
        public string NombreRecinto { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public long BoletosDisponibles { get; set; }
        public List<TipoBoletoCardVista> TiposBoleto { get; set; } =
            new List<TipoBoletoCardVista>();
    }

    public class TipoBoletoCardVista
    {
        public int IdTipoBoleto { get; set; }
        public string NombreTipo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public long Disponibles { get; set; }
    }

    public class OrdenEsperaVista
    {
        // "orden" = compra de boletos pendiente de pago; "evento" = evento creado sin pago aún
        public string TipoFila { get; set; } = "orden";
        public int IdOrden { get; set; }
        public int IdDetalleOrden { get; set; }
        public int IdTipoBoleto { get; set; }
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public string NombreTipoBoleto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string CantidadDisplay => TipoFila == "evento" ? "—" : Cantidad.ToString();
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string TotalFormateado => TipoFila == "evento" ? "—" : $"Bs {Total:F2}";
        public string FechaOrden { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class CompraHistorialVista
    {
        public int IdOrden { get; set; }
        public int IdDetalleOrden { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public string NombreTipoBoleto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Honorario { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalPagado { get; set; }
        public string TotalPagadoFormateado => $"Bs {TotalPagado:F2}";
        public string FechaCompra { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public string CompradorNombre { get; set; } = string.Empty;
        public string CompradorNit { get; set; } = string.Empty;
        public string CompradorCorreo { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
    }

    public class FacturaData
    {
        public int NumeroFactura { get; set; }
        public string FechaCompra { get; set; } = string.Empty;
        public string CompradorNombre { get; set; } = string.Empty;
        public string CompradorNit { get; set; } = string.Empty;
        public string CompradorCorreo { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty;
        public string NombreTipoBoleto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Honorario { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalPagado { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
    }

    // ─── CLASES LEGADO (compatibilidad con código existente) ─────────────────────

    public class EventoCarritoVista
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; } = string.Empty;
        public string NombreRecinto { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public string NombreReservante { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class ReservaCarritoVista
    {
        public int IdReserva { get; set; }
        public string NombreReservante { get; set; } = string.Empty;
        public string NombreRecinto { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
