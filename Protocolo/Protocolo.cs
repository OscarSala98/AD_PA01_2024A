using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Protocolo
{

    public class Protocolo
    {
        private static Dictionary<string, int> listadoClientes
            = new Dictionary<string, int>();


        //--------------------------------------------------------------------------------
        //Todos los metodos que suara el servidor.
        // Este método resuelve el pedido recibido por el servidor y devuelve una respuesta.
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            switch (pedido.Comando)
            {
                case "INGRESO":
                    // Verifica si los parámetros del pedido son correctos y si coinciden con los valores esperados.
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        // Genera una respuesta aleatoria de acceso concedido o acceso negado.
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
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    // Verifica si los parámetros del pedido son correctos.
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        // Valida la placa utilizando una expresión regular.
                        if (ValidarPlaca(placa))
                        {
                            // Obtiene el indicador de día correspondiente a la placa.
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };
                            // Incrementa el contador de solicitudes del cliente.
                            ContadorCliente(direccionCliente);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    // Verifica si el cliente tiene solicitudes previas registradas.
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        // Devuelve el número de solicitudes del cliente.
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

        // Este método valida si una placa cumple con el formato requerido.
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        // Este método obtiene el indicador de día correspondiente a una placa.
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

        // Este método incrementa el contador de solicitudes del cliente.
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

        //--------------------------------------------------------------------------------
        //Todos los metodos que usara el cliente.

        // Este método envía un pedido al servidor y devuelve la respuesta recibida.
        public static Respuesta HazOperacion(Pedido pedido, NetworkStream flujo)
        {
            if (flujo == null)
            {
                return null;
            }
            try
            {
                byte[] bufferTx = Encoding.UTF8.GetBytes(
                    pedido.Comando + " " + string.Join(" ", pedido.Parametros));

                flujo.Write(bufferTx, 0, bufferTx.Length);

                byte[] bufferRx = new byte[1024];

                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);

                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                var partes = mensaje.Split(' ');

                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {

            }
            return null;
        }



    }




    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        // Este método procesa un mensaje y devuelve un objeto Pedido.
        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }
    // la clase respuesta hace referencia a la respuesta que se le dara al cliente.
    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

}
