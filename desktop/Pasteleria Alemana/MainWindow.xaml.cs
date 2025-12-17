using System.Windows;

namespace Pasteleria_Alemana
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Mostrar Inicio por defecto
            MainFrame.Navigate(new InicioPage());
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InicioPage());
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            VerificacionHelper.VerificarAcceso("inventario", new InventarioPage(), MainFrame.NavigationService);
        }

        private void BtnFacturas_Click(object sender, RoutedEventArgs e)
        {
            VerificacionHelper.VerificarAcceso("ventas", new FacturasPage(), MainFrame.NavigationService);
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportesPage());
        }

        private void Frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void BtnPerfil_Click(object sender, RoutedEventArgs e)
        {
            BtnPerfil.ContextMenu.IsOpen = true;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Cierra esta ventana
            this.Close();

            // Vuelve al login
            LoginWindow login = new LoginWindow();
            login.Show();
        }

    }
}