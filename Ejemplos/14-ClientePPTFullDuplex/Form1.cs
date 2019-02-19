using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace _14_ClientePPTFullDuplex
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;
        String dato;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient(this.textBox1.Text, 2000);
                ns = client.GetStream();
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);
                dato = sr.ReadLine() + System.Environment.NewLine +
                       sr.ReadLine() + System.Environment.NewLine +
                       //sr.ReadLine() + System.Environment.NewLine +
                       sr.ReadLine();
                //no es necesario usar delegado porque tenemos un solo hilo
                this.label1.Text = dato;
            }
            catch (Exception error)
            {
                Debug.WriteLine("Error:" + error.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sw.WriteLine("#INSCRIBIR#" + this.textBox2.Text + "#"+this.textBox3.Text +"#");
            sw.Flush();
            dato = sr.ReadLine();

            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
            this.Invoke(dr);

            //abrir thread escucha
            Thread t = new Thread(this.EscuchaResultados);
            t.IsBackground = true;
            t.Start();
        }

        private void EscuchaResultados()
        {
            TcpListener newsock = new TcpListener(IPAddress.Any, System.Convert.ToInt32(this.textBox3.Text));
            newsock.Start();
            Debug.WriteLine("Escperando cliente");

            while (true)
            {
                TcpClient client = newsock.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);

                dato = sr.ReadLine();
                sw.WriteLine("#OK#");
                sw.Flush();

                DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
                this.Invoke(dr);

            }

        }
        delegate void DelegadoRespuesta();
        private void EscribirFormulario()
        {
            this.label1.Text = dato;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sw.WriteLine("#JUGADA#" + this.comboBox1.Text + "#");
            sw.Flush();
            dato = sr.ReadLine();
            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
            this.Invoke(dr);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sw.WriteLine("#PUNTUACION#" + this.comboBox1.Text + "#");
            sw.Flush();
            dato = sr.ReadLine();
            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
            this.Invoke(dr);
        }
    }
}
