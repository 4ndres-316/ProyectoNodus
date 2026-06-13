using Proyecto_Boletos.Db;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Proyecto_Boletos.vistas
{
    /// <summary>
    /// Lógica de interacción para PagoEventoDialog.xaml
    /// </summary>
    public enum ResultadoPago
    {
        Cancelado,
        SinPago,
        Pagar,
    }

    public partial class PagoEventoDialog : Window
    {
        public ResultadoPago Resultado { get; private set; } = ResultadoPago.Cancelado;
        public int IdMetodoPagoSeleccionado { get; private set; }
        public int Monto { get; private set; }
        public string Nota { get; private set; } = string.Empty;

        public PagoEventoDialog(List<MetodoPago> metodosPago)
        {
            InitializeComponent();
        }

        private void btnEfectivo_Click(object sender, RoutedEventArgs e)
        {
            gridEfectivo.Visibility = Visibility.Visible;
            gridQR.Visibility = Visibility.Hidden;


        }

        private void btnQR_Click(object sender, RoutedEventArgs e)
        {
            gridQR.Visibility = Visibility.Visible;
            gridEfectivo.Visibility = Visibility.Hidden;

            string codigoPago = $"Evento:{Resultado} | Monto:{Monto}";

            GenerarQR(codigoPago);
        }

        private void btnSinPago_Click(object sender, RoutedEventArgs e)
        {
            Resultado = ResultadoPago.SinPago;
            this.Close();
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            /*

            Resultado = ResultadoPago.Pagar;
            //Monto = monto;
            Nota = $"{txtNota.Text.Trim()}";
            this.Close();*/
        }

        private void GenerarQR(string texto)
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
    }
}
