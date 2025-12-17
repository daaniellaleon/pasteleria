using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Pasteleria_Alemana
{
    /// <summary>
    /// Interaction logic for InicioPage.xaml
    /// </summary>
    public partial class InicioPage : Page
    {
        private DispatcherTimer timer;

        public InicioPage()
        {
            InitializeComponent();
            IniciarReloj();
        }

        private void IniciarReloj()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Actualiza cada segundo
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime ahora = DateTime.Now;

            // Hora en formato 12h
            HoraText.Text = ahora.ToString("hh:mm");
            AmPmText.Text = ahora.ToString("tt", System.Globalization.CultureInfo.InvariantCulture).ToUpper();

            // Fecha en español
            FechaText.Text = ahora.ToString("dddd dd 'de' MMMM", new System.Globalization.CultureInfo("es-ES"));
        }

        private void IniciarVenta_Click(object sender, RoutedEventArgs e)
        {
            VerificacionHelper.VerificarAcceso("ventas", new CarPage(), this.NavigationService);
        }

     





    }
}
