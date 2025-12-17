using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Pasteleria_Alemana
{
    public partial class FacturasPage : Page
    {
        // 🟢 Modelo de datos de Factura (basado en compras)
        public class Factura
        {
            public int idFactura { get; set; }
            public string fechaEmision { get; set; }  // ahora string
            public decimal total { get; set; }
            public string metodo_pago { get; set; }
            public string status { get; set; }
        }



        // 🟢 Respuesta de la API
        public class ApiResponse
        {
            public List<Factura> facturas { get; set; }
            public int total { get; set; }
            public int page { get; set; }
            public int limit { get; set; }
        }



        // 🟢 Cliente HTTP para API
        private readonly HttpClient _http = new HttpClient();
        private readonly string baseUrl = "http://localhost:3000/api/facturas";

        // 🟢 Lista observable de facturas
        private readonly ObservableCollection<Factura> _facturas = new();
        private ICollectionView _facturasView;

        // 🟢 Paginación
        private int currentPage = 1;
        private int totalFacturas = 0;
        private int limit = 5;

        public FacturasPage()
        {
            InitializeComponent();

            // Configurar vista filtrable
            _facturasView = CollectionViewSource.GetDefaultView(_facturas);
            FacturasGrid.ItemsSource = _facturasView;

            _ = CargarFacturas();
        }

        public class BoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool b)
                    return b ? Visibility.Visible : Visibility.Collapsed;
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        private async Task CargarFacturas()
        {
            try
            {
                var response = await _http.GetStringAsync($"{baseUrl}?page={currentPage}&limit={limit}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response);

                _facturas.Clear();
                foreach (var f in apiResponse.facturas)
                {
                    // Mantener el DateTime original para que no dé error
                    _facturas.Add(new Factura
                    {
                        idFactura = f.idFactura,
                        fechaEmision = f.fechaEmision,
                        total = f.total,
                        metodo_pago = f.metodo_pago,
                        status = f.status
                    });
                }

                totalFacturas = apiResponse.total;
                TxtPage.Text = $"Pag {currentPage}";

                BtnPrev.IsEnabled = currentPage > 1;
                BtnNext.IsEnabled = currentPage * limit < totalFacturas;

                // Refrescar filtro después de cargar
                _facturasView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar facturas: {ex.Message}");
            }
        }

        // ⬅️ Botón anterior
        private async void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                await CargarFacturas();
            }
        }

        // ➡️ Botón siguiente
        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * limit < totalFacturas)
            {
                currentPage++;
                await CargarFacturas();
            }
        }

        // 📄 Ver Factura en PDF
        private void VerFactura_Click(object sender, RoutedEventArgs e)
        {
            if (FacturasGrid.SelectedItem is Factura factura)
            {
                string url = $"http://localhost:3000/api/facturas/pdf/{factura.idFactura}";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Seleccione una factura primero.");
            }
        }

        // 🔎 Filtro en tiempo real
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = (SearchBox.Text ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(query))
            {
                _facturasView.Filter = null; // Mostrar todas
            }
            else
            {
                _facturasView.Filter = item =>
                {
                    var factura = item as Factura;
                    if (factura == null) return false;

                    return factura.idFactura.ToString().Contains(query)
                        || factura.fechaEmision.Contains(query)
                        || factura.total.ToString("F2").Contains(query)
                        || (!string.IsNullOrEmpty(factura.metodo_pago) && factura.metodo_pago.ToLower().Contains(query))
                        || (!string.IsNullOrEmpty(factura.status) && factura.status.ToLower().Contains(query));
                };
            }

            _facturasView.Refresh();
        }

        // ❌ Botón limpiar búsqueda
        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            _facturasView.Filter = null;
            _facturasView.Refresh();
        }

        // 👉 Selección en DataGrid
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FacturasGrid.SelectedItem is Factura factura)
            {
                Console.WriteLine($"Factura seleccionada: {factura.idFactura}");
            }
        }
    }
}
