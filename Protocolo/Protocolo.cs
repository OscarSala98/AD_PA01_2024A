// *****************************************************************************
// Practica 07
// BrigethT
// Fecha de realización: 21/06/2024
// Fecha de entrega: 26/06/2024
// Conclusiones:
// *    El código muestra una forma de procesar mensajes de texto ya que los 
//      divide de una forma correcta en parámetros y tambien comandos lo que facilita la interpretación
//      y manipulación de los datos recibidos.
//
// *    El uso delas clases Pedido y Respuesta en un mismo documento se encuentran
//      bien diseñadas para representar las estructuras de datos necesarias en el protocolo que ser quiere
//      obtener ya que el método estático Procesar en la clase Pedido encapsula la lógica de
//      conversión de un mensaje de texto en un objeto de manera correcto para el objetivo requerido.
//
// Recomendaciones:
// *    Sería recomendable agregar validación para el mensaje de entrada en el método Procesar ya que
//      este podría generar varios errores.
//
// *    Es recomendable implementar el uso de excepciones en el código con el fin de no presentar problemas
//      al momento de ejecutar el protocolo.
//
// *****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Protocolo
{
    public static class Protocolo
    {
        public class Pedido
        {
            public string Comando { get; set; }
            public string[] Parametros { get; set; }

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

        public class Respuesta
        {
            public string Estado { get; set; }
            public string Mensaje { get; set; }

            public override string ToString()
            {
                return $"{Estado} {Mensaje}";
            }
        }

        // Método para realizar una operación basada en un pedido y obtener una respuesta
        public static Respuesta HazOperacion(Pedido pedido, TcpClient remoto, NetworkStream flujo)
        {
            // Verificar si el flujo de red es nulo
            if (flujo == null)
            {
                // Si el flujo es nulo, devuelve una respuesta nula
                return null;
            }

            try
            {
                // Convertir el pedido en bytes utilizando UTF-8 para enviar al cliente
                byte[] bufferTx = Encoding.UTF8.GetBytes(
                    pedido.Comando + " " + string.Join(" ", pedido.Parametros));

                // Enviar los bytes al cliente a través del flujo de red
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Preparar un buffer para recibir la respuesta del cliente
                byte[] bufferRx = new byte[1024];

                // Leer los bytes recibidos en el flujo de red y obtener la cantidad de bytes leídos
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);

                // Convertir los bytes recibidos en una cadena de texto utilizando UTF-8
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                // Dividir el mensaje en partes separadas por espacios
                var partes = mensaje.Split(' ');

                // Crear y devolver un objeto Respuesta basado en las partes del mensaje recibido
                return new Respuesta
                {
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };
            }
            catch (SocketException ex)
            {
                // Capturar errores de socket y devolver una Respuesta de error
                return new Respuesta
                {
                    Estado = "ERROR",
                    Mensaje = ex.Message // Utiliza el mensaje de la excepción como el mensaje de error
                };
            }

            // Si ocurre un error inesperado, devuelve una respuesta nula
            return null;
        }

        // Función para resolver un pedido y devolver una respuesta según el comando recibido
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente, Dictionary<string, int> listadoClientes)
        {
            // Inicializar una respuesta predeterminada indicando que el comando no es reconocido
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            // Evaluar el comando del pedido para determinar la respuesta adecuada
            switch (pedido.Comando)
            {
                case "INGRESO":
                    // Verificar si los parámetros de ingreso son válidos
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        // Generar una respuesta de acceso concedido o negado de manera aleatoria
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
                    // Verificar si los parámetros para el cálculo son válidos
                    if (pedido.Parametros.Length == 3)
                    {
                        // Obtener los parámetros del pedido para el cálculo
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];

                        // Validar la placa antes de continuar con el cálculo
                        if (ValidarPlaca(placa))
                        {
                            // Obtener un indicador específico del día basado en la placa
                            byte indicadorDia = ObtenerIndicadorDia(placa);

                            // Generar una respuesta con la placa y el indicador del día
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };

                            // Registrar la solicitud del cliente en el listado
                            ContadorCliente(direccionCliente, listadoClientes);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    // Verificar si existen solicitudes previas del cliente en el listado
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        // Generar una respuesta con el número de solicitudes previas del cliente
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

            // Devolver la respuesta final según el comando evaluado
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

        private static void ContadorCliente(string direccionCliente, Dictionary<string, int> listadoClientes)
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
