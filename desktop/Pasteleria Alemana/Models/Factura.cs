using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pasteleria_Alemana.Models
{
    public class Factura
    {
        public int idFactura { get; set; }
        public int idCliente { get; set; }
        public string fechaEmision { get; set; }
        public decimal total { get; set; }
    }
}
