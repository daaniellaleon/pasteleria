using System.Windows;

namespace Pasteleria_Alemana
{
    public partial class App : Application
    {
        // 🔑 Token JWT global para usar en toda la aplicación
        public static string Token { get; set; }
        public static string TokenRol { get; set; } = "";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Mostrar primero el LoginWindow
            LoginWindow login = new LoginWindow();
            login.Show();
        }
    }
}
