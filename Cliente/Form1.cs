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
        private TcpClient remoto;
        private NetworkStream flujo;

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                // Establecer la conexión con el servidor
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se pudo establecer conexión " + ex.Message,
                    "ERROR");
            }

            // Deshabilitar los controles relacionados con la placa
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
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña",
                    "ADVERTENCIA");
                return;
            }

            // Crear un objeto Pedido con los datos de inicio de sesión
            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };
            // Llamar al método HazOperacion de la clase Protocolo para realizar la operación de inicio de sesión
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(pedido, flujo);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                // Habilitar los controles relacionados con la placa y deshabilitar los controles de inicio de sesión
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else if (respuesta.Estado == "NOK" && respuesta.Mensaje == "ACCESO_NEGADO")
            {
                // Deshabilitar los controles relacionados con la placa y habilitar los controles de inicio de sesión
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales",
                    "ERROR");
                txtUsuario.Focus();
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            // Crear un objeto Pedido con los datos de consulta
            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };

            // Llamar al método HazOperacion de la clase Protocolo para realizar la operación de consulta
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(pedido, flujo);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }
            // este bloque de código se encarga de enviar una consulta al servidor, recibir la respuesta y
            // mostrar los resultados en la interfaz de usuario. También se encarga de actualizar las casillas
            // de verificación según el resultado obtenido.

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
                MessageBox.Show("Se recibió: " + respuesta.Mensaje,
                    "INFORMACIÓN");
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
            String mensaje = "hola";

            // Crear un objeto Pedido con los datos de solicitud de contador
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new[] { mensaje }
            };

            // Llamar al método HazOperacion de la clase Protocolo para realizar la operación de solicitud de contador
            Respuesta respuesta = Protocolo.Protocolo.HazOperacion(pedido, flujo);
            //El if en cuestión verifica si la variable respuesta es igual a null. Si es así, muestra un mensaje de
            //error indicando que hubo un error y luego retorna de la función. Esto se hace para manejar el caso
            //en el que no se reciba ninguna respuesta del servidor.
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");

            }
            else
            {
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + partes[0],
                    "INFORMACIÓN");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cerrar el flujo de red y la conexión con el servidor al cerrar el formulario
            flujo?.Close();
            if (remoto != null)
            {
                if (remoto.Connected)
                    remoto.Close();
            }
        }

        private void mayusculas(object sender, KeyPressEventArgs e)
        {
            // Convertir el carácter ingresado a mayúsculas
            e.KeyChar = char.ToUpper(e.KeyChar);
        }
    }
}
