using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Pasteleria_Alemana
{
    public partial class CarPage : Page
    {
        private readonly ObservableCollection<Producto> productosDisponibles = new();
        private ICollectionView productosView;
        private ObservableCollection<CarritoItem> carritoItems = new ObservableCollection<CarritoItem>();
        private HttpClient ventasClient;
        private HttpClient productosClient;


        public CarPage()
        {
            InitializeComponent();

            // Cliente para ventas
            ventasClient = new HttpClient { BaseAddress = new Uri("http://localhost:3000/api/ventas/") };
            // Cliente para productos
            productosClient = new HttpClient { BaseAddress = new Uri("http://localhost:3000/api/") };

            string token = App.Token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                ventasClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                productosClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                MessageBox.Show("Token no encontrado. Haz login nuevamente.");
                return;
            }

            CarritoDataGrid.ItemsSource = carritoItems;
            CargarCarrito();
            CargarProductosDisponibles();

        }

        private async void CargarProductosDisponibles()
        {
            try
            {
                var productos = await productosClient.GetFromJsonAsync<Producto[]>("productos");

                productosDisponibles.Clear();
                foreach (var p in productos)
                {
                    productosDisponibles.Add(p);
                }

                productosView = CollectionViewSource.GetDefaultView(productosDisponibles);
                ListaResultados.ItemsSource = productosView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando productos disponibles: " + ex.Message);
            }
        }


        // Cargar carrito desde la API
        private async void CargarCarrito()
        {
            try
            {
                var items = await ventasClient.GetFromJsonAsync<CarritoItem[]>(""); // /api/ventas/
                carritoItems.Clear();
                foreach (var item in items)
                    carritoItems.Add(item);

                TotalText.Text = $"Total: {carritoItems.Sum(i => i.total)}$";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando el carrito: " + ex.Message);
            }
        }

        // Eliminar producto del carrito
        private async void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int id = (int)button.Tag;
                try
                {
                    var response = await ventasClient.DeleteAsync($"{id}");
                    if (response.IsSuccessStatusCode)
                        CargarCarrito();
                    else
                        MessageBox.Show("Error eliminando producto");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error eliminando producto: " + ex.Message);
                }
            }
        }

        // Evento al escribir en el buscador
        private void TxtBuscarProducto_TextChanged(object sender, TextChangedEventArgs e)
        {
            var raw = TxtBuscarProducto.Text?.Trim() ?? string.Empty;
            var q = Normalize(raw);

            if (string.IsNullOrWhiteSpace(q))
            {
                productosView.Filter = item =>
                {
                    var prod = (Producto)item;
                    return prod.Stock > 0; // Mostrar solo productos con stock > 0 cuando no hay búsqueda
                };
                ListaResultados.Visibility = Visibility.Collapsed;
            }
            else
            {
                productosView.Filter = item =>
                {
                    var prod = (Producto)item;
                    return prod.Stock > 0 && (
                           Contains(prod.Nombre, q) ||
                           Contains(prod.Categoria, q) ||
                           prod.Precio.ToString().Contains(raw) ||
                           prod.Stock.ToString().Contains(raw));
                };
                ListaResultados.Visibility = Visibility.Visible;
            }


            productosView.Refresh();
        }

        private static string Normalize(string text)
        {
            var formD = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);

            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool Contains(string source, string query)
        {
            if (string.IsNullOrEmpty(source)) return false;
            return Normalize(source).Contains(query);
        }





        // Selección de producto de la lista
        private async void ListaResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaResultados.SelectedItem is Producto seleccionado)
            {
                int cantidad = 1; // se puede pedir al usuario
                var nuevoItem = new { nombre = seleccionado.Nombre, cantidad };

                try
                {
                    var response = await ventasClient.PostAsJsonAsync("agregar", nuevoItem);
                    if (response.IsSuccessStatusCode)
                    {
                        CargarCarrito();
                        TxtBuscarProducto.Clear();
                        ListaResultados.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show("Error agregando producto al carrito");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error agregando producto: " + ex.Message);
                }
            }
        }

        private void BtnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            // Forzar que termine la edición de la celda y la fila
            bool cellCommit = CarritoDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            bool rowCommit = CarritoDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (!cellCommit || !rowCommit)
            {
                MessageBox.Show("Termina de editar antes de continuar.", "Edición pendiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que el carrito no esté vacío
            if (carritoItems == null || !carritoItems.Any())
            {
                MessageBox.Show("El carrito está vacío. Agrega productos antes de continuar.", "Carrito vacío", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que todas las cantidades sean mayores que cero
            if (carritoItems.Any(item => item.cantidad <= 0))
            {
                MessageBox.Show("Todas las cantidades deben ser mayores que cero.", "Cantidad inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Listo para gestionar el pago");

            GestionarPagoPage Gestion = new GestionarPagoPage();
            this.NavigationService.Navigate(Gestion);
        }






        private async void CarritoDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Cantidad")
            {
                if (e.EditingElement is TextBox textBox)
                {
                    string text = textBox.Text;

                    if (!int.TryParse(text, out int nuevaCantidad) || nuevaCantidad < 1)
                    {
                        MessageBox.Show("Por favor ingresa un número entero válido mayor o igual a 1.", "Cantidad inválida", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Cancelar la edición para que el usuario corrija
                        e.Cancel = true;
                        return;
                    }

                    if (e.Row.Item is CarritoItem item)
                    {
                        int cantidadAnterior = item.cantidad;

                        try
                        {
                            var update = new { cantidad = nuevaCantidad };
                            var response = await ventasClient.PutAsJsonAsync($"{item.id}", update);

                            if (response.IsSuccessStatusCode)
                            {
                                item.cantidad = nuevaCantidad;
                                TotalText.Text = $"Total: {carritoItems.Sum(i => i.total):F2}$";
                            }
                            else
                            {
                                var errorMsg = await response.Content.ReadAsStringAsync();
                                MessageBox.Show($"No se puede actualizar cantidad: {errorMsg}");
                                item.cantidad = cantidadAnterior;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error actualizando cantidad: " + ex.Message);
                            item.cantidad = cantidadAnterior;
                        }
                        finally
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                CarritoDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                                CarritoDataGrid.Items.Refresh();
                            }), DispatcherPriority.Background);
                        }
                    }
                }
            }
        }
    }





    // Clases auxiliares
    public class CarritoItem
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public int cantidad { get; set; }
        public double precio_unitario { get; set; }
        public double total => Math.Round(cantidad * precio_unitario, 2);
    }






}