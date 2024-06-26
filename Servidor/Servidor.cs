// ************************************************************************
// Practica 07
// Juan Díaz
// Fecha de realización: 25/06/2024
// Fecha de entrega: 26/06/2024
// Resultados:
// * Se implementó con éxito un servidor TCP capaz de manejar solicitudes de clientes mediante un protocolo definido.
// * El servidor responde correctamente a las solicitudes "INGRESO" y "CALCULO", validando credenciales y placa respectivamente.
// * Se implementó un contador de operaciones "CALCULO" por cliente, reflejando estadísticas de uso.
// * Se realizaron pruebas exitosas de conexión y respuesta con múltiples clientes simulados.
// * La aplicación maneja adecuadamente errores de conexión y operaciones no reconocidas, proporcionando retroalimentación al cliente.

// Conclusiones:
// * La implementación de un servidor TCP robusto permite la gestión eficiente de solicitudes concurrentes, asegurando un servicio confiable.
// * El uso de un protocolo definido facilita la interoperabilidad entre cliente y servidor, asegurando la coherencia en la comunicación.
// * El contador de operaciones por cliente es útil para monitorear y gestionar la carga del servidor, mejorando la administración de recursos.
// * Las pruebas exitosas validan la funcionalidad y fiabilidad del servidor bajo condiciones de uso simuladas.

// Recomendaciones:
// * Implementar mecanismos de seguridad adicionales, como cifrado de datos, para proteger la integridad de las comunicaciones.
// * Considerar la implementación de un registro detallado de operaciones para análisis posterior y auditorías de seguridad.
// * Realizar pruebas de carga adicionales para evaluar el rendimiento del servidor bajo cargas extremas y optimizar según sea necesario.
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
        private static TcpListener escuchador; // Listener TCP para aceptar conexiones entrantes
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>(); // Almacena el número de solicitudes por cliente

        static void Main(string[] args)
        {
            try
            {
                escuchador = new TcpListener(IPAddress.Any, 8080); // Inicia el listener en todas las interfaces en el puerto 8080
                escuchador.Start(); // Comienza a escuchar por conexiones entrantes
                Console.WriteLine("Servidor inició en el puerto 8080..."); // Mensaje de inicio del servidor

                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient(); // Acepta un cliente entrante y devuelve un TcpClient
                    Console.WriteLine("Cliente conectado, dirección: {0}", cliente.Client.RemoteEndPoint.ToString()); // Muestra la dirección del cliente conectado
                    Thread hiloCliente = new Thread(ManipuladorCliente); // Crea un hilo para manejar al cliente
                    hiloCliente.Start(cliente); // Inicia el hilo para manejar al cliente
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al iniciar el servidor: " + ex.Message); // Captura y muestra errores de socket al iniciar el servidor
            }
            finally
            {
                escuchador?.Stop(); // Detiene el listener TCP en caso de finalización
            }
        }

        // Método para manejar las solicitudes de los clientes
        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj; // Convierte el objeto a TcpClient para manipularlo
            NetworkStream flujo = null; // Flujo de red para enviar y recibir datos con el cliente
            try
            {
                flujo = cliente.GetStream(); // Obtiene el flujo de red del cliente
                byte[] bufferTx; // Buffer para datos de salida
                byte[] bufferRx = new byte[1024]; // Buffer para datos de entrada con tamaño de 1024 bytes
                int bytesRx;

                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx); // Convierte datos recibidos a string
                    Pedido pedido = Pedido.Procesar(mensajeRx); // Procesa el mensaje recibido en un objeto Pedido
                    Console.WriteLine("Se recibió: " + pedido); // Muestra en consola el pedido recibido

                    Protocolo.Protocolo protocolo = new Protocolo.Protocolo(); // Instancia del protocolo para manejar el pedido
                    Respuesta respuesta = protocolo.ResolverPedido(pedido); // Resuelve el pedido utilizando el protocolo
                    Console.WriteLine("Se envió: " + respuesta); // Muestra en consola la respuesta enviada al cliente

                    bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString()); // Convierte la respuesta a bytes para enviarla
                    flujo.Write(bufferTx, 0, bufferTx.Length); // Envía la respuesta al cliente
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message); // Captura y muestra errores de socket al manejar un cliente
            }
            finally
            {
                flujo?.Close(); // Cierra el flujo de red
                cliente?.Close(); // Cierra la conexión con el cliente
            }
        }
    }
}
