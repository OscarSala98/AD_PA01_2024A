// ************************************************************************
// Practica 07
// Esteban Chicango
// Fecha de realización: 22/06/2024
// Fecha de entrega: 26/06/2024
//
// Resultados:
// Como se requería, se implmentó la claseProtocolo donde se maneja las operaciones y acciones entre el cliente y el servidor
// de la misma forma, estas crean una mejor comunicación y hacen más entendible el protocolo utilizado.
// Se cambió el mensaje que decía que se oescucha en el puerto 5000 cuando en realidad lo hacemos en el  8080
// El error sucedido encontrado en clase fue eliminado, se quitó los finally en el cliente al cerrar las conexiones.
// Este error aparece pues cierran la conexion cuando el form se está cargando, entonces salta la excpecion.
//
//Conclusiones:
// - El utilizar una clase protocolo facilita enormemente le utilización y entendimiento del código para pedidos y 
// respuestas, alivianando tambien el volumen de codigo en el servidor y en el cliente.
// - Las validaciones en la mayoría de los casos ayuda al cliente a efectuar de manera correcta el programa sin que
// este se caiga, de no utilizarlas el cliente puede generar complicaciones y el programa puede lanzar excepciones.
//
//Recomendaciones:
// -Cerrar las conexiones del cliente con el servidor al momento que un formulario se cierre, de no hacer esto, la conexión
// segruirá activa y puede generar complicaciones.
// - Utilizar puntos de para con el fin de analizar paso a paso los eventos que van sucediendo en el codigo, esto ayuda a
// detectar posibles fallos y entender de mejor manera los procesos realizados.
// - Utilizar un protocolo bien definido para las peticiones y respuetas, esto facilita la comunicación y entendimiento entre 
// ambas partes, el cliente y el servidor.
// *****************************************************************************



using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        // Se instancian los elementos necesarios para la conexión
        private static TcpListener escuchador;
        private static Dictionary<string, int> listadoClientes
            = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            try
            {
                //Se define un escucha que trabajará en el puerto 8080
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                //Se modifica el mensaje para que indique el puerto donde realmente está escuchando 8080
                Console.WriteLine("Servidor inició en el puerto 8080...");

                //Se usa el lazo while para manejar distintos clientes mediante hilos
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
                //Se controla el error y se muestra un mensaje si es que no se puede establecer la conexion
                Console.WriteLine("Error de socket al iniciar el servidor: " +
                    ex.Message);
            }
            finally 
            {
                escuchador?.Stop();
            }
        }

        //Se maneja la comunicación con el cliente
        private static void ManipuladorCliente(object obj)
        {
            //Se intancia un cliente TCP y un flujo con el que se va a trabajar
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;
            try
            {
                flujo = cliente.GetStream();
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                //Se lee y procesa los mensajes del cliente
                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    //Se procesa el mensaje recibido por parte del cliente (peticion), se lo transforma y se muestra el mensaje en consola del server
                    string mensajeRx =
                        Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    Pedido pedido = Pedido.Procesar(mensajeRx);
                    Console.WriteLine("Se recibió: " + pedido);

                    string direccionCliente =
                        cliente.Client.RemoteEndPoint.ToString();
                    //Se hace uso del protocolo creado
                    Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(pedido, direccionCliente, listadoClientes);
                    //En consola del server se muestra lo que se envió al cliente
                    Console.WriteLine("Se envió: " + respuesta);

                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }

            }
            //Se maneja la excepción al manejar al cliente
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


    }
}
