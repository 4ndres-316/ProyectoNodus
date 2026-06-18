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
    public partial class ReportesView : UserControl
    {
        private List<EventoReporteVista> _reporteActual;

        public ReportesView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dpFechaInicio.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dpFechaFin.SelectedDate = DateTime.Today;
        }

        private async void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Selecciona un rango de fechas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime inicio = dpFechaInicio.SelectedDate.Value.Date;
            DateTime fin = dpFechaFin.SelectedDate.Value.Date;

            if (inicio > fin)
            {
                MessageBox.Show("La fecha inicio no puede ser mayor a la fecha fin.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await GenerarReporte(inicio, fin);
        }

        private async Task GenerarReporte(DateTime inicio, DateTime fin)
        {
            pnlPlaceholder.Visibility = Visibility.Collapsed;
            txtSinDatos.Visibility = Visibility.Collapsed;
            txtCargando.Visibility = Visibility.Visible;
            panelStats.Visibility = Visibility.Collapsed;
            dgReportes.ItemsSource = null;
            btnExportar.IsEnabled = false;

            try
            {
                var todasFechas = (await ConexionDB.Client.From<FechaEvento>().Get()).Models;
                var fechasFiltradas = todasFechas
                    .Where(f => f.FechaInicio.Date >= inicio && f.FechaInicio.Date <= fin)
                    .ToList();

                var idsFechas = new HashSet<int>(fechasFiltradas.Select(f => f.Id));

                var todosEventos = (await ConexionDB.Client.From<Evento>().Get()).Models;
                var eventosFiltrados = todosEventos
                    .Where(ev => idsFechas.Contains(ev.IdFechaEvento))
                    .ToList();

                string estadoSel = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
                if (estadoSel != "Todos")
                    eventosFiltrados = eventosFiltrados
                        .Where(ev => string.Equals(ev.EstadoEvento, estadoSel, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                string categoriaSel = (cmbCategoria.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas";
                if (categoriaSel != "Todas")
                    eventosFiltrados = eventosFiltrados
                        .Where(ev => string.Equals(ev.Categoria, categoriaSel, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                if (eventosFiltrados.Count == 0)
                {
                    txtCargando.Visibility = Visibility.Collapsed;
                    txtSinDatos.Visibility = Visibility.Visible;
                    return;
                }

                var usuarios = (await ConexionDB.Client.From<Usuario>().Get()).Models;
                var usuarioMap = usuarios.ToDictionary(u => u.IdUsuario, u => u.NombreUsuario);

                var idsEventos = new HashSet<int>(eventosFiltrados.Select(ev => ev.IdEvento));

                var todosTipos = (await ConexionDB.Client.From<TipoBoleto>().Get()).Models;
                var tiposDelRango = todosTipos.Where(t => idsEventos.Contains(t.IdEvento)).ToList();
                var tiposPorEvento = tiposDelRango
                    .GroupBy(t => t.IdEvento)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var idsTipos = new HashSet<int>(tiposDelRango.Select(t => t.IdTipoBoleto));

                var todosDetalles = (await ConexionDB.Client.From<DetalleOrden>().Get()).Models;
                var detallesDelRango = todosDetalles.Where(d => idsTipos.Contains(d.IdTipoBoleto)).ToList();

                var todasOrdenes = (await ConexionDB.Client.From<Orden>().Get()).Models;
                var ordenesPagadas = new HashSet<int>(
                    todasOrdenes
                        .Where(o => string.Equals(o.EstadoOrden, "pagado", StringComparison.OrdinalIgnoreCase))
                        .Select(o => o.IdOrden)
                );

                var detallesPorTipo = detallesDelRango
                    .GroupBy(d => d.IdTipoBoleto)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var reporteRows = new List<EventoReporteVista>();
                foreach (var ev in eventosFiltrados)
                {
                    var fecha = fechasFiltradas.FirstOrDefault(f => f.Id == ev.IdFechaEvento);
                    string organizador = usuarioMap.TryGetValue(ev.IdOrganizador, out var nom) ? nom : $"ID {ev.IdOrganizador}";

                    int boletosVendidos = 0;
                    decimal totalRecaudado = 0;

                    if (tiposPorEvento.TryGetValue(ev.IdEvento, out var tipos))
                    {
                        foreach (var tipo in tipos)
                        {
                            if (detallesPorTipo.TryGetValue(tipo.IdTipoBoleto, out var detalles))
                            {
                                foreach (var det in detalles.Where(d => ordenesPagadas.Contains(d.IdOrden)))
                                {
                                    boletosVendidos += det.Cantidad;
                                    totalRecaudado += det.PrecioUnitario * det.Cantidad;
                                }
                            }
                        }
                    }

                    reporteRows.Add(new EventoReporteVista
                    {
                        IdEvento = ev.IdEvento,
                        NombreEvento = ev.NombreEvento,
                        Organizador = organizador,
                        Categoria = ev.Categoria,
                        FechaEvento = fecha != null ? fecha.FechaInicio.ToString("dd/MM/yyyy") : "—",
                        Estado = ev.EstadoEvento,
                        BoletosVendidos = boletosVendidos,
                        TotalRecaudado = totalRecaudado,
                        Asistentes = boletosVendidos
                    });
                }

                txtStatEventos.Text = reporteRows.Count.ToString();
                txtStatBoletos.Text = reporteRows.Sum(r => r.BoletosVendidos).ToString();
                txtStatRecaudado.Text = $"Bs {reporteRows.Sum(r => r.TotalRecaudado):N2}";
                txtStatCancelados.Text = reporteRows
                    .Count(r => string.Equals(r.Estado, "Cancelado", StringComparison.OrdinalIgnoreCase))
                    .ToString();

                _reporteActual = reporteRows;
                dgReportes.ItemsSource = _reporteActual;

                txtCargando.Visibility = Visibility.Collapsed;
                panelStats.Visibility = Visibility.Visible;
                btnExportar.IsEnabled = true;
            }
            catch (Exception ex)
            {
                txtCargando.Visibility = Visibility.Collapsed;
                pnlPlaceholder.Visibility = Visibility.Visible;
                MessageBox.Show($"Error al generar el reporte:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (_reporteActual == null || _reporteActual.Count == 0) return;

            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() != true) return;

            var panel = ConstruirPanelImpresion();
            panel.Measure(new Size(dlg.PrintableAreaWidth, double.PositiveInfinity));
            panel.Arrange(new Rect(new Size(dlg.PrintableAreaWidth, panel.DesiredSize.Height)));
            panel.UpdateLayout();

            double scaleX = dlg.PrintableAreaWidth / Math.Max(panel.ActualWidth, 1);
            double scale = Math.Min(scaleX, 1.0);

            var visual = new System.Windows.Media.DrawingVisual();
            using (var ctx = visual.RenderOpen())
            {
                var brush = new VisualBrush(panel);
                ctx.DrawRectangle(
                    brush, null,
                    new Rect(0, 0, panel.ActualWidth * scale, panel.ActualHeight * scale)
                );
            }

            dlg.PrintVisual(visual, $"Reporte Nodus — {DateTime.Now:dd/MM/yyyy}");
        }

        private StackPanel ConstruirPanelImpresion()
        {
            var panel = new StackPanel { Background = Brushes.White, Width = 720, Margin = new Thickness(20) };

            panel.Children.Add(new TextBlock
            {
                Text = "NODUS — Reporte de Eventos",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            });

            string inicio = dpFechaInicio.SelectedDate?.ToString("dd/MM/yyyy") ?? "—";
            string fin = dpFechaFin.SelectedDate?.ToString("dd/MM/yyyy") ?? "—";
            string estadoSel = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
            panel.Children.Add(new TextBlock
            {
                Text = $"Período: {inicio} — {fin}  |  Estado: {estadoSel}  |  Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                FontSize = 10,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 14)
            });

            var statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            for (int i = 0; i < 4; i++)
                statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

            AgregarStatImpresion(statsGrid, 0, txtStatEventos.Text, "Eventos");
            AgregarStatImpresion(statsGrid, 1, txtStatBoletos.Text, "Boletos vendidos");
            AgregarStatImpresion(statsGrid, 2, txtStatRecaudado.Text, "Recaudado");
            AgregarStatImpresion(statsGrid, 3, txtStatCancelados.Text, "Cancelados");
            panel.Children.Add(statsGrid);

            panel.Children.Add(new Border { Height = 1, Background = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 6) });

            panel.Children.Add(ConstruirFilaTabla(
                new[] { "Nombre Evento", "Organizador", "Categoría", "Fecha", "Estado", "Boletos", "Recaudado" },
                isHeader: true));

            foreach (var row in _reporteActual)
            {
                panel.Children.Add(ConstruirFilaTabla(new[]
                {
                    row.NombreEvento,
                    row.Organizador,
                    row.Categoria,
                    row.FechaEvento,
                    row.Estado,
                    row.BoletosVendidos.ToString(),
                    row.TotalRecaudadoFormateado
                }, isHeader: false));
            }

            return panel;
        }

        private void AgregarStatImpresion(Grid grid, int col, string valor, string etiqueta)
        {
            var sp = new StackPanel { Margin = new Thickness(4, 0, 4, 0) };
            sp.Children.Add(new TextBlock { Text = valor, FontSize = 15, FontWeight = FontWeights.Bold });
            sp.Children.Add(new TextBlock { Text = etiqueta, FontSize = 9, Foreground = Brushes.Gray });
            Grid.SetColumn(sp, col);
            grid.Children.Add(sp);
        }

        private Border ConstruirFilaTabla(string[] celdas, bool isHeader)
        {
            double[] widths = { 165, 100, 100, 70, 70, 55, 80 };
            var grid = new Grid { Background = isHeader ? Brushes.LightGray : Brushes.White };
            foreach (var w in widths)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });

            for (int i = 0; i < celdas.Length && i < widths.Length; i++)
            {
                var tb = new TextBlock
                {
                    Text = celdas[i],
                    FontSize = isHeader ? 9 : 8,
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                    Padding = new Thickness(4, 3, 4, 3),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(tb, i);
                grid.Children.Add(tb);
            }

            return new Border
            {
                Child = grid,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
        }
    }

    public class EventoReporteVista
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; }
        public string Organizador { get; set; }
        public string Categoria { get; set; }
        public string FechaEvento { get; set; }
        public string Estado { get; set; }
        public int BoletosVendidos { get; set; }
        public decimal TotalRecaudado { get; set; }
        public string TotalRecaudadoFormateado => $"Bs {TotalRecaudado:N2}";
        public int Asistentes { get; set; }
    }
}
