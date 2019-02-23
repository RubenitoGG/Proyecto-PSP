using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace Servidor
{
    public partial class Form1 : Form
    {
        string nombreJ1;
        string nombreJ2;
        int ronda = 1;
        List<string> colores;
        string ganador;

        public Form1()
        {
            InitializeComponent();
            colores = new List<string>();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Método que espera infinitos clientes y crea un hilo para manejar cada uno de ellos.
        /// </summary>
        private void EsperarClientes()
        {
            TcpListener newSock = new TcpListener(IPAddress.Any, 2000);
            newSock.Start();

            // ESCUCHA INFINITA DE CLIENTES:
            while (true)
            {
                TcpClient client = newSock.AcceptTcpClient(); // LÍNEA BLOQUEANTE, ESPERAMOS A QUE UN CLIENTE LO ATENDEMOS.
                Thread t = new Thread(() => ManejarCliente(client)); // CREAMOS HILO PARA ATENDER AL CLIENTE.
                t.Start();
            }
        }

        /// <summary>
        /// Método dónde atendemos las peticiones del cliente y le mandamos las respuestas.
        /// </summary>
        /// <param name="cliente"></param>
        private void ManejarCliente(TcpClient cliente)
        {
            // VARIABLES PARA COMUNICARSE CON EL CLIENTE:
            NetworkStream netStream = cliente.GetStream();
            StreamReader reader = new StreamReader(netStream);
            StreamWriter writer = new StreamWriter(netStream);

            // ESCUCHA INFINITA DE PETICIONES ENVIADAS DESDE EL CLIENTE:
            while (true)
            {
                try
                {
                    string[] data = reader.ReadLine().Split('@');

                    #region -Inscribir-
                    if (data[1] == "INSCRIBIR")
                    {
                        if (data[2] == "")
                        {
                            writer.WriteLine("@NOK@0B@");
                            writer.Flush();
                        }
                        else if (nombreJ1 == "") // GUARDAMOS JUGADOR 1:
                        {
                            nombreJ1 = data[1];

                            // ENVIAMOS RESPUESTA AL JUGADOR CON SU NÚMERO:
                            writer.WriteLine("@OK@1@");
                            writer.Flush();
                        }
                        else if (nombreJ2 == "") // GUARDAMOS JUGADOR 2:
                        {
                            nombreJ2 = data[1];

                            // ENVIAMOS RESPUESTA AL JUGADOR CON SU NÚMERO:
                            writer.WriteLine("@OK@2@");
                            writer.Flush();
                        }
                        else // YA TENEMOS DOS JUGADORES:
                        {
                            writer.WriteLine("@NOK@0A@");
                            writer.Flush();
                        }
                    }
                    #endregion
                    #region -Jugada-
                    else if (data[1] == "JUGADA")
                    {
                        if(ganador != null)
                        {
                            writer.WriteLine("Has ganado!");
                            writer.Flush();
                            cliente.Close();
                        }

                        if (nombreJ1 == "" || nombreJ2 == "") // SI NO HAY DOS JUGADORES DEVUELVE UN ERROR:
                        {
                            writer.WriteLine("@NOK@1A@");
                            writer.Flush();
                        }
                        else
                        {
                            // RECOGEMOS LA LISTA DE COLORES:
                            List<string> colores = new List<string>();
                            for (int i = 2; i < data.Length - 2; i++) // NO SEGURO SI ES -1 O -2.
                            {
                                colores.Add(data[i]);
                            }
                            // COMPROBAMOS QUE NO HAY COLORES DE MENOS NI DE MÁS:
                            if(colores.Count() == ronda)
                            {
                                for (int i = 0; i < colores.Count() - 1; i++) // NO SEGURO SI ES -1 O -2.
                                {
                                    // COMPROBAMOS COLOR A COLOR SI ESTÁN EN ORDEN:
                                    if(colores.ElementAt(i) != this.colores.ElementAt(i))
                                    {
                                        writer.WriteLine("@OK@INCORRECTO@");  // MESANJE DE COLORES INCORRECTOS.
                                        writer.Flush();

                                        // NOMBRAMOS GANADOR:
                                        if (ronda % 2 == 0)
                                            ganador = nombreJ1;
                                        else
                                            ganador = nombreJ2;

                                        cliente.Close();
                                    }

                                    // SI AÚN NO HAY GANADOR:
                                    if(ganador == null)
                                    {
                                        ronda++;
                                        writer.WriteLine("@OK@CORRECTO@"); // MENSAJE DE COLORES CORRECTOS.
                                        writer.Flush();
                                    }
                                }
                            }
                            else // ERROR DE NÚMERO DE COLORES INCORRECTO.
                            {
                                writer.WriteLine("@NOK@1B@");
                                writer.Flush();
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error.ToString());
                }
            }
        }
    }
}
