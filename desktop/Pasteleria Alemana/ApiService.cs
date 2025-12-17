using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pasteleria_Alemana
{
    public class ApiService
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string baseUrl = "http://localhost:3000/api"; // 🔹 ajusta al puerto de tu backend
        private string token; // 🔹 guardamos el token JWT

        // Permite setear el token desde login
        public void SetToken(string token)
        {
            this.token = token;
        }

        // Configura los headers (se usa en cada request)
        private void AddAuthHeader()
        {
            _http.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 🔹 LOGIN: obtiene el token JWT desde el backend
        public async Task<string> LoginAsync(string usuario, string password)
        {
            var json = JsonConvert.SerializeObject(new
            {
                usuario = usuario,
                password = password
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{baseUrl}/usuarios/login", content);
            var respuestaServidor = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                dynamic data = JsonConvert.DeserializeObject(respuestaServidor);
                string token = data?.token;

                if (!string.IsNullOrEmpty(token))
                {
                    SetToken(token);
                    App.Token = token; // 🔹 Guardamos globalmente en App.xaml.cs también
                }

                return token ?? "";
            }
            else
            {
                return "";
            }
        }

        // 🔹 LISTAR productos
        public async Task<List<Producto>> GetProductosAsync()
        {
            AddAuthHeader();
            var response = await _http.GetAsync($"{baseUrl}/productos");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Producto>>(json) ?? new List<Producto>();
        }

        // 🔹 CREAR producto
        public async Task<string> CrearProductoAsync(Producto producto)
        {
            AddAuthHeader();
            var json = JsonConvert.SerializeObject(new
            {
                nombre = producto.Nombre,
                categoria = producto.Categoria,
                precio_unitario = producto.Precio,
                stock = producto.Stock
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{baseUrl}/productos", content);

            var respuestaServidor = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return "OK";
            }
            else
            {
                return $"Error {response.StatusCode}: {respuestaServidor}";
            }
        }

        // 🔹 EDITAR producto
        public async Task<bool> EditarProductoAsync(int id, Producto producto)
        {
            AddAuthHeader();
            var json = JsonConvert.SerializeObject(new
            {
                nombre = producto.Nombre,
                categoria = producto.Categoria, // 👈 string
                precio_unitario = producto.Precio,
                stock = producto.Stock
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync($"{baseUrl}/productos/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show("❌ Error desde la API: " + error);
            }

            return response.IsSuccessStatusCode;
        }

    }
}
