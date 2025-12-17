using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Pasteleria_Alemana.InventarioPage;

namespace Pasteleria_Alemana
{
    public partial class AgregarInsumoWindow : Window
    {
        public Insumo NuevoInsumo { get; private set; }

        public AgregarInsumoWindow()
        {
            InitializeComponent();
            ConfigurarValoresPorDefecto();
            ConfigurarEventos();
            // Mostrar fecha actual
            TxtFechaIngreso.Text = "Fecha de ingreso: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void ConfigurarValoresPorDefecto()
        {
            TxtUnidad.Text = "Kg";
            CmbEstado.SelectedIndex = 0;
            Loaded += (s, e) => TxtInsumo.Focus();
        }

        private void ConfigurarEventos()
        {
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    BtnCancelar_Click(s, e);
                if (e.Key == Key.Enter && TodosLosCamposCompletos())
                    BtnConfirmar_Click(s, e);
            };
        }

        private bool TodosLosCamposCompletos()
        {
            return !string.IsNullOrWhiteSpace(TxtInsumo.Text) &&
                   !string.IsNullOrWhiteSpace(TxtCantidad.Text) &&
                   double.TryParse(TxtCantidad.Text, out var cantidad) && cantidad >= 0;
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
                return;

            CrearNuevoInsumo();
            CerrarConExito();
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(TxtInsumo.Text))
            {
                MostrarError("Por favor, ingresa el nombre del insumo.", TxtInsumo);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtCantidad.Text))
            {
                MostrarError("Por favor, ingresa la cantidad.", TxtCantidad);
                return false;
            }

            if (!double.TryParse(TxtCantidad.Text, out var cantidad) || cantidad < 0)
            {
                MostrarError("La cantidad debe ser un número válido mayor o igual a 0.", TxtCantidad);
                return false;
            }

            if (TxtInsumo.Text.Trim().Length < 2)
            {
                MostrarError("El nombre del insumo debe tener al menos 2 caracteres.", TxtInsumo);
                return false;
            }

            return true;
        }

        private void MostrarError(string mensaje, Control campo)
        {
            MessageBox.Show(mensaje, "Campo inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
            campo.Focus();
        }

        private void CrearNuevoInsumo()
        {
            NuevoInsumo = new Insumo
            {
                Nombre = TxtInsumo.Text.Trim(),
                Cantidad = double.Parse(TxtCantidad.Text),
                UnidadMedida = string.IsNullOrWhiteSpace(TxtUnidad.Text) ? "Unidades" : TxtUnidad.Text.Trim(),
                Proveedor = string.IsNullOrWhiteSpace(TxtProveedor.Text) ? "Sin especificar" : TxtProveedor.Text.Trim(),
                Estado = ((ComboBoxItem)CmbEstado.SelectedItem)?.Content?.ToString() ?? "Disponible"
            };
        }

        private void CerrarConExito()
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (HayDatosIngresados())
            {
                var resultado = MessageBox.Show(
                    "¿Estás seguro de que deseas cancelar? Se perderán los datos ingresados.",
                    "Confirmar cancelación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado != MessageBoxResult.Yes)
                    return;
            }

            DialogResult = false;
            Close();
        }

        private bool HayDatosIngresados()
        {
            return !string.IsNullOrWhiteSpace(TxtInsumo.Text) ||
                   !string.IsNullOrWhiteSpace(TxtCantidad.Text) ||
                   (TxtUnidad.Text != "Kg" && !string.IsNullOrWhiteSpace(TxtUnidad.Text)) ||
                   !string.IsNullOrWhiteSpace(TxtProveedor.Text) ||
                   CmbEstado.SelectedIndex != 0;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}