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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;
        private static Dictionary<string, int> listadoClientes
            = new Dictionary<string, int>();
        private static Protocolo.Protocolo protocolo = new Protocolo.Protocolo(); // Instancia de Protocolo

        static void Main(string[] args)
        {
            try
            {
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor inició en el puerto 5000...");

                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al iniciar el servidor: " +
                    ex.Message);
            }
            finally
            {
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;

            try
            {
                flujo = cliente.GetStream();
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    // Recibir mensaje del cliente
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    Console.WriteLine("Se recibió: " + mensajeRx);

                    // Resolver el pedido utilizando Protocolo
                    string respuesta = protocolo.ResolverPedido(mensajeRx, cliente.Client.RemoteEndPoint.ToString(), listadoClientes);
                    Console.WriteLine("Se envió: " + respuesta);

                    // Enviar respuesta al cliente
                    bufferTx = Encoding.UTF8.GetBytes(respuesta);
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                flujo?.Close();
                cliente?.Close();
            }
        }

        private static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = "ACCESO_CONCEDIDO"
                        };
                    }
                    else
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "NOK",
                            Mensaje = "ACCESO_NEGADO"
                        };
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        if (ValidarPlaca(placa))
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };
                            ContadorCliente(direccionCliente);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = listadoClientes[direccionCliente].ToString()
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }

            return respuesta;
        }

        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }

        private static void ContadorCliente(string direccionCliente)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++;
            }
            else
            {
                listadoClientes[direccionCliente] = 1;
            }
        }

    }
}
