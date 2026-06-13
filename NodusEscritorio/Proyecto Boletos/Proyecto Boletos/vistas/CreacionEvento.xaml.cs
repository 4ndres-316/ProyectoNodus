using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class CreacionEvento : Window
    {
        private int _idUsuario;
        private string _nombreUsuario;
        private List<Recinto> _recintos;
        private List<Servicio> _servicios;
        private List<Categoria> _categorias;
        private List<Proveedor> _proveedores;
        private List<ProveedorServicio> _proveedorServicios;
        private List<MetodoPago> _metodosPago;

        private List<FilaServicio> _filasServicio = new List<FilaServicio>();
        private List<FilaBoleto> _filasBoleto = new List<FilaBoleto>();

        public CreacionEvento(int idUsuario, string nombreUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
            _nombreUsuario = nombreUsuario;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                _recintos = (await ConexionDB.Client.From<Recinto>().Get()).Models;
                cmbRecinto.ItemsSource = _recintos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar recintos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                _categorias = (await ConexionDB.Client.From<Categoria>().Get()).Models;
                _servicios = (await ConexionDB.Client.From<Servicio>().Get()).Models;

                // Cruzar nombre de categoría en cada servicio
                foreach (var s in _servicios)
                {
                    var cat = _categorias.Find(c => c.IdCategoria == s.IdCategoria);
                    s.NombreCategoria = cat?.NombreCategoria ?? s.IdCategoria.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar servicios: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                _proveedores = (await ConexionDB.Client.From<Proveedor>().Get()).Models;
                _proveedorServicios = (await ConexionDB.Client.From<ProveedorServicio>().Get()).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                _metodosPago = (await ConexionDB.Client.From<MetodoPago>()
                    .Filter("estado", Supabase.Postgrest.Constants.Operator.Equals, "activo")
                    .Get()).Models;

                if (_metodosPago == null || _metodosPago.Count == 0)
                    _metodosPago = (await ConexionDB.Client.From<MetodoPago>().Get()).Models;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar métodos de pago: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─── DISPONIBILIDAD ──────────────────────────────────────────────────

        private async void cmbRecinto_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => await VerificarDisponibilidad();

        private async void Fecha_Changed(object sender, SelectionChangedEventArgs e)
            => await VerificarDisponibilidad();

        private async void Fecha_TextChanged(object sender, TextChangedEventArgs e)
            => await VerificarDisponibilidad();

        private async Task VerificarDisponibilidad()
        {
            if (cmbRecinto.SelectedValue == null ||
                dpFechaInicio.SelectedDate == null ||
                dpFechaFin.SelectedDate == null ||
                !TimeSpan.TryParse(txtHoraInicio.Text, out TimeSpan horaInicio) ||
                !TimeSpan.TryParse(txtHoraFin.Text, out TimeSpan horaFin))
            {
                txtDisponibilidad.Text = "Selecciona recinto y fechas";
                bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(236, 236, 236));
                txtDisponibilidad.Foreground = Brushes.Gray;
                return;
            }

            int idRecinto = (int)cmbRecinto.SelectedValue;
            var inicioSolicitado = dpFechaInicio.SelectedDate.Value.Date + horaInicio;
            var finSolicitado = dpFechaFin.SelectedDate.Value.Date + horaFin;

            if (finSolicitado <= inicioSolicitado)
            {
                txtDisponibilidad.Text = "La fecha fin debe ser posterior al inicio";
                bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));
                txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                return;
            }

            try
            {
                var eventosRecinto = (await ConexionDB.Client.From<Evento>()
                    .Filter("id_recinto", Supabase.Postgrest.Constants.Operator.Equals, idRecinto.ToString())
                    .Get()).Models;

                var fechasIds = eventosRecinto.Select(ev => ev.IdFechaEvento).Distinct().ToList();
                var fechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models
                                .Where(f => fechasIds.Contains(f.Id)).ToList();

                bool ocupado = fechas.Any(f =>
                {
                    var ini = f.FechaInicio.Date + f.HoraInicio;
                    var fin = f.FechaFin.Date + f.HoraFin;
                    return ini < finSolicitado && fin > inicioSolicitado;
                });

                if (ocupado)
                {
                    txtDisponibilidad.Text = "⚠ Recinto ocupado en ese horario";
                    bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));
                    txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                }
                else
                {
                    txtDisponibilidad.Text = "✔ Recinto disponible";
                    bdDisponibilidad.Background = new SolidColorBrush(Color.FromRgb(232, 245, 238));
                    txtDisponibilidad.Foreground = new SolidColorBrush(Color.FromRgb(30, 130, 80));
                }
            }
            catch { }
        }

        // ─── ES PÚBLICO ──────────────────────────────────────────────────────

        private void chkEsPublico_Changed(object sender, RoutedEventArgs e)
        {
            if (seccionBoletos == null) return;
            seccionBoletos.Visibility = chkEsPublico.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ─── SERVICIOS DINÁMICOS ─────────────────────────────────────────────

        private void btnAgregarServicio_Click(object sender, RoutedEventArgs e)
        {
            if (_servicios == null || _proveedores == null || _proveedorServicios == null)
            {
                MessageBox.Show("Los datos aún están cargando, espera un momento.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fila = new FilaServicio(_servicios, _proveedores, _proveedorServicios);
            fila.OnEliminar = () =>
            {
                _filasServicio.Remove(fila);
                panelServicios.Children.Remove(fila.Panel);
            };
            _filasServicio.Add(fila);
            panelServicios.Children.Add(fila.Panel);
        }

        // ─── BOLETOS DINÁMICOS ───────────────────────────────────────────────

        private void btnAgregarBoleto_Click(object sender, RoutedEventArgs e)
        {
            var fila = new FilaBoleto();
            fila.OnEliminar = () =>
            {
                _filasBoleto.Remove(fila);
                panelBoletos.Children.Remove(fila.Panel);
            };
            _filasBoleto.Add(fila);
            panelBoletos.Children.Add(fila.Panel);
        }

        // ─── GUARDAR ─────────────────────────────────────────────────────────

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreEvento.Text))
            { MessageBox.Show("El nombre del evento es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (cmbRecinto.SelectedValue == null)
            { MessageBox.Show("Selecciona un recinto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            { MessageBox.Show("Las fechas son obligatorias.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (!TimeSpan.TryParse(txtHoraInicio.Text, out TimeSpan horaInicio) ||
                !TimeSpan.TryParse(txtHoraFin.Text, out TimeSpan horaFin))
            { MessageBox.Show("Escribe horas válidas (Ej: 18:30).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (string.IsNullOrWhiteSpace(txtNombreReservante.Text))
            { MessageBox.Show("El nombre reservante es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            var ventanaPago = new PagoEventoDialog(_metodosPago);
            ventanaPago.Owner = this;
            ventanaPago.ShowDialog();

            if (ventanaPago.Resultado == ResultadoPago.Cancelado) return;

            btnGuardar.IsEnabled = false;
            txtEstadoGuardado.Text = "Guardando...";

            try
            {
                // 1. FechaEvento
                var fechaEvento = new FechaEvento
                {
                    FechaInicio = dpFechaInicio.SelectedDate.Value,
                    FechaFin = dpFechaFin.SelectedDate.Value,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin
                };
                var fechaResp = await ConexionDB.Client.From<FechaEvento>().Insert(fechaEvento);
                var fechaInserta = fechaResp.Models.First();

                // 2. Evento
                var evento = new Evento
                {
                    IdOrganizador = _idUsuario,
                    IdRecinto = (int)cmbRecinto.SelectedValue,
                    IdFechaEvento = fechaInserta.Id,
                    NombreEvento = txtNombreEvento.Text.Trim(),
                    Categoria = ((ComboBoxItem)cmbCategoria.SelectedItem).Content.ToString(),
                    EstadoEvento = "Programado",
                    NombreReservante = txtNombreReservante.Text.Trim(),
                    ImagenUrl = txtImagenUrl.Text.Trim(),
                    EsPublico = chkEsPublico.IsChecked == true
                };
                var eventoResp = await ConexionDB.Client.From<Evento>().Insert(evento);
                var eventoInserto = eventoResp.Models.First();
                int idEvento = eventoInserto.IdEvento;

                // 3. EventoServicios
                foreach (var fila in _filasServicio)
                {
                    if (fila.IdServicioSeleccionado == null) continue;
                    await ConexionDB.Client.From<EventoServicio>().Insert(new EventoServicio
                    {
                        IdEvento = idEvento,
                        IdServicio = fila.IdServicioSeleccionado.Value,
                        Cantidad = fila.Cantidad,
                        PrecioAcordado = fila.PrecioAcordado,
                        EstadoEventoServicio = "pendiente"
                    });
                }

                // 4. TipoBoletos
                foreach (var fila in _filasBoleto)
                {
                    if (string.IsNullOrWhiteSpace(fila.NombreTipo)) continue;
                    await ConexionDB.Client.From<TipoBoleto>().Insert(new TipoBoleto
                    {
                        IdEvento = idEvento,
                        NombreTipoBoleto = fila.NombreTipo,
                        Precio = fila.Precio,
                        CantidadTotal = fila.CantidadTotal,
                        CantidadDisponible = fila.CantidadTotal,
                        Descripcion = fila.Descripcion,
                        UrlImagen = fila.ImagenUrl
                    });
                }

                // 5. Pago opcional
                if (ventanaPago.Resultado == ResultadoPago.Pagar)
                {
                    var ordenResp = await ConexionDB.Client.From<Orden>().Insert(new Orden
                    {
                        IdUsuario = _idUsuario,
                        FechaOrden = DateTime.Now,
                        EstadoOrden = "pendiente",
                        DescuentoOrden = 0
                    });
                    var ordenInserta = ordenResp.Models.First();

                    await ConexionDB.Client.From<Pago>().Insert(new Pago
                    {
                        IdOrden = ordenInserta.IdOrden,
                        IdMetodoPago = ventanaPago.IdMetodoPagoSeleccionado,
                        MontoPago = ventanaPago.Monto,
                        Moneda = "BOB",
                        FechaPago = DateTime.Now,
                        EstadoPago = "pendiente",
                        ReferenciaPago = ventanaPago.Nota
                    });
                }

                MessageBox.Show("Evento creado exitosamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
                txtEstadoGuardado.Text = "";
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Seguro que deseas cancelar? Se perderán los datos.",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                this.Close();
        }
    }

    // ─── FILA SERVICIO ────────────────────────────────────────────────────────────

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

        private ComboBox _cmbCategoria;
        private ComboBox _cmbServicio;
        private ComboBox _cmbProveedor;
        private TextBox _txtCantidad;
        private TextBox _txtPrecio;
        private TextBlock _txtInfoProveedor;

        public FilaServicio(List<Servicio> servicios, List<Proveedor> proveedores,
                            List<ProveedorServicio> proveedorServicios)
        {
            _servicios = servicios;
            _proveedores = proveedores;
            _proveedorServicios = proveedorServicios;

            // Agrupar por NombreCategoria (ya cruzado en CargarDatos)
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
            Lbl("Servicio:"); fila.Children.Add(_cmbServicio);
            Lbl("Proveedor:"); fila.Children.Add(_cmbProveedor);
            fila.Children.Add(_txtInfoProveedor);
            Lbl("Cant:"); fila.Children.Add(_txtCantidad);
            Lbl("Precio Bs:"); fila.Children.Add(_txtPrecio);
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

            // Filtrar por NombreCategoria
            _cmbServicio.ItemsSource = _servicios.Where(s => s.NombreCategoria == categoria).ToList();
            _cmbServicio.IsEnabled = true;
            _cmbServicio.SelectedIndex = -1;
            _cmbProveedor.ItemsSource = null;
            _cmbProveedor.IsEnabled = false;
            _txtInfoProveedor.Text = "";
            IdServicioSeleccionado = null;
        }

        private void CmbServicio_Changed(object sender, SelectionChangedEventArgs e)
        {
            var servicio = _cmbServicio.SelectedItem as Servicio;
            if (servicio == null) return;

            IdServicioSeleccionado = servicio.IdServicio;

            var nitsProveedores = _proveedorServicios
                .Where(ps => ps.IdServicio == servicio.IdServicio)
                .Select(ps => ps.IdProveedor).ToList();

            var proveedoresFiltrados = _proveedores
                .Where(p => nitsProveedores.Contains(int.Parse(p.NitProveedor))).ToList();

            _cmbProveedor.ItemsSource = proveedoresFiltrados;
            _cmbProveedor.IsEnabled = proveedoresFiltrados.Count > 0;
            _cmbProveedor.SelectedIndex = -1;
            _txtInfoProveedor.Text = "";
        }

        private void CmbProveedor_Changed(object sender, SelectionChangedEventArgs e)
        {
            var proveedor = _cmbProveedor.SelectedItem as Proveedor;
            if (proveedor == null) return;

            // Precio viene de proveedor_servicio
            var ps = _proveedorServicios.Find(x =>
                x.IdProveedor == int.Parse(proveedor.NitProveedor) &&
                x.IdServicio == IdServicioSeleccionado);

            long precio = ps != null ? ps.Precio : 0;

            PrecioAcordado = (int)precio;
            _txtPrecio.Text = precio.ToString();
            _txtInfoProveedor.Text = $"Bs {precio}";
        }
    }

    // ─── FILA BOLETO ──────────────────────────────────────────────────────────────

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
            var txtNombre = new TextBox { Width = 130, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtPrecio = new TextBox { Width = 80, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtCantidad = new TextBox { Width = 70, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtDesc = new TextBox { Width = 150, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };
            var txtImagen = new TextBox { Width = 120, Padding = new Thickness(5), Margin = new Thickness(0, 0, 8, 0) };

            txtNombre.TextChanged += (s, e) => NombreTipo = txtNombre.Text;
            txtPrecio.TextChanged += (s, e) => { if (decimal.TryParse(txtPrecio.Text, out decimal p)) Precio = p; };
            txtCantidad.TextChanged += (s, e) => { if (int.TryParse(txtCantidad.Text, out int c)) CantidadTotal = c; };
            txtDesc.TextChanged += (s, e) => Descripcion = txtDesc.Text;
            txtImagen.TextChanged += (s, e) => ImagenUrl = txtImagen.Text;

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

            Lbl("Nombre:"); fila.Children.Add(txtNombre);
            Lbl("Precio Bs:"); fila.Children.Add(txtPrecio);
            Lbl("Cantidad:"); fila.Children.Add(txtCantidad);
            Lbl("Descripción:"); fila.Children.Add(txtDesc);
            Lbl("Imagen URL:"); fila.Children.Add(txtImagen);
            fila.Children.Add(btnEliminar);

            Panel = new Border
            {
                Child = fila,
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 4, 0, 4)
            };
        }
    }
}