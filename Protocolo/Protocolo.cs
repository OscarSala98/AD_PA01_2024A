using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Protocolo
{
    /// Clase que representa un pedido del cliente
    public class Pedido
    {
        // Variable para identificar el tipo de pedido
        public string Comando { get; set; }
        // Variable que contiene los parametros del pedido
        public string[] Parametros { get; set; }

        // Procesa un mensaje y lo convierte en un objeto Pedido
        public static Pedido Procesar(string mensaje)
        {
            // Separa el mensaje en partes usando espacios como delimitador.
            var partes = mensaje.Split(' ');
            // Crea y retorna un nuevo objeto Pedido con el comando y los parámetros.
            return new Pedido
            {
                Comando = partes[0].ToUpper(), // El comando se convierte a mayúsculas.
                Parametros = partes.Skip(1).ToArray() // Los parámetros son todos los elementos después del primer espacio.
            };
        }

        // Convierte el pedido a una cadena de texto
        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }
    /// Clase que representa una respuesta del servidor
    public class Respuesta
    {
        // Variable que controla el estado de la respuesta
        public string Estado { get; set; }
        // Variable que guarda el mensaje
        public string Mensaje { get; set; }

        // Convierte la respuesta a una cadena de texto
        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }
    ///Operaciones
    public class Protocolo
    {
        // Diccionario para llevar el conteo de solicitudes por cliente
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>();
        /// <summary>
        /// Método para realizar una operación con el cliente remoto.
        /// </summary>
        /// <param name="remoto">Cliente Tcp conectado.</param>
        /// <param name="pedido">Pedido a procesar.</param>
        /// <returns>Respuesta obtenida del cliente remoto.</returns>
        public static Respuesta HazOperacion(TcpClient remoto, Pedido pedido)
        {
            // Verifica si el cliente está conectado
            if (remoto == null || !remoto.Connected)
            {
                return Protocolo.CrearRespuesta("NOK", "No hay conexión");
            }

            NetworkStream flujo = null;
            try
            {
                flujo = remoto.GetStream();

                // Codifica el pedido en formato UTF-8 y lo envía al cliente
                byte[] bufferTx = Encoding.UTF8.GetBytes(
                    pedido.Comando + " " + string.Join(" ", pedido.Parametros));

                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Lee la respuesta del cliente
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                // Procesa la respuesta del cliente
                var partes = mensaje.Split(' ');
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {
                // Manejo de errores al transmitir datos
                return Protocolo.CrearRespuesta("NOK", "Error al transmitir: " + ex.Message);
                flujo?.Close();
            }
        }

        /// <summary>
        /// Método para resolver el pedido recibido del cliente.
        /// </summary>
        /// <param name="pedido">Pedido recibido del cliente.</param>
        /// <param name="direccionCliente">Dirección remota del cliente.</param>
        /// <returns>Respuesta generada según el pedido.</returns>
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            Respuesta respuesta = new Respuesta
            {
                Estado = "NOK",
                Mensaje = "Comando no reconocido"
            };

            switch (pedido.Comando)
            {
                // Caso para manejar el comando de ingreso
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
                // Caso para manejar el comando de cálculo
                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string placa = pedido.Parametros[2];
                        if (ValidarPlaca(placa))
                        {
                            // Obtiene el indicador de día basado en la placa
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };
                            // Incrementa el contador de solicitudes para el cliente
                            ContadorCliente(direccionCliente);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;
                // Caso para manejar el comando de contador
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
        // Método para validar una placa de vehículo.
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        // Método para obtener el indicador de día basado en la placa del vehículo.
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
        /// <summary>
        /// Método para crear una respuesta con un estado y mensaje dados.
        /// </summary>
        /// <param name="estado">Estado de la respuesta.</param>
        /// <param name="mensaje">Mensaje de la respuesta.</param>
        /// <returns>Respuesta creada con los parámetros dados.</returns>  
        private static Respuesta CrearRespuesta(string estado, string mensaje)
        {
            return new Respuesta { Estado = estado, Mensaje = mensaje };
        }

        // Método para incrementar el contador de solicitudes para un cliente.
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