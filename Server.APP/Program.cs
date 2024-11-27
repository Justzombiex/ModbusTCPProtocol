using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class TcpGenericServer
{
    static void Main()
    {
        const int port = 502; // Puerto TCP
        TcpListener server = null;

        try
        {
            // Configurar el servidor para escuchar en todas las interfaces
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            Console.WriteLine($"Servidor TCP genérico escuchando en el puerto {port}...");

            while (true)
            {
                // Aceptar una conexión entrante
                Console.WriteLine("Esperando una conexión...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Cliente conectado!");

                // Leer y procesar los datos recibidos
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    // Convertir los datos a texto y mostrarlos en la consola
                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Recibido: {data}");

                    // Enviar una respuesta genérica al cliente
                    string response = "Servidor genérico: datos recibidos.\n";
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }

                client.Close();
                Console.WriteLine("Cliente desconectado.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            server?.Stop();
        }
    }
}
