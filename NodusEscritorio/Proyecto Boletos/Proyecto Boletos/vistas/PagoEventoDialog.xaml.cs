using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Proyecto_Boletos.Db;

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
        public decimal Monto { get; private set; }
        public string Nota { get; private set; } = string.Empty;

        public PagoEventoDialog(List<MetodoPago> metodosPago)
        {
            InitializeComponent();
            cmbMetodoPago.ItemsSource = metodosPago;
            if (metodosPago?.Count > 0)
                cmbMetodoPago.SelectedIndex = 0;
        }

        private void btnSinPago_Click(object sender, RoutedEventArgs e)
        {
            Resultado = ResultadoPago.SinPago;
            this.Close();
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show(
                    "Ingresa un monto válido.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            if (cmbMetodoPago.SelectedValue == null)
            {
                MessageBox.Show(
                    "Selecciona un método de pago.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            Resultado = ResultadoPago.Pagar;
            IdMetodoPagoSeleccionado = (int)cmbMetodoPago.SelectedValue;
            Monto = monto;
            Nota = $"{txtNota.Text.Trim()} | Ref: {txtReferencia.Text.Trim()}";
            this.Close();
        }
    }
}
