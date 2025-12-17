using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Pasteleria_Alemana
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = TxtUsuario.Text;
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Debe ingresar usuario y contraseña",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 📌 Datos a enviar al backend
                    var data = new
                    {
                        username = usuario,
                        password = password
                    };

                    string json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // 📌 Llamada al API local con la ruta correcta
                    HttpResponseMessage response = await client.PostAsync(
                        "http://localhost:3000/api/usuarios/login",
                        content);

                    if (response.IsSuccessStatusCode)
            {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // API devuelve algo como { "token": "xxxxx" }
                        dynamic result = JsonConvert.DeserializeObject(responseBody);
                        string token = result?.token ?? "";

                        MessageBox.Show("✅ Login exitoso",
                                        "Éxito",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                        // 👉 Guardar token globalmente para futuras peticiones
                        App.Token = token;
                        App.TokenRol = GetRolFromToken(token);

                        // Abrir MainWindow
                        MainWindow main = new MainWindow();
                main.Show();

                // Cerrar ventana de login
                this.Close();
            }
            else
            {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"❌ Usuario o contraseña incorrectos\n{errorMsg}",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con el servidor:\n{ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
        // Función para extraer el rol desde el JWT
        private string GetRolFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Ajusta "role" según tu backend
                var rolClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "rol");
                return rolClaim?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }
}
}
