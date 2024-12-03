// ************************************************************************
// Practica 07
// Sala Oscar
// Fecha de realización: 27/11/2024
// Fecha de entrega: 04/11/2024
// Resultados
//  El programa en ejecución mostró un flujo funcional en el inicio de sesión,
//  donde el cliente envía correctamente las credenciales al servidor, que responde en el
//  formato esperado (OK ACCESO_CONCEDIDO o NOK ACCESO_NEGADO), habilitando o bloqueando
//  los controles adicionales según corresponda. Las consultas de placas y el contador de
//  solicitudes también funcionaron como se esperaba, con el cliente procesando correctamente
//  las respuestas del servidor y mostrando los resultados en la interfaz.
//  La comunicación cliente-servidor, basada en el protocolo de mensajes, demostró ser eficiente,
//  aunque es necesario realizar pruebas adicionales con entradas no válidas y revisar los logs del
//  servidor para garantizar que no existan errores ocultos o problemas en escenarios más complejos.

// ************************************************************************

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
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se puedo establecer conexión " + ex.Message,
                    "ERROR");
            }


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

            string mensaje = $"INGRESO {usuario} {contraseña}";
            Respuesta respuesta = HazOperacion(mensaje);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }
            string[] partes = respuesta.Mensaje.Split(' ');
            string estado = "OK";
            string mensajeRespuesta = "ACCESO_CONCEDIDO";


            if (estado == "OK" && mensajeRespuesta == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();
            }
            else if (estado == "NOK" && mensajeRespuesta == "ACCESO_NEGADO")
            {
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales",
                    "ERROR");
                txtUsuario.Focus();
            }
        }

        private Respuesta HazOperacion(string mensaje)
        {
            if (flujo == null)
            {
                MessageBox.Show("No hay conexión", "ERROR");
                return null;
            }

            try
            {
                // Enviar el mensaje al servidor
                byte[] bufferTx = Encoding.UTF8.GetBytes(mensaje);
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Recibir la respuesta desde el servidor
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);

                // Convertir la respuesta a un string
                string respuestaStr = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                // Procesar el string y convertirlo a un objeto Respuesta
                var partes = respuestaStr.Split(' ');
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = partes.Length > 1 ? partes[1] : string.Empty
                };
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Error al intentar transmitir " + ex.Message, "ERROR");
            }

            return null;
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            string mensaje = $"CALCULO {modelo} {marca} {placa}";
            Respuesta respuesta = HazOperacion(mensaje);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            string[] partes = respuesta.Mensaje.Split(' ');
            string estado = partes[0];
            string mensajeRespuesta = string.Join(" ", partes.Skip(1));

            if (estado == "NOK")
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

                MessageBox.Show("Se recibió: " + mensajeRespuesta,
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

            Respuesta respuesta = HazOperacion(mensaje);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            string[] partes = respuesta.Mensaje.Split(' ');
            string estado = partes[0];
            string mensajeRespuesta = string.Join(" ", partes.Skip(1));

            if (estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");

            }
            else
            {

                MessageBox.Show("El número de pedidos recibidos en este cliente es " + partes[0],
                    "INFORMACIÓN");
            }
        }

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
