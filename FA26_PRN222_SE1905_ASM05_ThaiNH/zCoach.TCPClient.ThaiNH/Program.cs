using System.Net.Sockets;
using System.Text;

Console.WriteLine("zCoach TCP CLIENT");

Console.OutputEncoding = Encoding.UTF8;

while (true)
{
    Console.Write("Enter Coach ID, phone number or ALL (type EXIT to quit): ");
    string? command = Console.ReadLine();

    if (command is null) break;
    command = command.Trim();

    if (string.IsNullOrWhiteSpace(command))
    {
        Console.WriteLine("Command must not be empty.");
        Console.WriteLine();
        continue;
    }

    if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    int port = 5000;

    try
    {
        using TcpClient client = new();
        await client.ConnectAsync("127.0.0.1", port);
        Console.WriteLine("Connect to Server:" + port.ToString() + " and send the request: " + command);

        using NetworkStream stream = client.GetStream();

        byte[] requestBytes = Encoding.UTF8.GetBytes(command + "\n");
        await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
        await stream.FlushAsync();

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        string response = await reader.ReadToEndAsync();

        Console.WriteLine("Server response:");
        Console.WriteLine(response);

        client.Close();
        Console.WriteLine("Disconnect to Server");

    }
    catch (SocketException)
    {
        Console.WriteLine("Cannot connect to server at 127.0.0.1:" + port.ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }

    Console.WriteLine();
}
