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
        string ipJ1;
        string ipJ2;

        int ronda = 1;
        List<string> colores;
        bool ganador;

        public Form1()
        {
            InitializeComponent();
            colores = new List<string>();

            ronda = 1;
            nombreJ1 = "";
            nombreJ2 = "";

            ganador = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(this.EsperarClientes);
            t.Start();
            button1.Enabled = false;
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
                    
                    if (ganador)
                    {
                        writer.WriteLine("@NOK@2A@");
                        writer.Flush();

                        cliente.Close();
                        netStream.Close();

                        return;
                    }

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
                            ipJ1 = cliente.Client.RemoteEndPoint.ToString().Split(':')[0];

                            // ENVIAMOS RESPUESTA AL JUGADOR CON SU NÚMERO:
                            writer.WriteLine("@OK@1@");
                            writer.Flush();
                        }
                        else if (nombreJ2 == "") // GUARDAMOS JUGADOR 2:
                        {
                            nombreJ2 = data[1];
                            ipJ2 = cliente.Client.RemoteEndPoint.ToString().Split(':')[0];

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
                        bool r = true;
                        //Debug.WriteLine(ronda);

                        if (ronda % 2 == 0) // ronda par == jugador 2
                        {
                            if (Convert.ToInt32(data[2]) != 2)
                            {
                                writer.WriteLine("@NOK@1C@");
                                writer.Flush();
                                r = false;
                            }
                        }
                        else //ronda impar == jugador 1
                        {
                            if (Convert.ToInt32(data[2]) == 2)
                            {
                                writer.WriteLine("@NOK@1C@");
                                writer.Flush();
                                r = false;
                            }
                        }

                        // TU TURNO:
                        if (r)
                        {
                            Debug.WriteLine(ronda);
                            
                            if (nombreJ1 == "" || nombreJ2 == "") // SI NO HAY DOS JUGADORES DEVUELVE UN ERROR:
                            {
                                writer.WriteLine("@NOK@1A@");
                                writer.Flush();
                            }
                            else
                            {
                                // RECOGEMOS LA LISTA DE COLORES:
                                List<string> nuevosColores = new List<string>();
                                for (int i = 3; i < data.Length - 1; i++)
                                {
                                    nuevosColores.Add(data[i]);
                                    Debug.WriteLine(data[i]);
                                }
                                // COMPROBAMOS QUE NO HAY COLORES DE MENOS NI DE MÁS:
                                if (nuevosColores.Count() == ronda)
                                {
                                    // COMPROBAMOS LOS COLORES:
                                    if (ComprobarLista(nuevosColores))
                                    {
                                        writer.WriteLine("@OK@CORRECTO@");
                                        writer.Flush();
                                        ronda++;
                                    }
                                    else
                                    {
                                        ganador = true;
                                        writer.WriteLine("@OK@INCORRECTO@");
                                        writer.Flush();
                                    }
                                }
                                else // ERROR DE NÚMERO DE COLORES INCORRECTO.
                                {
                                    writer.WriteLine("@NOK@1B@");
                                    writer.Flush();
                                }
                            }
                        }
                    }
                    #endregion
                    #region -VerLista-
                    else if (data[1] == "VER")
                    {
                        string envio = "@";
                        foreach (string c in colores)
                        {
                            envio += c + "@";
                        }

                        writer.WriteLine(envio);
                        writer.Flush();
                    }
                    #endregion
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error.ToString());
                }
            }
        }

        private bool ComprobarLista(List<string> nuevosColores)
        {
            string ultimo = nuevosColores.ElementAt(nuevosColores.Count() - 1);

            // Cogemos los colores a comprobar:
            nuevosColores.RemoveAt(nuevosColores.Count() - 1);

            if (colores.Count == 0)
            {
                this.colores.Add(ultimo);
                return true;
            }

            for (int i = 0; i < nuevosColores.Count(); i++)
            {
                Console.WriteLine(nuevosColores.ElementAt(i) + " --- " + this.colores.ElementAt(i));
                if (nuevosColores.ElementAt(i) != this.colores.ElementAt(i))
                    return false;
            }

            this.colores.Add(ultimo);
            return true;
        }
    }
}
