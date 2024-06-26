﻿using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Protocolo
{
    // Clase para representar un pedido del cliente al servidor
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        // Método estático para procesar el mensaje recibido y convertirlo en un objeto Pedido
        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(), // Convertir el comando a mayúsculas
                Parametros = partes.Skip(1).ToArray() // Tomar el resto como parámetros
            };
        }

        // Sobrescribir el método ToString para convertir el pedido en una cadena
        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    // Clase para representar la respuesta del servidor al cliente
    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        // Sobrescribir el método ToString para convertir la respuesta en una cadena
        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    // Clase para manejar la comunicación a través del protocolo definido
    public class Protocolo
    {
        private TcpClient _cliente; // Cliente TCP
        private NetworkStream _flujo; // Flujo de datos para la comunicación

        public Protocolo(TcpClient cliente)
        {
            _cliente = cliente;
            _flujo = _cliente.GetStream();
        }

        // Método para enviar un pedido y recibir la respuesta correspondiente
        public Respuesta HazOperacion(Pedido pedido)
        {
            if (_flujo == null)
            {
                throw new InvalidOperationException("No hay conexión.");
            }
            try
            {
                // Enviar el pedido al servidor
                byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.ToString());
                _flujo.Write(bufferTx, 0, bufferTx.Length);

                // Recibir la respuesta del servidor
                byte[] bufferRx = new byte[1024];
                int bytesRecibidos = _flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensajeRespuesta = Encoding.UTF8.GetString(bufferRx, 0, bytesRecibidos);

                // Procesar la respuesta y devolverla como objeto Respuesta
                var partes = mensajeRespuesta.Split(' ');
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException)
            {
                throw;
            }
        }

        // Método estático para resolver los pedidos recibidos del cliente
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente, ref Dictionary<string, int> listadoClientes)
        {
            Respuesta respuesta = new Respuesta { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 && pedido.Parametros[0] == "root" && pedido.Parametros[1] == "admin20")
                    {
                        respuesta = new Random().Next(2) == 0
                        ? new Respuesta
                        {
                           Estado = "OK",
                           Mensaje = "ACCESO_CONCEDIDO"
                        }
                        : new Respuesta
                        {
                          Estado = "NOK",
                          Mensaje = "ACCESO_NEGADO"
                        };
                    }
                    else
                    {
                        respuesta.Estado = "NOK";
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        respuesta.Estado = "OK";
                        string placa = pedido.Parametros[2].ToUpper();

                        // Calcular el día de restricción
                        int digito = int.Parse(placa.Last().ToString());
                        switch (digito)
                        {
                            case 1:
                            case 2:
                                respuesta.Mensaje = "DIA 32"; // Lunes
                                break;
                            case 3:
                            case 4:
                                respuesta.Mensaje = "DIA 16"; // Martes
                                break;
                            case 5:
                            case 6:
                                respuesta.Mensaje = "DIA 8"; // Miércoles
                                break;
                            case 7:
                            case 8:
                                respuesta.Mensaje = "DIA 4"; // Jueves
                                break;
                            case 9:
                            case 0:
                                respuesta.Mensaje = "DIA 2"; // Viernes
                                break;
                            default:
                                respuesta.Mensaje = "DIA 0"; // Sin restricción
                                break;
                        }

                        // Actualizar el contador de consultas del cliente
                        if (listadoClientes.ContainsKey(direccionCliente))
                        {
                            listadoClientes[direccionCliente]++;
                        }
                        else
                        {
                            listadoClientes[direccionCliente] = 1;
                        }
                    }
                    else
                    {
                        respuesta.Estado = "NOK";
                        respuesta.Mensaje = "Parámetros insuficientes para CALCULO";
                    }
                    break;

                case "CONTADOR":
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        respuesta.Estado = "OK";
                        respuesta.Mensaje = $"{listadoClientes[direccionCliente]}";
                    }
                    else
                    {
                        respuesta.Estado = "OK";
                        respuesta.Mensaje = "0";
                    }
                    break;

                default:
                    respuesta.Estado = "NOK";
                    respuesta.Mensaje = "Comando no reconocido";
                    break;
            }

            return respuesta;
        }
    }
}