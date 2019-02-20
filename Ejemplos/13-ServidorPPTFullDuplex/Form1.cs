using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace _13_ServidorPPTFullDuplex
{
    public partial class Form1 : Form
    {
        string jugador1 = ""; //nick
        string idJug1 = ""; //referncia IP:port
        string jugada1 = ""; //piedra,papel,tijera
        int puntos1 = 0;
        string jugador2 = ""; //nick
        string idJug2 = ""; //referncia IP:port
        string jugada2 = ""; //piedra,papel,tijera

        string listenPortJug1 = "";
        string listenPortJug2 = "";
        string ipJug1 = "";
        string ipJug2 = "";

        int numJugada = 1; //indice de jugada en curso
        string[] textoVueltaJugada = new string[100]; //guardar resultado jugadas (historico)

        Object o = new object(); //para lock

        int puntos2 = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void ManejarCliente(TcpClient cli)
        {
            string data;
            NetworkStream ns = cli.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns, Encoding.UTF8); //para ñ y acentos (no funciona con telnet, solo clientes .net)


            //informe de protocolo
            sw.WriteLine("#INSCRIBIR#nombre#puertoEscucha#");
            sw.WriteLine("#JUGADA#{piedra|papel|tijera}#");
            //sw.WriteLine("#RESULTADOJUGADA#numeroJugada#");
            sw.WriteLine("#PUNTUACION#");
            sw.Flush();

            //escucha infinita de peticiones enviadas desde un cliente
            while (true)
            {
                try
                {
                    data = sr.ReadLine(); //linea bloqueante
                    Debug.WriteLine(data);
                    String[] subdatos = data.Split('#');
                    #region comINSCRIBIR
                    if (subdatos[1] == "INSCRIBIR")
                    {
                        if (jugador1 == "")
                        {
                            jugador1 = subdatos[2];
                            idJug1 = cli.Client.RemoteEndPoint.ToString();
                            ipJug1 = idJug1.Split(':')[0];
                            listenPortJug1 = subdatos[3];
                            sw.WriteLine("#OK#");
                            sw.Flush();
                        }
                        else if (jugador2 == "")
                        {
                            jugador2 = subdatos[2];
                            idJug2 = cli.Client.RemoteEndPoint.ToString();
                            ipJug2 = idJug2.Split(':')[0];
                            listenPortJug2 = subdatos[3];
                            sw.WriteLine("#OK#");
                            sw.Flush();
                        }
                        else
                        {
                            sw.WriteLine("#NOK#ya hay dos jugadores#");
                            sw.Flush();
                        }
                    }
                    #endregion comINSCRIBIR
                    #region comJUGADA   
                    if (subdatos[1] == "JUGADA")
                    {
                        if ((subdatos[2] != "piedra") && (subdatos[2] != "papel") && (subdatos[2] != "tijera"))
                        {
                            sw.WriteLine("#NOK#valores validos: piedra/papel/tijera#");
                            sw.Flush();
                        }
                        //comprobar quien hace la jugada "y la guardamos"
                        else if (idJug1 == cli.Client.RemoteEndPoint.ToString() ||
                                  idJug2 == cli.Client.RemoteEndPoint.ToString())
                        {
                            if (idJug1 == cli.Client.RemoteEndPoint.ToString())
                            { //estamos con el jugador 1
                                jugada1 = subdatos[2];
                            }
                            else
                            { //estamos con el jugador 2
                                jugada2 = subdatos[2];
                            }
                            sw.WriteLine("#OK#" + numJugada + "#");
                            sw.Flush();
                            lock (o) //solo uno puede ejecutar ComprobarGanador 
                            {
                                //comprobar si tengo emitidas el par de jugadas
                                if (jugada1 != "" && jugada2 != "")
                                {
                                    ComprobarGanador();
                                }
                            }
                        }
                        else
                        {
                            sw.WriteLine("#NOK#el jugador no esta en la partida#");
                            sw.Flush();
                        }
                    }
                    #endregion
                    #region comPUNTUACION
                    if (subdatos[1] == "PUNTUACION")
                    {
                        sw.WriteLine("#OK#" + jugador1 + ":" + puntos1.ToString() + "#"
                                           + jugador2 + ":" + puntos2.ToString() + "#");
                        sw.Flush();
                    }
                    #endregion

                }
                catch (Exception error)
                {
                    Debug.WriteLine("Error: " + error.ToString());
                }
            }
            ns.Close();
            cli.Close();

        }

        private void ComprobarGanador()
        {
            //resolver la jugada
            //gana el 1
            if ((jugada1 == "piedra" && jugada2 == "tijera") ||
                (jugada1 == "papel" && jugada2 == "piedra") ||
                (jugada1 == "tijera" && jugada2 == "papel"))
            {
                textoVueltaJugada[numJugada - 1] = "#OK#GANADOR:" + jugador1 + "#";
                puntos1++;
            }
            //gana el 2
            else if ((jugada2 == "piedra" && jugada1 == "tijera") ||
                (jugada2 == "papel" && jugada1 == "piedra") ||
                (jugada2 == "tijera" && jugada1 == "papel"))
            {
                textoVueltaJugada[numJugada - 1] = "#OK#GANADOR:" + jugador2 + "#";
                puntos2++;
            }
            //empate
            else
            {
                textoVueltaJugada[numJugada - 1] = "#OK#empate#";
            }
            ComunicarResultadoClientes();
            numJugada++; //pasamos a la siguiente jugada
            jugada1 = "";
            jugada2 = "";
        }

        delegate void DelegadoRespuesta();
        string dato;
        private void EscribirFormulario()
        {
            this.label1.Text += dato + "@@@@"; 
        }

        private void ComunicarResultadoClientes()
        {
            //comunico resultado a los clientes
            TcpClient cliente;
            NetworkStream ns;
            StreamReader sr;
            StreamWriter sw;

            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);

            //envío info al primer cliente
            cliente = new TcpClient(ipJug1, System.Convert.ToInt32(listenPortJug1));
            ns = cliente.GetStream();
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.WriteLine(textoVueltaJugada[numJugada - 1]);
            sw.Flush();
            dato = sr.ReadLine();
            cliente.Close();
            this.Invoke(dr);

            //envío info al segundo cliente
            cliente = new TcpClient(ipJug2, System.Convert.ToInt32(listenPortJug2));
            ns = cliente.GetStream();
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.WriteLine(textoVueltaJugada[numJugada - 1]);
            sw.Flush();
            dato = sr.ReadLine();
            cliente.Close();
            this.Invoke(dr);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //thread de recepción de clientes
            Thread t = new Thread(this.EsperarClientes);
            t.Start();
            this.button1.Enabled = false;


        }

        private void EsperarClientes()
        {
            TcpListener newsock = new TcpListener(IPAddress.Any, 2000);
            newsock.Start();

            //escucha infinita de clientes que quieran conectarse
            while (true)
            {
                TcpClient client = newsock.AcceptTcpClient(); //linea bloqueante
                Thread t = new Thread(() => this.ManejarCliente(client));
                t.Start();
            }
        }
    }
}
