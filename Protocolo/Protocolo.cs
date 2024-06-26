using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Protocolo
{
    //Se define una clase Pedido, cuenta con dos propiedades
    public class Pedido
    {
        //comando es una cadena de texto simple con sus get y set
        public string Comando { get; set; }
        // parámetros es nun array de strings, almacena los datos asociados al comando
        public string[] Parametros { get; set; }

        //Se define un método estático Procesar que toma un string mensaje como parámetro de entrada y es procesado para instanciar un Pedido
        public static Pedido Procesar(string mensaje)
        {
            //Se divide en partes el mensaje, el separador o delimitador será el espacio
            var partes = mensaje.Split(' ');
            // Se crea una nueva instancia de pedido
            return new Pedido
            {
                //Se toma la primera parte del arreglo arriba definido y se asigna a la propiedad. ToUper lo hace mayúsculas
                Comando = partes[0].ToUpper(),
                //Se toma el resto de partes del arreglo, evitando la primera con "skip" y se convierte en un array el resultado
                Parametros = partes.Skip(1).ToArray()
            };
        }

        public override string ToString()
        {
            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    //Se define la clase Respuesta
    public class Respuesta
    {
        // se cuenta con un estado y un mensaje, ambos strings
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        // Se sobreescribe el método ToString para que retorne la variable Estado y Mensaje cuando se solicite.
        public override string ToString()
        {
            return $"{Estado} {Mensaje}";
        }
    }

    //Ahora se crea una clase protocolo que haga uso de las clases Pedido y Respuesta
    public class Protocolo
    {
        //Con este método se realiza la operación enviando un pedido a través de un flujo
        public static Respuesta HazOperacion(Pedido pedido, NetworkStream flujo)
        {
            //Se hace una validación, si existe una excepcion se devuelve un mensaje de no hay conexión
            if (flujo== null)
            {
                throw new InvalidOperationException("No hay conexión");
            }
            try
            {
                //Se transforma el pedido al arreglo de bytes para que pueda ser enviado, se le añade el espacio para poder separar después
                byte[] bufferTx = Encoding.UTF8.GetBytes(
                    pedido.Comando + " " + string.Join(" ", pedido.Parametros));
                //Se envía el pedido tomando como referencia el tamaño de trama
                flujo.Write(bufferTx, 0, bufferTx.Length);

                //La respuesta tambien se almacena en un buffer, se la lee y  se la convierte a string los bytes
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                var partes = mensaje.Split(' ');

                // El retorno será una nueva instancia de la clase respuesta
                return new Respuesta
                {
                    //Estado represetna el estado del pedido, el resto de la cadena se une y se asigna a la propiedad mensaje
                    Estado = partes[0],
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                };

            }
            //Si ocurre un error se notifica
            catch (SocketException ex)
            {
                throw new InvalidOperationException("Error al transmitir: " + ex.Message);
            }

        }

        //Con este método ResolverPedido se retornara una respuesa resolviendo el pedido del cliente en el sevidor
        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente, Dictionary<string, int> listadoClientes)
        {
            //Instanciamos una respuesta 
            Respuesta respuesta = new Respuesta
            { Estado = "NOK", Mensaje = "Comando no reconocido" };

            //Se evalúa el comando del pedido, con los datos y pasos que eran necesaros
            switch (pedido.Comando)
            {
                case "INGRESO":
                    //Se verifica si el usuario y password están bien, independientemente de esto se usa un randomico ´para el ingreso
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
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
                    //Si el los datos de inicio estan mal, siempre se deniega el acceso
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    //Se calcula o se valida la placa, se llama al método validarplaca para ver si cumple las normas de una placa
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
                            ContadorCliente(direccionCliente, listadoClientes);
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    //Se obtiene el conteo de solicitudes que ha hecho un cliente
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
        //Metodo estatico, recibe como parametro la placa y lo valida para ver si sus tres primeros caracteres son letras y si los 4 ultimos son números
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        //Este método recibe la placa y valida el último digito para definir que día no circula
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

        //El metodo ContadorCliente incrementa el contador del cliente que está realizando la solicitud
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
