using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pasteleria_Alemana
{
    public class Producto : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("id")]
        public int Id { get; set; }

        private string nombre = string.Empty;
        [JsonProperty("nombre")]
        public string Nombre
        {
            get => nombre;
            set { nombre = value; OnPropertyChanged(nameof(Nombre)); }
        }

        private string categoria = string.Empty;
        [JsonProperty("categoria")]
        public string Categoria
        {
            get => categoria;
            set { categoria = value; OnPropertyChanged(nameof(Categoria)); }
        }

        private decimal precio;
        [JsonProperty("precio_unitario")]
        public decimal Precio
        {
            get => precio;
            set { precio = value; OnPropertyChanged(nameof(Precio)); }
        }

        private int stock;
        [JsonProperty("stock")]
        public int Stock
        {
            get => stock;
            set
            {
                stock = value;
                Estado = stock <= 0 ? "Agotado" : "Disponible"; // recalcular Estado automáticamente
                OnPropertyChanged(nameof(Stock));
            }
        }

        private string estado = string.Empty;
        [JsonProperty("estado")]
        public string Estado
        {
            get => estado;
            set { estado = value; OnPropertyChanged(nameof(Estado)); }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


