﻿using System;
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        private TcpClient remoto; // Cliente TCP para la conexión al servidor
        private Protocolo.Protocolo protocolo; // Protocolo de comunicación para manejar los mensajes

        public FrmValidador()
        {
            InitializeComponent(); // Inicializar componentes del formulario
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                // Intentar conectar al servidor en localhost y puerto 8080
                remoto = new TcpClient("127.0.0.1", 8080);
                protocolo = new Protocolo.Protocolo(remoto);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("No se pudo establecer conexión " + ex.Message, "ERROR");
            }

            // Desactivar paneles y checkboxes iniciales
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
            // Obtener usuario y contraseña de los campos de texto
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;
            if (usuario == "" || contraseña == "")
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña", "ADVERTENCIA");
                return;
            }

            // Crear el pedido de ingreso con el comando y parámetros
            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };

            Respuesta respuesta;
            try
            {
                // Enviar el pedido y recibir la respuesta del servidor
                respuesta = protocolo.HazOperacion(pedido);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error: " + ex.Message, "ERROR");
                return;
            }

            // Procesar la respuesta del servidor
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

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            // Obtener modelo, marca y placa de los campos de texto
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            // Crear el pedido de cálculo con el comando y parámetros
            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };

            Respuesta respuesta;
            try
            {
                // Enviar el pedido y recibir la respuesta del servidor
                respuesta = protocolo.HazOperacion(pedido);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error: " + ex.Message, "ERROR");
                return;
            }

            // Procesar la respuesta del servidor
            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
                // Desmarcar todos los checkboxes
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
                // Establecer los checkboxes según la respuesta recibida
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
            // Crear un mensaje de solicitud para obtener el número de consultas
            string mensaje = "hola";

            // Crear el pedido de contador con el comando y parámetros
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new[] { mensaje }
            };

            Respuesta respuesta;
            try
            {
                // Enviar el pedido y recibir la respuesta del servidor
                respuesta = protocolo.HazOperacion(pedido);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error: " + ex.Message, "ERROR");
                return;
            }

            // Procesar la respuesta del servidor
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

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cerrar la conexión TCP si está activa
            if (remoto != null && remoto.Connected)
            {
                remoto.Close();
            }
        }
    }
}