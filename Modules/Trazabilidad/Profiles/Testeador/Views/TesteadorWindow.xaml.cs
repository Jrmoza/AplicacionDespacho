using System.Windows;
using AplicacionDespacho.Modules.Trazabilidad.Profiles.Testeador.ViewModels;

namespace AplicacionDespacho.Modules.Trazabilidad.Profiles.Testeador.Views
{
    public partial class TesteadorWindow : Window
    {
        public TesteadorWindow()
        {
            InitializeComponent();
            this.DataContext = new TesteadorViewModel();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}