using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        // Escuchador para aceptar conexiones TCP.
        private static TcpListener escuchador;

        static void Main(string[] args)
        {
            try
            {
                // Inicializa el escuchador para aceptar conexiones en cualquier IP en el puerto 8080.
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor inició en el puerto 5000...");

                // Bucle infinito para aceptar conexiones de clientes.
                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());
                    // Crea un nuevo hilo para manejar la conexión del cliente.
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                // Manejo de excepciones en caso de error al iniciar el servidor.
                Console.WriteLine("Error de socket al iniciar el servidor: " + ex.Message);
                escuchador?.Stop();
            }
        }

        // Método para manejar la conexión con el cliente.
        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream flujo = null;
            try
            {
                // Obtiene el flujo de datos del cliente.
                flujo = cliente.GetStream();
                byte[] bufferTx;
                byte[] bufferRx = new byte[1024];
                int bytesRx;

                // Bucle para leer los datos enviados por el cliente.
                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    // Convierte los bytes recibidos en un string.
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
                    // Procesa el mensaje recibido para crear un objeto Pedido.
                    Pedido pedido = Pedido.Procesar(mensajeRx);
                    Console.WriteLine("Se recibio: " + pedido);

                    // Obtiene la dirección del cliente.
                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString();
                    // Resuelve el pedido y obtiene una respuesta.
                    Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(pedido, direccionCliente);
                    Console.WriteLine("Se envió: " + respuesta);

                    // Envía la respuesta al cliente.
                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }

            }
            catch (SocketException ex)
            {
                // Manejo de excepciones en caso de error al manejar la conexión del cliente.
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
                flujo?.Close();
                cliente?.Close();
            }

        }
    }
}