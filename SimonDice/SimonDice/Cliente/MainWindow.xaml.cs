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
using System.Windows.Media;
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

        int numeroJugador;

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

                colores = new List<string>();

                writer.WriteLine("@INSCRIBIR@" + this.txt_nombre.Text + "@");
                writer.Flush();

                string[] data = reader.ReadLine().Split('@');

                Debug.WriteLine(data[2]);
                if (data[1] == "OK") // NOS CONECTAMOS
                {
                    this.txt_conexion.Text = "Conectado";
                    this.txt_conexion.Foreground = Brushes.Green;

                    if (data[2] == "1") // MOSTRAMOS QUE JUJGADOR SOMOS:
                    {
                        lbl_jugador.Content = "Jugador 1";
                        numeroJugador = 1;
                    }
                    else
                    {
                        lbl_jugador.Content = "Jugador 2";
                        numeroJugador = 2;
                    }

                    lbl_jugador.Foreground = Brushes.Black;
                    btn_conectar.IsEnabled = false;
                    btn_rojo.IsEnabled = true;
                    btn_verde.IsEnabled = true;
                    btn_azul.IsEnabled = true;
                    btn_amarillo.IsEnabled = true;
                    btn_enviar.IsEnabled = true;
                    btn_comprobar.IsEnabled = true;
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
            try
            {
                writer.WriteLine("@VER@");
                writer.Flush();

                MessageBox.Show(reader.ReadLine());
            }
            catch (Exception error)
            {
                Debug.WriteLine("Error: " + error.ToString());
            }
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
                string envio = "@JUGADA@" + numeroJugador + "@";

                foreach (string s in colores)
                {
                    envio += s + "@";
                }

                //MessageBox.Show(envio);
                writer.WriteLine(envio);
                writer.Flush();

                // Reiniciamos la lista para preparar la siguiente Jugada:
                colores = new List<string>();

                string[] data = reader.ReadLine().Split('@');
                if (data[1] == "OK")
                {
                    if (data[2] == "CORRECTO")
                    {
                        MessageBox.Show("Combinación de colores correcta.");
                    }
                    else // Perdiste:
                    {
                        lbl_jugador.Content = "PERDISTE";
                        MessageBox.Show("Combinación de colores incorrecta, perdiste.");
                    }
                }
                else
                {
                    if (data[2] == "1A")
                        MessageBox.Show("No hay dos jugadores.");
                    else if (data[2] == "1B")
                        MessageBox.Show("Número de colores incorrectos.");
                    else if (data[2] == "2A")
                    {
                        if (lbl_jugador.Content.ToString() != "PERDISTE")
                            lbl_jugador.Content = "GANASTE";

                        MessageBox.Show("Ya hay un ganador.");
                    }
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
