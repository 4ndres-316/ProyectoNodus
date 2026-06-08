using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Proyecto_Boletos
{
    public class Tema
    {
        public static string modo = "claro";

        public static void CambiarTema()
        {
            if (modo == "claro")
            {
                modo = "oscuro";

                Application.Current.Resources["Color1"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Oscuro1"]);

                Application.Current.Resources["Color2"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Oscuro2"]);

                Application.Current.Resources["Color3"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Oscuro3"]);

                Application.Current.Resources["Color4"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Oscuro4"]);

                Application.Current.Resources["txtActual"] =
                    Application.Current.Resources["txtOscuro"];

                Application.Current.Resources["imgActual"] =
                    Application.Current.Resources["imgOscuro"];

                Application.Current.Resources["TituloActual"] =
                    Application.Current.Resources["TituloOscuro"];
            }
            else
            {
                modo = "claro";

                Application.Current.Resources["Color1"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Claro1"]);

                Application.Current.Resources["Color2"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Claro2"]);

                Application.Current.Resources["Color3"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Claro3"]);

                Application.Current.Resources["Color4"] =
                    new SolidColorBrush((Color)Application.Current.Resources["Claro4"]);

                Application.Current.Resources["txtActual"] =
                    Application.Current.Resources["txtClaro"];

                Application.Current.Resources["imgActual"] =
                    Application.Current.Resources["imgClaro"];

                Application.Current.Resources["TituloActual"] =
                    Application.Current.Resources["TituloClaro"];
            }
        }
    }
}
