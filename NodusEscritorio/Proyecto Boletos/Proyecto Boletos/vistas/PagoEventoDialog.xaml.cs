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

        private List<MetodoPago> _metodosPago;
        private MetodoPago _metodoPagoActivo;
        private List<LineaDesglose> _desglose;

        // Constructor para reserva — solo costo del recinto
        public PagoEventoDialog(List<MetodoPago> metodosPago, decimal costoRecinto)
        {
            InitializeComponent();
            _metodosPago = metodosPago ?? new List<MetodoPago>();

            _desglose = new List<LineaDesglose>
            {
                new LineaDesglose { Concepto = "Costo Recinto", Monto = costoRecinto },
            };

            CargarMetodosPago();
        }

        // Constructor para evento — costo recinto + servicios
        public PagoEventoDialog(
            List<MetodoPago> metodosPago,
            decimal costoRecinto,
            List<LineaDesglose> servicios
        )
        {
            InitializeComponent();
            _metodosPago = metodosPago ?? new List<MetodoPago>();

            _desglose = new List<LineaDesglose>
            {
                new LineaDesglose { Concepto = "Costo Recinto", Monto = costoRecinto },
            };

            if (servicios != null)
                _desglose.AddRange(servicios);

            CargarMetodosPago();
        }

        // Constructor original sin desglose (para compatibilidad)
        public PagoEventoDialog(List<MetodoPago> metodosPago)
        {
            InitializeComponent();
            _metodosPago = metodosPago ?? new List<MetodoPago>();
            CargarMetodosPago();
        }

        private void CargarMetodosPago()
        {
            // Buscar métodos de pago Efectivo y QR entre los disponibles
            var efectivo = _metodosPago.FirstOrDefault(m =>
                m.Nombre.IndexOf("Efectivo", StringComparison.OrdinalIgnoreCase) >= 0
            );
            var qr = _metodosPago.FirstOrDefault(m =>
                m.Nombre.IndexOf("QR", StringComparison.OrdinalIgnoreCase) >= 0
                || m.Nombre.IndexOf("Qr", StringComparison.OrdinalIgnoreCase) >= 0
            );

            // Si no hay métodos específicos, usar los primeros disponibles
            if (efectivo == null && _metodosPago.Count > 0)
                efectivo = _metodosPago[0];
            if (qr == null && _metodosPago.Count > 1)
                qr = _metodosPago[1];

            if (efectivo != null)
                btnEfectivo.Tag = efectivo;
            if (qr != null)
                btnQR.Tag = qr;

            // Ocultar botón QR si no existe
            if (qr == null)
                btnQR.Visibility = Visibility.Collapsed;
            if (efectivo == null)
                btnEfectivo.Visibility = Visibility.Collapsed;
        }

        private void btnEfectivo_Click(object sender, RoutedEventArgs e)
        {
            gridEfectivo.Visibility = Visibility.Visible;
            gridQR.Visibility = Visibility.Collapsed;

            var metodo = btnEfectivo.Tag as MetodoPago;
            _metodoPagoActivo = metodo;
        }

        private void btnQR_Click(object sender, RoutedEventArgs e)
        {
            gridQR.Visibility = Visibility.Visible;
            gridEfectivo.Visibility = Visibility.Collapsed;

            var metodo = btnQR.Tag as MetodoPago;
            _metodoPagoActivo = metodo;

            string codigoPago = $"Nodus | Monto: {Monto} BOB";
            GenerarQR(codigoPago);
        }

        private void btnSinPago_Click(object sender, RoutedEventArgs e)
        {
            Resultado = ResultadoPago.SinPago;
            this.DialogResult = false;
            this.Close();
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (_metodoPagoActivo == null)
            {
                MessageBox.Show(
                    "Selecciona un método de pago (Efectivo o QR).",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show(
                    "Ingresa un monto válido mayor a 0.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            Resultado = ResultadoPago.Pagar;
            IdMetodoPagoSeleccionado = _metodoPagoActivo.IdMetodoPago;
            Monto = monto;
            Nota = txtNota.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }

        // Permite que quién abre el diálogo pueda pre-llenar el monto
        public void SetMonto(decimal monto)
        {
            Monto = monto;
            txtMonto.Text = monto.ToString("F2");
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
