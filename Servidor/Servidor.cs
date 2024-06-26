// *****************************************************************************
// Practica 07
// BrigethT 
// Fecha de realización: 21/06/2024
// Fecha de entrega: 26/06/2024
//
//
// Conclusiones:
// *    Se concluye que se puede implementar los metodos ResolverPedido y HazOperacion en la parte del protocolo
//      con el fin de optimizar el codigo en las otras clases y centralizar la parte lógica.
//
// *    Se tiene un mejor entendimiento de que realiza cada parte y cada método, tambien se tiene una composicìón
//      de clases en donde hay que tener el conocimiento del uso de los namespaces
//
// Recomendaciones:
// *    Es recomendable el uso de los try catchs para cada método ya que estos nos prodrian generar errores
//
// *    Se recomienda tambien utilizar correctamete parámetros de entradas entres funciones ya que comparte muchos 
//      parñametros como el remoto y el fujo los cuales van a ser utilizados en todos los métodos
//
// *****************************************************************************


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

        static void Main(string[] args)
        {
            try
            {
                // Crear un objeto TcpListener que escucha en cualquier dirección IP en el puerto 8080
                TcpListener escuchador = new TcpListener(IPAddress.Any, 8080);

                // Iniciar la escucha en el puerto especificado
                escuchador.Start();
                Console.WriteLine("Servidor inició en el puerto 5000...");

                // Esperar continuamente por conexiones entrantes de clientes
                while (true)
                {
                    // Aceptar una conexión entrante y obtener el TcpClient asociado
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());

                    // Iniciar un nuevo hilo para manejar las operaciones del cliente
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

        // Método para manejar las operaciones con un cliente conectado
        private static void ManipuladorCliente(object obj)
        {
            // Obtener el TcpClient del objeto recibido como parámetro
            TcpClient cliente = (TcpClient)obj;

            // Inicializar el flujo de red para comunicarse con el cliente
            NetworkStream flujo = null;

            try
            {
                // Obtener el flujo de red del cliente
                flujo = cliente.GetStream();

                // Definir buffers para la lectura y escritura de datos
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                // Bucle para recibir y procesar mensajes del cliente
                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    // Convertir los bytes recibidos en una cadena de texto
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                    // Procesar el mensaje recibido utilizando la clase Pedido del protocolo
                    Protocolo.Protocolo.Pedido pedido = Protocolo.Protocolo.Pedido.Procesar(mensajeRx);

                    // Mostrar en consola el pedido recibido
                    Console.WriteLine("Se recibió: " + pedido);

                    // Obtener la dirección del cliente remoto
                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();

                    // Resolver el pedido utilizando una función ficticia ResolverPedido del protocolo
                    Protocolo.Protocolo.Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(pedido, direccionCliente, listadoClientes);

                    // Mostrar en consola la respuesta que se enviará al cliente
                    Console.WriteLine("Se envió: " + respuesta);

                    // Convertir la respuesta en bytes para enviarla al cliente
                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());

                    // Enviar la respuesta al cliente a través del flujo de red
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                // Capturar errores de socket y mostrarlos en consola
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                // Cerrar el flujo de red y el cliente TCP en el bloque finally para asegurar la limpieza
                flujo?.Close();
                cliente?.Close();
            }
        }





    }
}
