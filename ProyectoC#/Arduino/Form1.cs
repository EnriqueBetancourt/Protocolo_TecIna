using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace Arduino
{
    public partial class Form1 : Form
    {
        private SerialPort Port;            // Puerto serial
        private bool isClosed = false;      // Indica si se debe cerrar el hilo
        private bool isPortOpen = false;    // Indica si el puerto está abierto
        private string completo = "";       // Mensaje acumulado

        public Form1()
        {
            InitializeComponent();

            Port = new SerialPort
            {
                PortName = "COM6", // Configura el puerto
                BaudRate = 9600,
                ReadTimeout = 500
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread hilo = new Thread(EscucharSerial) { IsBackground = true };
            hilo.Start();
        }

        private void BtnCon_Click(object sender, EventArgs e)
        {
            BtnCon.Enabled = false;
            BtnCon.SendToBack();

            BtnDiscon.Enabled = true;
            BtnDiscon.BringToFront();

            try
            {
                Port.Open();       // Abre el puerto
                isPortOpen = true; // Marca que el puerto está abierto

                MessageBox.Show("Conexión establecida correctamente con el puerto.", "Conexión exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                isPortOpen = false;
                MessageBox.Show($"No se pudo conectar al puerto: {ex.Message}", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDiscon_Click(object sender, EventArgs e)
        {
            BtnDiscon.Enabled = false;
            BtnDiscon.SendToBack();

            BtnCon.Enabled = true;
            BtnCon.BringToFront();

            try
            {
                Port.Close();       // Cierra el puerto
                isPortOpen = false; // Marca que el puerto está cerrado

                MessageBox.Show("Conexión cerrada.", "Conexión cerrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cerrar el puerto: {ex.Message}", "Error al cerrar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EscucharSerial()
        {
            bool processingMessage = false; // Indica si se está procesando un mensaje
            string messageBuffer = "";      // Almacena temporalmente el mensaje completo

            while (!isClosed)
            {
                if (!isPortOpen) // Si el puerto no está abierto, espera
                {
                    Thread.Sleep(100); // Evita consumir CPU innecesariamente
                    continue;
                }

                try
                {
                    string data = Port.ReadLine().Trim(); // Lee una línea completa del puerto serial
                    foreach (char ch in data)
                    {
                        // Agregar cada dato recibido a txtPrueba
                        txtPrueba.Invoke(new MethodInvoker(() =>
                        {
                            txtPrueba.AppendText(ch.ToString()); // Muestra el dato en txtPrueba
                        }));

                        if (ch == '@') // Comienza un nuevo mensaje
                        {
                            processingMessage = true;
                            messageBuffer = ""; // Limpia el buffer para un nuevo mensaje

                            // Cambia el texto del label al iniciar procesamiento
                            labelProceso.Invoke(new MethodInvoker(() =>
                            {
                                labelProceso.Text = "Procesando";
                            }));
                        }
                        else if (ch == '-' && processingMessage) // Finaliza el mensaje
                        {
                            processingMessage = false;
                            completo += messageBuffer + Environment.NewLine; // Almacena el mensaje completo
                            messageBuffer = ""; // Limpia el buffer

                            // Cambia el texto del label al finalizar procesamiento
                            labelProceso.Invoke(new MethodInvoker(() =>
                            {
                                labelProceso.Text = "Finalizado";
                            }));

                            // Muestra el mensaje en el cuadro de texto txtAlgo
                            txtAlgo.Invoke(new MethodInvoker(delegate {
                                txtAlgo.Text = completo;
                            }));
                        }
                        else if (processingMessage) // Continúa almacenando los caracteres del mensaje
                        {
                            messageBuffer += ch;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // Ignorar timeouts del puerto serial
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error en la comunicación serial: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void FormConnection_FormClosed(object sender, FormClosedEventArgs e)
        {
            isClosed = true; // Marca que el formulario se ha cerrado
            if (Port.IsOpen)
                Port.Close(); // Cierra el puerto si aún está abierto
        }
    }
}
