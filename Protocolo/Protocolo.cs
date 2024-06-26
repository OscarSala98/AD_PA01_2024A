using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Protocolo
{
    // Clase que representa un Pedido enviado al servidor
    public class Pedido
    {
        public string Comando { get; set; }      // Comando del pedido
        public string[] Parametros { get; set; } // Parámetros del comando

        // Método estático para procesar un mensaje y crear un Pedido
        public static Pedido Procesar(string mensaje)
        {
            var partes = mensaje.Split(' ');
            return new Pedido
            {
                Comando = partes[0].ToUpper(),    // Convertir comando a mayúsculas
                Parametros = partes.Skip(1).ToArray() // Resto de la cadena como parámetros
            };
        }

        // Sobrescritura del método ToString para obtener representación en cadena del Pedido
        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    // Clase que representa la Respuesta del servidor a un Pedido
    public class Respuesta
    {
        public string Estado { get; set; }   // Estado de la respuesta (OK/NOK)
        public string Mensaje { get; set; }  // Mensaje de la respuesta

        // Sobrescritura del método ToString para obtener representación en cadena de la Respuesta
        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    // Clase que implementa el protocolo de comunicación del servidor
    public class Protocolo
    {
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>(); // Diccionario para contar consultas por cliente

        // Método para procesar una operación en el servidor
        public Respuesta HazOperacion(Pedido pedido, NetworkStream stream)
        {
            // Serializar el objeto Pedido y enviarlo al servidor
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, pedido);

            // Leer la respuesta del servidor
            Respuesta respuesta = (Respuesta)formatter.Deserialize(stream);
            return respuesta;
        }

        // Método para resolver un Pedido y generar la Respuesta correspondiente
        public Respuesta ResolverPedido(Pedido pedido)
        {
            Respuesta respuesta = new Respuesta();

            switch (pedido.Comando)
            {
                case "INGRESO":
                    // Verificar credenciales de acceso
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        // Simular acceso concedido o negado aleatoriamente
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
                    // Procesar cálculo según los parámetros recibidos
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        if (ValidarPlaca(placa))
                        {
                            // Incrementar el contador de consultas por cliente
                            ContadorCliente(pedido.ToString());

                            // Obtener indicador de día según el último dígito de la placa
                            byte indicadorDia = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta
                            {
                                Estado = "OK",
                                Mensaje = $"{placa} {indicadorDia}"
                            };
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    // Responder con la cantidad total de consultas realizadas
                    respuesta = new Respuesta
                    {
                        Estado = "OK",
                        Mensaje = listadoClientes.Values.Sum().ToString() // Suma de todas las consultas realizadas
                    };
                    break;

                default:
                    respuesta.Estado = "NOK";
                    respuesta.Mensaje = "Comando no reconocido";
                    break;
            }

            return respuesta;
        }

        // Método privado para incrementar el contador de consultas por cliente
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

        // Método privado para validar el formato de la placa
        private bool ValidarPlaca(string placa)
        {
            // Aquí va la lógica para validar la placa
            Regex regex = new Regex(@"^[A-Za-z]{3}\d{4}$"); // se modificó para que admita tanto minúsculas como mayúsculas
            return regex.IsMatch(placa);
        }

        // Método privado para obtener el indicador de día según el último dígito de la placa
        private byte ObtenerIndicadorDia(string placa)
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

    }
}
