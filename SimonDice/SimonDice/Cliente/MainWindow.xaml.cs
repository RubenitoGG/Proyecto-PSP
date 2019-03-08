using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading;


namespace Cliente
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // VARIABLES PARA COMUNICARSE CON EL SERVIDOR:
        TcpClient cliente;
        NetworkStream netStream;
        StreamReader reader;
        StreamWriter writer;

        List<string> colores;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Nos conectamos al servidor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // CONECTAMOS AL SERVIDOR E INICIALIZAMOS LAS VARIABLES:
                cliente = new TcpClient(this.txt_ip.Text, 2000);
                netStream = cliente.GetStream();
                reader = new StreamReader(netStream);
                writer = new StreamWriter(netStream);
                
                List<string> colores = new List<string>();

                writer.WriteLine("@INSCRIBIR@" + this.txt_nombre + "@");
                writer.Flush();

                string[] data = reader.ReadLine().Split('@');

                if (data[1] == "OK") // NOS CONECTAMOS
                {
                    this.txt_conexion.Text = "Conectado";
                }
                else
                {
                    if (data[2] == "0A")
                        MessageBox.Show("Ya hay dos jugadores.");
                    else if (data[2] == "0B")
                        MessageBox.Show("Necesitas un nombre.");
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine("Error: " + error.ToString());
            }
        }

        /// <summary>
        /// Comprobamos los colores que tenemos que pulsar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Enviamos la lista de los colores y vemos si están correctos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                // METEMOS TODOS LOS COLORES EN LA LISTA:
                string envio = "@JUGADA@";

                foreach (string s in colores)
                {
                    envio += s + "@";
                }

                // ENVIAMOS TODO:
                writer.WriteLine(envio);
                writer.Flush();

                string[] data = reader.ReadLine().Split('@');
                if (data[1] == "OK")
                {
                    if (data[2] == "CORRECTO")
                        MessageBox.Show("Combinación de colores correcta.");
                    else
                        MessageBox.Show("Combinación de colores incorrecta, perdiste.");
                }
                else
                {
                    if (data[2] == "1A")
                        MessageBox.Show("No hay dos jugadores.");
                    else if (data[2] == "1B")
                        MessageBox.Show("Número de colores incorrectos.");
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine("Error: " + error.ToString());
            }
        }

        /// <summary>
        /// Añadimos color a la lista que se enviará.
        /// </summary>
        /// <param name="color"></param>
        private void AñadirColor(string color)
        {
            colores.Add(color);
        }

        private void btn_rojo_Click(object sender, RoutedEventArgs e)
        {
            AñadirColor("ROJO");
        }
        
        private void btn_verde_Click(object sender, RoutedEventArgs e)
        {
            AñadirColor("VERDE");
        }

        private void btn_azul_Click(object sender, RoutedEventArgs e)
        {
            AñadirColor("AZUL");
        }

        private void btn_amarillo_Click(object sender, RoutedEventArgs e)
        {
            AñadirColor("AMARILLO");
        }
    }
}
