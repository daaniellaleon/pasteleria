using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pasteleria_Alemana
{
    public partial class AgregarProductoWindow : Window
    {
        public Producto? NuevoProducto { get; private set; }

        public AgregarProductoWindow()
        {
            InitializeComponent();

            TxtFechaIngreso.Text = $"Fecha de ingreso: {DateTime.Now:dd/MM/yyyy HH:mm}";

            // Eventos de sanitización
            TxtProducto.TextChanged += Nombre_TextChanged;
            TxtPrecioUnitario.TextChanged += Precio_TextChanged;
            TxtPrecioUnitario.LostFocus += Precio_LostFocus;
            TxtStock.TextChanged += Stock_TextChanged;
        }

        // ===========================
        //  VALIDACIONES EN TIEMPO REAL
        // ===========================

        // Nombre → solo letras, espacios, tildes, max 50 caracteres
        private void Nombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int caret = tb.CaretIndex;

            string limpio = Regex.Replace(tb.Text, @"[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]", "");

            if (limpio.Length > 50)
                limpio = limpio.Substring(0, 50);

            if (tb.Text != limpio)
            {
                tb.Text = limpio;
                tb.CaretIndex = Math.Min(caret, limpio.Length);
            }
        }

        // Precio → solo números + 1 punto + 2 decimales
        private void Precio_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int caret = tb.CaretIndex;
            string texto = tb.Text;

            texto = Regex.Replace(texto, @"[^0-9.]", "");

            int punto = texto.IndexOf('.');
            if (punto > -1)
            {
                texto = texto.Substring(0, punto + 1) + texto[(punto + 1)..].Replace(".", "");
            }

            if (punto > -1 && texto.Length > punto + 3)
            {
                texto = texto.Substring(0, punto + 3);
            }

            if (tb.Text != texto)
            {
                tb.Text = texto;
                tb.CaretIndex = Math.Min(caret, texto.Length);
            }
        }

        // Al salir del TextBox, formatear a 0.00
        private void Precio_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtPrecioUnitario.Text, out var val))
                TxtPrecioUnitario.Text = val.ToString("0.00");
        }

        // Stock → solo números
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
        //  BOTÓN CONFIRMAR
        // ===========================

        private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtProducto.Text))
            {
                MessageBox.Show("Ingresa el nombre del producto.");
                return;
            }

            if (CmbCategoria.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una categoría.");
                return;
            }

            string categoria = ((ComboBoxItem)CmbCategoria.SelectedItem).Content.ToString();

            if (!decimal.TryParse(TxtPrecioUnitario.Text, out var precio))
            {
                MessageBox.Show("Precio inválido.");
                return;
            }

            if (!int.TryParse(TxtStock.Text, out var stock))
            {
                MessageBox.Show("Stock inválido.");
                return;
            }

            var producto = new Producto
            {
                Nombre = TxtProducto.Text.Trim(),
                Categoria = categoria,
                Precio = precio,
                Stock = stock
            };

            var api = new ApiService();
            api.SetToken(App.Token);

            var resultado = await api.CrearProductoAsync(producto);

            if (resultado == "OK")
            {
                MessageBox.Show("✅ Producto creado en el servidor");

                NuevoProducto = producto;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("❌ " + resultado);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
