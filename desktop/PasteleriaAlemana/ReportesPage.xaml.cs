using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace Pasteleria_Alemana
{
    public partial class ReportesPage : Page
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string baseUrl = "http://localhost:3000/api/reportes";

        public ReportesPage()
        {
            InitializeComponent();
            CargarOpcionesPorRol();
        }

        // ✅ Mostrar solo la opción de reporte que corresponde al rol
        private void CargarOpcionesPorRol()
        {
            TipoReporteBox.Items.Clear();

            if (App.TokenRol == "ventas")
            {
                TipoReporteBox.Items.Add(new ComboBoxItem { Content = "Ventas" });
            }
            else if (App.TokenRol == "inventario")
            {
                TipoReporteBox.Items.Add(new ComboBoxItem { Content = "Inventario" });
            }
            else
            {
                MessageBox.Show("⚠️ No tienes permisos para generar reportes.");
            }

            if (TipoReporteBox.Items.Count > 0)
                TipoReporteBox.SelectedIndex = 0;
        }

        private async void BtnGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 🟢 Leer parámetros seleccionados
                string tipoReporte = (TipoReporteBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                                     ?? TipoReporteBox.Text;

                string formato = (FormatoBox.SelectedItem as ComboBoxItem)?.Content.ToString().ToLower()
                                 ?? "pdf";

                DateTime? fechaInicio = FechaInicioPicker.SelectedDate;
                DateTime? fechaFin = FechaFinPicker.SelectedDate;

                // 🔴 Validación
                if (string.IsNullOrWhiteSpace(tipoReporte))
                {
                    MessageBox.Show("Debe seleccionar el tipo de reporte.");
                    return;
                }
                if (!fechaInicio.HasValue || !fechaFin.HasValue)
                {
                    MessageBox.Show("Debe seleccionar las fechas de inicio y fin.");
                    return;
                }
                if (fechaInicio > fechaFin)
                {
                    MessageBox.Show("La fecha de inicio no puede ser mayor que la fecha fin.");
                    return;
                }

                // 🚨 Validación de rol (seguridad extra)
                if ((tipoReporte == "Ventas" && App.TokenRol != "ventas") ||
                    (tipoReporte == "Inventario" && App.TokenRol != "inventario"))
                {
                    MessageBox.Show($"⚠️ No tienes permisos para generar reportes de {tipoReporte}.");
                    return;
                }

                // 🟡 Construir URL
                string url = $"{baseUrl}/{tipoReporte.ToLower()}?inicio={fechaInicio:yyyy-MM-dd}&fin={fechaFin:yyyy-MM-dd}&formato={formato}";

                // 🟢 Actualizar estado
                ReporteStatus.Text = "Generando reporte, por favor espere...";

                // 🚨 Primero validar si hay datos
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    ReporteStatus.Text = "⚠️ " + errorMsg;
                    MessageBox.Show(errorMsg, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Si hay datos -> abrir reporte en navegador o visor predeterminado
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                ReporteStatus.Text = $"📊 Reporte de {tipoReporte} generado en formato {formato.ToUpper()}";
            }
            catch (Exception ex)
            {
                ReporteStatus.Text = "Error al generar el reporte.";
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

    }
}
