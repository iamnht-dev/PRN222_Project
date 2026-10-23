using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace zCoach.TCPServer.ThaiNH
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("zCoach TCP SERVER");

            TcpListener? server = null;
            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                int port = 5000;
                server = new TcpListener(ipAddress, port);

                server.Start();
                Console.WriteLine("Server started at localhost:" + port.ToString());

                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected");

                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                server?.Stop();
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();

                // One line is one request, even when TCP splits the bytes into multiple reads.
                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                string? request = (await reader.ReadLineAsync())?.Trim();

                if (!string.IsNullOrWhiteSpace(request))
                {
                    string jsonResponse = string.Empty;

                    var noItem = new
                    {
                        RequestType = request,
                        Status = "Not found",
                        Message = "No items in this request " + request
                    };

                    bool getAll = request.Equals("ALL", StringComparison.OrdinalIgnoreCase);
                    bool hasId = int.TryParse(request, out int coachId);
                    var items = DataStore.Coaches
                        .Where(coach => getAll || (hasId && coach.CoachThaiNhid == coachId) || coach.Phone == request)
                        .ToList();

                    jsonResponse = items.Count > 0
                        ? JsonSerializer.Serialize(items)
                        : JsonSerializer.Serialize(noItem);

                    byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    await stream.FlushAsync();

                    Console.WriteLine($"Sent message to client for request: {request}");
                    client.Close();
                    Console.WriteLine($"Client disconnect");
                }
                else
                {
                    string error = JsonSerializer.Serialize(new { Error = "Invalid request" });
                    byte[] errorBytes = Encoding.UTF8.GetBytes(error);
                    await stream.WriteAsync(errorBytes, 0, errorBytes.Length);
                    await stream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }
}
