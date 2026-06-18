using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Proyecto_Boletos.Db;
using QRCoder;

namespace Proyecto_Boletos.vistas
{
    public enum ResultadoPago
    {
        Cancelado,
        SinPago,
        Pagar,
    }

    public class LineaDesglose
    {
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string MontoFormateado => $"Bs {Monto:F2}";
    }

    public partial class PagoEventoDialog : Window
    {
        public ResultadoPago Resultado { get; private set; } = ResultadoPago.Cancelado;
        public int IdMetodoPagoSeleccionado { get; private set; }
        public decimal Monto { get; private set; }
        public string Nota { get; private set; } = string.Empty;

        public string ClienteNombre { get; private set; } = string.Empty;
        public string ClienteApellido { get; private set; } = string.Empty;
        public string ClienteNit { get; private set; } = string.Empty;
        public string ClienteCorreo { get; private set; } = string.Empty;
        public decimal Descuento { get; private set; }
        public decimal TotalFinal { get; private set; }

        private List<MetodoPago> _metodosPago;
        private MetodoPago _metodoPagoActivo;
        private List<LineaDesglose> _desglose;
        private decimal _subtotal;

        public PagoEventoDialog(List<MetodoPago> metodosPago, List<LineaDesglose> desglose = null, Usuario clientePreseleccionado = null)
        {
            InitializeComponent();
            _metodosPago = metodosPago ?? new List<MetodoPago>();
            _desglose = desglose ?? new List<LineaDesglose>();
            _subtotal = _desglose.Sum(d => d.Monto);
            CargarMetodosPago();
            CargarDesglose();
            RecalcularTotal();

            if (clientePreseleccionado != null)
            {
                var partes = clientePreseleccionado.NombreUsuario.Trim().Split(' ');
                txtClienteNombre.Text = partes[0];
                txtClienteApellido.Text = partes.Length > 1
                    ? string.Join(" ", partes, 1, partes.Length - 1)
                    : "";
                txtClienteCorreo.Text = clientePreseleccionado.EmailUsuario;
            }
        }

        public PagoEventoDialog(List<MetodoPago> metodosPago, decimal costoRecinto)
            : this(metodosPago, new List<LineaDesglose>
              {
                  new LineaDesglose { Concepto = "Costo Recinto", Monto = costoRecinto }
              }) { }

        public PagoEventoDialog(List<MetodoPago> metodosPago, decimal costoRecinto, List<LineaDesglose> servicios)
            : this(metodosPago, new List<LineaDesglose>(
                new[] { new LineaDesglose { Concepto = "Costo Recinto", Monto = costoRecinto } }
                .Concat(servicios ?? Enumerable.Empty<LineaDesglose>()))) { }

        private void CargarMetodosPago()
        {
            var efectivo = _metodosPago.FirstOrDefault(m =>
                m.Nombre.IndexOf("Efectivo", StringComparison.OrdinalIgnoreCase) >= 0);
            var qr = _metodosPago.FirstOrDefault(m =>
                m.Nombre.IndexOf("QR", StringComparison.OrdinalIgnoreCase) >= 0
                || m.Nombre.IndexOf("Qr", StringComparison.OrdinalIgnoreCase) >= 0);

            if (efectivo == null && _metodosPago.Count > 0) efectivo = _metodosPago[0];
            if (qr == null && _metodosPago.Count > 1) qr = _metodosPago[1];

            if (efectivo != null) btnEfectivo.Tag = efectivo;
            if (qr != null) btnQR.Tag = qr;

            if (qr == null) btnQR.Visibility = Visibility.Collapsed;
            if (efectivo == null) btnEfectivo.Visibility = Visibility.Collapsed;
        }

        private void CargarDesglose()
        {
            lvDesglose.ItemsSource = _desglose.Count > 0
                ? _desglose
                : new List<LineaDesglose> { new LineaDesglose { Concepto = "Pedido sin desglose", Monto = 0 } };
        }

        private void RecalcularTotal()
        {
            decimal honorario = Math.Round(_subtotal * 0.10m, 2);
            decimal desc = decimal.TryParse(txtDescuento?.Text, out decimal d) && d >= 0 ? d : 0;
            decimal total = _subtotal + honorario - desc;
            if (total < 0) total = 0;

            TotalFinal = total;

            if (txtSubtotal != null) txtSubtotal.Text = $"Bs {_subtotal:F2}";
            if (txtHonorario != null) txtHonorario.Text = $"Bs {honorario:F2}";
            if (txtTotalFinal != null) txtTotalFinal.Text = $"Bs {total:F2}";
            if (txtMonto != null) txtMonto.Text = total.ToString("F2");
        }

        private void txtDescuento_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalcularTotal();
        }

        private void btnEfectivo_Click(object sender, RoutedEventArgs e)
        {
            gridEfectivo.Visibility = Visibility.Visible;
            gridQR.Visibility = Visibility.Collapsed;
            _metodoPagoActivo = btnEfectivo.Tag as MetodoPago;
            txtMonto.Text = TotalFinal.ToString("F2");
        }

        private void btnQR_Click(object sender, RoutedEventArgs e)
        {
            gridQR.Visibility = Visibility.Visible;
            gridEfectivo.Visibility = Visibility.Collapsed;
            _metodoPagoActivo = btnQR.Tag as MetodoPago;
            GenerarQR($"Nodus | Monto: {TotalFinal:F2} BOB");
        }

        private void btnSinPago_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClienteNombre.Text))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClienteApellido.Text))
            {
                MessageBox.Show("El apellido del cliente es obligatorio.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Ingresa un monto valido mayor a 0.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Monto = monto;
            Nota = txtNota?.Text.Trim() ?? string.Empty;
            ClienteNombre = txtClienteNombre.Text.Trim();
            ClienteApellido = txtClienteApellido.Text.Trim();
            ClienteNit = txtClienteNit.Text.Trim();
            ClienteCorreo = txtClienteCorreo.Text.Trim();
            Descuento = decimal.TryParse(txtDescuento.Text, out decimal desc) ? desc : 0;
            Resultado = ResultadoPago.SinPago;
            this.DialogResult = false;
            this.Close();
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (_metodoPagoActivo == null)
            {
                MessageBox.Show("Selecciona un metodo de pago (Efectivo o QR).",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClienteNombre.Text))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClienteApellido.Text))
            {
                MessageBox.Show("El apellido del cliente es obligatorio.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClienteNit.Text))
            {
                MessageBox.Show("El NIT o carnet del cliente es obligatorio.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Ingresa un monto valido mayor a 0.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Resultado = ResultadoPago.Pagar;
            IdMetodoPagoSeleccionado = _metodoPagoActivo.IdMetodoPago;
            Monto = monto;
            Nota = txtNota?.Text.Trim() ?? string.Empty;
            ClienteNombre = txtClienteNombre.Text.Trim();
            ClienteApellido = txtClienteApellido.Text.Trim();
            ClienteNit = txtClienteNit.Text.Trim();
            ClienteCorreo = txtClienteCorreo.Text.Trim();
            Descuento = decimal.TryParse(txtDescuento.Text, out decimal desc) ? desc : 0;

            this.DialogResult = true;
            this.Close();
        }

        public void SetMonto(decimal monto)
        {
            Monto = monto;
            if (txtMonto != null) txtMonto.Text = monto.ToString("F2");
        }

        private void GenerarQR(string texto)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);

                BitmapImage bitmap = new BitmapImage();
                using (MemoryStream ms = new MemoryStream(qrBytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                }
                imgCodigoQR.Source = bitmap;
            }
            catch { }
        }
    }
}
