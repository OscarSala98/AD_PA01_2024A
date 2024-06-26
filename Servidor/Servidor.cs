﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Protocolo;

namespace Servidor
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = null; // Servidor TCP para escuchar conexiones entrantes
            try
            {
                // Inicializar el servidor para escuchar en cualquier dirección IP y el puerto 8080
                listener = new TcpListener(IPAddress.Any, 8080);
                listener.Start();
                Console.WriteLine("Servidor iniciado...");

                // Diccionario para llevar el conteo de consultas por cliente
                Dictionary<string, int> listadoClientes = new Dictionary<string, int>();

                while (true)
                {
                    Console.WriteLine("Esperando conexión...");
                    TcpClient cliente = listener.AcceptTcpClient(); // Aceptar una nueva conexión
                    Console.WriteLine("Cliente conectado: " + cliente.Client.RemoteEndPoint);

                    // Iniciar un nuevo hilo para manejar al cliente
                    System.Threading.ThreadPool.QueueUserWorkItem(HandleClient, new ClientInfo { TcpClient = cliente, ListadoClientes = listadoClientes });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Detener el servidor si ocurre algún error
                listener?.Stop();
            }
        }

        static void HandleClient(object clientInfo)
        {
            ClientInfo info = (ClientInfo)clientInfo;
            TcpClient cliente = info.TcpClient;
            Dictionary<string, int> listadoClientes = info.ListadoClientes;

            string remoteEndPoint = cliente.Client.RemoteEndPoint.ToString(); // Dirección del cliente
            try
            {
                using (NetworkStream stream = cliente.GetStream()) // Flujo de datos para la comunicación con el cliente
                {
                    byte[] bufferRx = new byte[1024]; // Buffer para recibir datos

                    while (cliente.Connected) // Mientras el cliente esté conectado
                    {
                        int bytesRx = stream.Read(bufferRx, 0, bufferRx.Length); // Leer datos del cliente
                        if (bytesRx == 0)
                        {
                            break; // El cliente se ha desconectado
                        }

                        string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx); // Convertir los datos recibidos a cadena
                        Console.WriteLine("Mensaje recibido: " + mensaje);
                        Pedido pedido = Pedido.Procesar(mensaje); // Procesar el mensaje recibido

                        // Resolver el pedido y obtener la respuesta
                        Respuesta respuesta = Protocolo.Protocolo.ResolverPedido(pedido, cliente.Client.RemoteEndPoint.ToString(), ref listadoClientes);

                        // Enviar la respuesta al cliente
                        byte[] bufferTx = Encoding.UTF8.GetBytes(respuesta.ToString());
                        stream.Write(bufferTx, 0, bufferTx.Length);
                        Console.WriteLine("Respuesta enviada: " + respuesta.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la comunicación con el cliente: " + ex.Message);
            }
            finally
            {
                // Informar la desconexión del cliente y cerrar la conexión
                Console.WriteLine("Cliente desconectado: " + remoteEndPoint);
                cliente.Close();
            }
        }

        class ClientInfo
        {
            public TcpClient TcpClient { get; set; } // Cliente TCP
            public Dictionary<string, int> ListadoClientes { get; set; } // Diccionario para el conteo de consultas por cliente
        }
    }
}