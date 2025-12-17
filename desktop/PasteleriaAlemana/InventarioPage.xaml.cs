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

namespace Pasteleria_Alemana
{
    public partial class InventarioPage : Page
    {
        // 🟢 Modelo de datos para Insumo
        public class Insumo
        {
            public string Nombre { get; set; } = string.Empty;
            public double Cantidad { get; set; }
            public string UnidadMedida { get; set; } = string.Empty;
            public string Proveedor { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
        }

        // 🟢 Listas observables
        private readonly ObservableCollection<Insumo> _insumos = new();
        private readonly ObservableCollection<Producto> _productos = new();

        // 🟢 Vistas filtrables
        private ICollectionView _insumosView;
        private ICollectionView _productosView;

        private readonly ApiService api = new ApiService();

        public InventarioPage()
        {
            InitializeComponent();

            // 🔹 Datos de ejemplo INSUMOS
            _insumos.Add(new Insumo { Nombre = "Harina de trigo", Cantidad = 40, UnidadMedida = "Kg", Proveedor = "Molinos del Este", Estado = "Disponible" });
            _insumos.Add(new Insumo { Nombre = "Azúcar", Cantidad = 0, UnidadMedida = "Kg", Proveedor = "Azúcar Chiriquí S.A.", Estado = "Agotado" });
            _insumos.Add(new Insumo { Nombre = "Mantequilla", Cantidad = 13, UnidadMedida = "Kg", Proveedor = "Lácteos Chiriquí", Estado = "Disponible" });
            _insumos.Add(new Insumo { Nombre = "Huevos", Cantidad = 60, UnidadMedida = "Unidades", Proveedor = "Huevería El Sol", Estado = "Disponible" });
            _insumos.Add(new Insumo { Nombre = "Leche", Cantidad = 16, UnidadMedida = "Lts", Proveedor = "Lácteos de David", Estado = "Disponible" });

            // 🔹 Configurar vista de insumos
            _insumosView = CollectionViewSource.GetDefaultView(_insumos);
            dgInsumos.ItemsSource = _insumosView;

            // 🔹 Configurar vista de productos (observable)
            _productosView = CollectionViewSource.GetDefaultView(_productos);
            dgProductos.ItemsSource = _productosView;

            // Estado inicial → mostrar insumos
            dgInsumos.Visibility = Visibility.Visible;
            dgProductos.Visibility = Visibility.Collapsed;
            BtnAgregar.Content = "Agregar Insumo";
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            api.SetToken(App.Token); // 🔑 importante para autenticación
            await CargarProductos();
        }

        private async Task CargarProductos()
        {
            try
            {
                var productosServidor = await api.GetProductosAsync();

                _productos.Clear(); // limpiamos la lista observable
                foreach (var p in productosServidor)
                {
                    p.Estado = p.Stock <= 0 ? "Agotado" : "Disponible";
                    _productos.Add(p); // agregamos desde el servidor
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando productos: " + ex.Message);
            }
        }

        // 🔎 Filtrado en tiempo real
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var raw = (SearchBox.Text ?? string.Empty).Trim();
            var q = Normalize(raw);

            if (dgInsumos.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    _insumosView.Filter = null;
                }
                else
                {
                    _insumosView.Filter = item =>
                    {
                        var ins = (Insumo)item;
                        return Contains(ins.Nombre, q)
                            || Contains(ins.Proveedor, q)
                            || Contains(ins.UnidadMedida, q)
                            || Contains(ins.Estado, q)
                            || ins.Cantidad.ToString().Contains(raw);
                    };
                }
                _insumosView.Refresh();
            }
            else if (dgProductos.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    _productosView.Filter = null;
                }
                else
                {
                    _productosView.Filter = item =>
                    {
                        var prod = (Producto)item;
                        return Contains(prod.Nombre, q)
                            || Contains(prod.Categoria, q)
                            || Contains(prod.Estado, q)
                            || prod.Precio.ToString().Contains(raw)
                            || prod.Stock.ToString().Contains(raw);
                    };
                }
                _productosView.Refresh();
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            SearchBox.Focus();
        }

        private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (dgInsumos.Visibility == Visibility.Visible)
            {
                var ventana = new AgregarInsumoWindow();
                ventana.Owner = Window.GetWindow(this);
                if (ventana.ShowDialog() == true && ventana.NuevoInsumo != null)
                {
                    _insumos.Add(ventana.NuevoInsumo);
                }
            }
            else if (dgProductos.Visibility == Visibility.Visible)
            {
                var ventana = new AgregarProductoWindow();
                ventana.Owner = Window.GetWindow(this);

                if (ventana.ShowDialog() == true && ventana.NuevoProducto != null)
                {
                    // 🔹 Lo agregamos directamente a la colección observable
                    _productos.Insert(0, ventana.NuevoProducto);

                }
            }

        }

        private async void dgProductos_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var producto = e.Row.Item as Producto;
                if (producto != null)
                {
                    try
                    {
                        // 🔹 Crear copia del producto para enviar a la API
                        var productoActualizar = new Producto
                        {
                            Id = producto.Id,
                            Nombre = producto.Nombre,
                            Categoria = producto.Categoria,
                            Precio = producto.Precio,
                            Stock = producto.Stock
                            // No estableces Estado; la API lo calcula automáticamente
                        };

                        var actualizado = await api.EditarProductoAsync(producto.Id, productoActualizar);

                        if (!actualizado)
                        {
                            MessageBox.Show("❌ Error actualizando producto en la BD");
                        }
                        else
                        {
                            // 🔹 Actualizar Estado en memoria para que se vea en tiempo real
                            producto.Estado = producto.Stock <= 0 ? "Agotado" : "Disponible";
                            dgProductos.Items.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al actualizar producto: " + ex.Message);
                    }
                }
            }
        }








        private void BtnInvInsumo_Click(object sender, RoutedEventArgs e)
        {
            dgInsumos.Visibility = Visibility.Visible;
            dgProductos.Visibility = Visibility.Collapsed;
            BtnAgregar.Content = "Agregar Insumo";

            SearchBox_TextChanged(SearchBox, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None));
        }

        private void BtnInvProducto_Click(object sender, RoutedEventArgs e)
        {
            dgInsumos.Visibility = Visibility.Collapsed;
            dgProductos.Visibility = Visibility.Visible;
            BtnAgregar.Content = "Agregar Producto";

            SearchBox_TextChanged(SearchBox, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None));
        }

        private static bool Contains(string source, string query)
        {
            if (string.IsNullOrEmpty(source)) return false;
            return Normalize(source).Contains(query);
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

        private void BtnEditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var producto = (sender as FrameworkElement)?.DataContext as Producto;
            if (producto == null) return;

            var ventana = new EditarProductoWindow(producto);
            ventana.Owner = Window.GetWindow(this);

            if (ventana.ShowDialog() == true)
            {
                var p = ventana.ProductoEditado;

                // Actualizar los valores en el DataGrid
                producto.Nombre = p.Nombre;
                producto.Categoria = p.Categoria;
                producto.Precio = p.Precio;
                producto.Stock = p.Stock;
                producto.Estado = p.Estado;

                dgProductos.Items.Refresh();
            }
        }



    }
}
