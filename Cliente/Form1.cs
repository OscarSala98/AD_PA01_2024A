using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {

        // Variables de clase para el cliente TCP y el flujo de datos.
        private TcpClient remoto;
        private NetworkStream flujo;

        // Constructor de la clase, inicializa los componentes del formulario.
        public FrmValidador()
        {
            InitializeComponent();
        }

        // Método que se ejecuta cuando el formulario se carga.
        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                // Intenta conectar al servidor TCP en localhost en el puerto 8080.
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                // Manejo de excepciones en caso de error al conectar.
                MessageBox.Show("No se puedo establecer conexión " + ex.Message, "ERROR");
                flujo?.Close();
                remoto?.Close();
            }

            // Desactiva los paneles y checkboxes al inicio.
            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }

        // Método que se ejecuta cuando se hace clic en el botón "Iniciar".
        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;
            if (usuario == "" || contraseña == "")
            {
                // Verifica que ambos campos estén llenos.
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña", "ADVERTENCIA");
                return;
            }

            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(remoto, pedido);

            // Procesa la respuesta del servidor.
            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else if (respuesta.Estado == "NOK" && respuesta.Mensaje == "ACCESO_NEGADO")
            {
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales", "ERROR");
                txtUsuario.Focus();
            }
        }

        // Método que se ejecuta cuando se hace clic en el botón "Consultar".
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            // Crea un nuevo pedido de cálculo.
            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(remoto, pedido);

            // Procesa la respuesta del servidor.
            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
                chkLunes.Checked = false;
                chkMartes.Checked = false;
                chkMiercoles.Checked = false;
                chkJueves.Checked = false;
                chkViernes.Checked = false;
            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + respuesta.Mensaje, "INFORMACIÓN");
                byte resultado = Byte.Parse(partes[1]);
                switch (resultado)
                {
                    case 0b00100000:
                        chkLunes.Checked = true;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00010000:
                        chkMartes.Checked = true;
                        chkLunes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00001000:
                        chkMiercoles.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000100:
                        chkJueves.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00000010:
                        chkViernes.Checked = true;
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        break;
                    default:
                        chkLunes.Checked = false;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                }
            }
        }

        // Método que se ejecuta cuando se hace clic en el botón "Número de Consultas".
        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            //String mensaje = "hola";

            // Crea un nuevo pedido para obtener el contador.
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new[] { "hola" }
            };
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(remoto, pedido);

            // Procesa la respuesta del servidor.
            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + partes[0], "INFORMACIÓN");
            }
        }

        // Método que se ejecuta cuando el formulario se está cerrando.
        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (flujo != null)
                flujo.Close();
            if (remoto != null)
                if (remoto.Connected)
                    remoto.Close();
        }
    }
}