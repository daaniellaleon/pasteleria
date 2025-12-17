using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pasteleria_Alemana
{
    public partial class GestionarPagoPage : Page
    {
        private string metodoSeleccionado;
        private HttpClient httpClient;
        private Button botonSeleccionado;

        public GestionarPagoPage()
        {
            InitializeComponent();

            // Inicializar HttpClient una sola vez
            httpClient = new HttpClient();

            // Agregar token si existe
            string token = App.Token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Selección de método de pago
        private void MetodoPago_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                metodoSeleccionado = btn.Tag.ToString();
                MessageBox.Show($"Método seleccionado: {metodoSeleccionado}");

                if (botonSeleccionado != null)
                {
                    botonSeleccionado.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                    botonSeleccionado.Background = Brushes.White;
                    botonSeleccionado.BorderThickness = new Thickness(1);
                }

                btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                btn.BorderThickness = new Thickness(3);
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));

                botonSeleccionado = btn;
            }
        }

        // Confirmar pago
        private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(metodoSeleccionado))
            {
                MessageBox.Show("Selecciona un método de pago antes de continuar.");
                return;
            }

            try
            {
                HttpResponseMessage response;

                // Obtener carrito desde endpoint de ventas
                var carrito = await httpClient.GetFromJsonAsync<CarritoItem[]>("http://localhost:3000/api/ventas/");

                if (carrito == null || carrito.Length == 0)
                {
                    MessageBox.Show("El carrito está vacío.");
                    return;
                }

                decimal totalCompra = carrito.Sum(item => (decimal)item.total);

                if (metodoSeleccionado.ToLower() == "efectivo")
                {
                    string input = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Monto a pagar: ${totalCompra:0.00}\nIngrese el monto recibido:",
                        "Pago en efectivo",
                        totalCompra.ToString("0.00")
                    );

                    if (!decimal.TryParse(input, out decimal montoRecibido) || montoRecibido <= 0)
                    {
                        MessageBox.Show("Monto inválido.");
                        return;
                    }

                    // Llamar endpoint de efectivo correctamente
                    response = await httpClient.PostAsJsonAsync(
                        "http://localhost:3000/api/gestionarPagos/pagar-efectivo",
                        new { montoRecibido }
                    );
                }
                else
                {
                    // Pago digital en endpoint de ventas
                    response = await httpClient.PostAsJsonAsync(
                        $"http://localhost:3000/api/gestionarPagos/pagar/{metodoSeleccionado}",
                        new { }
                    );
                }

                if (!response.IsSuccessStatusCode)
                {
                    string contenido = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {contenido}");
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<PagoResponse>();

                // Mostrar resumen
                string resumen = $"Compra realizada\n\nTotal: ${result.compra.total}\n" +
                                 $"Método: {result.compra.metodo_pago}\n" +
                                 $"Estado: {result.compra.status}\n" +
                                 $"Transacción: {result.compra.transactionId}\n";

                if (metodoSeleccionado.ToLower() == "efectivo")
                {
                    resumen += $"Monto recibido: ${result.compra.montoRecibido:0.00}\n";
                    resumen += $"Cambio: ${result.compra.cambio:0.00}\n";
                }

                MessageBox.Show(resumen);

                // Volver a MainWindow
                MainWindow main = new MainWindow();
                main.Show();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnAtras_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as MainWindow;
            win.MainFrame.Navigate(new CarPage());
        }
    }

    // ================== MODELOS ==================
    public class PagoResponse
    {
        public string mensaje { get; set; }
        public Compra compra { get; set; }
    }

    public class Compra
    {
        public int id { get; set; }
        public int usuario_id { get; set; }
        public double total { get; set; }
        public string metodo_pago { get; set; }
        public string status { get; set; }
        public string transactionId { get; set; }
        public double? montoRecibido { get; set; }
        public double? cambio { get; set; }
        public ProductoCompra[] productos { get; set; }
    }

    public class ProductoCompra
    {
        public int producto_id { get; set; }
        public string nombre { get; set; }
        public int cantidad { get; set; }
        public double precio_unitario { get; set; }
        public double total { get; set; }
    }

   

}
