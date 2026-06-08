using System;
using System.Windows;
using Proyecto_Boletos.Db;

namespace Proyecto_Boletos.vistas
{
    public partial class ProveedoresWebView : Window
    {
        public ProveedoresWebView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                try
                {
                    var response = await ConexionDB.Client.From<Proveedor>().Get();
                    dgProveedoresWeb.ItemsSource = response.Models;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar proveedores web: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}