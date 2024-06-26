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
        private TcpClient remoto; // Cliente TCP para conexión con el servidor
        private NetworkStream flujo; // Flujo de red para enviar y recibir datos

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                remoto = new TcpClient("127.0.0.1", 8080); // Intenta conectar al servidor en localhost (127.0.0.1) en el puerto 8080
                flujo = remoto.GetStream(); // Obtiene el flujo de red del cliente
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se pudo establecer conexión: " + ex.Message, "ERROR"); // Muestra mensaje de error si no se puede conectar
                flujo?.Close(); // Cierra el flujo de red si se abrió
                remoto?.Close(); // Cierra la conexión TCP si se estableció
            }

            // Deshabilita controles relacionados con el ingreso de placa hasta iniciar sesión correctamente
            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;
            if (usuario == "" || contraseña == "")
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña", "ADVERTENCIA"); // Advierte sobre campos vacíos
                return;
            }

            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };

            Respuesta respuesta = HazOperacion(pedido); // Realiza la operación correspondiente al pedido

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error al procesar la solicitud", "ERROR"); // Muestra mensaje de error si no hay respuesta válida
                return;
            }

            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                // Habilita el panel de ingreso de placa y deshabilita el panel de login
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN"); // Informa sobre acceso concedido
                txtModelo.Focus(); // Pone el foco en el campo de modelo
            }
            else if (respuesta.Estado == "NOK" && respuesta.Mensaje == "ACCESO_NEGADO")
            {
                // Restablece la interfaz de login y muestra mensaje de acceso denegado
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales", "ERROR");
                txtUsuario.Focus(); // Pone el foco en el campo de usuario
            }
        }

        private Respuesta HazOperacion(Pedido pedido)
        {
            if (flujo == null)
            {
                MessageBox.Show("No hay conexión", "ERROR"); // Muestra mensaje de error si no hay conexión activa
                return null;
            }

            try
            {
                // Convierte el pedido a bytes y lo envía al servidor
                byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.Comando + " " + string.Join(" ", pedido.Parametros));
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Lee la respuesta del servidor
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                // Procesa la respuesta del servidor y la devuelve como objeto Respuesta
                var partes = mensaje.Split(' ');
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Error al intentar transmitir datos: " + ex.Message, "ERROR"); // Muestra mensaje de error si hay problemas de transmisión
            }

            return null;
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };

            Respuesta respuesta = HazOperacion(pedido); // Realiza la operación correspondiente al pedido

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error al procesar la solicitud", "ERROR"); // Muestra mensaje de error si no hay respuesta válida
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR"); // Informa sobre error en la solicitud
                // Reinicia los checkboxes de días
                chkLunes.Checked = false;
                chkMartes.Checked = false;
                chkMiercoles.Checked = false;
                chkJueves.Checked = false;
                chkViernes.Checked = false;
            }
            else
            {
                // Procesa y muestra la respuesta obtenida del servidor
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

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new string[0] // No se necesitan parámetros para el comando CONTADOR
            };

            Respuesta respuesta = HazOperacion(pedido); // Realiza la operación correspondiente al pedido

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error al obtener el contador", "ERROR"); // Muestra mensaje de error si no hay respuesta válida
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR"); // Informa sobre error en la solicitud
            }
            else
            {
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + respuesta.Mensaje,
                    "INFORMACIÓN"); // Muestra el número de pedidos recibidos por el cliente
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cierra el flujo de red y la conexión TCP al cerrar el formulario
            if (flujo != null)
                flujo.Close();
            if (remoto != null)
                if (remoto.Connected)
                    remoto.Close();
        }
    }
}
