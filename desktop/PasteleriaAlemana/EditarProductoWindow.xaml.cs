using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Pasteleria_Alemana
{
    public partial class EditarProductoWindow : Window
    {
        public Producto ProductoEditado { get; private set; }
        private readonly Producto _original;

        public EditarProductoWindow(Producto producto)
        {
            InitializeComponent();
            _original = producto;

            // Cargar valores
            TxtProducto.Text = producto.Nombre;
            TxtPrecio.Text = producto.Precio.ToString("0.00");
            TxtStock.Text = producto.Stock.ToString();

            // Seleccionar categoría en combo
            CmbCategoria.SelectedItem =
                CmbCategoria.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == producto.Categoria);

            // Sanitización
            TxtProducto.TextChanged += Nombre_TextChanged;
            TxtPrecio.TextChanged += Precio_TextChanged;
            TxtPrecio.LostFocus += Precio_LostFocus;
            TxtStock.TextChanged += Stock_TextChanged;
        }

        // ===========================
        // VALIDACIONES
        // ===========================

        private void Nombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int caret = tb.CaretIndex;

            string limpio = Regex.Replace(tb.Text, @"[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]", "");
            if (limpio.Length > 50) limpio = limpio[..50];

            if (tb.Text != limpio)
            {
                tb.Text = limpio;
                tb.CaretIndex = Math.Min(caret, limpio.Length);
            }
        }

        private void Precio_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int caret = tb.CaretIndex;

            string texto = Regex.Replace(tb.Text, @"[^0-9.]", "");

            int punto = texto.IndexOf('.');
            if (punto > -1)
                texto = texto.Substring(0, punto + 1) + texto[(punto + 1)..].Replace(".", "");

            if (punto > -1 && texto.Length > punto + 3)
                texto = texto.Substring(0, punto + 3);

            if (tb.Text != texto)
            {
                tb.Text = texto;
                tb.CaretIndex = Math.Min(caret, texto.Length);
            }
        }

        private void Precio_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtPrecio.Text, out var val))
                TxtPrecio.Text = val.ToString("0.00");
        }

        private void Stock_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int caret = tb.CaretIndex;

            string limpio = Regex.Replace(tb.Text, "[^0-9]", "");

            if (tb.Text != limpio)
            {
                tb.Text = limpio;
                tb.CaretIndex = Math.Min(caret, limpio.Length);
            }
        }

        // ===========================
        // BOTÓN GUARDAR
        // ===========================

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtPrecio.Text, out var precio))
            {
                MessageBox.Show("Precio inválido.");
                return;
            }

            if (!int.TryParse(TxtStock.Text, out var stock))
            {
                MessageBox.Show("Stock inválido.");
                return;
            }

            string categoria = ((ComboBoxItem)CmbCategoria.SelectedItem).Content.ToString();

            ProductoEditado = new Producto
            {
                Id = _original.Id,
                Nombre = TxtProducto.Text.Trim(),
                Categoria = categoria,
                Precio = precio,
                Stock = stock
            };

            var api = new ApiService();
            api.SetToken(App.Token);

            var resultado = await api.EditarProductoAsync(_original.Id, ProductoEditado);

            if (resultado)
            {
                MessageBox.Show("✔ Producto actualizado correctamente");
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("❌ Error actualizando el producto");
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }
    }
}
