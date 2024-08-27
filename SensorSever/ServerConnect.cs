using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SensorSever;

public class ServerConnect
{
    private TcpListener listener;

    public void StartServer()
    {
        listener = new TcpListener(IPAddress.Any, 8000);
        listener.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            // 在新线程中处理客户端通信
            new System.Threading.Thread(() => HandleClient(client)).Start();
        }
    }
    
    void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        while (true)
        {
            // 模拟传感器数据到达
            string command = "SomeMethod"; // 需要调用的方法名
            int messageType = 1; // 自定义消息类型，比如1表示调用方法

            byte[] bodyBuffer = Encoding.UTF8.GetBytes(command);
            int bodyLength = bodyBuffer.Length;

            // 构造包头（8字节：前4字节为包体长度，后4字节为消息类型）
            byte[] headerBuffer = new byte[8];
            BitConverter.GetBytes(bodyLength).CopyTo(headerBuffer, 0);
            BitConverter.GetBytes(messageType).CopyTo(headerBuffer, 4);

            // 发送包头
            stream.Write(headerBuffer, 0, headerBuffer.Length);

            // 发送包体
            stream.Write(bodyBuffer, 0, bodyBuffer.Length);

            // 模拟延时
            System.Threading.Thread.Sleep(5000);
        }
    }
    
   
}