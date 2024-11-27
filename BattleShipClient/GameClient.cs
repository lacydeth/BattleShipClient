using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipClient
{
    public class GameClient
    {
        private TcpClient tcpClient;
        private NetworkStream stream;

        public bool IsConnected { get; private set; }

        public void Connect(string ipAddress, int port)
        {
            try
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();
                IsConnected = true;
                StartReceiving();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                IsConnected = false;
            }
        }

        public void SendShot(int x, int y)
        {
            if (!IsConnected) return;

            string message = $"SHOT:{x},{y}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        private async void StartReceiving()
        {
            byte[] buffer = new byte[256];

            try
            {
                while (IsConnected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessServerResponse(response);
                    }
                }
            }
            catch
            {
                IsConnected = false;
                Console.WriteLine("Disconnected from server.");
            }
        }

        private void ProcessServerResponse(string response)
        {
            // Handle server messages (e.g., HIT, MISS, WIN, LOSE)
            Console.WriteLine($"Server response: {response}");
        }
    }
}
