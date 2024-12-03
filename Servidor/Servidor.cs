// ************************************************************************
// Practica 07
// Patricio Flor 
// Fecha de entrega: 13/03/2024
// Resultados:
// *    El presente sistema es capaz de realizar una conexión entre un cliente y un servidor, el cliente puede realizar consultas al servidor que buscan validar un vehículo,
//      el servidor responde a estas consultas con un mensaje que indica si la placa del vehículo es válida o no. Pero para ello es necesario pasar por un proceso de inicio de sesión
//      donde al colocar la clave y usuario no siempre podran ingresar.
//      La practica se basa aprender como es el correcto fucnionamiento de GitHub  

// Conclusiones:
// *    Se concluye que el utilizar la clase protocolo es una buena forma de poder realizar una conexión entre un cliente y un servidor, ya que esta
//      clase nos permite realizar la conexión de una forma más sencilla y rápida y poder reutilizar codigo lo cual mejora enormemente el tiempo de desarrollo.
// *    Se concluye que el uso de GitHub dentro del mundo estudiantil es infravalorada ya que el saber utilziarla nos permite tener mejor control de nuestros
//      proyectos ademas de poder trabajar con los compañeros de forma más sencilla y rápida.
// Recomendaciones:
// *    Se recominda, que se realice una correcta documentación de los proyectos que se realicen, ya que esto nos permite tener un
//      mejor control de los mismos y si se llegase a trabajar en equipo nos permite a los desarrolladores crear una mejor comunicación.
// *    Se recomienda, que se realice un correcto uso de GitHub ya que esta herramienta nos permite tener un mejor control de los
//      proyectos pero es necesario conecer todos los comandos, funciones y herramientas para lo cual es necesario tener conocimientos previos
//      o realizar un curso para comprender el correcto funcionamiento de esta herramienta.
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

        // Método principal del programa
        static void Main(string[] args)
        {
            try
            {
                // Crear un objeto TcpListener para escuchar en cualquier dirección IP en el puerto 8080
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor inició en el puerto 5000...");

                while (true)
                {
                    // Aceptar una conexión entrante de un cliente
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());

                    // Crear un hilo para manejar al cliente
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al iniciar el servidor: " + ex.Message);
            }
            finally
            {
                // Detener el objeto TcpListener
                escuchador?.Stop();
            }
        }

        // Método para manejar a un cliente
        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;
            try
            {
                // Obtener el flujo de red del cliente
                flujo = cliente.GetStream();
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    // Convertir los bytes recibidos en una cadena de texto
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                    // Procesar el mensaje recibido y obtener un objeto Pedido
                    Pedido pedido = Pedido.Procesar(mensajeRx);
                    Console.WriteLine("Se recibio: " + pedido);

                    // Obtener la dirección del cliente
                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();

                    // Resolver el pedido utilizando el método ResolverPedido de la clase Protocolo
                    Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(pedido, direccionCliente);
                    Console.WriteLine("Se envió: " + respuesta);

                    // Convertir la respuesta en bytes y enviarla al cliente
                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                // Cerrar el flujo y el cliente
                flujo?.Close();
                cliente?.Close();
            }
        }
    }
}
