using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class ProveedoresView : UserControl
    {
        private List<Proveedor> _proveedores;
        private List<Ciudad> _ciudades;
        private List<Servicio> _servicios;
        private List<Categoria> _categorias;
        private List<ProveedorServicio> _proveedorServicios;
        private List<DetalleProveedorServicio> _detalles;
        private Proveedor _proveedorSeleccionado;
        private bool _modoEdicion = false;
        private int _idRol;

        private readonly List<FilaServicioProveedor> _filasServicio =
            new List<FilaServicioProveedor>();

        public ProveedoresView(int idRol)
        {
            InitializeComponent();
            _idRol = idRol;

            Loaded += async (s, e) =>
            {
                await CargarDatos();
                ConstruirCatalogo("");
                ConstruirResumen("");
            };

            txtBuscar.TextChanged += (s, e) => FiltrarDirectorio();
            txtBuscarCatalogo.TextChanged += (s, e) =>
                ConstruirCatalogo(txtBuscarCatalogo.Text?.Trim() ?? "");
            txtBuscarResumen.TextChanged += (s, e) =>
                ConstruirResumen(txtBuscarResumen.Text?.Trim() ?? "");
        }

        public ProveedoresView()
            : this(0) { }

        // ─── CARGA DE DATOS ──────────────────────────────────────────────────

        private async Task CargarDatos()
        {
            try
            {
                _ciudades = (await ConexionDB.Client.From<Ciudad>().Get()).Models;
                cmbCiudad.ItemsSource = _ciudades;
            }
            catch { }

            try
            {
                _categorias = (await ConexionDB.Client.From<Categoria>().Get()).Models;
                _servicios = (await ConexionDB.Client.From<Servicio>().Get()).Models;

                foreach (var s in _servicios)
                {
                    var cat = _categorias?.Find(c => c.IdCategoria == s.IdCategoria);
                    s.NombreCategoria = cat?.NombreCategoria ?? $"Categoría {s.IdCategoria}";
                }
            }
            catch { }

            try
            {
                _detalles = (await ConexionDB.Client.From<DetalleProveedorServicio>().Get()).Models;
            }
            catch
            {
                _detalles = new List<DetalleProveedorServicio>();
            }

            try
            {
                _proveedorServicios = (
                    await ConexionDB.Client.From<ProveedorServicio>().Get()
                ).Models;
                _proveedores = (await ConexionDB.Client.From<Proveedor>().Get()).Models;

                foreach (var p in _proveedores)
                {
                    var ciudad = _ciudades?.Find(c => c.IdCiudad == p.IdCiudad);
                    p.NombreCiudad = ciudad?.NombreCiudad ?? string.Empty;

                    var ps = _proveedorServicios
                        ?.Where(x => x.IdProveedor == p.NitProveedor)
                        .ToList();
                    var partes = ps
                        ?.Select(x =>
                        {
                            var srv = _servicios?.Find(s => s.IdServicio == x.IdServicio);
                            return srv?.NombreServicio ?? x.IdServicio.ToString();
                        })
                        .ToList();

                    p.ServiciosConPrecio =
                        partes?.Count > 0 ? string.Join(" | ", partes) : "Sin servicios";
                }

                dgProveedores.ItemsSource = _proveedores;

                txtTotalProveedores.Text = _proveedores.Count.ToString();
                txtProveedoresActivos.Text = _proveedores
                    .Count(p => p.EstadoProveedor == "Activo")
                    .ToString();
                txtProveedoresInactivos.Text = _proveedores
                    .Count(p => p.EstadoProveedor == "Inactivo")
                    .ToString();
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

        // ─── TAB 1: CATÁLOGO DE SERVICIOS ────────────────────────────────────

        private void ConstruirCatalogo(string filtro)
        {
            panelCatalogo.Children.Clear();
            if (_categorias == null || _servicios == null || _proveedores == null)
                return;

            var fl = filtro.ToLower();

            foreach (var cat in _categorias.OrderBy(c => c.NombreCategoria))
            {
                var serviciosCat = _servicios.Where(s => s.IdCategoria == cat.IdCategoria).ToList();
                if (serviciosCat.Count == 0)
                    continue;

                var idsServiciosCat = new HashSet<int>(serviciosCat.Select(s => s.IdServicio));

                var gruposProveedor = (_proveedorServicios ?? new List<ProveedorServicio>())
                    .Where(ps => idsServiciosCat.Contains(ps.IdServicio))
                    .GroupBy(ps => ps.IdProveedor)
                    .Select(g =>
                    {
                        var prov = _proveedores.Find(p => p.NitProveedor == g.Key);
                        var items = g.Select(ps => new
                            {
                                Servicio = serviciosCat.Find(s => s.IdServicio == ps.IdServicio),
                                ps.Precio,
                            })
                            .Where(x =>
                                x.Servicio != null
                                && (
                                    string.IsNullOrEmpty(filtro)
                                    || x.Servicio.NombreServicio.ToLower().Contains(fl)
                                    || (prov?.NombreComercial ?? "").ToLower().Contains(fl)
                                    || cat.NombreCategoria.ToLower().Contains(fl)
                                )
                            )
                            .OrderBy(x => x.Servicio.NombreServicio)
                            .ToList();
                        return new { Prov = prov, Items = items };
                    })
                    .Where(x =>
                        x.Prov != null && x.Prov.EstadoProveedor == "Activo" && x.Items.Count > 0
                    )
                    .OrderBy(x => x.Prov.NombreComercial)
                    .ToList();

                if (gruposProveedor.Count == 0)
                    continue;

                // Encabezado categoría
                var header = new Border
                {
                    Margin = new Thickness(0, 14, 0, 8),
                    Padding = new Thickness(14, 8, 14, 8),
                    CornerRadius = new CornerRadius(8),
                };
                header.SetResourceReference(Border.BackgroundProperty, "Color1");
                header.Child = new TextBlock
                {
                    Text = cat.NombreCategoria.ToUpper(),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                };
                panelCatalogo.Children.Add(header);

                var wrap = new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 6),
                };

                foreach (var grupo in gruposProveedor)
                {
                    var cardStack = new StackPanel();

                    // Nombre del proveedor
                    cardStack.Children.Add(
                        new TextBlock
                        {
                            Text = grupo.Prov.NombreComercial,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                            Margin = new Thickness(0, 0, 0, 4),
                            TextWrapping = TextWrapping.Wrap,
                        }
                    );

                    cardStack.Children.Add(
                        new Border
                        {
                            BorderBrush = new SolidColorBrush(Color.FromRgb(225, 225, 225)),
                            BorderThickness = new Thickness(0, 1, 0, 0),
                            Margin = new Thickness(0, 2, 0, 8),
                        }
                    );

                    foreach (var srvItem in grupo.Items)
                    {
                        // Nombre del servicio como sub-título
                        cardStack.Children.Add(
                            new TextBlock
                            {
                                Text = srvItem.Servicio.NombreServicio,
                                FontSize = 12,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                                Margin = new Thickness(0, 2, 0, 4),
                            }
                        );

                        var dets = (_detalles ?? new List<DetalleProveedorServicio>())
                            .Where(d =>
                                d.IdProveedor == grupo.Prov.NitProveedor
                                && d.IdServicio == srvItem.Servicio.IdServicio
                            )
                            .OrderBy(d => d.NombreItem)
                            .ToList();

                        if (dets.Count > 0)
                        {
                            foreach (var det in dets)
                            {
                                var fD = new Grid { Margin = new Thickness(8, 2, 0, 2) };
                                fD.ColumnDefinitions.Add(
                                    new ColumnDefinition
                                    {
                                        Width = new GridLength(1, GridUnitType.Star),
                                    }
                                );
                                fD.ColumnDefinitions.Add(
                                    new ColumnDefinition { Width = GridLength.Auto }
                                );

                                var lblNom = new TextBlock
                                {
                                    Text = $"• {det.NombreItem}",
                                    FontSize = 11,
                                    Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                                    TextTrimming = TextTrimming.CharacterEllipsis,
                                };
                                Grid.SetColumn(lblNom, 0);

                                var lblPrecio = new TextBlock
                                {
                                    Text = $"Bs {det.PrecioItem:N0}",
                                    FontSize = 11,
                                    FontWeight = FontWeights.SemiBold,
                                    Foreground = new SolidColorBrush(Color.FromRgb(30, 120, 80)),
                                    Margin = new Thickness(8, 0, 0, 0),
                                };
                                Grid.SetColumn(lblPrecio, 1);

                                fD.Children.Add(lblNom);
                                fD.Children.Add(lblPrecio);
                                cardStack.Children.Add(fD);
                            }
                        }
                        else
                        {
                            cardStack.Children.Add(
                                new TextBlock
                                {
                                    Text = "Sin ítems registrados",
                                    FontSize = 11,
                                    FontStyle = FontStyles.Italic,
                                    Foreground = Brushes.Gray,
                                    Margin = new Thickness(8, 0, 0, 0),
                                }
                            );
                        }
                    }

                    wrap.Children.Add(
                        new Border
                        {
                            Child = cardStack,
                            Width = 260,
                            Margin = new Thickness(5),
                            Padding = new Thickness(14, 12, 14, 12),
                            Background = Brushes.White,
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(10),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                        }
                    );
                }

                panelCatalogo.Children.Add(wrap);
            }

            if (panelCatalogo.Children.Count == 0)
                panelCatalogo.Children.Add(
                    new TextBlock
                    {
                        Text = "No se encontraron proveedores.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 40, 0, 0),
                        FontSize = 14,
                    }
                );
        }

        // ─── TAB 3: RESUMEN DE PROVEEDORES ───────────────────────────────────

        private void ConstruirResumen(string filtro)
        {
            panelResumen.Children.Clear();
            if (_proveedores == null)
                return;

            var fl = filtro.ToLower();

            var lista = string.IsNullOrEmpty(filtro)
                ? _proveedores
                : _proveedores
                    .Where(p =>
                        (p.NombreComercial ?? "").ToLower().Contains(fl)
                        || (p.NombreCiudad ?? "").ToLower().Contains(fl)
                        || (p.ServiciosConPrecio ?? "").ToLower().Contains(fl)
                    )
                    .ToList();

            foreach (var p in lista.OrderBy(x => x.NombreComercial))
            {
                var serviciosDelProv = _proveedorServicios
                    ?.Where(ps => ps.IdProveedor == p.NitProveedor)
                    .Select(ps =>
                    {
                        var srv = _servicios?.Find(s => s.IdServicio == ps.IdServicio);
                        return new
                        {
                            Nombre = srv?.NombreServicio ?? "-",
                            ps.IdServicio,
                            ps.Precio,
                        };
                    })
                    .OrderBy(x => x.Nombre)
                    .ToList();

                var cardStack = new StackPanel { Margin = new Thickness(4) };

                // Cabecera proveedor
                var cabeceraGrid = new Grid();
                cabeceraGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                );
                cabeceraGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = GridLength.Auto }
                );

                var nombreTxt = new TextBlock
                {
                    Text = p.NombreComercial,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                };
                Grid.SetColumn(nombreTxt, 0);

                var estadoBadge = new Border
                {
                    Padding = new Thickness(8, 2, 8, 2),
                    CornerRadius = new CornerRadius(10),
                    Background =
                        p.EstadoProveedor == "Activo"
                            ? new SolidColorBrush(Color.FromRgb(232, 245, 233))
                            : new SolidColorBrush(Color.FromRgb(255, 235, 235)),
                    Child = new TextBlock
                    {
                        Text = p.EstadoProveedor,
                        FontSize = 11,
                        Foreground =
                            p.EstadoProveedor == "Activo"
                                ? new SolidColorBrush(Color.FromRgb(30, 130, 60))
                                : new SolidColorBrush(Color.FromRgb(180, 30, 30)),
                    },
                };
                Grid.SetColumn(estadoBadge, 1);

                cabeceraGrid.Children.Add(nombreTxt);
                cabeceraGrid.Children.Add(estadoBadge);
                cardStack.Children.Add(cabeceraGrid);

                cardStack.Children.Add(
                    new TextBlock
                    {
                        Text = $"Tel: {p.TelefonoComercial}   Ciudad: {p.NombreCiudad}",
                        FontSize = 11,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 4, 0, 8),
                    }
                );

                cardStack.Children.Add(
                    new Border
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        Margin = new Thickness(0, 0, 0, 8),
                    }
                );

                if (serviciosDelProv == null || serviciosDelProv.Count == 0)
                {
                    cardStack.Children.Add(
                        new TextBlock
                        {
                            Text = "Sin servicios registrados",
                            FontSize = 11,
                            FontStyle = FontStyles.Italic,
                            Foreground = Brushes.Gray,
                        }
                    );
                }
                else
                {
                    foreach (var srvInfo in serviciosDelProv)
                    {
                        cardStack.Children.Add(
                            new TextBlock
                            {
                                Text = srvInfo.Nombre,
                                FontSize = 12,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                                Margin = new Thickness(0, 2, 0, 2),
                            }
                        );

                        var dets = (_detalles ?? new List<DetalleProveedorServicio>())
                            .Where(d =>
                                d.IdProveedor == p.NitProveedor
                                && d.IdServicio == srvInfo.IdServicio
                            )
                            .OrderBy(d => d.NombreItem)
                            .ToList();

                        foreach (var det in dets)
                        {
                            var fila = new Grid { Margin = new Thickness(8, 1, 0, 1) };
                            fila.ColumnDefinitions.Add(
                                new ColumnDefinition
                                {
                                    Width = new GridLength(1, GridUnitType.Star),
                                }
                            );
                            fila.ColumnDefinitions.Add(
                                new ColumnDefinition { Width = GridLength.Auto }
                            );

                            var nomItem = new TextBlock
                            {
                                Text = $"• {det.NombreItem}",
                                FontSize = 11,
                                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                            };
                            Grid.SetColumn(nomItem, 0);

                            var precioItem = new TextBlock
                            {
                                Text = $"Bs {det.PrecioItem:N0}",
                                FontSize = 11,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = new SolidColorBrush(Color.FromRgb(30, 120, 80)),
                                Margin = new Thickness(8, 0, 0, 0),
                            };
                            Grid.SetColumn(precioItem, 1);

                            fila.Children.Add(nomItem);
                            fila.Children.Add(precioItem);
                            cardStack.Children.Add(fila);
                        }
                    }
                }

                panelResumen.Children.Add(
                    new Border
                    {
                        Child = cardStack,
                        Width = 280,
                        Margin = new Thickness(5),
                        Padding = new Thickness(14, 12, 14, 12),
                        Background = Brushes.White,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(10),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                    }
                );
            }

            if (panelResumen.Children.Count == 0)
                panelResumen.Children.Add(
                    new TextBlock
                    {
                        Text = "No se encontraron proveedores.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 40, 0, 0),
                        FontSize = 14,
                    }
                );
        }

        // ─── TAB 2: BUSCADOR DIRECTORIO ──────────────────────────────────────

        private void FiltrarDirectorio()
        {
            if (_proveedores == null)
                return;
            var texto = (txtBuscar.Text ?? "").Trim().ToLower();

            dgProveedores.ItemsSource = string.IsNullOrEmpty(texto)
                ? _proveedores
                : _proveedores
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

        // ─── FILAS DINÁMICAS DE SERVICIO EN EL FORMULARIO ───────────────────

        private void btnAgregarServicioForm_Click(object sender, RoutedEventArgs e)
        {
            if (_servicios == null)
                return;
            AgregarFilaServicio();
        }

        private void AgregarFilaServicio(
            int? idServicio = null,
            List<(string Nombre, long Precio)> items = null
        )
        {
            var fila = new FilaServicioProveedor(_servicios, idServicio, items);
            fila.OnEliminar = () =>
            {
                _filasServicio.Remove(fila);
                panelServiciosProveedor.Children.Remove(fila.Panel);
            };
            _filasServicio.Add(fila);
            panelServiciosProveedor.Children.Add(fila.Panel);
        }

        // ─── CRUD ─────────────────────────────────────────────────────────────

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            HabilitarFormulario(true);
            _modoEdicion = false;
            _proveedorSeleccionado = null;
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

            long nit = 0;
            if (!_modoEdicion)
            {
                if (
                    string.IsNullOrWhiteSpace(txtNitProveedor.Text)
                    || !long.TryParse(txtNitProveedor.Text.Trim(), out nit)
                )
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
            else
            {
                nit = _proveedorSeleccionado.NitProveedor;
            }

            foreach (var fila in _filasServicio)
            {
                if (fila.IdServicioSeleccionado == null)
                {
                    MessageBox.Show(
                        "Hay servicios sin seleccionar. Complétalos o elimínalos.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
            }

            var idsServicio = _filasServicio
                .Where(f => f.IdServicioSeleccionado != null)
                .Select(f => f.IdServicioSeleccionado.Value)
                .ToList();
            if (idsServicio.Count != idsServicio.Distinct().Count())
            {
                MessageBox.Show(
                    "Hay servicios repetidos. Cada servicio solo puede aparecer una vez.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            int? idCiudad = Convert.ToInt32(cmbCiudad.SelectedValue);

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
                }
                else
                {
                    await ConexionDB
                        .Client.From<Proveedor>()
                        .Insert(
                            new Proveedor
                            {
                                NitProveedor = nit,
                                NombreComercial = txtNombreComercial.Text.Trim(),
                                TelefonoComercial = txtTelefonoComercial.Text.Trim(),
                                IdCiudad = idCiudad,
                                EstadoProveedor = (
                                    (ComboBoxItem)cmbEstadoProveedor.SelectedItem
                                ).Content.ToString(),
                            }
                        );
                }

                // Reemplazar servicios
                await ConexionDB
                    .Client.From<ProveedorServicio>()
                    .Filter(
                        "id_proveedor",
                        Supabase.Postgrest.Constants.Operator.Equals,
                        nit.ToString()
                    )
                    .Delete();

                foreach (var fila in _filasServicio)
                {
                    if (fila.IdServicioSeleccionado == null)
                        continue;
                    await ConexionDB
                        .Client.From<ProveedorServicio>()
                        .Insert(
                            new ProveedorServicio
                            {
                                IdProveedor = nit,
                                IdServicio = fila.IdServicioSeleccionado.Value,
                                Precio = 0,
                            }
                        );
                }

                // Reemplazar ítems de detalle
                await ConexionDB
                    .Client.From<DetalleProveedorServicio>()
                    .Filter(
                        "id_proveedor",
                        Supabase.Postgrest.Constants.Operator.Equals,
                        nit.ToString()
                    )
                    .Delete();

                foreach (var fila in _filasServicio)
                {
                    if (fila.IdServicioSeleccionado == null)
                        continue;
                    foreach (var item in fila.Items)
                    {
                        if (string.IsNullOrWhiteSpace(item.Nombre))
                            continue;
                        await ConexionDB
                            .Client.From<DetalleProveedorServicio>()
                            .Insert(
                                new DetalleProveedorServicio
                                {
                                    IdProveedor = nit,
                                    IdServicio = fila.IdServicioSeleccionado.Value,
                                    NombreItem = item.Nombre.Trim(),
                                    PrecioItem = item.Precio,
                                }
                            );
                    }
                }

                await CargarDatos();
                ConstruirCatalogo(txtBuscarCatalogo.Text?.Trim() ?? "");
                ConstruirResumen(txtBuscarResumen.Text?.Trim() ?? "");
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
                    $"Error al guardar: {ex.Message}",
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
            txtNitProveedor.IsEnabled = false;
            txtNombreComercial.Text = _proveedorSeleccionado.NombreComercial;
            txtTelefonoComercial.Text = _proveedorSeleccionado.TelefonoComercial;
            cmbCiudad.SelectedValue = _proveedorSeleccionado.IdCiudad;

            foreach (ComboBoxItem item in cmbEstadoProveedor.Items)
                if (item.Content.ToString() == _proveedorSeleccionado.EstadoProveedor)
                {
                    cmbEstadoProveedor.SelectedItem = item;
                    break;
                }

            _filasServicio.Clear();
            panelServiciosProveedor.Children.Clear();

            var serviciosActuales = _proveedorServicios
                ?.Where(ps => ps.IdProveedor == _proveedorSeleccionado.NitProveedor)
                .ToList();

            foreach (var ps in serviciosActuales ?? new List<ProveedorServicio>())
            {
                var items = (_detalles ?? new List<DetalleProveedorServicio>())
                    .Where(d => d.IdProveedor == ps.IdProveedor && d.IdServicio == ps.IdServicio)
                    .Select(d => (d.NombreItem, d.PrecioItem))
                    .ToList();
                AgregarFilaServicio(ps.IdServicio, items);
            }

            HabilitarFormulario(true);
            txtNitProveedor.IsEnabled = false;
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

            if (
                MessageBox.Show(
                    $"¿Deshabilitar a '{_proveedorSeleccionado.NombreComercial}'?",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) != MessageBoxResult.Yes
            )
                return;

            try
            {
                _proveedorSeleccionado.EstadoProveedor = "Inactivo";
                await ConexionDB.Client.From<Proveedor>().Update(_proveedorSeleccionado);
                await CargarDatos();
                ConstruirCatalogo(txtBuscarCatalogo.Text?.Trim() ?? "");
                ConstruirResumen(txtBuscarResumen.Text?.Trim() ?? "");
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
            txtNitProveedor.IsEnabled = true;
            txtNombreComercial.Clear();
            txtTelefonoComercial.Clear();
            cmbCiudad.SelectedIndex = -1;
            cmbEstadoProveedor.SelectedIndex = 0;
            _filasServicio.Clear();
            panelServiciosProveedor.Children.Clear();
        }

        private void HabilitarFormulario(bool habilitar)
        {
            campos.IsEnabled = habilitar;
            dgProveedores.IsEnabled = !habilitar;
        }
    }

    // ─── FILA DE SERVICIO (con ítems dinámicos) ───────────────────────────────

    public class FilaServicioProveedor
    {
        public Border Panel { get; }
        public Action OnEliminar { get; set; }

        public int? IdServicioSeleccionado { get; private set; }

        private readonly ComboBox _cmbServicio;
        private readonly StackPanel _panelItems;
        private readonly List<FilaItemServicio> _items = new List<FilaItemServicio>();

        public List<(string Nombre, long Precio)> Items =>
            _items.Select(i => (i.NombreItem, i.PrecioItem)).ToList();

        public FilaServicioProveedor(
            List<Servicio> servicios,
            int? idServicio = null,
            List<(string Nombre, long Precio)> itemsExistentes = null
        )
        {
            _cmbServicio = new ComboBox
            {
                ItemsSource = servicios,
                DisplayMemberPath = "NombreServicio",
                SelectedValuePath = "IdServicio",
                MinWidth = 200,
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 10, 0),
            };

            if (idServicio.HasValue)
                _cmbServicio.SelectedValue = idServicio.Value;

            _cmbServicio.SelectionChanged += (s, e) =>
                IdServicioSeleccionado =
                    _cmbServicio.SelectedValue != null ? (int?)_cmbServicio.SelectedValue : null;

            var btnAgregarItem = new Button
            {
                Content = "+ Ítem",
                Height = 28,
                Padding = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
            };
            btnAgregarItem.Click += (s, e) => AgregarItem("", 0);

            var btnEliminar = new Button
            {
                Content = "✕ Eliminar servicio",
                Height = 28,
                Padding = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Cursor = System.Windows.Input.Cursors.Hand,
            };
            btnEliminar.Click += (s, e) => OnEliminar?.Invoke();

            // Fila superior: selector de servicio + botones
            var filaTop = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 6),
            };
            filaTop.Children.Add(
                new TextBlock
                {
                    Text = "Servicio:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0),
                    FontSize = 12,
                }
            );
            filaTop.Children.Add(_cmbServicio);
            filaTop.Children.Add(btnAgregarItem);
            filaTop.Children.Add(btnEliminar);

            // Panel donde van los ítems
            _panelItems = new StackPanel { Margin = new Thickness(20, 0, 0, 4) };

            var contenedor = new StackPanel();
            contenedor.Children.Add(filaTop);
            contenedor.Children.Add(_panelItems);

            Panel = new Border
            {
                Child = contenedor,
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(0, 0, 0, 8),
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
            };

            // Cargar ítems existentes si los hay
            if (itemsExistentes != null)
                foreach (var item in itemsExistentes)
                    AgregarItem(item.Nombre, item.Precio);
        }

        private void AgregarItem(string nombre, long precio)
        {
            var fila = new FilaItemServicio(nombre, precio);
            fila.OnEliminar = () =>
            {
                _items.Remove(fila);
                _panelItems.Children.Remove(fila.Panel);
            };
            _items.Add(fila);
            _panelItems.Children.Add(fila.Panel);
        }
    }

    // ─── FILA DE ÍTEM (nombre + precio individual) ───────────────────────────

    public class FilaItemServicio
    {
        public StackPanel Panel { get; }
        public Action OnEliminar { get; set; }

        private readonly TextBox _txtNombre;
        private readonly TextBox _txtPrecio;

        public string NombreItem => _txtNombre.Text.Trim();
        public long PrecioItem
        {
            get
            {
                long.TryParse(_txtPrecio.Text, out long p);
                return p;
            }
        }

        public FilaItemServicio(string nombre = "", long precio = 0)
        {
            _txtNombre = new TextBox
            {
                Text = nombre,
                Width = 160,
                Padding = new Thickness(4),
                Margin = new Thickness(0, 0, 6, 0),
            };

            _txtPrecio = new TextBox
            {
                Text = precio > 0 ? precio.ToString() : "",
                Width = 90,
                Padding = new Thickness(4),
                Margin = new Thickness(0, 0, 6, 0),
            };

            var btnEliminar = new Button
            {
                Content = "✕",
                Width = 26,
                Height = 26,
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Cursor = System.Windows.Input.Cursors.Hand,
            };
            btnEliminar.Click += (s, e) => OnEliminar?.Invoke();

            Panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 3, 0, 3),
            };
            Panel.Children.Add(
                new TextBlock
                {
                    Text = "•",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                }
            );
            Panel.Children.Add(_txtNombre);
            Panel.Children.Add(
                new TextBlock
                {
                    Text = "Bs",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 4, 0),
                    FontSize = 12,
                }
            );
            Panel.Children.Add(_txtPrecio);
            Panel.Children.Add(btnEliminar);
        }
    }
}
