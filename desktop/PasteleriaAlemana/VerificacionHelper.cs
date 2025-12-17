using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Pasteleria_Alemana
{
    public static class VerificacionHelper
    {
        public static void VerificarAcceso(string rolRequerido, Page paginaDestino, NavigationService navService)
        {
            if (App.TokenRol != rolRequerido)
            {
                MessageBox.Show($"No tienes permisos para acceder a {paginaDestino.GetType().Name}.");
                return;
            }

            navService.Navigate(paginaDestino);
        }
    }
}
